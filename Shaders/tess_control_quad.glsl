#version 410 core

layout(vertices = 4) out;

in vec3 vColor[];
out vec3 tcColor[];

uniform float uTessLevel;

void main()
{
    gl_out[gl_InvocationID].gl_Position = gl_in[gl_InvocationID].gl_Position;
    tcColor[gl_InvocationID] = vColor[gl_InvocationID];

    if (gl_InvocationID == 0) {
        gl_TessLevelOuter[0] = uTessLevel;
        gl_TessLevelOuter[1] = uTessLevel;
        gl_TessLevelOuter[2] = uTessLevel;
        gl_TessLevelOuter[3] = uTessLevel;
        gl_TessLevelInner[0] = uTessLevel;
        gl_TessLevelInner[1] = uTessLevel;
    }
}
