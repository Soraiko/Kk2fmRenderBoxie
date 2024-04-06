#version 330 compatibility
uniform sampler2D texture0;
uniform sampler2D bump_mapping;

in vec3 f_position;
in vec2 f_texcoord;
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
	vec4 color = texture(texture0, f_texcoord)*colormultiplicator;
	
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
	
    vec3 ambient = light0_ambiant_strength * light0_color;
	vec3 normal = texture(bump_mapping, f_texcoord).rgb;
	
	normal = normal * 2.0 - 1.0;
	normal = normalize(normal);

	/* Source from https://stackoverflow.com/users/607131/kvark */
	// compute derivations of the world position
	vec3 p_dx = dFdx(f_position);
	vec3 p_dy = dFdy(f_position);
	// compute derivations of the texture coordinate
	vec2 tc_dx = dFdx(-f_texcoord);
	vec2 tc_dy = dFdy(-f_texcoord);
	// compute initial tangent and bi-tangent
	vec3 t = normalize(tc_dy.y * p_dx - tc_dx.y * p_dy);
	vec3 b = normalize(tc_dy.x * p_dx - tc_dx.x * p_dy); // sign inversion
	// get new tangent from a given mesh normal
	vec3 n = normalize(f_normal);
	vec3 x = cross(n, t);
	t = cross(x, n);
	t = normalize(t);
	// get updated bi-tangent
	x = cross(b, n);
	b = cross(n, x);
	b = normalize(b);
	mat3 tbn = mat3(t, b, n);
	normal = normalize(tbn * normal);

	vec3 lightDir = normalize(light0_position - f_position);
	float diff = max(dot(normal, lightDir), 0.0);
	vec3 diffuse = diff * light0_color;
	vec3 amb_n_diffuse = (ambient + diffuse);

	gl_FragColor = vec4(amb_n_diffuse, 1.0) * color;
}