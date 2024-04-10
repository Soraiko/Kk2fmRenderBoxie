using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace BDxGraphiK
{
	public struct Texture
	{
		public static Texture whitePixel1x1;
		public static Texture bumpPixel1x1;

		public static Dictionary<string, Texture> Textures = new Dictionary<string, Texture>(0);
		public int Width;
		public int Height;

		/* For TexturePatches */
		public enum PatchOrientation
		{
			Horizontal = 0,
			Vertical = 1
		}
		public int X;
		public int Y;
		public int Count;
		public PatchOrientation Orientation;

		public int[] TextureMinFilter;
		public int[] TextureWrapS;
		public int[] TextureWrapT;

		public int Integer;
		public string Filename;



		public static string TestLateralPath(string ownerPath, string lateralPath)
		{
			if (File.Exists(Path.GetDirectoryName(ownerPath) + @"\" + lateralPath))
				return Path.GetDirectoryName(ownerPath) + @"\" + lateralPath;
			else
				return lateralPath;
		}

		static Texture()
		{
			whitePixel1x1 = Texture.LoadTexture(@"resources\whitePixel1x1.png", null, OpenTK.Graphics.OpenGL.TextureMinFilter.Nearest, TextureWrapMode.Repeat, TextureWrapMode.Repeat,true);
			bumpPixel1x1 = Texture.LoadTexture(@"resources\bumpPixel1x1.png", null, OpenTK.Graphics.OpenGL.TextureMinFilter.Nearest, TextureWrapMode.Repeat, TextureWrapMode.Repeat, true);
		}

		
		public static Texture LoadTexture(string filename, System.Drawing.Bitmap input_bmp, TextureMinFilter textureMinFilter, TextureWrapMode textureWrapS, TextureWrapMode textureWrapT, bool disposeTexture)
		{
			if (Textures.ContainsKey(filename))
			{
				return Textures[filename];
			}
			Texture output = new Texture();
			if (input_bmp == null && !File.Exists(filename))
			{
				return output;
			}


			int indexOfCurrDir = filename.IndexOf(Directory.GetCurrentDirectory());

			if (indexOfCurrDir > -1)
				filename = filename.Substring(indexOfCurrDir);

			System.Drawing.Bitmap bmp_to_buffer = input_bmp ?? (System.Drawing.Bitmap)System.Drawing.Image.FromFile(filename);
			output.Filename = filename;


			int depth = System.Drawing.Bitmap.GetPixelFormatSize(bmp_to_buffer.PixelFormat);

			if (depth != 32)
				bmp_to_buffer = bmp_to_buffer.Clone(new System.Drawing.Rectangle(0, 0, bmp_to_buffer.Width, bmp_to_buffer.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			
			output.Width = bmp_to_buffer.Width;
			output.Height = bmp_to_buffer.Height;
			System.Drawing.Imaging.BitmapData data = bmp_to_buffer.LockBits(new System.Drawing.Rectangle(0, 0, bmp_to_buffer.Width, bmp_to_buffer.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp_to_buffer.PixelFormat);
			PixelInternalFormat format = PixelInternalFormat.Rgba;

			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
			GL.GenTextures(1, out output.Integer);
			GL.BindTexture(TextureTarget.Texture2D, output.Integer);

			GL.TexImage2D(TextureTarget.Texture2D, 0, format, bmp_to_buffer.Width, bmp_to_buffer.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
			
			bmp_to_buffer.UnlockBits(data);
			if (disposeTexture)
				bmp_to_buffer.Dispose();

			output.TextureMinFilter = new int[] { (int)textureMinFilter };
			output.TextureWrapS = new int[] { (int)textureWrapS };
			output.TextureWrapT = new int[] { (int)textureWrapT };

			Textures.Add(filename, output);
			return output;
		}
	}
}
