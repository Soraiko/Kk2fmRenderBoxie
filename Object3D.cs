//#define MESH_RENDER_MODE_RECURSIVE <-- doesn't work for many mesh models
using Assimp;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using System.Xml.Linq;
using static BDxGraphiK.Mesh;
using static OpenTK.Graphics.OpenGL.GL;
using static SrkAlternatives.Mdlx;


namespace BDxGraphiK
{
	public class Object3D:BinableObject
	{
		public Dictionary<string, object> Variables = new Dictionary<string, object>(0);

		public static Texture whitePixel1x1;
		public static Texture bumpPixel1x1;

		public List<Mesh> Meshes = new List<Mesh>(0);
		public List<TextureMaterial> TextureMaterials = new List<TextureMaterial>(0);
		public Skeleton Skeleton = new Skeleton();


		public new void GenerateBinary()
		{
			//this.StreamRW = new BinaryRW(new FileStream(@"C:\Users\Daniel\source\repos\BDxGraphiK\bin\Debug\content\P_EX100\P_EX100.bin", FileMode.CreateNew));
			this.StreamRW = new BinaryRW(new MemoryStream());
			this.StreamRW.BinaryWriter.Write((int)this.ObjectFlag);
			this.StreamRW.BinaryWriter.Write(this.Meshes.Count);
			for (int i = 0; i < this.Meshes.Count; i++)
			{
				this.StreamRW.BinaryWriter.Write(this.Meshes[i].MaterialIndex);
			}

			/* ###################### TextureMaterials ###################### */

			this.StreamRW.BinaryWriter.Write(this.TextureMaterials.Count);

			for (int i = 0; i < this.TextureMaterials.Count; i++)
			{
				for (int j = 0; j < TextureMaterial.COUNT; j++)
				{
					if (TextureMaterials[i].Textures[j].Integer > 0)
					{
						this.StreamRW.BinaryWriter.Write(j);
						byte[] nameBytes = Encoding.Unicode.GetBytes(TextureMaterials[i].Textures[j].Filename);
						this.StreamRW.BinaryWriter.Write((byte)nameBytes.Length);
						this.StreamRW.BinaryWriter.Write(nameBytes);
					}
				}
				this.StreamRW.BinaryWriter.Write(-1);
			}

			/* ###################### Skeleton ###################### */

			if (this.Skeleton != null)
			{
				this.Skeleton.StreamRW = this.StreamRW;
				this.Skeleton.GenerateBinary();
			}

			for (int i = 0; i < this.Meshes.Count; i++)
			{
				this.Meshes[i].StreamRW = this.StreamRW;
				this.Meshes[i].GenerateBinary();
			}

			base.GenerateBinary();
		}

		public Object3D Clone()
		{
			Object3D object3D = new Object3D();
			object3D.Variables = this.Variables;
			object3D.StreamRW = this.StreamRW;
			object3D.StreamRW.BaseStream.Position = 0;
			object3D.QueryUniforms = this.QueryUniforms;
			object3D.QueryUniformsArrays = this.QueryUniformsArrays;
			object3D.BufferBinary();
			return object3D;
		}

