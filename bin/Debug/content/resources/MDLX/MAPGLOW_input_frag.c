#version 330 compatibility
uniform sampler2D texture0;

in vec3 f_position;
in vec2 f_texcoord;
in vec4 f_color;

uniform int glow_mesh;

void main()
{
	vec4 color = texture(texture0, f_texcoord);
	
	if (glow_mesh == 0)
	{
		color = vec4(0,0,0,1);
	}
	else
	{
		if (color.w < 0.999)
			color = vec4(0,0,0,1);
		else
		{
			color *= color.w;
			color.w = 0.5;
		}
	}
	
	gl_FragColor = color;
}