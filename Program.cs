using System;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using Tools;

namespace NoIPUpdateTool
{
    class Program
    {

        private static AppSettings _settings;
        const string _URL_API_IP4 = "https://api.ipify.org/?format=json";
        const string _URL_API_IP6 = "https://api6.ipify.org/?format=json";
        const string _URL_API_NOIP_UPDATE = "https://dynupdate.no-ip.com/nic/update";
        const string _USER_AGENT = "tip2tail NoIPUpdateTool/DotNetCore-VERSION mark@tip2tail.scot";

        static int Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "NoIPUpdateTool",
                Description = "A tool to update the NoIP Dynamic DNS system via API calls",
            };

            app.HelpOption(inherited: true);
            app.ThrowOnUnexpectedArgument = false;

            // Action - update
            app.Command("update", cmdUpdate =>
            {
                cmdUpdate.Description = "Runs the NoIP API update process";
                cmdUpdate.OnExecute(() =>
                {
                    return ExecCommand_Update() ? 0 : 1;
                });
            });
            // Action - list-settings
            app.Command("list-settings", cmdListSettings =>
            {
                cmdListSettings.Description = "Shows the settings file in JSON format";
                cmdListSettings.OnExecute(() =>
                {
                    Console.WriteLine("Settings (as JSON):");
                    Console.WriteLine("");
                    Console.WriteLine(Settings().ToJson());
                    return 0;
                });
            });
            // Action - set-creds
            app.Command("set-creds", cmdSetCreds =>
            {
                cmdSetCreds.Description = "Updates the username and password settings";
                var username = cmdSetCreds.Argument("username", "Username for NoIP").IsRequired();
                var password = cmdSetCreds.Argument("password", "Password for NoIP").IsRequired();
                cmdSetCreds.OnExecute(() =>
                {
                    Settings().Username = username.Value;
                    Settings().Password = password.Value;
                    SaveSettings();
                    Console.WriteLine("Credentials updated");
                    return 0;
                });
            });
            // Action - set-force-interval
            app.Command("set-interval", cmdInterval =>
            {
                cmdInterval.Description = "Sets the interval of executions before a forced update";
                var interval = cmdInterval.Argument<int>("interval", "Force update every X updates").IsRequired();
                cmdInterval.OnExecute(() =>
                {
                    Settings().Interval = interval.ParsedValue;
                    SaveSettings();
                    Console.WriteLine("Interval updated");
                    return 0;
                });
            });
            // Action - set-hosts
            app.Command("set-hosts", cmdHosts =>
            {
                cmdHosts.Description = "Sets the list of hostnames for updating on each execution";
                var hosts = cmdHosts.Argument("hosts", "NoIP hostnames (provide all to update)", true).IsRequired();
                cmdHosts.OnExecute(() =>
                {
                    Settings().Hosts = hosts.Values.ToArray();
                    SaveSettings();
                    Console.WriteLine("Hosts updated");
                    return 0;
                });
            });
            // Action - about
            app.Command("about", cmdAbout =>
            {
                cmdAbout.Description = "Displays the about informaition for this tool";
                cmdAbout.OnExecute(() =>
                {
                    Console.WriteLine("NoIPUpdateTool");
                    Console.WriteLine("by Mark Young (tip2tail)");
                    Console.WriteLine("========================");
                    Console.WriteLine("");
                    Console.WriteLine($"Version: {Assembly.GetEntryAssembly().GetName().Version.ToString()}");
                    Console.WriteLine("More information at: https://github.com/tip2tail/NoIPUpdateTool/");
                    Console.WriteLine("");
                    return 0;
                });
            });

            app.OnExecute(() =>
            {
                Console.WriteLine("This tool can only be used with an action specified from the list");
                app.ShowHelp();
                return 1;
            });

            return app.Execute(args);
        }

        private static bool ExecCommand_Update()
        {
            try {
                Console.WriteLine("Obtaining external IP addresses...");
                var ip4 = External.FromJson(Http.Get(_URL_API_IP4)).Ip;
                var validIp4 = Http.IsIPv4(ip4) ? "Valid" : "Invalid";
                var ip6 = External.FromJson(Http.Get(_URL_API_IP6)).Ip;
                var validIp6 = Http.IsIPv6(ip6) ? "Valid" : "Invalid";

                Console.WriteLine($"Current IPv4 Address: {ip4} ({validIp4})");
                Console.WriteLine($"Current IPv6 Address: {ip6} ({validIp6})");

                if (Settings().Hosts == null || Settings().Hosts.Length < 1)
                {
                    Console.WriteLine("No hosts defined.  Terminating.");
                    return false;
                }

                var doUpdate = true;
                if (Settings().LastIp4 == ip4)
                {
                    // Same as last time! Increment or Update?
                    doUpdate = false;
                    Settings().Interval++;
                    if (Settings().Interval >= Settings().ForceInterval)
                    {
                        Settings().Interval = 0;
                        doUpdate = true;
                    }
                }

                var authString = $"{Settings().Username}:{Settings().Password}".Base64();

                foreach (var host in Settings().Hosts)
                {
                    if (!doUpdate)
                    {
                        Console.WriteLine("IP unchanged - no updates!");
                        break;
                    }

                    // Do the update request
                    Console.WriteLine($"Processing update for host: {host}");
                    var url = $"{_URL_API_NOIP_UPDATE}?hostname={host}&myip={ip4}";
                    if (validIp6 == "Valid")
                    {
                        url += $"&myipv6={ip6}";
                    }
                    var outcome = Http.Get(url, _USER_AGENT.Replace(
                        "VERSION",
                        Assembly.GetEntryAssembly().GetName().Version.ToString()
                    ), authString);
                    Console.WriteLine($"Response: {outcome}");
                }

                // Save the current IP addresses
                Settings().LastIp4 = ip4;
                if (validIp6 == "Valid")
                {
                    Settings().LastIp6 = ip6;
                    Settings().UseIp6 = true;
                }
                else
                {
                    Settings().LastIp6 = "";
                    Settings().UseIp6 = false;
                }
                SaveSettings();

                Console.WriteLine("Update Complete");
                return true;
            } catch (Exception exception) {
                Console.WriteLine($"Exception: {exception.Message}");
                return false;
            }
        }

        private static AppSettings Settings(bool forceReload = false)
        {
            if (_settings == null || forceReload) {
                if (!SettingsFile.Exists("appsettings.json")) {
                    _settings = new AppSettings();
                    _settings.ForceInterval = 100;
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
    }
}
