using System;
using System.Collections.Generic;
using Godot;

public class ChunkGrid
{
    // use [y, x] for better visuals
    public MapChunk[,] chunks = {
        {null, null, null},
        {null, null, null},
        {null, null, null}
     };

    public MapChunk TopLeft
    {
        get
        {
            return chunks[0, 0];
        }
        set
        {
            chunks[0, 0] = value;
        }
    }

    public MapChunk TopCenter
    {
        get
        {
            return chunks[0, 1];
        }
        set
        {
            chunks[0, 1] = value;
        }
    }

    public MapChunk TopRight
    {
        get
        {
            return chunks[0, 2];
        }
        set
        {
            chunks[0, 2] = value;
        }
    }

    public MapChunk CenterLeft
    {
        get
        {
            return chunks[1, 0];
        }
        set
        {
            chunks[1, 0] = value;
        }
    }

    public MapChunk Center
    {
        get
        {
            return chunks[1, 1];
        }
        set
        {
            chunks[1, 1] = value;
        }
    }

    public MapChunk CenterRight
    {
        get
        {
            return chunks[1, 2];
        }
        set
        {
            chunks[1, 2] = value;
        }
    }

    public MapChunk BottomLeft
    {
        get
        {
            return chunks[2, 0];
        }
        set
        {
            chunks[2, 0] = value;
        }
    }

    public MapChunk BottomCenter
    {
        get
        {
            return chunks[2, 1];
        }
        set
        {
            chunks[2, 1] = value;
        }
    }

    public MapChunk BottomRight
    {
        get
        {
            return chunks[2, 2];
        }
        set
        {
            chunks[2, 2] = value;
        }
    }

    public void Shift(Vector2I direction)
    {
        // don't do anything if zero
        if (direction == Vector2I.Zero)
        {
            return;
        }

        // temporary array to hold the new positions
        MapChunk[,] newChunks = new MapChunk[3, 3];

        // Iterate over the grid and assign chunks to new positions
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                // calculate new x and y
                int newX = x + direction.X;
                int newY = y + direction.Y;

                // don't delete if out of bounds
                if (newX >= 3 || newX <= 0 || newY >= 3 || newY <= 0)
                {
                    // delete node
                    chunks[y, x].QueueFree();
                    continue;
                }

                // assign chunk to the new position
                newChunks[newY, newX] = chunks[y, x];
            }
        }

        // Update the chunks with the shifted chunks
        chunks = newChunks;
    }
}