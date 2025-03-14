using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Audio.OpenAL;

namespace Tetris;

public class Engine : GameWindow
{
    private const float SQUARE_SIZE = 0.03f;
    
    private readonly float[] verticiesBlock = new []
    {
        -SQUARE_SIZE,  SQUARE_SIZE,
         SQUARE_SIZE, -SQUARE_SIZE,
        -SQUARE_SIZE, -SQUARE_SIZE,
        
        -SQUARE_SIZE,  SQUARE_SIZE,
         SQUARE_SIZE,  SQUARE_SIZE,
         SQUARE_SIZE, -SQUARE_SIZE,
    };
    
    private readonly Dictionary<int, Vector3> COLOR_MAP = new Dictionary<int, Vector3>()
    {
        {00, new Vector3(0.07f, 0.07f, 0.07f)},
        {01, new Vector3(0.7f, 0.7f, 0.7f)},
        {02, new Vector3(0.7f, 0.7f, 0.0f)},
        {03, new Vector3(0.0f, 0.5f, 0.7f)},
        {04, new Vector3(0.5f, 0.0f, 0.7f)},
        {05, new Vector3(0.7f, 0.2f, 0.0f)},
        {06, new Vector3(0.0f, 0.0f, 1.0f)},
        {07, new Vector3(0.7f, 0.0f, 0.0f)},
        {08, new Vector3(0.0f, 0.7f, 0.0f)},
        {10, new Vector3(0.0f, 0.0f, 0.0f)},
    };

    private int shaderDefault;
    private int blockVAO;
    
    private int ulColor;
    private int ulModel;
    private int ulProjj;

    private int idSoundMusic;
    
    private readonly int gridWidth;
    private readonly int gridHeight;
    private readonly float gridHalfWidth;
    private readonly float gridHalfHeight;
    
    protected int[,] Grid;
    
    private float aspectRatio;
    
    public Engine(int _gridWidth, int _gridHeight) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        gridWidth = _gridWidth;
        gridHeight = _gridHeight;
        gridHalfWidth = gridWidth / 2.0f;
        gridHalfHeight = gridHeight / 2.0f;
        Grid = new int[gridWidth,gridHeight];
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();
        
        CreateShader();
        GL.UseProgram(shaderDefault);
        
        CreateBlockVAO();
        
        ulColor = GL.GetUniformLocation(shaderDefault, "color");
        ulModel = GL.GetUniformLocation(shaderDefault, "model");
        ulProjj = GL.GetUniformLocation(shaderDefault, "projj");
        
        InitializeAudio();
        
