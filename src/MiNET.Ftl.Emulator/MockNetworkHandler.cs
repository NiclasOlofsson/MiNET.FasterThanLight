using System.Net;
using log4net;
using MiNET.Net;

namespace MiNET.Ftl.Emulator
{
	public class MockNetworkHandler : INetworkHandler
	{
		private readonly NodeClient _nodeClient;
		private static readonly ILog Log = LogManager.GetLogger(typeof (MockNetworkHandler));

		public MockNetworkHandler(NodeClient nodeClient)
		{
			_nodeClient = nodeClient;
		}

		public void Close()
		{
		}


		public void SendDirectPackage(Package package)
		{
			SendPackage(package);
		}

		public void SendPackage(Package package)
		{
			//Log.Debug($"Client recieved package {package.GetType().Name}");

			if (package is McpeRespawn)
			{
				OnMcpeRespawn((McpeRespawn) package);
			}
			else if (package is McpeStartGame)
			{
				OnMcpeStartGame((McpeStartGame) package);
			}

			package.PutPool();
		}

		private void OnMcpeStartGame(McpeStartGame package)
		{
			_nodeClient.EntityId = package.entityId;
		}

		private void OnMcpeRespawn(McpeRespawn package)
		{
			_nodeClient.SpawnX = package.x;
			_nodeClient.SpawnY = package.y;
			_nodeClient.SpawnZ = package.z;
		}

		public IPEndPoint GetClientEndPoint()
		{
			return null;
		}
	}
}