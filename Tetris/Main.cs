using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Tetris.Objects;

namespace Tetris;

public class Main : GameWindow
{
    private const float SQUARE_SIZE = 0.02f;
    
    private readonly float[] verticiesBlock = new []
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
        {3, new Vector3(1.0f, 0.0f, 0.0f)},
        {4, new Vector3(1.0f, 0.0f, 1.0f)},
        {10, new Vector3(0.0f, 0.0f, 0.0f)},
    };
    
    private Shader shaderDefault;
    private Mesh meshBlock;
    private int uColor;
    private readonly int gridWidth;
    private readonly int gridHeight;
    private readonly int gridHalfWidth;
    private readonly int gridHalfHeight;
    
    protected int[,] Grid;
    
    public Main(int _gridWidth, int _gridHeight) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        gridWidth = _gridWidth;
        gridHeight = _gridHeight;
        gridHalfWidth = gridWidth / 2;
        gridHalfHeight = gridHeight / 2;
        Grid = new int[gridWidth,gridHeight];
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();
        string pathShaders = "../../../shaders/";
        shaderDefault = new Shader(pathShaders + "Default.vert", pathShaders + "Default.frag");
        meshBlock = new Mesh(verticiesBlock, shaderDefault);
        uColor = shaderDefault.getUniformLocation("color");
    }
    
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        shaderDefault.Use();
        
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                int v = Math.Abs(Grid[i, j]);
                Vector3 color = COLOR_MAP[v];
                Vector3 pos = new Vector3(i - gridHalfWidth, j - gridHalfHeight, 0.0f); 
                pos *= (SQUARE_SIZE * 2.25f);
                GL.Uniform3(uColor, color);
                meshBlock.render(shaderDefault, pos, Vector3.Zero, 1.0f);
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