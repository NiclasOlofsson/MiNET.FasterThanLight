using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiNET.Client;
using MiNET.Utils;

namespace MiNET.Ping
{
	class Program
	{
		static void Main(string[] args)
		{

			for (int i = 0; i < 10000; i++)
			{
				var endpoing = new IPEndPoint(Dns.GetHostEntry("sw.lbsg.net").AddressList[0], 19132);

				var client = new MiNetClient(endpoing, "Name", new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount)));
				client.StartClient();

				client.SendUnconnectedPing();
				Thread.Sleep(4000);
				client.StopClient();
			}

			Console.WriteLine("Pinger started.");

			Console.ReadLine();
		}
	}
}