		public new void BufferBinary()
		{
			this.StreamRW.BaseStream.Position = 0;
			if (this.StreamRW.BinaryReader.ReadInt32() != (int)ASCII4.Model)
				throw new Exception("Not a model.");

			int meshCount = 0;
			List<int> meshIndices = new List<int>(0);

			if (this.Generated)
			{
				this.StreamRW.BaseStream.Position += 4; /* skip mesh count */
				this.StreamRW.BaseStream.Position += 4 * this.Meshes.Count; /* skip material indices */
				this.StreamRW.BaseStream.Position += 4; /* skip material count */
				for (int i=0;i< this.TextureMaterials.Count;i++)
				{
					for (int j = 0; j < TextureMaterial.COUNT; j++)
					{
						if (this.TextureMaterials[i].Textures[j].Integer > 0)
						{
							this.StreamRW.BaseStream.Position += 4; /* skip material type */
							this.StreamRW.BaseStream.Position += 1; /* skip texture filename count byte */
							this.StreamRW.BaseStream.Position += Encoding.Unicode.GetByteCount(this.TextureMaterials[i].Textures[j].Filename); /* skip texture filename count */
						}
					}
					this.StreamRW.BaseStream.Position += 4; /* skip stop flags */
				}
			}
			else
			{
				meshCount = this.StreamRW.BinaryReader.ReadInt32();
				for (int i = 0; i < meshCount; i++)
					meshIndices.Add(this.StreamRW.BinaryReader.ReadInt32());
				int materialCount = this.StreamRW.BinaryReader.ReadInt32();

				while (this.TextureMaterials.Count < materialCount)
				{
					TextureMaterial textureMaterial = new TextureMaterial("");
					while (true)
					{
						int textureType = this.StreamRW.BinaryReader.ReadInt32();
						if (textureType == -1)
						{
							break;
						}
						else
						{
							byte filenameLength = this.StreamRW.BinaryReader.ReadByte();
							textureMaterial.Textures[textureType] = Texture.LoadTexture(Encoding.Unicode.GetString(this.StreamRW.BinaryReader.ReadBytes(filenameLength)), null, TextureMinFilter.Linear, OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat);
						}
					}
					this.TextureMaterials.Add(textureMaterial);
				}
			}

			meshCount = 0;
			while (this.StreamRW.BaseStream.Position < this.StreamRW.BaseStream.Length)
			{
				int objectFlag = this.StreamRW.BinaryReader.ReadInt32();
				switch ((BinableObject.ASCII4)objectFlag)
				{
					case BinableObject.ASCII4.Skeleton:
						if (this.Generated)
							this.Skeleton.BufferBinary(this.StreamRW.BaseStream.Position);
						else
						{
							Skeleton skeleton = new Skeleton();
							skeleton.StreamRW = this.StreamRW;
							skeleton.BufferBinary(this.StreamRW.BaseStream.Position);
							this.Skeleton = skeleton;
						}
							break;
					case BinableObject.ASCII4.Mesh:
						if (this.Generated)
						{
							this.Meshes[meshCount].BufferBinary(this.StreamRW.BaseStream.Position);
							meshCount++;
						}
						else
						{
							Mesh mesh = new Mesh(this);
							mesh.StreamRW = this.StreamRW;
							mesh.BufferBinary(this.StreamRW.BaseStream.Position);
							mesh.MaterialIndex = meshIndices[meshCount];
							meshCount++;
							this.AddMesh(mesh);
						}
					break;
				}
			}

			base.BufferBinary();
		}

		public static Object3D FromBinary(string filename)
		{
			Object3D object3D = new Object3D();
			object3D.StreamRW = new BinaryRW(new FileStream(filename, FileMode.Open));
			object3D.BufferBinary();
			return object3D;
		}

		public Object3D()
		{

		}

