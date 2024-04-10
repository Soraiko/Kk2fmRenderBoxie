#version 330 compatibility
layout (location = 0) in vec3 v_position;
layout (location = 3) in vec4 v_color;

out vec3 f_position;
out vec4 f_color;
out float f_alphatest;
uniform float alphatest;
uniform vec4 colormultiplicator;
out vec4 f_colormultiplicator;

void main()
{
    gl_Position =  gl_ProjectionMatrix * gl_ModelViewMatrix *  vec4(v_position, 1);
	f_position = v_position;
	f_color = v_color;
	f_alphatest = alphatest;
f_colormultiplicator = colormultiplicator;
}