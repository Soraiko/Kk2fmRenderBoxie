namespace BDxGraphiK
{
	partial class GLForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GLForm));
			this.glControl1 = new BDxGraphiK.GLControl();
			this.glControl2 = new BDxGraphiK.GLControl();
			this.glControl3 = new BDxGraphiK.GLControl();
			this.meshSkipRenders = new System.Windows.Forms.CheckBox();
			this.frustumCulling = new System.Windows.Forms.CheckBox();
			this.showMap = new System.Windows.Forms.CheckBox();
			this.showModels = new System.Windows.Forms.CheckBox();
			this.transformModels = new System.Windows.Forms.CheckBox();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.mapDiffuseRegions = new System.Windows.Forms.CheckBox();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.fog = new System.Windows.Forms.CheckBox();
			this.cheatEngine = new System.Windows.Forms.CheckBox();
			this.mapAlphaGlow = new System.Windows.Forms.CheckBox();
			this.multipleRenders = new System.Windows.Forms.CheckBox();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
			this.numericUpDown4 = new System.Windows.Forms.NumericUpDown();
			this.numericUpDown3 = new System.Windows.Forms.NumericUpDown();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.numericUpDown5 = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.numericUpDown6 = new System.Windows.Forms.NumericUpDown();
			this.interframeInterpolate = new System.Windows.Forms.CheckBox();
			this.texturePatches = new System.Windows.Forms.CheckBox();
			this.smartBlending = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).BeginInit();
			this.SuspendLayout();
			// 
			// glControl1
			// 
			this.glControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
			this.glControl1.FarPlane = 1E+07F;
			this.glControl1.FogColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.glControl1.FogFar = 1000F;
			this.glControl1.FogMax = 100F;
			this.glControl1.FogMin = 0F;
			this.glControl1.FogNear = float.NaN;
			this.glControl1.FormProportionate = true;
			this.glControl1.Light0Color = System.Drawing.Color.White;
			this.glControl1.Light0DiffuseStrength = 1F;
			this.glControl1.Light0Position = ((OpenTK.Vector3)(resources.GetObject("glControl1.Light0Position")));
			this.glControl1.Location = new System.Drawing.Point(0, 0);
			this.glControl1.Name = "glControl1";
			this.glControl1.NearPlane = 5F;
			this.glControl1.SampleCount = 8;
			this.glControl1.Size = new System.Drawing.Size(720, 675);
			this.glControl1.TabIndex = 4;
			this.glControl1.UpdateRate = 60F;
			// 
			// glControl2
			// 
			this.glControl2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
			this.glControl2.FarPlane = 1E+07F;
			this.glControl2.FogColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.glControl2.FogFar = 1000F;
			this.glControl2.FogMax = 100F;
			this.glControl2.FogMin = 0F;
			this.glControl2.FogNear = float.NaN;
			this.glControl2.FormProportionate = true;
			this.glControl2.Light0Color = System.Drawing.Color.White;
			this.glControl2.Light0DiffuseStrength = 1F;
			this.glControl2.Light0Position = ((OpenTK.Vector3)(resources.GetObject("glControl2.Light0Position")));
			this.glControl2.Location = new System.Drawing.Point(728, 14);
			this.glControl2.Name = "glControl2";
			this.glControl2.NearPlane = 5F;
			this.glControl2.SampleCount = 8;
			this.glControl2.Size = new System.Drawing.Size(523, 317);
			this.glControl2.TabIndex = 6;
			this.glControl2.UpdateRate = 60F;
			// 
			// glControl3
			// 
			this.glControl3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
			this.glControl3.FarPlane = 1E+07F;
			this.glControl3.FogColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this.glControl3.FogFar = 1000F;
			this.glControl3.FogMax = 100F;
			this.glControl3.FogMin = 0F;
			this.glControl3.FogNear = float.NaN;
			this.glControl3.FormProportionate = true;
			this.glControl3.Light0Color = System.Drawing.Color.White;
			this.glControl3.Light0DiffuseStrength = 1F;
			this.glControl3.Light0Position = ((OpenTK.Vector3)(resources.GetObject("glControl3.Light0Position")));
			this.glControl3.Location = new System.Drawing.Point(728, 346);
			this.glControl3.Name = "glControl3";
			this.glControl3.NearPlane = 5F;
			this.glControl3.SampleCount = 8;
			this.glControl3.Size = new System.Drawing.Size(523, 317);
			this.glControl3.TabIndex = 7;
			this.glControl3.UpdateRate = 60F;
			// 
			// meshSkipRenders
			// 
			this.meshSkipRenders.AutoSize = true;
			this.meshSkipRenders.BackColor = System.Drawing.Color.Transparent;
			this.meshSkipRenders.Checked = true;
			this.meshSkipRenders.CheckState = System.Windows.Forms.CheckState.Checked;
			this.meshSkipRenders.Location = new System.Drawing.Point(13, 124);
			this.meshSkipRenders.Name = "meshSkipRenders";
			this.meshSkipRenders.Size = new System.Drawing.Size(114, 17);
			this.meshSkipRenders.TabIndex = 9;
			this.meshSkipRenders.Text = "Mesh Skip-renders";
			this.meshSkipRenders.UseVisualStyleBackColor = false;
			// 
			// frustumCulling
			// 
			this.frustumCulling.AutoSize = true;
			this.frustumCulling.BackColor = System.Drawing.Color.Transparent;
			this.frustumCulling.Checked = true;
			this.frustumCulling.CheckState = System.Windows.Forms.CheckState.Checked;
			this.frustumCulling.Location = new System.Drawing.Point(13, 101);
			this.frustumCulling.Name = "frustumCulling";
			this.frustumCulling.Size = new System.Drawing.Size(243, 17);
			this.frustumCulling.TabIndex = 8;
			this.frustumCulling.Text = "Frustrum culling (none/cam_n_rem/remember)";
			this.frustumCulling.ThreeState = true;
			this.frustumCulling.UseVisualStyleBackColor = false;
			// 
			// showMap
			// 
			this.showMap.AutoSize = true;
			this.showMap.BackColor = System.Drawing.Color.Transparent;
			this.showMap.Checked = true;
			this.showMap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.showMap.Location = new System.Drawing.Point(13, 147);
			this.showMap.Name = "showMap";
			this.showMap.Size = new System.Drawing.Size(77, 17);
			this.showMap.TabIndex = 10;
			this.showMap.Text = "Show Map";
			this.showMap.UseVisualStyleBackColor = false;
			// 
			// showModels
			// 
			this.showModels.AutoSize = true;
			this.showModels.BackColor = System.Drawing.Color.Transparent;
			this.showModels.Checked = true;
			this.showModels.CheckState = System.Windows.Forms.CheckState.Checked;
			this.showModels.Location = new System.Drawing.Point(13, 170);
			this.showModels.Name = "showModels";
			this.showModels.Size = new System.Drawing.Size(90, 17);
			this.showModels.TabIndex = 11;
			this.showModels.Text = "Show Models";
			this.showModels.UseVisualStyleBackColor = false;
			// 
			// transformModels
			// 
			this.transformModels.AutoSize = true;
			this.transformModels.BackColor = System.Drawing.Color.Transparent;
			this.transformModels.Checked = true;
			this.transformModels.CheckState = System.Windows.Forms.CheckState.Checked;
			this.transformModels.Location = new System.Drawing.Point(13, 193);
			this.transformModels.Name = "transformModels";
			this.transformModels.Size = new System.Drawing.Size(109, 17);
			this.transformModels.TabIndex = 12;
			this.transformModels.Text = "Transform models";
			this.transformModels.UseVisualStyleBackColor = false;
			// 
			// checkBox1
			// 
			this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox1.AutoSize = true;
			this.checkBox1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox1.Location = new System.Drawing.Point(13, 46);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(87, 23);
			this.checkBox1.TabIndex = 13;
			this.checkBox1.Text = "PAUSE GAME";
			this.checkBox1.UseVisualStyleBackColor = false;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// mapDiffuseRegions
			// 
			this.mapDiffuseRegions.AutoSize = true;
			this.mapDiffuseRegions.BackColor = System.Drawing.Color.Transparent;
			this.mapDiffuseRegions.Checked = true;
			this.mapDiffuseRegions.CheckState = System.Windows.Forms.CheckState.Checked;
			this.mapDiffuseRegions.Location = new System.Drawing.Point(13, 216);
			this.mapDiffuseRegions.Name = "mapDiffuseRegions";
			this.mapDiffuseRegions.Size = new System.Drawing.Size(162, 17);
			this.mapDiffuseRegions.TabIndex = 14;
			this.mapDiffuseRegions.Text = "Map Models Diffuse Regions";
			this.mapDiffuseRegions.UseVisualStyleBackColor = false;
			// 
			// checkBox2
			// 
			this.checkBox2.Appearance = System.Windows.Forms.Appearance.Button;
			this.checkBox2.AutoSize = true;
			this.checkBox2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.checkBox2.Checked = true;
			this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.checkBox2.Location = new System.Drawing.Point(13, 14);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(49, 23);
			this.checkBox2.TabIndex = 15;
			this.checkBox2.Text = "MENU";
			this.checkBox2.UseVisualStyleBackColor = false;
			this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
			// 
			// fog
			// 
			this.fog.AutoSize = true;
			this.fog.BackColor = System.Drawing.Color.Transparent;
			this.fog.Checked = true;
			this.fog.CheckState = System.Windows.Forms.CheckState.Checked;
			this.fog.Location = new System.Drawing.Point(13, 240);
			this.fog.Name = "fog";
			this.fog.Size = new System.Drawing.Size(44, 17);
			this.fog.TabIndex = 16;
			this.fog.Text = "Fog";
			this.fog.UseVisualStyleBackColor = false;
			// 
			// cheatEngine
			// 
			this.cheatEngine.AutoSize = true;
			this.cheatEngine.BackColor = System.Drawing.Color.Transparent;
			this.cheatEngine.Checked = true;
			this.cheatEngine.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cheatEngine.Location = new System.Drawing.Point(83, 20);
			this.cheatEngine.Name = "cheatEngine";
			this.cheatEngine.Size = new System.Drawing.Size(40, 17);
			this.cheatEngine.TabIndex = 18;
			this.cheatEngine.Text = "CE";
			this.cheatEngine.UseVisualStyleBackColor = false;
			this.cheatEngine.Visible = false;
			// 
			// mapAlphaGlow
			// 
			this.mapAlphaGlow.AutoSize = true;
			this.mapAlphaGlow.BackColor = System.Drawing.Color.Transparent;
			this.mapAlphaGlow.Checked = true;
			this.mapAlphaGlow.CheckState = System.Windows.Forms.CheckState.Checked;
			this.mapAlphaGlow.Location = new System.Drawing.Point(13, 262);
			this.mapAlphaGlow.Name = "mapAlphaGlow";
			this.mapAlphaGlow.Size = new System.Drawing.Size(313, 17);
			this.mapAlphaGlow.TabIndex = 20;
			this.mapAlphaGlow.Text = "Map alpha channel glow (Experimental, dunno when to glow)";
			this.mapAlphaGlow.ThreeState = true;
			this.mapAlphaGlow.UseVisualStyleBackColor = false;
			// 
			// multipleRenders
			// 
			this.multipleRenders.AutoSize = true;
			this.multipleRenders.BackColor = System.Drawing.Color.Transparent;
			this.multipleRenders.Location = new System.Drawing.Point(13, 78);
			this.multipleRenders.Name = "multipleRenders";
			this.multipleRenders.Size = new System.Drawing.Size(100, 17);
			this.multipleRenders.TabIndex = 22;
			this.multipleRenders.Text = "Multiple renders";
			this.multipleRenders.UseVisualStyleBackColor = false;
			this.multipleRenders.CheckedChanged += new System.EventHandler(this.multipleRenders_CheckedChanged);
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.DecimalPlaces = 3;
			this.numericUpDown1.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericUpDown1.Location = new System.Drawing.Point(55, 495);
			this.numericUpDown1.Maximum = new decimal(new int[] {
            300000000,
            0,
            0,
            0});
			this.numericUpDown1.Minimum = new decimal(new int[] {
            30000000,
            0,
            0,
            -2147483648});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(120, 20);
			this.numericUpDown1.TabIndex = 23;
			this.numericUpDown1.Visible = false;
			// 
			// numericUpDown2
			// 
			this.numericUpDown2.DecimalPlaces = 3;
			this.numericUpDown2.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
			this.numericUpDown2.Location = new System.Drawing.Point(228, 495);
			this.numericUpDown2.Maximum = new decimal(new int[] {
            300000000,
            0,
            0,
            0});
			this.numericUpDown2.Minimum = new decimal(new int[] {
            30000000,
            0,
            0,
            -2147483648});
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new System.Drawing.Size(120, 20);
			this.numericUpDown2.TabIndex = 25;
			this.numericUpDown2.Visible = false;
			// 
			// numericUpDown4
			// 
			this.numericUpDown4.Location = new System.Drawing.Point(228, 594);
			this.numericUpDown4.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.numericUpDown4.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
			this.numericUpDown4.Name = "numericUpDown4";
			this.numericUpDown4.Size = new System.Drawing.Size(120, 20);
			this.numericUpDown4.TabIndex = 27;
			this.numericUpDown4.Visible = false;
			// 
			// numericUpDown3
			// 
			this.numericUpDown3.Location = new System.Drawing.Point(55, 594);
			this.numericUpDown3.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.numericUpDown3.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            -2147483648});
			this.numericUpDown3.Name = "numericUpDown3";
			this.numericUpDown3.Size = new System.Drawing.Size(120, 20);
			this.numericUpDown3.TabIndex = 26;
			this.numericUpDown3.Visible = false;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(55, 529);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 28;
			this.label1.Text = "label1";
			this.label1.Visible = false;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(226, 528);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 29;
			this.label2.Text = "label2";
			this.label2.Visible = false;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(55, 629);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(35, 13);
			this.label3.TabIndex = 30;
			this.label3.Text = "label3";
			this.label3.Visible = false;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(226, 629);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(35, 13);
			this.label4.TabIndex = 31;
			this.label4.Text = "label4";
			this.label4.Visible = false;
			// 
			// numericUpDown5
			// 
			this.numericUpDown5.Location = new System.Drawing.Point(527, 492);
			this.numericUpDown5.Name = "numericUpDown5";
			this.numericUpDown5.Size = new System.Drawing.Size(120, 20);
			this.numericUpDown5.TabIndex = 32;
			this.numericUpDown5.Visible = false;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(524, 518);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(35, 13);
			this.label5.TabIndex = 33;
			this.label5.Text = "label5";
			this.label5.Visible = false;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(524, 596);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(35, 13);
			this.label6.TabIndex = 35;
			this.label6.Text = "label6";
			this.label6.Visible = false;
			// 
			// numericUpDown6
			// 
			this.numericUpDown6.Location = new System.Drawing.Point(527, 570);
			this.numericUpDown6.Name = "numericUpDown6";
			this.numericUpDown6.Size = new System.Drawing.Size(120, 20);
			this.numericUpDown6.TabIndex = 34;
			this.numericUpDown6.Visible = false;
			// 
			// interframeInterpolate
			// 
			this.interframeInterpolate.AutoSize = true;
			this.interframeInterpolate.BackColor = System.Drawing.Color.Transparent;
			this.interframeInterpolate.Location = new System.Drawing.Point(13, 284);
			this.interframeInterpolate.Name = "interframeInterpolate";
			this.interframeInterpolate.Size = new System.Drawing.Size(236, 17);
			this.interframeInterpolate.TabIndex = 36;
			this.interframeInterpolate.Text = "Interframe models interpolated (Experimental)";
			this.interframeInterpolate.UseVisualStyleBackColor = false;
			this.interframeInterpolate.CheckedChanged += new System.EventHandler(this.interframeInterpolate_CheckedChanged);
			// 
			// texturePatches
			// 
			this.texturePatches.AutoSize = true;
			this.texturePatches.BackColor = System.Drawing.Color.Transparent;
			this.texturePatches.Checked = true;
			this.texturePatches.CheckState = System.Windows.Forms.CheckState.Checked;
			this.texturePatches.Location = new System.Drawing.Point(13, 329);
			this.texturePatches.Name = "texturePatches";
			this.texturePatches.Size = new System.Drawing.Size(103, 17);
			this.texturePatches.TabIndex = 37;
			this.texturePatches.Text = "Texture patches";
			this.texturePatches.UseVisualStyleBackColor = false;
			// 
			// smartBlending
			// 
			this.smartBlending.AutoSize = true;
			this.smartBlending.BackColor = System.Drawing.Color.Transparent;
			this.smartBlending.Checked = true;
			this.smartBlending.CheckState = System.Windows.Forms.CheckState.Checked;
			this.smartBlending.Location = new System.Drawing.Point(13, 307);
			this.smartBlending.Name = "smartBlending";
			this.smartBlending.Size = new System.Drawing.Size(257, 17);
			this.smartBlending.TabIndex = 38;
			this.smartBlending.Text = "Smart blending (Experimental, more render layers)";
			this.smartBlending.UseVisualStyleBackColor = false;
			// 
			// GLForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
			this.ClientSize = new System.Drawing.Size(1266, 675);
			this.Controls.Add(this.smartBlending);
			this.Controls.Add(this.texturePatches);
			this.Controls.Add(this.interframeInterpolate);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.numericUpDown6);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.numericUpDown5);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.numericUpDown4);
			this.Controls.Add(this.numericUpDown3);
			this.Controls.Add(this.numericUpDown2);
			this.Controls.Add(this.numericUpDown1);
			this.Controls.Add(this.multipleRenders);
			this.Controls.Add(this.mapAlphaGlow);
			this.Controls.Add(this.cheatEngine);
			this.Controls.Add(this.fog);
			this.Controls.Add(this.checkBox2);
			this.Controls.Add(this.mapDiffuseRegions);
			this.Controls.Add(this.checkBox1);
			this.Controls.Add(this.transformModels);
			this.Controls.Add(this.showModels);
			this.Controls.Add(this.showMap);
			this.Controls.Add(this.meshSkipRenders);
			this.Controls.Add(this.frustumCulling);
			this.Controls.Add(this.glControl3);
			this.Controls.Add(this.glControl2);
			this.Controls.Add(this.glControl1);
			this.ForeColor = System.Drawing.Color.White;
			this.Margin = new System.Windows.Forms.Padding(1);
			this.Name = "GLForm";
			this.Padding = new System.Windows.Forms.Padding(10);
			this.ShowInTaskbar = false;
			this.Text = "";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GLForm_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown4)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown3)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown5)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown6)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public GLControl glControl1;
		private GLControl glControl2;
		private GLControl glControl3;
		private System.Windows.Forms.CheckBox meshSkipRenders;
		private System.Windows.Forms.CheckBox frustumCulling;
		private System.Windows.Forms.CheckBox showMap;
		private System.Windows.Forms.CheckBox showModels;
		private System.Windows.Forms.CheckBox transformModels;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.CheckBox mapDiffuseRegions;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.CheckBox fog;
		private System.Windows.Forms.CheckBox cheatEngine;
		private System.Windows.Forms.CheckBox mapAlphaGlow;
		private System.Windows.Forms.CheckBox multipleRenders;
		public System.Windows.Forms.NumericUpDown numericUpDown1;
		public System.Windows.Forms.NumericUpDown numericUpDown2;
		public System.Windows.Forms.NumericUpDown numericUpDown4;
		public System.Windows.Forms.NumericUpDown numericUpDown3;
		public System.Windows.Forms.Label label1;
		public System.Windows.Forms.Label label2;
		public System.Windows.Forms.Label label3;
		public System.Windows.Forms.Label label4;
		public System.Windows.Forms.NumericUpDown numericUpDown5;
		public System.Windows.Forms.Label label5;
		public System.Windows.Forms.Label label6;
		public System.Windows.Forms.NumericUpDown numericUpDown6;
		private System.Windows.Forms.CheckBox interframeInterpolate;
		public System.Windows.Forms.CheckBox texturePatches;
		public System.Windows.Forms.CheckBox smartBlending;
	}
}