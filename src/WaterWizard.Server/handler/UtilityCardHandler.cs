// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 107 Zeilen
// - erick: 5 Zeilen
// - maxk2807: 1 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// (Keine Methoden/Funktionen gefunden)
// ===============================================

using System.Numerics;
using LiteNetLib;
using WaterWizard.Server;
using WaterWizard.Server.Card.utility;
using WaterWizard.Shared;

namespace WaterWizard.Server.handler;

/// <summary>
/// Verwaltet Utility-Karten und ihre Effekte.
/// </summary>
public class UtilityCardHandler
{
    private readonly GameState gameState;
    private readonly ParalizeHandler paralizeHandler;

    public UtilityCardHandler(GameState gameState, ParalizeHandler paralizeHandler)
    {
        this.gameState = gameState;
        this.paralizeHandler = paralizeHandler;
    }

    /// <summary>
    /// Behandelt die Ausführung einer Utility-Karte
    /// </summary>
    /// <param name="variant">Die Kartenvariante</param>
    /// <param name="targetCoords">Die Zielkoordinaten</param>
    /// <param name="caster">Der Spieler, der die Karte wirkt</param>
    /// <param name="defender">Der Spieler, der das Ziel ist</param>
    public void HandleUtilityCard(
        CardVariant variant,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer defender
    )
    {
        switch (variant)
        {
            case CardVariant.Paralize:
                Console.WriteLine($"[UtilityCardHandler] Paralize-Karte aktiviert!");
                Console.WriteLine(
                    $"[UtilityCardHandler] Caster (Angreifer): {caster.ToString()} (Port: {caster.Port})"
                );
                Console.WriteLine(
                    $"[UtilityCardHandler] Defender (Ziel): {defender.ToString()} (Port: {defender.Port})"
                );
                Console.WriteLine(
                    $"[UtilityCardHandler] Zielkoordinaten: ({targetCoords.X}, {targetCoords.Y})"
                );
                // paralizeHandler.HandleParalizeCard(caster, defender); //TODO:not needed if factory goes right
                break;
            case CardVariant.HoveringEye:
                Console.WriteLine($"[UtilityCardHandler] HoveringEye-Karte aktiviert!");
                HandleHoveringEye(targetCoords, caster, defender);
                break;
            case CardVariant.Teleport:
                Console.WriteLine($"[UtilityCardHandler] Teleport-Karte aktiviert!");
                HandleTeleport(targetCoords, caster, defender);
                break;
            case CardVariant.ConeOfCold:
                Console.WriteLine($"[UtilityCardHandler] ConeOfCold-Karte aktiviert!");
                HandleConeOfCold(targetCoords, caster, defender);
                break;
            case CardVariant.SummonShip:
                Console.WriteLine($"[UtilityCardHandler] SummonShip-Karte aktiviert!");
                HandleSummonShip(targetCoords, caster, defender);
                break;
            default:
                Console.WriteLine($"[UtilityCardHandler] Unbekannte Utility-Karte: {variant}");
                break;
        }
    }

    /// <summary>
    /// Behandelt die HoveringEye-Karte (permanente Überwachung)
    /// </summary>
    private void HandleHoveringEye(Vector2 targetCoords, NetPeer caster, NetPeer defender)
    {
        // TODO: Implementiere permanente Überwachung der Zielzelle
        Console.WriteLine(
            $"[UtilityCardHandler] HoveringEye placed at ({targetCoords.X}, {targetCoords.Y})"
        );
    }

    /// <summary>
    /// Behandelt die Teleport-Karte (Schiff teleportieren)
    /// </summary>
    private void HandleTeleport(Vector2 targetCoords, NetPeer caster, NetPeer defender)
    {
        int shipId = (int)(targetCoords.X) >> 16;
        int destinationX = (int)targetCoords.X & 0xFFFF;
        int destinationY = (int)targetCoords.Y;

        Console.WriteLine($"[UtilityCardHandler] Teleport ship {shipId} to ({destinationX}, {destinationY})");
    }

    /// <summary>
    /// Behandelt die ConeOfCold-Karte (Verlangsamung)
    /// </summary>
    private void HandleConeOfCold(Vector2 targetCoords, NetPeer caster, NetPeer defender)
    {
        // TODO: Implementiere Verlangsamungseffekt
        Console.WriteLine(
            $"[UtilityCardHandler] ConeOfCold at ({targetCoords.X}, {targetCoords.Y})"
        );
    }

    /// <summary>
    /// Behandelt die MinorIllusion-Karte (Täuschung)
    /// </summary>
    private void HandleMinorIllusion(Vector2 targetCoords, NetPeer caster, NetPeer defender)
    {
        // TODO: Implementiere Illusionseffekt
        Console.WriteLine(
            $"[UtilityCardHandler] MinorIllusion at ({targetCoords.X}, {targetCoords.Y})"
        );
    }

    /// <summary>
    /// Behandelt die Polymorph-Karte (Verwandlung)
    /// </summary>
    private void HandlePolymorph(Vector2 targetCoords, NetPeer caster, NetPeer defender)
    {
        // TODO: Implementiere Verwandlungseffekt
        Console.WriteLine(
            $"[UtilityCardHandler] Polymorph at ({targetCoords.X}, {targetCoords.Y})"
        );
    }

    /// <summary>
    /// Handles the SummonShip card (summons a ship at the target coordinates)
    /// </summary>
    /// <param name="targetCoords">Target Coordinates where the ship will be summoned</param>
    /// <param name="caster">The Player that uses the card</param>
    /// <param name="defender">The Player that is affected by the card</param>
    private void HandleSummonShip(Vector2 targetCoords, NetPeer caster, NetPeer defender)
    {
        var summonShipCard = new SummonShipCard();
        summonShipCard.ExecuteUtility(gameState, targetCoords, caster, defender);
        Console.WriteLine($"[UtilityCardHandler] SummonShip executed at ({targetCoords.X}, {targetCoords.Y})");
    }
}
