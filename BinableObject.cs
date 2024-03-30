using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BDxGraphiK
{
	public class BinableObject
	{
		public class BinaryRW
		{
			public Stream BaseStream;
			public BinaryReader BinaryReader;
			public BinaryWriter BinaryWriter;

			public BinaryRW() : this(new MemoryStream())
			{
				
			}

			public BinaryRW(Stream stream)
			{
				this.BaseStream = stream;
				this.BinaryReader = new BinaryReader(this.BaseStream);
				this.BinaryWriter = new BinaryWriter(this.BaseStream);
			}
		}

		public enum ASCII4
		{
			Model = 0x6C646F6D,
			Skeleton = 0x6C656B73,
			Mesh = 0x6873656D,
			Material = 0x6574616D
		}

		public ASCII4 ObjectFlag;
		private protected bool Generated = false;
		public BinaryRW StreamRW;

		public void GenerateBinary()
		{
			this.Generated = true;
		}

		public void BufferBinary()
		{

		}

		public string GetMD5()
		{
			string output = "";
			using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
			{
				byte[] hashBytes = md5.ComputeHash(this.StreamRW.BaseStream);
				for (int i=hashBytes.Length-1;i>=0;i--)
					output += hashBytes[i].ToString("x2");
			}
			return output;
		}
	}
}
