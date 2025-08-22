#version 330 core

in vec2 vUV;

uniform sampler2D uTexture; // bound at slot 0

out vec4 FragColor;

void main()
{
    FragColor = texture(uTexture, vUV);
}