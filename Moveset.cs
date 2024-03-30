using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace BDxGraphiK
{
	public class Moveset
	{
		public enum ColladaAnimationSourceType
		{
			TIME = 0,
			MATRICES = 1,
			INTERPOLATION = 2
		}


		public Vector3 GetEulers(Matrix4 computedMatrix, Vector3 constraint)
		{
			/*Vector3 rotate;

			Matrix4 mq = Matrix4.CreateFromQuaternion(computedMatrix.ExtractRotation());
			double sy = Math.Sqrt(mq.M11 * mq.M11 + mq.M12 * mq.M12);
			bool singular = sy < 1e-6;
			if (!singular)
			{
				rotate.X = (float)(Math.Atan2(mq.M23, mq.M33));
				rotate.Y = (float)(Math.Atan2(-mq.M13, sy));
				rotate.Z = (float)(Math.Atan2(mq.M12, mq.M11));
			}
			else
			{
				rotate.X = (float)(Math.Atan2(-mq.M32, mq.M22));
				rotate.Y = (float)(Math.Atan2(-mq.M13, sy));
				rotate.Z = (float)(0);
			}

			return rotate;*/

			Vector3 output = Vector3.Zero;

			Vector4 inputA = new Vector4(100, 0, 0,1);
			Vector4 inputB = new Vector4(0, 200, 0,1);
			Vector4 inputC = new Vector4(0, 0, 300,1);

			Vector3 correct_outputA = Vector4.Transform(inputA, computedMatrix).Xyz;
			Vector3 correct_outputB = Vector4.Transform(inputB, computedMatrix).Xyz;
			Vector3 correct_outputC = Vector4.Transform(inputC, computedMatrix).Xyz;

			Vector3 translate = computedMatrix.ExtractTranslation();
			Vector3 scale = computedMatrix.ExtractScale();

			List<Vector3> startPoints = new List<Vector3>(0);
			List<int> stepsCount = new List<int>(0);
			List<float> stepsRadians = new List<float>(0);

			startPoints.Add(new Vector3(-Mathematics.PI+ constraint.X, -Mathematics.PI + constraint.Y, -Mathematics.PI + constraint.Z));
			stepsCount.Add(4);
			stepsRadians.Add(Mathematics.TwoPI / (float)stepsCount[0]);




			float minDistanceSum = Single.MaxValue;

			for (int i=0; i<10; i++)
			{
				float step = stepsRadians[i];
				int steps = stepsCount[i];


				for (int x = 0; x < steps; x++)
				{
					float x_ = startPoints[i].X + step * x;
					for (int y = 0; y < steps; y++)
					{
						float y_ = startPoints[i].Y + step * y;
						for (int z = 0; z < steps; z++)
						{
							float z_ = startPoints[i].Z + step * z;

							Matrix4 newComputed = Matrix4.CreateScale(scale) * Matrix4.CreateRotationZ(z_)* Matrix4.CreateRotationY(y_) * Matrix4.CreateRotationX(x_) * Matrix4.CreateTranslation(translate);

							float dist =
								Vector3.Distance(Vector4.Transform(inputA, newComputed).Xyz, correct_outputA) +
								Vector3.Distance(Vector4.Transform(inputB, newComputed).Xyz, correct_outputB) +
								Vector3.Distance(Vector4.Transform(inputC, newComputed).Xyz, correct_outputC);

							if (dist< minDistanceSum)
							{
								minDistanceSum = dist;
								output.X = x_;
								output.Y = y_;
								output.Z = z_;
							}
						}
					}
				}

				float count = startPoints.Count + 1;


				startPoints.Add(new Vector3(output.X - Mathematics.PI/count, output.Y - Mathematics.PI / count, output.Z - Mathematics.PI / count));
				stepsCount.Add(4);
				stepsRadians.Add((Mathematics.TwoPI/ count) / (float)stepsCount[0]);
			}
			if (constraint.Length <0.0001)
			{
				output.X = (float)Math.Atan2(Math.Sin(output.X), Math.Cos(output.X));
				output.Y = (float)Math.Atan2(Math.Sin(output.Y), Math.Cos(output.Y));
				output.Z = (float)Math.Atan2(Math.Sin(output.Z), Math.Cos(output.Z));
			}

			return new Vector3(output.X,output.Y,output.Z);
		}

		public List<AnimationBinary> AnimationBinaries;
		public Moveset(string folderName, Object3D referenceModel)
		{
			this.AnimationBinaries = new List<AnimationBinary>(0);
			string[] filenames = Directory.GetFiles(folderName);

			int count = 0;

			foreach (string filename in filenames)
			{
				AnimationBinary animationBinary = new AnimationBinary(filename, referenceModel);
				AnimationBinaries.Add(animationBinary);

				continue;


			}
		}
	}
}
