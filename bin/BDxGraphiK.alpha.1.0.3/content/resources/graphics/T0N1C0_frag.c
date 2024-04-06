#version 330 compatibility

in vec3 f_position;
in vec3 f_normal;


uniform vec3 light0_position;
uniform vec3 light0_color;
uniform float light0_ambiant_strength;
uniform int alphatest;
uniform int enable_fog;
uniform vec4 colormultiplicator;
uniform vec3 fog_position;
uniform vec3 fog_color;
uniform float fog_near;
uniform float fog_far;
uniform float fog_min;
uniform float fog_max;

void main()
{
	vec4 color = colormultiplicator;
	
	if (enable_fog == 1)
	{
		vec3 beforeNear = vec3(color.x, color.y, color.z) * (fog_max/255.0) + vec3(fog_color.x, fog_color.y, fog_color.z) * (1-(fog_max/255.0));
		vec3 afterFar = vec3(color.x, color.y, color.z)  * (fog_min/255.0) + vec3(fog_color.x, fog_color.y, fog_color.z) * (1-(fog_min/255.0));
		
		float distance_fog_pos = distance(fog_position, f_position);
		float fog = 0.0;
		
		
		if (distance_fog_pos > fog_near)
		{
			fog = (distance_fog_pos - fog_near)/(fog_far-fog_near);
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
	
	if (alphatest == 1 && color.w <0.95)
		discard;
	if (alphatest == 2 && color.w >=0.95)
		discard;
	
	if (alphatest == 2)
		discard;
    vec3 ambient = light0_ambiant_strength * light0_color;
	vec3 normal = normalize(f_normal);

	vec3 lightDir = normalize(light0_position - f_position);
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * light0_color;
	vec3 amb_n_diffuse = (ambient + diffuse);

	gl_FragColor = vec4(amb_n_diffuse, 1.0) * color;
}