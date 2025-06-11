using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.ServerGameStates;
using WaterWizard.Shared;
using WaterWizard.Server.handler;

namespace WaterWizard.Server;

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
    public NetPeer[] players = new NetPeer[2];
    private static readonly int boardWidth = 12;
    private static readonly int boardHeight = 10;
    public readonly Cell[][,] boards = new Cell[2][,];
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
    private float thunderTimer = 0f;
    private const float THUNDER_INTERVAL = 1.75f; // Intervall zwischen Blitzeinschlägen in Sekunden


    
    public Mana Player1Mana { get; private set; } = new();
    public Mana Player2Mana { get; private set; } = new();
    public int Player1Gold { get; private set; } = 0;
    public int Player2Gold { get; private set; } = 0;

    public bool IsPlacementPhase()
    {
        return manager.CurrentState is PlacementState;
    }

    public void SetGold(int playerIndex, int amount)
    {
        if (playerIndex == 0)
            Player1Gold = amount;
        else if (playerIndex == 1)
            Player2Gold = amount;
    }

    /// <summary>
    /// Returns the NetPeer instance for the given player index (0 or 1).
    /// </summary>
    /// <param name="index">The index of the player (0 = Player 1, 1 = Player 2).</param>
    /// <returns>The NetPeer associated with the specified player index.</returns>
    /// <exception cref="InvalidOperationException">Thrown if no player is assigned at the given index.</exception>
    public NetPeer GetPlayer(int index)
    {
        if (index < 0 || index >= players.Length || players[index] == null)
            throw new InvalidOperationException($"No player at index {index}");

        return players[index];
    }

