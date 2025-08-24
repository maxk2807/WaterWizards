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

/// <summary>
/// Verwaltet die Netzwerkkommunikation, Lobby-Discovery und Verbindungen für den Client.
/// </summary>
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
    /// Startet die Suche nach verfügbaren Lobbies im Netzwerk.
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
    /// Gibt die Liste der gefundenen Lobbies zurück.
    /// </summary>
    public List<LobbyInfo> GetDiscoveredLobbies() => discoveredLobbies;

    /// <summary>
    /// Gibt den Host-Port zurück.
    /// </summary>
    public int GetHostPort() => hostPort;

    /// <summary>
    /// Prüft, ob ein Spieler verbunden ist.
    /// </summary>
    public bool IsPlayerConnected() =>
        hostService.ArePlayersConnected() || clientService.IsServerConnected();

    /// <summary>
    /// Verarbeitet den Lobby-Countdown.
    /// </summary>
    /// <param name="reader">Netzwerkleser mit Countdown-Daten</param>
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
    /// Verarbeitet Netzwerkereignisse (Empfang/Senden von Nachrichten).
    /// </summary>
    public void PollEvents()
    {
        hostService.PollEvents();
        clientService.PollEvents();
    }

    /// <summary>
    /// Beendet alle Netzwerkverbindungen (Host und Client) und setzt den Zustand zurück.
    /// </summary>
    public void Shutdown()
    {
        hostService.Shutdown();
        clientService.Shutdown();
        discoveredLobbies.Clear();
        LobbyCountdownSeconds = null;
    }

    /// <summary>
    /// Startet das Hosting einer neuen Spiel-Lobby.
    /// </summary>
    public void StartHosting()
    {
        hostService.StartHosting();
    }

    /// <summary>
    /// Sendet die Platzierung eines Schiffs an den Server.
    /// </summary>
    /// <param name="x">X-Koordinate des Schiffs.</param>
    /// <param name="y">Y-Koordinate des Schiffs.</param>
    /// <param name="width">Breite des Schiffs in Zellen.</param>
    /// <param name="height">Höhe des Schiffs in Zellen.</param>
    public void SendShipPlacement(int x, int y, int width, int height)
    {
        HandleShips.SendShipPlacement(x, y, width, height, Instance);
    }

    /// <summary>
    /// Sendet eine Anfrage an den Server, eine Karte zu kaufen.
    /// </summary>
    /// <param name="cardType">Der Typ der zu kaufenden Karte.</param>
    public static void RequestCardBuy(string cardType)
    {
        var handleCards = new HandleCards();
        handleCards.RequestCardBuy(cardType);
    }

    /// <summary>
    /// Führt das Ausspielen einer Karte mit einem Ziel aus und sendet die Aktion an den Server.
    /// </summary>
    /// <param name="card">Die zu spielende Karte.</param>
    /// <param name="hoveredCoords">Die angezielten Koordinaten.</param>
    public static void HandleCast(Cards card, GameBoard.Point hoveredCoords)
    {
        var handleCards = new HandleCards();
        handleCards.HandleCast(card, hoveredCoords);
    }

    /// <summary>
    /// Sendet einen Angriff auf eine bestimmte Zelle an den Server.
    /// </summary>
    /// <param name="x">X-Koordinate der Zelle.</param>
    /// <param name="y">Y-Koordinate der Zelle.</param>
    public static void SendAttack(int x, int y)
    {
        var handleAttacks = new HandleAttacks();
        handleAttacks.SendAttack(x, y);
    }

    /// <summary>
    /// Gibt die Liste aller aktuell verbundenen Spieler zurück.
    /// </summary>
    /// <returns>Liste der Spieler.</returns>
    public List<Player> GetConnectedPlayers()
    {
        if (hostService.IsRunning())
            return hostService.ConnectedPlayers;
        return clientService.ConnectedPlayers;
    }

    /// <summary>
    /// Prüft, ob dieser Client als Host fungiert.
    /// </summary>
    /// <returns>True, wenn als Host aktiv.</returns>
    public bool IsHost()
    {
        return hostService.IsRunning();
    }

    /// <summary>
    /// Startet das Spiel für alle Spieler (nur Host).
    /// </summary>
    internal void BroadcastStartGame()
    {
        hostService.BroadcastStartGame();
    }

    /// <summary>
    /// Prüft, ob der Client bereit ist.
    /// </summary>
    /// <returns>True, wenn der Client bereit ist.</returns>
    public bool IsClientReady()
    {
        return clientService.IsClientReady();
    }

    /// <summary>
    /// Wechselt den Ready-Status des Spielers (bereit/nicht bereit).
    /// </summary>
    internal void ToggleReadyStatus()
    {
        HandlePlayer.ToggleReadyStatus();
    }

    /// <summary>
    /// Baut eine Verbindung zu einem Server auf.
    /// </summary>
    /// <param name="ip">IP-Adresse des Servers.</param>
    /// <param name="port">Port des Servers.</param>
    internal void ConnectToServer(string ip, int port)
    {
        ServerConnection.ConnectToServer(ip, port);
    }

    /// <summary>
    /// Sendet eine Chatnachricht an den Server.
    /// </summary>
    /// <param name="message">Der Nachrichteninhalt.</param>
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
        Raylib.PlaySound(isWinner ? SoundManager.WinSound : SoundManager.DefeatSound);
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
