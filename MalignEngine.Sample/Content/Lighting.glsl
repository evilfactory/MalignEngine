#shader fragment

#version 330 core
in vec2 fUv;
in float fTexIndex;
in vec4 fColor;

uniform sampler2D uTextures[16];
uniform sampler2D uLightingTexture;

uniform vec2 uLights[10];

out vec4 FragColor;

void main()
{
	int index = int(fTexIndex);
	vec4 texColor = vec4(1.0, 1.0, 1.0, 1.0);
	texColor = texture(uTextures[0], fUv);
	vec4 lightColor = texture(uLightingTexture, fUv);


	FragColor = vec4(lightColor.rgb, 1.0) * texColor * (fColor / 255);
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