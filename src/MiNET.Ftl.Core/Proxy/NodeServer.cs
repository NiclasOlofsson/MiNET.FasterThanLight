using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using log4net;
using MiNET.Net;

namespace MiNET.Ftl.Core.Proxy
{
	public class NodeServer : IServer
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (NodeServer));

		private readonly EndPoint _serverEndpoint;

		public NodeServer(EndPoint serverEndpoint)
		{
			_serverEndpoint = serverEndpoint;
		}

		public IMcpeMessageHandler CreatePlayer(INetworkHandler session, PlayerInfo playerInfo)
		{
			Stopwatch timer = new Stopwatch();
			timer.Start();

			try
			{
				Log.Debug($"Proxy connecting to node on {_serverEndpoint}");

				TcpClient client = new TcpClient() /*{NoDelay = true}*/;
				client.NoDelay = true;
				client.ReceiveBufferSize = client.ReceiveBufferSize * 10;

				{
					var endPoint = (IPEndPoint) _serverEndpoint;
					client.Connect(endPoint);

					Log.Debug("Connected to node, requesting new player");

					var stream = client.GetStream();
					BinaryWriter writer = new BinaryWriter(new BufferedStream(stream, client.SendBufferSize)); ;
					BinaryReader reader = new BinaryReader(stream);

					FtlCreatePlayer message = new FtlCreatePlayer();
					message.username = playerInfo.Username;
					message.clientuuid = playerInfo.ClientUuid;
					message.serverAddress = playerInfo.ServerAddress;
					message.clientId = playerInfo.ClientId;
					message.skin = playerInfo.Skin;
					var bytes = message.Encode();

					writer.Write((byte)1);
					writer.Write(bytes.Length);
					writer.Write(bytes);
					writer.Flush();

					int port = reader.ReadInt32();
					Log.Debug("Recieved port for node message handler " + port);

					IMcpeMessageHandler handler = new ProxyMessageHandler(client, session);

					return handler;
				}
			}
			catch (Exception e)
			{
				Log.Error($"Failed communication with node after {timer.ElapsedMilliseconds}ms", e);
			}

			return null;
		}
	}
}