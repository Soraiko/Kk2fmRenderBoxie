//#define MESH_RENDER_MODE_RECURSIVE <-- doesn't work for many mesh models

using Assimp;
using System;
using System.Collections.Generic;
using System.IO;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static BDxGraphiK.Mesh;
using System.Collections;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace BDxGraphiK
{
	public class Mesh : BinableObject
	{
		public class Shader : IDisposable
		{
			public new string ToString()
			{
				return "T" + (((this.Flag & 1) > 0) ? "1" : "0") +
						"N" + (((this.Flag & 2) > 0) ? "1" : "0") +
						"C" + (((this.Flag & 4) > 0) ? "1" : "0") +
						"S" + (((this.Flag & 8) > 0) ? "1" : "0");
			}
			public int Handle;
			public int Flag;
			int VertexShader;
			int FragmentShader;
			public enum FogMode
			{
				None = 0,
				XYZ = 1,
				XZ = 2
			}
			public static List<Shader> FlaggedShaders;

			static Shader()
			{
				FlaggedShaders = new List<Shader>(0);

				for (int i = 0; i < 16; i++)
				{
					string fname = @"resources\graphics\" +
						"T" + (((i & 1) > 0) ? "1" : "0") +
						"N" + (((i & 2) > 0) ? "1" : "0") +
						"C" + (((i & 4) > 0) ? "1" : "0") +
						"S" + (((i & 8) > 0) ? "1" : "0");
					if (File.Exists(fname + "_vert.c") && File.Exists(fname.Remove(fname.Length - 2, 2) + "_frag.c"))
					{
						var shader = new Shader(fname + "_vert.c", fname.Remove(fname.Length - 2, 2) + "_frag.c");
						shader.Flag = i;
						FlaggedShaders.Add(shader);
					}
					else
						FlaggedShaders.Add(null);
				}
			}

			public Shader(string vertexPath, string fragmentPath)
			{
				this.Flag = 0;

				VertexShader = GL.CreateShader(ShaderType.VertexShader);
				GL.ShaderSource(VertexShader, System.IO.File.ReadAllText(vertexPath, System.Text.Encoding.ASCII));

				FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
				GL.ShaderSource(FragmentShader, System.IO.File.ReadAllText(fragmentPath, System.Text.Encoding.ASCII));

				GL.CompileShader(VertexShader);
				string infoLogVert = GL.GetShaderInfoLog(VertexShader);
				if (infoLogVert != System.String.Empty)
					System.Console.WriteLine(vertexPath+ "\r\n"+ infoLogVert);

				GL.CompileShader(FragmentShader);
				string infoLogFrag = GL.GetShaderInfoLog(FragmentShader);
				if (infoLogFrag != System.String.Empty)
					System.Console.WriteLine(fragmentPath+ "\r\n"+ infoLogFrag);

				Handle = GL.CreateProgram();

				GL.AttachShader(Handle, VertexShader);
				GL.AttachShader(Handle, FragmentShader);

				GL.LinkProgram(Handle);
				GL.UseProgram(Handle);

				GL.DetachShader(Handle, VertexShader);
				GL.DetachShader(Handle, FragmentShader);
				GL.DeleteShader(FragmentShader);
				GL.DeleteShader(VertexShader);
			}

			int bump_mapping = -1;
			int[] patches = new int[] {-1,-1,-1,-1};

			public void Use(TextureMaterial material, int handle)
			{
				var texture = material.Textures[(int)TextureMaterial.TextureType.Diffuse];
				var textureInteger = texture.Integer;

				if (textureInteger == 0)
				{
					texture = Texture.whitePixel1x1;
					textureInteger = Texture.whitePixel1x1.Integer;
				}

				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, textureInteger);

				GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, texture.TextureWrapS);
				GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, texture.TextureWrapT);
				GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, texture.TextureMinFilter);

				for (int p=0;p<material.TexturePatches.Length;p++)
				{
					texture = material.TexturePatches[p];
					GL.ActiveTexture((TextureUnit)((int)TextureUnit.Texture2+p));
					GL.BindTexture(TextureTarget.Texture2D, texture.Integer);

					if (patches[p] < 0)
						patches[p] = GL.GetUniformLocation(handle, "patch"+p);
					if (patches[p] > -1)
					{
						GL.Uniform1(patches[p], 2+p);
						GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, texture.TextureWrapS);
						GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, texture.TextureWrapT);
						GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, texture.TextureMinFilter);
					}
				}

				if (handle == this.Handle)
				{
					texture = material.Textures[(int)TextureMaterial.TextureType.BumpMapping];
					textureInteger = texture.Integer;

					if (textureInteger > 0)
					{
						GL.ActiveTexture(TextureUnit.Texture1);
						GL.BindTexture(TextureTarget.Texture2D, textureInteger);

						if (bump_mapping < 0)
							bump_mapping = GL.GetUniformLocation(handle, "bump_mapping");
						if (bump_mapping > -1)
							GL.Uniform1(bump_mapping, 1);
					}

				}
			}

			protected virtual void Dispose(bool disposing)
			{
				GL.DeleteProgram(this.Handle);
			}


			public void Dispose()
			{
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}
		}

		public Mesh ShadowMesh;
		public int IndexBufferObject;
		public int VertexBufferObject;
		public int VertexArrayObject;


		public OpenTK.Graphics.OpenGL.PrimitiveType PrimitiveType;

		public List<Vector3D> Positions;
		public List<List<short>> Influences;
		public List<List<float>> Weights;
		public List<Vector3D> TextureCoords;
		public List<Color4D> Colors;
		public List<Vector3D> Normals;
		public short[] Indices;
		public int PrimitiveCount;

		public Object3D Object3D;
		public Shader shader;
		public int MaterialIndex;

		public Mesh(Object3D object3d)
		{
			this.ObjectFlag = ASCII4.Mesh;
			this.Object3D = object3d;
			this.Positions = new List<Vector3D>();
			this.Normals = new List<Vector3D>();
			this.Colors = new List<Color4D>();
			this.TextureCoords = new List<Vector3D>();

			this.Influences = new List<List<short>>();
			this.Weights = new List<List<float>>();
			this.PrimitiveCount = 0;
			this.Indices = new short[0];
		}

		public const int POSITION_SIZE = 3 * sizeof(float);
		public const int TEXTURECOORDINATE_SIZE = 2 * sizeof(float);
		public const int NORMAL_SIZE = 3 * sizeof(float);
		public const int COLOR_SIZE = 4 * sizeof(byte);
		public const int INFLUENCE_WEIGHT_SIZE = 2 * sizeof(short);



		int BinaryFlag;
		byte[] VertexBinary;

		public Mesh3DComponentBits MeshFlag;

		public enum Mesh3DComponentBits
		{
			PositionsOnly =
				0b00000000000000000000000000000000,
			TextureCoords =
				0b00000000000000000000000000000001,
			Normals =
				0b00000000000000000000000000000010,
			Colors =
				0b00000000000000000000000000000100,
			Influences =
				0b00000000000000000000000000001000,
			Indices =
				0b00000000000000000000000000010000
		}

		public Vector4 Sphere;
		public float Area;

		public new void GenerateBinary()
		{
			int flag = (int)Mesh3DComponentBits.PositionsOnly;
			
			if (this.TextureCoords.Count > 0)
				flag = flag | (int)Mesh3DComponentBits.TextureCoords;
			if (this.Normals.Count > 0)
				flag = flag | (int)Mesh3DComponentBits.Normals;
			if (this.Colors.Count > 0)
				flag = flag | (int)Mesh3DComponentBits.Colors;
			if (this.Influences.Count > 0)
				flag = flag | (int)Mesh3DComponentBits.Influences;
			if (this.Indices.Length > 0)
				flag = flag | (int)Mesh3DComponentBits.Indices;

			this.MeshFlag = (Mesh3DComponentBits)flag;

			int maxInfCount = 0;
			for (int i = 0; i < this.Influences.Count; i++)
			{
				if (this.Influences[i].Count > maxInfCount)
					maxInfCount = this.Influences[i].Count;
			}

			if (this.StreamRW == null)
				this.StreamRW = new BinaryRW();

			this.StreamRW.BaseStream.Position = this.StreamRW.BaseStream.Length;
			this.StreamRW.BinaryWriter.Write((int)this.ObjectFlag);

			this.StreamRW.BinaryWriter.Write((int)this.MeshFlag);
			this.StreamRW.BinaryWriter.Write((int)this.PrimitiveType);

			this.StreamRW.BinaryWriter.Write(this.Positions.Count);

			List<Vector3d> vector3Ds = new List<Vector3d>(0);

			Vector3d middle = new Vector3d(0,0,0);

			for (int i = 0; i < this.Positions.Count; i++)
			{
				Vector3d currV3d =  new Vector3d(this.Positions[i].X, this.Positions[i].Y, this.Positions[i].Z);
				middle += currV3d;
				vector3Ds.Add(currV3d);
			}
			middle /= (double)this.Positions.Count;

			double radius = double.MinValue;

			for (int i = 0; i < this.Positions.Count; i++)
			{
				double dist = Vector3d.Distance(middle, vector3Ds[i]);
				if (dist > radius)
				{
					radius = dist;
				}
				this.StreamRW.BinaryWriter.Write(this.Positions[i].X);
				this.StreamRW.BinaryWriter.Write(this.Positions[i].Y);
				this.StreamRW.BinaryWriter.Write(this.Positions[i].Z);
				
				if ((flag & (int)Mesh3DComponentBits.TextureCoords) > 0)
				{
					this.StreamRW.BinaryWriter.Write(this.TextureCoords[i].X);
					this.StreamRW.BinaryWriter.Write(this.TextureCoords[i].Y);
				}
				if ((flag & (int)Mesh3DComponentBits.Normals) > 0)
				{
					this.StreamRW.BinaryWriter.Write(this.Normals[i].X);
					this.StreamRW.BinaryWriter.Write(this.Normals[i].Y);
					this.StreamRW.BinaryWriter.Write(this.Normals[i].Z);
				}
				if ((flag & (int)Mesh3DComponentBits.Colors) > 0)
				{
					this.StreamRW.BinaryWriter.Write((byte)Math.Min(Math.Max(0f, this.Colors[i].R) * 255, 255));
					this.StreamRW.BinaryWriter.Write((byte)Math.Min(Math.Max(0f, this.Colors[i].G) * 255, 255));
					this.StreamRW.BinaryWriter.Write((byte)Math.Min(Math.Max(0f, this.Colors[i].B) * 255, 255));
					this.StreamRW.BinaryWriter.Write((byte)Math.Min(Math.Max(0f, this.Colors[i].A) * 64, 255));
				}
				if ((flag & (int)Mesh3DComponentBits.Influences) > 0)
				{
					int reserved = this.Influences[i].Count;
					this.StreamRW.BinaryWriter.Write((byte)maxInfCount);
					int free = maxInfCount - reserved;

					for (byte j = 0; j < reserved; j++)
					{
						this.StreamRW.BinaryWriter.Write(this.Influences[i][j]);
						this.StreamRW.BinaryWriter.Write((short)Math.Min(short.MaxValue, Math.Max(0, (long)(this.Weights[i][j] * ushort.MaxValue))));
					}
					for (byte j = 0; j < free; j++)
					{
						this.StreamRW.BinaryWriter.Write((short)-1);
						this.StreamRW.BinaryWriter.Write((short)0);
					}
				}
			}
			this.Sphere = new Vector4((float)middle.X, (float)middle.Y, (float)middle.Z, (float)radius);

			if ((flag & (int)Mesh3DComponentBits.Indices) > 0)
			{
				this.StreamRW.BinaryWriter.Write(this.Indices.Length);
				for (int i = 0; i < this.Indices.Length; i++)
					this.StreamRW.BinaryWriter.Write(this.Indices[i]);
			}
			base.GenerateBinary();
		}

		public void BufferBinary(long offset)
		{
			int flag = this.StreamRW.BinaryReader.ReadInt32();
			this.MeshFlag = (Mesh3DComponentBits)flag;
			this.PrimitiveType = (OpenTK.Graphics.OpenGL.PrimitiveType)this.StreamRW.BinaryReader.ReadInt32();

			var vertexCount = this.StreamRW.BinaryReader.ReadInt32();

			var vertexStride = POSITION_SIZE;
			var positionsOffset = 0;
			var textureCoordinatesOffset = POSITION_SIZE;
			var normalsOffset = POSITION_SIZE;
			var colorsOffset = POSITION_SIZE;
			var infsCountOffset = POSITION_SIZE;


			this.StreamRW.BaseStream.Position += POSITION_SIZE;

			if ((flag & (int)Mesh3DComponentBits.TextureCoords) > 0)
			{
				normalsOffset += TEXTURECOORDINATE_SIZE;
				colorsOffset += TEXTURECOORDINATE_SIZE;
				infsCountOffset += TEXTURECOORDINATE_SIZE;
				vertexStride += TEXTURECOORDINATE_SIZE;
				this.StreamRW.BaseStream.Position += TEXTURECOORDINATE_SIZE;
			}
			if ((flag & (int)Mesh3DComponentBits.Normals) > 0)
			{
				colorsOffset += NORMAL_SIZE;
				infsCountOffset += NORMAL_SIZE;
				vertexStride += NORMAL_SIZE;
				this.StreamRW.BaseStream.Position += NORMAL_SIZE;
			}
			if ((flag & (int)Mesh3DComponentBits.Colors) > 0)
			{
				infsCountOffset += COLOR_SIZE;
				vertexStride += COLOR_SIZE;
				this.StreamRW.BaseStream.Position += COLOR_SIZE;
			}
			byte maxInfCount = this.StreamRW.BinaryReader.ReadByte();
			if ((flag & (int)Mesh3DComponentBits.Influences) > 0)
			{
				vertexStride += sizeof(byte) + INFLUENCE_WEIGHT_SIZE * maxInfCount;
			}


			this.StreamRW.BaseStream.Position = offset + 12;
			this.VertexBinary = this.StreamRW.BinaryReader.ReadBytes(vertexCount * vertexStride);

			if ((flag & (int)Mesh3DComponentBits.Indices) > 0)
			{
				if (this.Generated == false)
				{
					this.Indices = new short[this.StreamRW.BinaryReader.ReadInt32()];
					for (int i = 0; i < this.Indices.Length; i++)
						this.Indices[i] = this.StreamRW.BinaryReader.ReadInt16();
				}
				else
				{
					this.StreamRW.BaseStream.Position += 4;
					this.StreamRW.BaseStream.Position += this.Indices.Length * 2;
				}

				if (true)
				{
					int tipCounts = 3;
					if (this.PrimitiveType == OpenTK.Graphics.OpenGL.PrimitiveType.Quads)
						tipCounts = 4;

					for (int i = 0; i < this.Indices.Length; i++)
					{
						if (i > 0 && (i % tipCounts) == 0)
						{
							int ind0 = this.Indices[i - tipCounts];
							int ind1 = this.Indices[i - tipCounts + 1];
							int ind2 = this.Indices[i - tipCounts + 2];


							float x0_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind0 + positionsOffset + 0);
							float y0_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind0 + positionsOffset + 4);
							float z0_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind0 + positionsOffset + 8);

							float x1_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind1 + positionsOffset + 0);
							float y1_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind1 + positionsOffset + 4);
							float z1_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind1 + positionsOffset + 8);

							float x2_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind2 + positionsOffset + 0);
							float y2_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind2 + positionsOffset + 4);
							float z2_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind2 + positionsOffset + 8);

							if (tipCounts == 3)
								this.Area += CalculateTriangleArea(x0_, y0_, z0_, x1_, y1_, z1_, x2_, y2_, z2_);
							else
							{
								int ind3 = this.Indices[i - tipCounts + 3];

								float x3_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind3 + positionsOffset + 0);
								float y3_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind3 + positionsOffset + 4);
								float z3_ = System.BitConverter.ToSingle(this.VertexBinary, vertexStride * ind3 + positionsOffset + 8);

								this.Area += CalculateTriangleArea(x0_, y0_, z0_, x1_, y1_, z1_, x2_, y2_, z2_);
								this.Area += CalculateTriangleArea(x0_, y0_, z0_, x2_, y2_, z2_, x3_, y3_, z3_);
							}
						}
					}
				}
			}


			VertexBufferObject = GL.GenBuffer();
			VertexArrayObject = GL.GenVertexArray();

			GL.BindVertexArray(VertexArrayObject);
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, vertexCount * vertexStride, this.VertexBinary, BufferUsageHint.StaticDraw);

			if (this.Indices.Length > 0)
			{
				IndexBufferObject = GL.GenBuffer();
				GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.IndexBufferObject);
				GL.BufferData(BufferTarget.ElementArrayBuffer, this.Indices.Length * sizeof(short), this.Indices, BufferUsageHint.StaticDraw);
				this.PrimitiveCount = this.Indices.Length;
			}
			else
				this.PrimitiveCount = vertexCount;

			this.BinaryFlag = flag;

			this.shader = Shader.FlaggedShaders[flag & ~(int)Mesh.Mesh3DComponentBits.Indices];

			/* SECOND PASS */

			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexStride, positionsOffset);
			GL.EnableVertexAttribArray(0);

			if ((flag & (int)Mesh3DComponentBits.TextureCoords) > 0)
			{
				GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexStride, textureCoordinatesOffset);
				GL.EnableVertexAttribArray(1);
			}

			if ((flag & (int)Mesh3DComponentBits.Normals) > 0)
			{
				GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexStride, normalsOffset);
				GL.EnableVertexAttribArray(2);
			}

			if ((flag & (int)Mesh3DComponentBits.Colors) > 0)
			{
				GL.VertexAttribPointer(3, 4, VertexAttribPointerType.UnsignedByte, true, vertexStride, colorsOffset);
				GL.EnableVertexAttribArray(3);
			}
			if ((flag & (int)Mesh3DComponentBits.Influences) > 0)
			{
				GL.VertexAttribIPointer(8, 1, VertexAttribIntegerType.UnsignedByte, vertexStride, (IntPtr)(infsCountOffset));
				GL.EnableVertexAttribArray(8);

				for (int i = 0; i < maxInfCount; i++)
				{
					GL.VertexAttribIPointer(9 + i, 1, VertexAttribIntegerType.UnsignedInt, vertexStride, (IntPtr)(infsCountOffset + 1 + i * INFLUENCE_WEIGHT_SIZE));
					GL.EnableVertexAttribArray(9 + i);
				}
			}

			base.BufferBinary();
		}

		public bool SkipRender;
		public Mesh Next;

		public static void Query(Dictionary<string, int> QueryUniforms, List<object> QueryUniformsArrays, int handle)
		{
			for (int i = 0; i < QueryUniforms.Count; i++)
			{
				var el = QueryUniforms.ElementAt(i);
				int val = el.Value;
				if (handle == GLControl.AbsoluteShader)
				{
					val = GL.GetUniformLocation(handle, el.Key);
				}
				else
				if (val < 0)
				{
					QueryUniforms[el.Key] = GL.GetUniformLocation(handle, el.Key);
				}
				if (val > -1)
				{
					if (QueryUniformsArrays[i] is bool)
						GL.Uniform1(val, ((bool)QueryUniformsArrays[i]) ? 1 : 0);
					else if (QueryUniformsArrays[i] is int)
						GL.Uniform1(val, (int)QueryUniformsArrays[i]);
					else if (QueryUniformsArrays[i] is Vector4)
						GL.Uniform4(val, (Vector4)QueryUniformsArrays[i]);
					else if (QueryUniformsArrays[i] is Vector2)
						GL.Uniform2(val, (Vector2)QueryUniformsArrays[i]);
					else if (QueryUniformsArrays[i] is float)
						GL.Uniform1(val, (float)QueryUniformsArrays[i]);
				}

			}
		}

		public Dictionary<string, int> QueryUniforms = new Dictionary<string, int>(0);
		public List<object> QueryUniformsArrays = new List<object>(0);

		//public static float CalculateTriangleArea(Assimp.Vector3D pointA, Assimp.Vector3D pointB, Assimp.Vector3D pointC)
		public static float CalculateTriangleArea(
			float ax, float ay, float az,
			float bx, float by, float bz,
			float cx, float cy, float cz)
		{
			Vector3 a = new Vector3(ax, ay, az);
			Vector3 b = new Vector3(bx, by, bz);
			Vector3 c = new Vector3(cx, cy, cz);

			float sideA = Vector3.Distance(a, b);
			float sideB = Vector3.Distance(b, c);
			float sideC = Vector3.Distance(c, a);
			float semiPerimeter = (sideA + sideB + sideC) / 2;

			return (float)Math.Sqrt(semiPerimeter * (semiPerimeter - sideA) * (semiPerimeter - sideB) * (semiPerimeter - sideC));
		}

		public void Draw(Object3D object3D, int handle)
		{
			if (this.shader == null)
				return;

			if (this.SkipRender == false)
			{
				this.shader.Use(object3D.TextureMaterials[this.MaterialIndex], handle);

				Query(this.QueryUniforms, this.QueryUniformsArrays, handle);

				GL.BindVertexArray(VertexArrayObject);
				if (IndexBufferObject > 0)
				{
					GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.IndexBufferObject);
					GL.DrawElements(this.PrimitiveType, PrimitiveCount, DrawElementsType.UnsignedShort, 0);
				}
				else
					GL.DrawArrays(this.PrimitiveType, 0, PrimitiveCount);
			}
#if MESH_RENDER_MODE_RECURSIVE
			if (this.Next != null)
			{
				this.Next.Draw(object3D, fogMode);
			}
#endif
		}
	}
}
