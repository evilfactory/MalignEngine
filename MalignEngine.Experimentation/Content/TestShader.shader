#pragma fragment

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

#pragma vertex

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vUv;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 fUv;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = vec4(vPos, 1.0);
    fUv = vUv;
}