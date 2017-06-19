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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MiNET.Net;
using MiNET.Utils;

namespace MiNET.Ftl.Core.Node
{
	public class NodeNetworkHandler : INetworkHandler
	{
		public static DedicatedThreadPool FastThreadPool { get; set; }

		static NodeNetworkHandler()
		{
		}

		private static readonly ILog Log = LogManager.GetLogger(typeof(NodeNetworkHandler));

		private readonly MiNetServer _server;
		private readonly Player _player;
		private BinaryWriter _writer;
		private TcpClient _client;

		public NodeNetworkHandler(MiNetServer server, Player player, TcpClient client)
		{
			_server = server;
			_player = player;
			_client = client;

			new Thread(() =>
				{
					try
					{
						_server.ServerInfo.PlayerSessions.TryAdd((IPEndPoint) _client.Client.RemoteEndPoint, null);

						Log.Debug("RemoteNetworkHandler waiting for a connection... " + _client.Client.RemoteEndPoint);

						// Perform a blocking call to accept requests.
						// You could also user server.AcceptSocket() here.
						//_client = _listener.AcceptTcpClient();

						Log.Debug("RemoteNetworkHandler got a proxy connection... ");

						NetworkStream stream = _client.GetStream();
						BinaryReader reader = new BinaryReader(stream);
						_writer = new BinaryWriter(new BufferedStream(stream, _client.SendBufferSize));
						while (true)
						{
							if (_client == null) return;
							// Get a stream object for reading and writing

							try
							{
								int packageNs = reader.ReadByte();
								int len = reader.ReadInt32();
								byte[] bytes = reader.ReadBytes(len);

								Package message = null;
								if (packageNs == 0)
								{
									message = PackageFactory.CreatePackage(bytes[0], bytes, "mcpe");
								}
								else if (packageNs == 1)
								{
									message = PackageFactory.CreatePackage(bytes[0], bytes, "ftl");
								}

								if (message == null)
								{
									Log.Error($"Bad parse of message");
								}
								else
								{
									message.Timer.Restart();

									Log.Debug($"Got message {message.GetType().Name}");

									if (_server != null)
									{
										ServerInfo serverInfo = _server.ServerInfo;
										Interlocked.Increment(ref serverInfo.NumberOfPacketsInPerSecond);
									}

									//FastThreadPool.QueueUserWorkItem(() =>
									//{
									HandlePackage(message);
									message.PutPool();
									//});
								}
							}
							catch (Exception)
							{
								try
								{
									_player.Disconnect("Lost connection", false);
								}
								catch (Exception)
								{
									_client = null;
								}
							}
						}
					}
					catch (Exception e)
					{
					}
				})
				{IsBackground = true}.Start();
		}

		private Stopwatch _loginTimer = new Stopwatch();

		private void HandlePackage(Package message)
		{
			var handler = _player;
			//var result = Server.PluginManager.PluginPacketHandler(message, true, Player);
			////if (result != message) message.PutPool();
			//message = result;

			if (message == null)
			{
				return;
			}

			else if (typeof(McpeDisconnect) == message.GetType())
			{
				McpeDisconnect disconnect = (McpeDisconnect) message;
				Log.Warn("Got disconnect in node: " + disconnect.message);

				// Start encrypotion
				handler.Disconnect(disconnect.message, false);
			}

			else if (typeof(McpeClientToServerHandshake) == message.GetType())
			{
				_loginTimer.Start();
				// Start encrypotion
				handler.HandleMcpeClientToServerHandshake((McpeClientToServerHandshake) message);
			}

			else if (typeof(McpeUpdateBlock) == message.GetType())
			{
				// DO NOT USE. Will dissapear from MCPE any release. 
				// It is a bug that it leaks these messages.
			}

			else if (typeof(McpeRemoveBlock) == message.GetType())
			{
				handler.HandleMcpeRemoveBlock((McpeRemoveBlock) message);
			}

			else if (typeof(McpeAnimate) == message.GetType())
			{
				handler.HandleMcpeAnimate((McpeAnimate) message);
			}

			else if (typeof(McpeUseItem) == message.GetType())
			{
				handler.HandleMcpeUseItem((McpeUseItem) message);
			}

			else if (typeof(McpeEntityEvent) == message.GetType())
			{
				handler.HandleMcpeEntityEvent((McpeEntityEvent) message);
			}

			else if (typeof(McpeText) == message.GetType())
			{
				handler.HandleMcpeText((McpeText) message);
			}

			else if (typeof(McpeRemoveEntity) == message.GetType())
			{
				// Do nothing right now, but should clear out the entities and stuff
				// from this players internal structure.
			}

			else if (typeof(McpeLogin) == message.GetType())
			{
				handler.HandleMcpeLogin((McpeLogin) message);
			}

			else if (typeof(McpeMovePlayer) == message.GetType())
			{
				handler.HandleMcpeMovePlayer((McpeMovePlayer) message);
			}

			else if (typeof(McpeInteract) == message.GetType())
			{
				handler.HandleMcpeInteract((McpeInteract) message);
			}

			else if (typeof(McpeRespawn) == message.GetType())
			{
				handler.HandleMcpeRespawn((McpeRespawn) message);
			}

			else if (typeof(McpeBlockEntityData) == message.GetType())
			{
				handler.HandleMcpeBlockEntityData((McpeBlockEntityData) message);
			}

			else if (typeof(McpePlayerAction) == message.GetType())
			{
				handler.HandleMcpePlayerAction((McpePlayerAction) message);
			}

			else if (typeof(McpeDropItem) == message.GetType())
			{
				handler.HandleMcpeDropItem((McpeDropItem) message);
			}

			else if (typeof(McpeContainerSetSlot) == message.GetType())
			{
				handler.HandleMcpeContainerSetSlot((McpeContainerSetSlot) message);
			}

			else if (typeof(McpeContainerClose) == message.GetType())
			{
				handler.HandleMcpeContainerClose((McpeContainerClose) message);
			}

			else if (typeof(McpeMobEquipment) == message.GetType())
			{
				handler.HandleMcpeMobEquipment((McpeMobEquipment) message);
			}

			else if (typeof(McpeMobArmorEquipment) == message.GetType())
			{
				handler.HandleMcpeMobArmorEquipment((McpeMobArmorEquipment) message);
			}

			else if (typeof(McpeCraftingEvent) == message.GetType())
			{
				handler.HandleMcpeCraftingEvent((McpeCraftingEvent) message);
			}

			else if (typeof(McpeRequestChunkRadius) == message.GetType())
			{
				handler.HandleMcpeRequestChunkRadius((McpeRequestChunkRadius) message);
			}

			else if (typeof(McpeMapInfoRequest) == message.GetType())
			{
				handler.HandleMcpeMapInfoRequest((McpeMapInfoRequest) message);
			}

			else if (typeof(McpeItemFrameDropItem) == message.GetType())
			{
				handler.HandleMcpeItemFrameDropItem((McpeItemFrameDropItem) message);
			}

			else if (typeof(McpeResourcePackClientResponse) == message.GetType())
			{
				handler.HandleMcpeResourcePackClientResponse((McpeResourcePackClientResponse) message);
			}

			else if (typeof(McpePlayerInput) == message.GetType())
			{
				handler.HandleMcpePlayerInput((McpePlayerInput) message);
			}

			else
			{
				Log.Error($"Unhandled package: {message.GetType().Name} 0x{message.Id:X2} for user: {_player.Username}");
				return;
			}

			if (message.Timer.IsRunning)
			{
				long elapsedMilliseconds = message.Timer.ElapsedMilliseconds;
				if (elapsedMilliseconds > 1000)
				{
					Log.WarnFormat("Package (0x{1:x2}) handling too long {0}ms for {2}", elapsedMilliseconds, message.Id, _player.Username);
				}
			}
			else
			{
				Log.WarnFormat("Package (0x{0:x2}) timer not started for {1}.", message.Id, _player.Username);
			}
		}


