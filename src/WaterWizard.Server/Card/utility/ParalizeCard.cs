// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 36 Zeilen
// - Paul: 24 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public Vector2 AreaOfEffect => new();   (maxk2807: 33 Zeilen)
// ===============================================

using System.Numerics;
using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server.handler;
using WaterWizard.Server.Interface;
using WaterWizard.Server.ServerGameStates;
using WaterWizard.Shared;

namespace WaterWizard.Server.Card.utility;

public class ParalizeCard : IUtilityCard
{
    public CardVariant Variant => CardVariant.Paralize;

    public Vector2 AreaOfEffect => new(); // whole Battlefield

    public bool HasSpecialTargeting => true; // whole Battlefield


    public bool ExecuteUtility(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        var state = gameState.manager.CurrentState;
        if (state is InGameState ingame)
        {
            Console.WriteLine($"[ParalizeHandler] Paralize-Karte wird ausgeführt...");
            Console.WriteLine($"[ParalizeHandler] Caster (Angreifer): {caster.ToString()} (Port: {caster.Port})");
            Console.WriteLine($"[ParalizeHandler] Defender (Ziel): {opponent.ToString()} (Port: {opponent.Port})");

            // Finde den Spielerindex des Gegners
            int opponentIndex = -1;
            for (int i = 0; i < gameState.players.Length; i++)
            {
                if (gameState.players[i]?.Equals(opponent) == true)
                {
                    opponentIndex = i;
                    break;
                }
            }

            if (opponentIndex != -1)
            {
                // Aktiviere Paralize für 6 Sekunden
                ingame.paralizeHandler!.ActivateParalize(opponentIndex, 6.0f);
                Console.WriteLine($"[ParalizeHandler] Player {opponentIndex + 1} paralyzed for 6 seconds");
                Console.WriteLine($"[ParalizeHandler] Paralize-Effekt: Mana-Generierung für {opponentIndex + 1} Sekunden gestoppt");
            }
            else
            {
                Console.WriteLine("[ParalizeHandler] Could not find defender index for Paralize");
                return false;
            }
        }
        return true;
    }

    public bool IsValidTarget(GameState gameState, Vector2 targetCoords, NetPeer caster, NetPeer opponent)
    {
        return true;
    }
}