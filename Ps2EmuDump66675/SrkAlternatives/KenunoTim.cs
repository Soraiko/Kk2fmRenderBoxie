﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;

namespace SrkAlternatives
{
	public static class XDebug
	{
		public static void Assert(bool boolean, string message)
		{

		}
		public static void Fail(string message)
		{

		}
		public static void WriteLine(string message)
		{

		}
	}
	public static class XTrace
	{
		public static void Assert(bool test)
		{

		}
	}
	public class KenunoTim
	{
		public static List<STim> GetTextures(byte[] tim2data)
		{
			MemoryStream si = new MemoryStream(tim2data, false);
			BinaryReader br = new BinaryReader(si);

			int v00 = br.ReadInt32();
			if (v00 == 0)
			{
				// TIMf
				si.Position = 0;
				return ParseTex_TIMf(si, br);
			}
			else if (v00 == -1)
			{
				// TIMc .. TIMformat collection
				int v04 = br.ReadInt32();

				List<int> aloff = new List<int>();
				for (int x = 0; x<v04; x++)
					aloff.Add(br.ReadInt32());

				aloff.Add(tim2data.Length);

				List<List<STim>> alres = new List<List<STim>>();

				for (int x = 0; x<v04; x++)
				{
					si.Position = aloff[x];
					byte[] bin = br.ReadBytes(aloff[x + 1] - aloff[x]);

					MemoryStream siF = new MemoryStream(bin, false);
					BinaryReader brF = new BinaryReader(siF);

					alres.Add(ParseTex_TIMf(siF, brF));
				}
				return alres.Count != 0 ? alres[0] : new List<STim>(0);
			}
			else throw new NotSupportedException("Unknown v00 .. " + v00);
		}  

	

		public static List<STim> ParseTex_TIMf(MemoryStream si, BinaryReader br)
		{
			int v00 = br.ReadInt32();
			int v04 = br.ReadInt32();
			int v08 = br.ReadInt32(); // v08: cnt-pal1? each-size=0x90
			int v0c = br.ReadInt32(); // v0c: cnt-pal2? each-size=0xA0
			int v10 = br.ReadInt32(); // v10: off-pal2-tbl
			int v14 = br.ReadInt32(); // v14: off-pal1
			int v18 = br.ReadInt32(); // v18: off-pal2
			int v1c = br.ReadInt32(); // v1c: off-tex1?
			int v20 = br.ReadInt32(); // v20: off-tex2?

			SortedDictionary<int, int> map2to1 = new SortedDictionary<int, int>();

			si.Position = v10;
			for (int p1 = 0; p1 < v0c; p1++)
			{
				map2to1[p1] = br.ReadByte();
			}

			Texctx tc = new Texctx();

			List<int> offPalfrom1 = new List<int>();

			for (int p1 = 0; p1 < v08 + 1; p1++)
			{
				//int offp1 = v14 + 0x90 * p1;
				//si.Position = offp1 + 0x20;
				//tc.Do1(si);
				//int offPal = tc.offTex;
				//offPalfrom1.Add(offPal);
			}

			List<STim> pics = new List<STim>();
			for (int p2 = 0; p2 < v0c; p2++)
			{
				int offp1pal = v14;
				si.Position = offp1pal + 0x20;
				tc.Do1(si);
				int offPal = tc.offBin;

				int offp1tex = v14 + 0x90 * (1 + map2to1[p2]);
				si.Position = offp1tex + 0x20;
				tc.Do1(si);
				int offTex = tc.offBin;

				int offp2 = v18 + 0xA0 * p2 + 0x20;
				si.Position = offp2;
				STim st = tc.Do2(si);

				TextureAddressMode textureAddressMode = new TextureAddressMode();

				textureAddressMode.AddressU = (TextureWrapMode)((int)st.wms);
				textureAddressMode.AddressV = (TextureWrapMode)((int)st.wmt);
				textureAddressMode.Left = st.minu;
				textureAddressMode.Right = st.maxu;
				textureAddressMode.Top = st.minv;
				textureAddressMode.Bottom = st.maxv;

				st.TextureAddressMode = textureAddressMode;

				pics.Add(st);
			}
			return pics;
		}