		public void Close()
		{
			Log.Warn($"Called Close() node for {_player?.Username}");

			var writer = _writer;
			var client = _client;

			_writer = null;
			_client = null;

			PlayerNetworkSession value;
			_server.ServerInfo.PlayerSessions.TryRemove((IPEndPoint) client.Client.RemoteEndPoint, out value);

			lock (_writeLock)
			{
				if (writer != null)
				{
					try
					{
						writer.Write((byte) 0);
						writer.Write(-1); // Signale EOS
						writer.Flush();
						writer.Close();
					}
					catch (Exception)
					{
					}
				}

				if (client != null)
				{
					client.Close();
				}
			}
		}


		private object _writeLock = new object();

		public void SendPackageToProxy(Package package)
		{
			if (!_player.IsConnected) return;

			Log.Debug($"Writing package to proxy {package.GetType().Name}");


			//FastThreadPool.QueueUserWorkItem(() =>
			//{
			if (_server != null)
			{
				ServerInfo serverInfo = _server.ServerInfo;
				Interlocked.Increment(ref serverInfo.NumberOfPacketsOutPerSecond);
			}

			lock (_writeLock)
			{
				if (_writer == null) return;

				try
				{
					var bytes = package.Encode();
					if (bytes.Length > _client.SendBufferSize) Log.Warn($"Data of length {bytes.Length}bytes is bigger than TCP buffe {_client.SendBufferSize}bytes");
					_writer.Write((byte) 0);
					_writer.Write(bytes.Length);
					_writer.Write(bytes);
					_writer.Flush();
				}
				catch (Exception e)
				{
					try
					{
						Close();
					}
					catch (Exception ex)
					{
						_client = null;
					}
				}
			}

			var game = package as McpePlayStatus;
			if (game != null)
			{
				if (game.status == 3)
				{
					if (_loginTimer != null && _loginTimer.Elapsed > TimeSpan.FromSeconds(1))
					{
						_loginTimer = null;
						Log.Warn($"Took long time ({_loginTimer.ElapsedMilliseconds}ms) to login for user {_player?.Username}");
					}
				}
			}

			package.PutPool();
			//});
		}

		public void SendDirectPackage(Package package)
		{
			Log.Debug($"Writing package direct to proxy {package.GetType().Name}");

			SendPackageToProxy(package);
		}

		public IPEndPoint GetClientEndPoint()
		{
			return null;
		}

		public long GetNetworkNetworkIdentifier()
		{
			return 0;
		}


		private Timer _sendTicker;
		private Queue<Package> _sendQueueNotConcurrent = new Queue<Package>();
		private object _queueSync = new object();

		public void SendPackage(Package package)
		{
			SendPackageToProxy(package);
			//MiNetServer.TraceSend(package);

			//if (package == null) return;

			//lock (_queueSync)
			//{
			//	if (_sendTicker == null)
			//	{
			//		_sendTicker = new Timer(SendQueue, null, 10, 10); // RakNet send tick-time
			//	}

			//	_sendQueueNotConcurrent.Enqueue(package);
			//}
		}
	}
}