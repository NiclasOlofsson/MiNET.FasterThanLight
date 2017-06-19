using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MiNET.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MiNET.Ping
{
	public class WebSocketServer
	{
		public async void Start(string[] prefixes)
		{
			if (!HttpListener.IsSupported)
			{
				Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
				return;
			}

			// URI prefixes are required,
			// for example "http://contoso.com:8080/index/".
			if (prefixes == null || prefixes.Length == 0)
				throw new ArgumentException("prefixes");

			// Create a listener.
			HttpListener listener = new HttpListener();
			// Add the prefixes.
			foreach (string s in prefixes)
			{
				listener.Prefixes.Add(s);
			}
			listener.Start();
			Console.WriteLine("Listening...");
			while (true)
			{
				var context = await listener.GetContextAsync();
				if (context.Request.IsWebSocketRequest)
				{
					Console.WriteLine(context.Request.ContentEncoding);

					ProcessRequests(context);
				}
				else
				{
					Console.WriteLine("Sending 400 close");
					context.Response.StatusCode = 400;
					context.Response.Close();
				}
			}

			//// Note: The GetContext method blocks while waiting for a request. 
			//HttpListenerContext context = listener.GetContext();
			//Console.WriteLine("Got connection...");
			//HttpListenerRequest request = context.Request;
			//// Obtain a response object.
			//HttpListenerResponse response = context.Response;
			//// Construct a response.
			//string responseString = "<HTML><BODY> Hello world!</BODY></HTML>";
			//byte[] buffer = Encoding.UTF8.GetBytes(responseString);
			//// Get a response stream and write the response to it.
			//response.ContentLength64 = buffer.Length;
			//Stream output = response.OutputStream;
			//output.Write(buffer, 0, buffer.Length);
			//// You must close the output stream.
			//output.Close();
			//listener.Stop();
		}

		public class Message
		{
			public MessageHeader Header { get; set; }
			public MessageBody Body { get; set; }
		}

		public class MessageHeader
		{
			public int Version { get; set; }
			public string RequestId { get; set; }
			public string MessagePurpose { get; set; }
			public string MessageType { get; set; }
		}

		public class MessageBody
		{
			public string Body { get; set; }
			public string CommandName { get; set; }
			public string Origin { get; set; }
			public string Command { get; set; }
			public string EventName { get; set; }
		}


		public static void PrintKeysAndValues(NameValueCollection myCol)
		{
			Console.WriteLine("   KEY        VALUE");
			foreach (String s in myCol.AllKeys)
				Console.WriteLine("   {0,-10} {1}", s, myCol[s]);
			Console.WriteLine();
		}

		private async Task Receive(WebSocket socket)
		{
			try
			{
				byte[] buffer = new byte[1014];
				while (socket.State == WebSocketState.Open)
				{
					WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

					Console.WriteLine($"Receive content, len={result.Count}, MesageType={result.MessageType}");

					if (result.EndOfMessage)
					{
						Console.WriteLine($"Receive ALL content, len={result.Count}");
					}

					if (result.MessageType == WebSocketMessageType.Close)
					{
						Console.WriteLine($"Receive close {result.CloseStatus}");
						Console.WriteLine($"Receive close {result.CloseStatusDescription}");
						await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
					}
					else
					{

						var jsonSerializerSettings = new JsonSerializerSettings
						{
							PreserveReferencesHandling = PreserveReferencesHandling.None,
							Formatting = Formatting.Indented,
						};

						var commandJson = JsonConvert.DeserializeObject<dynamic>(Encoding.UTF8.GetString(buffer.Take(result.Count).ToArray()));
						Console.WriteLine($"Receive content:\n{commandJson}");
						//Console.WriteLine($"Receive content: {Package.HexDump(buffer)}");

						//await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Binary, result.EndOfMessage, CancellationToken.None);
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

		}

		private async void ProcessRequests(HttpListenerContext context)
		{
			try
			{
				WebSocketContext webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null);
				PrintKeysAndValues(webSocketContext.Headers);

				string ipAddres = context.Request.RemoteEndPoint.Address.ToString();
				Console.WriteLine($"Got WebSocket connection from {ipAddres}");

				WebSocket socket = webSocketContext.WebSocket;
				Console.WriteLine($"SubProtocol={socket.SubProtocol}");

				var receive = Receive(socket);

				await Task.Delay(1000);


				var settings = new JsonSerializerSettings();
				settings.NullValueHandling = NullValueHandling.Ignore;
				settings.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
				settings.MissingMemberHandling = MissingMemberHandling.Error;
				settings.Formatting = Formatting.Indented;
				settings.ContractResolver = new CamelCasePropertyNamesContractResolver();

				var message = new Message()
				{
					Header = new MessageHeader()
					{
						//RequestId = "External",
						//RequestId = "3",
						//RequestId = "automation",
						RequestId = Guid.NewGuid().ToString(),
						Version = 1,
						MessagePurpose = "subscribe",
						//MessagePurpose = "commandRequest"
						MessageType = "commandRequest"
					},
					//Body = "/kill @e"
					Body = new MessageBody()
					{
						Origin= "External",
						//CommandName = "helpasdf",
						//Command = "helpasdf",
						EventName = "BlockPlaced",
						//Body = ""
					}
				};

				string content = JsonConvert.SerializeObject(message, settings);
				//string content = "{\"body\":{\"input\":{\"dimension\":\"overworld\",\"chunkX\":13,\"chunkZ\":4,\"height\":128},\"origin\":{\"type\":\"player\"},\"name\":\"getchunkdata\",\"version\":1,\"overload\":\"default\"},\"header\":{\"requestId\":\"c8c4d791-1e88-49a4-bbef-204c5462982d\",\"messagePurpose\":\"commandRequest\",\"version\":1,\"messageType\":\"commandRequest\"}}";

				Console.WriteLine(content);

				byte[] buffer = Encoding.UTF8.GetBytes(content);

				Console.WriteLine($"Sending content");
				await socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);

				await receive;

			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			var server = new WebSocketServer();
			server.Start(new string[] {"http://+:8181/"});

			Console.WriteLine("HTTP server started.");
			Console.ReadLine();
		}

		//static void Main(string[] args)
		//{
		//	TcpListener listener = new TcpListener(IPAddress.Any, 8181);
		//	listener.Start();

		//	while (true)
		//	{
		//		Console.WriteLine("Waiting for accept");
		//		Socket socket = listener.AcceptSocket();
		//		Console.WriteLine("Accepted socket");
		//		var buffer = new byte[short.MaxValue];
		//		int len = socket.Receive(buffer);
		//		var bytes = new byte[len];
		//		Buffer.BlockCopy(buffer, 0, bytes, 0, len);
		//		Console.WriteLine($"Receive: {Package.HexDump(bytes)}");

		//		socket.Send(Encoding.UTF8.GetBytes("/kill @e"));
		//		socket.Close();
		//		Console.WriteLine("Accepted socket");
		//	}
		//}

		//static void Main(string[] args)
		//{
		//	SimpleListenerExample(new string[] { "http://*:80/" });
		//	Console.WriteLine("HTTP server started.");

		//	Console.ReadLine();

		//	return;

		//	for (int i = 0; i < 10000; i++)
		//	{
		//		var endpoing = new IPEndPoint(Dns.GetHostEntry("sw.lbsg.net").AddressList[0], 19132);

		//		var client = new MiNetClient(endpoing, "Name", new DedicatedThreadPool(new DedicatedThreadPoolSettings(Environment.ProcessorCount)));
		//		client.StartClient();

		//		client.SendUnconnectedPing();
		//		Thread.Sleep(4000);
		//		client.StopClient();
		//	}

		//	Console.WriteLine("Pinger started.");

		//	Console.ReadLine();
		//}
	}
}