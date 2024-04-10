using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using OpenTK;
using BDxGraphiK;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenKh;
using SrkAlternatives;
using static BDxGraphiK.MDLX;
using static SrkAlternatives.Mdlx;
using Assimp;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Windows.Forms;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Reflection;

namespace BDxGraphiK
{
	public class MDLX
	{
		public Dictionary<string, object> Variables = new Dictionary<string, object>(0);
		public Dictionary<string, List<Object3D>> Models = new Dictionary<string, List<Object3D>>(0);

		public SrkAlternatives.Mdlx mdlx;

		public MDLX()
		{

		}

		Bitmap[] Textures;
		Bitmap[] TexturePatches = new Bitmap[0];
		Rectangle[] Patch_DestinationRectangles;
		
		int[] Patch_Counts;
		public int[] Patch_DestinationTextureIndices;
		public int[] PatchOffsets;
		public int[] PatchSizes;

		public MDLX(string filename)
		{
			FileStream fs = new FileStream(filename, FileMode.Open);
			this.mdlx = new SrkAlternatives.Mdlx(fs);

			if (fs.Length < 1024 * 1024)
			{
				for (int i=0;i< this.mdlx.Files.Count;i++)
				{
					if (this.mdlx.Files[i].Type == 7)
					{
						MDLXTimEditor.MdlxTextures.FromTIMBytes(ref this.mdlx.Files[i].Data, true);

						this.Textures = MDLXTimEditor.MdlxTextures.Textures;
						this.TexturePatches = MDLXTimEditor.MdlxTextures.TexturePatches;
						this.PatchOffsets = MDLXTimEditor.MdlxTextures.PatchOffsets;
						this.PatchSizes = MDLXTimEditor.MdlxTextures.PatchOffsets;
						this.Patch_Counts = MDLXTimEditor.MdlxTextures.Patch_Counts;
						this.Patch_DestinationRectangles = MDLXTimEditor.MdlxTextures.Patch_DestinationRectangles;
						this.Patch_DestinationTextureIndices = MDLXTimEditor.MdlxTextures.Patch_DestinationTextureIndices;
						break;
					}
				}
			}
			


			for (int i = 0; i< mdlx.models.Count; i++)
			{
				Object3D currentModel = new Object3D();
				currentModel.ObjectFlag = Object3D.ASCII4.Model;
				currentModel.Meshes = new List<Mesh>(0);
				currentModel.TextureMaterials = new List<TextureMaterial>(0);

				int grayColors = 0;
				int allColors = 0;
				bool hasColor_ = false;

				for (int j = 0; j < mdlx.models[i].Textures.Count; j++)
				{
					Bitmap bmp = mdlx.models[i].Textures[j];

					var mat = new TextureMaterial("material" + j.ToString("d3"));
					mat.Textures[0] = 
					Texture.LoadTexture(
					Path.GetFileNameWithoutExtension(filename) +"["+i+"][texture" + j.ToString("d3") + ".png][-1]",
					bmp,
					OpenTK.Graphics.OpenGL.TextureMinFilter.Linear,
					mdlx.models[i].WrapModes[j][0],
					mdlx.models[i].WrapModes[j][1],
					true);

					currentModel.TextureMaterials.Add(mat);
				}

				for (int ti = 0; ti < currentModel.TextureMaterials.Count; ti++)
				{
					if (this.TexturePatches.Length > 0)
					{
						if (currentModel.TextureMaterials[ti].TexturePatches.Length == 0)
						{
							List<Texture> patches = new List<Texture>(0);

							for (int k = 0; k < this.TexturePatches.Length; k++)
							{
								if (this.Patch_DestinationTextureIndices[k] == ti)
								{
									int width = this.TexturePatches[k].Width;
									Texture t =
											Texture.LoadTexture(
											Path.GetFileNameWithoutExtension(filename) + "[" + i + "][texture" + ti.ToString("d3") + ".png][" + patches.Count + "]",
											this.TexturePatches[k],
											OpenTK.Graphics.OpenGL.TextureMinFilter.Linear,
											mdlx.models[i].WrapModes[ti][0],
											mdlx.models[i].WrapModes[ti][1],
											true);
									t.X = this.Patch_DestinationRectangles[k].X;
									t.Y = this.Patch_DestinationRectangles[k].Y;

									if (this.Patch_DestinationRectangles[k].Width < width)
									{
										t.Orientation = Texture.PatchOrientation.Horizontal;
									}
									else
									{
										t.Orientation = Texture.PatchOrientation.Vertical;
									}
									t.Count = this.Patch_Counts[k];

									patches.Add(t);
								}
							}

							TextureMaterial tm = currentModel.TextureMaterials[ti];
							tm.TexturePatches = patches.ToArray();
							currentModel.TextureMaterials[ti] = tm;
						}
					}
				}
				var skeleton = mdlx.models[i].Skeleton;
				if (skeleton != null)
				{
					skeleton.ComputeMatrices();
					currentModel.Skeleton = skeleton;
				}

				for (int j=0;j< mdlx.models[i].Meshes.Count + mdlx.models[i].ShadowMeshes.Count; j++)
				{
					Mdlx.Mesh alternativeMesh = null;
					bool shadow = j >= mdlx.models[i].Meshes.Count;

					if (shadow)
						alternativeMesh = mdlx.models[i].ShadowMeshes[j- mdlx.models[i].Meshes.Count];
					else
						alternativeMesh = mdlx.models[i].Meshes[j];



					bool hasController = alternativeMesh.influences.Count > 0 && alternativeMesh.influences[0].Length > 0;
					Mesh mesh = new Mesh(currentModel);
					mesh.PrimitiveType = OpenTK.Graphics.OpenGL.PrimitiveType.Triangles;

					if (alternativeMesh.TextureIndex < currentModel.TextureMaterials.Count)
					{
						mesh.MaterialIndex = alternativeMesh.TextureIndex;
					}

					List<List<KeyValuePair<short, float>>> weight_influences = new List<List<KeyValuePair<short, float>>>(0);
					List<Vector3> positions_unIndexed = new List<Vector3>(0);
					List<Vector2> textureCoords_unIndexed = new List<Vector2>(0);
					List<System.Drawing.Color> colors_unIndexed = new List<System.Drawing.Color>(0);

					if (hasController)
					{
						for (int k = 0; k < alternativeMesh.reverseVertices.Count; k++)
						{
							Vector3 v = Vector3.Zero;
							List<KeyValuePair<short, float>> weight_influence = new List<KeyValuePair<short, float>>(0);

							for (int l = 0; l < alternativeMesh.reverseVertices[k].Length; l++)
							{
								Vector4 v4 = alternativeMesh.reverseVertices[k][l];
								weight_influence.Add(new KeyValuePair<short, float>(alternativeMesh.influences[k][l], v4.W));
								if (mdlx.models[i].Skeleton != null)
								{
									v4 = Vector4.Transform(v4, mdlx.models[i].Skeleton.Joints[alternativeMesh.influences[k][l]].ComputedTransform);
								}
								v.X += v4.X;
								v.Y += v4.Y;
								v.Z += v4.Z;
							}
							weight_influence.Sort((x, y) => (x.Value.CompareTo(y.Value)));
							weight_influences.Add(weight_influence);
							positions_unIndexed.Add(v);
						}
					}
					else
					{
						for (int k = 0; k < alternativeMesh.reverseVertices.Count; k++)
						{
							positions_unIndexed.Add(alternativeMesh.reverseVertices[k][0].Xyz);
						}
					}

					List<ushort> tri = new List<ushort>(0);
					List<OpenTK.Vector2> texCoords_triBuffer = new List<OpenTK.Vector2>(0);
					List<System.Drawing.Color> colors_triBuffer = new List<System.Drawing.Color>(0);
					bool hasColor = alternativeMesh.colors.Count > 1;
					if (hasColor)
						hasColor_ = true;
					bool hasTextcoord = alternativeMesh.textureCoordinates.Count > 1;
					List<ushort> inputIndices = new List<ushort>(0);

					for (int k=0;k<alternativeMesh.triangleFlags.Count;k++)
					{
						System.Drawing.Color col = hasColor ? alternativeMesh.colors[k] : System.Drawing.Color.White;
						Vector2 texCoord = hasTextcoord ? alternativeMesh.textureCoordinates[k] : Vector2.Zero;
						tri.Add(alternativeMesh.vertexIndices[k]);
						texCoords_triBuffer.Add(texCoord);
						colors_triBuffer.Add(col);

						if (tri.Count > 3)
						{
							tri.RemoveAt(0);
							texCoords_triBuffer.RemoveAt(0);
							colors_triBuffer.RemoveAt(0);
						}

						if (alternativeMesh.triangleFlags[k] == 0x00 || alternativeMesh.triangleFlags[k] == 0x20)
						{
							inputIndices.Add(tri[0]);
							inputIndices.Add(tri[1]);
							inputIndices.Add(tri[2]);
							textureCoords_unIndexed.Add(texCoords_triBuffer[0]);
							textureCoords_unIndexed.Add(texCoords_triBuffer[1]);
							textureCoords_unIndexed.Add(texCoords_triBuffer[2]);
							colors_unIndexed.Add(colors_triBuffer[0]);
							colors_unIndexed.Add(colors_triBuffer[1]);
							colors_unIndexed.Add(colors_triBuffer[2]);
						}
						if (alternativeMesh.triangleFlags[k] == 0x00 || alternativeMesh.triangleFlags[k] == 0x30)
						{
							inputIndices.Add(tri[2]);
							inputIndices.Add(tri[1]);
							inputIndices.Add(tri[0]);
							textureCoords_unIndexed.Add(texCoords_triBuffer[2]);
							textureCoords_unIndexed.Add(texCoords_triBuffer[1]);
							textureCoords_unIndexed.Add(texCoords_triBuffer[0]);
							colors_unIndexed.Add(colors_triBuffer[2]);
							colors_unIndexed.Add(colors_triBuffer[1]);
							colors_unIndexed.Add(colors_triBuffer[0]);
						}
					}

					List<List<float>> weights = new List<List<float>>(0);
					List<List<short>> influences = new List<List<short>>(0);
					List<Vector3> positions = new List<Vector3>(0);
					List<Vector2> textureCoords = new List<Vector2>(0);
					List<Vector3> normals = new List<Vector3>(0);
					List<System.Drawing.Color> colors = new List<System.Drawing.Color>(0);

					List<short> outputIndices = new List<short>(0);
					for (int l=0;l<inputIndices.Count;l++)
					{
						int vertexIndex = inputIndices[l];
						Vector3 position = positions_unIndexed[vertexIndex];
						Vector2 textureCoord = textureCoords_unIndexed[l];
						Vector3 normal = Vector3.UnitZ;
						System.Drawing.Color color = System.Drawing.Color.White;
						if (hasColor)
							color = colors_unIndexed[l];

						List<KeyValuePair<short, float>> weight_influence = null;
						if (hasController)
							weight_influence = weight_influences[vertexIndex];

						int foundIndex = -1;
						for (int k = 0; k < positions.Count; k++)
						{
							if (colors[k].A != color.A) continue;
							if (colors[k].R != color.R) continue;
							if (colors[k].G != color.G) continue;
							if (colors[k].B != color.B) continue;
							if (Vector3.Distance(positions[k], position) > 0.0001) continue;
							if (Vector2.Distance(textureCoords[k], textureCoord) > 0.0001) continue;
							if (Vector3.Distance(normals[k], normal) > 0.0001) continue;
							if (hasController && influences.Count > 0)
							{
								if (influences[k].Count != weight_influence.Count)
									continue;
								bool cont = false;
								for (int m = 0; m < influences[k].Count; m++)
								{
									if (influences[k][m] != weight_influence[m].Key)
									{
										cont = true;
										break;
									}
									if (Math.Abs(weights[k][m] - weight_influence[m].Value) > 0.01)
									{
										cont = true;
										break;
									}
								}
								if (cont)
									continue;
							}
							foundIndex = k;
							break;
						}
						if (foundIndex < 0)
						{
							foundIndex = positions.Count;
							positions.Add(position);
							textureCoords.Add(textureCoord);
							colors.Add(color);
							if (hasColor && (color.R != color.G || color.G != color.B))
							{
								grayColors++;
							}
							allColors++;
							normals.Add(normal);
							if (hasController)
							{
								List<short> influence = new List<short>(0);
								List<float> weight = new List<float>(0);

								for (int k = 0; k < weight_influence.Count; k++)
								{
									influence.Add(weight_influence[k].Key);
									weight.Add(weight_influence[k].Value);
								}
								influences.Add(influence);
								weights.Add(weight);
							}
						}
						outputIndices.Add((short)foundIndex);
					}
					mesh.Indices = outputIndices.ToArray();
					for (int p = 0; p < positions.Count; p++)
					{
						mesh.Positions.Add(new Assimp.Vector3D(positions[p].X, positions[p].Y, positions[p].Z));
						//mesh.Normals.Add(new Assimp.Vector3D(0,0,0));
					}
					float denominateur = 255f;
					if (mdlx.models[i].DoubleOpacities[mesh.MaterialIndex])
						denominateur = 128f;

					if (hasColor)
						for (int c = 0; c < colors.Count; c++)
							mesh.Colors.Add(new Assimp.Color4D(colors[c].R / 255f, colors[c].G / 255f, colors[c].B / 255f, colors[c].A / denominateur));

					for (int t = 0; t < textureCoords.Count; t++)
						mesh.TextureCoords.Add(new Assimp.Vector3D(textureCoords[t].X, textureCoords[t].Y, 0f));

					if (hasController)
					{
						mesh.Influences.AddRange(influences);
						mesh.Weights.AddRange(weights);
					}

					if (j >= mdlx.models[i].Meshes.Count)
						currentModel.AddShadowMesh(mesh);
					else
						currentModel.AddMesh(mesh);

					/*if (true)
					{
						mesh.QueryUniforms.Add("glow_mesh", -1);
						mesh.QueryUniformsArrays.Add(mdlx.models[i].DoubleOpacities[mesh.MaterialIndex]);
					}*/
				}
				if (this.Models.ContainsKey(mdlx.models[i].Name) == false)
					this.Models[mdlx.models[i].Name] = new List<Object3D>(0);

				this.Models[mdlx.models[i].Name].Add(currentModel);

				for (int qu = 0; qu < currentModel.QueryUniforms.Count; qu++)
				{
					var keypair = currentModel.QueryUniforms.ElementAt(qu);
					if (keypair.Key == "fog_mode")
					{
						currentModel.QueryUniformsArrays[qu] = mdlx.models[i].Name == "SK0" ?
							(int)Mesh.Shader.FogMode.None : (int)Mesh.Shader.FogMode.XYZ;
					}
					if (keypair.Key == "has_patch")
					{
						currentModel.QueryUniformsArrays[qu] = this.TexturePatches.Length > 0;
					}
					if (keypair.Key == "colormultiplicator")
					{
						if (hasColor_ && grayColors / (float)allColors > 0.1)
						{
							/*Console.WriteLine("");
							Console.WriteLine(filename);
							Console.WriteLine(grayColors / (float)allColors);*/
							currentModel.Variables["ignore_multiplicator"] = true;
						}
					}
				}


				currentModel.GenerateBinary();
				currentModel.BufferBinary();
				foreach (Mesh m in currentModel.Meshes)
				{
					m.QueryUniforms.Add("glow_mesh", -1);
					m.QueryUniformsArrays.Add(mdlx.models[i].Name == "MAP" && m.Area < 400000);
				}
			}
			

			bool map = false;
			foreach (Bar barFile in mdlx.Files)
			{
				if (barFile.Name == "MAP")
				{
					map = true;
					break;
				}
			}
			if (map)
			{
				this.Variables["frustrum_bytes"] = new List<byte[]>(0);
				this.Variables["frustrum_counts"] = new List<int>(0);
				this.Variables["frustrum_dirty"] = true;
				foreach (Bar barFile in mdlx.Files)
				{
					if (barFile.Type == (ushort)BuilderMdlx.BAR.EntryType.FogColor)
					{
						if (barFile.Data.Length >= 0x55)
						{
							this.Variables["BackColor"] = System.Drawing.Color.FromArgb(255, barFile.Data[0x00], barFile.Data[0x01], barFile.Data[0x02]);
							this.Variables["FogColor"] = System.Drawing.Color.FromArgb(255, barFile.Data[0x44 + 0x00], barFile.Data[0x44 + 0x01], barFile.Data[0x44 + 0x02]);
							this.Variables["FogNear"] = System.BitConverter.ToSingle(barFile.Data, 0x48);
							this.Variables["FogFar"] = System.BitConverter.ToSingle(barFile.Data, 0x4C);
							this.Variables["FogMin"] = System.BitConverter.ToSingle(barFile.Data, 0x50);
							this.Variables["FogMax"] = System.BitConverter.ToSingle(barFile.Data, 0x54);
						}
						break;
					}
				}
				if (this.Variables.ContainsKey("BackColor") == false)
				{
					this.Variables["BackColor"] = null;
				}
			}
			fs.Close();
		}



