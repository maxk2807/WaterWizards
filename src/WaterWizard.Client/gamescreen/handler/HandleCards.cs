// ===============================================
// Autoren-Statistik (automatisch generiert):
// - erick: 109 Zeilen
// - Erickk0: 34 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using LiteNetLib;
using LiteNetLib.Utils;
using Raylib_cs;
using WaterWizard.Client.Assets.Sounds.Manager;
using WaterWizard.Client.network;
using WaterWizard.Shared;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles active card-related messages received from the server.
/// </summary>
public class HandleCards
{
    readonly ClientService clientService = NetworkManager.Instance.clientService;

    /// <summary>
    /// Handles the active cards message received from the server.
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the serialized ship data sent from the server</param>
    public static void HandleActiveCards(NetPacketReader reader)
    {
        var activeCardsNum = reader.GetInt();
        List<Cards> activeCards = [];
        for (int i = 0; i < activeCardsNum; i++)
        {
            var variant = reader.GetString();
            var remainingDuration = reader.GetFloat();
            Cards card = new(Enum.Parse<CardVariant>(variant))
            {
                remainingDuration = remainingDuration,
            };
            activeCards.Add(card);
            Console.WriteLine($"[Client] ActivateCard received: {variant}");
        }

        GameStateManager.Instance.GameScreen.activeCards!.UpdateActiveCards(activeCards);
    }

    /// <summary>
    /// Requests the server to buy a card of the specified type.
    /// </summary>
    /// <param name="cardType">The type of card to buy, specified as a string.</param>
    public void RequestCardBuy(string cardType)
    {
        if (clientService.client != null && clientService.client.FirstPeer != null)
        {
            var writer = new NetDataWriter();
            writer.Put("BuyCard");
            writer.Put(cardType);
            clientService.client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine("[Client] Kaufe Karte");
        }
        else
        {
            Console.WriteLine(
                "[Client] Kein Server verbunden, PlaceShip konnte nicht gesendet werden."
            );
        }
    }

    public static void HandleCardManaSpent(NetPacketReader reader)
    {
        var variantString = reader.GetString();
        var variant = Enum.Parse<CardVariant>(variantString);
        var playerHand = GameStateManager.Instance.GameScreen.playerHand;
        playerHand!.RemoveCard(new Cards(variant));
        Raylib.PlaySound(SoundManager.GetCardSound(variant));
    }

    /// <summary>
    /// Handles the casting of a card at the specified coordinates.
    /// </summary>
    /// <param name="card">The card to be cast.</param>
    /// <param name="hoveredCoords">The coordinates where the card is to be cast.</param>
    public void HandleCast(Cards card, GameBoard.Point hoveredCoords)
    {
        if (clientService.client != null && clientService.client.FirstPeer != null)
        {
            NetDataWriter writer = new();
            writer.Put("CastCard");
            writer.Put(card.Variant.ToString());
            writer.Put(hoveredCoords.X);
            writer.Put(hoveredCoords.Y);
            clientService.client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Karte wirken: {card.Variant} an Position ({hoveredCoords.X}, {hoveredCoords.Y})");
        }
        else
        {
            Console.WriteLine(
                "[Client] Kein Server verbunden, PlaceShip konnte nicht gesendet werden."
            );
        }
    }

    /// <summary>
    /// Handles the casting of the teleport card with ship selection and destination.
    /// </summary>
    /// <param name="card">The teleport card to be cast.</param>
    /// <param name="shipIndex">The index of the ship to teleport.</param>
    /// <param name="destinationCoords">The destination coordinates.</param>
    public void HandleTeleportCast(Cards card, int shipId, GameBoard.Point destinationCoords)
    {
        if (clientService.client != null && clientService.client.FirstPeer != null)
        {
            NetDataWriter writer = new();
            writer.Put("CastCard");
            writer.Put(card.Variant.ToString());

            // Encode ship ID in the higher 16 bits of X coordinate
            int encodedX = (shipId << 16) | (destinationCoords.X & 0xFFFF);
            writer.Put(encodedX);
            writer.Put(destinationCoords.Y);

            clientService.client.FirstPeer.Send(writer, DeliveryMethod.ReliableOrdered);
            Console.WriteLine($"[Client] Teleport-Karte wirken: Schiff {shipId} zur Position ({destinationCoords.X}, {destinationCoords.Y})");
        }
        else
        {
            Console.WriteLine(
                "[Client] Kein Server verbunden, Teleport konnte nicht gesendet werden."
            );
        }
    }

    /// <summary>
    /// Handles notification that the opponent used a card
    /// </summary>
    /// <param name="reader">The NetPacketReader containing the card information</param>
    public static void HandleOpponentUsedCard(NetPacketReader reader)
    {
        Console.WriteLine("[Client] Received OpponentUsedCard message");
        var cardVariant = reader.GetString();
        Console.WriteLine($"[Client] Card variant: {cardVariant}");

        if (Enum.TryParse<CardVariant>(cardVariant, out var variant))
        {
            var opponentHand = GameStateManager.Instance.GameScreen.opponentHand;
            if (opponentHand != null && opponentHand.Cards.Count > 0)
            {
                opponentHand.Cards.RemoveAt(0);
                Console.WriteLine($"[Client] Opponent used card {variant}, removed from opponent hand display. Remaining cards: {opponentHand.Cards.Count}");
            }
            else
            {
                Console.WriteLine($"[Client] Could not remove card - opponent hand is null or empty");
            }
        }
        else
        {
            Console.WriteLine($"[Client] Failed to parse card variant: {cardVariant}");
        }
    }
}
