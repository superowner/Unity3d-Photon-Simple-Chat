using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon.SocketServer;
using System.IO;
using ExitGames.Logging;
using log4net.Config;
using ExitGames.Logging.Log4Net;
using Photon.SocketServer.Diagnostics;
using ExitGames.Diagnostics.Counter;
using ExitGames.Diagnostics.Monitoring;

namespace ChatServer
{
    class ChatServerApplication : ApplicationBase
    {
        private static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return new ChatServerPeer(initRequest);
        }

        protected override void Setup()
        {
            log4net.GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(this.ApplicationRootPath, "log");

            // log4net
            string path = Path.Combine(this.BinaryPath, "log4net.config");
            var file = new FileInfo(path);
            if (file.Exists)
            {
                LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
                XmlConfigurator.ConfigureAndWatch(file);
            }

            Log.InfoFormat("Created application Instance: type={0}", Instance.GetType());

            CounterPublisher.DefaultInstance.AddStaticCounterClass(typeof(Counter), "ChatServer");
            Protocol.AllowRawCustomValues = true;
        }

        protected override void TearDown()
        {
        }
    }

    /// <summary>
    /// Counter on application level
    /// </summary>
    public static class Counter
    {
        /// <summary>
        /// Absolute number of games active (in the game cache).
        /// </summary>
        [PublishCounter("ChatServers")]
        public static readonly NumericCounter ChatServers = new NumericCounter("ChatServers");
    }
}