		public static void DrawMap(MDLX map, bool showMap, bool fogEnabled, GLControl sender)
		{

			if (showMap)
			{
				if (map.Models.ContainsKey("SK0"))
				{
					var sk0s = map.Models["SK0"];
					foreach (Object3D obj in sk0s)
						obj.Draw(false);
				}

				if (map.Models.ContainsKey("SK1"))
				{
					var sk1s = map.Models["SK1"];
					foreach (Object3D obj in sk1s)
						obj.Draw(false);
				}

				if (map.Models.ContainsKey("MAP"))
				{
					var maps = map.Models["MAP"];
					foreach (Object3D obj in maps)
						obj.Draw(false);
				}
			}
		}

		public class RAM_Model
		{
			public const int GRAPHICS_ACTIVITY_PTR = 0x0032BAD8;
			public const int FRAME_SKIP = 0x00349E1C;
			public const int CAMERA_TARGET = 0x00348754;

			public new string ToString()
			{
				return this.ObjentryModelName;
			}

			public RAM_Model Keyblade = null;


			public SrkProcessStream ProcessStream;
			public int MemoryRegionAddress;
			public string ObjentryModelName;

			public int MDLXAddress;
			public int MSETAddress;

			List<byte[]> ANBs;
			public MDLX Model;


			public int ScalesBufferAddress;
			public int RotatesBufferAddress;
			public int TranslatesBufferAddress;
			public int MatrixBufferAddress;

