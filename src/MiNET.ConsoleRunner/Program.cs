using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using MiNET.Entities;
using MiNET.Ftl.Core.Node;
using MiNET.Utils;
using MiNET.Worlds;

// Configure log4net using the .config file

[assembly: XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called TestApp.exe.config in the application base
// directory (i.e. the directory containing TestApp.exe)
// The config file will be watched for changes.

namespace MiNET.ConsoleRunner
{
	public class MiNetService
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (MiNetService));

		/// <summary>
		///     The programs entry point.
		/// </summary>
		/// <param name="args">The arguments.</param>
		private static void Main2(string[] args)
		{
			XmlConfigurator.Configure();

			NodeNetworkHandler.FastThreadPool = new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount));

			var server = new MiNetServer();

			int threads;
			int iothreads;
			ThreadPool.GetMinThreads(out threads, out iothreads);
			ThreadPool.SetMinThreads(4000, iothreads);

			var localServerManager = new NodeServerManager(server, 51234);
			server.ServerManager = localServerManager;
			server.ServerRole = ServerRole.Node;
			server.LevelManager = new SpreadLevelManager(60);

			server.StartServer();

			Console.WriteLine("MiNET running...");
			Console.ReadLine();
		}

		private static void Main(string[] args)
		{
			XmlConfigurator.Configure();

			Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

			Console.WriteLine("MiNET starting on " + Environment.ProcessorCount + " concurent threads" );

			var server = new MiNetServer();
			server.ServerRole = ServerRole.Full;
			//server.LevelManager = new SpreadLevelManager(Environment.ProcessorCount * 10);
			server.PlayerFactory = new LobbyStylePlayerFactory();

			//int threads;
			//int iothreads;
			//ThreadPool.GetMinThreads(out threads, out iothreads);
			//ThreadPool.SetMinThreads(4000, iothreads);

			server.StartServer();

			Console.WriteLine("MiNET running...");
			Console.ReadLine();
		}
	}

	public class LobbyStylePlayerFactory : PlayerFactory
	{
		public override Player CreatePlayer(MiNetServer server, IPEndPoint endPoint)
		{
			var player = new Player(server, endPoint);
			player.MaxViewDistance = Config.GetProperty("MaxViewDistance", 22);
			player.MoveRenderDistance = Config.GetProperty("MoveRenderDistance", 1);
			player.HungerManager = new AlwaysFullHungerManager(player);
			player.HealthManager = new NoDamageHealthManager(player);
			OnPlayerCreated(new PlayerEventArgs(player));
			return player;
		}
	}


	public class NoDamageHealthManager : HealthManager
	{
		public NoDamageHealthManager(Entity entity) : base(entity)
		{
		}

		public override void TakeHit(Entity source, int damage = 1, DamageCause cause = DamageCause.Unknown)
		{
			//base.TakeHit(source, 0, cause);
		}

		public override void OnTick()
		{
		}
	}

	//internal class SpreadLevelManager : LevelManager
	//{
	//	private static readonly ILog Log = LogManager.GetLogger(typeof (SpreadLevelManager));

	//	private readonly int _numberOfLevels;

	//	public SpreadLevelManager(int numberOfLevels)
	//	{
	//		Log.Warn($"Creating and caching {numberOfLevels} levels");

	//		//Level template = CreateLevel("Default", null);

	//		_numberOfLevels = numberOfLevels;
	//		Levels = new List<Level>();
	//		Parallel.For(0, numberOfLevels, i =>
	//		{
	//			var name = "Default" + i;
	//			//Levels.Add(CreateLevel(name, template._worldProvider));
	//			Levels.Add(CreateLevel(name, null));
	//			Log.Warn($"Created level {name}");
	//		});

	//		Log.Warn("DONE Creating and caching worlds");
	//	}

	//	public override Level GetLevel(Player player, string name)
	//	{
	//		Random rand = new Random();

	//		return Levels[rand.Next(0, _numberOfLevels)];
	//	}

	//	public virtual Level CreateLevel(string name, IWorldProvider provider)
	//	{
	//		GameMode gameMode = Config.GetProperty("GameMode", GameMode.Survival);
	//		Difficulty difficulty = Config.GetProperty("Difficulty", Difficulty.Peaceful);
	//		int viewDistance = Config.GetProperty("ViewDistance", 11);

	//		IWorldProvider worldProvider = null;
	//		worldProvider = provider ?? new FlatlandWorldProvider();

	//		var level = new Level(name, worldProvider, gameMode, difficulty, viewDistance);
	//		level.Initialize();

	//		OnLevelCreated(new LevelEventArgs(null, level));

	//		return level;
	//	}

	//	public event EventHandler<LevelEventArgs> LevelCreated;

	//	protected virtual void OnLevelCreated(LevelEventArgs e)
	//	{
	//		LevelCreated?.Invoke(this, e);
	//	}
	//}
}