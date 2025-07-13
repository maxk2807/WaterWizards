// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 62 Zeilen
// - maxk2807: 2 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Shared;

namespace WaterWizard.Server.handler;

/// <summary>
/// Handles the Cell State for the Game
/// </summary>
public class CellHandler
{
    /// <summary>
    /// Initializes the Boards
    /// </summary>
    /// <returns>
    /// 3D Cell Array where the First Dimension is a 2 Element Array
    /// where the Elements correspond to the 2 Players
    /// </returns>
    public static Cell[][,] InitBoards()
    {
        Cell[,] player1Board = new Cell[GameState.boardWidth, GameState.boardHeight];
        Cell[,] player2Board = new Cell[GameState.boardWidth, GameState.boardHeight];
        for (int i = 0; i < GameState.boardWidth; i++)
        {
            for (int j = 0; j < GameState.boardHeight; j++)
            {
                player1Board[i, j] = new(CellState.Empty);
                player2Board[i, j] = new(CellState.Empty);
            }
        }
        return [player1Board, player2Board];
    }

    /// <summary>
    /// Reveals a specific cell to both attacker and defender
    /// </summary>
    /// <param name="attacker">The attacker who needs to see the result on opponent's board</param>
    /// <param name="defender">The defender who needs to see where they got attacked</param>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="isHit">Whether it was a hit or miss</param>
    /// <param name="cardVariant">Which Card is it?</param>
    public static void SendCellReveal(NetPeer attacker, NetPeer defender, int x, int y, bool isHit, string cardVariant)
    {
        var attackerWriter = new NetDataWriter();
        attackerWriter.Put("CellReveal");
        attackerWriter.Put(x);
        attackerWriter.Put(y);
        attackerWriter.Put(isHit);
        attackerWriter.Put(false);
        attackerWriter.Put(cardVariant);
        attacker.Send(attackerWriter, DeliveryMethod.ReliableOrdered);

        var defenderWriter = new NetDataWriter();
        defenderWriter.Put("CellReveal");
        defenderWriter.Put(x);
        defenderWriter.Put(y);
        defenderWriter.Put(isHit);
        defenderWriter.Put(true);
        defenderWriter.Put(cardVariant);
        defender.Send(defenderWriter, DeliveryMethod.ReliableOrdered);

        Console.WriteLine(
            $"[Server] Cell reveal sent to both players: ({x},{y}) = {(isHit ? "hit" : "miss")}"
        );
    }
}