using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit.TableTop.RPG
{
    public class Movement : MonoBehaviour, IMoveableRPGPiece, IRegenerateable, IDisplayableRPGAttribute
    {
        //Declarations
        [Header("Movement Attributes")]
        [SerializeField] private int _currentMovePoints;
        [SerializeField] private int _maxMovePoints;

        [Header("Movement Costs")]
        [SerializeField] private int _adjacentMoveCost = 10;
        [SerializeField] private int _diagonalMoveCost = 14;

        [Header("References")]
        [SerializeField] private GamePiece _gamePieceReference;
        [SerializeField] private IUIDisplayController _displayControllerRef;

        [Header("Debugging Utils")]
        [SerializeField] private bool _isDebugActive = false;

        //Events
        public delegate void MovementEvent();
        public event MovementEvent OnGamePieceMoved;
        public event MovementEvent OnMPValueChanged;



        //Declarations
        private void Awake()
        {
            InitializeSettings();
        }

        private void OnEnable()
        {
            OnMPValueChanged += UpdateDisplayWrapper;
        }

        private void OnDisable()
        {
            OnMPValueChanged -= UpdateDisplayWrapper;
        }

        //Internal Utils
        private void InitializeSettings()
        {
            _gamePieceReference = GetComponent<GamePiece>();
            _displayControllerRef = GetComponent<IUIDisplayController>();

            _currentMovePoints = _maxMovePoints;
        }

        private bool IsGamePieceInPlay()
        {
            return _gamePieceReference.IsInPlay();
        }

        private bool IsDestinationValid((int, int) xyDestination)
        {
            STKDebugLogger.LogStatement(_isDebugActive, $"Validating Move Command to Position {xyDestination.Item1},{xyDestination.Item2}...");

            GameBoard gameBoard = _gamePieceReference.GetGameBoard();
            bool doesCellExistOnGrid = gameBoard.GetGrid().IsCellInGrid(xyDestination.Item1, xyDestination.Item2); ;
            bool isCellUnoccupied = !gameBoard.IsPositionOccupied(xyDestination, _gamePieceReference.GetBoardLayer());

            STKDebugLogger.LogStatement(_isDebugActive, $"Validation Results for {_gamePieceReference.gameObject.name}:" +
                $"\nDoesCellExistOnGrid: {doesCellExistOnGrid}" +
                $"\nIsCellUnoccupied: {isCellUnoccupied}" +
                $"\nIsDestinationvalid: {doesCellExistOnGrid && isCellUnoccupied}");

            return doesCellExistOnGrid && isCellUnoccupied;
        }

        private void DecrementMovePoints(int cost)
        {
            STKDebugLogger.LogStatement(_isDebugActive, $"Decrementing {cost} MovePoints from {_gamePieceReference.gameObject.name}...");
            _currentMovePoints -= cost;
            STKDebugLogger.LogStatement(_isDebugActive, $"New total movePoints: {_currentMovePoints}");
            TriggerMPChangeEvent();
        }

        private void TriggerMPChangeEvent()
        {
            OnMPValueChanged?.Invoke();
        }




        //Getters, Setters, & Commands
        public GamePiece GetGamePiece()
        {
            return _gamePieceReference;
        }

        public int GetCurrentMovePoints()
        {
            return _currentMovePoints;
        }

        public void SetCurrentMovePoints(int value)
        {
            _currentMovePoints = Mathf.Clamp(value, 0, _maxMovePoints);
            TriggerMPChangeEvent();
        }

        public int GetMaxMovePoints()
        {
            return _maxMovePoints;
        }

        public void SetMaxMovePoints(int value)
        {
            _maxMovePoints = Mathf.Max(1, value);

            //Reflect the new maxiumum
            SetCurrentMovePoints(_currentMovePoints);
        }

        public void MoveToNeighborCell((int, int) xyDirection)
        {
            if (IsGamePieceInPlay())
            {
                //Ignore 0,0 Move Command
                if (xyDirection.Item1 == 0 && xyDirection.Item2 == 0)
                {
                    STKDebugLogger.LogStatement(_isDebugActive, $"Movement unnecessary for {_gamePieceReference.gameObject.name} in direction " +
                        $"{xyDirection.Item1},{xyDirection.Item2}.\n" +
                        $"Ignoring move command");

                    return;
                }


                //Clamp Movement
                int xDirection = Mathf.Clamp(xyDirection.Item1, -1, 1);
                int yDirection = Mathf.Clamp(xyDirection.Item2, -1, 1);


                //Calculate move Cost
                STKDebugLogger.LogStatement(_isDebugActive, $"Calculating Movement cost for {_gamePieceReference.gameObject.name} in direction " +
                    $"{xDirection},{yDirection}...");

                int moveCost;
                if (xDirection != 0 && yDirection != 0)
                    moveCost = _diagonalMoveCost;
                else moveCost = _adjacentMoveCost;


                if (_currentMovePoints >= moveCost)
                {
                    //Create xyPosition
                    int xDestination = _gamePieceReference.GetGridPosition().Item1 + xDirection;
                    int yDestination = _gamePieceReference.GetGridPosition().Item2 + yDirection;
                    (int, int) xyDestination = (xDestination, yDestination);

                    if (IsDestinationValid(xyDestination))
                    {
                        DecrementMovePoints(moveCost);

                        STKDebugLogger.LogStatement(_isDebugActive, $"Moving {_gamePieceReference.gameObject.name} to cell {xDestination},{yDestination}...");
                        _gamePieceReference.SetGridPosition(xyDestination);
                        STKDebugLogger.LogStatement(_isDebugActive, $"Move Completed. New Position: " +
                            $"{_gamePieceReference.GetGridPosition().Item1},{_gamePieceReference.GetGridPosition().Item2}");

                        OnGamePieceMoved?.Invoke();
                    }

                    else
                        STKDebugLogger.LogStatement(_isDebugActive, $"Move Command Aborted due to Invalid Destination");

                }

                else
                    STKDebugLogger.LogStatement(_isDebugActive, $"Insufficient MovePoints on {_gamePieceReference.gameObject.name} for Move Command" +
                        $"in direction ({xDirection},{yDirection}):\n" +
                        $"Needed: {moveCost}\n" +
                        $"Owned: {_currentMovePoints}");
            }

            else
                STKDebugLogger.LogStatement(_isDebugActive, $"GamePiece {_gamePieceReference.gameObject.name} not in play." +
                    $"\n Ignoring Move Command");


        }

        public void RegenerateAttributes()
        {
            SetCurrentMovePoints(_maxMovePoints);
        }

        public void UpdateAttributeInDisplay(IUIDisplayController displayController)
        {
            displayController.UpdateData();
        }

        public void UpdateDisplayWrapper()
        {
            UpdateAttributeInDisplay(_displayControllerRef);
        }
    }


}
