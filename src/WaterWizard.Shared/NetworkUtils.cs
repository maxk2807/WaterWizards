/// <summary>
/// Stellt Hilfsmethoden für Netzwerkoperationen bereit, z.B. zum Ermitteln der lokalen und öffentlichen IP-Adresse.
/// </summary>
public static class NetworkUtils
{
    /// <summary>
    /// Gibt die lokale IP-Adresse des aktuellen Rechners zurück.
    /// </summary>
    /// <returns>Lokale IPv4-Adresse als String</returns>
    public static string GetLocalIPAddress()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }
        catch
        {
            return "127.0.0.1";
        }
    }

    /// <summary>
    /// Gibt die öffentliche IP-Adresse des aktuellen Rechners zurück.
    /// </summary>
    /// <returns>Öffentliche IPv4-Adresse als String</returns>
    public static string GetPublicIPAddress()
    {
        try
        {
            using var client = new HttpClient();
            // Abrufen derffentlichen IP-Adresse von einem externen Dienst
            var response = client.GetStringAsync("https://api.ipify.org").Result;
            if (response == null)
            {
                return "funktioniert nicht";
            }

            return response;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Abrufen derffentlichen IP-Adresse: {ex.Message}");
            return "Unbekannt";
        }
    }
}
