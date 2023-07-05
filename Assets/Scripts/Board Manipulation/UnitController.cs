using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit;
using SullysToolkit.TableTop;

public enum Team
{
    Dark,
    Light
}


public class UnitController : MonoBehaviour, ITurnListener
{
    //Declarations
    [Header("Control Settings")]
    [SerializeField] private bool _isControlsUnlocked = false;
    [SerializeField] private Team _team;
    private TurnPhase _unlockPhase;
    [SerializeField] private bool _isTurnOver = true;

    [Header("Unit Selection Settings")]
    [SerializeField] [Min(.1f)] private float _selectionCooldown = .5f;
    [SerializeField] private bool _isSelectorReady = true;
    [SerializeField] private CheckersUnitAttributes _selectedCheckersUnit;
    [SerializeField] private GameObject _highlightGraphicPrefab;
    [SerializeField] private Transform _highlightContainer;
    [SerializeField] private List<GameObject> _currentHighlights;

    [Header("References")]
    [SerializeField] private TurnSystem _turnSystemRef;
    [SerializeField] private GameBoard _gameBoardRef;
    [SerializeField] private MouseToWorld2D _mouseTracker2D;

    [Header("Debugging Utils")]
    [SerializeField] private bool _isDebugActive = false;
    [SerializeField] private bool _startTurnSystemCmd = false;
    [SerializeField] private bool _passTurnCmd = false;


    //Monobehaviours
    private void Awake()
    {
        if (_currentHighlights.Count == 0)
            _currentHighlights = new List<GameObject>();

        DetermineUnlockPhase();
    }

    private void Start()
    {
        AddThisControllerToTurnListener();
    }

    private void Update()
    {
        if (_isDebugActive)
            ListenForDebugCommands();

        if (_isControlsUnlocked)
            ListenForSelection();
    }



    //Internal Utils
    private void AddThisControllerToTurnListener()
    {
        if (_turnSystemRef != null)
            _turnSystemRef.AddTurnListener(this);
    }

    private void DetermineUnlockPhase()
    {
        if (_team == Team.Dark)
            _unlockPhase = TurnPhase.MainPhase;
        else if (_team == Team.Light)
            _unlockPhase = TurnPhase.ReactionPhase;
    }

    private void PassTurn()
    {
        _isTurnOver = true;
    }

    private void ListenForSelection()
    {
        if (Input.GetMouseButton(0) && _isSelectorReady)
        {
            (int, int) selectedPosition = CaptureBoardLocationOfMouse();

            if (selectedPosition != (-1, -1))
            {
                //Have we made a selection, yet?
                if (_selectedCheckersUnit == null && _gameBoardRef.IsPositionOccupied(selectedPosition, GameBoardLayer.Units))
                {
                    //Save selection if we selected our own piece
                    if (_gameBoardRef.GetPieceOnPosition(selectedPosition, GameBoardLayer.Units).CompareTag(_team.ToString()))
                    {
                        _selectedCheckersUnit = _gameBoardRef.GetPieceOnPosition(selectedPosition, GameBoardLayer.Units).GetComponent<CheckersUnitAttributes>();

                        //Show selection's possible moves
                        HighlightPossibleMoves();

                        //Cooldown Selector
                        CooldownSelector();
                    }
                }

                else if (_selectedCheckersUnit != null)
                {
                    if (IsSelectedMoveValid(selectedPosition))
                    {
                        //Clear Possible Moves graphics
                        ClearHighlights();

                        //Move piece to new location
                        _selectedCheckersUnit.GetComponent<GamePiece>().SetGridPosition(selectedPosition);

                        //clear selection
                        _selectedCheckersUnit = null;

                        //Cooldown Selector
                        CooldownSelector();

                        //End turn
                        PassTurn();
                    }
                }
            }

            else
                STKDebugLogger.LogStatement(_isDebugActive, $"{name}, ID:{GetInstanceID()} Captured a position off grid. Selection Ignored");
            
        }
    }

