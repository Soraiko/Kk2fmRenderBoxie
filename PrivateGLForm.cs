using Assimp;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Platform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace BDxGraphiK
{
	public partial class PrivateGLForm : Form
	{
		public OpenTK.GLControl gLControl;

		public List<BDxGraphiK.GLControl> OnlyGLControls;
		public List<Control> AllControls;

		Form form;
		GameWindow gameWindow;
		bool noBackground = false;

		Padding padding;
		public new Padding Padding
		{
			get
			{
				base.Padding = this.padding;
				return base.Padding;
			}
			set
			{
				this.padding = value;
				base.Padding = this.padding;
			}
		}

		Size clientSize;
		public new Size ClientSize
		{
			get
			{
				base.ClientSize = this.clientSize;
				return base.ClientSize;
			}
			set
			{
				this.clientSize = value;
				base.ClientSize = this.clientSize;
			}
		}

		public PrivateGLForm()
		{
			InitializeComponent();
		}

		public void Recursive_GLControl_Search(Control parent)
		{
			var glControl = parent as GLControl;
			if (glControl!=null)
				this.OnlyGLControls.Insert(0, glControl);
			if (parent.Parent!=null)
			this.AllControls.Add(parent);

			foreach (Control control in parent.Controls)
			{
				Recursive_GLControl_Search(control);
			}
		}


		private void GLForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (noBackground == false)
				gameWindow.Close();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			timer1.Enabled = false;

			this.Opacity = 0;
			this.Visible = true;

			gameWindow.Run(this.OnlyGLControls[0].UpdateRate);
			this.Close();
		}


		private void GLForm_Load(object sender, EventArgs e)
		{
			gameWindow.Location = this.Location;
			gameWindow.Size = this.Size;

			if (noBackground)
			{
				this.Close();
				gameWindow.Run(this.OnlyGLControls[0].UpdateRate);
			}
			else
			{
				this.Visible = false;
				this.Opacity = 0;
				timer1.Enabled = true;
			}
		}

		public new event EventHandler Load;
		public new event EventHandler Resize;
		public new event EventHandler Move;
		public event EventHandler UpdateFrame;

		private void GameWindow_Load(object sender, EventArgs e)
		{
			if (this.Load != null)
				this.Load(sender, e);
		}

		private void GameWindow_Resize(object sender, EventArgs e)
		{
			if (noBackground)
			{
				this.Size = gameWindow.Size;
				this.OnlyGLControls[0].Size = new Size(gameWindow.Width, gameWindow.Height);

				GLControl.GLViewport(0, 0, gameWindow.Width, gameWindow.Height);
			}
			else
			{
				this.Size = gameWindow.Size;
				GLControl.GLViewport(this.OnlyGLControls[0].Location.X, gameWindow.Height - this.OnlyGLControls[0].Location.Y - this.OnlyGLControls[0].ClientSize.Height, this.OnlyGLControls[0].ClientSize.Width, this.OnlyGLControls[0].ClientSize.Height);
			}
			if (this.Resize != null)
				this.Resize(sender, e);
		}

		private void GameWindow_Move(object sender, EventArgs e)
		{
			this.Location = gameWindow.Location;
			if (this.Move != null)
				this.Move(sender, e);
		}

		bool updatedOnce = false;

		private void GameWindow_UpdateFrame(object sender, FrameEventArgs e)
		{
			if (this.UpdateFrame != null)
				this.UpdateFrame(sender, e);
			this.updatedOnce = true;
		}

		private void GameWindow_RenderFrame(object sender, FrameEventArgs e)
		{
			if (this.updatedOnce == false)
				return;
			if (noBackground)
			{
				GL.ClearColor(this.OnlyGLControls[0].BackColor);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(this.OnlyGLControls[0].FieldOfView, this.OnlyGLControls[0].AspectRatio, this.OnlyGLControls[0].NearPlane, this.OnlyGLControls[0].FarPlane);
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref perpective);

				if (this.OnlyGLControls[0].RenderFrame != null)
				{
					RenderGL(this.OnlyGLControls[0]);
				}
			}
			else
			{
				GL.ClearColor(this.BackColor);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

				GL.Enable(EnableCap.ScissorTest);
				GL.ClearColor(this.OnlyGLControls[0].BackColor);
				GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				GLControl.GLScissor(this.OnlyGLControls[0].Location.X, gameWindow.Height - this.OnlyGLControls[0].Location.Y - this.OnlyGLControls[0].ClientSize.Height, this.OnlyGLControls[0].ClientSize.Width, this.OnlyGLControls[0].ClientSize.Height);

				GL.Disable(EnableCap.ScissorTest);

				Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(this.OnlyGLControls[0].FieldOfView, this.OnlyGLControls[0].AspectRatio, this.OnlyGLControls[0].NearPlane, this.OnlyGLControls[0].FarPlane);
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref perpective);

				if (this.OnlyGLControls[0].RenderFrame != null)
				{
					RenderGL(this.OnlyGLControls[0]);
				}
			}


			gameWindow.SwapBuffers();
			ATR++;
		}

		public void Initialize()
		{
			this.OnlyGLControls = new List<GLControl>(0);
			this.AllControls = new List<Control>(0);

			Recursive_GLControl_Search(this);



			if (this.OnlyGLControls.Count == this.AllControls.Count && this.AllControls.Count == 1)
			{
				for (int i = 0; i < this.OnlyGLControls.Count; i++)
				{
					Size glsize = this.OnlyGLControls[i].Size;
					Point gllocation = this.OnlyGLControls[i].Location;
					Size clientSize = this.ClientSize;
					Padding padding = this.Padding;
				}

				gameWindow = new GameWindow(base.Width, base.Height, new GraphicsMode(GraphicsMode.Default.ColorFormat, GraphicsMode.Default.Depth, GraphicsMode.Default.Stencil, this.OnlyGLControls[0].SampleCount));
				gameWindow.Title = this.Text;
				gameWindow.Icon = base.Icon;
				gameWindow.Load += GameWindow_Load;
				gameWindow.Move += GameWindow_Move; ;
				gameWindow.Resize += GameWindow_Resize;
				gameWindow.UpdateFrame += GameWindow_UpdateFrame;
				gameWindow.RenderFrame += GameWindow_RenderFrame;

				base.Load += GLForm_Load;
				base.FormClosing += GLForm_FormClosing;
				base.ShowInTaskbar = false;

				if (this.OnlyGLControls[0].Anchor == (System.Windows.Forms.AnchorStyles)(
					System.Windows.Forms.AnchorStyles.Top |
					System.Windows.Forms.AnchorStyles.Bottom |
					System.Windows.Forms.AnchorStyles.Left |
					System.Windows.Forms.AnchorStyles.Right) && base.ClientSize == this.OnlyGLControls[0].Size)
				{
					this.OnlyGLControls[0].Anchor = AnchorStyles.None;
					this.Padding = System.Windows.Forms.Padding.Empty;
					this.OnlyGLControls[0].Dock = DockStyle.Fill;
				}

				if (this.OnlyGLControls[0].Dock == DockStyle.Fill && base.Padding == System.Windows.Forms.Padding.Empty)
				{
					noBackground = true;
				}
			}
			else if (this.OnlyGLControls.Count>0)
			{
				for (int i=0;i< this.OnlyGLControls.Count;i++)
				{
					var curr = this.OnlyGLControls[i];
					var parent = curr.Parent as GLControl;
					if (parent != null)
					{
						this.OnlyGLControls.Remove(curr);
						var parentIndex = this.OnlyGLControls.IndexOf(parent);
						this.OnlyGLControls.Insert(parentIndex+1, curr);
					}
				}

				Application.Idle += Application_Idle;
				form = new Form();
				form.Padding = base.Padding;
				base.Padding = Padding.Empty;
				base.Load += PrivateGLForm_Load;

				base.SizeChanged += PrivateGLForm_SizeChanged;
				base.LocationChanged += PrivateGLForm_LocationChanged;

				
				gLControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, this.OnlyGLControls[0].SampleCount));
				gLControl.Dock = DockStyle.Fill;
				gLControl.Paint += GLControl_Paint;


				for (int i=0;i< this.Controls.Count; i++)
				{
					if (this.Controls[i] is GLControl || this.Controls[i] is Panel)
					{
						form.Controls.Add(this.Controls[i]);
						i--;
					}
				}
				var controls = this.Controls;
				this.Controls.Add(gLControl);
			}
		}

		private void PrivateGLForm_LocationChanged(object sender, EventArgs e)
		{
			form.Location = base.Location;
			if (this.Move != null)
				this.Move(sender, e);
		}

		private void PrivateGLForm_Load(object sender, EventArgs e)
		{
			form.Location = base.Location;
			form.Size = base.Size;
			form.Opacity = 0;

			form.Show();
			for (int i = 0; i < this.OnlyGLControls.Count; i++)
			{
				var curr = this.OnlyGLControls[i];

				var relative = curr.RelativeLocation;
				curr.Proportions = new RectangleF(
					relative.X / (float)base.ClientSize.Width,
					relative.Y / (float)base.ClientSize.Height,
					curr.Width / (float)base.ClientSize.Width,
					curr.Height / (float)base.ClientSize.Height);
			}
			if (this.Load != null)
				this.Load(sender, e);
		}

		private void GLControl_Paint(object sender, PaintEventArgs e)
		{
			Render(sender, e);
		}

		private void Application_Idle(object sender, EventArgs e)
		{
			while (gLControl.IsIdle)
			{
				Render(sender, e);
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			Application.Idle -= Application_Idle;

			base.OnClosing(e);
		}

		public static long ATR = 0;
		public void Render(object sender, EventArgs e)
		{
			if (this.UpdateFrame != null)
				this.UpdateFrame(sender, e);

			GLControl.GLViewport(0, 0, base.ClientSize.Width, base.ClientSize.Height);
			GL.ClearColor(this.BackColor);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


			GL.Enable(EnableCap.ScissorTest);
			for (int i=0;i< this.OnlyGLControls.Count;i++)
			{
				GLControl glControl = this.OnlyGLControls[i];
				if (glControl.Visible == false)
					continue;
				if (glControl.FormProportionate)
				{
					var proportions = glControl.Proportions;
					GLControl.GLViewport(
						(int)(base.ClientSize.Width * proportions.X),
						base.ClientSize.Height - (int)(base.ClientSize.Height * proportions.Y) - (int)(base.ClientSize.Height * proportions.Height),
						(int)(base.ClientSize.Width * proportions.Width),
						(int)(base.ClientSize.Height * proportions.Height));
					GLControl.GLScissor(
						(int)(base.ClientSize.Width * proportions.X),
						base.ClientSize.Height - (int)(base.ClientSize.Height * proportions.Y)- (int)(base.ClientSize.Height * proportions.Height),
						(int)(base.ClientSize.Width * proportions.Width),
						(int)(base.ClientSize.Height * proportions.Height));
				}
				else
				{
					var relativeLoc = glControl.RelativeLocation;
					GLControl.GLViewport(relativeLoc.X, base.ClientSize.Height - relativeLoc.Y - glControl.Height, glControl.Width, glControl.Height);
					GLControl.GLScissor(relativeLoc.X, base.ClientSize.Height - relativeLoc.Y - glControl.Height, glControl.Width, glControl.Height);
				}

				if (glControl.BackColor.A > 0)
				{
					GL.ClearColor(glControl.BackColor);
					GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				}
				float aspect = glControl.AspectRatio;

				if (glControl.FormProportionate)
				{
					var proportions = glControl.Proportions;
					aspect = (base.ClientSize.Width * proportions.Width) / (base.ClientSize.Height * proportions.Height);
					if (aspect <= 0)
						aspect = Mathematics.MinimumAspectRatio;
				}
				Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView((float)((glControl.FieldOfView/180.0)*Math.PI), aspect, glControl.NearPlane, glControl.FarPlane);
				
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref perpective);

				if (glControl.RenderFrame != null)
				{
					RenderGL(glControl);
				}
			}
			GL.Disable(EnableCap.ScissorTest);
			gLControl.SwapBuffers();
			ATR++;
		}

		public void RenderGL(GLControl glControl)
		{
			SetUniforms(glControl);
			glControl.RenderStep = 0;

			/*
			BlendingFactor[] enumArray = (BlendingFactor[])Enum.GetValues(typeof(BlendingFactor));

			OpenTK.Graphics.OpenGL.DepthFunction[] blendfArray = (OpenTK.Graphics.OpenGL.DepthFunction[])Enum.GetValues(typeof(OpenTK.Graphics.OpenGL.DepthFunction));

			int index0 = (int)Program.glForm.numericUpDown1.Value;
			int index1 = (int)Program.glForm.numericUpDown2.Value;
			int index2 = (int)Program.glForm.numericUpDown3.Value;
			int index3 = (int)Program.glForm.numericUpDown4.Value;

			int index4 = (int)Program.glForm.numericUpDown5.Value;
			int index5 = (int)Program.glForm.numericUpDown6.Value;

			if (index0 > enumArray.Length - 1) index0 = enumArray.Length - 1;
			if (index1 > enumArray.Length - 1) index1 = enumArray.Length - 1;
			if (index2 > enumArray.Length - 1) index2 = enumArray.Length - 1;
			if (index3 > enumArray.Length - 1) index3 = enumArray.Length - 1;

			if (index4 > blendfArray.Length - 1) index4 = blendfArray.Length - 1;
			if (index5 > blendfArray.Length - 1) index5 = blendfArray.Length - 1;


			Program.glForm.label1.Text = enumArray[index0].ToString();
			Program.glForm.label2.Text = enumArray[index1].ToString();
			Program.glForm.label3.Text = enumArray[index2].ToString();
			Program.glForm.label4.Text = enumArray[index3].ToString();

			Program.glForm.label5.Text = blendfArray[index4].ToString();
			Program.glForm.label6.Text = blendfArray[index5].ToString();

			GL.BlendFunc(enumArray[index0], enumArray[index1]);
			GL.DepthFunc(blendfArray[index4]);
			SetAlphaStep(-1f);
			glControl.RenderFrame(glControl, null);

			GL.BlendFunc(enumArray[index2], enumArray[index3]);
			GL.DepthFunc(blendfArray[index5]);
			SetAlphaStep(1f);
			glControl.RenderFrame(glControl, null);*/

			SetAlphaStep(1f);
			GL.DepthFunc(OpenTK.Graphics.OpenGL.DepthFunction.Lequal);
			glControl.RenderFrame(glControl, null);
			glControl.RenderStep++;

			SetAlphaStep(2f);
			GL.DepthFunc(OpenTK.Graphics.OpenGL.DepthFunction.Always);
			glControl.RenderFrame(glControl, null);
			glControl.RenderStep++;

			SetAlphaStep(3f);
			GL.DepthFunc(OpenTK.Graphics.OpenGL.DepthFunction.Lequal);
			glControl.RenderFrame(glControl, null);
			glControl.RenderStep++;

			SetAlphaStep(4f);
			GL.DepthFunc(OpenTK.Graphics.OpenGL.DepthFunction.Lequal);
			glControl.RenderFrame(glControl, null);
			glControl.RenderStep++;

			//SetAlphaStep(5f);
			GL.ClearColor(0,0,0,1);

			for (int r = 0; r < glControl.RenderLayers.Count; r++)
			{
				var layer = glControl.RenderLayers.ElementAt(r).Value;
				if (layer.Initialized && layer.Enabled)
				{
					layer.BindBuffer();
					glControl.RenderFrame(glControl, null);
					glControl.RenderStep++;
					layer.UnbindBuffer();
					layer.Draw();
				}
			}
			
		}

		void SetUniforms(GLControl glControl)
		{
			for (int i = 0; i < GLControl.ShaderPrograms.Count; i++)
			{
				int program = GLControl.ShaderPrograms[i];
				GL.UseProgram(program);

				int location = GLControl.UniformLocations[i][GLControl.FogColor_UniformName];
				if (location < 0)
				{
					location = GL.GetUniformLocation(program, GLControl.FogColor_UniformName);
					GLControl.UniformLocations[i][GLControl.FogColor_UniformName] = location;
				}
				if (location > -1)
				{
					GL.Uniform3(location, (float)(glControl.FogColor.R / 255f), (float)(glControl.FogColor.G / 255f), (float)(glControl.FogColor.B / 255f));
				}

				location = GLControl.UniformLocations[i][GLControl.FogInfo_UniformName];
				if (location < 0)
				{
					location = GL.GetUniformLocation(program, GLControl.FogInfo_UniformName);
					GLControl.UniformLocations[i][GLControl.FogInfo_UniformName] = location;
				}
				if (location > -1)
				{
					GL.Uniform4(location, glControl.FogNear, glControl.FogFar, glControl.FogMin, glControl.FogMax);
				}

				location = GLControl.UniformLocations[i][GLControl.Light0Position_UniformName];
				if (location < 0)
				{
					location = GL.GetUniformLocation(program, GLControl.Light0Position_UniformName);
					GLControl.UniformLocations[i][GLControl.Light0Position_UniformName] = location;
				}
				if (location > -1)
				{
					GL.Uniform3(location, glControl.Light0Position);
				}

				location = GLControl.UniformLocations[i][GLControl.Light0Color_UniformName];
				if (location < 0)
				{
					location = GL.GetUniformLocation(program, GLControl.Light0Color_UniformName);
					GLControl.UniformLocations[i][GLControl.Light0Color_UniformName] = location;
				}
				if (location > -1)
				{
					GL.Uniform3(location, (float)(glControl.Light0Color.R / 255f), (float)(glControl.Light0Color.G / 255f), (float)(glControl.Light0Color.B / 255f));
				}

				location = GLControl.UniformLocations[i][GLControl.Light0DiffuseStrength_UniformName];
				if (location < 0)
				{
					location = GL.GetUniformLocation(program, GLControl.Light0DiffuseStrength_UniformName);
					GLControl.UniformLocations[i][GLControl.Light0DiffuseStrength_UniformName] = location;
				}
				if (location > -1)
				{
					GL.Uniform1(location, glControl.Light0DiffuseStrength);
				}
			}
		}

		void SetAlphaStep(float value)
		{
			for (int i=0;i<GLControl.ShaderPrograms.Count;i++)
			{
				int program = GLControl.ShaderPrograms[i];
				GL.UseProgram(program);

				int location = GLControl.UniformLocations[i][GLControl.AlphaTest_UniformName];
				if (location < 0)
				{
					location = GL.GetUniformLocation(program, GLControl.AlphaTest_UniformName);
					GLControl.UniformLocations[i][GLControl.AlphaTest_UniformName] = location;
				}
				if (location > -1)
				{
					GL.Uniform1(location, value);
				}
			}
		}

		private void PrivateGLForm_SizeChanged(object sender, EventArgs e)
		{
			form.Size = base.Size;
			Render(sender, e);
			if (this.Resize != null)
				this.Resize(sender, e);
		}
	}
}
