using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject playerPrefab;
    public GameObject collectiblePrefab;
    public GameObject enemyPrefab;

    private int[,] maze = {
        {1, 1, 1, 1, 1, 1, 1, 1, 1},
        {1, 2, 0, 3, 0, 0, 4, 3, 1},
        {1, 0, 1, 1, 0, 1, 1, 0, 1},
        {1, 0, 0, 0, 0, 0, 3, 0, 1},
        {1, 3, 1, 1, 0, 1, 1, 0, 1},
        {1, 0, 0, 0, 0, 3, 0, 0, 1},
        {1, 1, 1, 1, 1, 1, 1, 1, 1}
    };

    void Start()
    {
        GenerateMaze();
    }

    void GenerateMaze()
    {
        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int z = 0; z < maze.GetLength(1); z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                if (maze[x, z] == 1)
                {
                    Instantiate(wallPrefab, position, Quaternion.identity, transform);
                }
                else
                {
                    Instantiate(floorPrefab, position, Quaternion.identity, transform);

                    if (maze[x, z] == 2)
                    {
                        Instantiate(playerPrefab, position + Vector3.up, Quaternion.identity);
                    }
                    else if (maze[x, z] == 3)
                    {
                        Instantiate(collectiblePrefab, position + Vector3.up * 0.5f, Quaternion.identity);
                    }
                    else if (maze[x, z] == 4)
                    {
                        Instantiate(enemyPrefab, position + Vector3.up, Quaternion.identity);
                    }
                }
            }
        }
    }
}
