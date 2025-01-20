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
        {00, new Vector3(0.05f, 0.05f, 0.05f)},
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

    private int idSoundFunky;
    
    private readonly int gridWidth;
    private readonly int gridHeight;
    private readonly float gridHalfWidth;
    private readonly float gridHalfHeight;
    
    protected int[,] Grid;
    
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
        CreateBlockVAO();
        
        ulColor = GL.GetUniformLocation(shaderDefault, "color");
        ulModel = GL.GetUniformLocation(shaderDefault, "model");
        ulProjj = GL.GetUniformLocation(shaderDefault, "projj");
        
        InitializeAudio();
        
        var soundData = CreateFunkySound();
        idSoundFunky = LoadSound(soundData.Item1, soundData.Item2);
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
        GL.UseProgram(shaderDefault);
        
        float aspectRatio = Size.X / (float)Size.Y;
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
        Vector3 pos = new Vector3(_x - gridHalfWidth, _y - gridHalfHeight, 0.0f); 
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
    }
    
    protected override void OnUnload()
    {
        GL.DeleteProgram(shaderDefault);
        AL.DeleteSource(idSoundFunky);
        ShutdownAudio();
    }

    protected void PlayFunkySound()
    {
        AL.SourcePlay(idSoundFunky);
    }
    
    private (short[], int) CreateFunkySound()
    {
        int freq0 = 220;
        int sampleRate = 44100;
        short[] data = new short[4410];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = (short)(MathF.Sin((i * freq0 * MathF.PI * 2) / sampleRate) * short.MaxValue);
        }
        return (data, 44100);
    }
    
    protected int LoadSound(short[] _data, int _freq)
    {
        int buffer = AL.GenBuffer();
        int source = AL.GenSource();
        AL.BufferData(buffer, ALFormat.Mono16, ref _data[0], _data.Length * sizeof(short), _freq);
        AL.Source(source, ALSourcei.Buffer, buffer);
        AL.Source(source, ALSourcef.Gain, 1.0f);
        AL.DeleteBuffer(buffer);
        return source;
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
}