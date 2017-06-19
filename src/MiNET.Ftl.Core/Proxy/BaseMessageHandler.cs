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

using log4net;
using MiNET.Net;

namespace MiNET.Ftl.Core.Proxy
{
	public abstract class BaseMessageHandler : IMcpeMessageHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(typeof(BaseMessageHandler));

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

		public void HandleMcpeClientToServerHandshake(McpeClientToServerHandshake message)
		{
			if (message == null)
			{
				message = new McpeClientToServerHandshake();
				var bytes = message.Encode();
				WriteBytes(bytes);
			}
			else
			{
				WritePackage(message);
			}
		}

		public void HandleMcpeResourcePackClientResponse(McpeResourcePackClientResponse message)
		{
			WritePackage(message);
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

		public void HandleMcpeLevelSoundEvent(McpeLevelSoundEvent message)
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

		public void HandleMcpeBlockPickRequest(McpeBlockPickRequest message)
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

		public void HandleMcpeEntityFall(McpeEntityFall message)
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

		public void HandleMcpeAdventureSettings(McpeAdventureSettings message)
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

		public void HandleMcpeItemFrameDropItem(McpeItemFrameDropItem message)
		{
			WritePackage(message);
		}

		public void HandleMcpeCommandStep(McpeCommandStep message)
		{
			WritePackage(message);
		}

		public void HandleMcpeCommandBlockUpdate(McpeCommandBlockUpdate message)
		{
		}

		public void HandleMcpeResourcePackChunkRequest(McpeResourcePackChunkRequest message)
		{
			WritePackage(message);
		}
	}
}