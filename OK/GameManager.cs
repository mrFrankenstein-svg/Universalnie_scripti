using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private List<GameManager_ListOfCharacters> listOfCharacters= new List<GameManager_ListOfCharacters>();
    public static GameManager Instance { get; private set; }

    public static Action<GameObject,byte> OnCaracterHasIncreasedTheValue;

    public int score;
    public bool isPaused;
    

    private void OnEnable()
    {
        ItemCollector.OnItemHasBeenPickedUp += AddScore;
        ÑharacterStats.OnCreatingTheCharacterStatsScript += AddingANewCharacterToTheList;
    }
    private void OnDisable()
    {
        ItemCollector.OnItemHasBeenPickedUp -= AddScore;
        ÑharacterStats.OnCreatingTheCharacterStatsScript -= AddingANewCharacterToTheList;
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void AddingANewCharacterToTheList(GameObject obj, ÑharacterStats stats)
    {
        GameManager_ListOfCharacters characterScript = new GameManager_ListOfCharacters(obj, stats);
        listOfCharacters.Add(characterScript);
        Debug.Log(listOfCharacters);
    }

    void AddScore(GameObject obj, PickupItem item)
    {
        //score += amount;
        GameManager_ListOfCharacters listObj = FindCharacterInList(obj);
        if (listObj != null)
        {
            listObj.stats.values += item.value;
            OnCaracterHasIncreasedTheValue?.Invoke(obj, listObj.stats.values);
            ObjectHub.hub.Release(item.gameObject);
        }
        else 
        {
            Debug.LogError(ErorsList.OBJECT_FIND_EROR + obj+ " in "+ listOfCharacters);
        }
        
    }
    private GameManager_ListOfCharacters FindCharacterInList(GameObject obj)
    {
        for (int i = 0; i < listOfCharacters.Count; i++) 
        {
            if (listOfCharacters[i].objectItself == obj)
            { 
                return listOfCharacters[i];
            }
        }
        return null;
    }
    public void PauseGame() { isPaused = true; Time.timeScale = 0; }
    public void ResumeGame() { isPaused = false; Time.timeScale = 1; }
    public void RestartScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}

