// ===============================================
// Autoren-Statistik (automatisch generiert):
// - justinjd00: 32 Zeilen
//
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new();   (justinjd00: 20 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.environment;

public class SpawnRocksCard : IEnvironmentCard
{
    public CardVariant Variant => CardVariant.SpawnRocks;

    public Vector2 AreaOfEffect => new(); // gesamtes Spielfeld

    public bool HasSpecialTargeting => false;

    public bool ExecuteEnvironment(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer opponent
    )
    {
        // Für beide Spieler je einen Stein platzieren
        for (int playerIndex = 0; playerIndex < gameState.boards.Length; playerIndex++)
        {
            var board = gameState.boards[playerIndex];
            RockFactory.GenerateRocks(board, 1); // genau 1 Stein
            var rockPositions = RockHandler.GetRockPositions(board);
            RockHandler.SyncRocksToClient(gameState.players[playerIndex], rockPositions);
        }
        return true;
    }

    public bool IsValidTarget(
        GameState gameState,
        Vector2 targetCoords,
        NetPeer caster,
        NetPeer defender
    ) => true;
}
