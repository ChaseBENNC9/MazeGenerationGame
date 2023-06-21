using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//The Purpose of this script is to the control the enemy AI. 
//It Calculates the shortest distance between the Enemy and the Player.
public class AiController : MonoBehaviour
{

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 140;

    private Node[,] graph;
    public Node[,] Graph
    {
        get { return graph; }
        set { graph = value; }
    }



    private GameObject monster;
    public GameObject Monster
    {
        get { return monster; }
        set { monster = value; }
    }
    private GameObject player;
    public GameObject Player
    {
        get { return player; }
        set { player = value; }
    }
    private float hallWidth;
    public float HallWidth
    {
        get { return hallWidth; }
        set { hallWidth = value; }
    }
    [SerializeField] private float monsterSpeed;
    private int startRow = -1;
    private int startCol = -1;
    // Start is called before the first frame update
    public void StartAI()
    {
        startRow = graph.GetUpperBound(0) - 1; //Sets the starting position to the end of the generated maze
        startCol = graph.GetUpperBound(1) - 1;
    }

    // Update is called once per frame
    void Update()
    {
        if (startRow != -1 && startCol != -1) //If the monster is not in the destroyed position. 
        {
            int playerCol = (int)Mathf.Round(player.transform.position.x / hallWidth); //Finds the column and row of the player
            int playerRow = (int)Mathf.Round(player.transform.position.z / hallWidth);

            List<Node> path = FindPath(startRow, startCol, playerRow, playerCol); //Finds the path for the AI to take to get to the player

            if (path != null && path.Count > 1) //if there is a path to follow and the ai is not in the same cell as the player
            {
                Node nextNode = path[1]; //Sets the node to move to next
                float nextX = nextNode.y * hallWidth; //gets the X and Z Coordinates of the next node
                float nextZ = nextNode.x * hallWidth;
                Vector3 endPosition = new Vector3(nextX, 0f, nextZ);  //Gets the next position in a Vector3
                float step = monsterSpeed * Time.deltaTime;
                monster.transform.position = Vector3.MoveTowards(monster.transform.position, endPosition, step); //Moves the monster to the next Node Position
                Vector3 targetDirection = endPosition - monster.transform.position; //Calculates the direction of the next node
                Vector3 newDirection = Vector3.RotateTowards(monster.transform.forward, targetDirection, step, 0.0f); //Creates the new direction
                monster.transform.rotation = Quaternion.LookRotation(newDirection); //rotates the monster to be looking in the same direction as movement
                if (monster.transform.position == endPosition) //If the monster has reached the destination then update the starting position to be the current node
                {
                    startRow = nextNode.x;
                    startCol = nextNode.y;
                }
            }
        }
    }

    private int CalculateDistanceCost(Node a, Node b) //Calculates the costs - the distance from the node to the start and the distance from the node to the goal.
    {
        int xDistance = Mathf.Abs(a.x - b.x);
        int yDistance = Mathf.Abs(a.y - b.y);
        int remaining = xDistance - yDistance;
        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }



    private Node GetLowestFCostNode(List<Node> pathNodeList) //Finds the lowest Cost, from the list of nodes
    {
        Node lowestFCostNode = pathNodeList[0]; //Sets the first item to the lowest
        for (int i = 1; i < pathNodeList.Count; i++) //loops through the list until it finds something lower
            if (pathNodeList[i].fCost < lowestFCostNode.fCost)
                lowestFCostNode = pathNodeList[i]; //If something lower has been found the lowest cost is updated

        return lowestFCostNode; //Returns the lowest value
    }




