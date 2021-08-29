
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoop : MonoBehaviour
{
    public static int passedShifts;
    public static int numberOfShifts = 2;
    public static GameLoop instance;

    [SerializeField] GameObject player;
    [SerializeField] DishesSpawner spawner;
    [SerializeField] GameObject winPanel;


    Shift currentShift;
    private Coroutine changingShift;

    private void Awake()
    {
        if (instance != null) throw new Exception("More than one Game Manager");
        instance = this;
        currentShift = FindObjectOfType<Shift>();
        if (passedShifts > 0)
        {
            StartGame();
        }

    }

    private void OnEnable()
    {
        Sink.OnDishesCleaned += CheckGoalCompletition;
    }

    public void StartGame()
    {

        player.GetComponent<PlayerInteraction>().enabled = true;
        player.GetComponent<PlayerMovement>().enabled = true;

        spawner.shift = currentShift;
        currentShift.StartShift();
        spawner.enabled = true;

    }

    public void CheckGoalCompletition()
    {
        if (currentShift.CheckGoalsCompletition())
        {
            if (changingShift != null) return;
            changingShift = StartCoroutine(ChangeShift());
        }
        else if (currentShift.hasEnded)
        {
            RestartScene();
        }
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator ChangeShift()
    {
        passedShifts++;
        DeactivateAllDishes();
        spawner.enabled = false;


        if (passedShifts >= numberOfShifts)
        {
            WinGame();
            yield break;
        }

        player.GetComponent<PlayerInteraction>().enabled = false;


        yield return StartCoroutine(currentShift.Break());

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }


    private void DeactivateAllDishes()
    {
        foreach (AIBehaviour dish in FindObjectsOfType<AIBehaviour>())
        {
            dish.gameObject.SetActive(false);
        }
    }

    public void WinGame()
    {
        winPanel.SetActive(true);
    }

}
