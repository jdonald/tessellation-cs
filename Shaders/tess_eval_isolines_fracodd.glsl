#version 410 core

layout(isolines, fractional_odd_spacing) in;

in vec3 tcColor[];
out vec3 teColor;
out vec3 tePosition;

uniform mat4 uModelViewProjection;

void main()
{
    float u = gl_TessCoord.x;
    float v = gl_TessCoord.y;

    vec3 p0 = gl_in[0].gl_Position.xyz;
    vec3 p1 = gl_in[1].gl_Position.xyz;
    vec3 p2 = gl_in[2].gl_Position.xyz;
    vec3 p3 = gl_in[3].gl_Position.xyz;

    vec3 pos = mix(mix(p0, p1, u), mix(p2, p3, u), v);

    tePosition = pos;
    gl_Position = uModelViewProjection * vec4(pos, 1.0);

    teColor = mix(mix(tcColor[0], tcColor[1], u),
                  mix(tcColor[2], tcColor[3], u), v);
}
