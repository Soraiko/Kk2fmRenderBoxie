#define DEAL_ROTATE_WITH_DECOMPUTED
#define DEAL_ROTATE_WITH_DECOMPUTED_ONL
using Assimp;
using Assimp.Unmanaged;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace BDxGraphiK
{
	public class AnimatedController
	{
		public Object3D Model;
		public Moveset CurrentMoveset;

		public Matrix4[] MatricesBuffer;
		public Matrix4[] InterpolationBuffer;

		public int AnimationIndex = -1;
		int oldAnimationIndex = -1;

		public float AnimationFrame = 0f;
		float oldAnimationFrame = 0f;

		float Interpolation = 1f;
		public float FrameStep = 1f;
		
		public float InterpolationStep = 0.15f;

		public AnimatedController(string setFilename)
		{
			MultilineEnumGetter enumGetter = new MultilineEnumGetter(setFilename, '=');
			string modelFname;
			if (enumGetter.GetSingleValue("Model", out modelFname))
			{
				if (Path.GetExtension(modelFname) == ".bin")
					this.Model = Object3D.FromBinary(modelFname);
				else if (Path.GetExtension(modelFname) == ".mdlx")
					this.Model = null;// new MDLX(modelFname);
				else
					this.Model = new Object3D(modelFname);
				string movesetFname;
				if (enumGetter.GetSingleValue("Moveset", out movesetFname))
				{
					this.CurrentMoveset = new Moveset(movesetFname, this.Model);
					/*for (int i=0;i<this.CurrentMoveset.AnimationBinaries.Count;i++)
					{
						this.CurrentMoveset.AnimationBinaries[i].ExportDAE(modelFname, Path.GetDirectoryName(modelFname)+@"\"+ Path.GetFileNameWithoutExtension(modelFname) + "-" + i.ToString("d3") + ".dae", this.Model);
					}*/
				}
			}
		}

		public void Draw()
		{

		}

		public enum ComputeType
		{
			None = 0,
			BetweenFrames = 1,
			BetweenAnimations = 2
		}

		public void Update()
		{
			var skeleton = this.Model.Skeleton;
			if (skeleton == null)
				return;
			int jointsCount = skeleton.Joints.Count;
			if (jointsCount == 0)
				return;

			if (this.MatricesBuffer == null)
			{
				this.MatricesBuffer = new Matrix4[jointsCount];
				this.InterpolationBuffer = new Matrix4[jointsCount];
			}

			if (this.CurrentMoveset != null)
			{
				var animationBinaries = this.CurrentMoveset.AnimationBinaries;
				if (this.AnimationIndex < 0 || this.AnimationIndex >= animationBinaries.Count)
				{
					skeleton.ResetTransforms();
				}
				else
				{
					/* all ok context */

					var animation = animationBinaries[this.AnimationIndex];

					if (this.AnimationIndex != this.oldAnimationIndex && this.oldAnimationIndex>-1)
					{
						this.Interpolation = 0f;
						this.AnimationFrame = 0f;

						for (int i = 0; i < jointsCount; i++)
							this.InterpolationBuffer[i] = this.MatricesBuffer[i];
#if (DEAL_ROTATE_WITH_DECOMPUTED)
						skeleton.ReverseComputedMatrices(ref this.InterpolationBuffer, 0);
#endif
#if (DEAL_ROTATE_WITH_DECOMPUTED_ONLY)
						skeleton.ReverseComputedMatrices(ref this.InterpolationBuffer, 0);
#endif
					}

					int current_frame = (int)Math.Floor(this.AnimationFrame);
					int next_frame = (current_frame + 1);
					if (next_frame >= animation.FrameCount)
					{
						if (animation.LoopFrame == animation.FrameCount)
							next_frame = current_frame;
						else
							next_frame = animation.LoopFrame;
					}

					float decimals = this.AnimationFrame - current_frame;
					bool decimal_ = decimals < 0.000001;

					int current_position = current_frame * jointsCount;
					int next_position = next_frame * jointsCount;

					bool interpolate_ = this.Interpolation < 1f;



					if (this.AnimationFrame < animation.LoopFrame || animation.LoopFrame != animation.FrameCount)
					{
						ComputeType compute_type = ComputeType.None;

						if (interpolate_)
							compute_type = ComputeType.BetweenAnimations;
						else if (!decimal_)
							compute_type = ComputeType.BetweenFrames;

						Matrix4[] animationData = animation.AnimationData;

#if (DEAL_ROTATE_WITH_DECOMPUTED)
						if (compute_type == ComputeType.BetweenAnimations)
							animationData = animation.ReversedAnimationData;
#endif
#if (DEAL_ROTATE_WITH_DECOMPUTED_ONLY)
						animationData = animation.ReversedAnimationData;
#endif

						for (int i = 0; i < jointsCount; i++)
						{
							this.MatricesBuffer[i] = animationData[current_position++];

							var matrix_a = Matrix4.Zero;
							var matrix_b = Matrix4.Zero;

							var one = 0f;
							var one_minus = 0f;

							if (interpolate_)
							{
								matrix_a = this.InterpolationBuffer[i];
								matrix_b = this.MatricesBuffer[i];
								one = this.Interpolation;
								one_minus = 1f - this.Interpolation;
							}
							else
							{
								if (!decimal_)
								{
									matrix_a = this.MatricesBuffer[i];
									matrix_b = animationData[next_position++];
									one = decimals;
									one_minus = 1f - decimals;
								}
							}

							if (compute_type == ComputeType.BetweenAnimations)
							{

								var translation_ab_v3 = matrix_a.ExtractTranslation() * one_minus + matrix_b.ExtractTranslation() * one;

								var rotation_a_q = matrix_a.ExtractRotation(true);
								var rotation_b_q = matrix_b.ExtractRotation(true);

								var rotation_ab_q = (rotation_a_q * one_minus + rotation_b_q * one);


								var scale_ab_v3 = matrix_a.ExtractScale() * one_minus + matrix_b.ExtractScale() * one;

								var translation_ab_matrix = Matrix4.CreateTranslation(translation_ab_v3);
								var rotation_ab_matrix = Matrix4.CreateFromQuaternion(rotation_ab_q);
								var scale_ab_matrix = Matrix4.CreateScale(scale_ab_v3);


								this.MatricesBuffer[i] = scale_ab_matrix * rotation_ab_matrix * translation_ab_matrix;
							}
							else if (compute_type == ComputeType.BetweenFrames)
							{
								this.MatricesBuffer[i] = matrix_a * one_minus + matrix_b * one;
							}
						}

#if (DEAL_ROTATE_WITH_DECOMPUTED_ONLY)
						skeleton.ComputeMatrices(ref this.MatricesBuffer, 0);
#elif (DEAL_ROTATE_WITH_DECOMPUTED)
						if (compute_type == ComputeType.BetweenAnimations)
							skeleton.ComputeMatrices(ref this.MatricesBuffer, 0);
#endif

						skeleton.PassTransforms(ref this.MatricesBuffer);
					}

					if (interpolate_ && this.FrameStep > 0)
						this.Interpolation += this.InterpolationStep;

					this.AnimationFrame += this.FrameStep;

					if (this.AnimationFrame >= animation.FrameCount)
						this.AnimationFrame = Mathematics.Floor(animation.LoopFrame + (this.AnimationFrame % animation.FrameCount));

					this.oldAnimationIndex = this.AnimationIndex;
					this.oldAnimationFrame = this.AnimationFrame;
				}
			}

			skeleton.SendMatricesToUniformObject();
		}
	}
}
