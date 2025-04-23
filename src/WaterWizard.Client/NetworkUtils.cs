using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace WaterWizard.Client
{
    public static class NetworkUtils
    {
        public static string GetLocalIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch
            {
                // Ignorieren
            }
            return "127.0.0.1";
        }

        public static string GetPublicIPAddress()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // Abrufen der öffentlichen IP-Adresse von einem externen Dienst
                    var response = client.GetStringAsync("https://api.ipify.org").Result;
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen der öffentlichen IP-Adresse: {ex.Message}");
                return "Unbekannt";
            }
        }
    }
}
