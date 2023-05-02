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
        if(Input.GetKeyDown(KeyCode.F))
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
        constructor.GenerateNewMaze(rows,cols,OnTreasureTrigger);
            
        aIController.Graph = constructor.graph;
        aIController.Player = CreatePlayer();
        aIController.Monster = CreateMonster(OnMonsterTrigger); 
        aIController.HallWidth = constructor.hallWidth;         
        aIController.StartAI();

    }




    private void OnTreasureTrigger(GameObject trigger, GameObject other)
    { 
        Debug.Log("You Won!");
        aIController.StopAI();
        GameObject[] helpers = GameObject.FindGameObjectsWithTag("Helper");
        foreach(GameObject helper in helpers)
        { 
            Destroy(helper);         
        }
    }


    private void OnMonsterTrigger(GameObject trigger, GameObject other)
    {
        Debug.Log("Gotcha");
        aIController.StopAI();
        constructor.GenerateNewMaze(rows,cols,OnTreasureTrigger);
            
        aIController.Graph = constructor.graph;
        aIController.Player = CreatePlayer();
        aIController.Monster = CreateMonster(OnMonsterTrigger); 
        aIController.HallWidth = constructor.hallWidth;         
        aIController.StartAI();

    }




    private void FindGoal()
    {

        GameObject[] helpers = GameObject.FindGameObjectsWithTag("Helper");
        foreach(GameObject helper in helpers)
        { 
            Destroy(helper);         
        }
        Vector3 helperPos = aIController.Player.transform.position;
        int helperCol = (int)Mathf.Round(helperPos.x / constructor.hallWidth);
        int helperRow = (int)Mathf.Round(helperPos.z / constructor.hallWidth);
        Debug.Log(constructor.goalCol + "," + constructor.goalRow);
        List<Node> path = aIController.FindPath(helperRow,helperCol,constructor.goalRow,constructor.goalCol);
        Debug.Log(path);
         if(path != null && path.Count > 1)
        {
            foreach(Node n in path)
            {

                    Node nextNode = n;
                    float nextX = nextNode.y * constructor.hallWidth;
                    float nextZ = nextNode.x * constructor.hallWidth;
                    Vector3 endPosition = new Vector3(nextX, 0f, nextZ);
                    GameObject helper = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    helper.name = "Helper";
                    helper.tag = "Helper";
                    helper.GetComponent<SphereCollider>().isTrigger = true;
                    helper.transform.position = new Vector3(endPosition.x,1f,endPosition.z);


                    if(helper.transform.position == endPosition)
                    {
                        helperRow = nextNode.x;
                        helperCol = nextNode.y;
                    }
                }

            
        }
         


        
    }
}