			public int BonesCount;


			public static bool[] RememberFrustrum = new bool[0];
			public byte[] MemRegionData;

			public static Vector3 UpAxises = new Vector3(Vector3.UnitX.X, -Vector3.UnitY.Y, -Vector3.UnitZ.Z);

			public Vector4 ColorMultiplicator;

			public Vector3 RootTransform = new Vector3(Single.NaN);


			public OpenTK.Quaternion WorldRotation = new OpenTK.Quaternion(Single.NaN, Single.NaN, Single.NaN, Single.NaN);
			public int ANBOffset;

			public float Frame = Single.NaN;
			public float MaxFrame = Single.NaN;



			public bool ReadMemRegionData(Object3D model)
			{
				this.ProcessStream.Read(this.MemoryRegionAddress, ref this.MemRegionData);

				int ramReadObjentryAddress = System.BitConverter.ToInt32(this.MemRegionData, 0x08);
				if (ramReadObjentryAddress > 0x01C80000 && ramReadObjentryAddress < 0x01D00000)
				{
					string objentryModelName = "obj/" + this.ProcessStream.ReadString(ramReadObjentryAddress + 8, 0x20, true) + ".mdlx";
					if (this.ObjentryModelName != objentryModelName)
						return false;
				}
				else
					return false;

				int ptr = System.BitConverter.ToInt32(this.MemRegionData, 0x670);
				if (ptr > this.MemoryRegionAddress && ptr < this.MemoryRegionAddress + 0x01000000)
				{
					if (this.MatrixBufferAddress > 0 && this.MatrixBufferAddress != this.ProcessStream.ReadInt32(ptr + 0x28))
					{
						model.Skeleton.MatricesBuffer = new float[model.Skeleton.MatricesBuffer.Length];
						model.Skeleton.SendMatricesToUniformObject();
						return false;
					}
				}
				return true;
			}


