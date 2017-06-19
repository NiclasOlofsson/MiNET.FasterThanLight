using MiNET.Net;

namespace MiNET.Ftl.Core.Net
{
	public class FtlPackageFactory
	{
		public static Package CreatePackage(byte messageId, byte[] buffer)
		{
			Package package = null;
			switch (messageId)
			{
				case 0x01:
					package = FtlCreatePlayer.CreateObject();
					//package.Timer.Start();
					package.Decode(buffer);
					return package;
			}

			return null;
		}
	}


	public partial class FtlCreatePlayer : Package<FtlCreatePlayer>
	{
		public string username; // = null;
		public UUID clientuuid; // = null;
		public string serverAddress; // = null;
		public long clientId; // = null;
		public Skin skin; // = null;

		public FtlCreatePlayer()
		{
			Id = 0x01;
		}

		protected override void EncodePackage()
		{
			base.EncodePackage();

			BeforeEncode();

			Write(username);
			Write(clientuuid);
			Write(serverAddress);
			Write(clientId);
			Write(skin);

			AfterEncode();
		}

		partial void BeforeEncode();
		partial void AfterEncode();

		protected override void DecodePackage()
		{
			base.DecodePackage();

			BeforeDecode();

			username = ReadString();
			clientuuid = ReadUUID();
			serverAddress = ReadString();
			clientId = ReadLong();
			skin = ReadSkin();

			AfterDecode();
		}

		partial void BeforeDecode();
		partial void AfterDecode();
	}
}