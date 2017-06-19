#region LICENSE

// The contents of this file are subject to the Common Public Attribution
// License Version 1.0. (the "License"); you may not use this file except in
// compliance with the License. You may obtain a copy of the License at
// https://github.com/NiclasOlofsson/MiNET/blob/master/LICENSE. 
// The License is based on the Mozilla Public License Version 1.1, but Sections 14 
// and 15 have been added to cover use of software over a computer network and 
// provide for limited attribution for the Original Developer. In addition, Exhibit A has 
// been modified to be consistent with Exhibit B.
// 
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License for
// the specific language governing rights and limitations under the License.
// 
// The Original Code is Niclas Olofsson.
// 
// The Original Developer is the Initial Developer.  The Initial Developer of
// the Original Code is Niclas Olofsson.
// 
// All portions of the code written by Niclas Olofsson are Copyright (c) 2014-2017 Niclas Olofsson. 
// All Rights Reserved.

#endregion

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
		private static readonly ILog Log = LogManager.GetLogger(typeof(NodeClient));

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
				ClientUuid = new UUID(Guid.NewGuid().ToString()),
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

			_toNodeHandler.HandleMcpeClientToServerHandshake(null);

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
			movePlayerPacket.runtimeEntityId = EntityId;
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