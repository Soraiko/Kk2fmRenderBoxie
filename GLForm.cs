using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using static SrkAlternatives.Mdlx;
using OpenKh;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Runtime.InteropServices;
using System.Reflection;
using OpenTK.Audio.OpenAL;
using System.Security.Cryptography;
using System.Security.Policy;
using static BDxGraphiK.Mesh;
using static System.Net.WebRequestMethods;
using static BDxGraphiK.MDLX;

namespace BDxGraphiK
{
	public partial class GLForm : PrivateGLForm
	{
		Padding padding;

		public GLForm()
		{
			InitializeComponent();
			this.Initialize();

			//this.Opacity = 0.7f;
			this.Load += GLForm_Load;
			this.UpdateFrame += GLForm_UpdateFrame;

			padding = glControl1.Parent.Padding;
			this.glControl1.RenderFrame += bigViewport_RenderFrame;
			this.glControl2.RenderFrame += smallViewport_RenderFrame;
			this.glControl3.RenderFrame += smallViewport_RenderFrame2;
		}

		string pcsx2pName = "";
		public Process pcsx2;
		public SrkProcessStream stream;
		bool justOpen;

		private void GLForm_UpdateFrame(object sender, EventArgs e)
		{
			if (pcsx2pName.Length == 0)
			{
				foreach (Process p in Process.GetProcesses())
				{
					if (p.ProcessName.Contains("pcsx2"))
					{
						pcsx2pName = p.ProcessName;
						
						break;
					}
				}
			}
			else
			{
				if (pcsx2 != null && pcsx2.Id != 0 && Process.GetProcessById(pcsx2.Id) != null)
				{
					PCSX2Loop();
					justOpen = false;
				}
				else
				{
					justOpen = true;
					stream = null;
					pcsx2 = null;
					foreach (Process p in Process.GetProcessesByName(pcsx2pName))
					{
						pcsx2 = p;
						stream = new SrkProcessStream(p);
						stream.BaseOffset = 0x20000000;
					}
				}
			}
		}

		MDLX.RAM_Model map;

		Stopwatch stp = new Stopwatch();

		long lastSecond = 0;
		long lastTick = 0;
		public long totalTicks = 0;

		public void PCSX2Loop()
		{
			if (justOpen)
				CreateThread();

			string mapName = "m" + stream.ReadString(0x00348D69, 31, true);

			/* Graphics active */
			if (stream.ReadInt32(0x0032BAD8) == 1)
			{
				if (map == null)
				{
					int mapAddress = stream.ReadInt32(0x00348D88);
					if (stream.ReadString(mapAddress, 3, false) == "BAR" && stream.ReadString(mapAddress + 0x14, 4, true) == "MAP")
					{
						map = MDLX.RAM_Model.LoadMAP(stream, mapAddress, mapName);
					}
				}
				else
				{
					glControl1.RenderLayers["map_glow"].Enabled = mapAlphaGlow.Checked;

					SmoothCam(4f);
				}
			}
			else
			{
				map = null;
			}


			if (stp.ElapsedMilliseconds > lastSecond + 1000)
			{
				float divide = 1f;
				
				this.Text = ((totalTicks - lastTick) / (divide * 3f)) +"FPS";
				lastTick = totalTicks;
				lastSecond = stp.ElapsedMilliseconds;
			}
		}

		private unsafe void bigViewport_RenderFrame(object sender, EventArgs e)
		{
			Matrix4 lookat = Matrix4.LookAt(cameraPosition, cameraLookAt, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);

			RAM_Model.DrawMap(map, sender as GLControl, fog.Checked ? Mesh.Shader.FogMode.XYZ : Shader.FogMode.None, frustumCulling.Checked);
			totalTicks++;
		}


		private void smallViewport_RenderFrame(object sender, EventArgs e)
		{
			Matrix4 lookat = Matrix4.LookAt(cameraLookAt + new Vector3(500, 500, 500), cameraLookAt, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);

			RAM_Model.DrawMap(map, sender as GLControl, fog.Checked ? Mesh.Shader.FogMode.XYZ : Shader.FogMode.None, frustumCulling.Checked);
		}

		private void smallViewport_RenderFrame2(object sender, EventArgs e)
		{
			Matrix4 lookat = Matrix4.LookAt(cameraLookAt + new Vector3(0, 50, 150), cameraLookAt, Vector3.UnitY);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);

			RAM_Model.DrawMap(map, sender as GLControl, fog.Checked ? Mesh.Shader.FogMode.XYZ : Shader.FogMode.None, frustumCulling.Checked);
		}

		public Vector3 cameraPosition = Vector3.Zero;
		public Vector3 cameraLookAt = Vector3.Zero;

		public void SmoothCam(float step)
		{
			if (stream.ReadInt32(0x00348754) == 0)
				step = 1f;

			int position = 0x003A7FC0;
			int lookAt = 0x003A7FD0;

			Vector3 camPosition = new Vector3(stream.ReadSingle(position), -stream.ReadSingle(position + 4), -stream.ReadSingle(position + 8));
			Vector3 camLookAt = new Vector3(stream.ReadSingle(lookAt), -stream.ReadSingle(lookAt + 4), -stream.ReadSingle(lookAt + 8));

			cameraPosition += (camPosition - cameraPosition) / step;
			cameraLookAt += (camLookAt - cameraLookAt) / step;
		}


		System.Threading.Thread objentryThread = null;
		List<Object3D> objects3D = new List<Object3D>(0);

