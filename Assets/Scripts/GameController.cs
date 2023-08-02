using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit.TableTop;
using SullysToolkit;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    [SerializeField] private TurnSystem _turnSystem;
    [SerializeField] private GameObject _startButton;
    [SerializeField] private GameObject _restartButton;
    [SerializeField] private UIController _uiController;
    [SerializeField] private UnitController _darkTeamController;
    [SerializeField] private UnitController _lightTeamController;
    [SerializeField] private GameBoard _gameBoard;
    [SerializeField] private GameObject _winningPiecesHighlightPrefab;
    [SerializeField] private GameObject _drawHighlightPrefab;
    [SerializeField] private int _turnsUntilDraw = 40;
    [SerializeField] private AudioManager _audioManager;


    private void HighlightPieces( List<(int,int)> xyPositions, GameObject highlightPrefab)
    {
        foreach ((int,int) xyPosition in xyPositions)
        {
            if (_gameBoard.GetGrid().IsCellInGrid(xyPosition.Item1,xyPosition.Item2))
            {
                GameObject newHighlight = Instantiate(highlightPrefab, _gameBoard.GetGrid().GetPositionFromCell(xyPosition.Item1, xyPosition.Item2),
                                            Quaternion.identity, transform);

            }
        }
    }

    private List<(int,int)> BuildAllPositionsList()
    {
        List<(int, int)> allOccupiedPositions = new List<(int, int)>();

        foreach ((int,int) position in _darkTeamController.GetAllUnitPositions())
            allOccupiedPositions.Add(position);

        foreach ((int, int) position in _lightTeamController.GetAllUnitPositions())
            allOccupiedPositions.Add(position);

        return allOccupiedPositions;
    }

    public void StartGame()
    {
        _startButton.SetActive(false);
        _restartButton.SetActive(true);
        _turnSystem.StartTurnSystem();
        _uiController.ReinitializeDrawCounter();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void EndGameViaVictory(UnitController winningTeamController)
    {
        _turnSystem.StopTurnSystem();
        HighlightPieces(winningTeamController.GetAllUnitPositions(), _winningPiecesHighlightPrefab);

        _uiController.ShowGameOverUI();
        _uiController.HideDarkTurnUI();
        _uiController.HideLightTurnUI();

        _audioManager.PlayGameOver();
    }

    public void EndGameViaDraw()
    {
        _turnSystem.StopTurnSystem();
        HighlightPieces(BuildAllPositionsList(), _drawHighlightPrefab);

        _uiController.ShowDrawGameOverUI();
        _uiController.HideDarkTurnUI();
        _uiController.HideLightTurnUI();

        _audioManager.PlayGameOver();
    }

    public int GetTurnsUntilDraw()
    {
        return _turnsUntilDraw;
    }

    
}
