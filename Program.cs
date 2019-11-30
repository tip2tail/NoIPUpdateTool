using System;
using Tools;
using static Tools.SettingsFile;

namespace NoIPUpdateTool
{
    class Program
    {

        private static AppSettings _settings;
        const string _URL_API_IP4 = "https://api.ipify.org/?format=json";
        const string _URL_API_IP6 = "https://api6.ipify.org/?format=json";
        const string _URL_API_NOIP_UPDATE = "https://dynupdate.no-ip.com/nic/update";
        const string _USER_AGENT = "tip2tail NoIPUpdateTool/DotNetCore-1.0.0 mark@tip2tail.scot";
        const int _FORCE_EVERY_INTERVAL = 100;

        private static AppSettings Settings(bool forceReload = false)
        {
            if (_settings == null || forceReload) {
                if (!SettingsFile.Exists("appsettings.json")) {
                    _settings = new AppSettings();
                    SaveSettings();
                } else {
                    _settings =  AppSettings.FromJson(SettingsFile.LoadJson("appsettings.json"));
                }
            }
            return _settings;
        }

        private static void SaveSettings()
        {
            var settings = Settings();
            SettingsFile.Save("appsettings.json", settings.ToJson());
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Obtaining external IP addresses...");
            var ip4 = External.FromJson(Http.Get(_URL_API_IP4)).Ip;
            var validIp4 = Http.IsIPv4(ip4) ? "Valid" : "Invalid";
            var ip6 = External.FromJson(Http.Get(_URL_API_IP6)).Ip;
            var validIp6 = Http.IsIPv6(ip6) ? "Valid" : "Invalid";

            Console.WriteLine($"Current IPv4 Address: {ip4} ({validIp4})");
            Console.WriteLine($"Current IPv6 Address: {ip6} ({validIp6})");

            if (Settings().Hosts == null || Settings().Hosts.Length < 1) {
                Console.WriteLine("No hosts defined.  Terminating.");
                return;
            }

            var doUpdate = true;
            if (Settings().LastIp4 == ip4) {
                // Same as last time! Increment or Update?
                doUpdate = false;
                Settings().Interval++;
                if (Settings().Interval >= _FORCE_EVERY_INTERVAL) {
                    Settings().Interval = 0;
                    doUpdate = true;
                }
            }

            var authString = $"{Settings().Username}:{Settings().Password}".Base64();

            foreach (var host in Settings().Hosts)
            {
                if (!doUpdate) {
                    Console.WriteLine("IP unchanged - no updates!");
                    break;
                }

                Console.WriteLine($"Processing update for host: {host}");
                var url = $"{_URL_API_NOIP_UPDATE}?hostname={host}&myip={ip4}";
                if (validIp6 == "Valid") {
                    url += $"&myipv6={ip6}";
                }
                var outcome = Http.Get(url, _USER_AGENT, authString);
                Console.WriteLine($"Response: {outcome}");
            }

            // Save the current IP addresses
            Settings().LastIp4 = ip4;
            if (validIp6 == "Valid") {
                Settings().LastIp6 = ip6;
                Settings().UseIp6 = true;
            } else {
                Settings().LastIp6 = "";
                Settings().UseIp6 = false;
            }
            SaveSettings();

            Console.WriteLine("Update Complete");
        }
    }
}
