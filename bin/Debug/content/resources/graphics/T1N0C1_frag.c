#version 330 compatibility
uniform sampler2D texture0;

in vec3 f_position;
in vec2 f_texcoord;
in vec4 f_color;
in float f_alphatest;
in float f_skinned;


uniform int fog_mode;
uniform vec4 colormultiplicator;
uniform vec3 fog_color;
uniform vec4 fog_near_far_min_max;

void main()
{
	mat4 inverseViewMatrix = inverse(gl_ModelViewMatrix);
	vec3 eyePosition = vec3(inverseViewMatrix[3][0], inverseViewMatrix[3][1], inverseViewMatrix[3][2]);
	vec4 fcolor = f_color * vec4(1,1,1,3.984375) * colormultiplicator;
	vec4 color = texture(texture0, f_texcoord) * fcolor;
	
	if ((f_alphatest > 0.5 && f_alphatest < 1.5) && (f_skinned > 0 || fog_mode > 0 || color.w <0.95))
		discard;
	
	if ((f_alphatest > 1.5 && f_alphatest < 2.5) && (f_skinned > 0 || fog_mode > 0 || (color.w < 0.01 || color.w >=0.95)))
		discard;
	
	if ((f_alphatest > 2.5 && f_alphatest < 3.5) && (color.w <0.95))
		discard;
	
	if ((f_alphatest > 3.5 && f_alphatest < 4.5) && (fog_mode == 0 || (color.w < 0.01 || color.w >=0.95)))
		discard;
	
	float near = fog_near_far_min_max.x;
	float far = fog_near_far_min_max.y;
	if (fog_mode > 0 && isnan(near) == false)
	{
		vec3 beforeNear = vec3(color.x, color.y, color.z) * (fog_near_far_min_max.w/255.0) + vec3(fog_color.x, fog_color.y, fog_color.z) * (1-(fog_near_far_min_max.w/255.0));
		vec3 afterFar = vec3(color.x, color.y, color.z)  * (fog_near_far_min_max.z/255.0) + vec3(fog_color.x, fog_color.y, fog_color.z) * (1-(fog_near_far_min_max.z/255.0));
		
		vec3 axis = vec3(1,1,1);
		if (fog_mode == 2)
			axis.y = 0;
		
		float distance_fog_pos = distance(eyePosition*axis, f_position*axis);
		float fog = 0.0;
		
		
		if (distance_fog_pos > near)
		{
			fog = (distance_fog_pos - near)/(far-near);
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