		public Object3D(string filename)
		{
			this.ObjectFlag = ASCII4.Model;
			Assimp.AssimpContext assimpContext = new Assimp.AssimpContext();
			var scene = assimpContext.ImportFile(filename);
			Dictionary<string, TextureMaterial> materials = new Dictionary<string, TextureMaterial>(0);

			List<string> AnyBoneName = new List<string>(0);

			for (int i=0;i< scene.Meshes.Count;i++)
			{
				Assimp.Mesh mesh = scene.Meshes[i];
				for (int j = 0; j < mesh.BoneCount; j++)
				{
					if (AnyBoneName.Contains(mesh.Bones[j].Name) == false)
					{
						AnyBoneName.Add(mesh.Bones[j].Name);
					}
				}
			}
			if (AnyBoneName.Count > 0)
			{
				List<Assimp.Node> jointsNodes = new List<Assimp.Node>(0);
				for (int i = 0; i < scene.RootNode.Children.Count; i++)
				{
					var currNode = scene.RootNode.Children[i];
					for (int j = 0; j < AnyBoneName.Count; j++)
					if (currNode.FindNode(AnyBoneName[j]) != null)
					{
						jointsNodes.Add(currNode);
						break;
					}
				}
				this.Skeleton = new Skeleton();
				foreach (Assimp.Node node in jointsNodes)
					this.Skeleton.JointsTree.Add(RecursiveJointChildren(node));
			}

			List<string> bonesInlineNames = new List<string>(0);
			if (this.Skeleton != null)
				for (int i = 0; i < this.Skeleton.Joints.Count; i++)
					bonesInlineNames.Add(this.Skeleton.Joints[i].Name);

			for (int i=0;i< scene.Meshes.Count;i++)
			{
				Assimp.Mesh mesh = scene.Meshes[i];
				int[] indices_tab = mesh.GetIndices();
				int[] indicesPerTip_tab = new int[indices_tab.Length];

				List<List<Vector3D>> vertices_s = new List<List<Vector3D>>(0);
				List<List<Vector3D>> texcoords_s = new List<List<Vector3D>>(0);
				List<List<Vector3D>> normals_s = new List<List<Vector3D>>(0);
				List<List<Color4D>> colors_s = new List<List<Color4D>>(0);
				List<short[]> indices_s = new List<short[]>(0);
				List<List<List<short>>> influences_s = new List<List<List<short>>>(0);
				List<List<List<float>>> weights_s = new List<List<List<float>>>(0);
				List<OpenTK.Graphics.OpenGL.PrimitiveType> primitiveTypes = new List<OpenTK.Graphics.OpenGL.PrimitiveType>(0);

				List<int> material_indices_s = new List<int>(0);

				if (mesh.FaceCount > 0)
				{
					List<List<short>> influences = new List<List<short>>(0);
					List<List<float>> weights = new List<List<float>>(0);

					bool hasInfluences = false;

					for (int j = 0; j < mesh.Vertices.Count; j++)
					{
						influences.Add(new List<short>(0));
						weights.Add(new List<float>(0));
					}

					for (int j = 0; j < mesh.Bones.Count; j++)
					for (int k = 0; k < mesh.Bones[j].VertexWeightCount; k++)
					{
						hasInfluences = true;
						influences[mesh.Bones[j].VertexWeights[k].VertexID].Add((short)bonesInlineNames.IndexOf(mesh.Bones[j].Name));
						weights[mesh.Bones[j].VertexWeights[k].VertexID].Add(mesh.Bones[j].VertexWeights[k].Weight);
					}
					

					for (int j = 0; j < mesh.FaceCount; j++)
					{
						int currCount = mesh.Faces[j].IndexCount;
						for (int k = 0; k < mesh.Faces[j].IndexCount; k++)
							indicesPerTip_tab[mesh.Faces[j].Indices[k]] = currCount;
					}

					int lastTipCount = -1;
					if (indicesPerTip_tab.Length > 0)
						lastTipCount = indicesPerTip_tab[0];

					List<Vector3D> vertices = new List<Vector3D>(0);
					List<List<short>> influences_ = new List<List<short>>(0);
					List<List<float>> weights_ = new List<List<float>>(0);
					List<Vector3D> texcoords = new List<Vector3D>(0);
					List<Vector3D> normals = new List<Vector3D>(0);
					List<Color4D> colors = new List<Color4D>(0);
					List<short> indices = new List<short>(0);

					for (int j = 0; j < indicesPerTip_tab.Length+1; j++)
					{
						int currTipCount = -1;
						if (j< indicesPerTip_tab.Length)
							currTipCount = indicesPerTip_tab[j];

						if (currTipCount != lastTipCount)
						{
							vertices_s.Add(vertices);
							texcoords_s.Add(texcoords);
							normals_s.Add(normals);
							colors_s.Add(colors);
							indices_s.Add(indices.ToArray());
							influences_s.Add(influences_);
							weights_s.Add(weights_);

							switch (lastTipCount)
							{
								case 2: primitiveTypes.Add(OpenTK.Graphics.OpenGL.PrimitiveType.Lines); break;
								case 3: primitiveTypes.Add(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles); break;
								case 4: primitiveTypes.Add(OpenTK.Graphics.OpenGL.PrimitiveType.Quads); break;
							}

							if (scene.Materials[mesh.MaterialIndex].HasName == false)
								scene.Materials[mesh.MaterialIndex].Name = "material_" + DateTime.Now.ToBinary().ToString();

							int indexOf = materials.Keys.ToList().IndexOf(scene.Materials[mesh.MaterialIndex].Name);
							if (indexOf<0)
							{
								material_indices_s.Add(materials.Count);
								TextureMaterial material = new TextureMaterial(scene.Materials[mesh.MaterialIndex].Name);

								if (scene.Materials[mesh.MaterialIndex].HasTextureDiffuse)
									material.Textures[(int)TextureMaterial.TextureType.Diffuse] = Texture.LoadTexture(Texture.TestLateralPath(filename, scene.Materials[mesh.MaterialIndex].TextureDiffuse.FilePath), null, TextureMinFilter.Linear, OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat);
								
								if (scene.Materials[mesh.MaterialIndex].HasTextureHeight)
									material.Textures[(int)TextureMaterial.TextureType.BumpMapping] = Texture.LoadTexture(Texture.TestLateralPath(filename, scene.Materials[mesh.MaterialIndex].TextureHeight.FilePath), null, TextureMinFilter.Linear, OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat, OpenTK.Graphics.OpenGL.TextureWrapMode.Repeat);

								materials.Add(material.Name, material);
							}
							else
								material_indices_s.Add(indexOf);

							vertices = new List<Vector3D>(0);
							influences_ = new List<List<short>>(0);
							weights_ = new List<List<float>>(0);
							texcoords = new List<Vector3D>(0);
							normals = new List<Vector3D>(0);
							colors = new List<Color4D>(0);
							indices = new List<short>(0);
							if (currTipCount < 0)
								break;
						}

						int index = indices_tab[j];
						Vector3D curr_position = mesh.Vertices[index];

						Vector3D curr_texcoord = new Vector3D(Single.NaN,0,0);
						Vector3D curr_normal = new Vector3D(Single.NaN, 0,0);
						Color4D curr_color = new Color4D(Single.NaN, 1,1,1);

						if (mesh.HasTextureCoords(0))
							curr_texcoord = mesh.TextureCoordinateChannels[0][index] * new Vector3D(1f, -1f, 1f);

						if (mesh.HasNormals)
							curr_normal = mesh.Normals[index];

						if (mesh.HasVertexColors(0))
							curr_color = mesh.VertexColorChannels[0][index];

						int newIndex = vertices.Count;
						for (int m = 0; m < vertices.Count; m++)
						{
							if (curr_position.Equals(vertices[m]))
							{
								bool same = true;
								if (hasInfluences)
								{
									same = false;
									int matchCount = 0;
									for (int n = 0; n < influences[index].Count; n++)
									{
										if (influences[index][n] == influences_[m][n])
										{
											matchCount++;
										}
									}
									if (matchCount == influences_[m].Count)
									{
										matchCount = 0;
										for (int n = 0; n < weights[index].Count; n++)
										{
											if (weights[index][n] == weights_[m][n])
											{
												matchCount++;
											}
										}
										if (matchCount == weights_[m].Count)
											same = true;
									}
								}
								if (same && Single.IsNaN(curr_color.R) == false)
								{
									same = colors[m].Equals(curr_color);
								}
								if (same && Single.IsNaN(curr_normal.X) == false)
								{
									same = normals[m].Equals(curr_normal);
								}
								if (same && Single.IsNaN(curr_texcoord.X) == false)
								{
									same = texcoords[m].Equals(curr_texcoord);
								}
								if (same)
								{
									newIndex = m;
									break;
								}
							}
						}

						if (newIndex == vertices.Count)
						{
							vertices.Add(curr_position);
							influences_.Add(influences[index]);
							weights_.Add(weights[index]);

							if (Single.IsNaN(curr_texcoord.X) == false)
								texcoords.Add(curr_texcoord);

							if (Single.IsNaN(curr_normal.X) == false)
								normals.Add(curr_normal);

							if (Single.IsNaN(curr_color.R) == false)
								colors.Add(curr_color);
						}

						indices.Add((short)newIndex);

						lastTipCount = currTipCount;
					}
				}


				for (int j=0;j< indices_s.Count;j++)
				{
					Mesh m = new Mesh(this);

					m.Positions = vertices_s[j];
					m.TextureCoords = texcoords_s[j];
					m.Normals = normals_s[j];
					m.Colors = colors_s[j];
					m.Indices = indices_s[j];

					m.Influences = influences_s[j];
					m.Weights = weights_s[j];

					m.PrimitiveType = primitiveTypes[j];
					m.MaterialIndex = material_indices_s[j];

					this.AddMesh(m);
				}
			}
			this.TextureMaterials = materials.Values.ToList();
			this.GenerateBinary();
			this.BufferBinary();
		}

