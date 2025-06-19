using LiteNetLib;


namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles utility card-related messages received from the server.
/// </summary>
public class HandleUtility
{
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

        var gameScreen = GameStateManager.Instance.GameScreen;
        if (gameScreen != null)
        {
            if (isOpponent)
            {
                var playerBoard = gameScreen.playerBoard;
                if (playerBoard != null)
                {
                    playerBoard.MarkCellAsHoveringEyeRevealed(revealX, revealY, hasShip);
                    Console.WriteLine(
                        $"[Client] Opponent - HoveringEye revealed on own board: ({revealX},{revealY}) = {(hasShip ? "ship present" : "empty")}"
                    );
                }
            }
            else
            {
                var opponentBoard = gameScreen.opponentBoard;
                if (opponentBoard != null)
                {
                    opponentBoard.MarkCellAsHoveringEyeRevealed(revealX, revealY, hasShip);
                    Console.WriteLine(
                        $"[Client] Caster - HoveringEye revealed on opponent board: ({revealX},{revealY}) = {(hasShip ? "ship present" : "empty")}"
                    );
                }
            }
        }
    }
}