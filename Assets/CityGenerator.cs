using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CityGenerator : MonoBehaviour {

    public GameObject wallCubePrefab;

    public int width;
    public int height;
    public int roomMinSize;
    public int roomMaxSize;
    public int roomDensity;
    public float gap;
    public float passageSpawnChance;
    public int seed;

    public bool autoSeed = true;

    void OnEnable()
    {
        if (autoSeed)
            seed = Random.Range(0, 999999);
        StartCoroutine(StartGenerator());
    }

    private struct Room
    {
        public int left, top, width, height;
    }

    List<Room> rooms = new List<Room>();

    public /*static*/ int[,] CreateDungeon(int dungeonWidth, int dungeonHeight, int roomDensity, int roomMinSize, int roomMaxSize, float passageSpawnChance, int seed)
    {
        System.Random random = new System.Random(seed);

        int[,] dungeon = new int[dungeonWidth, dungeonHeight];

        for (int i = 0; i < dungeonWidth; i++)
        {
            for (int j = 0; j < dungeonHeight; j++)
            {
                dungeon[i, j] = 0;
            }
        }

        int carvedArea = 0;
        int maxCarvedArea = (dungeonWidth / 2) * (dungeonHeight / 2);

        for (int i = 0; i < roomDensity; i++)
        {
            int carved;
            CreateRoom(rooms, dungeon, random, dungeonWidth, dungeonHeight, roomDensity, roomMinSize, roomMaxSize, true, out carved);
            carvedArea += carved;
        }

        do
        {
            int mazeStartX;
            int mazeStartY;
            do
            {
                mazeStartX = random.Next(1, (dungeonWidth / 2 - 1) * 2 + 1 + 2);
                mazeStartY = random.Next(1, (dungeonHeight / 2 - 1) * 2 + 1 + 2);
                if (mazeStartX % 2 == 0)
                {
                    mazeStartX--;
                }
                if (mazeStartY % 2 == 0)
                {
                    mazeStartY--;
                }
            }
            while (dungeon[mazeStartX, mazeStartY] != 0);
            int carved;
            StartMaze(dungeon, random, dungeonWidth, dungeonHeight, mazeStartX, mazeStartY, out carved);
            carvedArea += carved;
        }
        while (carvedArea < maxCarvedArea);

        for (int i = 0; i < rooms.Count; i++)
        {
            bool roomHasPassage = false;
            do
            {
                for (int x = rooms[i].left; x < rooms[i].left + rooms[i].width; x++)
                {
                    if ((rooms[i].top > 1) && ((dungeon[x, rooms[i].top-2] == 1) || (dungeon[x, rooms[i].top - 2] == 2)))
                    {
                        if (random.NextDouble() < passageSpawnChance)
                        {
                            roomHasPassage = true;
                            dungeon[x, rooms[i].top - 1] = 3;
                        }
                    }
                    if ((rooms[i].top+rooms[i].height-1 < dungeonHeight-2) && ((dungeon[x, rooms[i].top + rooms[i].height - 1 + 2] == 1) || (dungeon[x, rooms[i].top + rooms[i].height - 1 + 2] == 2)))
                    {
                        if (random.NextDouble() < passageSpawnChance)
                        {
                            roomHasPassage = true;
                            dungeon[x, rooms[i].top + rooms[i].height - 1 + 1] = 3;
                        }
                    }
                    
                }
                for (int y = rooms[i].top; y < rooms[i].top + rooms[i].height; y++)
                {
                    if ((rooms[i].left > 1) && ((dungeon[rooms[i].left - 2, y] == 1) || (dungeon[rooms[i].left - 2, y] == 2)))
                    {
                        if (random.NextDouble() < passageSpawnChance)
                        {
                            roomHasPassage = true;
                            dungeon[rooms[i].left - 1, y] = 3;
                        }
                    }
                    if ((rooms[i].left + rooms[i].width - 1 < dungeonWidth - 2) && ((dungeon[rooms[i].left + rooms[i].width - 1 + 2, y] == 1) || (dungeon[rooms[i].left + rooms[i].width - 1 + 2, y] == 2)))
                    {
                        if (random.NextDouble() < passageSpawnChance)
                        {
                            roomHasPassage = true;
                            dungeon[rooms[i].left + rooms[i].width - 1 + 1, y] = 3;
                        }
                    }
                }
            }
            while (roomHasPassage == false);
        }

        FillDungeon(random, dungeon, this.transform, wallCubePrefab, gap);

        return dungeon;
    }

    private static void CreateRoom(List<Room> rooms, int[,] dungeon, System.Random random, int dungeonWidth, int dungeonHeight, int roomDensity, int roomMinSize, int roomMaxSize, bool discardIfIntersects, out int carved)
    {
        carved = 0;
        int roomWidth = random.Next(roomMinSize, roomMaxSize + 1);
        int roomHeight = random.Next(roomMinSize, roomMaxSize + 1);
        if (roomWidth % 2 == 0)
        {
            roomWidth--;
        }
        if (roomHeight % 2 == 0)
        {
            roomHeight--;
        }
        int maxLeftRoomCorner = dungeonWidth - roomWidth - 1;
        int maxTopRoomCorner = dungeonHeight - roomHeight - 1;
        int leftRoomCorner = random.Next(1, maxLeftRoomCorner + 1);
        int topRoomCorner = random.Next(1, maxTopRoomCorner + 1);
        if (leftRoomCorner % 2 == 0)
        {
            leftRoomCorner--;
        }
        if (topRoomCorner % 2 == 0)
        {
            topRoomCorner--;
        }
        if (discardIfIntersects)
        {
            for (int i = -1 + leftRoomCorner; i <= leftRoomCorner + roomWidth; i++)
            {
                for (int j = -1 + topRoomCorner; j <= topRoomCorner + roomHeight; j++)
                {
                    if (dungeon[i,j] != 0)
                    {
                        return;
                    }
                }
            }
        }
        for (int i = leftRoomCorner; i < leftRoomCorner + roomWidth; i++)
        {
            for (int j = topRoomCorner; j < topRoomCorner + roomHeight; j++)
            {
                dungeon[i, j] = 1;
            }
        }
        carved = (roomWidth / 2 + 1) * (roomHeight / 2 + 1);
        rooms.Add(new Room { left = leftRoomCorner, top = topRoomCorner, width = roomWidth, height = roomHeight });
    }

    private static void StartMaze(int[,] dungeon, System.Random random, int dungeonWidth, int dungeonHeight, int startX, int startY, out int carved)
    {
        MazeStep(/*new List<MazeCellCoordinates>(),*/ dungeon, random, dungeonWidth, dungeonHeight, startX, startY, out carved);
    }


    private static void MazeStep(int[,] dungeon, System.Random random, int dungeonWidth, int dungeonHeight, int x, int y, out int carved)
    {
        dungeon[x, y] = 2;
        carved = 1;
        var array = new int[] { 1, 2, 3, 4 };
        Shuffle(random, array);
        int tempCarved;
        for (int i = 0; i < 4; i++)
        {
            tempCarved = 0;
            switch (array[i])
            {
                // right
                case 1:
                    if ((x + 2 <= dungeonWidth-2) && (dungeon[x+2, y] == 0))
                    {
                        dungeon[x + 1, y] = 2;
                        MazeStep(dungeon, random, dungeonWidth, dungeonHeight, x + 2, y, out tempCarved);
                    }
                    break;
                // left
                case 2:
                    if ((x - 2 >= 1) && (dungeon[x - 2, y] == 0))
                    {
                        dungeon[x - 1, y] = 2;
                        MazeStep(dungeon, random, dungeonWidth, dungeonHeight, x - 2, y, out tempCarved);
                    }
                    break;
                // down
                case 3:
                    if ((y + 2 <= dungeonHeight - 2) && (dungeon[x, y + 2] == 0))
                    {
                        dungeon[x, y + 1] = 2;
                        MazeStep(dungeon, random, dungeonWidth, dungeonHeight, x, y + 2, out tempCarved);
                    }
                    break;
                // up
                case 4:
                    if ((y - 2 >= 1) && (dungeon[x, y - 2] == 0))
                    {
                        dungeon[x, y - 1] = 2;
                        MazeStep(dungeon, random, dungeonWidth, dungeonHeight, x, y - 2, out tempCarved);
                    }
                    break;
            }
            carved += tempCarved;
        }
    }

    private static void FillDungeon(System.Random random, int[,] dungeon, Transform dungeonContainer, GameObject wallCubePrefab, float gap)
    {
        for (int i = 0; i < dungeon.GetLength(0); i++)
        {
            for (int j = 0; j < dungeon.GetLength(1); j++)
            {
                if (dungeon[i, j] == 0)
                {
                    GameObject wallCube = GameObject.Instantiate<GameObject>(wallCubePrefab);
                    wallCube.transform.SetParent(dungeonContainer, true);
                    wallCube.transform.localPosition = new Vector3(i * gap, 0, j * gap);
                    int scale = random.Next(6, 11);
                    int hscale = random.Next(4, 9);
                    wallCube.transform.localScale = scale * Vector3.up + Vector3.right * hscale / 6 + Vector3.forward * hscale / 6;
                    wallCube.GetComponentInChildren<MeshRenderer>().material.mainTextureScale = new Vector2(hscale, hscale * scale);
                    wallCube.GetComponentInChildren<MeshRenderer>().material.color = new Color(0.5f + 0.5f * (float)random.NextDouble(), 0.5f + 0.5f * (float)random.NextDouble(), 0.5f + 0.5f * (float)random.NextDouble());
                }
            }
        }
    }

    public IEnumerator StartGenerator()
    {

        var dungeon = CreateDungeon(width, height, roomDensity, roomMinSize, roomMaxSize, passageSpawnChance, seed);
        
        //dungeonWallContainer.localScale = new Vector3(1f, wallHeight, 1f);

        

        yield return null;
    }

    public static void Shuffle<T>(System.Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

}
