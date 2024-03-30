using System;
using OpenTK;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BDxGraphiK
{
	public class Joint
	{
		public string Name;
		public Matrix4 Transform;
		public Matrix4 ComputedTransform;

		//public Matrix4d Transformd;
		//public Matrix4d ComputedTransformd;

		public Vector4 Rotate;
		public Vector3 Translate;
		public Vector3 Scale;

		public void CalculateMatrixFromAngles()
		{
			this.Transform =
			Matrix4.CreateScale(this.Scale) *
			Matrix4.CreateFromAxisAngle(Vector3.UnitX, this.Rotate.X) *
			Matrix4.CreateFromAxisAngle(Vector3.UnitY, this.Rotate.Y) *
			Matrix4.CreateFromAxisAngle(Vector3.UnitZ, this.Rotate.Z) *
			Matrix4.CreateTranslation(this.Translate);
		}

		public void CalculateAnglesFromMatrices()
		{
			this.Scale = this.Transform.ExtractScale();
			this.Translate = this.Transform.ExtractTranslation();
			Matrix4 mq = Matrix4.CreateFromQuaternion(this.Transform.ExtractRotation());
			double sy = Math.Sqrt(mq.M11 * mq.M11 + mq.M12 * mq.M12);
			bool singular = sy < 1e-6;
			if (!singular)
			{
				this.Rotate.X = (float)Math.Atan2(mq.M23, mq.M33);
				this.Rotate.Y = (float)(Math.Atan2(-mq.M13, sy));
				this.Rotate.Z = (float)(Math.Atan2(mq.M12, mq.M11));
			}
			else
			{
				this.Rotate.X = (float)(Math.Atan2(-mq.M32, mq.M22));
				this.Rotate.Y = (float)(Math.Atan2(-mq.M13, sy));
				this.Rotate.Z = 0;
			}
		}

		public int IndexInBuffer;
		public Joint Parent;
		public List<Joint> Children;
		public bool Dirty;

		public Joint(string name)
		{
			this.Name = name;
			this.Transform = Matrix4.CreateScale(1f);
			this.ComputedTransform = Matrix4.CreateScale(1f);
			//this.Transformd = Matrix4d.Identity;
			//this.ComputedTransformd = Matrix4d.Identity;
			this.IndexInBuffer = -1;
			this.Children = new List<Joint>(0);
		}
	}
}
