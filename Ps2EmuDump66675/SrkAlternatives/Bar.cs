#define read_mdlx_from_ram_feature
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace SrkAlternatives
{
	public class Bar : BitConverter
	{
		public const long RAM_PARTY_POINTER_PLAYER = 0x00341708;
		public const long RAM_PARTY_POINTER_PARTNER1 = 0x0034170C;
		public const long RAM_PARTY_POINTER_PARTNER2 = 0x00341710;
		public const long RAM_PARTY_POINTER_LOCK_ON_TARGET = 0x01C5FFF4;
		public const long RAM_MAP_POINTER = 0x00348D00;

		public new void Dispose()
		{
			if (this.Files.Count == 0)
			Array.Clear(this.Data, 0, this.Data.Length);
			else
			{

				for (int i = 0; i < this.Files.Count; i++)
					this.Files[i].Dispose();
			}
			this.Files.Clear();
			this.Data = null;
			base.Dispose();
		}

		public long AbsolutePosition;
		public ushort Type;
		public string Name;
		public int Length;
		public List<Bar> Files;
		static byte[] buffer = new byte[4];

		public Bar(Stream stream, ushort type, string name, int length)
		{
			this.Files = new List<Bar>(0);
			this.AbsolutePosition = stream.Position;
			this.Type = type;
			this.Name = name;
			this.Length = length;

			int child_count = 0;

			stream.Read(buffer, 0, 3);

			if (Encoding.ASCII.GetString(buffer).Contains("BAR"))
			{
				stream.Position++;
				stream.Read(buffer, 0, 4);
				child_count = global::System.BitConverter.ToInt32(buffer,0);
			}

			if (child_count > 0)
			{
				for (int i = 0; i < child_count; i++)
				{
					stream.Position = this.AbsolutePosition + 0x10 + i * 0x10;
					stream.Read(buffer, 0, 2);
					ushort type_ = global::System.BitConverter.ToUInt16(buffer, 0); stream.Position += 2;
					stream.Read(buffer, 0, 4);
					string name_ = Encoding.ASCII.GetString(buffer).TrimEnd('\x0');
					stream.Read(buffer, 0, 4);
					int offset_ = global::System.BitConverter.ToInt32(buffer, 0);
					stream.Read(buffer, 0, 4);
					int length_ = global::System.BitConverter.ToInt32(buffer, 0);

					if (length_ == 0)
						continue;

					if (stream is FileStream || stream is MemoryStream)
						stream.Position = this.AbsolutePosition + offset_;
					else
						stream.Position = offset_;

					bool contains = false;

					for (int f=0;f< this.Files.Count;f++)
					{
						if (this.Files[f].AbsolutePosition == stream.Position)
						{
							contains = true;
							this.Files.Add(this.Files[f]);
							break;
						}
					}
					if (contains == false)
					{
						Bar child = new Bar(stream, type_, name_, length_);
						this.Files.Add(child);
					}
				}
			}
			else
			{
				int alignedLength = this.Length;
				while (alignedLength % 16 > 0) alignedLength++;
				if (this.Type == 4)
					alignedLength += 0x30;

				stream.Position = this.AbsolutePosition;
				this.Data = new byte[alignedLength];
				stream.Read(this.Data, 0, this.Length);

				if (!(stream is FileStream) && !(stream is MemoryStream))
				switch (this.Type)
				{
					case 9:
						Array.Copy(new byte[0x30], 0, this.Data, 0, 0x30);
						break;
					case 4:
						Array.Copy(new byte[0x30], 0, this.Data, 0, 0x30);
						if (global::System.BitConverter.ToInt16(this.Data, 0x90) == 3)
						{
							int bonesCounts = global::System.BitConverter.ToInt16(this.Data, 0xA0);
							int bonesOff = +0x90 + global::System.BitConverter.ToInt32(this.Data, 0xA4);
							for (int i = 0; i < bonesCounts; i++)
							{
								short boneIndex = global::System.BitConverter.ToInt16(this.Data, bonesOff + i * 0x40);
								Array.Copy(global::System.BitConverter.GetBytes((int)boneIndex), 0, this.Data, bonesOff + i * 0x40, 4);

								short parentIndex = global::System.BitConverter.ToInt16(this.Data, bonesOff + 0x04 + i * 0x40);
								Array.Copy(global::System.BitConverter.GetBytes((int)parentIndex), 0, this.Data, bonesOff + 0x04 + i * 0x40, 4);
							}
						}
						break;
					case 7:
						int tim_header_6s = global::System.BitConverter.ToInt32(this.Data, 0x14);
						int tim_header_6s_count = global::System.BitConverter.ToInt32(this.Data, 0x08) + 1;
						for (int i = 0; i < tim_header_6s_count; i++)
						{
							int address = global::System.BitConverter.ToInt32(this.Data, tim_header_6s + i * 0x90 + 0x74);
							address -= (int)this.AbsolutePosition;
							Array.Copy(global::System.BitConverter.GetBytes(address), 0, this.Data, tim_header_6s + i * 0x90 + 0x74, 4);
						}
						break;
				}
				return;
			}
		}

		public void Save(string filename)
		{
			FileStream fs = new FileStream(filename, FileMode.Create);
			BinaryWriter bw = new BinaryWriter(fs);
			bw.Write(Encoding.ASCII.GetBytes("BAR\x1"));
			bw.Write(this.Files.Count);
			bw.Write(new byte[8]);


			for (int i = 0; i < this.Files.Count; i++)
			{
				bw.Write((int)this.Files[i].Type);
				bw.Write(Encoding.ASCII.GetBytes(this.Files[i].Name));
				if (this.Files[i].Name.Length < 4)
				{
					bw.Write(new byte[4 - this.Files[i].Name.Length]);
				}
				bw.Write(0);
				bw.Write(this.Files[i].Length);
			}
			for (int i = 0; i < this.Files.Count; i++)
			{
				fs.Position = 0x18 + i * 0x10;
				bw.Write((int)fs.Length);
				fs.Position = fs.Length;
				bw.Write(this.Files[i].GetData());
			}
			bw.Close();
		}

		public byte[] GetData()
		{
			if (this.Files.Count == 0)
				return this.Data;

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);
			bw.Write(Encoding.ASCII.GetBytes("BAR\x1"));
			bw.Write(this.Files.Count);
			bw.Write(new byte[8]);


			for (int i = 0; i < this.Files.Count; i++)
			{
				bw.Write((int)this.Files[i].Type);
				bw.Write(Encoding.ASCII.GetBytes(this.Files[i].Name));
				if (this.Files[i].Name.Length < 4)
				{
					bw.Write(new byte[4 - this.Files[i].Name.Length]);
				}
				bw.Write(0);
				bw.Write(this.Files[i].Length);
			}
			for (int i = 0; i < this.Files.Count; i++)
			{
				ms.Position = 0x18 + i * 0x10;
				bw.Write((int)ms.Length);
				ms.Position = ms.Length;
				bw.Write(this.Files[i].GetData());
			}
			byte[] output = new byte[ms.Length];
			ms.Position = 0;
			ms.Read(output,0,output.Length);
			bw.Close();
			return output;
		}

		public override string ToString()
		{
			return this.Name + " (0x" + this.AbsolutePosition.ToString("X8") + ") size " + this.Length.ToString("X8");
		}
	}
}
