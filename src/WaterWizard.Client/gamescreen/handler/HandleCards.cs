using LiteNetLib;
using LiteNetLib.Utils;
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
            Console.Write("[Client] Karte wirken");
        }
        else
        {
            Console.WriteLine(
                "[Client] Kein Server verbunden, PlaceShip konnte nicht gesendet werden."
            );
        }
    }
}
