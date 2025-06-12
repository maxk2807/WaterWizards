using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.ServerGameStates;
using WaterWizard.Shared;

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
    public static readonly int boardWidth = 12;
    public static readonly int boardHeight = 10;
    public readonly Cell[][,] boards = new Cell[2][,];
    public Cell[,] Player1 => boards[0];
    public Cell[,] Player2 => boards[1];
    public readonly List<Cards>[] hands;
    private readonly NetManager server;
    private readonly ServerGameStateManager manager;
    public List<Cards> Player1Hand => hands[0];
    public List<Cards> Player2Hand => hands[1];
    public static List<Cards>? ActiveCards { get; private set; }
    public static List<Cards>? UtilityStack { get; private set; }
    public static List<Cards>? DamageStack { get; private set; }
    public static List<Cards>? EnvironmentStack { get; private set; }
    public static List<Cards>? Graveyard { get; private set; }

    private Timer activationTimer;
    public static float thunderTimer = 0f;
    public static float THUNDER_INTERVAL = 1.75f; // Intervall zwischen Blitzeinschlägen in Sekunden


    
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

        boards = CellHandler.InitBoards();
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

        CardHandler cardHandler = new(this);

        activationTimer = new Timer(_ => cardHandler.UpdateActiveCards(500), null, 0, 500);
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
                        CellHandler.SendCellReveal(attacker, defender, x, y, true);
                    }
                }
                else
                {
                    Console.WriteLine($"[Server] Cell ({x},{y}) already damaged");
                    CellHandler.SendCellReveal(attacker, defender, x, y, true);
                }
                break;
            }
        }

        if (!hit)
        {
            Console.WriteLine($"[Server] Miss at ({x},{y})");
            CellHandler.SendCellReveal(attacker, defender, x, y, false); // Updated to include defender
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

        Console.WriteLine(
            $"[Server] Attack result sent: attacker sees result, defender sees damage"
        );
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
}
