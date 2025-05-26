using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.ServerGameStates;
using WaterWizard.Shared;

namespace WaterWizard.Server;

public class PlacedShip
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Represents the true Gamestate of the Game as it is on the Server.
/// Includes:
/// <list type="table">
///     <item>
///         <term>Players</term>
///         <description>The <see cref="NetPeer"/>s of the connected players</description>
///     </item>
///     <item>
///         <term>Boards</term>
///         <description>The Gameboards of the Players, in the Form of 2D <see cref="Cell"/> Arrays.</description>
///     </item>
///     <item>
///         <term>Hands</term>
///         <description>The Hands of Cards of the Players, in the Form of <see cref="Cards"/> Lists</description>
///     </item>
///     <item>
///         <term>Active Cards</term>
///         <description>The Cards that were played are currently exercising an Active Effect</description>
///     </item>
///     <item>
///         <term>Stacks</term>
///         <description>
///             <see cref="UtilityStack"/>, <see cref="DamageStack"/> and <see cref="EnvironmentStack"/>.
///             <see cref="Cards"/> Lists that are filled with the Cards of the <see cref="CardType"/>s
///             <see cref="CardType.Healing"/> + <see cref="CardType.Utility"/>, <see cref="CardType.Damage"/>
///             and <see cref="CardType.Environment"/> respectively.
///         </description>
///     </item>
///     <item>
///         <term>Graveyard</term>
///         <description>The Cards that were already played go here. <see cref="Cards"/> List</description>
///     </item>
/// </list>
/// <para>Also Handles Ship Placements, Card Buying and Casting, Mana and Gold generation. etc. </para>
/// </summary>
public class GameState
{
    private NetPeer[] players = new NetPeer[2];
    private static readonly int boardWidth = 12;
    private static readonly int boardHeight = 10;
    private readonly Cell[][,] boards = new Cell[2][,];
    public Cell[,] Player1 => boards[0];
    public Cell[,] Player2 => boards[1];
    public readonly List<Cards>[] hands;
    private readonly NetManager server;
    private readonly ServerGameStateManager manager;
    public List<Cards> Player1Hand => hands[0];
    public List<Cards> Player2Hand => hands[1];
    public List<Cards> ActiveCards { get; private set; }
    public List<Cards> UtilityStack { get; private set; }
    public List<Cards> DamageStack { get; private set; }
    public List<Cards> EnvironmentStack { get; private set; }
    public List<Cards> Graveyard { get; private set; }

    private Timer activationTimer;

    private readonly Dictionary<NetPeer, List<PlacedShip>> playerShips = new();

    private bool IsPlacementPhase()
    {
        return manager.CurrentState is PlacementState;
    }

    public void AddShip(NetPeer player, PlacedShip ship)
    {
        if (!playerShips.ContainsKey(player))
            playerShips[player] = new List<PlacedShip>();
        playerShips[player].Add(ship);
    }

    public IReadOnlyList<PlacedShip> GetShips(NetPeer player)
    {
        if (playerShips.TryGetValue(player, out var ships))
            return ships;
        return [];
    }

    public void PrintAllShips()
    {
        foreach (var kvp in playerShips)
        {
            Console.WriteLine($"Schiffe von Spieler {kvp.Key}:");
            foreach (var ship in kvp.Value)
            {
                Console.WriteLine(
                    $"  Schiff: X={ship.X}, Y={ship.Y}, W={ship.Width}, H={ship.Height}"
                );
            }
        }
    }

    /// <summary>
    /// A new Gamestate.
    /// </summary>
    /// <param name="server">Corresponding NetManager server to access Clients (Players)</param>
    /// <param name="manager">Corresponding <see cref="ServerGameStateManager"/></param>
    /// <exception cref="InvalidOperationException"></exception>
    public GameState(NetManager server, ServerGameStateManager manager)
    {
        int connectedCount = server.ConnectedPeerList.Count;
        if (connectedCount < 1 || connectedCount > 2)
            throw new InvalidOperationException("Game requires 1 or 2 connected players.");

        players = new NetPeer[connectedCount];
        for (int i = 0; i < connectedCount; i++)
            players[i] = server.ConnectedPeerList[i];

        boards = InitBoards();
        hands =
        [
            [],
            [],
        ];
        ActiveCards = [];
        UtilityStack = Cards.GetCardsOfType(CardType.Utility);
        UtilityStack.AddRange(Cards.GetCardsOfType(CardType.Healing));
        DamageStack = Cards.GetCardsOfType(CardType.Damage);
        EnvironmentStack = Cards.GetCardsOfType(CardType.Environment);
        Graveyard = [];
        this.server = server;
        this.manager = manager;

        activationTimer = new Timer(
            _ => UpdateActiveCards(500),
            null,
            0,
            500);
    }

