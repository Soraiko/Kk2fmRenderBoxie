#version 330 compatibility
uniform sampler2D texture0;
uniform sampler2D bump_mapping;

in vec3 f_position;
in vec2 f_texcoord;
in vec3 f_normal;
in vec4 f_color;

uniform vec3 light0_position;
uniform vec3 light0_color;
uniform float light0_diffuse_strength;
in float f_alphatest;
uniform int fog_mode;
in vec4 f_colormultiplicator;
uniform vec4 fog_color;
uniform vec4 fog_near_far_min_max;

void main()
{
	vec2 ftexcoord = f_texcoord;
	mat4 inverseViewMatrix = inverse(gl_ModelViewMatrix);
	vec3 eyePosition = vec3(inverseViewMatrix[3][0], inverseViewMatrix[3][1], inverseViewMatrix[3][2]);
	
	vec4 fcolor = f_color * 2 * f_colormultiplicator;
	
	vec4 color = texture(texture0, ftexcoord)*vec4(1,1,1,2) * fcolor;
	
	
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
		color *= fog_color.w;
	}
	
    vec3 ambient = light0_diffuse_strength * light0_color;
	vec3 normal = texture(bump_mapping, ftexcoord).rgb;
	
	normal = normal * 2.0 - 1.0;
	normal = normalize(normal);

	/* Source from https://stackoverflow.com/users/607131/kvark */
	// compute derivations of the world position
	vec3 p_dx = dFdx(f_position);
	vec3 p_dy = dFdy(f_position);
	// compute derivations of the texture coordinate
	vec2 tc_dx = dFdx(-ftexcoord);
	vec2 tc_dy = dFdy(-ftexcoord);
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
	
	gl_FragColor = vec4(amb_n_diffuse, 1.0) * color;
}