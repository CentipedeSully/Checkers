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
    [SerializeField] private bool _isGameOver = false;

    [Header("Unit Selection Settings")]
    [SerializeField] [Min(.1f)] private float _selectionCooldown = .5f;
    [SerializeField] private bool _isSelectorReady = true;
    [SerializeField] private CheckersUnitAttributes _selectedCheckersUnit;
    [SerializeField] private List<CheckersUnitAttributes> _unitsWithJumps;
    [SerializeField] private List<CheckersUnitAttributes> _unitsWithMoves;
    [SerializeField] private List<CheckersUnitAttributes> _availableTeamUnits;
    [SerializeField] private GameObject _moveOptionsHighlightPrefab;
    [SerializeField] private GameObject _selectablePiecesHighlightPrefab;
    [SerializeField] private Transform _highlightContainer;
    private List<GameObject> _currentHighlights;

    private bool _isJumpAvaiable = false;
    private (int, int) _jumpOrigin;
    private (int, int) _jumpEnd;

    [Header("References")]
    [SerializeField] private TurnSystem _turnSystemRef;
    [SerializeField] private GameBoard _gameBoardRef;
    [SerializeField] private MouseToWorld2D _mouseTracker2D;
    [SerializeField] private UnitController _opponentController;

    [Header("Debugging Utils")]
    [SerializeField] private bool _isDebugActive = false;
    [SerializeField] private bool _startTurnSystemCmd = false;
    [SerializeField] private bool _passTurnCmd = false;


    //Monobehaviours
    private void Awake()
    {
        _currentHighlights = new List<GameObject>();
        _availableTeamUnits = new List<CheckersUnitAttributes>();
        _unitsWithJumps = new List<CheckersUnitAttributes>();
        _unitsWithMoves = new List<CheckersUnitAttributes>();

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

        if (_isControlsUnlocked && AreAnyMovesAvailable())
            ListenForSelection();

        else if (_isControlsUnlocked && AreAnyMovesAvailable() == false)
        {
            _isGameOver = true;
            STKDebugLogger.LogStatement(_isDebugActive,$"Game Over. No moves detected.");
        }
            
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
            //STKDebugLogger.LogStatement(_isDebugActive, $"Selected Cell: {selectedPosition.Item1},{selectedPosition.Item2}");

            if (selectedPosition != (-1, -1))
            {
                //Have we made a selection, yet?
                if (_selectedCheckersUnit == null)
                {
                    if (AreAnyJumpsAvailable() && CanUnitJump(selectedPosition))
                    {
                        ClearHighlights();
                        CommitSelection(_gameBoardRef.GetPieceOnPosition(selectedPosition, GameBoardLayer.Units));
                        _isJumpAvaiable = true;
                        _jumpOrigin = selectedPosition;
                        STKDebugLogger.LogStatement(_isDebugActive, $"Jump Origin: {_jumpOrigin.Item1},{_jumpOrigin.Item2}");
                    }
                        

                    else if (AreAnyMovesAvailable() && CanPieceMove(selectedPosition))
                    {
                        ClearHighlights();
                        CommitSelection(_gameBoardRef.GetPieceOnPosition(selectedPosition, GameBoardLayer.Units));
                    }
                        
                }

                else if (_selectedCheckersUnit != null)
                {
                    if (IsSelectedMoveValid(selectedPosition) && _isJumpAvaiable)
                    {
                        //Clear Possible Moves graphics
                        ClearHighlights();

                        //Move piece to new location
                        _selectedCheckersUnit.GetComponent<GamePiece>().SetGridPosition(selectedPosition);

                        //Remove The Jumped Piece
                        _jumpEnd = selectedPosition;
                        STKDebugLogger.LogStatement(_isDebugActive, $"Jump End: {_jumpEnd.Item1},{_jumpEnd.Item2}");
                        RemoveJumpedPiece();

                        //recalculate the moves and jumps on the team 
                        FindAllPossibleJumps();
                        FindAllPossibleMoves();

                        //Cooldown Selector
                        CooldownSelector();

                        if (CanUnitJump(_selectedCheckersUnit) == false)
                        {
                            _isJumpAvaiable = false;

                            //clear selection
                            _selectedCheckersUnit = null;

                            //End turn
                            PassTurn();
                        }

                        else
                        {
                            HighlightPossibleMoves();
                            _jumpOrigin = _jumpEnd;
                        }
                            
                    }

                    else if (IsSelectedMoveValid(selectedPosition))
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

    private void CommitSelection(GamePiece piece)
    {
        _selectedCheckersUnit = piece.GetComponent<CheckersUnitAttributes>();

        //Show selection's possible moves
        HighlightPossibleMoves();

        //Cooldown Selector
        CooldownSelector();
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

        if (CanUnitJump(unit))
        {
            List<(int,int)> availableJumps = CalculateJumpMoves(unit);
            //STKDebugLogger.LogStatement(_isDebugActive, $"Jumps Detected. Supplying {availableJumps.Count} alternative Jump Moves To Selection...");
            return availableJumps;
        }
            

        else
        {
            List<(int, int)> worldBoardPositions = new List<(int, int)>();
            List<(int, int)> relativeMovementPositions = unit.GetLegalMoveDirectionsList();

            //Convert each move direction of the unit into world cells
            //Add the new world cell to the return list only if BOTH 1) the position exists on the board AND 2) the position is currently unoccupied 
            foreach ((int, int) direction in relativeMovementPositions)
            {
                GamePiece unitGamePiece = unit.GetComponent<GamePiece>();

                (int, int) worldPosition;
                worldPosition.Item1 = unitGamePiece.GetGridPosition().Item1 + direction.Item1;
                worldPosition.Item2 = unitGamePiece.GetGridPosition().Item2 + direction.Item2;

                if (IsCellInGrid(worldPosition) && !IsPositionOccupied(worldPosition))
                    worldBoardPositions.Add(worldPosition);
            }

            return worldBoardPositions;
        }
    }

    private void HighlightPossibleMoves()
    {
        List<(int, int)> availableCellPositions = CalculateWorldMoveableBoardPositionsFromCheckersUnit(_selectedCheckersUnit);
        HighlightPositions(availableCellPositions, _moveOptionsHighlightPrefab);
    }

    private void HighlightUnitsWithMoves()
    {
        List<(int, int)> xyPositions = new List<(int, int)>();

         foreach (CheckersUnitAttributes unit in _unitsWithMoves)
            xyPositions.Add(unit.GetComponent<GamePiece>().GetGridPosition());

        HighlightPositions(xyPositions, _selectablePiecesHighlightPrefab);
    }

    private void HighlightUnitsWithJumps()
    {
        List<(int, int)> xyPositions = new List<(int, int)>();

        foreach (CheckersUnitAttributes unit in _unitsWithJumps)
            xyPositions.Add(unit.GetComponent<GamePiece>().GetGridPosition());

        HighlightPositions(xyPositions, _selectablePiecesHighlightPrefab);
    }

    private void HighlightPositions(List<(int,int)> highlightPositions, GameObject highlingPrefab)
    {
        foreach ((int, int) xyPosition in highlightPositions)
        {
            GameObject newHighlight = Instantiate(_moveOptionsHighlightPrefab, _gameBoardRef.GetGrid().GetPositionFromCell(xyPosition.Item1, xyPosition.Item2), Quaternion.identity, _highlightContainer);
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

    private bool CanPieceMove(CheckersUnitAttributes unit)
    {
        return _unitsWithMoves.Contains(unit);
    }

    private bool CanPieceMove(GamePiece piece)
    {
        return CanPieceMove(piece.GetComponent<CheckersUnitAttributes>());
    }

    private bool CanPieceMove((int,int) xyPosition)
    {
        if (IsAllyOnPosition(xyPosition))
            return CanPieceMove(_gameBoardRef.GetPieceOnPosition(xyPosition, GameBoardLayer.Units));
        else return false;
    }

    private List<(int,int)> CalculateJumpMoves(CheckersUnitAttributes jumpingUnit)
    {
        if (jumpingUnit == null)
        {
            STKDebugLogger.LogWarning($"Unable to calculate jump moves from a null checker unit {name}, ID:{jumpingUnit.GetInstanceID()}");
            return null;
        }

        List<(int, int)> possibleJumpPositions = new List<(int, int)>();
        List<(int, int)> relativeMovementPositions = jumpingUnit.GetLegalMoveDirectionsList();
        GamePiece unitGamePiece = jumpingUnit.GetComponent<GamePiece>();
        (int, int) worldPosition;

        //Find all possible jumps for every move direction of the unit
        foreach ((int, int) direction in relativeMovementPositions)
        {
            worldPosition.Item1 = unitGamePiece.GetGridPosition().Item1 + direction.Item1;
            worldPosition.Item2 = unitGamePiece.GetGridPosition().Item2 + direction.Item2;

            if (IsEnemyOnPosition(worldPosition))
            {
                //the landing position is twice the move direction. Singular jumps occur in straight lines
                (int, int) landingPosition = (worldPosition.Item1 + direction.Item1, worldPosition.Item2 + direction.Item2);

                if (IsCellInGrid(landingPosition) && !IsPositionOccupied(landingPosition))
                    possibleJumpPositions.Add(landingPosition);
            }
        } 

        return possibleJumpPositions;
    }

    private bool CanUnitJump(CheckersUnitAttributes unit)
    {
        return _unitsWithJumps.Contains(unit);
    }

    private bool CanUnitJump(GamePiece piece)
    {
        return CanUnitJump(piece.GetComponent<CheckersUnitAttributes>());
    }

    private bool CanUnitJump((int,int) xyPosition)
    {
        if (IsAllyOnPosition(xyPosition))
            return CanUnitJump(_gameBoardRef.GetPieceOnPosition(xyPosition, GameBoardLayer.Units));
        else return false;
    }

    private bool IsCellInGrid((int,int) xyPosition)
    {
        return _gameBoardRef.GetGrid().IsCellInGrid(xyPosition.Item1, xyPosition.Item2);
    }

    private bool IsPositionOccupied((int,int) xyPosition)
    {
        return _gameBoardRef.IsPositionOccupied(xyPosition, GameBoardLayer.Units);
    }

    private bool IsEnemyOnPosition((int, int) xyPosition)
    {
        if (IsCellInGrid(xyPosition) && IsPositionOccupied(xyPosition))
        {
            GamePiece foundPiece = _gameBoardRef.GetPieceOnPosition(xyPosition, GameBoardLayer.Units);
            return !foundPiece.CompareTag(_team.ToString());
        }

        else return false;
    }

    private bool IsAllyOnPosition((int,int) xyPosition)
    {
        if (IsCellInGrid(xyPosition) && IsPositionOccupied(xyPosition))
        {
            GamePiece foundPiece = _gameBoardRef.GetPieceOnPosition(xyPosition, GameBoardLayer.Units);
            return foundPiece.CompareTag(_team.ToString());
        }

        else return false;
    }

    private void FindAllPossibleJumps()
    {
        List<CheckersUnitAttributes> foundJumps = new List<CheckersUnitAttributes>();

        foreach(CheckersUnitAttributes unit in _availableTeamUnits)
        {
            if (CalculateJumpMoves(unit).Count > 0)
                foundJumps.Add(unit);
        }

        _unitsWithJumps = foundJumps;
    }

    private void FindAllPossibleMoves()
    {
        List<CheckersUnitAttributes> foundMoves = new List<CheckersUnitAttributes>();

        foreach (CheckersUnitAttributes unit in _availableTeamUnits)
        {
            if (CalculateWorldMoveableBoardPositionsFromCheckersUnit(unit).Count > 0)
                foundMoves.Add(unit);
        }

        _unitsWithMoves = foundMoves;
    }

    private bool AreAnyJumpsAvailable()
    {
        return _unitsWithJumps.Count > 0;
    }

    private bool AreAnyMovesAvailable()
    {
        return _unitsWithMoves.Count > 0;
    }

    private bool DoesUnitHaveJumpAvaiable((int,int) xyPosition)
    {
        if (IsAllyOnPosition(xyPosition))
            return DoesUnitHaveJumpAvaiable(_gameBoardRef.GetPieceOnPosition(xyPosition, GameBoardLayer.Units));
        else return false;
    }

    private bool DoesUnitHaveJumpAvaiable(GamePiece piece)
    {
        return DoesUnitHaveJumpAvaiable(piece.GetComponent<CheckersUnitAttributes>());
    }

    private bool DoesUnitHaveJumpAvaiable(CheckersUnitAttributes unit)
    {
        return _unitsWithJumps.Contains(unit);
    }

    private void RemoveJumpedPiece()
    {
        (int, int) jumpedPosition = (-99, -99);
        if (_jumpOrigin.Item1 > _jumpEnd.Item1)
            jumpedPosition.Item1 = _jumpEnd.Item1 + 1;
        else if (_jumpOrigin.Item1 < _jumpEnd.Item1)
            jumpedPosition.Item1 = _jumpOrigin.Item1 + 1;

        if (_jumpOrigin.Item2 > _jumpEnd.Item2)
            jumpedPosition.Item2 = _jumpEnd.Item2 +1;
        else if (_jumpOrigin.Item2 < _jumpEnd.Item2)
            jumpedPosition.Item2 = _jumpOrigin.Item2 + 1;

        STKDebugLogger.LogStatement(_isDebugActive, $"Jumped Position: {jumpedPosition.Item1},{jumpedPosition.Item2}");
        _opponentController.RemoveUnitFromGame(_gameBoardRef.GetPieceOnPosition(jumpedPosition, GameBoardLayer.Units).GetComponent<CheckersUnitAttributes>());

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
        FindAllPossibleJumps();
        FindAllPossibleMoves();

        if (AreAnyJumpsAvailable())
            HighlightUnitsWithJumps();

        else if (AreAnyMovesAvailable())
            HighlightUnitsWithMoves();

        _isControlsUnlocked = true;
        _isTurnOver = false;
    }

    public void AddUnitToTeam(CheckersUnitAttributes newPiece)
    {
        if (!_availableTeamUnits.Contains(newPiece))
            _availableTeamUnits.Add(newPiece);
    }

    public void RemoveUnitFromGame(CheckersUnitAttributes existingPiece)
    {
        if (_availableTeamUnits.Contains(existingPiece))
        {
            _availableTeamUnits.Remove(existingPiece);
            _gameBoardRef.RemoveGamePieceFromBoard(existingPiece.GetComponent<GamePiece>());
        }
            
    }    

}
