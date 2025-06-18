using LiteNetLib;
using LiteNetLib.Utils;
using WaterWizard.Server;
using WaterWizard.Server.handler;

namespace WaterWizard.Server.ServerGameStates;

/// <summary>
/// Server-Spielzustand für die eigentliche Spielphase (nach Platzierung).
/// </summary>
public class InGameState(NetManager server, GameState gameState) : IServerGameState
{
    private readonly NetManager server = server;
    private readonly GameState gameState = gameState;

    private System.Timers.Timer? manaTimer;
    private ManaHandler? manaHandler;
    public ParalizeHandler? paralizeHandler;
    private UtilityCardHandler? utilityCardHandler;

    /// <summary>
    /// Wird beim Eintritt in die Spielphase aufgerufen.
    /// </summary>
    public void OnEnter()
    {
        var writer = new NetDataWriter();
        writer.Put("StartInGamePhase");
        foreach (var peer in server.ConnectedPeerList)
        {
            peer.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        // Initialisiere Handler
        paralizeHandler = new ParalizeHandler(gameState);
        manaHandler = new ManaHandler(gameState, paralizeHandler);
        utilityCardHandler = new UtilityCardHandler(gameState, paralizeHandler);

        // Mana-Timer starten
        // Mana alle 4 Sekunden
        manaTimer = new System.Timers.Timer(4_000);
        manaTimer.Elapsed += (sender, e) => UpdateMana();
        manaTimer.AutoReset = true;
        manaTimer.Start();
    }

    private void UpdateMana()
    {
        manaHandler?.UpdateMana();
    }

    /// <summary>
    /// Wird beim Verlassen des States aufgerufen (hier leer).
    /// </summary>
    public void OnExit()
    {
        manaTimer?.Stop();
        manaTimer?.Dispose();
    }

    public void HandleNetworkEvent(
        NetPeer peer,
        NetPacketReader reader,
        NetManager serverInstance,
        ServerGameStateManager manager,
        string messageType
    )
    {
        Console.WriteLine(
            $"[InGameState] HandleNetworkEvent called for peer {peer} with messageType {messageType}. Reader position: {reader.Position}"
        );
        CardHandler cardHandler = new(gameState);
        switch (messageType)
        {
            case "PlaceShip":
                ShipHandler.HandleShipPlacement(peer, reader, gameState);
                break;
            case "BuyCard":
                CardHandler.HandleCardBuying(serverInstance, peer, reader);
                break;
            case "CastCard":
                cardHandler.HandleCardCasting(serverInstance, peer, reader, gameState, paralizeHandler!, utilityCardHandler!);
                break;
            case "Attack":
                int x = reader.GetInt();
                int y = reader.GetInt();
                Console.WriteLine($"[Server] Attack received at ({x}, {y}) from {peer}");
                var defender = FindOpponent(peer);
                if (defender != null)
                {
                    AttackHandler.Initialize(gameState);
                    AttackHandler.HandleAttack(peer, defender, x, y);
                }
                else
                    Console.WriteLine("[Server] Kein Gegner gefunden für Attack.");
                break;
            default:
                Console.WriteLine($"[InGameState] Unbekannter Nachrichtentyp: {messageType}");
                break;
        }
    }

    private NetPeer? FindOpponent(NetPeer attacker)
    {
        foreach (var peer in server.ConnectedPeerList)
        {
            if (!peer.Equals(attacker))
                return peer;
        }
        return null;
    }
}
