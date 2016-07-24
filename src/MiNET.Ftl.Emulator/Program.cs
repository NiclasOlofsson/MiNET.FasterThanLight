using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using log4net.Config;
using MiNET.Ftl.Core.Proxy;
using MiNET.Utils;

// Configure log4net using the .config file

[assembly: XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called TestApp.exe.config in the application base
// directory (i.e. the directory containing TestApp.exe)
// The config file will be watched for changes.

namespace MiNET.Ftl.Emulator
{
	public class Emulator
	{
		private const int TimeBetweenSpawns = 1000;
		private static readonly TimeSpan DurationOfConnection = TimeSpan.FromMinutes(1);
		private const int NumberOfBots = 2;
		private const int RanSleepMin = 150;
		private const int RanSleepMax = 450;
		private const int RequestChunkRadius = 5;

		private static bool _running = true;

		public bool Running
		{
			get { return _running; }
			set { _running = value; }
		}

		private static void Main(string[] args)
		{
			XmlConfigurator.Configure();

			try
			{
				AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
				Console.WriteLine("Press <Enter> to start emulation...");
				Console.ReadLine();

				int threads;
				int iothreads;
				ThreadPool.GetMinThreads(out threads, out iothreads);

				DedicatedThreadPool threadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(threads));
				ProxyMessageHandler.FastThreadPool = threadPool;

				ThreadPool.SetMinThreads(32000, 4000);
				ThreadPool.SetMaxThreads(32000, 4000);

				Emulator emulator = new Emulator {Running = true};
				Stopwatch watch = new Stopwatch();
				watch.Start();

				long start = DateTime.UtcNow.Ticks;

				//IPEndPoint endPoint = new IPEndPoint(Dns.GetHostEntry("yodamine.com").AddressList[0], 51234);
				IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 51234);

				for (int j = 0; j < NumberOfBots; j++)
				{
					string playerName = $"TheGrey{j + 1:D3}";

					ClientEmulator client = new ClientEmulator(threadPool, emulator, DurationOfConnection,
						playerName, (int) (DateTime.UtcNow.Ticks - start), endPoint,
						RanSleepMin, RanSleepMax, RequestChunkRadius);

					new Thread(o => { client.EmulateClient(); }) {IsBackground = true}.Start();
					//ThreadPool.QueueUserWorkItem(delegate { client.EmulateClient(); });

					Thread.Sleep(TimeBetweenSpawns);
				}

				Console.WriteLine("Press <enter> to stop all clients.");
				Console.ReadLine();
				emulator.Running = false;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			Console.WriteLine("Emulation complete. Press <enter> to exit.");
			Console.ReadLine();
		}

		private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
		{
			Console.WriteLine("ERROR." + unhandledExceptionEventArgs.ExceptionObject);
		}
	}
}