public class VoxelData
{
    int[,,] data =
    {
        // x
        {
            // y
            {
                //z
                0, 1, 1
            },
            {
                1, 1, 1
            },
            {
                1, 1, 0
            }
        },
        {
            {
                0, 1, 1
            },
            {
                1, 1, 1
            },
            {
                1, 1, 0
            }
        },
        {
            {
                0, 1, 1
            },
            {
                1, 1, 1
            },
            {
                1, 1, 0
            }
        },
    };

    public int Width => data.GetLength(0);
    public int Height => data.GetLength(1);
    public int Depth => data.GetLength(2);

    public int GetCell(int x, int y, int z)
    {
        return data[x, y, z];
    }

    public int GetNeighbor(int x, int y, int z, Direction dir)
    {
        DataCoordinate offsetToCheck = offsets[(int) dir];
        // Todo: make overload of + operator to add vector3
        DataCoordinate neighborCoord =
            new DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);

        // Todo: make handling of Y value
        if (
            neighborCoord.x < 0 || neighborCoord.x >= Width 
                                ||
            neighborCoord.y < 0 || neighborCoord.y >= Height 
                                ||
            neighborCoord.z < 0 || neighborCoord.z >= Depth
        )
        {
            return 0;
        }

        return GetCell(neighborCoord.x, neighborCoord.y, neighborCoord.z);
    }

    private DataCoordinate[] offsets =
    {
        new DataCoordinate(0, 0, 1),
        new DataCoordinate(1, 0, 0),
        new DataCoordinate(0, 0, -1),
        new DataCoordinate(-1, 0, 0),
        new DataCoordinate(0, 1, 0),
        new DataCoordinate(0, -1, 0),
    };

    struct DataCoordinate
    {
        public int x;
        public int y;
        public int z;

        public DataCoordinate(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}

public enum Direction
{
    North,
    East,
    South,
    West,
    Up,
    Down
}