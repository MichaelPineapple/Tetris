using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Tetris.Objects;

namespace Tetris;

public class Main : GameWindow
{
    private const float SQUARE_SIZE = 0.02f;
    protected const int GRID_HEIGHT = 20;
    protected const int GRID_WIDTH = 10;
    
    private readonly float[] verticiesSquare = new []
    {
        -SQUARE_SIZE,  SQUARE_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
         SQUARE_SIZE, -SQUARE_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 
        -SQUARE_SIZE, -SQUARE_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
        
        -SQUARE_SIZE,  SQUARE_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
         SQUARE_SIZE,  SQUARE_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
         SQUARE_SIZE, -SQUARE_SIZE, 0.0f,    0.0f, 0.0f, 0.0f, 0.0f, 0.0f,
    };
    
    private readonly Dictionary<int, Vector3> COLOR_MAP = new Dictionary<int, Vector3>()
    {
        {0, new Vector3(0.1f, 0.1f, 0.1f)},
        {1, new Vector3(1.0f, 1.0f, 1.0f)},
        {2, new Vector3(0.0f, 1.0f, 0.0f)},
    };
    
    private Shader shaderDefault;
    private Mesh meshSquare;
    
    protected int[,] grid = new int[GRID_WIDTH,GRID_HEIGHT];
    
    public Main(int _width, int _height, string _title) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        Size = (_width, _height);
        Title = _title;
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();
        string pathShaders = "../../../shaders/";
        shaderDefault = new Shader(pathShaders + "Default.vert", pathShaders + "Default.frag");
        meshSquare = new Mesh(verticiesSquare, shaderDefault);
    }
    
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        shaderDefault.Use();
        
        int uColor = shaderDefault.getUniformLocation("color");
        
        for (int i = 0; i < GRID_WIDTH; i++)
        {
            for (int j = 0; j < GRID_HEIGHT; j++)
            {
                int v = Math.Abs(grid[i, j]);
                Vector3 color = COLOR_MAP[v];
                Vector3 pos = new Vector3(i - (GRID_WIDTH / 2), j - (GRID_HEIGHT / 2), 0.0f); 
                pos *= (SQUARE_SIZE * 2.5f);
                GL.Uniform3(uColor, color);
                meshSquare.render(shaderDefault, pos, Vector3.Zero, 1.0f);
            }
        }
        
        SwapBuffers();
    }
        
    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }
    
    protected override void OnUnload()
    {
        shaderDefault.dispose();
    }
}