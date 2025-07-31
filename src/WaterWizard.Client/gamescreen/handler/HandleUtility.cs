// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 138 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using WaterWizard.Client.gamescreen.ships;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles utility card-related messages received from the server.
/// </summary>
public class HandleUtility
{
    /// <summary>
    /// Handles the ship teleportation message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the teleport data</param>
    public static void HandleShipTeleported(NetPacketReader reader)
    {
        reader.GetInt();
        int originalX = reader.GetInt();
        int originalY = reader.GetInt();
        int newX = reader.GetInt();
        int newY = reader.GetInt();
        bool isOpponent = reader.GetBool();

        var gameScreen = GameStateManager.Instance.GameScreen;
        if (gameScreen != null)
        {
            if (isOpponent)
            {
                var opponentBoard = gameScreen.opponentBoard;
                if (opponentBoard != null)
                {
                    int originalPixelX =
                        (int)opponentBoard.Position.X + originalX * opponentBoard.CellSize;
                    int originalPixelY =
                        (int)opponentBoard.Position.Y + originalY * opponentBoard.CellSize;
                    int newPixelX = (int)opponentBoard.Position.X + newX * opponentBoard.CellSize;
                    int newPixelY = (int)opponentBoard.Position.Y + newY * opponentBoard.CellSize;

                    GameShip? shipToUpdate = null;
                    foreach (var ship in opponentBoard.Ships)
                    {
                        if (ship.X == originalPixelX && ship.Y == originalPixelY)
                        {
                            shipToUpdate = ship;
                            break;
                        }
                    }

                    if (shipToUpdate != null)
                    {
                        shipToUpdate.X = newPixelX;
                        shipToUpdate.Y = newPixelY;

                        Console.WriteLine(
                            $"[Client] Opponent teleported ship from grid({originalX},{originalY}) to grid({newX},{newY}), pixels({originalPixelX},{originalPixelY}) to pixels({newPixelX},{newPixelY})"
                        );
                    }
                    else
                    {
                        Console.WriteLine(
                            $"[Client] Could not find opponent ship at pixel position ({originalPixelX},{originalPixelY})"
                        );
                    }
                }
            }
            else
            {
                var playerBoard = gameScreen.playerBoard;
                if (playerBoard != null)
                {
                    int originalPixelX =
                        (int)playerBoard.Position.X + originalX * playerBoard.CellSize;
                    int originalPixelY =
                        (int)playerBoard.Position.Y + originalY * playerBoard.CellSize;
                    int newPixelX = (int)playerBoard.Position.X + newX * playerBoard.CellSize;
                    int newPixelY = (int)playerBoard.Position.Y + newY * playerBoard.CellSize;

                    GameShip? shipToUpdate = null;
                    foreach (var ship in playerBoard.Ships)
                    {
                        if (ship.X == originalPixelX && ship.Y == originalPixelY)
                        {
                            shipToUpdate = ship;
                            break;
                        }
                    }

                    if (shipToUpdate != null)
                    {
                        shipToUpdate.X = newPixelX;
                        shipToUpdate.Y = newPixelY;

                        Console.WriteLine(
                            $"[Client] You teleported ship from grid({originalX},{originalY}) to grid({newX},{newY}), pixels({originalPixelX},{originalPixelY}) to pixels({newPixelX},{newPixelY})"
                        );
                    }
                    else
                    {
                        Console.WriteLine(
                            $"[Client] Could not find your ship at pixel position ({originalPixelX},{originalPixelY})"
                        );
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handles the hovering eye reveal message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the reveal data</param>
    public static void HandleHoveringEyeReveal(NetPacketReader reader)
    {
        int revealX = reader.GetInt();
        int revealY = reader.GetInt();
        bool hasShip = reader.GetBool();
        bool isOpponent = reader.GetBool();

        Console.WriteLine($"[CLIENT] HoveringEye message received: ({revealX},{revealY}) hasShip={hasShip} isOpponent={isOpponent}");

        var gameScreen = GameStateManager.Instance.GameScreen;
        if (gameScreen != null)
        {
            if (isOpponent)
            {
                var opponentBoard = gameScreen.opponentBoard;
                if (opponentBoard != null)
                {
                    opponentBoard.MarkCellAsHoveringEyeRevealed(revealX, revealY, hasShip);
                    Console.WriteLine($"[CLIENT] Updated opponent board cell ({revealX},{revealY}) with hasShip={hasShip}");
                }
            }
            else
            {
                var playerBoard = gameScreen.playerBoard;
                if (playerBoard != null)
                {
                    if (!hasShip)
                    {
                        playerBoard.MarkCellAsHoveringEyeRevealed(revealX, revealY, hasShip);
                    }
                    Console.WriteLine($"[CLIENT] Updated player board cell ({revealX},{revealY}) with hasShip={hasShip}");
                }
            }
        }
    }
}
