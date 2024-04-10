#version 330 compatibility
uniform sampler2D texture0;

in vec3 f_position;
in vec2 f_texcoord;
in vec4 f_color;
in float f_skinned;

in vec4 f_colormultiplicator;

void main()
{
	vec4 color = vec4(1,1,1,1);
	
	mat4 inverseViewMatrix = inverse(gl_ModelViewMatrix);
	vec3 eyePosition = vec3(inverseViewMatrix[3][0], inverseViewMatrix[3][1], inverseViewMatrix[3][2]);
	
	if (f_skinned > 0 && f_colormultiplicator.w < 0.999)
	{
		vec4 texColor = texture(texture0, f_texcoord);
		if (texColor.a<0.95)
			discard;
		texColor *= f_colormultiplicator;
		/*color.x = (1-texColor.a) * texColor.x + texColor.a;
		color.y = (1-texColor.a) * texColor.y + texColor.a;
		color.z = (1-texColor.a) * texColor.z + texColor.a;*/
		
		color.x = texColor.x;
		color.y = texColor.y;
		color.z = texColor.z;
		
		color.w = 1;
	}
	
	
	gl_FragColor = color;
}