			public void UpdateWithMemregionData(MDLX mdlx, Object3D model, bool transformModels, bool texturePatches)
			{
				/*if (this.ObjentryModelName.Contains("GENTL") == false)
					return;*/


				int tim2Offsets = System.BitConverter.ToInt32(this.MemRegionData, 0x670);
				int mdlx_tim2Offset = this.ProcessStream.ReadInt32(tim2Offsets+0x34);
				int size = this.ProcessStream.ReadInt32(mdlx_tim2Offset + 0x04)*2;

				int buffered_tim2Offset = this.ProcessStream.ReadInt32(tim2Offsets+0x38);
				byte[] timData = new byte[size];
				this.ProcessStream.Read(buffered_tim2Offset, ref timData);


				for (int qu = 0; qu < model.QueryUniforms.Count; qu++)
				{
					var keypair = model.QueryUniforms.ElementAt(qu);

					if (keypair.Key == "has_patch")
					{
						model.QueryUniformsArrays[qu] = texturePatches && mdlx.TexturePatches.Length > 0;
					}
				}
				if (texturePatches && mdlx.TexturePatches != null && mdlx.TexturePatches.Length > 0)
				{
					List<string>[] uniformNames = new List<string>[model.TextureMaterials.Count];
					List<int>[] uniformVals = new List<int>[model.TextureMaterials.Count];

					for (int i = 0; i < size; i += 0x10)
					{
						int offset2 = System.BitConverter.ToInt32(timData, 0x0C + i);
						if (offset2 > mdlx_tim2Offset && offset2 < mdlx_tim2Offset + 0x1000000)
						{
							int offset1 = System.BitConverter.ToInt32(timData, 0x08 + i);
							if (offset1 > mdlx_tim2Offset && offset1 < mdlx_tim2Offset + 0x1000000)
							{
								int tIndex = -1;
								int val = this.ProcessStream.ReadInt16(offset2) & 0b1111;
								int pIndex = this.ProcessStream.ReadInt16(offset2 + 6);

								if (val > 0)
									pIndex--;

								offset2 -= mdlx_tim2Offset;

								for (int j = 0; tIndex < 0 && j < mdlx.PatchOffsets.Length; j++)
								{
									if (offset2 > mdlx.PatchOffsets[j] && offset2 < mdlx.PatchOffsets[j] + 0x3000)
									{
										tIndex = mdlx.Patch_DestinationTextureIndices[j];
									}
								}
								if (tIndex > -1)
								{
									if (uniformNames[tIndex] == null)
									{
										uniformNames[tIndex] = new List<string>(0);
										uniformVals[tIndex] = new List<int>(0);
									}

									uniformNames[tIndex].Add("patch" + uniformNames[tIndex].Count + "_index");
									uniformVals[tIndex].Add(pIndex);
								}
							}
						}
					}
					for (int j = 0; j < model.Meshes.Count; j++)
					{
						Mesh m = model.Meshes[j];
						var uniformsList = m.QueryUniforms.Keys.ToList();

						if (uniformNames[m.MaterialIndex] != null)
						{
							for (int k = 0; k < uniformNames[m.MaterialIndex].Count; k++)
							{
								int indexOf = uniformsList.IndexOf(uniformNames[m.MaterialIndex][k]);
								if (indexOf > -1)
								{
									m.QueryUniformsArrays[indexOf] = uniformVals[m.MaterialIndex][k];
								}
							}
						}
					}
				}

				float opacity = System.BitConverter.ToSingle(this.MemRegionData, 0x834) * System.BitConverter.ToSingle(this.MemRegionData, 0x82C) * this.MemRegionData[0x3AC+3]/128f;
				int ANBOffset = System.BitConverter.ToInt32(this.MemRegionData, 0x14C);
				float Frame = System.BitConverter.ToSingle(this.MemRegionData, 0x170);
				float MaxFrame = System.BitConverter.ToSingle(this.MemRegionData, 0x16C);

				int positionInfo_offset = this.ProcessStream.ReadInt32(ANBOffset + 0x90 + 0x2C);

				bool[] positionInfo_hasCurveIndex = new bool[3 + 3 + 3];
				Vector3[] positionInfo_scale_rot_trans = new Vector3[3];
				bool hasOffset = false;

				if (positionInfo_offset > 0)
				{
					byte[] positionInfoBytes = new byte[0x60];
					this.ProcessStream.Read(positionInfo_offset + ANBOffset + 0x90, ref positionInfoBytes);
					hasOffset = System.BitConverter.ToInt32(positionInfoBytes, 0x0C) > 0;
					for (int i = 0; i < positionInfo_hasCurveIndex.Length; i++)
					{
						positionInfo_hasCurveIndex[i] = System.BitConverter.ToInt32(positionInfoBytes, 0x30 + i * 4) > -1;
						positionInfo_scale_rot_trans[i / 3][i % 3] = System.BitConverter.ToSingle(positionInfoBytes, (i / 3) * 0x10 + (i % 3) * 0x04);
					}
				}


				/*double readDouble = System.BitConverter.ToSingle(this.MemRegionData, 0x55C) + MathHelper.Pi;
				readDouble = Math.Atan2(Math.Sin(readDouble), Math.Cos(readDouble));

				if (Single.IsNaN(this.WorldRotation.Y) == false)
				{
					double difference = this.WorldRotation.Y - readDouble;
					while (difference < 0) difference += 2 * Math.PI;
					while (difference >= 2 * Math.PI) difference -= 2 * Math.PI;
					if (difference <= Math.PI) readDouble = (this.WorldRotation.Y - difference); else readDouble = (this.WorldRotation.Y + (2 * Math.PI - difference));
				}*/

				Matrix4[] transform = new Matrix4[1];


				Array.Copy(System.BitConverter.GetBytes(-1f*System.BitConverter.ToSingle(this.MemRegionData, 0x40 + 0x34)), 0, this.MemRegionData, 0x40 + 0x34, 4);
				Array.Copy(System.BitConverter.GetBytes(-1f*System.BitConverter.ToSingle(this.MemRegionData, 0x40 + 0x38)), 0, this.MemRegionData, 0x40 + 0x38, 4);

				Marshal.Copy(this.MemRegionData, 0x40, Marshal.UnsafeAddrOfPinnedArrayElement(transform, 0), 0x40);

				OpenTK.Quaternion rotation = transform[0].ExtractRotation();
				rotation = new OpenTK.Quaternion(rotation.X, -rotation.Y, -rotation.Z, rotation.W);


				Matrix4[] matrices = new Matrix4[this.BonesCount];

				Marshal.Copy(this.MemRegionData, this.MatrixBufferAddress-this.MemoryRegionAddress + this.BonesCount * 0x40, Marshal.UnsafeAddrOfPinnedArrayElement(matrices, 0), this.BonesCount * 0x40);


				bool ingame = this.ProcessStream.ReadInt32(MDLX.RAM_Model.CAMERA_TARGET) > 0;

				rotation *= new OpenTK.Quaternion(0f, System.BitConverter.ToSingle(this.MemRegionData, 0x1EC), 0f);

				Vector3 rootTransform = new Vector3(
				System.BitConverter.ToSingle(this.MemRegionData, 0x1E0),
				System.BitConverter.ToSingle(this.MemRegionData, 0x1E4),
				System.BitConverter.ToSingle(this.MemRegionData, 0x1E8));

				Vector3 transTest = matrices[0].ExtractTranslation();

				if (ingame)
				{
					for (int i = 0; i < 3; i++)
					{
						if (Math.Abs(this.RootTransform[i]) < 0.000001)
							transTest[i] = 0;
					}
					bool fixPos = (hasOffset == false || ANBOffset != this.ANBOffset);

					if (hasOffset && ANBOffset == this.ANBOffset && Vector3.Distance(this.RootTransform, rootTransform) > positionInfo_scale_rot_trans[2].Length/10f)
						fixPos = true;
					if (fixPos && (transTest + this.RootTransform).Length < this.RootTransform.Length / 10f)
						rootTransform = this.RootTransform;
					for (int m = 0; m < this.BonesCount; m++)
					{
						this.Matrices[m] = matrices[m] * Matrix4.CreateTranslation(rootTransform);
					}
				}
				else
				{
					for (int m = 0; m < this.BonesCount; m++)
					{
						this.Matrices[m] = matrices[m] * Matrix4.CreateTranslation(UpAxises * -rootTransform);
					}
				}

				model.Skeleton.ReverseComputedMatrices(ref this.Matrices, 0);


				int transformAttachMemRegionPointer = System.BitConverter.ToInt32(this.MemRegionData, 0x570);
				int transformAttachBone = System.BitConverter.ToInt32(this.MemRegionData, 0x574);

				if (transformAttachMemRegionPointer > 0 && transformModels == false)
				{
					rotation = new OpenTK.Quaternion(0,0,0);
					this.Transform = Matrix4.Identity;
				}
				else
					this.Transform = Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(transform[0].ExtractTranslation());

				if (transformModels && transformAttachMemRegionPointer > 0)
				{
					int[] modelsMemRegionsArray = Program.glForm.modelsMemRegions.ToArray();
					MDLX.RAM_Model[] modelsArray = Program.glForm.models.ToArray();

					//return (this.ProcessStream.ReadByte(this.MemoryRegionAddress + 0x10d) & 0b01) == 0;

					for (int i = 0; i < modelsMemRegionsArray.Length; i++)
					{
						var model_ = modelsArray[i];
						if (modelsMemRegionsArray[i] == transformAttachMemRegionPointer)
						{
							/*if (model_.ObjentryModelName.Contains("/W_") == false || System.BitConverter.ToInt32(model_.MemRegionData, 0xAF8) == this.MemoryRegionAddress)
							{
								int attachBone = transformAttachBone;
								if (attachBone > model_.BonesCount)
								{

								}
								else
								{
									this.Transform = model_.Matrices[attachBone] * model_.Transform;
								}
							}*/
							opacity *= model_.ColorMultiplicator.W;
							break;
						}
					}
					if (ANBOffset == 0)
					{
						opacity = 0;
						this.Transform = Matrix4.CreateScale(0f);
					}



					if ((this.MemRegionData[0x10D] & 0b10) > 0)
					{
						opacity = 0;
						this.Transform = Matrix4.CreateScale(0f);
					}
				}

				this.RootTransform = rootTransform;

				model.Skeleton.ComputeMatrices(ref this.Matrices, 0);


				this.ANBOffset = ANBOffset;
				this.Frame = Frame;
				this.MaxFrame = MaxFrame;

				this.ColorMultiplicator = new Vector4(this.MemRegionData[0x8BC + 0x00] / 128f, this.MemRegionData[0x8BC + 0x01] / 128f, this.MemRegionData[0x8BC + 0x02] / 128f, opacity);
				this.WorldRotation = rotation;
			}

