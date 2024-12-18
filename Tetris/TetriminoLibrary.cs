using OpenTK.Mathematics;

namespace Tetris;

public static class TetriminoLibrary
{
    private static Random rnd = new Random();
    
    private static Tetrimino[] TETRIMINOS = new []
    {
        // Square
        new Tetrimino(5,
            new Vector2i[]{(-1, 0), (-1, 1), (0, 0), (0, 1)}
        ),
            
        // Long
        new Tetrimino(5,
            new Vector2i[]{(0, -1), (0, 0), (0, 1), (0, 2)},
            new Vector2i[]{(-1, 0), (0, 0), (1, 0), (2, 0)}
        ),
        
        // T
        new Tetrimino(5,
            new Vector2i[]{(-1, 0), (0, 0), (1, 0), (0, -1)},
            new Vector2i[]{(0, 1), (0, 0), (0, -1), (1, 0)},
            new Vector2i[]{(-1, 0), (0, 0), (1, 0), (0, 1)},
            new Vector2i[]{(0, 1), (0, 0), (0, -1), (-1, 0)}
        ),
        
        // L
        new Tetrimino(5,
            new Vector2i[]{(-1, 1), (-1, 0), (-1, -1), (0, -1)},
            new Vector2i[]{(0, 1), (0, 0), (0, -1), (-1, -1)}
        ),
    };

    public static Tetrimino GET_RANDOM()
    {
        int rndIndex = rnd.Next(0, TETRIMINOS.Length);
        //rndIndex = 1;
        return TETRIMINOS[rndIndex];
    }
    
}