#version 330 core

layout(location = 0) in vec3 aPos;
layout(location = 1) in vec2 aUV;

uniform mat4 uModel;     // per-cube transform
uniform mat4 uViewProj;  // camera projection * view

out vec2 vUV;

void main()
{
    gl_Position = uViewProj * uModel * vec4(aPos, 1.0);
    vUV = aUV;
}