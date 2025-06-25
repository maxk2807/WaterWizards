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
    private System.Timers.Timer? goldTimer;
    private ManaHandler? manaHandler;
    public ParalizeHandler? paralizeHandler;
    private GoldHandler? goldHandler;
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
        goldHandler = new GoldHandler(gameState);
        utilityCardHandler = new UtilityCardHandler(gameState, paralizeHandler);

        manaTimer = new System.Timers.Timer(4000);
        manaTimer.Elapsed += (sender, e) => UpdateMana();
        manaTimer.AutoReset = true;
        manaTimer.Start();

        goldTimer = new System.Timers.Timer(2000);
        goldTimer.Elapsed += (sender, e) => UpdateGold();
        goldTimer.AutoReset = true;
        goldTimer.Start();
    }

    private void UpdateMana()
    {
        if (gameState.IsPaused) return;
        manaHandler?.UpdateMana();
    }

    private void UpdateGold()
    {
        if (gameState.IsPaused) return;
        goldHandler?.UpdateGold();
    }

    /// <summary>
    /// Wird beim Verlassen des States aufgerufen (hier leer).
    /// </summary>
    public void OnExit()
    {
        manaTimer?.Stop();
        manaTimer?.Dispose();
        
        goldTimer?.Stop();
        goldTimer?.Dispose();
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
            case "Surrender":
                HandleSurrender(peer);
                break;
            case "PauseToggle":
                HandlePauseToggle(serverInstance);
                break;
            default:
                Console.WriteLine($"[InGameState] Unbekannter Nachrichtentyp: {messageType}");
                break;
        }
    }

    private void HandlePauseToggle(NetManager serverInstance)
    {
        gameState.IsPaused = !gameState.IsPaused;
        Console.WriteLine($"[Server] Game pause state toggled to: {gameState.IsPaused}");

        var writer = new NetDataWriter();
        writer.Put("UpdatePauseState");
        writer.Put(gameState.IsPaused);
        
        foreach (var p in serverInstance.ConnectedPeerList)
        {
            p.Send(writer, DeliveryMethod.ReliableOrdered);
        }
        Console.WriteLine($"[Server] Sent pause state '{gameState.IsPaused}' to all clients.");
    }

    /// <summary>
    /// Handles the surrender button usage
    /// </summary>
    /// <param name="surrenderingPlayer">The player which surrenders</param>
    private void HandleSurrender(NetPeer surrenderingPlayer)
    {
        Console.WriteLine($"[Server] Player {surrenderingPlayer} has surrendered");

        var opponent = FindOpponent(surrenderingPlayer);
        if (opponent != null)
        {
            gameState.HandleSurrender(opponent, surrenderingPlayer);
        }
        else
        {
            Console.WriteLine("[Server] No opponent found for surrender handling");
        }
    }

    public NetPeer? FindOpponent(NetPeer attacker)
    {
        foreach (var peer in server.ConnectedPeerList)
        {
            if (!peer.Equals(attacker))
                return peer;
        }
        return null;
    }
}