		class MI
		{
			public SortedDictionary<string, int> col2off = new SortedDictionary<string, int>();

			public void Add(string col, int off)
			{
				col2off[col] = off;
			}
		}

		class Texctx
		{
			public byte[] gs = new byte[1024 * 1024 * 4];
			public int t0PSM;
			public int offBin;

			public void Do1(Stream si)
			{
				BinaryReader br = new BinaryReader(si);
				UInt64 cm;

				UInt64 r50 = br.ReadUInt64(); cm = br.ReadUInt64(); XDebug.Assert(0x50 == cm, cm.ToString("X16") + " ≠ 0x50"); // 0x50 BITBLTBUF
				int SBP = ((int)(r50)) & 0x3FFF;
				int SBW = ((int)(r50 >> 16)) & 0x3F;
				int SPSM = ((int)(r50 >> 24)) & 0x3F;
				int DBP = ((int)(r50 >> 32)) & 0x3FFF;
				int DBW = ((int)(r50 >> 48)) & 0x3F;
				int DPSM = ((int)(r50 >> 56)) & 0x3F;
				XTrace.Assert(SBP == 0);
				XTrace.Assert(SBW == 0);
				XTrace.Assert(SPSM == 0);
				XTrace.Assert(DPSM == 0 || DPSM == 19 || DPSM == 20);

				UInt64 r51 = br.ReadUInt64(); cm = br.ReadUInt64(); XDebug.Assert(0x51 == cm, cm.ToString("X16") + " ≠ 0x51"); // 0x51 TRXPOS
				int SSAX = ((int)(r51)) & 0x7FF;
				int SSAY = ((int)(r51 >> 16)) & 0x7FF;
				int DSAX = ((int)(r51 >> 32)) & 0x7FF;
				int DSAY = ((int)(r51 >> 48)) & 0x7FF;
				int DIR = ((int)(r51 >> 59)) & 3;
				XTrace.Assert(SSAX == 0);
				XTrace.Assert(SSAY == 0); //!
				XTrace.Assert(DSAX == 0); //!
				XTrace.Assert(DSAY == 0); //!
				XTrace.Assert(DIR == 0); //!

				UInt64 r52 = br.ReadUInt64(); cm = br.ReadUInt64(); XDebug.Assert(0x52 == cm, cm.ToString("X16") + " ≠ 0x52"); // 0x52 TRXREG
				int RRW = ((int)(r52 >> 0)) & 0xFFF;
				int RRH = ((int)(r52 >> 32)) & 0xFFF;

				UInt64 r53 = br.ReadUInt64(); cm = br.ReadUInt64(); XDebug.Assert(0x53 == cm, cm.ToString("X16") + " ≠ 0x53"); // 0x53 TRXDIR
				int XDIR = ((int)(r53)) & 2;
				XTrace.Assert(XDIR == 0);

				int eop = br.ReadUInt16();
				XTrace.Assert(8 != (eop & 0x8000));

				si.Position += 18;
				offBin = br.ReadInt32();

				int cbTex = (eop & 0x7FFF) << 4;
				int MyDBH = cbTex / 8192 / DBW;

				{
					byte[] binTmp = new byte[Math.Max(8192, cbTex)]; // decoder needs at least 8kb
					si.Position = offBin;
					si.Read(binTmp, 0, cbTex);
					byte[] binDec;
					if (DPSM == 0)
					{
						binDec = OpenKh.Kh2.Ps2.Encode32(binTmp, DBW, MyDBH);
					}
					else if (DPSM == 19)
					{
						binDec = OpenKh.Kh2.Ps2.Encode8(binTmp, DBW / 2, cbTex / 8192 / (DBW / 2));
					}
					else if (DPSM == 20)
					{
						binDec = OpenKh.Kh2.Ps2.Encode4(binTmp, DBW / 2, cbTex / 8192 / Math.Max(1, DBW / 2));
					}
					else
					{
						throw new NotSupportedException("DPSM = " + DPSM + "?");
					}
					Array.Copy(binDec, 0, gs, 256 * DBP, cbTex);
				}

				XDebug.WriteLine(string.Format("# p1 {0:x4}	  {1,6} {2}", DBP, cbTex, DPSM));
			}

