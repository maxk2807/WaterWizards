using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using Microsoft.VisualBasic;
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

    private readonly Dictionary<NetPeer, List<PlacedShip>> playerShips = new();

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
            Console.WriteLine($"  Schiff: X={ship.X}, Y={ship.Y}, W={ship.Width}, H={ship.Height}");
        }
    }
}

    public GameState(NetManager server, ServerGameStateManager manager)
    {
        int connectedCount = server.ConnectedPeerList.Count;
        if (connectedCount < 1 || connectedCount > 2)
            throw new InvalidOperationException("Game requires 1 or 2 connected players.");

        players = new NetPeer[connectedCount];
        for (int i = 0; i < connectedCount; i++)
            players[i] = server.ConnectedPeerList[i];

        boards = InitBoards();
        hands = [[], []];
        ActiveCards = [];
        UtilityStack = Cards.GetCardsOfType(CardType.Utility);
        UtilityStack.AddRange(Cards.GetCardsOfType(CardType.Healing));
        DamageStack = Cards.GetCardsOfType(CardType.Damage);
        EnvironmentStack = Cards.GetCardsOfType(CardType.Environment);
        Graveyard = [];
        this.server = server;
        this.manager = manager;
    }

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

    public void HandleShipPlacement(NetPeer peer, NetPacketReader reader)
    {
        int x = reader.GetInt();
        int y = reader.GetInt();
        int width = reader.GetInt();
        int height = reader.GetInt();

        Console.WriteLine(x + " " + y + " " + width + " " + height);

        int playerIndex = Array.IndexOf(players, peer);

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                var cell = boards[playerIndex][x + i, y + j];
                if (cell.CellState != CellState.Empty)
                {
                    Console.WriteLine("[Server Error] Cell at [" + x + "," + y + "] not Empty, can't place Ship");
                }
                cell.CellState = CellState.Ship;
            }
        }
        AddShip(peer, new PlacedShip { X = x, Y = y, Width = width, Height = height });

        NetDataWriter writer = new();
        writer.Put("ShipPosition");
        writer.Put(x);
        writer.Put(y);
        writer.Put(width);
        writer.Put(height);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        Console.WriteLine("[Server] Successfull Ship Handling");
    }

    public void HandleAttack(NetPeer attacker, NetPeer defender, int x, int y)
{
    Console.WriteLine($"[Server] HandleAttack called: attacker={attacker}, defender={defender}, coords=({x},{y})");
    var ships = GetShips(defender);
    foreach (var ship in ships)
    {
        if (x >= ship.X && x < ship.X + ship.Width &&
            y >= ship.Y && y < ship.Y + ship.Height)
        {
            Console.WriteLine($"[Server] Treffer auf Schiff bei ({x},{y}) von Spieler {defender.ToString()}");
            return;
        }
    }
    Console.WriteLine($"[Server] Kein Schiff getroffen bei ({x},{y}) von Spieler {defender.ToString()}");
}

    internal void HandleCardBuying(NetPeer peer, NetPacketReader reader)
    {
        string cardType = reader.GetString();
        Console.WriteLine($"[Server] Trying to Buy {cardType} Card");
        Cards? card = cardType switch
        {
            "Utility" => RandomCard(UtilityStack),//TODO: Actually Paying
            "Damage" => RandomCard(DamageStack),//TODO: Actually Paying
            "Environment" => RandomCard(EnvironmentStack),//TODO: Actually Paying
            _ => throw new Exception("Invalid CardType: " + cardType + " . Has to be a string of either: Utility, Damage or Environment"),
        };
        NetDataWriter writer = new();
        writer.Put("BoughtCard");
        writer.Put(card.Variant.ToString());
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        if(server.ConnectedPeerList.Count == 2)
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

    public void HandleCardCasting(NetPeer peer, NetPacketReader reader)
    {
        //TODO: Handle Mana Cost
        string cardVariantString = reader.GetString();
        int cardX = reader.GetInt();
        int cardY = reader.GetInt();
        if(Enum.TryParse<CardVariant>(cardVariantString, out var variant))
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
}