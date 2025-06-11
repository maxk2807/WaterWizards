using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.network;
using static WaterWizard.Client.gamescreen.GameBoard;

namespace WaterWizard.Client.gamescreen.cards;

public class CastingUI
{
    public static CastingUI Instance => instance ??= new();
    private static CastingUI? instance;

    private GameScreen GameScreen => GameStateManager.Instance.GameScreen;
    private int CellSize => GameScreen.playerBoard!.CellSize;

    private bool aiming = false;
    private GameCard? cardToAim;

    public void Draw()
    {
        if (aiming)
        {
            DrawCastAim(cardToAim!);
        }
    }

    private void DrawCastAim(GameCard gameCard)
    {
        var mousePos = Raylib.GetMousePosition();
        Vector2 aim = gameCard.card.TargetAsVector();
        if ((int)aim.X == 0 && (int)aim.Y == 0)
        {
            var txt = $"Click to cast {gameCard.card.Variant}";
            var txtWidth = Raylib.MeasureText(txt, 20);
            Raylib.DrawText(
                txt,
                (int)mousePos.X - (txtWidth / 2),
                (int)mousePos.Y - 20,
                20,
                Color.Black
            );

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                aiming = false;
                NetworkManager.HandleCast(cardToAim!.card, new());
            }
        }
        else
        {
            Point? hoveredCoords;
            Vector2 boardPos;
            if (gameCard.card.Target!.Ally)
            {
                hoveredCoords = GameScreen.playerBoard!.GetCellFromScreenCoords(mousePos);
                boardPos = GameScreen.playerBoard.Position;
            }
            else
            {
                hoveredCoords =
                    GameStateManager.Instance.GameScreen.opponentBoard!.GetCellFromScreenCoords(
                        mousePos
                    );
                boardPos = GameStateManager.Instance.GameScreen.opponentBoard!.Position;
            }
            var txt = $"Aim & Click to cast {gameCard.card.Variant}";
            var txtWidth = Raylib.MeasureText(txt, 20);
            Raylib.DrawText(
                txt,
                (int)mousePos.X - (txtWidth / 2),
                (int)mousePos.Y - 20,
                20,
                Color.Black
            );

            if (!hoveredCoords.HasValue)
                return;

            var onScreenX =
                boardPos.X + (hoveredCoords.Value.X - (float)Math.Floor(aim.X / 2f)) * CellSize;
            var onScreenY =
                boardPos.Y + (hoveredCoords.Value.Y - (float)Math.Floor(aim.Y / 2f)) * CellSize;
            var r = new Rectangle(onScreenX, onScreenY, aim.X * CellSize, aim.Y * CellSize);
            Raylib.DrawRectangleLinesEx(r, 2, Color.Red);

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                aiming = false;
                NetworkManager.HandleCast(cardToAim!.card, hoveredCoords.Value);
            }
        }
    }

    public void StartDrawingCardAim(GameCard gameCard)
    {
        aiming = true;
        cardToAim = gameCard;
    }
}
