using Assimp;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static BDxGraphiK.Moveset;

namespace BDxGraphiK
{
	public class AnimationBinary
	{
		public Matrix4[] AnimationData;
		public Matrix4[] ReversedAnimationData;

		public int FrameCount = 0;
		public int LoopFrame = 0;

		public AnimationBinary(string filename, Object3D referenceModel)
		{
			using (BinaryReader binaryReader = new BinaryReader(new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read)))
			{
				this.FrameCount = binaryReader.ReadInt32();
				this.LoopFrame = binaryReader.ReadInt32();

				/*if (this.LoopFrame < this.FrameCount)
					this.LoopFrame++;*/

				binaryReader.BaseStream.Position = 16;

				int count = (int)binaryReader.BaseStream.Length - 16;
				int countMatrices = count / 64;
				AnimationData = new Matrix4[countMatrices];
				ReversedAnimationData = new Matrix4[countMatrices];

				byte[] matricesBytes = binaryReader.ReadBytes(count);

				Marshal.Copy(matricesBytes, 0, Marshal.UnsafeAddrOfPinnedArrayElement(AnimationData, 0), count);
				Marshal.Copy(matricesBytes, 0, Marshal.UnsafeAddrOfPinnedArrayElement(ReversedAnimationData, 0), count);

				referenceModel.Skeleton.ReverseComputedMatrices(ref ReversedAnimationData, -1);
			}
		}



		public void ExportDAE(string inputfilename, string outputfilename, Object3D referenceModel)
		{
			List<string> output = new List<string>(0);

			string[] lines_input = File.ReadAllLines(inputfilename);
			bool library_met = false;
			foreach (string line_input in lines_input)
			{
				if (library_met == false && line_input.Contains("<library_"))
				{
					library_met = true;
					string[] lines = File.ReadAllLines(@"sample_animation.dae");
					output.Add(lines[0]);
					for (int i = 0; i < referenceModel.Skeleton.Joints.Count; i++)
					{
						int sourcetype = 0;
						for (int j = 1; j < lines.Length - 1; j++)
						{
							if (lines[j].Contains("</source>"))
								sourcetype++;
							int indexKokkara = lines[j].IndexOf("kokkarahazime");
							if (indexKokkara > -1)
							{
								string tabs = lines[j].Substring(0, indexKokkara);
								string online = tabs;
								switch ((ColladaAnimationSourceType)sourcetype)
								{
									case ColladaAnimationSourceType.TIME:
										for (int k = 0; k < this.FrameCount; k++)
										{
											online += Math.Round(k * 0.0333333333333f, 6).ToString("0.000000") + " ";
										}
										output.Add(online);
										break;
									case ColladaAnimationSourceType.MATRICES:
										for (int k = 0; k < this.FrameCount; k++)
										{
											Matrix4 mat = this.ReversedAnimationData[k * referenceModel.Skeleton.Joints.Count + i];

											//output.Add(tabs + mat.M11.ToString("0.000000") + " " + mat.M21.ToString("0.000000") + " " + mat.M31.ToString("0.000000") + " " + mat.M41.ToString("0.000000"));
											//output.Add(tabs + mat.M12.ToString("0.000000") + " " + mat.M22.ToString("0.000000") + " " + mat.M32.ToString("0.000000") + " " + mat.M42.ToString("0.000000"));
											//output.Add(tabs + mat.M13.ToString("0.000000") + " " + mat.M23.ToString("0.000000") + " " + mat.M33.ToString("0.000000") + " " + mat.M43.ToString("0.000000"));
											//output.Add(tabs + mat.M14.ToString("0.000000") + " " + mat.M24.ToString("0.000000") + " " + mat.M34.ToString("0.000000") + " " + mat.M44.ToString("0.000000"));

											output.Add(tabs +
											mat.M11.ToString("0.000000") + " " + mat.M21.ToString("0.000000") + " " + mat.M31.ToString("0.000000") + " " + mat.M41.ToString("0.000000") + " " +
											mat.M12.ToString("0.000000") + " " + mat.M22.ToString("0.000000") + " " + mat.M32.ToString("0.000000") + " " + mat.M42.ToString("0.000000") + " " +
											mat.M13.ToString("0.000000") + " " + mat.M23.ToString("0.000000") + " " + mat.M33.ToString("0.000000") + " " + mat.M43.ToString("0.000000") + " " +
											mat.M14.ToString("0.000000") + " " + mat.M24.ToString("0.000000") + " " + mat.M34.ToString("0.000000") + " " + mat.M44.ToString("0.000000"));
										}
										break;
									case ColladaAnimationSourceType.INTERPOLATION:
										for (int k = 0; k < this.FrameCount; k++)
										{
											online += "LINEAR" + " ";
										}
										output.Add(online);
										break;
								}
							}
							else
							{
								int countEquals = lines[j].IndexOf("count=\"");
								if (countEquals > -1)
								{
									countEquals += 7;
									if ((ColladaAnimationSourceType)sourcetype == ColladaAnimationSourceType.MATRICES)
									{
										if (lines[j].Contains("stride=\""))
										{
											output.Add(lines[j].Insert(countEquals, this.FrameCount.ToString()).Replace("joint1", referenceModel.Skeleton.Joints[i].Name));
										}
										else
											output.Add(lines[j].Insert(countEquals, (this.FrameCount * 16).ToString()).Replace("joint1", referenceModel.Skeleton.Joints[i].Name));
									}
									else
										output.Add(lines[j].Insert(countEquals, this.FrameCount.ToString()).Replace("joint1", referenceModel.Skeleton.Joints[i].Name));
								}
								else
									output.Add(lines[j].Replace("joint1", referenceModel.Skeleton.Joints[i].Name));
							}
						}
					}
					output.Add(lines[lines.Length - 1]);

				}
				output.Add(line_input);
			}
			File.WriteAllLines(outputfilename, output.ToArray());
		}

	}
}
