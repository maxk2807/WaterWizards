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

    /// <summary>
    /// Draws the Aim when Casting. Handles Targeted Cards like Firebolt (target cells) or Heal (target Ships) differently from 
    /// Non-targeted like Thunder (random cells over a duration)
    /// </summary>
    /// <param name="gameCard">The Card to Cast</param>
    private void DrawCastAim(GameCard gameCard)
    {
        var mousePos = Raylib.GetMousePosition();
        Vector2 aim = gameCard.card.TargetAsVector();
        if (IsTargeted(aim))
        {
            HandleTargeted(gameCard, mousePos, aim);
        }
        else if (!IsTargeted(aim))
        {
            
        }
        else
        {
            HandleNonTargeted(gameCard, mousePos);
        }
    }

    /// <summary>
    /// Handles the Drawing of The Cursor for Targeted Cards, 
    /// i.e. when the Targets are Cells
    /// </summary>
    /// <param name="gameCard">The Card to Cast</param>
    /// <param name="mousePos">Get from <see cref="Raylib.GetMousePosition()"/></param>
    /// <param name="aim">Vector showing the size of the Cursor, get from <see cref="Cards.TargetAsVector"/></param>
    private void HandleTargeted(GameCard gameCard, Vector2 mousePos, Vector2 aim)
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

        if (hoveredCoords.HasValue)
        {
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

    /// <summary>
    /// Handles the Drawing of The Cursor for Non-Targeted Cards, 
    /// i.e. when the Targets are Random Cells or the entire Battlefield
    /// </summary>
    /// <param name="gameCard">Card to Cast</param>
    /// <param name="mousePos">Get from <see cref="Raylib.GetMousePosition()"/></param>
    private void HandleNonTargeted(GameCard gameCard, Vector2 mousePos)
    {
        Vector2 shipCoords = new();
        if (gameCard.card.Target!.Target.Contains("ship"))
        {
            GameBoard board = gameCard.card.Target.Ally ?
                GameScreen.playerBoard! :
                GameScreen.opponentBoard!;
            Point? hoveredCoords = board.GetCellFromScreenCoords(mousePos);
            Vector2 boardPos = board.Position;
            if (hoveredCoords.HasValue)
            {
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

    /// <summary>
    /// Whether the Cursor is Targeted, i.e. if the passed Vector is not 0
    /// </summary>
    /// <param name="aim">The Vector of the Cursor. example: 1x1 for Single Cell Target</param>
    /// <returns>Wether the Cursor is Targeted, i.e. targets Cells</returns>
    private static bool IsTargeted(Vector2 aim)
    {
        return (int)aim.X >= 0 && (int)aim.Y >= 0;
    }

    public void StartDrawingCardAim(GameCard gameCard)
    {
        aiming = true;
        cardToAim = gameCard;
    }
}
