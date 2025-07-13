// ===============================================
// Autoren-Statistik (automatisch generiert):
// - maxk2807: 139 Zeilen
// - erick: 66 Zeilen
// - Erickk0: 17 Zeilen
// 
// Methoden/Funktionen in dieser Datei (Hauptautor):
// - public static NetworkManager Instance => instance ??= new NetworkManager();   (maxk2807: 131 Zeilen)
// ===============================================

using LiteNetLib;
using Raylib_cs;
using WaterWizard.Client.Assets.Sounds.Manager;
using WaterWizard.Client.gamescreen;
using WaterWizard.Client.gamescreen.handler;
using WaterWizard.Shared;

namespace WaterWizard.Client.network;

public class NetworkManager
{
    private static NetworkManager? instance;
    public static NetworkManager Instance => instance ??= new NetworkManager();
    public HostService hostService { get; private set; }
    public ClientService clientService { get; private set; }
    public ServerConnection ServerConnection { get; private set; }
    public readonly List<LobbyInfo> discoveredLobbies = [];
    public readonly int hostPort = 7777;
    public int? LobbyCountdownSeconds { get; set; }

    public NetworkManager()
    {
        hostService = new(this);
        clientService = new(this);
        ServerConnection = new ServerConnection();
    }

    /// <summary>
    /// Starts discovering available server lobbies on the network.
    /// </summary>
    public void DiscoverLobbies()
    {
        if (hostService.IsRunning())
        {
            Console.WriteLine(
                "[Client] Suche nach entfernten Lobbies (lokale Lobby wird ausgeblendet)..."
            );
        }
        else
        {
            discoveredLobbies.Clear();
            Console.WriteLine("[Client] Suche nach verfügbaren Lobbies...");
        }

        clientService.InitializeClientForDiscovery();
        LobbyHandler.SendDiscoveryRequests(Instance);
    }

    /// <summary>
    /// Returns the list of discovered lobbies.
    /// </summary>
    public List<LobbyInfo> GetDiscoveredLobbies() => discoveredLobbies;

    public int GetHostPort() => hostPort;

    public bool IsPlayerConnected() =>
        hostService.ArePlayersConnected() || clientService.IsServerConnected();

    /// <summary>
    /// Verarbeitet den Countdown für die Lobby.
    /// </summary>
    /// <param name="reader">Der NetPacketReader, der die Nachricht enthält.</param>
    /// <returns></returns>
    public void HandleLobbyCountdown(NetPacketReader reader)
    {
        int secondsLeft = reader.GetInt();
        if (secondsLeft <= 0)
        {
            LobbyCountdownSeconds = null;
        }
        else
        {
            LobbyCountdownSeconds = secondsLeft;
        }
        Console.WriteLine($"[Host] Lobby countdown: {LobbyCountdownSeconds}");
    }

    /// <summary>
    /// Verarbeitet eingehende und ausgehende Netzwerkereignisse.
    /// Muss aufgerufen werden, um Nachrichten zu empfangen und zu senden.
    /// </summary>
    public void PollEvents()
    {
        hostService.PollEvents();
        clientService.PollEvents();
    }

    public void Shutdown()
    {
        hostService.Shutdown();
        clientService.Shutdown();
        discoveredLobbies.Clear();
        LobbyCountdownSeconds = null;
    }

    public void StartHosting()
    {
        hostService.StartHosting();
    }

    public void SendShipPlacement(int x, int y, int width, int height)
    {
        HandleShips.SendShipPlacement(x, y, width, height, Instance);
    }

    public static void RequestCardBuy(string cardType)
    {
        var handleCards = new HandleCards();
        handleCards.RequestCardBuy(cardType);
    }

    public static void HandleCast(Cards card, GameBoard.Point hoveredCoords)
    {
        var handleCards = new HandleCards();
        handleCards.HandleCast(card, hoveredCoords);
    }

    public static void SendAttack(int x, int y)
    {
        var handleAttacks = new HandleAttacks();
        handleAttacks.SendAttack(x, y);
    }

    public List<Player> GetConnectedPlayers()
    {
        if (hostService.IsRunning())
            return hostService.ConnectedPlayers;
        return clientService.ConnectedPlayers;
    }

    public bool IsHost()
    {
        return hostService.IsRunning();
    }

    internal void BroadcastStartGame()
    {
        hostService.BroadcastStartGame();
    }

    public bool IsClientReady()
    {
        return clientService.IsClientReady();
    }

    internal void ToggleReadyStatus()
    {
        HandlePlayer.ToggleReadyStatus();
    }

    internal void ConnectToServer(string ip, int port)
    {
        ServerConnection.ConnectToServer(ip, port);
    }

    internal void SendChatMessage(string message)
    {
        ChatHandler.SendChatMessage(message, Instance);
    }

    /// <summary>
    /// Handles the game over message received from the server.
    /// </summary>
    /// <param name="reader">A NetPacketReader containing the serialized game over data from the server. Contains a string indicating the game result</param>
    public static void HandleGameOverMessage(NetPacketReader reader)
    {
        string result = reader.GetString();
        bool isWinner = DetermineIfPlayerIsWinner(result);

        string winnerMessage = isWinner
            ? "Congratulations! You emerged victorious!"
            : "You fought well, but victory slipped away. Better luck next time!";

        Console.WriteLine($"[Client] Game Over - Result: {result}, IsWinner: {isWinner}");

        Instance.clientService.clientReady = false;
        Instance.LobbyCountdownSeconds = null;

        GameStateManager.Instance.SetStateToGameOver(isWinner, winnerMessage);
        Raylib.PlaySound(isWinner ? SoundManager.WinSound : SoundManager.WinSound);
    }

    /// <summary>
    /// Determines if the player is the winner based on the game result string.
    /// </summary>
    /// <param name="result">The game result string received from the server containing winner information or game outcome details.</param>
    /// <returns>True if the current player won the game, false otherwise.</returns>
    private static bool DetermineIfPlayerIsWinner(string result)
    {
        if (
            result.Equals("Victory", StringComparison.OrdinalIgnoreCase)
            || result.Contains("win", StringComparison.OrdinalIgnoreCase)
            || result.Contains("victory", StringComparison.OrdinalIgnoreCase)
            || result.Contains("won", StringComparison.OrdinalIgnoreCase)
        )
        {
            return true;
        }

        if (
            result.Equals("Defeat", StringComparison.OrdinalIgnoreCase)
            || result.Contains("lose", StringComparison.OrdinalIgnoreCase)
            || result.Contains("lost", StringComparison.OrdinalIgnoreCase)
            || result.Contains("defeat", StringComparison.OrdinalIgnoreCase)
        )
        {
            return false;
        }

        Console.WriteLine($"[Client] Unclear game result: {result}. Assuming defeat.");
        return false;
    }

    /// <summary>
    /// Handles the casting of the teleport card with ship selection and destination.
    /// </summary>
    /// <param name="card">The teleport card to be cast.</param>
    /// <param name="shipIndex">The index of the ship to teleport.</param>
    /// <param name="destinationCoords">The destination coordinates.</param>
    public void HandleTeleportCast(Cards card, int shipIndex, GameBoard.Point destinationCoords)
    {
        var handleCards = new HandleCards();
        handleCards.HandleTeleportCast(card, shipIndex, destinationCoords);
    }
}