		public void AddMesh(Mesh mesh)
		{
			if (this.Meshes.Count > 0)
				this.Meshes[this.Meshes.Count - 1].Next = mesh;
			this.Meshes.Add(mesh);
		}

		public void AddShadowMesh(Mesh mesh)
		{
			for (int i=0;i< this.Meshes.Count;i++)
			{
				if (this.Meshes[i].ShadowMesh == null)
				{
					this.Meshes[i].ShadowMesh = mesh;
					if (i>0)
					{
						this.Meshes[i - 1].ShadowMesh.Next = mesh;
					}
					break;
				}
			}
		}

		public Joint RecursiveJointChildren(Assimp.Node parentNode)
		{
			Joint joint = new Joint(parentNode.Name);
			joint.Transform = Mathematics.ConvertMatrix(parentNode.Transform);
			joint.IndexInBuffer = this.Skeleton.Joints.Count;
			this.Skeleton.Joints.Add(joint);

			for (int j = 0; j < parentNode.ChildCount; j++)
			{
				Joint child = RecursiveJointChildren(parentNode.Children[j]);
				child.ComputedTransform *= joint.ComputedTransform;
				child.Parent = joint;
				joint.Children.Add(child);
			}
			return joint;
		}

		public Dictionary<string, int> QueryUniforms = new Dictionary<string, int>
		{
			["fog_mode"] = -1,
			["colormultiplicator"] = -1
		};

		public List<object> QueryUniformsArrays = new List<object>
		{
			(int)Mesh.Shader.FogMode.None,
			new Vector4(1f)
		};

		public Texture Shadow = Texture.whitePixel1x1;
		public Texture Draw(bool shadow)
		{
			var output = Texture.whitePixel1x1;

			if (this.Meshes.Count == 0)
				return output;

			int matrices_loc = GL.GetUniformBlockIndex(this.Meshes[0].shader.Handle, "transform_data");
			if (matrices_loc > -1)
			{
				GL.UniformBlockBinding(this.Meshes[0].shader.Handle, matrices_loc, 0);
				GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, this.Skeleton.UniformBufferObject);
			}

			if (this.Meshes.Count >0)
			{
				int handle = this.Meshes[0].shader.Handle;

				if (GLControl.AbsoluteShader > -1)
					handle = GLControl.AbsoluteShader;

				GL.UseProgram(handle);
				Query(this.QueryUniforms, this.QueryUniformsArrays, handle);

				for (int i = 0; i < this.Meshes.Count; i++)
				{
					this.Meshes[i].Draw(this, handle);
#if MESH_RENDER_MODE_RECURSIVE
					break;
#endif
				}
			}

			return output;
		}
	}
}
