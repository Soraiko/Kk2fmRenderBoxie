using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ProgressBar;
using static BDxGraphiK.GLControl;
using System.Runtime.CompilerServices;

namespace BDxGraphiK
{
	public class GLControl:Panel
	{
		public static System.Drawing.Rectangle Viewport;
		public static void GLViewport(int x, int y, int width, int height)
		{
			Viewport.X = x;
			Viewport.Y = y;
			Viewport.Width = width;
			Viewport.Height = height;
			GL.Viewport(x, y, width, height);
		}

		public static System.Drawing.Rectangle Scissor;
		public static void GLScissor(int x, int y, int width, int height)
		{
			Scissor.X = x;
			Scissor.Y = y;
			Scissor.Width = width;
			Scissor.Height = height;
			GL.Scissor(x, y, width, height);
		}

		public static int AbsoluteShader = -1;

		public class RenderLayer
		{
			int framebuffer = -1;
			int renderbuffer = -1;
			int framebufferTexture = -1;

			int framebufferWidth = 1;
			int framebufferHeight = 1;

			Object3D layerModel;

			public bool Initialized
			{
				get
				{
					return framebuffer > -1;
				}
			}
			public bool Enabled
			{
				get;set;
			}


			Mesh.Shader inputShader;
			Mesh.Shader outputShader;


			public void Initialize(
				TextureMinFilter inputFilter,
				TextureMinFilter outputFilter,
				int renderWidth,
				int renderHeight,
				Mesh.Shader inputShader,
				Mesh.Shader outputShader)
			{
				this.framebufferWidth = renderWidth;
				this.framebufferHeight = renderHeight;
				this.inputShader = inputShader;
				this.outputShader = outputShader;

				this.framebuffer = GL.GenFramebuffer();
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);

				GL.CreateRenderbuffers(1, out this.renderbuffer);
				GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, this.renderbuffer);

				GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthStencil, this.framebufferWidth, framebufferHeight);
				GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, this.renderbuffer);


				this.framebufferTexture = GL.GenTexture();
				GL.BindTexture(TextureTarget.Texture2D, framebufferTexture);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, this.framebufferWidth, framebufferHeight, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)inputFilter);
				GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)ChooseMagFilter(inputFilter));
				GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, framebufferTexture, 0);

				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

				layerModel = new Object3D("resources/layer.dae");
				layerModel.TextureMaterials[0].Textures[0].Integer = framebufferTexture;
				layerModel.TextureMaterials[0].Textures[0].TextureMinFilter = new int[] { (int)outputFilter };
			}


			public TextureMagFilter ChooseMagFilter(TextureMinFilter minFilter)
			{
				if (Enum.IsDefined(typeof(TextureMagFilter), (int)minFilter))
				{
					return (TextureMagFilter)((int)minFilter);
				}
				else
				{
					switch (minFilter)
					{
						case TextureMinFilter.LinearMipmapNearest:
						case TextureMinFilter.LinearMipmapLinear:
						case TextureMinFilter.LinearClipmapLinearSgix:
						case TextureMinFilter.LinearClipmapNearestSgix:
							return TextureMagFilter.Linear;
					}
					return TextureMagFilter.Nearest;
				}
			}

			public void BindBuffer()
			{
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
				GL.Viewport(0, 0, framebufferWidth, framebufferHeight);
				GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
				AbsoluteShader = this.inputShader.Handle;
			}

			public void UnbindBuffer()
			{
				GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
				GL.Viewport(Viewport.X, Viewport.Y, Viewport.Width, Viewport.Height);
				AbsoluteShader = -1;
			}

			public void Draw()
			{
				AbsoluteShader = this.outputShader.Handle;
				Matrix4 orhto = Matrix4.CreateOrthographic(1, 1, 0.1f, 10f);

				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref orhto);

				Matrix4 lookat = Matrix4.LookAt(new Vector3(0, 0, 1), new Vector3(0, 0, 0), Vector3.UnitY);
				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookat);

				layerModel.Draw(false, Mesh.Shader.FogMode.None);
				AbsoluteShader = -1;
			}
		}


		public Dictionary<string, RenderLayer> RenderLayers = new Dictionary<string, RenderLayer>(0);

		public GLControl()
		{

		}

		public new string ToString()
		{
			return this.Name;
		}
		public static List<int> ShaderPrograms = new List<int>(0);
		public static List<Dictionary<string, int>> UniformLocations = new List<Dictionary<string, int>>(0);

		public static string AlphaTest_UniformName = "";

		public static string FogColor_UniformName = "";
		Color fogColor = Color.Transparent;
		public Color FogColor
		{
			get
			{
				return this.fogColor;
			}
			set
			{
				this.fogColor = value;
			}
		}

		float fogMin = 0f;
		public float FogMin
		{
			get
			{
				return this.fogMin;
			}
			set
			{
				this.fogMin = value;
			}
		}

		float fogMax = 100f;
		public float FogMax
		{
			get
			{
				return this.fogMax;
			}
			set
			{
				this.fogMax = value;
			}
		}

		public static string FogInfo_UniformName = "";
		float fogNear = 0f;
		public float FogNear
		{
			get
			{
				return this.fogNear;
			}
			set
			{
				this.fogNear = value;
			}
		}

		float fogFar = 100f;
		public float FogFar
		{
			get
			{
				return this.fogFar;
			}
			set
			{
				this.fogFar = value;
			}
		}


		public static string Light0Position_UniformName = "";
		Vector3 light0Position = Vector3.Zero;
		public Vector3 Light0Position
		{
			get
			{
				return this.light0Position;
			}
			set
			{
				this.light0Position = value;
			}
		}

		public static string Light0Color_UniformName = "";
		Color light0Color = Color.Transparent;
		public Color Light0Color
		{
			get
			{
				return this.light0Color;
			}
			set
			{
				this.light0Color = value;
			}
		}

		public static string Light0DiffuseStrength_UniformName = "";
		float light0DiffuseStrength = 0f;
		public float Light0DiffuseStrength
		{
			get
			{
				return this.light0DiffuseStrength;
			}
			set
			{
				this.light0DiffuseStrength = value;
			}
		}


		public EventHandler RenderFrame;

		public RectangleF Proportions;
		bool formProportionate;
		public bool FormProportionate
		{
			get
			{
				return this.formProportionate;
			}
			set
			{
				this.formProportionate = value;
			}
		}

		public float FieldOfView = 90f;
		public float AspectRatio
		{
			get
			{
				float val = this.Width / (float)this.Height;
				return val > 0 ? val : Mathematics.MinimumAspectRatio;
			}
		}
		Size size;
		public new Size Size
		{
			get
			{
				if (Process.GetCurrentProcess().MainWindowHandle == IntPtr.Zero)
				base.Size = this.size;
				return base.Size;
			}
			set
			{
				this.size = value;
				base.Size = this.size;
			}
		}


		public Point RelativeLocation
		{
			get
			{
				var loc = base.PointToScreen(Point.Empty);
				var control = this as Control;
				while (control.Parent != null)
				{
					control = control.Parent;
				}
				var rootLoc = control.PointToScreen(Point.Empty);
				return new Point(loc.X- rootLoc.X, loc.Y-rootLoc.Y);
			}
		}

		Point location;
		public new Point Location
		{
			get
			{
				if (Process.GetCurrentProcess().MainWindowHandle == IntPtr.Zero)
					base.Location = this.location;
				return base.Location;
			}
			set
			{
				this.location = value;
				base.Location = this.location;
			}
		}

		float nearPlane = 1f;
		public float NearPlane
		{
			get
			{
				return this.nearPlane;
			}
			set
			{
				this.nearPlane = value;
			}
		}

		float farPlane = 1000000f;
		public float FarPlane
		{
			get
			{
				return this.farPlane;
			}
			set
			{
				this.farPlane = value;
			}
		}

		float updateRate = 60f;
		public float UpdateRate
		{
			get
			{
				return this.updateRate;
			}
			set
			{
				this.updateRate = value;
			}
		}

		int sampleCount = 8;
		public int SampleCount
		{
			get
			{
				return this.sampleCount;
			}
			set
			{
				this.sampleCount = value;
			}
		}
	}
}
