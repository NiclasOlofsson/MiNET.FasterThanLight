using log4net;
using MiNET.Net;

namespace MiNET.Ftl.Core.Proxy
{
	public abstract class BaseMessageHandler : IMcpeMessageHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof (BaseMessageHandler));

		public abstract void WritePackage(Package message);

		public abstract void WriteBytes(byte[] message);

		public void Disconnect(string reason, bool sendDisconnect = true)
		{
			//Log.Warn("Got disconnect in proxy: " + reason);

			McpeDisconnect message = McpeDisconnect.CreateObject();
			message.message = reason;
			WriteBytes(message.Encode());
			message.PutPool();
		}

		public void HandleMcpeLogin(McpeLogin message)
		{
			WritePackage(message);
		}

		public void HandleMcpeClientMagic(McpeClientMagic message)
		{
			if (message == null)
			{
				message = new McpeClientMagic();
				var bytes = message.Encode();
				WriteBytes(bytes);
			}
			else
			{
				WritePackage(message);
			}
		}

		public void HandleMcpeText(McpeText message)
		{
			WritePackage(message);
		}

		public void HandleMcpeMovePlayer(McpeMovePlayer message)
		{
			WritePackage(message);
		}

		public void HandleMcpeRemoveBlock(McpeRemoveBlock message)
		{
			WritePackage(message);
		}

		public void HandleMcpeEntityEvent(McpeEntityEvent message)
		{
			WritePackage(message);
		}

		public void HandleMcpeMobEquipment(McpeMobEquipment message)
		{
			WritePackage(message);
		}

		public void HandleMcpeMobArmorEquipment(McpeMobArmorEquipment message)
		{
			WritePackage(message);
		}

		public void HandleMcpeInteract(McpeInteract message)
		{
			WritePackage(message);
		}

		public void HandleMcpeUseItem(McpeUseItem message)
		{
			WritePackage(message);
		}

		public void HandleMcpePlayerAction(McpePlayerAction message)
		{
			WritePackage(message);
		}

		public void HandleMcpeAnimate(McpeAnimate message)
		{
			WritePackage(message);
		}

		public void HandleMcpeRespawn(McpeRespawn message)
		{
			WritePackage(message);
		}

		public void HandleMcpeDropItem(McpeDropItem message)
		{
			WritePackage(message);
		}

		public void HandleMcpeContainerClose(McpeContainerClose message)
		{
			WritePackage(message);
		}

		public void HandleMcpeContainerSetSlot(McpeContainerSetSlot message)
		{
			WritePackage(message);
		}

		public void HandleMcpeCraftingEvent(McpeCraftingEvent message)
		{
			WritePackage(message);
		}

		public void HandleMcpeBlockEntityData(McpeBlockEntityData message)
		{
			WritePackage(message);
		}

		public void HandleMcpePlayerInput(McpePlayerInput message)
		{
			WritePackage(message);
		}

		public void HandleMcpeMapInfoRequest(McpeMapInfoRequest message)
		{
			WritePackage(message);
		}

		public void HandleMcpeRequestChunkRadius(McpeRequestChunkRadius message)
		{
			WritePackage(message);
		}

		public void HandleMcpeItemFramDropItem(McpeItemFramDropItem message)
		{
			WritePackage(message);
		}
	}
}