// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 152 Zeilen
// - Erickk0: 50 Zeilen
// - jlnhsrm: 28 Zeilen
// - justinjd00: 17 Zeilen
// - maxk2807: 13 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Client.gamescreen.cards;
using WaterWizard.Server.Card.environment;
using WaterWizard.Server.handler;
using WaterWizard.Server.ServerGameStates;
using WaterWizard.Shared;

namespace WaterWizard.Server.handler;

/// <summary>
/// Verwaltet das Kaufen und Ausspielen von Karten sowie die Kommunikation mit den Clients bezüglich Kartenaktionen.
/// </summary>
public class CardHandler(GameState gameState)
{
    private readonly GameState? gameState = gameState;

    /// <summary>
    /// Verarbeitet den Kauf einer Karte durch einen Client und sendet die Karte zurück.
    /// </summary>
    /// <param name="peer">The <see cref="NetPeer"/> Client sending the Placement Request</param>
    /// <param name="reader"><see cref="NetPacketReader"/> with the Request Data</param>
    public static void HandleCardBuying(
        NetManager server,
        NetPeer peer,
        NetPacketReader reader,
        GameState gameState
    )
    {
        string cardType = reader.GetString();
        Console.WriteLine($"[Server] Trying to Buy {cardType} Card");
        if (
            GameState.UtilityStack == null
            || GameState.DamageStack == null
            || GameState.EnvironmentStack == null
            || GameState.HealingStack == null
        )
        {
            Console.WriteLine("[Server] Card stacks are not initialized, cannot buy card.");
            return;
        }
        Cards? card = cardType switch
        {
            "Utility" => RandomCard(GameState.UtilityStack),
            "Damage" => RandomCard(GameState.DamageStack),
            "Environment" => RandomCard(GameState.EnvironmentStack),
            "Healing" => RandomCard(GameState.HealingStack), // Healing hat eigenen Stack
            _ => throw new Exception(
                "Invalid CardType: "
                    + cardType
                    + " . Has to be a string of either: Utility, Damage, Environment or Healing"
            ),
        };
        if (card == null)
        {
            Console.WriteLine("[Server] No card found in the stack, cannot buy card.");
            return;
        }

        int playerIndex = gameState.Server.ConnectedPeerList.IndexOf(peer);
        int goldCost = card.Gold;
        InGameState? inGame = gameState.manager.CurrentState as InGameState;
        GoldHandler goldHandler = inGame!.goldHandler!;

        if (!goldHandler.CanSpendGold(playerIndex, goldCost))
        {
            Console.WriteLine(
                $"[Server] Player {playerIndex} has insufficient gold: {(playerIndex == 0 ? gameState.Player1Gold : gameState.Player2Gold)} ({goldCost} required). Purchase cancelled."
            );
            return;
        }

        bool success = goldHandler?.SpendGold(playerIndex, goldCost) ?? false;
        if (!success)
        {
            Console.WriteLine(
                $"[Server] Player {playerIndex} could not spend gold despite CanSpendGold check. Purchase cancelled."
            );
            return;
        }

        Console.WriteLine(
            $"[Server] Player {playerIndex} bought {cardType} card ({card.Variant}) for {goldCost} gold."
        );

        NetDataWriter writer = new();
        writer.Put("BoughtCard");
        writer.Put(card.Variant.ToString());
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
        if (server == null)
        {
            Console.WriteLine("[Server] GameState.server is null, cannot send card to opponent.");
            return;
        }
        if (server.ConnectedPeerList.Count == 2)
        {
            writer = new();
            writer.Put("OpponentBoughtCard");
            writer.Put(card.Type.ToString());
            var opponent = server.ConnectedPeerList.Find(p => !p.Equals(peer));
            opponent?.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        Console.WriteLine($"[Server] Player_{peer.Port} Bought Card {card.Variant}");
    }

    private static Cards RandomCard(List<Cards> stack)
    {
        var index = (int)(stack.Count * Random.Shared.NextSingle());
        return stack[index];
    }

    /// <summary>
    /// Verarbeitet das Ausspielen einer Karte durch einen Client und führt die Kartenfähigkeit aus.
    /// </summary>
    /// <param name="peer">The <see cref="NetPeer"/> Client sending the Placement Request</param>
    /// <param name="reader"><see cref="NetPacketReader"/> with the Request Data</param>
    public void HandleCardCasting(
        NetManager server,
        NetPeer peer,
        NetPacketReader reader,
        GameState gameState,
        ParalizeHandler paralizeHandler,
        UtilityCardHandler utilityCardHandler
    )
    {
        string cardVariantString = reader.GetString();
        int cardX = reader.GetInt();
        int cardY = reader.GetInt();

        Console.WriteLine(
            $"[CardHandler] Kartenausspielung empfangen von {peer.ToString()} (Port: {peer.Port})"
        );
        Console.WriteLine($"[CardHandler] Kartenvariante: {cardVariantString}");
        Console.WriteLine($"[CardHandler] Zielkoordinaten: ({cardX}, {cardY})");

        if (Enum.TryParse<CardVariant>(cardVariantString, out var variant))
        {
            var defender = server.ConnectedPeerList.Find(p => !p.Equals(peer));
            if (gameState == null)
            {
                Console.WriteLine("[Server] GameState is null, cannot handle CardCast.");
                return;
            }

            if (defender != null)
            {
                Console.WriteLine(
                    $"[CardHandler] Gegner gefunden: {defender.ToString()} (Port: {defender.Port})"
                );
                Console.WriteLine($"[CardHandler] Starte Kartenausführung für {variant}...");
                // Mana-cost-handling
                int playerIndex = gameState.Server.ConnectedPeerList.IndexOf(peer);
                int manaCost = new Cards(variant).Mana;
                InGameState? inGame = gameState.manager.CurrentState as InGameState;
                ManaHandler manaHandler = inGame!.manaHandler!;

                if (!manaHandler.CanSpendMana(playerIndex, manaCost))
                {
                    Console.WriteLine(
                        $"[Server] Player {playerIndex} has insufficient mana: "
                            + $"{(playerIndex == 0 ? gameState.Player1Mana.CurrentMana : gameState.Player2Mana.CurrentMana)} "
                            + $"({manaCost} required). Cast cancelled."
                    );
                    return;
                }

                bool success = manaHandler.SpendMana(playerIndex, manaCost);
                if (success)
                {
                    SendCardManaSpent(peer, variant);
                }
                else
                {
                    Console.WriteLine(
                        $"[Server] Player {playerIndex} could not spend mana despite CanSpendMana check. Cast cancelled."
                    );
                    return;
                }

                Console.WriteLine(
                    $"[Server] Player {playerIndex} casted card {variant} for {manaCost} mana."
                );
                // end of Mana-cost-handling

                if (gameState.PlayerHands == null)
                {
                    Console.WriteLine(
                        "[CardHandler] WARNING: PlayerHands is null - initializing empty hands"
                    );
                    gameState.PlayerHands = [];
                }

                if (gameState.PlayerHands.TryGetValue(peer, out var hand))
                {
                    Console.WriteLine(
                        $"[CardHandler] Player {peer} has {hand.Count} cards in hand"
                    );
                    foreach (var card in hand)
                    {
                        Console.WriteLine($"[CardHandler] - {card.Variant}");
                    }
                }
                else
                {
                    Console.WriteLine(
                        $"[CardHandler] Player {peer} has no hand tracked on server - this might be normal for testing"
                    );
                }

                CardAbilities.HandleAbility(variant, gameState, new Vector2(cardX, cardY), peer, defender);
                
                var cardToRemove = new Cards(variant);
                bool cardRemoved = gameState.RemoveCardFromPlayerHand(peer, cardToRemove);

                if (cardRemoved)
                {
                    Console.WriteLine(
                        $"[CardHandler] Successfully removed {variant} from player hand"
                    );
                }
                else
                {
                    Console.WriteLine(
                        $"[CardHandler] Could not remove {variant} from player hand (hand might not be tracked)"
                    );
                }

                NotifyOpponentCardUsed(defender, variant);
                Console.WriteLine($"[CardHandler] Kartenausführung für {variant} abgeschlossen");
            }
            else
            {
                Console.WriteLine("[Server] Kein Gegner gefunden für CardCast.");
            }
        }
        else
        {
            Console.WriteLine($"[Server] Casting Failed. Variant {cardVariantString} unknown");
        }
    }

    private void SendCardManaSpent(NetPeer peer, CardVariant variant)
    {
        NetDataWriter writer = new();
        writer.Put("CardManaSpent");
        writer.Put(variant.ToString());
        peer.Send(writer, DeliveryMethod.ReliableOrdered);
    }

    /// <summary>
    /// Notifies the opponent that a card was used (for visual hand updates)
    /// </summary>
    /// <param name="opponent">The opponent to notify</param>
    /// <param name="usedCard">The card that was used</param>
    private static void NotifyOpponentCardUsed(NetPeer opponent, CardVariant usedCard)
    {
        var writer = new NetDataWriter();
        writer.Put("OpponentUsedCard");
        writer.Put(usedCard.ToString());
        opponent.Send(writer, DeliveryMethod.ReliableOrdered);
        Console.WriteLine($"[Server] Notified opponent about card usage: {usedCard}");
    }

    internal static void CardActivation(GameState gameState, CardVariant variant, int duration)
    {
        Console.WriteLine($"[CardHandler.CardActivation] Called with variant: {variant}, duration: {duration}");

        if (gameState == null)
        {
            Console.WriteLine("[Server] GameState is null, cannot activate card.");
            return;
        }

        if (GameState.ActiveCards == null)
        {
            Console.WriteLine("[Server] GameState.ActiveCards is null, initializing...");
            GameState.ActiveCards = new List<Cards>();
        }

        int displayDuration = duration == 0 ? 1500 : Math.Max(duration * 1000, 1500);

        Console.WriteLine($"[Server] Activate Card {variant} for {duration} seconds (display duration: {displayDuration}ms)");

        var newCard = new Cards(variant) { remainingDuration = displayDuration };
        GameState.ActiveCards.Add(newCard);

        Console.WriteLine($"[Server] Active cards count after adding: {GameState.ActiveCards.Count}");
        foreach (var card in GameState.ActiveCards)
        {
            Console.WriteLine($"[Server] - Active card: {card.Variant} with {card.remainingDuration}ms remaining");
        }

        if (gameState.players != null)
        {
            Console.WriteLine($"[Server] Sending active cards update to {gameState.players.Length} players");
            foreach (var player in gameState.players)
            {
                Console.WriteLine($"[Server] Sending to player: {player}");
                SendActiveCardsUpdate(player);
            }
        }
        else
        {
            Console.WriteLine("[Server] No players found to send active cards update");
        }
    }

    public static void UpdateActiveCards(GameState gameState, float passedTime)
    {
        if (GameState.ActiveCards == null || GameState.ActiveCards.Count == 0)
        {
            return;
        }
        if (gameState == null)
        {
            Console.WriteLine("[Server] GameState is null, cannot update active cards.");
            return;
        }
        for (int i = GameState.ActiveCards.Count - 1; i >= 0; i--)
        {
            var card = GameState.ActiveCards[i];
            card.remainingDuration -= passedTime;

            if (card.remainingDuration <= 0) // Ist Karte abgelaufen
            {
                Console.WriteLine(
                    $"[Server] Card {card.Variant} expired, removing from active cards"
                );
                GameState.ActiveCards.RemoveAt(i);

                foreach (var player in gameState.players)
                {
                    SendActiveCardsUpdate(player);
                }

                if (card.Variant == CardVariant.Thunder)
                {
                    ThunderCard.ThunderEffectExpired(gameState);
                }
                continue;
            }

            CardAbilities.HandleActivationEffect(gameState, card, passedTime);
        }
    }

    private static void SendActiveCardsUpdate(NetPeer player)
    {
        var writer = new NetDataWriter();
        writer.Put("ActiveCards");
        if (GameState.ActiveCards == null)
        {
            Console.WriteLine("[Server] GameState.ActiveCards is null, cannot send active cards.");
            return;
        }
        writer.Put(GameState.ActiveCards.Count);
        foreach (var card in GameState.ActiveCards)
        {
            writer.Put(card.Variant.ToString());
            writer.Put(card.remainingDuration);
        }
        player.Send(writer, DeliveryMethod.ReliableOrdered);
    }
}
