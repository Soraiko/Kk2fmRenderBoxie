/*
 * /!\ DISCLAIMER /!\:
 * Designed after and in dear memory of the BAR file extension used 
 * for the resource files of the PS2 game Kingdom Hearts 2 Final Mix.
 * I do not own the concept of the BAR filetype.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace BuilderMdlx
{
	public class BAR
	{
		const string tag = "BAR";
		public BarHeader Header;
		public struct BarHeader
		{
			public string Tag;
			public byte BarVersion_0x03;
			public int Address_0x08;
			public int Flag_0x0C;
			public int Replace_0x0C;
		}

		public BarEntry Entry;
		public struct BarEntry
		{
			public BAR Prototype;
			public ushort EntryType_0x00;
			public ushort DuplicateFlag_0x02;
			public string Name_0x04;
			public int Offset_0x08;
			public int Size_0x0C;
		}

		private static Dictionary<string, EntryType> _extensions = new Dictionary<string, EntryType>
		{
			[".anb"] = EntryType.Anb,
			[".pax"] = EntryType.Pax,
			[".mset"] = EntryType.Motionset,
			[".imd"] = EntryType.Imgd,
			[".imz"] = EntryType.Imgz,
			[".seb"] = EntryType.Seb,
			[".wd"] = EntryType.Wd,
			[".vag"] = EntryType.Vag
		};

		/*
		 * EntryType Dictionnary and enum from OpenKh project:
		 * https://github.com/OpenKH/OpenKh/blob/master/OpenKh.Kh2/Bar.cs
		 * 02:49 07/08/2022
		 */
		private static Dictionary<EntryType, int> _alignments = new Dictionary<EntryType, int>
		{
			[EntryType.Model] = 0x10,
			[EntryType.DrawOctalTree] = 0x04,
			[EntryType.CollisionOctalTree] = 0x10,
			[EntryType.ModelTexture] = 0x80,
			[EntryType.Motion] = 0x10,
			[EntryType.Tim2] = 0x40,
			[EntryType.CameraOctalTree] = 0x10,
			[EntryType.AreaDataSpawn] = 0x04,
			[EntryType.AreaDataScript] = 0x04,
			[EntryType.FogColor] = 0x04,
			[EntryType.ColorOctalTree] = 0x10,
			[EntryType.Anb] = 0x10,
			[EntryType.Pax] = 0x10,
			[EntryType.MapCollision2] = 0x10,
			[EntryType.Motionset] = 0x10,
			[EntryType.BgObjPlacement] = 0x04,
			[EntryType.Event] = 0x04,
			[EntryType.ModelCollision] = 0x04,
			[EntryType.Imgd] = 0x10,
			[EntryType.Seqd] = 0x10,
			[EntryType.Layout] = 0x10,
			[EntryType.Imgz] = 0x10,
			[EntryType.AnimationMap] = 0x10,
			[EntryType.Seb] = 0x40,
			[EntryType.Wd] = 0x40,
			[EntryType.IopVoice] = 0x40,
			[EntryType.RawBitmap] = 0x80,
			[EntryType.MemoryCard] = 0x40,
			[EntryType.WrappedCollisionData] = 0x10,
			[EntryType.Unknown39] = 0x10,
			[EntryType.Minigame] = 0x10,
			[EntryType.Progress] = 0x10,
			[EntryType.BarUnknown] = 0x10,
			[EntryType.Vag] = 0x10
		};

		public enum EntryType
		{
			Dummy =							0,
			Binary =						1,
			List =							2,
			Bdx =							3,
			Model =							4,
			DrawOctalTree =					5,
			CollisionOctalTree =			6,
			ModelTexture =					7,
			Dpx =							8,
			Motion =						9,
			Tim2 =							10,
			CameraOctalTree =				11,
			AreaDataSpawn =					12,
			AreaDataScript =				13,
			FogColor =						14,
			ColorOctalTree =				15,
			MotionTriggers =				16,
			Anb =							17,
			Pax =							18,
			MapCollision2 =					19,
			Motionset =						20,
			BgObjPlacement =				21,
			Event =							22,
			ModelCollision =				23,
			Imgd =							24,
			Seqd =							25,
			Unknown26 =						26,
			Unknown27 =						27,
			Layout =						28,
			Imgz =							29,
			AnimationMap =					30,
			Seb =							31,
			Wd =							32,
			Unknown33 =						33,
			IopVoice =						34,
			Unknown35 =						35,
			RawBitmap =						36,
			MemoryCard =					37,
			WrappedCollisionData =			38,
			Unknown39 =						39,
			Unknown40 =						40,
			Unknown41 =						41,
			Minigame =						42,
			JimiData =						43,
			Progress =						44,
			Synthesis =						45,
			BarUnknown =					46,
			Vibration =						47,
			Vag =							48
		}

		public const long RAM_PARTY_POINTER_PLAYER = 0x00341708;
		public const long RAM_PARTY_POINTER_PARTNER1 = 0x0034170C;
		public const long RAM_PARTY_POINTER_PARTNER2 = 0x00341710;
		public const long RAM_PARTY_POINTER_LOCK_ON_TARGET = 0x01C5FFF4;
		public const long RAM_MAP_POINTER = 0x00348D00;

		List<BAR> files;

		public System.Collections.ObjectModel.ReadOnlyCollection<BAR> Files
		{
			get
			{
				return this.files.AsReadOnly();
			}
		}

		public BAR()
		{
			this.files = new List<BAR>(0);
		}

		public BAR(byte[] bytes) : this(new MemoryStream(bytes)) {}

		public BAR(Stream stream) : this()
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			long streamPosition = stream.Position;

			if (streamPosition + 4 <= stream.Length)
			{
				this.Header.Tag = Encoding.ASCII.GetString(binaryReader.ReadBytes(3));
				bool barTagOk = String.Equals(this.Header.Tag, tag);
				this.Header.BarVersion_0x03 = binaryReader.ReadByte();

				if (barTagOk)
				{
					int filesCount = binaryReader.ReadInt32();
					this.Header.Address_0x08 = binaryReader.ReadInt32();
					int flagrep = binaryReader.ReadInt32();
					this.Header.Flag_0x0C = (int)((flagrep & 0xE0000000) >> 29);
					this.Header.Replace_0x0C = (int)(flagrep & 0x1FFFFFFF);

					for (int f = 0; f < filesCount; f++)
					{
						stream.Position = 0x10 + streamPosition + 0x10 * f;

						BarEntry entry = new BarEntry();
						entry.EntryType_0x00 = binaryReader.ReadUInt16();
						entry.DuplicateFlag_0x02 = binaryReader.ReadUInt16();
						entry.Name_0x04 = Encoding.ASCII.GetString(binaryReader.ReadBytes(4));
						entry.Offset_0x08 = binaryReader.ReadInt32();
						entry.Size_0x0C = binaryReader.ReadInt32();

						stream.Position = streamPosition + entry.Offset_0x08;
						BAR subFile = null;

						if (entry.DuplicateFlag_0x02 == 0)
							subFile = new BAR(stream);
						else
							subFile = new BAR();

						subFile.Entry = entry;

						if (subFile.Header.Tag != tag && entry.Size_0x0C > 0)
						{
							stream.Position = streamPosition + entry.Offset_0x08;
							subFile.data = binaryReader.ReadBytes(entry.Size_0x0C);
						}
						this.files.Add(subFile);
					}
					/* Determine prototypes */
					for (int f=0;f< this.files.Count;f++)
					{
						if (this.files[f].Entry.DuplicateFlag_0x02 != 0)
						{
							for (int e=f-1;e>=0;e--)
							{
								if (this.files[e].Entry.DuplicateFlag_0x02 == 0 
									&& this.files[e].Entry.Offset_0x08 == this.files[f].Entry.Offset_0x08)
								{
									this.files[f].Entry.Prototype = this.files[e];
									break;
								}
							}
						}
					}
				}
			}
		}

		public byte[] Data
		{
			get
			{
				return this.data;
			}
			set
			{
				this.Entry.Size_0x0C = value.Length;
				this.data = value;
			}
		}

		byte[] data;

		public byte[] GetData()
		{
			return GetData(0);
		}

		public byte[] GetData(int alignByte)
		{
			if (this.files.Count == 0)
				return this.data;

			MemoryStream ms = new MemoryStream();
			BinaryWriter bw = new BinaryWriter(ms);

			bw.Write(Encoding.ASCII.GetBytes(tag));
			bw.Write(this.Header.BarVersion_0x03);
			bw.Write(this.files.Count);
			bw.Write(this.Header.Address_0x08);
			bw.Write((this.Header.Flag_0x0C << 29) | this.Header.Replace_0x0C);


			for (int f = 0; f < this.files.Count; f++)
			{
				bw.Write(this.files[f].Entry.EntryType_0x00);
				bw.Write(this.files[f].Entry.DuplicateFlag_0x02);
				bw.Write(Encoding.ASCII.GetBytes(this.files[f].Entry.Name_0x04));
				this.files[f].Entry.Offset_0x08 = -1;
				bw.Write(this.files[f].Entry.Offset_0x08); /* Offsets will be recalculated. */
				bw.Write(this.files[f].Entry.Size_0x0C);
			}

			for (int i = 0; i < this.files.Count; i++)
			{
				long offsetToWrite = ms.Length;
				long additionnalBytes = 0;

				int alignement = 0x10;
				if (_alignments.ContainsKey((EntryType)this.files[i].Entry.EntryType_0x00))
					alignement = _alignments[(EntryType)this.files[i].Entry.EntryType_0x00];

				long align = ms.Length % alignement;
				if (align > 0)
					additionnalBytes = alignement - align;

				offsetToWrite += additionnalBytes;

				if (this.files[i].Entry.DuplicateFlag_0x02 != 0 && this.files[i].Entry.Prototype != null)
					offsetToWrite = this.files[i].Entry.Prototype.Entry.Offset_0x08;

				this.files[i].Entry.Offset_0x08 = (int)offsetToWrite;

				ms.Position = 0x18 + i * 0x10;
				bw.Write(this.files[i].Entry.Offset_0x08);
				ms.Position = ms.Length;

				if (this.files[i].Entry.DuplicateFlag_0x02 == 0 && (this.files[i].Entry.Size_0x0C > 0 || this.files[i].DynamicEntrySize))
				{
					if (additionnalBytes > 0)
						bw.Write(new byte[additionnalBytes]);

					var data = this.files[i].GetData();
					bw.Write(data);

					if (this.files[i].DynamicEntrySize)
					{
						ms.Position = 0x1C + i * 0x10;
						bw.Write(data.Length);

						ms.Position = ms.Length;
					}
				}
			}
			if (alignByte > 0)
			{
				long align = ms.Length % alignByte;
				if (align > 0)
					bw.Write(new byte[alignByte - align]);
			}
			
			byte[] output = new byte[ms.Length];
			ms.Position = 0;
			ms.Read(output, 0, output.Length);
			bw.Close();
			return output;
		}

		bool dynamicEntrySize;

		public bool DynamicEntrySize
		{
			set {this.dynamicEntrySize = value;}
			get {return this.dynamicEntrySize && this.files.Count > 0;}
		}

		public void Add(object file, string name)
		{
			this.Insert(this.files.Count, file, name);
		}

		public void Insert(int index, object file, string name)
		{
			BAR objectAsBar = file as BAR;
			if (objectAsBar == null)
			{
				objectAsBar = new BAR();
				if (file is byte[])
					objectAsBar.Data = file as byte[];
			}
			else
				objectAsBar.DynamicEntrySize = true;

			objectAsBar.Entry.Name_0x04 = name;

			this.files.Insert(index, objectAsBar);
		}

		public void Remove(BAR file)
		{
			int indexOf = this.files.IndexOf(file);
			if (indexOf>-1)
			{
				this.RemoveAt(indexOf);
			}
		}

		public void RemoveAt(int index)
		{
			BAR concerned = this.files[index];
			List<BAR> duplicates = new List<BAR>(0);

			for (int i=0;i<this.files.Count;i++)
			{
				if (this.files[i].Entry.Prototype == concerned)
				{
					duplicates.Add(this.files[i]);
				}
			}
			if (duplicates.Count > 0)
			{
				Console.WriteLine("The file you are trying to remove (id: "+ concerned.Entry.Name_0x04+ ") has "+ duplicates.Count + " duplicates. Remove them all before to pursue ? (y/n)");
				if (Console.ReadKey().Key == ConsoleKey.Y)
				{
					while (duplicates.Count>0)
					{
						this.files.Remove(duplicates[0]);
						duplicates.RemoveAt(0);
					}
				}
				else
				{
					Console.WriteLine("\nYou chose not to remove the duplicates.");
					System.Threading.Thread.Sleep(1000);
					Console.WriteLine("Which other entry should they duplicate instead ?");
					do
					{
						Console.WriteLine("Enter an index between 0 and " + (this.files.Count - 1) + " for a file that is NOT a duplicate entry, NOR the entry you are about to remove:");
						string input = Console.ReadLine();
						int inputInt = -1;
						if (int.TryParse(input, out inputInt))
						{
							if (inputInt >= 0 && inputInt < this.files.Count && this.files[inputInt].Entry.DuplicateFlag_0x02 == 0 && this.files[inputInt] != concerned)
							{
								for (int i = 0; i < duplicates.Count; i++)
								{
									duplicates[i].Entry.Prototype = this.files[inputInt];
								}
								break;
							}
						}
					}
					while (true);
				}
			}
			this.files.Remove(concerned);
		}

		public void Save(string filename)
		{
			int alignByte = 0;
			string extension = Path.GetExtension(filename);

			if (_extensions.ContainsKey(extension))
				alignByte = _alignments[_extensions[extension]];

			File.WriteAllBytes(filename, this.GetData(alignByte));
		}
	}
}
