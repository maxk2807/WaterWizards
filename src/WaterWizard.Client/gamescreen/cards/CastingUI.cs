// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 210 Zeilen
// - erick: 141 Zeilen
// - justinjd00: 46 Zeilen
// - Erickk0: 23 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public static CastingUI Instance => instance ??= new();   (maxk2807: 13 Zeilen)
// - private Vector2 selectedShipCoords = new();   (maxk2807: 187 Zeilen)
// ===============================================

using System.Numerics;
using Raylib_cs;
using WaterWizard.Client.gamescreen.ships;
using WaterWizard.Client.network;
using WaterWizard.Shared;
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

    /// <summary>
    /// When Casting Battlefield Cards, checks if the first click on the card is over (mouse button is up again).
    /// False if not up yet, true if was up already
    /// </summary>
    private bool firstUpBattleField;

    private bool isTeleportSelectionPhase = false;
    private int selectedShipIndex = -1;
    private Vector2 selectedShipCoords = new();

    public GameHand? PlayerHand { get; private set; }

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
        // Sonderbehandlung für SummonShip: sofort casten, kein Ziel auswählen
        if (gameCard.card.Variant == WaterWizard.Shared.CardVariant.SummonShip)
        {
            aiming = false;
            NetworkManager.HandleCast(gameCard.card, new Point(0, 0));
            return;
        }

        var mousePos = Raylib.GetMousePosition();
        Vector2 aim = gameCard.card.TargetAsVector();

        // Spezialbehandlung für battlefield-Ziele wie Paralize
        if (gameCard.card.Target!.Target == "battlefield")
        {
            var txt = $"Klicken Sie irgendwo, um {gameCard.card.Variant} zu wirken";
            var txtWidth = Raylib.MeasureText(txt, 20);
            Raylib.DrawText(
                txt,
                (int)mousePos.X - (txtWidth / 2),
                (int)mousePos.Y - 20,
                20,
                Color.Black
            );
            if (!firstUpBattleField && Raylib.IsMouseButtonUp(MouseButton.Left))
            {
                firstUpBattleField = true;
            }
            else if (!firstUpBattleField && !Raylib.IsMouseButtonUp(MouseButton.Left))
            {
                return;
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Left))
            {
                aiming = false;
                NetworkManager.HandleCast(cardToAim!.card, new Point(0, 0));
            }
            return;
        }

        if (IsTargeted(aim))
        {
            HandleTargeted(gameCard, mousePos, aim);
        }
        else if (IsTeleportCard(gameCard))
        {
            HandleTeleportCard(gameCard, mousePos);
        }
        else if (IsTargetShip(gameCard))
        {
            // Console.WriteLine("Target Ship");
            HandleShipTargeted(gameCard, mousePos);
        }
        else
        {
            HandleNonTargeted(gameCard, mousePos);
        }
    }

    /// <summary>
    /// Handles the Drawing of the Cursor For Cards that Target Ships
    /// </summary>
    /// <param name="gameCard">Card to Cast</param>
    /// <param name="mousePos">Get from <see cref="Raylib.GetMousePosition()"/></param>
    private void HandleShipTargeted(GameCard gameCard, Vector2 mousePos)
    {
        Vector2 shipCoords = new();
        GameBoard board = gameCard.card.Target!.Ally
            ? GameScreen.playerBoard!
            : GameScreen.opponentBoard!;
        Point? hoveredCoords = board.GetCellFromScreenCoords(mousePos);
        Vector2 boardPos = board.Position;
        bool hoveringShip = false;
        if (hoveredCoords.HasValue)
        {
            var ship = GameScreen.playerBoard!.Ships.Find(ship =>
            {
                Console.WriteLine(
                    $"shipPos: {((ship.X - boardPos.X) / CellSize, (ship.Y - boardPos.Y) / CellSize)}"
                );
                Console.WriteLine($"shipBounds: {(ship.Width / CellSize, ship.Height / CellSize)}");
                Console.WriteLine(
                    $"hoveredCoords: {(hoveredCoords.Value.X, hoveredCoords.Value.Y)}"
                );
                return hoveredCoords.Value.X >= (ship.X - boardPos.X) / CellSize
                    && hoveredCoords.Value.X < (ship.X + ship.Width - boardPos.X) / CellSize
                    && hoveredCoords.Value.Y >= (ship.Y - boardPos.Y) / CellSize
                    && hoveredCoords.Value.Y < (ship.Y + ship.Height - boardPos.Y) / CellSize;
            });
            if (hoveringShip = ship != null)
            {
                var r = new Rectangle(ship!.X, ship.Y, ship.Width, ship.Height);
                Raylib.DrawRectangleLinesEx(r, 2, Color.Red);
                shipCoords = new(
                    (ship.X - board.Position.X) / CellSize,
                    (ship.Y - board.Position.Y) / CellSize
                );
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

        if (hoveringShip && Raylib.IsMouseButtonPressed(MouseButton.Left))
        {
            aiming = false;
            NetworkManager.HandleCast(cardToAim!.card, new((int)shipCoords.X, (int)shipCoords.Y));
        }
    }

    private static bool IsTargetShip(GameCard gameCard)
    {
        return gameCard.card.Target!.Target.Contains("ship");
    }

    /// <summary>
    /// Checks if the card is a teleport card.
    /// </summary>
    /// <param name="gameCard">The game card to check</param>
    /// <returns>True if the card is a teleport card, false otherwise</returns>
    private static bool IsTeleportCard(GameCard gameCard)
    {
        return gameCard.card.Variant == CardVariant.Teleport;
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
                if (gameCard.card.Variant == CardVariant.HoveringEye)
                {
                    Raylib.PlaySound(
                        WaterWizard.Client.Assets.Sounds.Manager.SoundManager.SweeperSound
                    );
                }
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
        return (int)aim.X > 0 && (int)aim.Y > 0;
    }

    /// <summary>
    /// Starts drawing the aiming UI for a card that's about to be cast.
    /// </summary>
    /// <param name="gameCard">The game card to aim</param>
    public void StartDrawingCardAim(GameCard gameCard)
    {
        firstUpBattleField = false;
        aiming = true;
        cardToAim = gameCard;

        isTeleportSelectionPhase = false;
        selectedShipIndex = -1;
    }

    /// <summary>
    /// Handles the teleport card casting UI which requires two steps:
    /// 1. Select a ship to teleport
    /// 2. Select a destination for that ship
    /// </summary>
    /// <param name="gameCard">The teleport card being cast</param>
    /// <param name="mousePos">Current mouse position</param>
    private void HandleTeleportCard(GameCard gameCard, Vector2 mousePos)
    {
        GameBoard board = GameScreen.playerBoard!;
        Point? hoveredCoords = board.GetCellFromScreenCoords(mousePos);
        Vector2 boardPos = board.Position;

        if (!isTeleportSelectionPhase)
        {
            string txt = "Select a ship to teleport";
            int txtWidth = Raylib.MeasureText(txt, 20);
            Raylib.DrawText(
                txt,
                (int)mousePos.X - (txtWidth / 2),
                (int)mousePos.Y - 20,
                20,
                Color.Black
            );

            bool hoveringShip = false;
            int shipIndex = -1;
            GameShip? hoveredShip = null;

            if (hoveredCoords.HasValue)
            {
                for (int i = 0; i < board.Ships.Count; i++)
                {
                    var ship = board.Ships[i];
                    bool isHovered =
                        hoveredCoords.Value.X >= (ship.X - boardPos.X) / CellSize
                        && hoveredCoords.Value.X < (ship.X + ship.Width - boardPos.X) / CellSize
                        && hoveredCoords.Value.Y >= (ship.Y - boardPos.Y) / CellSize
                        && hoveredCoords.Value.Y < (ship.Y + ship.Height - boardPos.Y) / CellSize;

                    if (isHovered)
                    {
                        hoveringShip = true;
                        hoveredShip = ship;
                        shipIndex = i;
                        break;
                    }
                }
            }

            if (hoveringShip && hoveredShip != null)
            {
                var r = new Rectangle(
                    hoveredShip.X,
                    hoveredShip.Y,
                    hoveredShip.Width,
                    hoveredShip.Height
                );
                Raylib.DrawRectangleLinesEx(r, 3, Color.Yellow);

                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    isTeleportSelectionPhase = true;
                    selectedShipIndex = shipIndex;
                    selectedShipCoords = new(
                        (hoveredShip.X - boardPos.X) / CellSize,
                        (hoveredShip.Y - boardPos.Y) / CellSize
                    );
                    Console.WriteLine(
                        $"[Client] Selected ship {shipIndex} at ({selectedShipCoords.X}, {selectedShipCoords.Y}) for teleport"
                    );
                }
            }
        }
        else
        {
            string txt = "Select a destination";
            int txtWidth = Raylib.MeasureText(txt, 20);
            Raylib.DrawText(
                txt,
                (int)mousePos.X - (txtWidth / 2),
                (int)mousePos.Y - 20,
                20,
                Color.Black
            );

            GameShip selectedShip = board.Ships[selectedShipIndex];
            var shipRect = new Rectangle(
                selectedShip.X,
                selectedShip.Y,
                selectedShip.Width,
                selectedShip.Height
            );
            Raylib.DrawRectangleLinesEx(shipRect, 3, Color.Yellow);

            if (hoveredCoords.HasValue)
            {
                float shipWidth = (float)selectedShip.Width / CellSize;
                float shipHeight = (float)selectedShip.Height / CellSize;

                float previewX = boardPos.X + hoveredCoords.Value.X * (float)CellSize;
                float previewY = boardPos.Y + hoveredCoords.Value.Y * (float)CellSize;

                var previewRect = new Rectangle(
                    previewX,
                    previewY,
                    shipWidth * CellSize,
                    shipHeight * CellSize
                );
                Raylib.DrawRectangleLinesEx(previewRect, 2, Color.Green);

                if (Raylib.IsMouseButtonPressed(MouseButton.Left))
                {
                    aiming = false;
                    isTeleportSelectionPhase = false;
                    Raylib.PlaySound(
                        WaterWizard.Client.Assets.Sounds.Manager.SoundManager.TeleportSound
                    );
                    NetworkManager.Instance.HandleTeleportCast(
                        cardToAim!.card,
                        selectedShipIndex,
                        new Point(hoveredCoords.Value.X, hoveredCoords.Value.Y)
                    );
                }
            }

            if (Raylib.IsMouseButtonPressed(MouseButton.Right))
            {
                isTeleportSelectionPhase = false;
                selectedShipIndex = -1;
            }
        }
    }

    /// <summary>
    /// Cancels the current card casting process and resets all casting-related state.
    /// This stops the aiming phase and clears any selected targets without executing the cast.
    /// </summary>
    public void CancelCasting()
    {
        if (aiming)
        {
            aiming = false;
            cardToAim = null;
            isTeleportSelectionPhase = false;
            selectedShipIndex = -1;
        }
    }
}
