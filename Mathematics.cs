using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDxGraphiK
{
	public static class Mathematics
	{
		public const float PI = 3.1415926535897932384626433832795f;
		public const float TwoPI = 6.283185307179586476925286766559f;
		public const float PiOverTwo = 1.5707963267948966192313216916398f;
		public const float MinimumAspectRatio = 0.0001f;

		public static float Floor(float base_)
		{
			return (float)Math.Floor(base_);
		}

		public static OpenTK.Matrix4 ConvertMatrix(Assimp.Matrix4x4 m)
		{
			return new OpenTK.Matrix4(
				m.A1, m.B1, m.C1, m.D1,
				m.A2, m.B2, m.C2, m.D2,
				m.A3, m.B3, m.C3, m.D3,
				m.A4, m.B4, m.C4, m.D4);
		}

		public static OpenTK.Matrix4 ConvertMatrix(OpenTK.Matrix4d m)
		{
			return new OpenTK.Matrix4(
				(float)m.M11, (float)m.M12, (float)m.M13, (float)m.M14,
				(float)m.M21, (float)m.M22, (float)m.M23, (float)m.M24,
				(float)m.M31, (float)m.M32, (float)m.M33, (float)m.M34,
				(float)m.M41, (float)m.M42, (float)m.M43, (float)m.M44);
		}

		public static OpenTK.Matrix4d ConvertMatrix(OpenTK.Matrix4 m)
		{
			return new OpenTK.Matrix4d(
				(double)m.M11, (double)m.M12, (double)m.M13, (double)m.M14,
				(double)m.M21, (double)m.M22, (double)m.M23, (double)m.M24,
				(double)m.M31, (double)m.M32, (double)m.M33, (double)m.M34,
				(double)m.M41, (double)m.M42, (double)m.M43, (double)m.M44);
		}

	}
}