			public STim Do2(Stream si)
			{
				BinaryReader br = new BinaryReader(si);
				UInt64 command;

				UInt64 r3f = br.ReadUInt64(); XTrace.Assert((command = br.ReadUInt64()) == 0x3F); // 0x3F TEXFLUSH
				UInt64 r34 = br.ReadUInt64(); XTrace.Assert((command = br.ReadUInt64()) == 0x34); // 0x34 MIPTBP1_1
				UInt64 r36 = br.ReadUInt64(); XTrace.Assert((command = br.ReadUInt64()) == 0x36); // 0x36 MIPTBP2_1
				UInt64 r16 = br.ReadUInt64(); XTrace.Assert((command = br.ReadUInt64()) == 0x16); // 0x16 TEX2_1
				int t2PSM = ((int)(r16 >> 20)) & 0x3F;
				int t2CBP = ((int)(r16 >> 37)) & 0x3FFF;
				int t2CPSM = ((int)(r16 >> 51)) & 0xF;
				int t2CSM = ((int)(r16 >> 55)) & 0x1;
				int t2CSA = ((int)(r16 >> 56)) & 0x1F;
				int t2CLD = ((int)(r16 >> 61)) & 0x7;
				XTrace.Assert(t2PSM == 19); // PSMT8
				XTrace.Assert(t2CPSM == 0); // PSMCT32
				XTrace.Assert(t2CSM == 0); // CSM1
				XTrace.Assert(t2CSA == 0);
				XTrace.Assert(t2CLD == 4);

				UInt64 r14 = br.ReadUInt64(); XTrace.Assert((command = br.ReadUInt64()) == 0x14);// 0x14 TEX1_1
				int t1LCM = ((int)(r14 >> 0)) & 1;
				int t1MXL = ((int)(r14 >> 2)) & 7;
				int t1MMAG = ((int)(r14 >> 5)) & 1;
				int t1MMIN = ((int)(r14 >> 6)) & 7;
				int t1MTBA = ((int)(r14 >> 9)) & 1;
				int t1L = ((int)(r14 >> 19)) & 3;
				int t1K = ((int)(r14 >> 32)) & 0xFFF;

				UInt64 r06 = br.ReadUInt64(); XTrace.Assert((command = br.ReadUInt64()) == 0x06);// 0x06 TEX0_1
				int t0TBP0 = ((int)(r06 >> 0)) & 0x3FFF;
				int t0TBW = ((int)(r06 >> 14)) & 0x3F;
				t0PSM = ((int)(r06 >> 20)) & 0x3F;
				int t0TW = ((int)(r06 >> 26)) & 0xF;
				int t0TH = ((int)(r06 >> 30)) & 0xF;
				int t0TCC = ((int)(r06 >> 34)) & 0x1;
				int t0TFX = ((int)(r06 >> 35)) & 0x3;
				int t0CBP = ((int)(r06 >> 37)) & 0x3FFF;
				int t0CPSM = ((int)(r06 >> 51)) & 0xF;
				int t0CSM = ((int)(r06 >> 55)) & 0x1;
				int t0CSA = ((int)(r06 >> 56)) & 0x1F;
				int t0CLD = ((int)(r06 >> 61)) & 0x7;
				XTrace.Assert(t0PSM == 19 || t0PSM == 20);
				XTrace.Assert(t0TCC == 1);
				XTrace.Assert(t0CPSM == 0);
				XTrace.Assert(t0CSM == 0);
				//XTrace.Assert(t0CSA == 0);
				XTrace.Assert(t0CLD == 0);

				UInt64 r08 = br.ReadUInt64(); XTrace.Assert((command = br.ReadUInt64()) == 0x08);// 0x08 CLAMP_1
				int c1WMS = ((int)(r08 >> 0)) & 0x3;
				int c1WMT = ((int)(r08 >> 2)) & 0x3;
				int c1MINU = ((int)(r08 >> 4)) & 0x3FF;
				int c1MAXU = ((int)(r08 >> 14)) & 0x3FF;
				int c1MINV = ((int)(r08 >> 24)) & 0x3FF;
				int c1MAXV = ((int)(r08 >> 34)) & 0x3FF;

				int sizetbp0 = (1 << t0TW) * (1 << t0TH);
				byte[] buftbp0 = new byte[Math.Max(8192, sizetbp0)]; // needs at least 8kb
				Array.Copy(gs, 256 * t0TBP0, buftbp0, 0, Math.Min(gs.Length - 256 * t0TBP0, Math.Min(buftbp0.Length, sizetbp0)));
				byte[] bufcbpX = new byte[8192];

				Array.Copy(gs, 256 * t0CBP, bufcbpX, 0, bufcbpX.Length);

				for (int i = 0; i < bufcbpX.Length; i += 4)
				{
					if (bufcbpX[i + 3] == 255)
					{
						for (int j = 0; j < bufcbpX.Length; j += 4)
						{
							if (bufcbpX[j + 3]<128)
							bufcbpX[j + 3] = (byte)Math.Min(bufcbpX[j + 3] * 2, 128);
						}
						break;
					}
				}

				XDebug.WriteLine(string.Format("# p2 {0:x4} {1:x4} {2,2}", t0TBP0, t0CBP, t0CSA));

				STim st = null;
				if (t0PSM == 0x13) st = TexUt2.Decode8(buftbp0, bufcbpX, t0TBW, 1 << t0TW, 1 << t0TH);
				if (t0PSM == 0x14) st = TexUt2.Decode4Ps(buftbp0, bufcbpX, t0TBW, 1 << t0TW, 1 << t0TH, t0CSA);
				if (st != null)
				{
					st.tfx = (TFX)t0TFX;
					st.tcc = (TCC)t0TCC;
					st.wms = (WM)c1WMS;
					st.wmt = (WM)c1WMT;
					st.minu = c1MINU;
					st.maxu = c1MAXU;
					st.minv = c1MINV;
					st.maxv = c1MAXV;
				}
				return st;
			}
		}
		public enum TFX {Modulate = 0, Decal = 1, HL = 2, HL2 = 3}
		public enum TCC {RGB = 0, RGBA = 1}
		public enum WM {REPEAT = 0, CLAMP = 1, RClamp = 2, RRepeat = 3}

