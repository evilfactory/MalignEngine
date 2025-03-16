#shader vertex
#version 300 es


precision highp float;
layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in float TexIndex;
layout(location = 3) in vec4 vertexColor;

out vec2 v_TexCoord;
out vec4 v_vertexColor;
out float v_TexIndex;
//out float gl_PointSize;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

uniform float u_Time;
uniform float u_PointSize;

void main() {
	gl_Position = uModel * uView * uProjection * position;
	//gl_Position.y = gl_Position.y + sin(u_Time + gl_Position.x)/5.0f;

	v_TexCoord = texCoord;
	v_vertexColor = vertexColor;
	v_TexIndex = TexIndex;
	gl_PointSize = u_PointSize;
}


#shader fragment
#version 300 es


precision highp float;
layout(location = 0) out vec4 color;

in vec2 v_TexCoord;
in vec4 v_vertexColor;

uniform sampler2D uTextures[16];

uniform float u_scale;
uniform float u_movex;
uniform float u_movey;
uniform int u_iterations;

float map(float x, float in_min, float in_max, float out_min, float out_max)
{
    return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
}

void main() {
    vec4 tex = texture(uTextures[0], vec2(0,0));
    float maxIterations = float(u_iterations);

    float a = map(gl_FragCoord.x, 0.0f, 650.0f, -u_scale + (u_movex * u_scale), u_scale + (u_movex * u_scale));
    float b = map(gl_FragCoord.y, 0.0f, 650.0f, -u_scale + (u_movey * u_scale), u_scale + (u_movey * u_scale));

    float ca = a;
    float cb = b;

    float n = 0.0f;
    float z = 0.0f;

    while (n < maxIterations)
    {
        float aa = a * a - b * b;
        float bb = 2.0f * a * b;

        a = aa + ca;
        b = bb + cb;

        if (abs(a + b) > 16.0f)
            break;

        n++;
    }
    float bright = n / 100.0f;

    if (n == maxIterations)
    {
        bright = 0.0f;
    }

	color = v_vertexColor * bright;
	
}