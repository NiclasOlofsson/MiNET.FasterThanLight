using System;
using System.Net;
using System.Text;
using log4net;
using MiNET.Ftl.Core.Proxy;
using MiNET.Net;
using MiNET.Utils;

namespace MiNET.Ftl.Emulator
{
	public class NodeClient
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (NodeClient));

		private readonly IPEndPoint _endPoint;
		private readonly string _username;
		private readonly DedicatedThreadPool _threadPool;
		private BaseMessageHandler _toNodeHandler;
		private MockNetworkHandler _fromNodeHander;

		public NodeClient(IPEndPoint endPoint, string username, DedicatedThreadPool threadPool)
		{
			_endPoint = endPoint;
			_username = username;
			_threadPool = threadPool;
		}

		public int ChunkRadius { get; set; }
		public int ClientId { get; set; }
		public bool IsRunning { get; set; } = true;
		public PlayerLocation CurrentLocation { get; set; }
		public float SpawnX { get; set; }
		public float SpawnY { get; set; }
		public float SpawnZ { get; set; }
		public long EntityId { get; set; }

		public void StartClient()
		{
			Skin skin = new Skin
			{
				Slim = false,
				Texture = Encoding.Default.GetBytes(new string('Z', 8192)),
				SkinType = "Standard_Custom"
			};

			var nodeServer = new NodeServer(_endPoint);

			PlayerInfo playerInfo = new PlayerInfo
			{
				Username = _username,
				ClientUuid = new UUID(Guid.NewGuid()),
				ClientId = ClientId,
				ServerAddress = "localhost",
				Skin = skin
			};

			_fromNodeHander = new MockNetworkHandler(this);
			_toNodeHandler = (BaseMessageHandler) nodeServer.CreatePlayer(_fromNodeHander, playerInfo);

			if (_toNodeHandler == null)
			{
				IsRunning = false;
				return;
			}

			_toNodeHandler.HandleMcpeClientMagic(null);

			McpeRequestChunkRadius radius = McpeRequestChunkRadius.CreateObject();
			radius.chunkRadius = ChunkRadius;
			
			_toNodeHandler.WriteBytes(radius.Encode());
			radius.PutPool();

			IsRunning = true;
		}

		public void StopClient()
		{
		}

		public void SendMcpeMovePlayer()
		{
			//Log.Debug($"Sending move {EntityId}: {CurrentLocation}");

			McpeMovePlayer movePlayerPacket = McpeMovePlayer.CreateObject();
			movePlayerPacket.entityId = EntityId;
			movePlayerPacket.x = CurrentLocation.X;
			movePlayerPacket.y = CurrentLocation.Y;
			movePlayerPacket.z = CurrentLocation.Z;
			movePlayerPacket.yaw = 91;
			movePlayerPacket.pitch = 28;
			movePlayerPacket.headYaw = 91;

			_toNodeHandler.WriteBytes(movePlayerPacket.Encode());
			movePlayerPacket.PutPool();
		}

		public void SendDisconnectionNotification()
		{
			_toNodeHandler.Disconnect("Client disconneted");
			IsRunning = false;
		}
	}
}