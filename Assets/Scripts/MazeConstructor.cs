using UnityEngine;

//This Script Generates a new Maze based on the provided Dimensions. It Removes the current maze before generatng and showing a new one and placing the goal at the end.

public class MazeConstructor : MonoBehaviour
{
    public bool showDebug;

    [SerializeField] private Material mazeMat1;
    [SerializeField] private Material mazeMat2;
    [SerializeField] private Material startMat;
    [SerializeField] private Material treasureMat;

    public Node[,] graph;
    private MazeMeshGenerator meshGenerator;


    public float hallWidth { get; private set; }
    public int goalRow { get; private set; }
    public int goalCol { get; private set; }

    public float placementThreshold = 0.1f;   // chance of empty space

    public int[,] data
    {
        get; private set;
    }

    void Awake()
    {
        meshGenerator = new MazeMeshGenerator();
        hallWidth = meshGenerator.width;
        // default to walls surrounding a single empty cell
        data = new int[,]
        {
            {1, 1, 1},
            {1, 0, 1},
            {1, 1, 1}
        };
    }

    public void GenerateNewMaze(int sizeRows, int sizeCols, TriggerEventHandler treasureCallback)
    {
        DisposeOldMaze();

        if (sizeRows % 2 == 0 && sizeCols % 2 == 0)
            Debug.LogError("Odd numbers work better for dungeon size.");

        data = FromDimensions(sizeRows, sizeCols);

        goalRow = data.GetUpperBound(0) - 1;
        goalCol = data.GetUpperBound(1) - 1;

        graph = new Node[sizeRows, sizeCols];

        for (int i = 0; i < sizeRows; i++)
            for (int j = 0; j < sizeCols; j++)
                graph[i, j] = data[i, j] == 0 ? new Node(i, j, true) : new Node(i, j, false);



        DisplayMaze();
        PlaceGoal(treasureCallback);
    }




    public int[,] FromDimensions(int sizeRows, int sizeCols)
    {
        int[,] maze = new int[sizeRows, sizeCols];
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        for (int i = 0; i <= rMax; i++)
            for (int j = 0; j <= cMax; j++)
                if (i == 0 || j == 0 || i == rMax || j == cMax)
                    maze[i, j] = 1;
                else if (i % 2 == 0 && j % 2 == 0 && Random.value > placementThreshold)
                {
                    maze[i, j] = 1;

                    int a = Random.value < .5 ? 0 : (Random.value < .5 ? -1 : 1);
                    int b = a != 0 ? 0 : (Random.value < .5 ? -1 : 1);
                    maze[i + a, j + b] = 1;
                }
        return maze;
    }




    private void DisplayMaze()
    {
        GameObject go = new GameObject();
        go.transform.position = Vector3.zero;
        go.name = "Procedural Maze";
        go.tag = "Generated";

        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = meshGenerator.FromData(data);

        MeshCollider mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mf.mesh;

        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.materials = new Material[2] { mazeMat1, mazeMat2 };


    }

    public void DisposeOldMaze()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Generated"); //Finds all the objects with the generated tag and clears them
        foreach (GameObject go in objects)
        {
            Destroy(go);
        }
        GameObject[] helpers = GameObject.FindGameObjectsWithTag("Helper"); //Finds all the objects with the helper tag and clears them
        print(helpers.Length);
        foreach (GameObject go in helpers)
        {
            Destroy(go);
        }

    }


    void OnGUI() //Shows the 2D display of the Generated maze when debug mode is selected
    {
        if (!showDebug)
            return;

        int[,] maze = data;
        int rMax = maze.GetUpperBound(0);
        int cMax = maze.GetUpperBound(1);

        string msg = "";

        for (int i = rMax; i >= 0; i--)
        {
            for (int j = 0; j <= cMax; j++)
                msg += maze[i, j] == 0 ? "...." : "==";
            msg += "\n";
        }

        GUI.Label(new Rect(20, 20, 500, 500), msg);
    }


    private void PlaceGoal(TriggerEventHandler treasureCallback) //Creates the goal at the end of the maze,
    {
        GameObject treasure = GameObject.CreatePrimitive(PrimitiveType.Cube);
        treasure.transform.position = new Vector3(goalCol * hallWidth, .5f, goalRow * hallWidth);

        treasure.name = "Treasure"; //Names the object for easy identification
        treasure.tag = "Generated";// Gives the generated tag so it can be removed easily

        treasure.GetComponent<BoxCollider>().isTrigger = true; //Collision will trigger the OnTriggerEnter method, Object is not Solid
        treasure.GetComponent<MeshRenderer>().sharedMaterial = treasureMat;

        TriggerEventRouter tc = treasure.AddComponent<TriggerEventRouter>();
        tc.callback = treasureCallback;
    }

}