#version 330 compatibility
uniform sampler2D texture0;
uniform sampler2D patch0;
uniform sampler2D patch1;
uniform sampler2D patch2;
uniform sampler2D patch3;

in vec3 f_position;
in vec2 f_texcoord;
in vec4 f_color;


in float f_alphatest;
in float f_skinned;

uniform int fog_mode;
in vec4 f_colormultiplicator;
uniform vec3 fog_color;
uniform vec4 fog_near_far_min_max;

uniform int has_patch;

uniform vec4 patch0_x_y_w_h;
uniform float patch0_o_c;

uniform vec4 patch1_x_y_w_h;
uniform float patch1_o_c;

uniform vec4 patch2_x_y_w_h;
uniform float patch2_o_c;

uniform vec4 patch3_x_y_w_h;
uniform float patch3_o_c;

uniform vec4 patch4_x_y_w_h;
uniform float patch4_o_c;

uniform vec2 patch_orw_orh;


uniform int patch0_index;
uniform int patch1_index;
uniform int patch2_index;
uniform int patch3_index;

void main()
{
	vec2 ftexcoord = f_texcoord;
	vec4 fcolor = f_color * vec4(1,1,1,3.984375)*f_colormultiplicator;
	
	vec4 originalcolor = texture(texture0, ftexcoord) * fcolor;
	vec4 color = originalcolor;
	
	
	mat4 inverseViewMatrix = inverse(gl_ModelViewMatrix);
	vec3 eyePosition = vec3(inverseViewMatrix[3][0], inverseViewMatrix[3][1], inverseViewMatrix[3][2]);
	
	if (has_patch == 1 && patch_orw_orh.length>0)
	{
		for (int i = 0; i < 4; ++i)
		{
			int patch_index = -1;
			switch (i)
			{
				case 0: patch_index = patch0_index; break;
				case 1: patch_index = patch1_index; break;
				case 2: patch_index = patch2_index; break;
				case 3: patch_index = patch3_index; break;
			}
			
			if (patch_index>-1)
			{
				float patch_o_c = patch0_o_c;
				vec4 patch_x_y_w_h = patch0_x_y_w_h;
				
				if (i>0)
				switch (i)
				{
					case 1: patch_o_c = patch1_o_c; patch_x_y_w_h = patch1_x_y_w_h; break;
					case 2: patch_o_c = patch2_o_c; patch_x_y_w_h = patch2_x_y_w_h; break;
					case 3: patch_o_c = patch3_o_c; patch_x_y_w_h = patch3_x_y_w_h; break;
				}
			
				float count = patch_o_c;
				bool horizontal = count < 0;
				if (horizontal) count = -count;
				
				float left = patch_x_y_w_h.x / patch_orw_orh.x;
				float right = left + patch_x_y_w_h.z / patch_orw_orh.x;
				float top = patch_x_y_w_h.y / patch_orw_orh.y;
				float bottom = top + (patch_x_y_w_h.w / count) / patch_orw_orh.y;
				
				
				
				float offsetLeft = 0.5/patch_orw_orh.x;
				float offsetRight = -0.5/patch_orw_orh.x;
				
				float offsetTop = 0.5/patch_orw_orh.y;
				float offsetBottom = -0.5/patch_orw_orh.y;
				
				if (
				ftexcoord.x > left
				&& ftexcoord.x < right
				&& ftexcoord.y > top
				&& ftexcoord.y < bottom
				)
				{
					vec2 newLoc = vec2((ftexcoord.x-left)/(right-left), (ftexcoord.y-top)/(bottom-top));
					newLoc.y = newLoc.y/count;
					newLoc.y += (1/count) * patch_index;
					
					switch (i)
					{
						case 0: color = texture(patch0, newLoc) * f_colormultiplicator; break;
						case 1: color = texture(patch1, newLoc) * f_colormultiplicator; break;
						case 2: color = texture(patch2, newLoc) * f_colormultiplicator; break;
						case 3: color = texture(patch3, newLoc) * f_colormultiplicator; break;
					}
					if (color.a>0.5 && 
						(ftexcoord.x < left
						|| ftexcoord.x > right+offsetRight
						|| ftexcoord.y < top+ offsetTop
						|| ftexcoord.y > bottom+offsetBottom))
						{
								color = originalcolor;
						}
				}
			}
		}
	}
	
	if ((f_alphatest > 0.5 && f_alphatest < 1.5) && (fog_mode > 0 || color.w <0.95))
		discard;
	
	if ((f_alphatest > 1.5 && f_alphatest < 2.5) && (f_skinned > 0 || fog_mode > 0 || (color.w < 0.01 || color.w >=0.95)))
		discard;
	
	if ((f_alphatest > 2.5 && f_alphatest < 3.5) && (color.w <0.95))
		discard;
	
	if ((f_alphatest > 3.5 && f_alphatest < 4.5) && (fog_mode == 0 || (color.w < 0.01 || color.w >=0.95)))
		discard;
	
	if ((f_alphatest > 4.5 && f_alphatest < 5.5) && (color.w <0.95))
		discard;
	
	if ((f_alphatest > 5.5 && f_alphatest < 6.5) && (color.w < 0.01 || color.w >=0.95))
		discard;
	
	if (fog_mode > 0 && isnan(fog_near_far_min_max.x) == false)
	{
		vec3 beforeNear = vec3(color.x, color.y, color.z) * (fog_near_far_min_max.w/255.0) + vec3(fog_color.x, fog_color.y, fog_color.z) * (1-(fog_near_far_min_max.w/255.0));
		vec3 afterFar = vec3(color.x, color.y, color.z)  * (fog_near_far_min_max.z/255.0) + vec3(fog_color.x, fog_color.y, fog_color.z) * (1-(fog_near_far_min_max.z/255.0));
		
		vec3 axis = vec3(1,1,1);
		if (fog_mode == 2)
			axis.y = 0;
		
		float distance_fog_pos = distance(eyePosition*axis, f_position*axis);
		float fog = 0.0;
		
		
		if (distance_fog_pos > fog_near_far_min_max.x)
		{
			fog = (distance_fog_pos - fog_near_far_min_max.x)/(fog_near_far_min_max.y-fog_near_far_min_max.x);
			if (fog > 1.0)
			{
				fog = 1.0;
			}
		}
		vec3 output = beforeNear * (1.0-fog) + afterFar * fog;
		
		color.x = output.x;
		color.y = output.y;
		color.z = output.z;
	}
	
	
	gl_FragColor = color;
}