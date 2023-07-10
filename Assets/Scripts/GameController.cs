using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private TurnSystem _turnSystem;
    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _restartButton;

    public void StartTurnSystem()
    {
        _startButton.SetActive(false);
        _restartButton.SetActive(true);
        _turnSystem.StartTurnSystem();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}