    /// <summary>
    /// Initializes the Boards
    /// </summary>
    /// <returns>
    /// 3D Cell Array where the First Dimension is a 2 Element Array
    /// where the Elements correspond to the 2 Players
    /// </returns>
    private static Cell[][,] InitBoards()
    {
        Cell[,] player1Board = new Cell[boardWidth, boardHeight];
        Cell[,] player2Board = new Cell[boardWidth, boardHeight];
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                player1Board[i, j] = new(CellState.Empty);
                player2Board[i, j] = new(CellState.Empty);
            }
        }
        return [player1Board, player2Board];
    }

    /// <summary>
    /// Handles the Placement of the ships. Receives the Position of the ship placement
    /// from the Client. Validates placement and sends error messages if invalid.
    /// </summary>
    /// <param name="peer">The <see cref="NetPeer"/> Client sending the Placement Request</param>
    /// <param name="reader"><see cref="NetPacketReader"/> with the Request Data</param>
    public void HandleShipPlacement(NetPeer peer, NetPacketReader reader)
    {
        int x = reader.GetInt();
        int y = reader.GetInt();
        int width = reader.GetInt();
        int height = reader.GetInt();

        int size = Math.Max(width, height);

        if (IsPlacementPhase())
        {
            var allowedShips = new Dictionary<int, int>
            {
                { 5, 1 },
                { 4, 2 },
                { 3, 2 },
                { 2, 4 },
                { 1, 5 },
            };

            var playerShipList = GetShips(peer);
            int alreadyPlaced = playerShipList.Count(s => Math.Max(s.Width, s.Height) == size);

            // 1. Zu viele Schiffe dieser Länge?
            if (!allowedShips.ContainsKey(size) || alreadyPlaced >= allowedShips[size])
            {
                NetDataWriter errorWriter = new();
                errorWriter.Put("ShipPlacementError");
                errorWriter.Put(
                    $"Du darfst nur {allowedShips.GetValueOrDefault(size, 0)} Schiffe der Länge {size} platzieren!"
                );
                peer.Send(errorWriter, DeliveryMethod.ReliableOrdered);
                return;
            }

            // 2. Überlappung mit eigenen Schiffen verhindern
            foreach (var ship in playerShipList)
            {
                bool overlap =
                    x < ship.X + ship.Width
                    && x + width > ship.X
                    && y < ship.Y + ship.Height
                    && y + height > ship.Y;
                if (overlap)
                {
                    NetDataWriter errorWriter = new();
                    errorWriter.Put("ShipPlacementError");
                    errorWriter.Put("Schiffe dürfen sich nicht überlappen!");
                    peer.Send(errorWriter, DeliveryMethod.ReliableOrdered);
                    return;
                }
            }
        }

        // 3. Felder auf dem Board prüfen
        int playerIndex = Array.IndexOf(players, peer);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var cell = boards[playerIndex][x + i, y + j];
                if (cell.CellState != CellState.Empty)
                {
                    NetDataWriter errorWriter = new();
                    errorWriter.Put("ShipPlacementError");
                    errorWriter.Put("Feld ist bereits belegt!");
                    peer.Send(errorWriter, DeliveryMethod.ReliableOrdered);
                    return;
                }
            }
        }

        // Schiff platzieren
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                boards[playerIndex][x + i, y + j].CellState = CellState.Ship;
            }
        }
        AddShip(
            peer,
            new PlacedShip
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
            }
        );

        NetDataWriter writer = new();
        writer.Put("ShipPosition");
        writer.Put(x);
        writer.Put(y);
        writer.Put(width);
        writer.Put(height);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public void HandleAttack(NetPeer attacker, NetPeer defender, int x, int y)
    {
        Console.WriteLine(
            $"[Server] HandleAttack called: attacker={attacker}, defender={defender}, coords=({x},{y})"
        );
        var ships = GetShips(defender);
        foreach (var ship in ships)
        {
            if (x >= ship.X && x < ship.X + ship.Width && y >= ship.Y && y < ship.Y + ship.Height)
            {
                Console.WriteLine(
                    $"[Server] Treffer auf Schiff bei ({x},{y}) von Spieler {defender.ToString()}"
                );
                return;
            }
        }
        Console.WriteLine(
            $"[Server] Kein Schiff getroffen bei ({x},{y}) von Spieler {defender.ToString()}"
        );
    }

    /// <summary>
    /// Handles the Buying of Cards from a CardStack. Takes a random Card from the corresponding CardStack
    /// of the <see cref="CardType"/> given in <paramref name="reader"/>
    /// </summary>
    /// <param name="peer">The <see cref="NetPeer"/> Client sending the Placement Request</param>
    /// <param name="reader"><see cref="NetPacketReader"/> with the Request Data</param>
    public void HandleCardBuying(NetPeer peer, NetPacketReader reader)
    {
        string cardType = reader.GetString();
        Console.WriteLine($"[Server] Trying to Buy {cardType} Card");
        Cards? card = cardType switch
        {
            "Utility" => RandomCard(UtilityStack), //TODO: Actually Paying
            "Damage" => RandomCard(DamageStack), //TODO: Actually Paying
            "Environment" => RandomCard(EnvironmentStack), //TODO: Actually Paying
            _ => throw new Exception(
                "Invalid CardType: "
                    + cardType
                    + " . Has to be a string of either: Utility, Damage or Environment"
            ),
        };
        NetDataWriter writer = new();
        writer.Put("BoughtCard");
        writer.Put(card.Variant.ToString());
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        if (server.ConnectedPeerList.Count == 2)
        {
            writer = new();
            writer.Put("OpponentBoughtCard");
            writer.Put(card.Type.ToString());
            var opponent = server.ConnectedPeerList.Find(p => !p.Equals(peer));
            opponent?.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        Console.WriteLine($"[Server] Player_{peer.Port} Bought Card {card.Variant}");
    }

    private static Cards RandomCard(List<Cards> stack)
    {
        var index = (int)(stack.Count * Random.Shared.NextSingle());
        return stack[index];
    }

    /// <summary>
    /// Handles the Casting of Cards from the <see cref="NetPeer"/>s hand. //TODO: handle Mana Cost.
    /// Calls the Ability in <see cref="CardAbilities"/>
    /// </summary>
    /// <param name="peer">The <see cref="NetPeer"/> Client sending the Placement Request</param>
    /// <param name="reader"><see cref="NetPacketReader"/> with the Request Data</param>
    public void HandleCardCasting(NetPeer peer, NetPacketReader reader)
    {
        //TODO: Handle Mana Cost
        string cardVariantString = reader.GetString();
        int cardX = reader.GetInt();
        int cardY = reader.GetInt();
        if (Enum.TryParse<CardVariant>(cardVariantString, out var variant))
        {
            // Gegner finden
            var defender = server.ConnectedPeerList.Find(p => !p.Equals(peer));
            if (defender != null)
            {
                CardAbilities.HandleAbility(variant, this, new Vector2(cardX, cardY), defender);
            }
            else
            {
                Console.WriteLine("[Server] Kein Gegner gefunden für CardCast.");
            }
        }
        else
        {
            Console.WriteLine($"[Server] Casting Failed. Variant {cardVariantString} unknown");
        }
    }

    internal void CardActivation(CardVariant variant, int duration)
    {
        Console.WriteLine($"[Server] Activate Card {variant} for {duration} seconds");
        ActiveCards.Add(new(variant)
        {
            remainingDuration = duration * 1000f
        });
    }

    private void UpdateActiveCards(float passedTime)
    {
        List<Cards> toDelete = [];
        Dictionary<Cards, float> toSend = [];
        NetDataWriter writer = new();
        writer.Put("ActiveCards");
        foreach (Cards card in ActiveCards)
        {
            card.remainingDuration -= passedTime;
            if (card.remainingDuration <= 0)
            {
                toDelete.Add(card);
            }
            else
            {
                toSend.Add(card, card.remainingDuration);
                CardAbilities.HandleActivationEffect(card, passedTime);
            }
        }
        writer.Put(toSend.Count);
        foreach (var pair in toSend)
        {
            writer.Put(pair.Key.Variant.ToString());
            writer.Put(pair.Value);
        }
        server.ConnectedPeerList.ForEach(client => client.Send(writer, DeliveryMethod.ReliableOrdered));
        bool success = toDelete.All(ActiveCards.Remove);
    }
}
