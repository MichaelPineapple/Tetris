using OpenTK.Mathematics;

namespace Tetris;

public struct Tetrimino
{
    public int Color;
    public Vector2i[][] Rotations;
    
    public Tetrimino(int _color, params Vector2i[][] _rotations)
    {
        Color = _color;
        Rotations = _rotations;
    }
}