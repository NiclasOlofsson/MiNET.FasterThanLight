using System;
using System.Collections.Generic;
using System.Net;

namespace MiNET.Ftl.Core.Proxy
{
	public class ProxyServerManager : IServerManager
	{
		private readonly List<NodeServer> _knownNodes = new List<NodeServer>();

		public ProxyServerManager(List<EndPoint> knownNodes)
		{
			foreach (EndPoint endPoint in knownNodes)
			{
				var node = new NodeServer(endPoint);
				_knownNodes.Add(node);
			}
		}

		public IServer GetServer()
		{
			Random random = new Random();
			int idx = random.Next(0, _knownNodes.Count);

			var remoteServer = _knownNodes[idx];

			return remoteServer;
		}
	}
}