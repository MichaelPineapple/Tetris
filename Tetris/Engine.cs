using MclTK;
using MclTK.Audio;
using MclTK.Graphics;
using MclTK.Graphics.Shaders;
using MclTK.Window;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Audio.OpenAL;

namespace Tetris;

public class Engine
{
    protected MclWindow win;
    
    private const float SQUARE_SIZE = 0.03f;
    
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

    private MclShaderDefault2D shaderDefault;
    private int blockVAO;

    private int idSoundMusic;
    
    private readonly int gridWidth;
    private readonly int gridHeight;
    private readonly float gridHalfWidth;
    private readonly float gridHalfHeight;
    
    protected int[,] Grid;
    
    public Engine(int _gridWidth, int _gridHeight)
    {
        gridWidth = _gridWidth;
        gridHeight = _gridHeight;
        gridHalfWidth = gridWidth / 2.0f;
        gridHalfHeight = gridHeight / 2.0f;
        Grid = new int[gridWidth,gridHeight];

        win = new MclWindow();
        win.Title = "Tetris";
        win.FuncLoad = OnLoad;
        win.FuncRender = OnRender;
    }
    
    private void OnLoad()
    {
        shaderDefault = new MclShaderDefault2D();
        shaderDefault.Use();

        float[] blockMesh = MclGL.MakeRectangleMesh(SQUARE_SIZE, SQUARE_SIZE);
        blockVAO = MclGL.CreateVAO(blockMesh, shaderDefault.handle);
        GL.BindVertexArray(blockVAO);

        string appPath = MclUtils.GetAppPath(false);
        AudioData audioDataMusic = MclAL.ReadFile(appPath + "/Audio/Tetris4Mcl.wav");
        idSoundMusic = MclAL.CreateSource(audioDataMusic);
        AL.Source(idSoundMusic, ALSourceb.Looping, true);
        AL.SourcePlay(idSoundMusic);
    }
    
    private void OnRender()
    {
        float aspectRatio = win.MclAspectRatio;
        Matrix4 proj = Matrix4.CreateOrthographicOffCenter(-aspectRatio, aspectRatio, -1.0f, 1.0f, 1.0f, -1.0f);
        shaderDefault.SetProjj(proj);
        
        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++) RenderBlock(i, j);
        }
    }
    
    private void RenderBlock(int _x, int _y)
    {
        int v = Math.Abs(Grid[_x, _y]);
        Vector3 color = COLOR_MAP[v];
        Vector3 pos = new Vector3((_x + 0.5f) - gridHalfWidth, _y - gridHalfHeight, 0.0f); 
        const float offset = (SQUARE_SIZE * 2.25f);
        pos *= offset;
        Matrix4 model = Matrix4.CreateTranslation(pos);
        shaderDefault.SetModel(model);
        shaderDefault.SetColor(color);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
}