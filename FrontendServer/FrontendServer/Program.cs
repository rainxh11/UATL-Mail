using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Files;
using EmbedIO.WebApi;
using Swan.Logging;
using System.Linq;

namespace UATL.Mail.FrontendServer
{
    internal class Program
    {
        public static object UseFileCache { get; private set; }


        static async Task Main(string[] args)
        {
            var htmlDir = new DirectoryInfo(AppContext.BaseDirectory + @"\html");
            //var htmlDir = new DirectoryInfo(@"D:\WebProjects\University-APP\uatl-mail\Frontend\dist");
            var config = ConfigHelper.GetConfig();

            using (var webServer = CreateWebServer(htmlDir, config.Urls))
            {

                await webServer.RunAsync();

                Console.ReadKey(true);
            }
        }

        private static WebServer CreateWebServer(DirectoryInfo htmlDir, IEnumerable<string> urls)
        {
            var server = new WebServer(o => o
                    .WithUrlPrefixes(urls)
                    .WithMode(HttpListenerMode.Microsoft))
                .WithLocalSessionManager()
                .WithStaticFolder("/", htmlDir.FullName, true, m => m
                    .WithContentCaching(true)
                    .PreferCompressionFor("text/javascript", true)
                    .PreferCompressionFor("application/json", true)
                    .PreferCompressionFor("text/css", true)
                    .HandleMappingFailed((ctx,map) => {
                        ctx.Redirect("/");
                        return Task.CompletedTask;
                    })
                    )
                .HandleUnhandledException(async (ctx, ex) =>
                {
                    ex.Message.Error();
                    ctx.Response.StatusCode = 500;
                    await ctx.SendStringAsync("","", System.Text.Encoding.UTF8);
                })
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx =>
                {
                    ctx.Redirect("/");
                    return Task.CompletedTask;
                }) );



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
            if(!logDir.Exists)
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
}

