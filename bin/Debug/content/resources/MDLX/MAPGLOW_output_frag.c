#version 330 compatibility
uniform sampler2D texture0;

in vec3 f_position;
in vec2 f_texcoord;


/*
BIG THANKS TO STACK OVERFLOW USER HARRY  WITH German Shepherd profile pic

https://stackoverflow.com/users/126537/harry

AND GIMP TEAM.
*/
vec4 GIMPColorToAlpha(float pA, float p1, float p2, float p3, float r1, float r2, float r3)
{
	float mA = 1.0;
	float mX = 1.0;
	
	float aA, a1, a2, a3;
	// a1 calculation: minimal alpha giving r1 from p1
	if (p1 > r1) a1 = mA * (p1 - r1) / (mX - r1);
	else if (p1 < r1) a1 = mA * (r1 - p1) / r1;
	else a1 = 0.0;
	// a2 calculation: minimal alpha giving r2 from p2
	if (p2 > r2) a2 = mA * (p2 - r2) / (mX - r2);
	else if (p2 < r2) a2 = mA * (r2 - p2) / r2;
	else a2 = 0.0;
	// a3 calculation: minimal alpha giving r3 from p3
	if (p3 > r3) a3 = mA * (p3 - r3) / (mX - r3);
	else if (p3 < r3) a3 = mA * (r3 - p3) / r3;
	else a3 = 0.0;
	// aA calculation: max(a1, a2, a3)
	aA = a1;
	if (a2 > aA) aA = a2;
	if (a3 > aA) aA = a3;
	// apply aA to pixel:
	if (aA >= mA / mX) {
	pA = aA * pA / mA;
	p1 = mA * (p1 - r1) / aA + r1;
	p2 = mA * (p2 - r2) / aA + r2;
	p3 = mA * (p3 - r3) / aA + r3;
	} else {
	pA = 0;
	p1 = 0;
	p2 = 0;
	p3 = 0;
	}
	return vec4(p1, p2, p3, pA/255.0);
}


uniform float show_glow_texture;

void main()
{
	if (show_glow_texture > 1f)
	{
		vec2 textureSize = vec2(512,512);
		
		float blurRadius = 30.0;
		vec3 blur = vec3(0.0);
		const int kernelSize = 15;
		float sigma = blurRadius / 3.0;
		float totalWeight = 0.0;

		for (int x = -kernelSize; x <= kernelSize; ++x) {
			for (int y = -kernelSize; y <= kernelSize; ++y) {
				vec2 offset = vec2(x, y);
				float weight = exp(-(dot(offset, offset)) / (sigma * sigma));
				blur += texture(texture0, f_texcoord + offset / textureSize).rgb * weight;
				totalWeight += weight;
			}
		}
		
		vec4 color = vec4(blur / (totalWeight), 1.0);
		color = GIMPColorToAlpha(color.a, color.r*255.0, color.g*255.0, color.b*255.0, 0.0,0.0,0.0);
		gl_FragColor = color;
	}
	else
	{
		gl_FragColor = texture(texture0, f_texcoord);
	}
}