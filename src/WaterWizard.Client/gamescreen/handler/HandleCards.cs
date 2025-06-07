using LiteNetLib;
using WaterWizard.Shared;

namespace WaterWizard.Client.gamescreen.handler;

/// <summary>
/// Handles active card-related messages received from the server.
/// </summary>
public class HandleCards
{
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

        GameStateManager.Instance.GameScreen.activeCards!.UpdateActiveCards(
            activeCards
        );
    }
}