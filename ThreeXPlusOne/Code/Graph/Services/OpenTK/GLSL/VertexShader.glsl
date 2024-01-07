#version 330 core
layout(location = 0) in vec3 aPosition; // Position
layout(location = 1) in vec4 aColor;    // Color

out vec4 vertexColor; // Pass color to the fragment shader

void main()
{
    gl_Position = vec4(aPosition, 1.0);
    vertexColor = aColor;
}