using System;
using Tools;

namespace NoIPUpdateTool
{
    class Program
    {

        const string _URL_API_IP4 = "https://api.ipify.org/?format=json";
        const string _URL_API_IP6 = "https://api6.ipify.org/?format=json";

        static void Main(string[] args)
        {
            var settings = AppSettings.FromJson(SettingsFile.LoadJson("appsettings.json"));
            
            var ip4 = External.FromJson(Http.Get(_URL_API_IP4));
            var ip6 = External.FromJson(Http.Get(_URL_API_IP6));
            Console.WriteLine("Hello World!");
            Console.WriteLine($"IPv4 Address: {ip4.Ip}");
            if (Http.IsIPv4(ip4.Ip)) {
                Console.WriteLine("This is a valid IPv4 Address");
            } else {
                Console.WriteLine("This is NOT a valid IPv4 Address");
            }
            Console.WriteLine($"IPv6 Address: {ip6.Ip}");
            if (Http.IsIPv6(ip6.Ip))
            {
                Console.WriteLine("This is a valid IPv6 Address");
            }
            else
            {
                Console.WriteLine("This is NOT a valid IPv6 Address");
            }
        }
    }
}
