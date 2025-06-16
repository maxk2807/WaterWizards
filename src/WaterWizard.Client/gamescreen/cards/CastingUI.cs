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

        // Spezialbehandlung für battlefield-Ziele wie Paralize
        if (gameCard.card.Target!.Target == "battlefield")
        {
            Raylib.DrawText(
                $"Klicken Sie irgendwo, um {gameCard.card.Variant} zu wirken",
                (int)mousePos.X - 100,
                (int)mousePos.Y - 20,
                20,
                Color.Black
            );

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                aiming = false;
                NetworkManager.HandleCast(cardToAim!.card, new Point(0, 0));
            }
            return;
        }

        if ((int)aim.X == 0 && (int)aim.Y == 0)
        {
            Vector2 shipCoords = new();
            if (gameCard.card.Target!.Target.Contains("ship"))
            {
                GameBoard board = gameCard.card.Target.Ally ?
                    GameScreen.playerBoard! :
                    GameScreen.opponentBoard!;
                Point? hoveredCoords = board.GetCellFromScreenCoords(mousePos);
                Vector2 boardPos = board.Position;
                if (!hoveredCoords.HasValue)
                    return;

                var ship = GameScreen.playerBoard!.Ships.Find(ship =>
                {
                    return hoveredCoords.Value.X >= (ship.X - boardPos.X) / CellSize &&
                        hoveredCoords.Value.X < (ship.X + ship.Width - boardPos.X) / CellSize &&
                        hoveredCoords.Value.Y >= (ship.Y - boardPos.Y) / CellSize &&
                        hoveredCoords.Value.Y < (ship.Y + ship.Height - boardPos.Y) / CellSize;
                });
                if (ship != null)
                {
                    var r = new Rectangle(ship.X, ship.Y, ship.Width, ship.Height);
                    Raylib.DrawRectangleLinesEx(r, 2, Color.Red);
                    shipCoords = new((ship.X - board.Position.X) / CellSize, (ship.Y - board.Position.Y) / CellSize);
                }
            }
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
                NetworkManager.HandleCast(cardToAim!.card, new((int)shipCoords.X, (int)shipCoords.Y));
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
