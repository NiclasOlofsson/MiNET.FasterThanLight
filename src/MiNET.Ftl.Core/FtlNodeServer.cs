using System;
using System.Threading;
using MiNET.Ftl.Core.Node;
using MiNET.Ftl.Core.Proxy;
using MiNET.Utils;

namespace MiNET.Ftl.Core
{
	public class FtlNodeServer
	{
		MiNetServer _server;

		public FtlNodeServer()
		{
			NodeNetworkHandler.FastThreadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount));

			int threads;
			int iothreads;
			ThreadPool.GetMinThreads(out threads, out iothreads);
			ThreadPool.SetMinThreads(4000, iothreads);

			_server = new MiNetServer();
			_server.ServerRole = ServerRole.Node;
			_server.ServerManager = new NodeServerManager(_server, 51234);
			_server.LevelManager = new SpreadLevelManager(1);
		}

		public void StartServer()
		{
			_server.StartServer();
		}

		public void StopServer()
		{
			_server.StopServer();
		}
	}
}