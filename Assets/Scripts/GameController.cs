using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
//Tis is the Game Controller Script, It controls the main game loop , creates a player and enemy at oppisite ends of the maze and Controls when the Game is finished
[RequireComponent(typeof(MazeConstructor))]

public class GameController : MonoBehaviour
{

    public GameObject playerPrefab;
    public GameObject monsterPrefab;
    private AiController aIController;
    private MazeConstructor constructor;

    [SerializeField] private int rows;
    [SerializeField] private int cols;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {

            FindGoal();
        }
    }


    void Awake()
    {
        constructor = GetComponent<MazeConstructor>();
        aIController = GetComponent<AiController>();
    }

    private GameObject CreatePlayer()
    {
        Vector3 playerStartPosition = new Vector3(constructor.hallWidth, 1, constructor.hallWidth);
        GameObject player = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        player.tag = "Generated";

        return player;
    }

    private GameObject CreateMonster(TriggerEventHandler monsterCallback)
    {
        Vector3 monsterPosition = new Vector3(constructor.goalCol * constructor.hallWidth, 0f, constructor.goalRow * constructor.hallWidth);
        GameObject monster = Instantiate(monsterPrefab, monsterPosition, Quaternion.identity);
        monster.tag = "Generated";
        TriggerEventRouter tm = monster.AddComponent<TriggerEventRouter>();
        tm.callback = monsterCallback;



        return monster;
    }

    void Start()
    {
        constructor.GenerateNewMaze(rows, cols, OnTreasureTrigger);

        aIController.Graph = constructor.graph;
        aIController.Player = CreatePlayer();
        aIController.Monster = CreateMonster(OnMonsterTrigger);
        aIController.HallWidth = constructor.hallWidth;
        aIController.StartAI();

    }




    private void OnTreasureTrigger(GameObject trigger, GameObject other) //When the trigger of the treasure has been activated, the player wins, the ai is disabled and helpers are removed
    {
        Debug.Log("You Won!");
        aIController.StopAI();
        GameObject[] helpers = GameObject.FindGameObjectsWithTag("Helper");
        foreach (GameObject helper in helpers)
        {
            Destroy(helper);
        }
    }


    private void OnMonsterTrigger(GameObject trigger, GameObject other) //if the trigger of the enemy has been activated by the player, the monster wins, the ai is disabled and a new maze is created
    {
        Debug.Log("Gotcha");
        aIController.StopAI();
        constructor.GenerateNewMaze(rows, cols, OnTreasureTrigger);

        aIController.Graph = constructor.graph;
        aIController.Player = CreatePlayer();
        aIController.Monster = CreateMonster(OnMonsterTrigger);
        aIController.HallWidth = constructor.hallWidth;
        aIController.StartAI();

    }




    private void FindGoal()
    {

        GameObject[] helpers = GameObject.FindGameObjectsWithTag("Helper"); //Adds all the helpers to an array
        foreach (GameObject helper in helpers) //clears the helpers in the helper array
        {
            Destroy(helper);
        }
        Vector3 helperPos = aIController.Player.transform.position; //Sets the initial location of the helper to the player's current position
        int helperCol = (int)Mathf.Round(helperPos.x / constructor.hallWidth); //Finds the Row and Column of the helper from the given position
        int helperRow = (int)Mathf.Round(helperPos.z / constructor.hallWidth);
        Debug.Log(constructor.goalCol + "," + constructor.goalRow);
        List<Node> path = aIController.FindPath(helperRow, helperCol, constructor.goalRow, constructor.goalCol); //sets the path between the helper and the goal
        Debug.Log(path);
        if (path != null && path.Count > 1) //when the path exists and has more than 1 element
        {
            foreach (Node n in path) //loops through each node in the path
            {

                Node nextNode = n;
                float nextX = nextNode.y * constructor.hallWidth;
                float nextZ = nextNode.x * constructor.hallWidth;
                Vector3 endPosition = new Vector3(nextX, 0f, nextZ); //sets the end Position to the position of the next node
                GameObject helper = GameObject.CreatePrimitive(PrimitiveType.Sphere); //Creates the sphere object
                helper.name = "Helper";
                helper.tag = "Helper"; //ads the helper tag to the helper object
                helper.GetComponent<SphereCollider>().isTrigger = true; // disables physics collisions for the helper
                helper.transform.position = new Vector3(endPosition.x, 1f, endPosition.z); //sets the positon of the helper sphere



            }


        }




    }
}
