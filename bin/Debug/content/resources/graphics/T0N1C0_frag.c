#version 330 compatibility

in vec3 f_position;
in vec3 f_normal;


uniform vec3 light0_position;
uniform vec3 light0_color;
uniform float light0_diffuse_strength;
in float f_alphatest;
uniform int fog_mode;
in vec4 f_colormultiplicator;
uniform vec3 fog_color;
uniform vec4 fog_near_far_min_max;

void main()
{
	mat4 inverseViewMatrix = inverse(gl_ModelViewMatrix);
	vec3 eyePosition = vec3(inverseViewMatrix[3][0], inverseViewMatrix[3][1], inverseViewMatrix[3][2]);
	vec4 color = f_colormultiplicator;
	
	if ((f_alphatest > 0.5 && f_alphatest < 1.5) && (fog_mode > 0 || color.w <0.95))
		discard;
	
	if ((f_alphatest > 1.5 && f_alphatest < 2.5) && (fog_mode > 0 || (color.w < 0.01 || color.w >=0.95)))
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
	
    vec3 ambient = light0_diffuse_strength * light0_color;
	vec3 normal = normalize(f_normal);

	vec3 lightDir = normalize(light0_position - f_position);
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * light0_color;
	vec3 amb_n_diffuse = (ambient + diffuse);

	gl_FragColor = vec4(amb_n_diffuse, 1.0) * color;
}