    private void ListenForDebugCommands()
    {
        if (_startTurnSystemCmd)
        {
            _startTurnSystemCmd = false;

            if (_turnSystemRef.IsTurnSystemActive() == false)
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"{GetConcreteListenerNameForDebugging()} attempting to start the turnSystem...");
                _turnSystemRef.StartTurnSystem();
            }
        }

        if (_passTurnCmd)
        {
            _passTurnCmd = false;
            PassTurn();
        }
    }

    private (int,int) CaptureBoardLocationOfMouse()
    {
        Vector3 mousePosition = _mouseTracker2D.GetWorldPosition();

        if (_gameBoardRef.GetGrid().IsPositionOnGrid(mousePosition))
            return _gameBoardRef.GetGrid().GetCellFromPosition(mousePosition);

        else
            return (-1, -1);
    }

    private bool IsSelectedMoveValid((int,int) xyPosition)
    {
        if (CalculateWorldMoveableBoardPositionsFromCheckersUnit(_selectedCheckersUnit).Contains(xyPosition))
            return true;
        else return false;
    }

    private List<(int,int)> CalculateWorldMoveableBoardPositionsFromCheckersUnit(CheckersUnitAttributes unit)
    {
        if (unit == null)
        {
            STKDebugLogger.LogWarning($"Unable to calculate world moves from a null checker unit {name}, ID:{unit.GetInstanceID()}");
            return null;
        }

        List<(int, int)> worldBoardPositions = new List<(int, int)>();
        List<(int, int)> relativeMovementPositions = unit.GetLegalMoveDirectionsList();

        //Convert each move direction of the unit into world cells
        //Add them to the return list if the position exists on the board
        foreach((int,int) direction in relativeMovementPositions)
        {
            GamePiece unitGamePiece = unit.GetComponent<GamePiece>();

            (int, int) worldPosition;
            worldPosition.Item1 = unitGamePiece.GetGridPosition().Item1 + direction.Item1;
            worldPosition.Item2 = unitGamePiece.GetGridPosition().Item2 + direction.Item2;

            if (_gameBoardRef.GetGrid().IsCellInGrid(worldPosition.Item1, worldPosition.Item2))
                worldBoardPositions.Add(worldPosition);
        }

        return worldBoardPositions;
    }

    private void HighlightPossibleMoves()
    {
        List<(int, int)> availableCellPositions = CalculateWorldMoveableBoardPositionsFromCheckersUnit(_selectedCheckersUnit);

        foreach ((int,int) cellPosition in availableCellPositions)
        {
            GameObject newHighlight = Instantiate(_highlightGraphicPrefab, _gameBoardRef.GetGrid().GetPositionFromCell(cellPosition.Item1, cellPosition.Item2), Quaternion.identity, _highlightContainer);
            _currentHighlights.Add(newHighlight);
        }
    }

    private void ClearHighlights()
    {
        foreach (GameObject highlight in _currentHighlights)
            Destroy(highlight);

        _currentHighlights.Clear();
    }

    private void ReadySelector()
    {
        _isSelectorReady = true;
    }

    private void CooldownSelector()
    {
        _isSelectorReady = false;
        Invoke("ReadySelector", _selectionCooldown);
    }

    //Getters, Setters, & Commands
    public string GetConcreteListenerNameForDebugging()
    {
        return name + " ID: " + gameObject.GetInstanceID();
    }

    public int GetResponsePhase()
    {
        return (int)_unlockPhase;
    }

    public ITurnBroadcaster GetTurnBroadcaster()
    {
        return (ITurnBroadcaster)_turnSystemRef;
    }

    public bool IsTurnListenerReadyToPassPhase()
    {
        return _isTurnOver;
    }

    public void ResetResponseFlag()
    {
        _isControlsUnlocked = false;
        _isTurnOver = true;
    }

    public void ResetUtilsOnTurnSystemInterruption()
    {
        ResetResponseFlag();
    }

    public void RespondToNotification(int turnNumber)
    {
        _isControlsUnlocked = true;
        _isTurnOver = false;
    }



}
