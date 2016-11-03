using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MiNET.Net;

namespace MiNET.Ftl.Core.Node
{
	public class NodeServerManager : IServerManager
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (NodeServerManager));

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
					client.SendBufferSize = client.SendBufferSize*10;
					NodeNetworkHandler.FastThreadPool.QueueUserWorkItem(() =>
					{
						try
						{
							Log.Debug("LocalServerManager Got a connection from proxy... ");

							// Get a stream object for reading and writing
							NetworkStream stream = client.GetStream();
							BinaryReader reader = new BinaryReader(stream);

							int len = reader.ReadInt32();
							byte[] bytes = reader.ReadBytes(len);
							if (bytes[0] != 0x01)
							{
								Log.Error("Got a bad packet");
							}
							else
							{
								FtlCreatePlayer message = PackageFactory.CreatePackage(bytes[0], bytes, "ftl") as FtlCreatePlayer;
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

									Player player = _server.PlayerFactory.CreatePlayer(_server, (IPEndPoint) client.Client.RemoteEndPoint);
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