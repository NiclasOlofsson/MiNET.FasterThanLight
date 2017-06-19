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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MiNET.Ftl.Core.Net;
using FtlCreatePlayer = MiNET.Net.FtlCreatePlayer;

namespace MiNET.Ftl.Core.Node
{
	public class NodeServerManager : IServerManager
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(NodeServerManager));

		private MiNetServer _server;
		public TcpListener _listener;

		public NodeServerManager(MiNetServer server, int port = 0)
		{
			_server = server;

			_listener = new TcpListener(IPAddress.Loopback, port);

			// Start listening for client requests.
			_listener.Start();

			new Thread(() =>
				{
					// Enter the listening loop.
					while (true)
					{
						Log.Debug("LocalServerManager Waiting for a connection... " + _listener.LocalEndpoint);

						// Perform a blocking call to accept requests.
						// You could also user server.AcceptSocket() here.
						//using (TcpClient client = _listener.AcceptTcpClient())
						TcpClient client = _listener.AcceptTcpClient();
						client.NoDelay = true;
						client.SendBufferSize = client.SendBufferSize * 10;
						NodeNetworkHandler.FastThreadPool.QueueUserWorkItem(() =>
						{
							try
							{
								Log.Debug("LocalServerManager Got a connection from proxy... ");

								// Get a stream object for reading and writing
								NetworkStream stream = client.GetStream();
								BinaryReader reader = new BinaryReader(stream);

								int packageNs = reader.ReadByte();
								int len = reader.ReadInt32();
								byte[] bytes = reader.ReadBytes(len);
								if (bytes[0] != 0x01)
								{
									Log.Error("Got a bad packet");
								}
								else
								{
									FtlCreatePlayer message = FtlPackageFactory.CreatePackage(bytes[0], bytes) as FtlCreatePlayer;
									if (message == null)
									{
										Log.Error($"Bad parse of message");
									}
									else
									{
										Log.Debug($"Username={message.username}");
										Log.Debug($"Client UUID={message.clientuuid}");
										Log.Debug($"Server Address={message.serverAddress}");
										Log.Debug($"Client ID={message.clientId}");
										Log.Debug($"Got Skin={message.skin != null}");

										PlayerInfo playerInfo = new PlayerInfo()
										{
											Username = message.username,
											ClientUuid = message.clientuuid,
											ServerAddress = message.serverAddress,
											ClientId = message.clientId,
											Skin = message.skin
										};

										Player player = _server.PlayerFactory.CreatePlayer(_server, (IPEndPoint) client.Client.RemoteEndPoint, playerInfo);
										player.UseCreativeInventory = false;

										var handler = new NodeNetworkHandler(_server, player, client);

										player.NetworkHandler = handler;
										player.CertificateData = null;
										player.Username = message.username;
										player.ClientUuid = message.clientuuid;
										player.ServerAddress = message.serverAddress;
										player.ClientId = message.clientId;
										player.Skin = message.skin;
										message.PutPool();

										BinaryWriter writer = new BinaryWriter(new BufferedStream(stream, client.SendBufferSize));
										IPEndPoint endpoint = (IPEndPoint) client.Client.LocalEndPoint;
										writer.Write(endpoint.Port);
										writer.Flush();
									}
								}
							}
							catch (Exception e)
							{
								try
								{
									client.Close();
								}
								catch (Exception)
								{
								}
							}
						});
						//{IsBackground = true}.Start();
					}
				})
				{IsBackground = true}.Start();
		}

		public IServer GetServer()
		{
			// Never called on node
			return null;
		}
	}
}