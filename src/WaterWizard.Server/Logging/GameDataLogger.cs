// Hauptklasse im Server-Namespace
using System;
using System.Collections.Generic;
using System.Text.Json;
using WaterWizard.Server.Models;

namespace WaterWizard.Server.Logging
{
    /// <summary>
    /// Verantwortlich für das Speichern und Verwalten von Spielzuständen im Speicher.
    /// 
    /// Speichert alle Daten für jede Session und deren Spieler in einer strukturieren Form,
    /// inklusive Historie der Zustände (Mana, Gold, Karten, Schiffe).
    /// </summary>
    public class GameDataLogger
    {
        private readonly Dictionary<string, GameSession> _sessions = new();

        /// <summary>
        /// Speichert oder aktualisiert Daten eines Spielers in einer Session.
        /// Jede Aufzeichnung erzeugt einen neuen Eintrag in der History.
        /// </summary>
        public void LogPlayerData(
            string sessionId,
            string playerName,
            int mana,
            int gold,
            List<string> cards,
            List<string> shipsInStock)
        {
            if (!_sessions.ContainsKey(sessionId))
            {
                _sessions[sessionId] = new GameSession
                {
                    SessionId = sessionId,
                    Players = new Dictionary<string, PlayerHistory>()
                };
            }

            var session = _sessions[sessionId];

            if (!session.Players.ContainsKey(playerName))
            {
                session.Players[playerName] = new PlayerHistory
                {
                    PlayerName = playerName,
                    Entries = new List<PlayerData>()
                };
            }

            session.Players[playerName].Entries.Add(new PlayerData
            {
                Timestamp = DateTime.UtcNow,
                Mana = mana,
                Gold = gold,
                Cards = new List<string>(cards),
                ShipsInStock = new List<string>(shipsInStock)
            });
        }

        /// <summary>
        /// Gibt alle gespeicherten Daten als JSON-String zurück.
        /// </summary>
        public string GetJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(_sessions, options);
        }

        /// <summary>
        /// Gibt den internen Zustand der Sessions zur direkten Weiterverarbeitung zurück.
        /// </summary>
        public Dictionary<string, GameSession> GetSessions() => _sessions;

        /// <summary>
        /// Entfernt alle Daten für eine bestimmte Session.
        /// </summary>
        public void ResetSession(string sessionId)
        {
            _sessions.Remove(sessionId);
        }

        /// <summary>
        /// Entfernt alle gespeicherte Spielzustände.
        /// </summary>
        public void ResetAll()
        {
            _sessions.Clear();
        }
    }
}
