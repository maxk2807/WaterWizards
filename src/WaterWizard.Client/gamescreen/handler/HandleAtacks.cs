using LiteNetLib;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles attack-related messages received from the server.
/// </summary>
public class HandleAttacks
{
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

        if (isDefender)
        {
            if (hit)
            {
                var playerBoard = GameStateManager.Instance.GameScreen?.playerBoard;
                if (playerBoard != null)
                {
                    playerBoard.MarkCellAsHit(x, y, true);

                    foreach (var ship in playerBoard.Ships)
                    {
                        int cellSize = playerBoard.CellSize;
                        int shipCellX = (ship.X - (int)playerBoard.Position.X) / cellSize;
                        int shipCellY = (ship.Y - (int)playerBoard.Position.Y) / cellSize;
                        int shipWidth = ship.Width / cellSize;
                        int shipHeight = ship.Height / cellSize;

                        if (x >= shipCellX && x < shipCellX + shipWidth &&
                            y >= shipCellY && y < shipCellY + shipHeight)
                        {
                            int relativeX = x - shipCellX;
                            int relativeY = y - shipCellY;
                            ship.AddDamage(relativeX, relativeY);

                            Console.WriteLine($"[Client] Our ship hit at ({x},{y})! Ship destroyed: {shipDestroyed}");
                            break;
                        }
                    }
                }
            }
            else
            {
                var playerBoard = GameStateManager.Instance.GameScreen?.playerBoard;
                playerBoard?.MarkCellAsHit(x, y, false);
                Console.WriteLine($"[Client] Enemy missed at ({x},{y})");
            }
        }
        else
        {
            var opponentBoard = GameStateManager.Instance.GameScreen?.opponentBoard;
            if (opponentBoard != null)
            {
                opponentBoard.SetCellState(x, y, hit ? Gamescreen.CellState.Hit : Gamescreen.CellState.Miss);
                Console.WriteLine($"[Client] Our attack at ({x},{y}): {(hit ? "HIT" : "MISS")}");
            }
        }
    }
}