		public enum TextureWrapMode
		{
			Repeat,
			Clamp,
			RegionClamp,
			RegionRepeat
		}

		public class TextureAddressMode
		{
			public TextureWrapMode AddressU { get; set; }

			public TextureWrapMode AddressV { get; set; }

			public int Left { get; set; }

			public int Top { get; set; }

			public int Right { get; set; }

			public int Bottom { get; set; }


		}


		public class STim
		{
			public TextureAddressMode TextureAddressMode;
			public Bitmap pic;
			public TFX tfx = TFX.Modulate;
			public TCC tcc = TCC.RGB;
			public WM wms = WM.REPEAT, wmt = WM.REPEAT;
			/// <summary>
			/// Clamp U Lo ; UMSK
			/// </summary>
			public int minu = 0;
			/// <summary>
			/// clamp U Hi ; UFIX
			/// </summary>
			public int maxu = 0;
			public int minv = 0;
			public int maxv = 0;

			public STim(Bitmap pic)
			{
				this.pic = pic;
			}

			public int UMSK { get { return minu; } }
			public int VMSK { get { return minv; } }
			public int UFIX { get { return maxu; } }
			public int VFIX { get { return maxv; } }

			/*public object[] Generate()
			{
				if (wms == WM.RRepeat && wmt == WM.RRepeat)
				{
					Bitmap p = new Bitmap(UMSK + 1, VMSK + 1);
					using (Graphics cv = Graphics.FromImage(p))
					{
						cv.DrawImage(
							pic,
							new Point[] {
							new Point(0, 0),
							new Point(p.Width, 0),
							new Point(0, p.Height),
							},
							new Rectangle(UFIX, VFIX, UMSK + 1, VMSK + 1),
							GraphicsUnit.Pixel
							);
					}
					return new object[] { p, wms };
				}
				else if (wms == WM.RClamp && wmt == WM.RClamp)
				{
					Bitmap p = new Bitmap(pic);

					using (Graphics cv = Graphics.FromImage(p))
					{
						int x0 = 0, y0 = 0;
						int x1 = minu, y1 = minv;
						int x2 = maxu, y2 = maxv;
						int x3 = p.Width, y3 = p.Height;
						cv.CompositingMode = global::System.Drawing.Drawing2D.CompositingMode.SourceCopy;


						//TL
						cv.FillRectangle(
							new SolidBrush(p.GetPixel(x1, y1)),
							Rectangle.FromLTRB(x0, y0, x1, y1)
							);
						//TC
						cv.DrawImage(
							p,
							new Point[] {
							new Point(x1, y0),
							new Point(x2, y0),
							new Point(x1, y1),
							},
							Rectangle.FromLTRB(x1, y1, x2, y1 + 1),
							GraphicsUnit.Pixel
							);
						//TR
						cv.FillRectangle(
							new SolidBrush(p.GetPixel(x2, y1)),
							Rectangle.FromLTRB(x2, y0, x3, y1)
							);
						//ML
						cv.DrawImage(
							p,
							new Point[] {
							new Point(x0, y1),
							new Point(x1, y1),
							new Point(x0, y2),
							},
							Rectangle.FromLTRB(x1, y1, x1 + 1, y2),
							GraphicsUnit.Pixel
							);
						//MR
						cv.DrawImage(
							p,
							new Point[] {
							new Point(x2, y1),
							new Point(x3, y1),
							new Point(x2, y2),
							},
							Rectangle.FromLTRB(x2 - 1, y1, x2, y2),
							GraphicsUnit.Pixel
							);

						//BL
						cv.FillRectangle(
							new SolidBrush(p.GetPixel(x1, y2)),
							Rectangle.FromLTRB(x0, y2, x1, y3)
							);
						//BC
						cv.DrawImage(
							p,
							new Point[] {
							new Point(x1, y2),
							new Point(x2, y2),
							new Point(x1, y3),
							},
							Rectangle.FromLTRB(x1, y2 - 1, x2, y2),
							GraphicsUnit.Pixel
							);
						//BR
						cv.FillRectangle(
							new SolidBrush(p.GetPixel(x2, y2)),
							Rectangle.FromLTRB(x2, y2, x3, y3)
							);


					}

					return new object[] { p, wms };
				}
				else if (wms != wmt)
				{
					XDebug.Fail(wms + " ≠ " + wmt);
				}
				return new object[] { pic, wms };
			}*/
		}