        string? appPath = Path.GetDirectoryName(Environment.ProcessPath);
        appPath += "/../../.."; // Remove when building for release.
        idSoundMusic = LoadAudio(appPath + "/Audio/Tetris4Mcl.wav");
        AL.Source(idSoundMusic, ALSourceb.Looping, true);
        AL.SourcePlay(idSoundMusic);
    }
    
    private void CreateShader()
    {
        const string vertexShaderSource =
            "#version 330 core \n" +
            "in vec2 vert;" +
            "uniform mat4 model;" +
            "uniform mat4 projj;" +
            "void main(){" +
            "gl_Position = vec4(vert, 0.0, 1.0) * model * projj;" +
            "}";

        const string fragmentShaderSource =
            "#version 330 core \n" +
            "out vec4 FragColor;" +
            "uniform vec3 color;" +
            "void main(){" +
            "FragColor = vec4(color, 1.0f);" +
            "}";
        
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        
        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        
        GL.CompileShader(vertexShader);
        GL.CompileShader(fragmentShader);
        
        GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int succ1);
        GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int succ2);
        
        if (succ1 == 0) Console.WriteLine(GL.GetShaderInfoLog(vertexShader));
        if (succ2 == 0) Console.WriteLine(GL.GetShaderInfoLog(fragmentShader));
        
        shaderDefault = GL.CreateProgram();

        GL.AttachShader(shaderDefault, vertexShader);
        GL.AttachShader(shaderDefault, fragmentShader);
        
        GL.LinkProgram(shaderDefault);
        GL.GetProgram(shaderDefault, GetProgramParameterName.LinkStatus, out int succ3);
        
        if (succ3 == 0) Console.WriteLine(GL.GetProgramInfoLog(shaderDefault));
        
        GL.DetachShader(shaderDefault, vertexShader);
        GL.DetachShader(shaderDefault, fragmentShader);
        
        GL.DeleteShader(fragmentShader);
        GL.DeleteShader(vertexShader);
    }
    
    private void CreateBlockVAO()
    {
        const int typeSize = sizeof(float); 
        blockVAO = GL.GenVertexArray();
        GL.BindVertexArray(blockVAO);
        int VBO = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
        GL.BufferData(BufferTarget.ArrayBuffer, verticiesBlock.Length * typeSize, verticiesBlock, BufferUsageHint.DynamicDraw);
        int aVert = GL.GetAttribLocation(shaderDefault, "vert");
        GL.VertexAttribPointer(aVert, 2, VertexAttribPointerType.Float, false, 2 * typeSize, 0);
        GL.EnableVertexAttribArray(aVert);
    }
    
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        
        Matrix4 proj = Matrix4.CreateOrthographicOffCenter(-aspectRatio, aspectRatio, -1.0f, 1.0f, 1.0f, -1.0f);
        GL.UniformMatrix4(ulProjj, true, ref proj);
        
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++) RenderBlock(i, j);
        }
        
        SwapBuffers();
    }
    
    private void RenderBlock(int _x, int _y)
    {
        int v = Math.Abs(Grid[_x, _y]);
        Vector3 color = COLOR_MAP[v];
        Vector3 pos = new Vector3((_x + 0.5f) - gridHalfWidth, _y - gridHalfHeight, 0.0f); 
        const float offset = (SQUARE_SIZE * 2.25f);
        pos *= offset;
        Matrix4 model = Matrix4.CreateTranslation(pos);
        
        GL.UniformMatrix4(ulModel, true, ref model);
        GL.Uniform3(ulColor, color);
        GL.BindVertexArray(blockVAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
        
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        aspectRatio = Size.X / (float)Size.Y;
    }
    
    protected override void OnUnload()
    {
        GL.DeleteProgram(shaderDefault);
        AL.DeleteSource(idSoundMusic);
        ShutdownAudio();
    }

    private void InitializeAudio()
    {
        ALDevice device = ALC.OpenDevice(null);
        ALContext context = ALC.CreateContext(device, (int[])null);
        ALC.MakeContextCurrent(context);
    }
    
    private void ShutdownAudio()
    {
        ALContext context = ALC.GetCurrentContext();
        ALDevice device = ALC.GetContextsDevice(context);
        ALC.MakeContextCurrent(ALContext.Null);
        ALC.DestroyContext(context);
        ALC.CloseDevice(device);
    }

    private int LoadAudio(string _filename)
    {
        const string EX = "Invalid file format!";
        
        FileStream stream = File.Open(_filename, FileMode.Open);
        BinaryReader reader = new BinaryReader(stream);
        
        string signature = new string(reader.ReadChars(4));
        if (signature != "RIFF") throw new NotSupportedException(EX);

        reader.ReadInt32();

        string format = new string(reader.ReadChars(4));
        if (format != "WAVE") throw new NotSupportedException(EX);
            
        string format_signature = new string(reader.ReadChars(4));
        if (format_signature != "fmt ") throw new NotSupportedException(EX);

        reader.ReadInt32();
        reader.ReadInt16();
        int num_channels = reader.ReadInt16();
        int sample_rate = reader.ReadInt32();
        reader.ReadInt32();
        reader.ReadInt16();
        int bits_per_sample = reader.ReadInt16();
        
        string data_signature = new string(reader.ReadChars(4));
        if (data_signature != "data") throw new NotSupportedException(EX);

        reader.ReadInt32();

        ALFormat soundFormat;
        switch (num_channels)
        {
            case 1: soundFormat = (bits_per_sample == 8 ? ALFormat.Mono8 : ALFormat.Mono16); break;
            case 2: soundFormat = (bits_per_sample == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16); break;
            default: throw new NotSupportedException(EX);
        }

        byte[] data = reader.ReadBytes((int)reader.BaseStream.Length);
        
        int buffer = AL.GenBuffer();
        int source = AL.GenSource();
        AL.BufferData(buffer, soundFormat, ref data[0], data.Length * sizeof(byte), sample_rate);
        AL.Source(source, ALSourcei.Buffer, buffer);
        AL.DeleteBuffer(buffer);
        
        return source;
    }
}