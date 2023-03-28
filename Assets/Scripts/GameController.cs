using System;
using UnityEngine;

[RequireComponent(typeof(MazeConstructor))]           

public class GameController : MonoBehaviour
{

    public GameObject playerPrefab;
    private MazeConstructor constructor;
    [SerializeField] private int rows;
    [SerializeField] private int cols;
    void Awake()
    {
        constructor = GetComponent<MazeConstructor>();
    }
    
    private void CreatePlayer()
    {
        Vector3 playerStartPosition = new Vector3(constructor.hallWidth, 1, constructor.hallWidth);  
        GameObject player = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
        player.tag = "Generated";
    }


    void Start() 
    {
        constructor.GenerateNewMaze(rows,cols);
            
        CreatePlayer();
    }
}