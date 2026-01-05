#version 410 core

layout(triangles, fractional_even_spacing, ccw) in;

in vec3 tcColor[];
out vec3 teColor;
out vec3 tePosition;

uniform mat4 uModelViewProjection;

void main()
{
    vec3 p0 = gl_TessCoord.x * gl_in[0].gl_Position.xyz;
    vec3 p1 = gl_TessCoord.y * gl_in[1].gl_Position.xyz;
    vec3 p2 = gl_TessCoord.z * gl_in[2].gl_Position.xyz;
    vec3 pos = p0 + p1 + p2;

    tePosition = pos;
    gl_Position = uModelViewProjection * vec4(pos, 1.0);

    teColor = gl_TessCoord.x * tcColor[0] +
              gl_TessCoord.y * tcColor[1] +
              gl_TessCoord.z * tcColor[2];
}
