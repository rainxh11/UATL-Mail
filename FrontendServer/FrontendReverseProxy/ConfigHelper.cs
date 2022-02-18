using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Swan.Logging;
using EmbedIO;
using LogLevel = Swan.Logging.LogLevel;
using EmbedIO.Actions;
using EmbedIO.Files;

namespace UATL.Mail.FrontendServer
{
    public class FrontendWebServer
    {
        public static WebServer CreateWebServer(DirectoryInfo htmlDir, IEnumerable<string> urls)
        {
            HttpClient httpClient = new HttpClient();

            var server = new WebServer(o => o
                    .WithUrlPrefixes(urls)
                    .WithMicrosoftHttpListener()
                    .WithMode(HttpListenerMode.Microsoft))
                .WithLocalSessionManager()
                .WithStaticFolder("/", htmlDir.FullName, true, m => m
                    .WithContentCaching(true)
                    .PreferCompressionFor("text/javascript", true)
                    .PreferCompressionFor("application/json", true)
                    .PreferCompressionFor("text/css", true)
                    .HandleMappingFailed((ctx, map) => {
                        ctx.Redirect("/");
                        return Task.CompletedTask;
                    })
                    )
                .HandleUnhandledException(async (ctx, ex) =>
                {
                    ex.Message.Error();
                    ctx.Response.StatusCode = 500;
                    await ctx.SendStringAsync("", "", System.Text.Encoding.UTF8);
                })
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx =>
                {
                    ctx.Redirect("/");
                    return Task.CompletedTask;
                }));



            // Listen for state changes.
            server.StateChanged += (s, e) =>
            {
                @$"[State Change] Old State: {e.OldState} | New State: {e.NewState}".Debug();
            };
            InitLogger();

            return server;
        }
        private static void InitLogger()
        {
            var logDir = new DirectoryInfo(AppContext.BaseDirectory + @$"\Log");
            if (!logDir.Exists)
                logDir.Create();

            var fileLogger = new FileLogger(AppContext.BaseDirectory + @$"\Log\UATLMail_Webserver_Log.log", true)
            {
                LogLevel = LogLevel.Debug
            };

            Logger.RegisterLogger(fileLogger);

            /*foreach (var logLevel in Enum.GetNames<LogLevel>())
            {
                var fileLogger = new FileLogger(AppContext.BaseDirectory + @$"\Log\[{logLevel.ToUpperInvariant()}]UATLMail_Webserver_Log.log", true);

                Logger.RegisterLogger(fileLogger);
            }*/
        }
    }

    public class ServerConfig
    {
        public IEnumerable<string> Urls { get; set; } = new List<string>();
        public string Certificate { get; set; }
        public string CerfiticateKey { get; set; }

    }

    public class ConfigHelper
    {
        public static ServerConfig GetConfig()
        {
            var config = new ServerConfig();
            try
            {
                var configFilePath = AppContext.BaseDirectory + @"\Config.json";
                var json = File.ReadAllText(configFilePath);
                config = JsonSerializer.Deserialize<ServerConfig>(json);
            }
            catch(Exception ex)
            {
                Logger.Log(ex, "ConfigFile", "Config File Deserialization Problem", "UATL.Mail.FrontendServer.ConfigHelper.GetConfig()");
            }
            finally
            {
                if(config.Urls.Count() == 0)
                {
                    Logger.Log("ConfigFile", "Config Urls was empty, searching for availlable network interfaces.", messageType: LogLevel.Info,"UATL.Mail.FrontendServer.ConfigHelper.GetConfig()");

                    config.Urls = GetHosts();
                }
            }
            return config;
        }

        private static IEnumerable<string> GetHosts(int port = 80)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                throw new Exception("There's no availlable network to bind the server to!");

            var availlableInterfaces = NetworkInterface
                .GetAllNetworkInterfaces()
                .SelectMany(x => x.GetIPProperties().UnicastAddresses)
                .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                .Where(x => CheckPortIsAvaillable(x.Address, port));

            foreach (var netInterface in availlableInterfaces)
            {
                yield return $"http://{netInterface.Address}:{port}/";
            }
        }
        private static bool CheckPortIsAvaillable(IPAddress ip, int port = 80)
        {
            return !IPGlobalProperties
                .GetIPGlobalProperties()
                .GetActiveTcpConnections()
                .Any(x => x.LocalEndPoint.Port == port && x.LocalEndPoint.Address == ip);
        }

    }
}