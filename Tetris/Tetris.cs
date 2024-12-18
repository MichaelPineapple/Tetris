using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Tetris.Objects;

namespace Tetris;

public class Tetris : Main
{
    private const double TICK_TIME = 0.5f;
    private const int TETRINOME_LEN = 4;
    
    private double timer;
    private Vector2i[] tetrinome = new Vector2i[TETRINOME_LEN];
    private int tetrinomeColor = -2;
    
    public Tetris() : base(1000, 1000, "Tetris")
    {
        Console.WriteLine("Hello, Tetris!");
        SpawnTetrinome();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

        if (KeyboardState.IsKeyPressed(Keys.Left)) MoveTetrinome(-1, 0);
        if (KeyboardState.IsKeyPressed(Keys.Right)) MoveTetrinome(1, 0);
        
        timer += e.Time;
        if (timer >= TICK_TIME)
        {
            OnTick();
            timer = 0.0f;
        }
    }
    
    private void OnTick()
    {
        bool moveResult = MoveTetrinome(0, -1);
        if (!moveResult)
        { 
            for (int i = 0; i < TETRINOME_LEN; i++)
            {
                Vector2i a = tetrinome[i];
                grid[a.X, a.Y] = 1;
            }
            
            SpawnTetrinome();  
        }
    }
    
    
    private void SpawnTetrinome()
    {
        tetrinome[0] = new Vector2i(5, 18);
        tetrinome[1] = new Vector2i(5, 17);
        tetrinome[2] = new Vector2i(5, 16);
        tetrinome[3] = new Vector2i(5, 15);
        
        SetTetrinome(tetrinomeColor);
    }
    
    private bool MoveTetrinome(int _x, int _y)
    {
        Vector2i[] next = new Vector2i[TETRINOME_LEN];
        for (int i = 0; i < TETRINOME_LEN; i++)
        {
            Vector2i a = tetrinome[i] + new Vector2i(_x, _y);
            int x = a.X;
            int y = a.Y;
            if (x < 0 || x >= GRID_WIDTH) return false;
            if (y < 0 || y >= GRID_HEIGHT) return false;
            if (grid[x, y] > 0) return false;
            next[i] = a;
        }
        
        SetTetrinome(0);
        tetrinome = next;
        SetTetrinome(tetrinomeColor);

        return true;
    }
    
    private void SetTetrinome(int _val)
    {
        for (int i = 0; i < TETRINOME_LEN; i++)
        {
            Vector2i a = tetrinome[i];
            grid[a.X, a.Y] = _val;
        }
    }
}