// TODO: für HandleCardBuying() gut verwendbar
    public void SyncGoldToClient(int playerIndex)
    {
        var peer = GetPlayer(playerIndex);
        var writer = new NetDataWriter();
        writer.Put("UpdateGold");
        writer.Put(playerIndex);
        writer.Put(playerIndex == 0 ? Player1Gold : Player2Gold);
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    public GameState(NetManager server, ServerGameStateManager manager)
    {
        int connectedCount = server.ConnectedPeerList.Count;
        if (connectedCount < 1 || connectedCount > 2)
            throw new InvalidOperationException("Game requires 1 or 2 connected players.");

        players = new NetPeer[2]; 
        for (int i = 0; i < connectedCount; i++)
            players[i] = server.ConnectedPeerList[i];
        
        for (int i = connectedCount; i < 2; i++)
            players[i] = null;

        Console.WriteLine("\n[Server] Initial Player Setup:");
        Console.WriteLine("----------------------------------------");
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null)
            {
                Console.WriteLine($"Player {i + 1}: {players[i]}");
                Console.WriteLine($"  - Owns Board[{i}]");
                
                if (connectedCount > 1)
                {
                    var opponentIndex = i == 0 ? 1 : 0;
                    Console.WriteLine($"  - Opponent: {players[opponentIndex]}");
                    Console.WriteLine($"  - Opponent's Board: Board[{opponentIndex}]");
                }
                else
                {
                    Console.WriteLine($"  - No opponent (single player mode)");
                }
            }
            else
            {
                Console.WriteLine($"Player {i + 1}: [Empty Slot]");
            }
        }
        Console.WriteLine("----------------------------------------\n");

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

        activationTimer = new Timer(_ => UpdateActiveCards(500), null, 0, 500);
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
        ActiveCards.Add(new(variant) { remainingDuration = duration * 1000f });
        
        // Sofort die aktive Karte an alle Clients senden
        foreach (var player in players)
        {
            SendActiveCardsUpdate(player);
        }
    }

    private void UpdateActiveCards(float passedTime)
    {
        for (int i = ActiveCards.Count - 1; i >= 0; i--)
        {
            var card = ActiveCards[i];
            card.remainingDuration -= passedTime;

            if (card.remainingDuration <= 0)
            {
                // Karte ist abgelaufen
                ActiveCards.RemoveAt(i);
                
                foreach (var player in players)
                {
                    SendActiveCardsUpdate(player);
                }
                
                if (card.Variant == CardVariant.Thunder)
                {
                    Console.WriteLine("\n[Server] Thunder Card expired");
                    Console.WriteLine("----------------------------------------");
                    foreach (var player in players)
                    {
                        NetDataWriter resetWriter = new();
                        resetWriter.Put("ThunderReset");
                        player.Send(resetWriter, DeliveryMethod.ReliableOrdered);
                        Console.WriteLine($"Sent ThunderReset to player: {player}");
                    }
                    Console.WriteLine("----------------------------------------\n");
                }
                continue;
            }

            CardAbilities.HandleActivationEffect(card, passedTime);

            if (card.Variant == CardVariant.Thunder)
            {
                thunderTimer -= passedTime / 1000f;
                
                if (thunderTimer <= 0)
                {
                    thunderTimer = THUNDER_INTERVAL;
                    Console.WriteLine("\n[Server] Thunder Strike Round");
                    Console.WriteLine("----------------------------------------");
                    
                    for (int playerIndex = 0; playerIndex < 2; playerIndex++)
                    {
                        var targetPlayer = players[playerIndex];
                        Console.WriteLine($"\nGenerating strike for Board[{playerIndex}] owned by {targetPlayer}");
                        
                        int x = Random.Shared.Next(0, boardWidth);
                        int y = Random.Shared.Next(0, boardHeight);
                        Console.WriteLine($"Strike coordinates: ({x}, {y})");

                        bool hit = ShipHandler.GetShips(targetPlayer).Any(ship =>
                            x >= ship.X && x < ship.X + ship.Width &&
                            y >= ship.Y && y < ship.Y + ship.Height
                        );

                        // Detailed strike information
                        Console.WriteLine($"Strike Result:");
                        Console.WriteLine($"  - Target Board: Board[{playerIndex}]");
                        Console.WriteLine($"  - Board Owner: {targetPlayer}");
                        Console.WriteLine($"  - Coordinates: ({x}, {y})");
                        Console.WriteLine($"  - Hit: {(hit ? "YES" : "NO")}");

                        NetDataWriter thunderWriter = new();
                        thunderWriter.Put("ThunderStrike");
                        thunderWriter.Put(targetPlayer.ToString());
                        thunderWriter.Put(x);
                        thunderWriter.Put(y);
                        thunderWriter.Put(hit);

                        // Log message sending
                        Console.WriteLine("\nSending strike information to players:");
                        foreach (var player in players)
                        {
                            player.Send(thunderWriter, DeliveryMethod.ReliableOrdered);
                            Console.WriteLine($"  - Sent to {player}");
                            Console.WriteLine($"    - {(player == targetPlayer ? "This is their board" : "This is their opponent's board")}");
                            Console.WriteLine($"    - They should show this on their {(player == targetPlayer ? "playerBoard" : "opponentBoard")}");
                        }

                        if (hit)
                        {
                            var hitShip = ShipHandler.GetShips(targetPlayer).First(ship =>
                                x >= ship.X && x < ship.X + ship.Width &&
                                y >= ship.Y && y < ship.Y + ship.Height
                            );
                            Console.WriteLine($"\nHit Details:");
                            Console.WriteLine($"  - Hit ship at position: ({hitShip.X}, {hitShip.Y})");
                            Console.WriteLine($"  - Ship size: {hitShip.Width}x{hitShip.Height}");
                            Console.WriteLine($"  - Current damage: {hitShip.DamagedCells.Count}/{hitShip.MaxHealth}");
                        }
                    }
                    Console.WriteLine("----------------------------------------\n");
                }
            }
        }
    }

    /// <summary>
    /// Handles the attack from one player to another.
    /// Checks if the attack hits a ship and updates the game state accordingly.
    /// If a ship is hit, it checks if the ship is destroyed and sends the result to both players.
    /// </summary>
    /// <param name="attacker">The player who initiated the attack</param>
    /// <param name="defender">The player who was attacked</param>
    /// <param name="x">The x-coordinate of the attack</param>
    /// <param name="y">The y-coordinate of the attack</param>
    public void HandleAttack(NetPeer attacker, NetPeer defender, int x, int y)
    {
        Console.WriteLine(
            $"[Server] HandleAttack called: attacker={attacker}, defender={defender}, coords=({x},{y})"
        );

        var ships = ShipHandler.GetShips(defender);
        bool hit = false;
        PlacedShip? hitShip = null;

        foreach (var ship in ships)
        {
            if (x >= ship.X && x < ship.X + ship.Width && y >= ship.Y && y < ship.Y + ship.Height)
            {
                hit = true;
                hitShip = ship;

                bool newDamage = ship.DamageCell(x, y);

                if (newDamage)
                {
                    Console.WriteLine(
                        $"[Server] New damage at ({x},{y}) on ship at ({ship.X},{ship.Y})"
                    );

                    if (ship.IsDestroyed)
                    {
                        Console.WriteLine($"[Server] Ship at ({ship.X},{ship.Y}) destroyed!");
                        ShipHandler.SendShipReveal(attacker, ship);
                    }
                    else
                    {
                        SendCellReveal(attacker, defender, x, y, true); 
                    }
                }
                else
                {
                    Console.WriteLine($"[Server] Cell ({x},{y}) already damaged");
                    SendCellReveal(attacker, defender, x, y, true); 
                }
                break;
            }
        }

        if (!hit)
        {
            Console.WriteLine($"[Server] Miss at ({x},{y})");
            SendCellReveal(attacker, defender, x, y, false); // Updated to include defender
        }

        /// <summary>
        /// Sends the result of the attack to both players.
        /// /// </summary>
        /// <param name="attacker">The player who initiated the attack</param>
        /// <param name="defender">The player who was attacked</param>
        /// <param name="x">The x-coordinate of the attack</param>
        /// <param name="y">The y-coordinate of the attack</param>
        /// <param name="hit">Whether the attack hit a ship</param>
        /// <param name="shipDestroyed">Whether the ship was destroyed</param>
        SendAttackResult(attacker, defender, x, y, hit, hitShip?.IsDestroyed ?? false);

        if (hit && hitShip?.IsDestroyed == true)
        {
            CheckGameOver();
        }
    }

    /// <summary>
    /// Reveals a specific cell to both attacker and defender
    /// </summary>
    /// <param name="attacker">The attacker who needs to see the result on opponent's board</param>
    /// <param name="defender">The defender who needs to see where they got attacked</param>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="isHit">Whether it was a hit or miss</param>
    public void SendCellReveal(NetPeer attacker, NetPeer defender, int x, int y, bool isHit)
    {
        var attackerWriter = new NetDataWriter();
        attackerWriter.Put("CellReveal");
        attackerWriter.Put(x);
        attackerWriter.Put(y);
        attackerWriter.Put(isHit);
        attackerWriter.Put(false); 
        attacker.Send(attackerWriter, DeliveryMethod.ReliableOrdered);

        var defenderWriter = new NetDataWriter();
        defenderWriter.Put("CellReveal");
        defenderWriter.Put(x);
        defenderWriter.Put(y);
        defenderWriter.Put(isHit);
        defenderWriter.Put(true); 
        defender.Send(defenderWriter, DeliveryMethod.ReliableOrdered);

        Console.WriteLine(
            $"[Server] Cell reveal sent to both players: ({x},{y}) = {(isHit ? "hit" : "miss")}"
        );
    }

    

    /// <summary>
    /// Sends the result of an attack to both players.
    /// </summary>
    /// <param name="attacker">The player who initiated the attack</param>
    /// <param name="defender">The player who was attacked</param>
    /// <param name="x">The x-coordinate of the attack</param>
    /// <param name="y">The y-coordinate of the attack</param>
    /// <param name="hit">Whether the attack hit a ship</param>
    /// <param name="shipDestroyed">Whether the ship was destroyed</param>
    private void SendAttackResult(
        NetPeer attacker,
        NetPeer defender,
        int x,
        int y,
        bool hit,
        bool shipDestroyed
    )
    {
        var attackerWriter = new NetDataWriter();
        attackerWriter.Put("AttackResult");
        attackerWriter.Put(x);
        attackerWriter.Put(y);
        attackerWriter.Put(hit);
        attackerWriter.Put(shipDestroyed);
        attackerWriter.Put(false); 
        attacker.Send(attackerWriter, DeliveryMethod.ReliableOrdered);

        var defenderWriter = new NetDataWriter();
        defenderWriter.Put("AttackResult");
        defenderWriter.Put(x);
        defenderWriter.Put(y);
        defenderWriter.Put(hit);
        defenderWriter.Put(shipDestroyed);
        defenderWriter.Put(true); 
        defender.Send(defenderWriter, DeliveryMethod.ReliableOrdered);

        Console.WriteLine($"[Server] Attack result sent: attacker sees result, defender sees damage");
    }

    /// <summary>
    /// Checks if the game is over by checking if any player's ships are all destroyed.
    /// If so, it broadcasts the game over message to all players.
    /// </summary>
    public void CheckGameOver()
    {
        foreach (var player in players)
        {
            if (player != null && ShipHandler.AreAllShipsDestroyed(player))
            {
                var winner = players.FirstOrDefault(p => p != player);
                if (winner != null)
                {
                    BroadcastGameOver(winner, player);
                }
            }
        }
    }

    /// <summary>
    /// Broadcasts the game over message to all players.
    /// </summary>
    /// <param name="winner">The winning player</param>
    /// <param name="loser">The losing player</param>
    private void BroadcastGameOver(NetPeer winner, NetPeer loser)
    {
        // Send to winner
        var winnerWriter = new NetDataWriter();
        winnerWriter.Put("GameOver");
        winnerWriter.Put("Victory");
        winner.Send(winnerWriter, DeliveryMethod.ReliableOrdered);

        // Send to loser
        var loserWriter = new NetDataWriter();
        loserWriter.Put("GameOver");
        loserWriter.Put("Defeat");
        loser.Send(loserWriter, DeliveryMethod.ReliableOrdered);

        Console.WriteLine($"[Server] Game Over! Winner: {winner}, Loser: {loser}");

        var gameOverTimer = new Timer(
            _ =>
            {
                Console.WriteLine("[Server] Transitioning back to lobby after game over...");
                manager.ChangeState(new LobbyState(server));

                foreach (var playerKey in Program.ConnectedPlayers.Keys.ToList())
                {
                    Program.ConnectedPlayers[playerKey] = false;
                }
                Program.PlacementReadyPlayers.Clear();

                ShipHandler.playerShips.Clear();
                Console.WriteLine("[Server] Ship placements cleared for next game.");

                var playerListWriter = new NetDataWriter();
                playerListWriter.Put("PlayerList");
                playerListWriter.Put(Program.ConnectedPlayers.Count);
                foreach (var kvp in Program.ConnectedPlayers)
                {
                    string playerName = Program.PlayerNames.GetValueOrDefault(kvp.Key, "Unknown");
                    playerListWriter.Put(kvp.Key);
                    playerListWriter.Put(playerName);
                    playerListWriter.Put(kvp.Value);
                }

                foreach (var peer in server.ConnectedPeerList)
                {
                    peer.Send(playerListWriter, DeliveryMethod.ReliableOrdered);
                }

                Console.WriteLine("[Server] Players reset to not ready, returned to lobby.");

                if (_ is Timer timer)
                {
                    timer.Dispose();
                }
            },
            null,
            500,
            Timeout.Infinite
        );
    }

    private void SendActiveCardsUpdate(NetPeer player)
    {
        var writer = new NetDataWriter();
        writer.Put("ActiveCards");
        writer.Put(ActiveCards.Count);
        foreach (var card in ActiveCards)
        {
            writer.Put(card.Variant.ToString());
            writer.Put(card.remainingDuration);
        }
        player.Send(writer, DeliveryMethod.ReliableOrdered);
    }
}
