using OpenTK.Graphics.OpenGL4;

namespace ThreeXPlusOne.Code.Graph.Services.OpenTK;

public class OpenTKShader
{
    public readonly int Handle;
    private readonly int _vertexShader;
    private readonly int _fragmentShader;

    public OpenTKShader(string vertexPath, string fragmentPath)
    {
        // Load and compile the vertex shader
        var shaderSource = File.ReadAllText(vertexPath);
        _vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(_vertexShader, shaderSource);
        CompileShader(_vertexShader);

        // Load and compile the fragment shader
        shaderSource = File.ReadAllText(fragmentPath);
        _fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(_fragmentShader, shaderSource);
        CompileShader(_fragmentShader);

        // Link both shaders into a shader program
        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, _vertexShader);
        GL.AttachShader(Handle, _fragmentShader);
        LinkProgram(Handle);
    }

    private static void CompileShader(int shader)
    {
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
        if (code != (int)All.True)
        {
            var infoLog = GL.GetShaderInfoLog(shader);
            throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
        }
    }

    private static void LinkProgram(int program)
    {
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
        if (code != (int)All.True)
        {
            var infoLog = GL.GetProgramInfoLog(program);
            throw new Exception($"Error occurred whilst linking Program({program}).\n\n{infoLog}");
        }
    }

    public void Use()
    {
        GL.UseProgram(Handle);
    }

    public void Dispose()
    {
        GL.DetachShader(Handle, _vertexShader);
        GL.DetachShader(Handle, _fragmentShader);
        GL.DeleteShader(_fragmentShader);
        GL.DeleteShader(_vertexShader);
        GL.DeleteProgram(Handle);
    }
}