			public Matrix4[] Matrices;
			public Matrix4 Transform;
			public int lastANBForInterpolate;


			public static bool UpdateModel(RAM_Model modelRamModel, bool mapDiffuseRegions, bool meshSkipRenders, bool transformModels, bool texturePatches)
			{
				for (int i = 0; i < modelRamModel.Model.Models.Count; i++)
				{
					List<Object3D> models = modelRamModel.Model.Models.ElementAt(i).Value;
					foreach (Object3D model in models)
					{
						int memRegion = modelRamModel.MemoryRegionAddress;

						if (modelRamModel.ReadMemRegionData(model))
							modelRamModel.UpdateWithMemregionData(modelRamModel.Model, model, transformModels, texturePatches);
						else
							return false;

						int totalMeshesCount = model.Meshes.Count;
						for (int m=0;m<model.Meshes.Count;m++)
						{
							if (model.Meshes[m].ShadowMesh != null)
								totalMeshesCount++;
						}
						if (totalMeshesCount > 0)
						{
							int pos = modelRamModel.ScalesBufferAddress - memRegion - 0x10;
							int totalCount_ = totalMeshesCount;
							while (totalCount_ > 16)
							{
								totalCount_ -= 16;
								pos -= 16;
							}

							/*int motionTriggerOffset = System.BitConverter.ToInt32(modelRamModel.MemRegionData, 0x150);
							if (motionTriggerOffset > 0)
							{
								byte[] motionTriggerBytes = new byte[0x1000];
								modelRamModel.ProcessStream.Read(motionTriggerOffset, ref motionTriggerBytes);
								byte countSecondGroup = motionTriggerBytes[1];
								int offsetSecondGroup = System.BitConverter.ToInt16(motionTriggerBytes, 0x02);

								for (int s=0;s< countSecondGroup; s++)
								{
									bool apppear = motionTriggerBytes[offsetSecondGroup + 2] == 0x1B;
									if ((motionTriggerBytes[offsetSecondGroup + 2] == 0x1A || apppear) && motionTriggerBytes[offsetSecondGroup + 3] > 0)
									{
										int whichMesh = System.BitConverter.ToInt16(motionTriggerBytes, offsetSecondGroup + 0x04);
										if (whichMesh<model.Meshes.Count)
										{
											modelRamModel.MemRegionData[pos + whichMesh] = (byte)(apppear?1:0);
										}
									}
									offsetSecondGroup += 4 + motionTriggerBytes[offsetSecondGroup+3]*2;
								}
							}*/

							for (int m = 0; m < model.Meshes.Count; m++)
							{
								model.Meshes[m].SkipRender = (pos < 0 || pos + m > modelRamModel.MemRegionData.Length - 1) || (meshSkipRenders && modelRamModel.MemRegionData[pos+m] == 0);
							}
						}

						for (int qu = 0; qu < model.QueryUniforms.Count; qu++)
						{
							var keypair = model.QueryUniforms.ElementAt(qu);
							if (keypair.Key == "colormultiplicator")
							{
								model.QueryUniformsArrays[qu] = model.Variables.ContainsKey("ignore_multiplicator")==false&& mapDiffuseRegions ? modelRamModel.ColorMultiplicator : new Vector4(Vector3.One, modelRamModel.ColorMultiplicator.W);
							}
						}
						break;
					}
					return models.Count > 0;
				}
				return false;
			}

