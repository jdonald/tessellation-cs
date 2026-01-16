#version 410 core

in vec3 teColor;
in vec3 tePosition;

out vec4 FragColor;

uniform bool uWireframeMode;
uniform vec3 uWireframeColor;

void main()
{
    if (uWireframeMode) {
        FragColor = vec4(uWireframeColor, 1.0);
    } else {
        FragColor = vec4(teColor, 1.0);
    }
}
