using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using log4net;
using MiNET.Utils;

namespace MiNET.Ftl.Emulator
{
	public class ClientEmulator
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (ClientEmulator));

		private readonly DedicatedThreadPool _threadPool;

		public int RanMin { get; set; }
		public int RanMax { get; set; }
		public int ChunkRadius { get; set; }

		public IPEndPoint EndPoint { get; }

		public Emulator Emulator { get; private set; }
		public string Name { get; set; }
		public int ClientId { get; set; }
		public Random Random { get; set; } = new Random();
		public TimeSpan TimeToRun { get; set; }

		public ClientEmulator(DedicatedThreadPool threadPool, Emulator emulator, TimeSpan timeToRun, string name, int clientId, IPEndPoint endPoint, int ranMin = 150, int ranMax = 450, int chunkRadius = 8)
		{
			_threadPool = threadPool;
			Emulator = emulator;
			TimeToRun = timeToRun;
			Name = name;
			ClientId = clientId;
			EndPoint = endPoint;
			RanMin = ranMin;
			RanMax = ranMax;
			ChunkRadius = chunkRadius;
		}

		public void EmulateClient()
		{
			try
			{
				int threads;
				int iothreads;
				ThreadPool.GetAvailableThreads(out threads, out iothreads);

				Console.WriteLine("Client {0} connecting... Worker: {1}, IOC: {2}", Name, threads, iothreads);

				var client = new NodeClient(EndPoint, Name, _threadPool)
				{
					ChunkRadius = ChunkRadius,
					ClientId = ClientId
				};

				client.StartClient();

				Stopwatch watch = new Stopwatch();
				watch.Start();

				//Thread.Sleep(3000);
				Console.WriteLine($"{watch.ElapsedMilliseconds} Client started, running... {client.IsRunning}, {Emulator.Running}");

				while (client.IsRunning && Emulator.Running && watch.Elapsed < TimeToRun)
				{
					float y = Random.Next(7, 10) + /*24*/ 55;
					float length = Random.Next(5, 20);

					double angle = 0.0;
					const double angleStepsize = 0.05;
					float heightStepsize = (float) (Random.NextDouble()/5);

					while (angle < 2*Math.PI && Emulator.Running && client.IsRunning)
					{
						float x = (float) (length*Math.Cos(angle));
						float z = (float) (length*Math.Sin(angle));
						y += heightStepsize;

						x += client.SpawnX;
						z += client.SpawnZ;

						client.CurrentLocation = new PlayerLocation(x, y, z);
						_threadPool.QueueUserWorkItem(() => { client.SendMcpeMovePlayer(); });

						Thread.Sleep(Random.Next(RanMin, RanMax));
						angle += angleStepsize;
					}
				}

				if (client.IsRunning)
				{
					client.SendDisconnectionNotification();
				}

				client.StopClient();
				Console.WriteLine($"{watch.ElapsedMilliseconds} Client stopped. {client.IsRunning}, {Emulator.Running}");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}
}