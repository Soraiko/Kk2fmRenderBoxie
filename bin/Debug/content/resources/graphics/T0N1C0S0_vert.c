#version 330 compatibility
layout (location = 0) in vec3 v_position;
layout (location = 2) in vec3 v_normal;

out vec3 f_position;
out vec3 f_normal;
uniform vec4 colormultiplicator;
out vec4 f_colormultiplicator;

void main()
{
    gl_Position =  gl_ProjectionMatrix * gl_ModelViewMatrix *  vec4(v_position, 1);
	f_normal = v_normal;
f_colormultiplicator = colormultiplicator;
}