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

		public MDLX(string filename)
		{
			FileStream fs = new FileStream(filename, FileMode.Open);
			this.mdlx = new SrkAlternatives.Mdlx(fs);
			this.mdlx.ExportDAE(@"D:\Users\Daniel\Desktop\daes\model.dae");

			for (int i = 0; i< mdlx.models.Count; i++)
			{
				Object3D currentModel = new Object3D();
				currentModel.ObjectFlag = Object3D.ASCII4.Model;
				currentModel.Meshes = new List<Mesh>(0);
				currentModel.TextureMaterials = new List<TextureMaterial>(0);


				for (int j = 0; j < mdlx.models[i].Textures.Count; j++)
				{
					var mat = new TextureMaterial("material" + j.ToString("d3"));
					mat.Textures[0] = 
					
					Texture.LoadTexture(
					Path.GetFileNameWithoutExtension(filename) +"["+i+"][texture" + j.ToString("d3") + ".png]",
					mdlx.models[i].Textures[j],
					OpenTK.Graphics.OpenGL.TextureMinFilter.Linear,
					mdlx.models[i].WrapModes[j][0],
					mdlx.models[i].WrapModes[j][1]);
					currentModel.TextureMaterials.Add(mat);
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
				}
				currentModel.QueryUniforms.Add("glow_mesh", -1);
				currentModel.QueryUniformsArrays.Add(mdlx.models[i].Name == "MAP");

				currentModel.GenerateBinary();
				currentModel.BufferBinary();
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



		public static void DrawMap(MDLX map, bool fogEnabled, GLControl sender)
		{
			if (fogEnabled && map.Variables.ContainsKey("BackColor"))
			{
				sender.BackColor = (Color)map.Variables["BackColor"];
				sender.FogColor = (Color)map.Variables["FogColor"];
				sender.FogNear = (float)map.Variables["FogNear"];
				sender.FogFar = (float)map.Variables["FogFar"];
				sender.FogMin = (float)map.Variables["FogMin"];
				sender.FogMax = (float)map.Variables["FogMax"];
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

		public class RAM_Model
		{
			public new string ToString()
			{
				return this.ObjentryModelName;
			}


			public List<BinaryWriter> binaryWriters = new List<BinaryWriter>(0);
			public BinaryWriter binaryWriter = null;

			/*public int PlayingANB_Address
			{
				get
				{
					int address = this.ProcessStream.ReadInt32(this.MemoryRegionAddress + 0x14C) - 0x100;
					byte[] buff = new byte[0x100];
					buff = this.ProcessStream.Read(address, 0x100);
					for (int i = 0xF0; i > 0; i -= 0x10)
					{
						if (buff[i] != 0x42) continue;
						if (buff[i + 1] != 0x41) continue;
						if (buff[i + 2] != 0x52) continue;
						if (buff[i + 3] != 0x01) continue;
						return address + i;
					}
					return -1;
				}
				set
				{
					byte[] buff = new byte[0x100];
					buff = this.ProcessStream.Read(value, 0x100);
					int count = BitConverter.ToInt32(buff, 4);
					for (int i = 0; i < count; i++)
					{
						if (BitConverter.ToInt16(buff, 0x10 + i * 0x10) == 9)
						{
							value = BitConverter.ToInt32(buff, 0x18 + i * 0x10);
							break;
						}
					}
					this.ProcessStream.WriteInt32(this.MemoryRegionAddress + 0x14C, value);
				}
			}*/

			public RAM_Model Keyblade = null;

			public int TransformAttachMemRegionPointer
			{
				get
				{
					return this.ProcessStream.ReadInt32(this.MemoryRegionAddress + 0x570);
				}
				set
				{
					this.ProcessStream.WriteInt32(this.MemoryRegionAddress + 0x570, value);
				}
			}

			public int TransformAttachBone
			{
				get
				{
					return this.ProcessStream.ReadInt32(this.MemoryRegionAddress + 0x574);
				}
				set
				{
					this.ProcessStream.WriteInt32(this.MemoryRegionAddress + 0x574, value);
				}
			}

			public int KeybladeMemRegionPointer
			{
				get
				{
					return this.ProcessStream.ReadInt32(this.MemoryRegionAddress + 0xAF8);
				}
				
			}

			public bool RenderShadow
			{
				get
				{
					return (this.ProcessStream.ReadByte(this.MemoryRegionAddress + 0x10d) & 0b10) == 0;
				}
			}
			public bool RenderMeshes
			{
				get
				{
					return (this.ProcessStream.ReadByte(this.MemoryRegionAddress + 0x10d) & 0b01) == 0;
				}
			}

			public int FallingHeaderIndex;

			public byte TransitingFlag
			{
				get { return this.ProcessStream.ReadByte(this.MemoryRegionAddress + 0x0C); }
				set { this.ProcessStream.WriteByte(this.MemoryRegionAddress + 0x0C, value); }
			}

			public float Opacity
			{
				get { return this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x834)  * this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x82C); }
				set { this.ProcessStream.WriteSingle(this.MemoryRegionAddress + 0x834, value); }
			}

			public int MotionTrigger_Address
			{
				get { return this.ProcessStream.ReadInt32(this.MemoryRegionAddress + 0x150); }
				set { this.ProcessStream.WriteInt32(this.MemoryRegionAddress + 0x150, value); }
			}

			public int FallingAnimation
			{
				get { return this.ProcessStream.ReadInt32(this.MemoryRegionAddress + 0x100); }
				set { this.ProcessStream.WriteInt32(this.MemoryRegionAddress + 0x100, value); }
			}

			public float FallingAcceleration
			{
				get { return this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x120); }
				set { this.ProcessStream.WriteSingle(this.MemoryRegionAddress + 0x120, value); }
			}

			public float AnimationMaxFrame
			{
				get { return this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x16C); }
				set { this.ProcessStream.WriteSingle(this.MemoryRegionAddress + 0x16C, value); }
			}

			public float AnimationFrame
			{
				get { return this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x170); }
				set { this.ProcessStream.WriteSingle(this.MemoryRegionAddress + 0x170, value); }
			}

			public float AnimationPlaybackStep
			{
				get { return this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x234); }
				set { this.ProcessStream.WriteSingle(this.MemoryRegionAddress + 0x234, value); }
			}

			public float PositionX
			{
				get { return this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x540); }
				set { this.ProcessStream.WriteSingle(this.MemoryRegionAddress + 0x540, value); }
			}
			public Vector3 SpawnPosition;

			public float PositionY
			{
				get { return this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x544); }
				set { this.ProcessStream.WriteSingle(this.MemoryRegionAddress + 0x544, value); }
			}
			public float PositionZ
			{
				get { return this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x548); }
				set { this.ProcessStream.WriteSingle(this.MemoryRegionAddress + 0x548, value); }
			}

			public float RotationY
			{
				get { return -this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x66C) + MathHelper.Pi; }
				set { this.ProcessStream.WriteSingle(this.MemoryRegionAddress + 0x66C, -(value- MathHelper.Pi)); }
			}

			float lastRotY = Single.NaN;

			public Vector4 ColorMultiplicator
			{
				get
				{
					byte[] vals = this.ProcessStream.Read(this.MemoryRegionAddress + 0x8BC, 4);
					return new Vector4(vals[0] / 128f, vals[1] / 128f, vals[2] / 128f, this.Opacity);
				}
			}

			public float PrincipalRotationY
			{
				get
				{
					double val = -this.ProcessStream.ReadSingle(this.MemoryRegionAddress + 0x55C) + MathHelper.Pi;
					val = Math.Atan2(Math.Sin(val), Math.Cos(val));

					if (Single.IsNaN(lastRotY) == false)
					{
						double difference = lastRotY - val;
						while (difference < 0)
							difference += 2 * Math.PI;
						while (difference >= 2 * Math.PI)
							difference -= 2 * Math.PI;
						if (difference <= Math.PI)
							val = (lastRotY - difference);
						else
							val = (lastRotY + (2 * Math.PI - difference));
					}
					lastRotY = (float)val;
					return lastRotY;
				}
			}

			public float GetPrincipalRotation(double val)
			{
				val = Math.Atan2(Math.Sin(val), Math.Cos(val));

				if (Single.IsNaN(lastRotY) == false)
				{
					double difference = lastRotY - val;
					while (difference < 0)
						difference += 2 * Math.PI;
					while (difference >= 2 * Math.PI)
						difference -= 2 * Math.PI;
					if (difference <= Math.PI)
						val = (lastRotY - difference);
					else
						val = (lastRotY + (2 * Math.PI - difference));
				}
				lastRotY = (float)val;
				return lastRotY;
			}

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

			static long mapRenderTick = 0;

			public static bool[] RememberFrustrum = new bool[0];

			public static void UpdateModel(RAM_Model modelRamModel, bool mapColorRegions)
			{
				for (int i = 0; i < modelRamModel.Model.Models.Count; i++)
				{
					List<Object3D> models = modelRamModel.Model.Models.ElementAt(i).Value;
					foreach (Object3D model in models)
					{
						for (int qu = 0; qu < model.QueryUniforms.Count; qu++)
						{
							var keypair = model.QueryUniforms.ElementAt(qu);
							if (keypair.Key == "colormultiplicator")
							{
								model.QueryUniformsArrays[qu] = mapColorRegions ? modelRamModel.ColorMultiplicator : Vector4.One;
							}
						}

					}
				}
			}

			public static void DrawModel(RAM_Model modelRamModel, GLControl sender)
			{
				for (int i=0;i< modelRamModel.Model.Models.Count;i++)
				{
					List<Object3D> models = modelRamModel.Model.Models.ElementAt(i).Value;
					foreach (Object3D model in models)
					{
						model.Draw(false);
					}
				}
			}

			public static void DrawMap(RAM_Model mapRamModel, GLControl sender, bool fogEnabled, CheckState frustrumCullingEnabled, Frustrum frustrum)
			{
				if (mapRamModel !=null)
				{
					long currTick = Program.glForm.totalTicks;
					if (currTick != mapRenderTick)
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
						if (mapRamModel.Model.Models.ContainsKey("MAP"))
						{
						}
						mapRenderTick = currTick;
					}
					MDLX.DrawMap(mapRamModel.Model, fogEnabled, sender);
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

			public bool Banned;

			public RAM_Model(SrkProcessStream processStream, int memoryRegionAddress)
			{
				this.Banned = false;
				int ramReadObjentryAddress = processStream.ReadInt32(memoryRegionAddress + 8);

				this.ProcessStream = processStream;
				this.MemoryRegionAddress = memoryRegionAddress;


				this.MDLXAddress = processStream.ReadInt32(memoryRegionAddress + 0x7AC);
				/*if (processStream.ReadString(this.MDLXAddress, 3, false) != "BAR")
				{
					this.Banned = true;
					AddToBans(ramReadObjentryAddress);
					return;
				}*/
				this.MSETAddress = processStream.ReadInt32(memoryRegionAddress + 0x140);


				/*if (processStream.ReadString(this.MSETAddress, 3, false) != "BAR")
					return;*/

				if (Directory.Exists("obj") == false)
					Directory.CreateDirectory("obj");

				string objentryModelName = "";
				string objentryMsetName = "";

				objentryModelName = "obj/" + processStream.ReadString(ramReadObjentryAddress + 8, 0x20, true) + ".mdlx";
				this.ObjentryModelName = objentryModelName;

				if (this.MSETAddress > 0)
					objentryMsetName = "obj/" + processStream.ReadString(ramReadObjentryAddress + 8 + 0x20, 0x20, true) + ".mset";

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
						this.Banned = true;
						AddToBans(ramReadObjentryAddress);
						return;
					}
				}

				if (ramModelsHistory.ContainsKey(objentryModelName))
				{
					var mdlxToClone = ramModelsHistory[objentryModelName].Model;
					this.Model = new MDLX();
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
					this.Banned = true;
					AddToBans(ramReadObjentryAddress);
					return;
				}

				if (ramModelsHistory.ContainsKey(objentryModelName) == false)
					ramModelsHistory.Add(objentryModelName, this);

				//Object3D model = this.Model.Models.ElementAt(0).Value[0];

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
					}
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
