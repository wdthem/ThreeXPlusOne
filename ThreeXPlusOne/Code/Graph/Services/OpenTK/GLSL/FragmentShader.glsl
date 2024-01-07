#version 330 core
in vec4 vertexColor; // Received from the vertex shader
out vec4 FragColor;

void main()
{
    FragColor = vertexColor; // Use the color passed from the vertex shader
}