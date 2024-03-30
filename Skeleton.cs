using System;
using System.Text;
using OpenTK;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace BDxGraphiK
{
	public class Skeleton:BinableObject
	{
		const int MAX_JOINTS_BUFFER_COUNT = 512;
		public List<Joint> JointsTree;
		public List<Joint> Joints;
		public float[] MatricesBuffer;
		public int UniformBufferObject;
		public Matrix4 Transform;
		public Matrix4[] InitialTransforms;
		//public Matrix4d Transformd;

		public Skeleton()
		{
			this.ObjectFlag = ASCII4.Skeleton;
			this.JointsTree = new List<Joint>(0);
			this.Joints = new List<Joint>(0);
			this.Transform = Matrix4.Identity;
			//this.Transformd = Matrix4d.Identity;
		}

		public new void GenerateBinary()
		{
			if (this.Joints.Count==0)
				return;

			if (this.StreamRW == null)
				this.StreamRW = new BinaryRW();

			this.StreamRW.BaseStream.Position = this.StreamRW.BaseStream.Length;
			this.StreamRW.BinaryWriter.Write((int)this.ObjectFlag);
			this.StreamRW.BinaryWriter.Write(this.Joints.Count);
			for (int i = 0; i < this.Joints.Count; i++)
			{
				if (this.Joints[i].Parent != null)
					this.StreamRW.BinaryWriter.Write(this.Joints[i].Parent.IndexInBuffer);
				else
					this.StreamRW.BinaryWriter.Write(-1);

				byte[] nameBytes = Encoding.Unicode.GetBytes(this.Joints[i].Name);
				this.StreamRW.BinaryWriter.Write((byte)nameBytes.Length);
				this.StreamRW.BinaryWriter.Write(nameBytes);
				for (int j = 0; j < 16; j++)
					this.StreamRW.BinaryWriter.Write(this.Joints[i].Transform[j % 4, j / 4]);
			}
			base.GenerateBinary();
		}

		public void BufferBinary(long offset)
		{
			if (this.Generated)
			{
				this.StreamRW.BaseStream.Position += 4;
				for (int i = 0; i < this.Joints.Count; i++)
					this.StreamRW.BaseStream.Position += 4 + 1 + Encoding.Unicode.GetByteCount(this.Joints[i].Name) + 4 * 4 * sizeof(float);
			}
			else
			{
				this.StreamRW.BaseStream.Position = offset + 0;
				int bonesCount = this.StreamRW.BinaryReader.ReadInt32();
				List<int> parentIndices = new List<int>(0);

				for (int i = 0; i < bonesCount; i++)
				{
					parentIndices.Add(this.StreamRW.BinaryReader.ReadInt32());
					byte nameLength = this.StreamRW.BinaryReader.ReadByte();
					string name = Encoding.Unicode.GetString(this.StreamRW.BinaryReader.ReadBytes(nameLength));
					Matrix4 mat = Matrix4.Identity;
					for (int j = 0; j < 16; j++)
						mat[j % 4, j / 4] = this.StreamRW.BinaryReader.ReadSingle();

					Joint joint = new Joint(name);
					joint.Transform = mat;
					joint.IndexInBuffer = i;
					this.Joints.Add(joint);
				}
				for (int i = 0; i < bonesCount; i++)
				{
					if (parentIndices[i] < 0)
					{
						this.JointsTree.Add(this.Joints[i]);
					}
					else
					{
						this.Joints[i].Parent = this.Joints[parentIndices[i]];
						this.Joints[i].Parent.Children.Add(this.Joints[i]);
					}
				}
			}
			this.MatricesBuffer = new float[2 * MAX_JOINTS_BUFFER_COUNT * 4 * 4];
			this.InitialTransforms = new Matrix4[this.Joints.Count];

			this.ComputeMatrices();
			this.InitTransforms();

			this.UniformBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.UniformBuffer, this.UniformBufferObject);
			GL.BufferData(BufferTarget.UniformBuffer, MatricesBuffer.Length * sizeof(float), MatricesBuffer, BufferUsageHint.DynamicCopy);
		}

		public void ComputeMatrices()
		{
			for (int i = 0; i < this.Joints.Count; i++)
			{
				this.Joints[i].ComputedTransform = this.Joints[i].Transform * 1f;
				//this.Joints[i].ComputedTransformd = this.Joints[i].Transformd;

				if (this.Joints[i].Parent == null)
				{
					this.Joints[i].ComputedTransform *= this.Transform;
					//this.Joints[i].ComputedTransformd *= this.Transformd;
				}
				else
				{
					this.Joints[i].ComputedTransform *= this.Joints[i].Parent.ComputedTransform;
					//this.Joints[i].ComputedTransformd *= this.Joints[i].Parent.ComputedTransformd;
				}
			}
		}
		public void ComputeMatrices(ref Matrix4[] matrices, int startOffset)
		{
			for (int i = 0; i < this.Joints.Count; i++)
			{
				if (this.Joints[i].Parent == null)
				{
					matrices[startOffset + i] *= this.Transform;
				}
				else
				{
					matrices[startOffset + i] *= matrices[startOffset + this.Joints[i].Parent.IndexInBuffer];
				}
			}
		}

		public void ReverseComputedMatrices()
		{
			for (int i = 0; i < this.Joints.Count; i++)
			{
				this.Joints[i].Transform = this.Joints[i].ComputedTransform * 1f;
				//this.Joints[i].Transformd = this.Joints[i].ComputedTransformd;
				this.Joints[i].Dirty = true;
			}
			int dirtyCount;
			do
			{
				dirtyCount = this.Joints.Count;
				for (int i = 0; i < this.Joints.Count; i++)
				{
					if (this.Joints[i].Dirty)
					{
						int childrenDirtyCount = 0;
						for (int j = 0; j < this.Joints[i].Children.Count; j++)
						{
							if (this.Joints[i].Children[j].Dirty)
								childrenDirtyCount++;
						}
						if (childrenDirtyCount == 0) /* Children's children are all calculated already. */
						{
							if (this.Joints[i].Parent != null)
							{
								try
								{
									this.Joints[i].Transform *= Matrix4.Invert(this.Joints[i].Parent.ComputedTransform);
									//this.Joints[i].Transformd *= Matrix4d.Invert(this.Joints[i].Parent.ComputedTransformd);
								}
								catch
								{
									this.Joints[i].Transform = Matrix4.CreateScale(1f);
									//this.Joints[i].Transformd = Matrix4d.Identity;
								}
							}
							this.Joints[i].Dirty = false;
							dirtyCount--;
						}
					}
					else
						dirtyCount--;
				}
			}
			while (dirtyCount > 0);
		}

		public void ReverseComputedMatrices(ref Matrix4[] matrices, int startOffset)
		{
			var jointsCount = this.Joints.Count;

			for (int i = 0; i < jointsCount; i++)
				this.Joints[i].Dirty = true;


			int dirtyCount;
			do
			{
				dirtyCount = jointsCount;
				for (int i = 0; i < jointsCount; i++)
				{
					if (this.Joints[i].Dirty)
					{
						int childrenDirtyCount = 0;
						for (int j = 0; j < this.Joints[i].Children.Count; j++)
						{
							if (this.Joints[i].Children[j].Dirty)
								childrenDirtyCount++;
						}
						if (childrenDirtyCount == 0) /* Children's children are all calculated already. */
						{
							if (this.Joints[i].Parent != null)
							{
								if (startOffset<0)
								{
									int pos = 0;
									while (pos< matrices.Length)
									{
										try
										{
											matrices[pos + i] *= Matrix4.Invert(matrices[pos + this.Joints[i].Parent.IndexInBuffer]);
										}
										catch
										{
											matrices[pos + i] = Matrix4.CreateScale(1f);
										}
										pos += jointsCount;
									}
								}
								else
								{
									try
									{
										matrices[startOffset + i] *= Matrix4.Invert(matrices[startOffset + this.Joints[i].Parent.IndexInBuffer]);
									}
									catch
									{
										matrices[startOffset + i] = Matrix4.CreateScale(1f);
									}
								}
							}
							this.Joints[i].Dirty = false;
							dirtyCount--;
						}
					}
					else
						dirtyCount--;
				}
			}
			while (dirtyCount > 0);
		}


		public void InitTransforms()
		{
			for (int i = 0; i < this.Joints.Count; i++)
			{
				Matrix4 computed = this.Joints[i].ComputedTransform;
				this.InitialTransforms[i] = computed;
				Matrix4 inverse = Matrix4.Identity;

				try {inverse = Matrix4.Invert(computed);} catch {}
				for (int j = 0; j < 16; j++)
				{
					this.MatricesBuffer[i * 4 * 4 + j] = computed[j % 4, j / 4];
					this.MatricesBuffer[MAX_JOINTS_BUFFER_COUNT * 4 * 4 + i * 4 * 4 + j] = inverse[j % 4, j / 4];
				}
			}
		}

		public void ResetTransforms()
		{

			for (int i = 0; i < this.Joints.Count; i++)
				this.Joints[i].ComputedTransform = this.InitialTransforms[i] * this.Transform;
		}

		public void PassComputedTransforms()
		{
			for (int i = 0; i < this.Joints.Count; i++)
				for (int j = 0; j < 16; j++)
					this.MatricesBuffer[i * 4 * 4 + j] = this.Joints[i].ComputedTransform[j % 4, j / 4];
		}

		public void PassTransforms(ref Matrix4[] inputArray)
		{
			for (int i = 0; i < inputArray.Length; i++)
			{
				for (int j = 0; j < 16; j++)
					this.MatricesBuffer[i * 16 + j] = inputArray[i][j % 4, j / 4];
			}
		}

		public void SendMatricesToUniformObject()
		{
			GL.BindBuffer(BufferTarget.UniformBuffer, this.UniformBufferObject);
			IntPtr matricesPtr = GL.MapBuffer(BufferTarget.UniformBuffer, BufferAccess.WriteOnly);
			System.Runtime.InteropServices.Marshal.Copy(this.MatricesBuffer, 0, matricesPtr, this.Joints.Count * 4 * 4);
			GL.UnmapBuffer(BufferTarget.UniformBuffer);
		}
	}
}
