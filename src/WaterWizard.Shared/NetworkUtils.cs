using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace WaterWizard.Shared
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

        public static string GetPublicIPAddress()
        {
            try
            {
                using var client = new HttpClient();
                // Abrufen der �ffentlichen IP-Adresse von einem externen Dienst
                var response = client.GetStringAsync("https://api.ipify.org").Result;
                if(response == null)
                {
                    return "funktioniert nicht";
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen der �ffentlichen IP-Adresse: {ex.Message}");
                return "Unbekannt";
            }
        }
    }
}