			public static void DrawModel(RAM_Model ramModel, GLControl sender, bool interframeInterpolate, bool transformModels)
			{
				for (int i=0;i< ramModel.Model.Models.Count;i++)
				{
					List<Object3D> models = ramModel.Model.Models.ElementAt(i).Value;
					foreach (Object3D model in models)
					{
						if (sender.RenderStep == 0)
						{
							for (int m=0;m< ramModel.BonesCount;m++)
							{
								if (transformModels)
								{
									if (ramModel.lastANBForInterpolate == ramModel.ANBOffset && interframeInterpolate)
									{
										Matrix4 a = model.Skeleton.Joints[m].ComputedTransform;
										Matrix4 b = (ramModel.Matrices[m] * ramModel.Transform);

										Vector3 scale_a = a.ExtractScale();
										Vector3 scale_b = b.ExtractScale();

										OpenTK.Quaternion rotate_a = a.ExtractRotation();
										OpenTK.Quaternion rotate_b = b.ExtractRotation();

										Vector3 translate_a = a.ExtractTranslation();
										Vector3 translate_b = b.ExtractTranslation();


										model.Skeleton.Joints[m].ComputedTransform =
											Matrix4.CreateScale(scale_b) *
											Matrix4.CreateFromQuaternion(OpenTK.Quaternion.Slerp(rotate_a, rotate_b, 0.5f)) *
											Matrix4.CreateTranslation(translate_b);


									}
									else
									{
										model.Skeleton.Joints[m].ComputedTransform = (ramModel.Matrices[m] * ramModel.Transform );
									}
								}
								else
								{
									model.Skeleton.Joints[m].ComputedTransform = (model.Skeleton.InitialTransforms[m] * ramModel.Transform);
								}
							}

							model.Skeleton.PassComputedTransforms();
							model.Skeleton.SendMatricesToUniformObject();
							ramModel.lastANBForInterpolate = ramModel.ANBOffset;
						}
						model.Draw(false);
						break;
					}
				}
			}

			public static void DrawMap(RAM_Model mapRamModel, GLControl sender, bool showMap, bool fogEnabled, CheckState frustrumCullingEnabled, Frustrum frustrum)
			{
				if (mapRamModel !=null)
				{
					if (sender.RenderStep == 0)
					{
						if (frustrum != null)
						{
							for (int m=0;m< mapRamModel.Model.Models.Count;m++)
							{
								var currModel = mapRamModel.Model.Models.ElementAt(m).Value;
								List<Mesh> meshes = currModel[0].Meshes;

								bool[] frustrums = new bool[meshes.Count];
								if (frustrumCullingEnabled != CheckState.Unchecked)
								{
									if (RememberFrustrum.Length == 0)
										RememberFrustrum = new bool[meshes.Count];

									if (mapRamModel.Model.Models.ElementAt(m).Key == "MAP")
									{
										int mapDetailsPtr = mapRamModel.ProcessStream.ReadInt32(0x01D9EAAC);
										if (mapRamModel.ProcessStream.ReadUInt32(mapDetailsPtr + 0x25587) == 0x80808080)
										{
											byte[] meshBytes = Program.glForm.stream.Read(mapDetailsPtr + 0x25700, meshes.Count);

											for (int i = 0; i < meshBytes.Length; i++)
											{
												if (meshBytes[i] > 0)
												{
													RememberFrustrum[i] = true;
												}
											}
										}
										for (int i = 0; i < meshes.Count; i++)
										{
											if (frustrumCullingEnabled == CheckState.Indeterminate)
												frustrums[i] =  RememberFrustrum[i] == false;
											else
												frustrums[i] =  RememberFrustrum[i] == false || frustrum.SphereInFrustum(meshes[i].Sphere) == false;
										}
									}
									else
									{
										for (int i = 0; i < meshes.Count; i++)
										{
											if (frustrumCullingEnabled == CheckState.Indeterminate)
												frustrums[i] = false;
											else
												frustrums[i] = frustrum.SphereInFrustum(meshes[i].Sphere) == false;
										}
									}
								}
								for (int i = 0; i < meshes.Count; i++)
								{
									meshes[i].SkipRender = frustrumCullingEnabled != CheckState.Unchecked && frustrums[i];
								}
							}
						}
						if (fogEnabled && mapRamModel.Model.Variables.ContainsKey("BackColor") && ((Color)mapRamModel.Model.Variables["FogColor"]).Name != "ff808080")
						{
							sender.BackColor = (Color)mapRamModel.Model.Variables["BackColor"];
							sender.FogColor = (Color)mapRamModel.Model.Variables["FogColor"];
							sender.FogNear = (float)mapRamModel.Model.Variables["FogNear"];
							sender.FogFar = (float)mapRamModel.Model.Variables["FogFar"];
							sender.FogMin = (float)mapRamModel.Model.Variables["FogMin"];
							sender.FogMax = (float)mapRamModel.Model.Variables["FogMax"];
						}
						else
						{
							sender.BackColor = Color.Black;
							sender.FogColor = Color.Transparent;
							sender.FogNear = Single.NaN;
							sender.FogFar = 100f;
							sender.FogMin = 0f;
							sender.FogMax = 100f;
						}
					}
					MDLX.DrawMap(mapRamModel.Model, showMap, fogEnabled, sender);
				}
			}

