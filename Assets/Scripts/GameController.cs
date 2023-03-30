using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

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
        if(Input.GetKeyDown("f"))
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
        GameObject helper = Instantiate(monsterPrefab, monsterPosition, Quaternion.identity);
        helper.tag = "Generated";   
        TriggerEventRouter tm = helper.AddComponent<TriggerEventRouter>();
        tm.callback = monsterCallback;



        return helper; 
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

        GameObject helper = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        helper.transform.position = aIController.Player.transform.position;
        int helperCol = (int)Mathf.Round(helper.transform.position.x / constructor.hallWidth);
        int helperRow = (int)Mathf.Round(helper.transform.position.z / constructor.hallWidth);
        Debug.Log(constructor.goalCol + "," + constructor.goalRow);
        List<Node> path = aIController.FindPath(helperRow,helperCol,constructor.goalRow,constructor.goalCol);
         while(path != null && path.Count > 1)
            {
                Node nextNode = path[1];
                float nextX = nextNode.y * constructor.hallWidth;
                float nextZ = nextNode.x * constructor.hallWidth;
                Vector3 endPosition = new Vector3(nextX, 0f, nextZ);
                float step =  15 * Time.deltaTime;
                helper.transform.position = Vector3.MoveTowards(helper.transform.position, endPosition, step);
            
                if(helper.transform.position == endPosition){
                    helperRow = nextNode.x;
                    helperCol = nextNode.y;
                }
            }


        
    }
}
