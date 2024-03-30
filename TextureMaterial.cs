using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDxGraphiK
{
	public struct TextureMaterial
	{
		public const int COUNT = 2;
		public readonly string Name;
		public Texture[] Textures;

		public enum TextureType
		{
			Diffuse = 0,
			BumpMapping = 1
		}

		public TextureMaterial(string name)
		{
			this.Name = name;
			this.Textures = new Texture[COUNT];
		}
	}
}