			static Dictionary<string, RAM_Model> ramModelsHistory = new Dictionary<string, RAM_Model>();

			public static RAM_Model LoadMAP(SrkProcessStream processStream, int mapAddress, string filename)
			{
				RAM_Model ram_Model = new RAM_Model();
				ram_Model.ProcessStream = processStream;
				ram_Model.MDLXAddress = mapAddress;

				if (Directory.Exists("map\\jp") == false)
					Directory.CreateDirectory("map\\jp");

				if (File.Exists(filename) == false)
				{
					OpenKh.ProcessStream modelDumpProcessStream = new OpenKh.ProcessStream(processStream.BaseProcess);
					modelDumpProcessStream.BaseOffset = processStream.BaseOffset;
					modelDumpProcessStream.Position = mapAddress;
					SrkAlternatives.Mdlx mdlx = new SrkAlternatives.Mdlx(modelDumpProcessStream);
					mdlx.Save(filename);
				}
				if (ramModelsHistory.ContainsKey(filename))
				{
					ram_Model.Model = ramModelsHistory[filename].Model;
					ram_Model.Model.Variables = ramModelsHistory[filename].Model.Variables;
				}
				else
				{
					ram_Model.Model = new MDLX(filename);
					ramModelsHistory.Add(filename, ram_Model);
				}

				return ram_Model;
			}

			public RAM_Model()
			{

			}
			public static List<string> ram_ban_log;
			public static List<int> ram_ban_log_integers;
			public static int ObjentryAddress = 0x1C94100;

			static RAM_Model()
			{
				if (Directory.Exists("resources/MDLX") == false)
				{
					Directory.CreateDirectory("resources/MDLX");
					File.WriteAllLines("resources/MDLX/ram_ban_objects.log", new string[0]);
				}
				ram_ban_log = File.ReadAllLines("resources/MDLX/ram_ban_objects.log").ToList();
				ram_ban_log_integers = new List<int>(0);
				for (int i=0;i<ram_ban_log.Count;i++)
				{
					ram_ban_log_integers.Add(Convert.ToInt32(ram_ban_log[i], 16));
				}
			}

			public bool Aborted;

