#version 330 compatibility
layout (location = 0) in vec3 v_position;
layout (location = 1) in vec2 v_texcoord;

out vec3 f_position;
out vec2 f_texcoord;
out float f_alphatest;
uniform float alphatest;

void main()
{
    gl_Position =  gl_ProjectionMatrix * gl_ModelViewMatrix *  vec4(v_position, 1);
	f_position = v_position;
	f_texcoord = v_texcoord;
	f_alphatest = alphatest;
}