    private List<Node> GetNeighbourList(Node currentNode) //Finds the Neighboors of the current node, Where the AI is able to move from the current location
    {
        List<Node> neighbourList = new List<Node>();

        if (currentNode.x - 1 >= 0)
        {
            neighbourList.Add(graph[currentNode.x - 1, currentNode.y]);

            if (currentNode.y - 1 >= 0)
                neighbourList.Add(graph[currentNode.x - 1, currentNode.y - 1]);
            if (currentNode.y + 1 < graph.GetLength(1))
                neighbourList.Add(graph[currentNode.x - 1, currentNode.y + 1]);
        }

        if (currentNode.x + 1 < graph.GetLength(0))
        {
            neighbourList.Add(graph[currentNode.x + 1, currentNode.y]);

            if (currentNode.y - 1 >= 0)
                neighbourList.Add(graph[currentNode.x + 1, currentNode.y - 1]);
            if (currentNode.y + 1 < graph.GetLength(1))
                neighbourList.Add(graph[currentNode.x + 1, currentNode.y + 1]);
        }

        if (currentNode.y - 1 >= 0)
            neighbourList.Add(graph[currentNode.x, currentNode.y - 1]);
        if (currentNode.y + 1 < graph.GetLength(1))
            neighbourList.Add(graph[currentNode.x, currentNode.y + 1]);

        return neighbourList;
    }


    private List<Node> CalculatePath(Node endNode) //Once the Goal has been found, Traces backwards to find the path to get there
    {
        List<Node> path = new List<Node>();
        path.Add(endNode);
        Node currentNode = endNode;
        while (currentNode.cameFromNode != null)
        {
            path.Add(currentNode.cameFromNode);
            currentNode = currentNode.cameFromNode;
        }
        path.Reverse(); //Reverse the path list so it can be traced back.
        return path;
    }



    public List<Node> FindPath(int startX, int startY, int endX, int endY) //Finds a path for the AI to travel from given positions
    {
        Node startNode = graph[startX, startY]; //The start point of the path
        Node endNode = graph[endX, endY]; //the end point of the path

        List<Node> openList = new List<Node> { startNode }; 
        List<Node> closedList = new List<Node>(); //Visited nodes are added to the closed list so they are not re-evaluated

        int graphWidth = graph.GetLength(0); //The outer bounds of the graph
        int graphHeight = graph.GetLength(1);

        for (int x = 0; x < graphWidth; x++)
            for (int y = 0; y < graphHeight; y++)
            {
                Node pathNode = graph[x, y];
                pathNode.gCost = int.MaxValue; //assigns each node the highest node possible so when it is evaluated it can assign the actual cost
                pathNode.CalculateFCost(); //Calculates the initial cost but will be updated when the path is being evaluated
                pathNode.cameFromNode = null; //set to null initially as no paths have been calculated to know where the previous node was
            }

        startNode.gCost = 0;
        startNode.hCost = CalculateDistanceCost(startNode, endNode);
        startNode.CalculateFCost();

        while (openList.Count > 0) //As long as there are nodes left to evaluate
        {
            Node currentNode = GetLowestFCostNode(openList);
            if (currentNode == endNode) //if the current node is the target point then return the calculated path
                return CalculatePath(endNode);

            openList.Remove(currentNode); //remove the evaluated node from the openList and add it to the closedList
            closedList.Add(currentNode);

            foreach (Node neighbourNode in GetNeighbourList(currentNode))
            {
                if (closedList.Contains(neighbourNode)) continue; //if the node has already been evaluated then continue to the next iteration of the loop

                if (!neighbourNode.isWalkable) //if the node is a wall
                {
                    closedList.Add(neighbourNode); //add it to the closed list and continue to the next iteration of the loop
                    continue;
                }

                int tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNode, neighbourNode);
                if (tentativeGCost < neighbourNode.gCost) 
                {
                    neighbourNode.cameFromNode = currentNode;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.hCost = CalculateDistanceCost(neighbourNode, endNode);
                    neighbourNode.CalculateFCost();

                    if (!openList.Contains(neighbourNode))
                        openList.Add(neighbourNode);
                }
            }
        }

        //out of nodes on the open list
        return null;
    }


    public void StopAI() //Disables the AI
    {
        startRow = -1;
        startCol = -1;
        Destroy(monster); //destroys the monster gameobject
    }




}