		class TexUt2
		{
			public static STim Decode8(byte[] picbin, byte[] palbin, int tbw, int cx, int cy)
			{
				Bitmap pic = new Bitmap(cx, cy, global::System.Drawing.Imaging.PixelFormat.Format8bppIndexed);
				tbw /= 2;
				XDebug.Assert(tbw != 0, "Invalid");
				byte[] bin = OpenKh.Kh2.Ps2.Decode8(picbin, tbw, Math.Max(1, picbin.Length / 8192 / tbw));
				BitmapData bd = pic.LockBits(Rectangle.FromLTRB(0, 0, pic.Width, pic.Height), ImageLockMode.WriteOnly, global::System.Drawing.Imaging.PixelFormat.Format8bppIndexed);

				try
				{
					int buffSize = bd.Stride * bd.Height;
					global::System.Runtime.InteropServices.Marshal.Copy(bin, 0, bd.Scan0, Math.Min(bin.Length, buffSize));
				}
				finally
				{
					pic.UnlockBits(bd);
				}
				ColorPalette pals = pic.Palette;
				int psi = 0;

				byte[] palb2 = new byte[1024];
				for (int t = 0; t < 256; t++)
				{
					int toi = repl(t);
					Array.Copy(palbin, 4 * t + 0, palb2, 4 * toi + 0, 4);
				}
				Array.Copy(palb2, 0, palbin, 0, 1024);

				for (int pi = 0; pi < 256; pi++)
				{
					int alpha = AcUt.GetA(palbin[psi + 4 * pi + 3]) ^ (pi & 1);
					pals.Entries[pi] = CUtil.Gamma(Color.FromArgb(
						alpha,
						Math.Min(255, palbin[psi + 4 * pi + 0] + 0),
						Math.Min(255, palbin[psi + 4 * pi + 1] + 0),
						Math.Min(255, palbin[psi + 4 * pi + 2] + 0)
						), γ);
				}
				pic.Palette = pals;
				STim output = new STim(pic);
				return output;
			}
			readonly static sbyte[] tbl = new sbyte[] {
			0,
			0,
			6,
			6,
			-2,
			-2,
			4,
			4,
			-4,
			-4,
			2,
			2,
			-6,
			-6,
			0,
			0,
			16,
			16,
			22,
			22,
			14,
			14,
			20,
			20,
			12,
			12,
			18,
			18,
			10,
			10,
			16,
			16,
			32,
			32,
			38,
			38,
			30,
			30,
			36,
			36,
			28,
			28,
			34,
			34,
			26,
			26,
			32,
			32,
			48,
			48,
			54,
			54,
			46,
			46,
			52,
			52,
			44,
			44,
			50,
			50,
			42,
			42,
			48,
			48,
			-48,
			-48,
			-42,
			-42,
			-50,
			-50,
			-44,
			-44,
			-52,
			-52,
			-46,
			-46,
			-54,
			-54,
			-48,
			-48,
			-32,
			-32,
			-26,
			-26,
			-34,
			-34,
			-28,
			-28,
			-36,
			-36,
			-30,
			-30,
			-38,
			-38,
			-32,
			-32,
			-16,
			-16,
			-10,
			-10,
			-18,
			-18,
			-12,
			-12,
			-20,
			-20,
			-14,
			-14,
			-22,
			-22,
			-16,
			-16,
			0,
			0,
			6,
			6,
			-2,
			-2,
			4,
			4,
			-4,
			-4,
			2,
			2,
			-6,
			-6,
			0,
			0,
		};
			public static int repl(int t)
			{
				return (t & 0x80) | ((t & 0x7F) + tbl[t & 0x7F]);
			}
			class AcUt
			{
				public static byte GetA(byte a)
				{
					return a;
					//return (byte)Math.Min(a * 255 / 0x80, 255);
				}
			}

