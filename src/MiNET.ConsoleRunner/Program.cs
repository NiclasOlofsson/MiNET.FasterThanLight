using System;
using System.Threading;
using log4net;
using log4net.Config;
using MiNET.Ftl.Core.Node;
using MiNET.Utils;

// Configure log4net using the .config file

[assembly: XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called TestApp.exe.config in the application base
// directory (i.e. the directory containing TestApp.exe)
// The config file will be watched for changes.

namespace MiNET.ConsoleRunner
{
	public class MiNetService
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (MiNetService));

		/// <summary>
		///     The programs entry point.
		/// </summary>
		/// <param name="args">The arguments.</param>
		private static void Main(string[] args)
		{
			XmlConfigurator.Configure();

			NodeNetworkHandler.FastThreadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount));

			var server = new MiNetServer();

			int threads;
			int iothreads;
			ThreadPool.GetMinThreads(out threads, out iothreads);
			ThreadPool.SetMinThreads(4000, iothreads);

			var localServerManager = new NodeServerManager(server, 51234);
			server.ServerManager = localServerManager;
			server.ServerRole = ServerRole.Node;
			server.LevelManager = new SpreadLevelManager(60);

			server.StartServer();

			Console.WriteLine("MiNET running...");
			Console.ReadLine();
		}
	}
}