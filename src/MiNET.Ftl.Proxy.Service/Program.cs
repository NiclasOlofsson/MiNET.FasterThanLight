using System;
using log4net;
using log4net.Config;
using MiNET.Ftl.Core;
using Topshelf;

// Configure log4net using the .config file

[assembly: XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called TestApp.exe.config in the application base
// directory (i.e. the directory containing TestApp.exe)
// The config file will be watched for changes.

namespace MiNET.Ftl.Proxy.Service
{
	public class MiNetFtlProxyService
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (MiNetFtlProxyService));

		private FtlProxyServer _server;

		/// <summary>
		///     Starts this instance.
		/// </summary>
		private void Start()
		{
			Log.Info("Starting MiNET FTL proxy");
			_server = new FtlProxyServer();
			_server.StartServer();
		}

		/// <summary>
		///     Stops this instance.
		/// </summary>
		private void Stop()
		{
			Log.Info("Stopping MiNET FTL proxy");
			_server.StopServer();
		}

		/// <summary>
		///     The programs entry point.
		/// </summary>
		/// <param name="args">The arguments.</param>
		private static void Main(string[] args)
		{
			XmlConfigurator.Configure();

			HostFactory.Run(host =>
			{
				host.Service<MiNetFtlProxyService>(s =>
				{
					s.ConstructUsing(construct => new MiNetFtlProxyService());
					s.WhenStarted(service => service.Start());
					s.WhenStopped(service => service.Stop());
				});

				host.RunAsLocalService();
				host.SetDisplayName("MiNET FTL Proxy Service");
				host.SetDescription("MiNET faster-than-light proxy server.");
				host.SetServiceName("MiNET Proxy");
			});
		}

		/// <summary>
		///     Determines whether is running on mono.
		/// </summary>
		/// <returns></returns>
		public static bool IsRunningOnMono()
		{
			return Type.GetType("Mono.Runtime") != null;
		}
	}
}