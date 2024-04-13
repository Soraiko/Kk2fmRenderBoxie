#version 330 compatibility
uniform sampler2D texture0;

in vec3 f_position;
in vec2 f_texcoord;
in vec4 f_color;
in float f_skinned;

uniform vec4 colormultiplicator;
uniform int has_color;

void main()
{
	vec4 color = texture(texture0, f_texcoord) * vec4(1,1,1,2) * f_color * colormultiplicator;
	if (has_color == 1)
	{
		color*=2;
	}
	
	mat4 inverseViewMatrix = inverse(gl_ModelViewMatrix);
	vec3 eyePosition = vec3(inverseViewMatrix[3][0], inverseViewMatrix[3][1], inverseViewMatrix[3][2]);
	
	if (color.a < 1.1)
	{
		color = vec4(0,0,0,1);
	}
	
	
	gl_FragColor = color;
}