			public static STim Decode4(byte[] picbin, byte[] palbin, int tbw, int cx, int cy)
			{
				Bitmap pic = new Bitmap(cx, cy, global::System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
				tbw /= 2;
				XDebug.Assert(tbw != 0, "Invalid");
				byte[] bin = OpenKh.Kh2.Ps2.Decode4(picbin, tbw, Math.Max(1, picbin.Length / 8192 / tbw));
				BitmapData bd = pic.LockBits(Rectangle.FromLTRB(0, 0, pic.Width, pic.Height), ImageLockMode.WriteOnly, global::System.Drawing.Imaging.PixelFormat.Format4bppIndexed);

				try
				{
					int buffSize = bd.Stride * bd.Height;
					global::System.Runtime.InteropServices.Marshal.Copy(bin, 0, bd.Scan0, Math.Min(bin.Length, buffSize));
				}
				finally
				{
					pic.UnlockBits(bd);
				}
				ColorPalette pals = pic.Palette;
				int psi = 0;
				for (int pi = 0; pi < 16; pi++)
				{
					int alpha = AcUt.GetA(palbin[psi + 4 * pi + 3]);
					
					pals.Entries[pi] = CUtil.Gamma(Color.FromArgb(
						alpha,
						palbin[psi + 4 * pi + 0],
						palbin[psi + 4 * pi + 1],
						palbin[psi + 4 * pi + 2]
						), γ);
				}
				pic.Palette = pals;
				STim output = new STim(pic);
				return output;
			}

			public static STim Decode4Ps(byte[] picbin, byte[] palbin, int tbw, int cx, int cy, int csa)
			{

				Bitmap pic = new Bitmap(cx, cy, global::System.Drawing.Imaging.PixelFormat.Format4bppIndexed);
				tbw = Math.Max(1, tbw / 2);
				XDebug.Assert(tbw != 0, "Invalid");
				byte[] bin = OpenKh.Kh2.Ps2.Decode4(picbin, tbw, Math.Max(1, picbin.Length / 8192 / tbw));
				BitmapData bd = pic.LockBits(Rectangle.FromLTRB(0, 0, pic.Width, pic.Height), ImageLockMode.WriteOnly, global::System.Drawing.Imaging.PixelFormat.Format4bppIndexed);

				try
				{
					int buffSize = bd.Stride * bd.Height;
					global::System.Runtime.InteropServices.Marshal.Copy(bin, 0, bd.Scan0, Math.Min(bin.Length, buffSize));
				}
				finally
				{
					pic.UnlockBits(bd);
				}
				ColorPalette pals = pic.Palette;
				int psi = 64 * csa;
				byte[] palb2 = new byte[1024];
				for (int t = 0; t < 256; t++)
				{
					int toi = repl(t);
					Array.Copy(palbin, 4 * t + 0, palb2, 4 * toi + 0, 4);
				}
				//Array.Copy(palb2, 0, palbin, 0, 1024);
				for (int pi = 0; pi < 16; pi++)
				{
					int alpha = AcUt.GetA(palb2[psi + 4 * pi + 3]);

					pals.Entries[pi] = CUtil.Gamma(Color.FromArgb(
						alpha,
						palb2[psi + 4 * pi + 0],
						palb2[psi + 4 * pi + 1],
						palb2[psi + 4 * pi + 2]
						), γ);
				}
				pic.Palette = pals;
				STim output = new STim(pic);

				return output;
			}

			const float γ = 1f; //0.5f;
		}

