#version 330 compatibility
uniform sampler2D texture0;

in vec3 f_position;
in vec2 f_texcoord;
in vec4 f_color;

uniform int glow_mesh;

void main()
{
	vec4 color = texture(texture0, f_texcoord);
	
	if (glow_mesh == 0 || color.w < 0.999)
		discard;
	
	mat4 inverseViewMatrix = inverse(gl_ModelViewMatrix);
	vec3 eyePosition = vec3(inverseViewMatrix[3][0], inverseViewMatrix[3][1], inverseViewMatrix[3][2]);
	
	if (distance(eyePosition, f_position) > 10000 || distance(f_color.xyz, vec3(1,1,1)) > 0.01)
	{
		color = vec4(0,0,0,1);
	}
	else
	{
		color *= color.w;
		//color = f_color * color;
		color.w = 0.5;
	}
	
	gl_FragColor = color;
}