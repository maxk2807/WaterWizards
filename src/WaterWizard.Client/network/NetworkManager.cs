using LiteNetLib;
using WaterWizard.Client.gamescreen;
using WaterWizard.Shared;

namespace WaterWizard.Client.network;

public class NetworkManager
{
    private static NetworkManager? instance;
    public static NetworkManager Instance => instance ??= new NetworkManager();

    public HostService hostService { get; private set; }
    public ClientService clientService { get; private set; }

    public readonly List<LobbyInfo> discoveredLobbies = [];

    public readonly int hostPort = 7777;

    public int? LobbyCountdownSeconds { get; set; }

    private NetworkManager()
    {
        hostService = new(this);
        clientService = new(this);
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
        clientService.SendDiscoveryRequests();
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
        clientService.SendShipPlacement(x, y, width, height);
    }

    public void RequestCardBuy(string cardType)
    {
        clientService.RequestCardBuy(cardType);
    }

    public void HandleCast(Cards card, GameBoard.Point hoveredCoords)
    {
        clientService.HandleCast(card, hoveredCoords);
    }

    public void SendAttack(int x, int y)
    {
        clientService.SendAttack(x, y);
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

    internal bool IsClientReady()
    {
        return clientService.IsClientReady();
    }

    internal void ToggleReadyStatus()
    {
        clientService.ToggleReadyStatus();
    }

    internal void ConnectToServer(string ip, int port)
    {
        clientService.ConnectToServer(ip, port);
    }

    internal void RefreshLobbies()
    {
        clientService.RefreshLobbies();
    }

    internal void SendChatMessage(string message)
    {
        clientService.SendChatMessage(message);
    }

    internal void SendPlacementReady()
    {
        clientService.SendPlacementReady();
    }

    public void HandleGameOverMessage(NetPacketReader reader)
    {
        string result = reader.GetString();
        bool isWinner = DetermineIfPlayerIsWinner(result);
        string winnerMessage = isWinner ? "Congratulations! You won!" : "Better luck next time!";

        GameStateManager.Instance.SetStateToGameOver(isWinner, winnerMessage);
    }

    private bool DetermineIfPlayerIsWinner(string result)
    {
        // You'll need to implement this based on how your server sends the result
        // For now, this is a placeholder
        return result.Contains("win") || result.Contains("victory");
    }
}