		class Texi : Hexi
		{
			public STim st;

			public Texi(int off, STim st)
				: base(off)
			{
				this.st = st;
			}
			public Texi(int off, MI mi, STim st)
				: base(off, mi)
			{
				this.st = st;
			}
		}

		class Vifi : Hexi
		{
			public byte[] vifpkt;

			public Vifi(int off, byte[] vifpkt)
				: base(off)
			{
				this.vifpkt = vifpkt;
			}
			public Vifi(int off, MI mi, byte[] vifpkt)
				: base(off, mi)
			{
				this.vifpkt = vifpkt;
			}
		}
		class Hexi
		{
			public int off, len;
			public MI mi = null;

			public Hexi(int off)
			{
				this.off = off;
				this.len = 0;
			}
			public Hexi(int off, int len)
			{
				this.off = off;
				this.len = len;
			}
			public Hexi(int off, MI mi)
			{
				this.off = off;
				this.mi = mi;
			}
		}
		class CUtil
		{
			public static Color Gamma(Color a, float gamma)
			{
				return Color.FromArgb(
					a.A,
					Math.Min(255, (int)(Math.Pow(a.R / 255.0, gamma) * 255.0)),
					Math.Min(255, (int)(Math.Pow(a.G / 255.0, gamma) * 255.0)),
					Math.Min(255, (int)(Math.Pow(a.B / 255.0, gamma) * 255.0))
					);
			}
		}


	}
}