			public RAM_Model(SrkProcessStream processStream, int memoryRegionAddress)
			{
				if (processStream.ReadInt32(memoryRegionAddress + 0x148) != memoryRegionAddress)
				{
					this.Aborted = true;
					return;
				}
					
				int ramReadObjentryAddress = processStream.ReadInt32(memoryRegionAddress + 8);

				this.ProcessStream = processStream;
				this.MemoryRegionAddress = memoryRegionAddress;

				this.MDLXAddress = processStream.ReadInt32(memoryRegionAddress + 0x7AC);
				if (processStream.ReadString(this.MDLXAddress, 3, false) != "BAR")
				{
					this.Aborted = true;
					return;
				}
				this.MSETAddress = processStream.ReadInt32(memoryRegionAddress + 0x140);


				if (this.MSETAddress>0 && processStream.ReadString(this.MSETAddress, 3, false) != "BAR")
				{
					this.Aborted = true;
					return;
				}

				if (Directory.Exists("obj") == false)
					Directory.CreateDirectory("obj");

				string objentryModelName = "";
				string objentryMsetName = "";

				objentryModelName = "obj/" + processStream.ReadString(ramReadObjentryAddress + 8, 0x20, true) + ".mdlx";
				this.ObjentryModelName = objentryModelName;

				if (this.MSETAddress > 0)
					objentryMsetName = "obj/" + processStream.ReadString(ramReadObjentryAddress + 8 + 0x20, 0x20, true);

				if (File.Exists(objentryModelName) == false)
				{
					OpenKh.ProcessStream modelDumpProcessStream = new OpenKh.ProcessStream(processStream.BaseProcess);
					modelDumpProcessStream.BaseOffset = processStream.BaseOffset;
					modelDumpProcessStream.Position = this.MDLXAddress;
					try
					{
						SrkAlternatives.Mdlx mdlx = new SrkAlternatives.Mdlx(modelDumpProcessStream);
						mdlx.Save(objentryModelName);
					}
					catch
					{
						this.Aborted = true;
						return;
					}
				}

				if (ramModelsHistory.ContainsKey(objentryModelName))
				{
					var mdlxToClone = ramModelsHistory[objentryModelName].Model;
					this.Model = new MDLX();
					this.Model.Textures = mdlxToClone.Textures;
					this.Model.TexturePatches = mdlxToClone.TexturePatches;
					this.Model.PatchOffsets = mdlxToClone.PatchOffsets;
					this.Model.PatchSizes = mdlxToClone.PatchOffsets;
					this.Model.Patch_Counts = mdlxToClone.Patch_Counts;
					this.Model.Patch_DestinationRectangles = mdlxToClone.Patch_DestinationRectangles;
					this.Model.Patch_DestinationTextureIndices = mdlxToClone.Patch_DestinationTextureIndices;
					for (int i = 0; i < mdlxToClone.Models.Count; i++)
					{
						var keyPair = mdlxToClone.Models.ElementAt(i);
						List<Object3D> liste = keyPair.Value;
						List<Object3D> objects = new List<Object3D>(0);
						for (int j = 0; j < liste.Count; j++)
							objects.Add(liste[j].Clone());

						this.Model.Models.Add(keyPair.Key, objects);
					}
				}
				else
				{
					this.Model = new MDLX(objentryModelName);
				}


				//this.Model.Variables Ajouter variables 

				if (this.Model.Models.Count == 0 ||
					this.Model.Models.ElementAt(0).Value == null ||
					this.Model.Models.ElementAt(0).Value.Count == 0 ||
					this.Model.Models.ElementAt(0).Value[0].Skeleton == null ||
					this.Model.Models.ElementAt(0).Value[0].Skeleton.Joints.Count == 0)
				{
					this.Aborted = true;
					return;
				}

				if (ramModelsHistory.ContainsKey(objentryModelName) == false)
					ramModelsHistory.Add(objentryModelName, this);


				if (this.MSETAddress > 0 && Directory.Exists(objentryMsetName) == false)
					Directory.CreateDirectory(objentryMsetName);

				int ptr = processStream.ReadInt32(memoryRegionAddress + 0x670);
				if (ptr > memoryRegionAddress && ptr < memoryRegionAddress + 0x01000000)
				{
					int ptra = processStream.ReadInt32(ptr + 0x1C);
					if (ptra > memoryRegionAddress && ptra < memoryRegionAddress + 0x01000000)
					{
						this.ScalesBufferAddress = ptra;
					}
					int ptrb = processStream.ReadInt32(ptr + 0x20);
					if (ptrb > memoryRegionAddress && ptrb < memoryRegionAddress + 0x01000000)
					{
						this.RotatesBufferAddress = ptrb;
					}
					int ptrc = processStream.ReadInt32(ptr + 0x24);
					if (ptrc > memoryRegionAddress && ptrc < memoryRegionAddress + 0x01000000)
					{
						this.TranslatesBufferAddress = ptrc;
					}

					int ptrd = processStream.ReadInt32(ptr + 0x28);
					if (ptrd > memoryRegionAddress && ptrd < memoryRegionAddress + 0x01000000)
					{
						this.MatrixBufferAddress = ptrd;
						this.BonesCount = this.Model.Models.ElementAt(0).Value[0].Skeleton.Joints.Count;
						this.Matrices = new Matrix4[this.BonesCount];
						this.MemRegionData = new byte[this.MatrixBufferAddress-this.MemoryRegionAddress + 0x40 * this.BonesCount * 2];
					}
					else
					{
						this.Aborted = true;
						return;
					}
				}
				else
				{
					this.Aborted = true;
					return;
				}

				/*
				int anbCount = processStream.ReadInt32(this.MSETAddress + 4);
				byte[] buffer = processStream.Read(this.MSETAddress, 0x10 + anbCount * 0x10);

				this.ANBs = new List<byte[]>(0);
				for (int i = 0; i < anbCount; i++)
				{
					byte[] nom_offset_headerOffset = new byte[12];

					Array.Copy(buffer, 0x14 + i * 0x10, nom_offset_headerOffset, 0, 4);
					for (int j = 0; j < 4; j++)
					{
						if (nom_offset_headerOffset[j] < 0x20)
							nom_offset_headerOffset[j] = 0x5F;
					}
					Array.Copy(buffer, 0x18 + i * 0x10, nom_offset_headerOffset, 4, 4);
					Array.Copy(BitConverter.GetBytes(0x18 + i * 0x10), 0, nom_offset_headerOffset, 8, 4);

					if (BitConverter.ToInt32(nom_offset_headerOffset, 4) <= 0) continue;

					bool append = true;

					for (int a = 0; a < this.ANBs.Count; a++)
					{
						if (this.ANBs[a][0x04 + 3] != nom_offset_headerOffset[0x04 + 3]) continue;
						if (this.ANBs[a][0x04 + 2] != nom_offset_headerOffset[0x04 + 2]) continue;
						if (this.ANBs[a][0x04 + 1] != nom_offset_headerOffset[0x04 + 1]) continue;
						if (this.ANBs[a][0x04 + 0] != nom_offset_headerOffset[0x04 + 0]) continue;

						append = false;
						if (this.ANBs[a][0x00] == 0x44 &&
							this.ANBs[a][0x01] == 0x55 &&
							this.ANBs[a][0x02] == 0x4D &&
							this.ANBs[a][0x03] == 0x4D &&
							nom_offset_headerOffset[0x00] != 0x44 &&
							nom_offset_headerOffset[0x01] != 0x55 &&
							nom_offset_headerOffset[0x02] != 0x4D &&
							nom_offset_headerOffset[0x03] != 0x4D)
						{
							this.ANBs[a] = nom_offset_headerOffset;
						}
						break;
					}

					if (append)
					{
						this.ANBs.Add(nom_offset_headerOffset);
					}
				}
				for (int a = 0; a < this.ANBs.Count; a++)
				{
					int anb_offset = BitConverter.ToInt32(this.ANBs[a], 4);
					int sub_bar_count = processStream.ReadInt32(anb_offset + 4);
					for (int b = 0; b < sub_bar_count; b++)
					{
						if (processStream.ReadInt16(anb_offset + 0x10 + b * 0x10) == 9)
						{
							int file9_address = processStream.ReadInt32(anb_offset + 0x18 + b * 0x10);
							int animation_header = file9_address + 0x90;

							int origin_translation_offset = processStream.ReadInt32(animation_header + 0x2C);
							if (origin_translation_offset > 0)
							{
								//processStream.Write(animation_header + origin_translation_offset, new byte[0x30], 0x30); dump : décalage avec centre (animation, à la fin de l'ANB)
							}
							break;
						}
					}
				}*/
			}

			public static void AddToBans(int objentryAddress)
			{
				if (ram_ban_log.Contains(objentryAddress.ToString("X8")) == false)
				{
					ram_ban_log.Add(objentryAddress.ToString("X8"));
					ram_ban_log_integers.Add(objentryAddress);
					File.WriteAllLines("resources/MDLX/ram_ban_objects.log", ram_ban_log);
				}
			}

			public int ANBs_Count
			{
				get { return this.ANBs.Count; }
			}

			public string GetANBName(int index)
			{
				if (index > this.ANBs.Count - 1)
					index = this.ANBs.Count - 1;
				return Encoding.ASCII.GetString(this.ANBs[index], 0, 4);
			}

			public int GetANBOffset(int index)
			{
				if (index > this.ANBs.Count - 1)
					index = this.ANBs.Count - 1;
				return System.BitConverter.ToInt32(this.ANBs[index], 4);
			}

			public int GetANBHeaderOffset(int index)
			{
				if (index > this.ANBs.Count - 1)
					index = this.ANBs.Count - 1;
				return System.BitConverter.ToInt32(this.ANBs[index], 8);
			}

		}
	}
}
