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

namespace MiNET.Ftl.Node.Service
{
	public class MiNetFtlNodeService
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (MiNetFtlNodeService));

		private FtlNodeServer _server;

		/// <summary>
		///     Starts this instance.
		/// </summary>
		private void Start()
		{
			Log.Info("Starting MiNET FTL node");
			_server = new FtlNodeServer();
			_server.StartServer();
		}

		/// <summary>
		///     Stops this instance.
		/// </summary>
		private void Stop()
		{
			Log.Info("Stopping MiNET FTL node");
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
				host.Service<MiNetFtlNodeService>(s =>
				{
					s.ConstructUsing(construct => new MiNetFtlNodeService());
					s.WhenStarted(service => service.Start());
					s.WhenStopped(service => service.Stop());
				});

				host.RunAsLocalService();
				host.SetDisplayName("MiNET FTL Node Service");
				host.SetDescription("MiNET faster-than-light node server.");
				host.SetServiceName("MiNET Node");
			});
		}
	}
}