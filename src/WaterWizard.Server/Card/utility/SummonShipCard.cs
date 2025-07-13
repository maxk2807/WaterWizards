// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 31 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new(1, 1);   (justinjd00: 19 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.utility;

public class SummonShipCard : IUtilityCard
{
    public CardVariant Variant => CardVariant.SummonShip;

    public Vector2 AreaOfEffect => new(1, 1); // Ein Feld, aber eigentlich beliebig

    public bool HasSpecialTargeting => false;

    public bool ExecuteUtility(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        // Spielerindex ermitteln
        int playerIndex = gameState.GetPlayerIndex(caster);
        gameState.AllowShipPlacementInGame[playerIndex] = true;
        // Sende an den Client, dass er ein neues Schiff platzieren darf
        var writer = new LiteNetLib.Utils.NetDataWriter();
        writer.Put("SummonShip");
        caster.Send(writer, LiteNetLib.DeliveryMethod.ReliableOrdered);
        return true;
    }

    public bool IsValidTarget(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    ) => true;
}
