using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Tetris;

public class Tetris : Main
{
    private const double TICK_TIME = 0.5f;
    private const int GRID_WIDTH = 12;
    private const int GRID_HEIGHT = 22;
    private const int BORDER_COLOR = 10;
    
    private readonly Vector2i SPAWN_LOCATION = (GRID_WIDTH / 2, GRID_HEIGHT - 1);
    
    private double timer;
    private Stack<int> rowStack = new Stack<int>();
    
    private Vector2i[] hand = [];
    private Vector2i handOffset;
    private Tetrimino handTetrimino;
    private int handRotation;
    
    public Tetris() : base(GRID_WIDTH, GRID_HEIGHT)
    {
        Console.WriteLine("Hello, Tetris!");
        Size = (1000, 1000);
        Title = "Tetris";
        GenerateGridBorder();
        ResetHand();
        SpawnTetrimino();
    }

    private void GenerateGridBorder()
    {
        for (int i = 0; i < GRID_WIDTH; i++) Grid[i, 0] = BORDER_COLOR;
        for (int i = 0; i < GRID_HEIGHT; i++)
        {
            Grid[0, i] = BORDER_COLOR;
            Grid[GRID_WIDTH - 1, i] = BORDER_COLOR;
        }
    }
    
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);

        if (KeyboardState.IsKeyDown(Keys.Escape)) Close();

        if (KeyboardState.IsKeyPressed(Keys.Left)) MoveHand(-1, 0);
        if (KeyboardState.IsKeyPressed(Keys.Right)) MoveHand(1, 0);
        if (KeyboardState.IsKeyPressed(Keys.Up)) RotateHand();
        if (KeyboardState.IsKeyPressed(Keys.Down)) Slam();
        
        timer += e.Time;
        if (timer >= TICK_TIME)
        {
            OnTick();
            timer = 0.0f;
        }
    }

    private void OnTick()
    {
        while (rowStack.Count > 0)
        {
            int row = rowStack.Pop();
            ShiftDown(row);
        }

        bool moveResult = MoveHand(0, -1);
        if (!moveResult)
        { 
            SetHand(1);
            ResetHand();
            CheckRows();
            SpawnTetrimino();
        }
    }

    private void ResetHand()
    {
        hand = new Vector2i[0];
        handOffset = SPAWN_LOCATION;
        handRotation = 0;
        handTetrimino = new Tetrimino(0);
    }

    private void CheckRows()
    {
        for (int i = 1; i < GRID_HEIGHT; i++)
        {
            bool full = true;
            for (int j = 1; j < GRID_WIDTH - 1; j++)
            {
                int v = Grid[j, i];
                if (v < 1) full = false;
            }

            if (full)
            {
                for (int j = 1; j < GRID_WIDTH - 1; j++) Grid[j, i] = 3;
                rowStack.Push(i);
            }
        }
    }
    
    private void ShiftDown(int _y)
    {
        for (int i = _y + 1; i < GRID_HEIGHT; i++)
        {
            for (int j = 1; j < GRID_WIDTH - 1; j++)
            {
                int v = Grid[j, i];
                if (v > -1)
                {
                    Grid[j, i] = 0;
                    Grid[j, i - 1] = v;
                }
            }
        }
    }
    
    private void SpawnTetrimino()
    {
        handTetrimino = TetriminoLibrary.GET_RANDOM();
        Vector2i[] blocks = handTetrimino.Rotations[0];
        Vector2i[] next = applyOffset(blocks, handOffset);
        if (!UpdateHand(next)) Console.WriteLine("Game Over");
    }
    
    private void Slam()
    {
        bool loop = true;
        while (loop) loop = MoveHand(0, -1);
    }
    
    private bool MoveHand(int _x, int _y)
    {
        Vector2i offset = new Vector2i(_x, _y);
        Vector2i[] next = applyOffset(hand, offset);
        bool result = UpdateHand(next);
        if (result) handOffset += offset;
        return result;
    }
    
    private void RotateHand()
    {
        handRotation = handRotation + 1;
        if (handRotation >= handTetrimino.Rotations.Length) handRotation = 0;
        Vector2i[] blocks = handTetrimino.Rotations[handRotation];
        Vector2i[] next = applyOffset(blocks, handOffset);
        UpdateHand(next);
    }

    private bool UpdateHand(Vector2i[] _next)
    {
        for (int i = 0; i < _next.Length; i++)
        {
            Vector2i a = _next[i];
            if (InBounds(a) && Grid[a.X, a.Y] > 0) return false;
        }
        SetHand(0);
        hand = _next;
        SetHand(-handTetrimino.Color);
        return true;
    }
    
    private void SetHand(int _val)
    {
        for (int i = 0; i < hand.Length; i++)
        {
            Vector2i a = hand[i];
            if (InBounds(a)) Grid[a.X, a.Y] = _val;
        }
    }
    
    private bool InBounds(Vector2i _a)
    {
        if (_a.X < 0 || _a.X >= GRID_WIDTH) return false;
        if (_a.Y < 0 || _a.Y >= GRID_HEIGHT) return false;
        return true;
    }
    
    private Vector2i[] applyOffset(Vector2i[] _input, Vector2i _offset)
    {
        Vector2i[] output = new Vector2i[_input.Length];
        for (int i = 0; i < _input.Length; i++) output[i] = (_input[i] + _offset);
        return output;
    }
}