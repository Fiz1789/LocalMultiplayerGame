using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RowdyRobots
{
public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public static GameManager instance;


    public List<PlayerInput> playerCharacterPrefabs;

    public List<int> playerDeviceIds;

    public List<Color> playerCharacterOuterColors;
    public List<Color> playerCharacterInnerColors;
    public GameState gameState; 

    public enum GameState
    {
        // MainMenu,
        CharacterSelection,
        Battle
    }


    void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    
    void Start()
    {
     gameState = GameState.CharacterSelection;  
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
}