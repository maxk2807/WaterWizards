using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.healing;

/// <summary>
/// Heilungs-Karte „Mass Mending“: Heilt gleichzeitig mehrere eigene Schiffe.
/// Jedes beschädigte Schiff erhält standardmäßig 1 Zelle Heilung.
/// </summary>
public class MassMendingCard : IHealingCard
{
    public CardVariant Variant => CardVariant.MassMending;

    public Vector2 AreaOfEffect => new(); // targets all own ships

    public int BaseHealing => 1;

    public bool HasSpecialTargeting => false;

    /// <summary>
    /// Heilt auf allen eigenen Schiffen jeweils genau eine beschädigte Zelle
    /// und sendet entsprechende Heal-Updates an den Caster.
    /// </summary>
    /// <param name="gameState">Aktueller Spielzustand (Server-seitig).</param>
    /// <param name="targetCoords">Ignoriert (AoE auf alle eigenen Schiffe).</param>
    /// <param name="caster">Wirker der Karte (Peer des Spielers).</param>
    /// <param name="opponent">Gegnerischer Peer.</param>
    /// <returns><c>true</c>, wenn die Heilung ausgeführt wurde.</returns>
    public bool ExecuteHealing(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        var ships = ShipHandler.GetShips(caster);
        foreach (var ship in ships)
        {
            if (ship.DamagedCells.Count > 0)
            {
                var (x, y) = ship.DamagedCells.First();
                ship.HealCell(x, y);
                ShipHandler.SendHealing(new(x, y), true, caster);
            }
        }
        return true;
    }

    /// <summary>
    /// Prüft die Zielgültigkeit für Mass Mending. Immer gültig,
    /// da die Karte alle eigenen Schiffe betrifft.
    /// </summary>
    /// <returns>Immer <c>true</c>.</returns>
    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
        => true;
}