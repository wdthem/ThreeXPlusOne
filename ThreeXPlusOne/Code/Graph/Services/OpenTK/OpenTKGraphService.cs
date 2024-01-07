using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph.Services.OpenTK;

public class OpenTKGraphService(IConsoleHelper consoleHelper) : GameWindow(GameWindowSettings.Default,
                                                                           new NativeWindowSettings()
                                                                           {
                                                                               ClientSize = new Vector2i(800, 600),
                                                                               Title = "OpenTK Example",
                                                                           }), IGraphService
{
    private int _vertexArray;
    private int _vertexBuffer;
    private OpenTKShader? _shader; // Assume Shader is a custom class for handling shaders

    public GraphProvider GraphProvider => GraphProvider.OpenTK;

    public void InitializeGraph(int width, int height)
    {
        consoleHelper.WriteLine("");
    }

    public void DrawConnection(DirectedGraphNode node)
    {

    }

    public void DrawNode(DirectedGraphNode node, bool drawNumbersOnNodes, bool distortNodes)
    {

    }

    public void GenerateBackgroundStars(int starCount)
    {

    }

    public void SaveGraphImage()
    {
        throw new NotImplementedException("Saving 3D graph not supported");
    }


    protected override void OnLoad()
    {
        base.OnLoad();

        // Initialize shaders, vertex buffer, etc.
        _shader = new OpenTKShader("VertexShader.glsl", "FragmentShader.glsl");
        _vertexArray = GL.GenVertexArray();
        _vertexBuffer = GL.GenBuffer();

        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);

        // Set up vertex data (and buffer(s)) and configure vertex attributes
        float[] vertices = {
            // Vertex coordinates
            1.0f, 1.0f, 0.0f, // Point 1
            2.0f, 2.0f, 0.0f, // Point 2
            // Add as many points as needed
        };

        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        if (_shader == null)
        {
            throw new Exception("Could not render frame - OpenTKShader object was null");
        }

        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Use shader program
        _shader.Use();

        // Bind vertex array and draw
        GL.BindVertexArray(_vertexArray);
        GL.DrawArrays(PrimitiveType.Points, 0, 2); // Adjust the count as needed

        SwapBuffers();
    }

    protected override void OnUnload()
    {
        base.OnUnload();

        GL.DeleteVertexArray(_vertexArray);
        GL.DeleteBuffer(_vertexBuffer);
        _shader?.Dispose();
        _shader = null;
    }
}