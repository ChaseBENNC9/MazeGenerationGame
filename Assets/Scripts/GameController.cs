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
}
