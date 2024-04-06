#version 330 compatibility
layout (location = 0) in vec3 v_position;
layout (location = 2) in vec3 v_normal;
layout (location = 3) in vec4 v_color;

out vec3 f_position;
out vec3 f_normal;
out vec4 f_color;

void main()
{
    gl_Position =  gl_ProjectionMatrix * gl_ModelViewMatrix *  vec4(v_position, 1);
	f_position = v_position;
	f_normal = v_normal;
	f_color = v_color;
}