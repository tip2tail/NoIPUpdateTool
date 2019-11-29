using System;
using System.IO;
using System.Net;

namespace Tools
{

    public static class Http
    {

        public static string Get(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static bool IsIPv4(string ip)
        {
            if (ip == null) {
                return false;
            }
            if (IPAddress.TryParse(ip, out var address)) {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    return true;
                }
            }
            return false;
        }

        public static bool IsIPv6(string ip)
        {
            if (ip == null)
            {
                return false;
            }
            if (IPAddress.TryParse(ip, out var address))
            {
                if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsIP(string ip)
        {
            return (IsIPv4(ip) || IsIPv6(ip));
        }

    }

}