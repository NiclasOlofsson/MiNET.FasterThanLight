using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using MiNET.Ftl.Core.Proxy;
using MiNET.Utils;

namespace MiNET.Ftl.Core
{
	public class FtlProxyServer
	{
		MiNetServer _server;

		public FtlProxyServer()
		{
			int threads;
			int iothreads;
			ThreadPool.GetMinThreads(out threads, out iothreads);

			DedicatedThreadPool threadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount));
			ProxyMessageHandler.FastThreadPool = threadPool;

			ThreadPool.GetMaxThreads(out threads, out iothreads);
			ThreadPool.SetMinThreads(4000, 4000);
			ThreadPool.SetMaxThreads(threads, 4000);

			List<EndPoint> remoteServers = new List<EndPoint>();
			remoteServers.Add(new IPEndPoint(IPAddress.Loopback, 51234));
			_server = new MiNetServer();
			_server.ServerRole = ServerRole.Proxy;
			_server.ServerManager = new ProxyServerManager(remoteServers);
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