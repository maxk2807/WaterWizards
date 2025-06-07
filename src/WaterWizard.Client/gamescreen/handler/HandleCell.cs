using LiteNetLib;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles the cell position message received from the server.
/// </summary>
public class HandleCell
{
    /// <summary>
    /// Handles the cell reveal message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleCellReveal(NetPacketReader reader)
    {
        int x = reader.GetInt();
        int y = reader.GetInt();
        bool isHit = reader.GetBool();

        var opponentBoard = GameStateManager.Instance.GameScreen!.opponentBoard;
        if (opponentBoard != null)
        {
            opponentBoard.SetCellState(
                x,
                y,
                isHit ? Gamescreen.CellState.Hit : Gamescreen.CellState.Miss
            );
            Console.WriteLine($"[Client] Cell revealed: ({x},{y}) = {(isHit ? "hit" : "miss")}");
        }
    }
}