using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.Collections.ObjectModel;
using System.Drawing;
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
    private List<DirectedGraphNode>? _nodes;
    private int _vertexArray;
    private int _vertexBuffer;
    private OpenTKShader? _shader;

    public GraphProvider GraphProvider => GraphProvider.OpenTK;
    public ReadOnlyCollection<int> SupportedDimensions => new(new List<int> { 3 });

    //IGraphService methods
    public void InitializeGraph(List<DirectedGraphNode> nodes, int width, int height)
    {
        _nodes = nodes;
        consoleHelper.WriteLine("");
    }

    public void Draw(bool drawNumbersOnNodes, bool distortNodes, bool drawConnections)
    {
        throw new NotImplementedException();
    }

    public void GenerateBackgroundStars(int starCount)
    {
        throw new NotImplementedException();
    }

    public void SaveGraphImage()
    {
        throw new NotImplementedException("Saving 3D graph not supported");
    }


    //GameWindow overrides
    protected override void OnLoad()
    {
        if (_nodes == null)
        {
            throw new Exception("Could not render graph - Nodes object was null");
        }

        base.OnLoad();

        _shader = new OpenTKShader("VertexShader.glsl", "FragmentShader.glsl");
        _vertexArray = GL.GenVertexArray();
        _vertexBuffer = GL.GenBuffer();

        GL.BindVertexArray(_vertexArray);
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);

        //TODO: you would need to add in the glColor values here, like:
        /*
        //Vertex 1
        1.0f, 2.0f, 3.0f, // Position
        1.0f, 0.0f, 0.0f, 1.0f, // Color (Red)
        */

        List<float> verticesList = [];

        foreach (DirectedGraphNode node in _nodes)
        {
            float[] glColor = GetOpenGLColor(node.Color);

            verticesList.Add(node.Position.X);
            verticesList.Add(node.Position.Y);
            verticesList.Add(node.Z);

            verticesList.Add(glColor[0]);
            verticesList.Add(glColor[1]);
            verticesList.Add(glColor[2]);
            verticesList.Add(glColor[3]);
        }

        float[] vertices = [.. verticesList];

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
        _nodes = null;
    }

    private static float[] GetOpenGLColor(Color color)
    {
        return
        [
            color.R / 255.0f, // Red
            color.G / 255.0f, // Green
            color.B / 255.0f, // Blue
            color.A / 255.0f  // Alpha
        ];
    }
}