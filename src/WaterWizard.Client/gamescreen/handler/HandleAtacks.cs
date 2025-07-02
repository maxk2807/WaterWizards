using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.network;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles attack-related messages received from the server.
/// </summary>
public class HandleAttacks
{
    readonly ClientService clientService = NetworkManager.Instance.clientService;

    /// <summary>
    /// Handles the attack result message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleAttackResult(NetPacketReader reader)
    {
        int x = reader.GetInt();
        int y = reader.GetInt();
        bool hit = reader.GetBool();
        bool shipDestroyed = reader.GetBool();
        bool isDefender = reader.GetBool();

        Console.WriteLine(
            $"[Client] HandleAttackResult: ({x},{y}) hit={hit} isDefender={isDefender}"
        );

        if (isDefender)
        {
            Console.WriteLine("[Client] Processing as DEFENDER");
            var playerBoard = GameStateManager.Instance.GameScreen?.playerBoard;
            if (playerBoard != null)
            {
                Console.WriteLine(
                    $"[Client] Before MarkCellAsHit - Cell state: {playerBoard._gridStates[x, y]}"
                );
                playerBoard.MarkCellAsHit(x, y, hit);
                Console.WriteLine(
                    $"[Client] After MarkCellAsHit - Cell state: {playerBoard._gridStates[x, y]}"
                );

                if (hit)
                {
                    foreach (var ship in playerBoard.Ships)
                    {
                        int cellSize = playerBoard.CellSize;
                        int shipCellX = (ship.X - (int)playerBoard.Position.X) / cellSize;
                        int shipCellY = (ship.Y - (int)playerBoard.Position.Y) / cellSize;
                        int shipWidth = ship.Width / cellSize;
                        int shipHeight = ship.Height / cellSize;

                        if (
                            x >= shipCellX
                            && x < shipCellX + shipWidth
                            && y >= shipCellY
                            && y < shipCellY + shipHeight
                        )
                        {
                            int relativeX = x - shipCellX;
                            int relativeY = y - shipCellY;
                            ship.AddDamage(relativeX, relativeY);

                            Console.WriteLine(
                                $"[Client] Our ship hit at ({x},{y})! Ship destroyed: {shipDestroyed}"
                            );
                            break;
                        }
                    }
                    Console.WriteLine($"[Client] Enemy hit us at ({x},{y})");
                }
                else
                {
                    Console.WriteLine($"[Client] Enemy missed at ({x},{y})");
                }
            }
            else
            {
                Console.WriteLine("[Client] ERROR: playerBoard is null!");
            }
        }
        else
        {
            var opponentBoard = GameStateManager.Instance.GameScreen?.opponentBoard;
            if (opponentBoard != null)
            {
                opponentBoard.SetCellState(
                    x,
                    y,
                    hit ? Gamescreen.CellState.Hit : Gamescreen.CellState.Miss
                );
                Console.WriteLine($"[Client] Our attack at ({x},{y}): {(hit ? "HIT" : "MISS")}");
            }
        }
    }

    /// <summary>
    /// Requests the server to initiate an attack at the specified coordinates.
    /// </summary>
    /// <param name="x">The X coordinate of the attack.</param>
    /// <param name="y">The Y coordinate of the attack.</param>
    public void SendAttack(int x, int y)
    {
        if (clientService.client != null && clientService.client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("Attack");
            writer.Put(x);
            writer.Put(y);
            clientService.client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Attack initiated at ({x}, {y})");
        }
    }

// float transparency = 0.5f;
//         writer.Put(transparency);

    public static void HandleCellReveal(NetPacketReader reader)
    {
        int revealX = reader.GetInt();
        int revealY = reader.GetInt();
        bool isHit = reader.GetBool();
        bool isDefender = reader.GetBool();

        var gameScreen = GameStateManager.Instance.GameScreen;
        if (gameScreen != null)
        {
            if (isDefender)
            {
                var playerBoard = gameScreen.playerBoard;
                if (playerBoard != null)
                {
                    playerBoard.MarkCellAsHit(revealX, revealY, isHit);

                    if (isHit)
                    {
                        foreach (var ship in playerBoard.Ships)
                        {
                            int cellSize = playerBoard.CellSize;
                            int shipCellX = (ship.X - (int)playerBoard.Position.X) / cellSize;
                            int shipCellY = (ship.Y - (int)playerBoard.Position.Y) / cellSize;
                            int shipWidth = ship.Width / cellSize;
                            int shipHeight = ship.Height / cellSize;

                            if (
                                revealX >= shipCellX
                                && revealX < shipCellX + shipWidth
                                && revealY >= shipCellY
                                && revealY < shipCellY + shipHeight
                            )
                            {
                                int relativeX = revealX - shipCellX;
                                int relativeY = revealY - shipCellY;
                                ship.AddDamage(relativeX, relativeY);
                                Console.WriteLine(
                                    $"[Client] Added damage to ship at ({relativeX},{relativeY})"
                                );
                                break;
                            }
                        }
                    }

                    Console.WriteLine(
                        $"[Client] Defender - Cell revealed on own board: ({revealX},{revealY}) = {(isHit ? "hit" : "miss")}"
                    );
                }
            }
            else
            {
                var opponentBoard = gameScreen.opponentBoard;
                if (opponentBoard != null)
                {
                    opponentBoard.SetCellState(
                        revealX,
                        revealY,
                        isHit ? Gamescreen.CellState.Hit : Gamescreen.CellState.Miss
                    );
                    Console.WriteLine(
                        $"[Client] Attacker - Cell revealed on opponent board: ({revealX},{revealY}) = {(isHit ? "hit" : "miss")}"
                    );
                }
            }
        }

        Console.WriteLine(
            $"[Client] Cell revealed: ({revealX},{revealY}) = {(isHit ? "hit" : "miss")} isDefender={isDefender}"
        );
    }
}
