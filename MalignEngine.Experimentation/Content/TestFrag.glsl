#version 330 core

uniform sampler2D uTexture;

in vec2 fUv;

out vec4 FragColor;

void main()
{
	vec4 texColor = vec4(1.0, 1.0, 1.0, 1.0);
	
	texColor = texture(uTexture, fUv);

	if (texColor.a < 0.1)
	{
		discard;
	}

	FragColor = texColor;
}