		private void GLForm_Load(object sender, EventArgs e)
		{
			stp.Start();
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.Enable(EnableCap.Blend);
			GL.Enable(EnableCap.ColorArray);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
			GL.DepthFunc(OpenTK.Graphics.OpenGL.DepthFunction.Lequal);
			GL.Enable(EnableCap.Multisample);

			GLControl.ShaderPrograms = new List<int>(0);
			GLControl.UniformLocations = new List<Dictionary<string, int>>(0);

			GLControl.AlphaTest_UniformName = "alphatest";
			GLControl.Light0Position_UniformName = "light0_position";
			GLControl.Light0Color_UniformName = "light0_color";
			GLControl.Light0DiffuseStrength_UniformName = "light0_diffuse_strength";
			GLControl.FogColor_UniformName = "fog_color";
			GLControl.FogInfo_UniformName = "fog_near_far_min_max";

			foreach (Mesh.Shader shader in Mesh.Shader.FlaggedShaders)
			{
				Dictionary<string, int> keyValuePairs = new Dictionary<string, int>(0);

				keyValuePairs.Add(GLControl.AlphaTest_UniformName, -1);
				keyValuePairs.Add(GLControl.Light0Position_UniformName, -1);
				keyValuePairs.Add(GLControl.Light0Color_UniformName, -1);
				keyValuePairs.Add(GLControl.Light0DiffuseStrength_UniformName, -1);
				keyValuePairs.Add(GLControl.FogColor_UniformName, -1);
				keyValuePairs.Add(GLControl.FogInfo_UniformName, -1);

				GLControl.UniformLocations.Add(keyValuePairs);
				GLControl.ShaderPrograms.Add(shader.Handle);
			}

			GLControl.RenderLayer mapGlow = new GLControl.RenderLayer();
			string sourceCcode = System.IO.File.ReadAllText("resources/MDLX/MAPGLOW_output_frag.c");
			string[] textureSize = sourceCcode.Substring(sourceCcode.IndexOf("textureSize =")).Split(new char[] { '(', ')' })[1].Split(',');
			mapGlow.Initialize(TextureMinFilter.Linear, TextureMinFilter.Linear, int.Parse(textureSize[0]), int.Parse(textureSize[1]), new Shader("resources/graphics/T1N0C1S0_vert.c", "resources/MDLX/MAPGLOW_input_frag.c"), new Shader("resources/graphics/T1N0C1S0_vert.c", "resources/MDLX/MAPGLOW_output_frag.c"));
			glControl1.RenderLayers.Add("map_glow", mapGlow);

			multipleRenders_CheckedChanged(null, null);
		}


		List<int> modelsMemRegions = new List<int>(0);
		List<MDLX.RAM_Model> models = new List<MDLX.RAM_Model>(0);
		public List<int> awaitingModelsMemRegions = new List<int>();

		public unsafe void SearchModels()
		{
			int blockSize = 0x100000;
			byte[] buffer = new byte[blockSize + 0x150];
			int i;
			byte* t = (byte*)&i;

			for (i = 0x01000000; i < 0x04000000;)
			{
				stream.Read(i, ref buffer);
				i -= 0x10;
				for (int j = 0; j < blockSize; j += 0x10)
				{
					i += 0x10;

					if ((*(t + 1)) != buffer[j + 0x149]) continue;
					if ((*(t + 2)) != buffer[j + 0x14A]) continue;
					if ((*(t + 0)) != buffer[j + 0x148]) continue;
					if ((*(t + 3)) != buffer[j + 0x14B]) continue;

					int objEntry = BitConverter.ToInt32(buffer, j + 0x08);
					if (objEntry > 0x01C94000 && objEntry < 0x01CC09C0)
					{
						if (MDLX.RAM_Model.ram_ban_log_integers.Contains(objEntry))
							continue;

						if (awaitingModelsMemRegions.Contains(i) || modelsMemRegions.Contains(i))
							continue;

						Console.WriteLine("New model entry found at " + i.ToString("X8"));
						awaitingModelsMemRegions.Add(i);
					}
				}
			}
		}


		public void CreateThread()
		{
			if (objentryThread != null)
			{
				if (objentryThread.IsAlive)
					objentryThread.Abort();
			}
			objentryThread = new System.Threading.Thread(() => {
				while (true)
					SearchModels();
			});
			objentryThread.Start();
		}


		private void GLForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (objentryThread!= null && objentryThread.IsAlive)
			objentryThread.Abort();
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if (stream == null)
				return;
			stream.WriteSingle(0x00349E0c, checkBox1.Checked ? 0f : 1f);
		}

		private void checkBox2_CheckedChanged(object sender, EventArgs e)
		{
			checkBox1.Visible = checkBox2.Checked;
			meshSkipRenders.Visible = checkBox2.Checked;
			frustumCulling.Visible = checkBox2.Checked;
			showMap.Visible = checkBox2.Checked;
			showModels.Visible = checkBox2.Checked;
			transformModels.Visible = checkBox2.Checked;
			mapDiffuseRegions.Visible = checkBox2.Checked;
			fog.Visible = checkBox2.Checked;
			mapAlphaGlow.Visible = checkBox2.Checked;
			multipleRenders.Visible = checkBox2.Checked;
		}

		private void multipleRenders_CheckedChanged(object sender, EventArgs e)
		{
			glControl2.Visible = (multipleRenders.Checked);
			glControl3.Visible = (multipleRenders.Checked);

			glControl1.FormProportionate = (multipleRenders.Checked);

			if (multipleRenders.Checked)
			{
				glControl1.Parent.Padding = padding;
				glControl1.Dock = DockStyle.None;
			}
			else
			{
				glControl1.Parent.Padding = new Padding(0,0,0,0);
				glControl1.Dock = DockStyle.Fill;
			}

		}
	}
}
