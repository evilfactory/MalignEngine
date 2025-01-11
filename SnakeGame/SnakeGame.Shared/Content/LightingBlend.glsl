#shader fragment

#version 330 core
in vec2 fUv;
in float fTexIndex;
in vec4 fColor;

uniform sampler2D uTextures[16];

out vec4 FragColor;

void main()
{
	int index = int(fTexIndex);
	vec4 texColor = vec4(1.0, 1.0, 1.0, 1.0);

	switch (index) { // glsl apparently doesn't support dynamic indexing :thumbsup:
	case 0:
		texColor = texture(uTextures[0], fUv);
		break;
	case 1:
		texColor = texture(uTextures[1], fUv);
		break;
	case 2:
		texColor = texture(uTextures[2], fUv);
		break;
	case 3:
		texColor = texture(uTextures[3], fUv);
		break;
	case 4:
		texColor = texture(uTextures[4], fUv);
		break;
	case 5:
		texColor = texture(uTextures[5], fUv);
		break;
	case 6:
		texColor = texture(uTextures[6], fUv);
		break;
	case 7:
		texColor = texture(uTextures[7], fUv);
		break;
	case 8:
		texColor = texture(uTextures[8], fUv);
		break;
	case 9:
		texColor = texture(uTextures[9], fUv);
		break;
	case 10:
		texColor = texture(uTextures[10], fUv);
		break;
	case 11:
		texColor = texture(uTextures[11], fUv);
		break;
	case 12:
		texColor = texture(uTextures[12], fUv);
		break;
	case 13:
		texColor = texture(uTextures[13], fUv);
		break;
	case 14:
		texColor = texture(uTextures[14], fUv);
		break;
	case 15:
		texColor = texture(uTextures[15], fUv);
		break;
	}
	
	if (texColor.a < 0.1)
	{
		discard;
	}

	FragColor = texColor * (fColor / 255);
}

#shader vertex

#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec2 vUv;
layout (location = 2) in float vTexIndex;
layout (location = 3) in vec4 vColor;


uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 fUv;
out float fTexIndex;
out vec4 fColor;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
    fUv = vUv;
    fTexIndex = vTexIndex;
	fColor = vColor;
}