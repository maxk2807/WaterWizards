// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 72 Zeilen
// - Erickk0: 39 Zeilen
// - jdewi001: 33 Zeilen
// - erick: 11 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using Raylib_cs;
using WaterWizard.Shared;

namespace WaterWizard.Client.gamescreen.cards;

public class ActiveCards(GameScreen gameScreen)
{
    private ActiveCardsHand? _cards;
    public List<GameCard> Cards => _cards?.Cards ?? [];

    private int X,
        Y;
    private int Width,
        Height;
    private int ScreenWidth => gameScreen._gameStateManager.screenWidth;
    private int ScreenHeight => gameScreen._gameStateManager.screenHeight;

    public void Initialize()
    {
        Width = (int)(ScreenWidth * 0.274f);
        Height = (int)(ScreenHeight * 0.25f);
        X = (int)(ScreenWidth * 0.725f);
        Y = (ScreenHeight - Height) / 2;
        _cards = new(gameScreen, X + Width / 2, Y + Height / 2 - gameScreen.cardHeight / 2);
        _cards.EmptyHand();
    }

    public void Draw()
    {
        Rectangle outerRec = new(X, Y, Width, Height);
        Raylib.DrawRectangleRec(outerRec, Color.White);
        Raylib.DrawRectangleLinesEx(outerRec, 2, Color.Black);
        Raylib.DrawText("Active Cards", (int)outerRec.X + 2, (int)outerRec.Y + 2, 15, Color.Black);

        _cards?.Draw(true);
    }

    internal void UpdateActiveCards(List<Cards> activeCards)
    {
        _cards!.EmptyHand();
        activeCards.ForEach(_cards.AddCard);
    }

    private class ActiveCardsHand(GameScreen gameScreen, int centralX, int cardY)
        : GameHand(gameScreen, centralX, cardY)
    {
        private readonly GameScreen gameScreen = gameScreen;
        private readonly int centralX = centralX;
        private readonly int cardY = cardY;

        internal override void HandleCast(
            GameCard gameCard
        ) { /*Can't cast active cards*/
        }

        public override void Draw(bool front)
        {
            int availableHandWidth = (int)(gameScreen._gameStateManager.screenWidth * 0.2f);
            int totalCardWidth = Cards.Count * gameScreen.cardWidth;
            int excess = totalCardWidth - availableHandWidth;
            int offset = excess > 0 ? excess / Cards.Count : 0;
            for (int i = 0; i < Cards.Count; i++)
            {
                int cardX;
                int effectiveCardWidth = gameScreen.cardWidth - offset;
                bool areCardsEven = Cards.Count % 2 == 0;
                cardX = areCardsEven
                    ? -(Cards.Count / 2 * effectiveCardWidth) + i * effectiveCardWidth
                    : -effectiveCardWidth / 2
                        - Cards.Count / 2 * effectiveCardWidth
                        + i * effectiveCardWidth;

                Cards[i].Draw(centralX + cardX, cardY, front);

                var card = Cards[i].card;
                int radius = 15;

                // Zeichne den äußeren Kreis
                Raylib.DrawCircleLines(
                    centralX + cardX + radius,
                    cardY + radius,
                    radius,
                    Color.Black
                );

                // Berechne den Fortschritt für die Karte
                if (int.TryParse(card.Duration, out int totalDuration) && totalDuration > 0)
                {
                    float remainingSeconds = card.remainingDuration / 1000f;
                    float progress = remainingSeconds / totalDuration;

                    if (card.remainingDuration <= 0)
                    {
                        Raylib.DrawCircleSector(
                            new(centralX + cardX + radius, cardY + radius),
                            radius,
                            0,
                            360,
                            100,
                            Color.Gray
                        );
                    }
                    else
                    {
                        progress = Math.Max(0, Math.Min(1, progress));
                        int degrees = (int)(progress * 360);

                        if (degrees > 0)
                        {
                            Raylib.DrawCircleSector(
                                new(centralX + cardX + radius, cardY + radius),
                                radius,
                                0,
                                degrees,
                                100,
                                Color.Black
                            );
                        }
                    }
                }
                else if (card.Duration == "permanent")
                {
                    Raylib.DrawCircleSector(
                        new(centralX + cardX + radius, cardY + radius),
                        radius,
                        0,
                        360,
                        100,
                        Color.Black
                    );
                }
                else if (card.Duration == "instant")
                {
                    Raylib.DrawCircleSector(
                        new(centralX + cardX + radius, cardY + radius),
                        radius,
                        0,
                        360,
                        100,
                        Color.Yellow
                    );
                }
            }
        }
    }

    /// <summary>
    /// Updates the active cards, reducing their remaining duration.
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds</param>
    public void Update(float deltaTime)
    {
        if (_cards == null || _cards.Cards.Count == 0)
            return;

        for (int i = _cards.Cards.Count - 1; i >= 0; i--)
        {
            var gameCard = _cards.Cards[i];
            
            if (gameCard.card.Duration != "permanent" && gameCard.card.Duration != "instant")
            {
                if (gameCard.card.remainingDuration > 0)
                {
                    // Update for visual countdown only
                    gameCard.card.remainingDuration -= deltaTime * 1000; 
                    
                    Console.WriteLine($"[Client] Card {gameCard.card.Variant} remaining: {gameCard.card.remainingDuration}ms");
                    
                    if (gameCard.card.remainingDuration <= 0)
                    {
                        Console.WriteLine($"[Client] Card {gameCard.card.Variant} display countdown finished");
                        gameCard.card.remainingDuration = 0;
                    }
                }
            }
        }
    }
}
