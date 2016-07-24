using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using log4net;
using MiNET.Net;
using MiNET.Utils;

namespace MiNET.Ftl.Core.Proxy
{
	public class ProxyMessageHandler : BaseMessageHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (ProxyMessageHandler));

		public static DedicatedThreadPool FastThreadPool { get; set; }

		static ProxyMessageHandler()
		{
			//int threads;
			//int iothreads;
			//ThreadPool.GetMinThreads(out threads, out iothreads);
			//FastThreadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(threads));
		}


		private readonly IPEndPoint _endPoint;
		private readonly INetworkHandler _networkHandler;
		private BinaryWriter _writer;
		private TcpClient _client;


		public ProxyMessageHandler(TcpClient client, INetworkHandler networkHandler)
		{
			_client = client;
			_endPoint = (IPEndPoint) client.Client.RemoteEndPoint;
			_networkHandler = networkHandler;

			Log.Debug("Connecting from proxy to local server handler");


			Log.Debug("CONNECTED from proxy to local server handler");

			var stream = _client.GetStream();
			_writer = new BinaryWriter(stream);
			BinaryReader reader = new BinaryReader(stream);

			new Thread(() =>
			{
				while (true)
				{
					if (_client == null) return;

					try
					{
						int len = reader.ReadInt32();
						if (len == -1)
						{
							Close();
							return;
						}

						byte[] bytes = reader.ReadBytes(len);

						if (len == 0) continue;

						//FastThreadPool.QueueUserWorkItem(() =>
						//{
							var message = PackageFactory.CreatePackage(bytes[0], bytes, "mcpe");
							if (message == null)
							{
								message = PackageFactory.CreatePackage(bytes[0], bytes, "ftl");

								if (message == null)
								{
									Log.Error($"Bad parse of message");
								}
							}

							if (message != null)
							{
								message.Timer.Restart();

								Log.Debug($"Got message {message.GetType().Name}");
								_networkHandler.SendPackage(message);
							}
						//});
					}
					catch (Exception e)
					{
						Log.Error("Receive error", e);
						Close();
					}
				}
			})
			{IsBackground = true}.Start();
		}

		private object _writeLock = new object();

		private void Close()
		{
			Log.Warn("Closing proxy connection to node");

			lock (_writeLock)
			{
				try
				{
					if (_writer != null)
					{
						_writer.Flush();
						_writer.Close();
						_writer = null;
					}
				}
				catch (Exception)
				{
				}


				if (_client != null)
				{
					_client.Close();
				}

				_client = null;

				_networkHandler.Close();
			}
		}

		public override void WritePackage(Package message)
		{
			lock (_writeLock)
			{
				if (_writer == null) return;

				if (message == null) return;

				Log.Debug($"Sending from proxy to node {message.GetType().Name}");
				try
				{
					_writer.Write(message.Bytes.Length);
					_writer.Write(message.Bytes);
					_writer.Flush();
				}
				catch (Exception e)
				{
					Log.Error("Failed to write message to node", e);
					try
					{
						Close();
					}
					catch (Exception ex)
					{
					}
				}
			}
		}

		public override void WriteBytes(byte[] message)
		{
			lock (_writeLock)
			{
				if (_writer == null) return;

				if (message == null) return;

				Log.Debug($"Sending from proxy to node {message.GetType().Name}");
				try
				{
					_writer.Write(message.Length);
					_writer.Write(message);
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
					}
				}
			}
		}
	}
}