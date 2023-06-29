using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SullysToolkit.TableTop
{
    public enum GameBoardLayer
    {
        Undefined,
        Units,
        PointsOfInterest,
        Terrain
    }

    public class GameBoard : MonoBehaviour
    {
        //Declarations
        [Header("Board Size")]
        [SerializeField] [Min(1)] private int _rows = 1;
        [SerializeField] [Min(1)] private int _columns = 1;
        [SerializeField] [Min(.1f)] private float _cellSize = .1f;
        [SerializeField] private List<GamePiece> _gamePiecesInPlay;
        [SerializeField] private bool _isBoardInitialized;
        private GridSystem<bool> _boardGrid;

        [Header("TurnSystem Settings")]
        [SerializeField] private TurnSystem _turnSystem;

        [Header("Debug Settings")]
        [SerializeField] private bool _isDebugActive;



        //Monobehaviours
        private void Awake()
        {
            InitializeGamePieceList();   
        }




        //Internal Utils
        private void CreateBoardGrid()
        {
            Vector3 boardOrigin = new Vector3(transform.position.x - (_columns * _cellSize) / 2, transform.position.y - _rows * _cellSize / 2, transform.position.z);
            _boardGrid = new GridSystem<bool>(_columns, _rows, _cellSize, boardOrigin, () => { return false; });
            _boardGrid.SetDebugDrawDuration(999);
            _boardGrid.SetDebugDrawing(true);
        }

        private void InitializeGamePieceList()
        {
            _gamePiecesInPlay = new List<GamePiece>();
        }

        private void SetGamePieceAsChild(GamePiece gamePiece)
        {
            gamePiece.transform.SetParent(this.transform);
        }

        private void SubscribePieceToTurnSystem(GamePiece gamePiece)
        {
            if (_turnSystem != null)
            {
                ITurnListener turnListener = gamePiece.GetComponent<ITurnListener>();
                if (turnListener != null)
                    _turnSystem.AddTurnListener(turnListener);
            }
            
        }

        private void UnsubscribePieceFromTurnSystem(GamePiece gamePiece)
        {
            if (_turnSystem != null)
            {
                ITurnListener turnListener = gamePiece.GetComponent<ITurnListener>();
                if (turnListener != null)
                    _turnSystem.RemoveTurnListener(turnListener);
            }
            
        }



        //Getters, Setters, & Commands
        public void InitializeGameBoard(int rows, int columns)
        {
            _rows = Mathf.Max(1, rows);
            _columns = Mathf.Max(1, columns);

            InitializeGamePieceList();
            CreateBoardGrid();
            _isBoardInitialized = true;
        }

        public bool IsGameBoardInitialized()
        {
            return _isBoardInitialized;
        }

        public GridSystem<bool> GetGrid()
        {
            if (_isBoardInitialized)
                return _boardGrid;
            else
            {
                STKDebugLogger.LogWarning("Board not initialized. Returning null");
                return null;
            }
        }

        public int GetRowCount()
        {
            return _rows;
        }

        public int GetColumnCount()
        {
            return _columns;
        }

        public float GetCellSize()
        {
            return _cellSize;
        }

        public List<GamePiece> GetAllGamePiecesInPlay()
        {
            if (_isBoardInitialized)
                return _gamePiecesInPlay;

            else
            {
                STKDebugLogger.LogWarning("Board not initalized. No pieces in play to return. Returning null");
                return null;
            }
            
        }

        public List<GamePiece> GetPiecesInLayer(GameBoardLayer layer)
        {
            if (_isBoardInitialized)
            {
                LogStatement($"Fetching all active gamePieces in layer {layer}...");
                List<GamePiece> specifiedGamePiecesList =
                    (from gamePiece in _gamePiecesInPlay
                     where gamePiece.GetBoardLayer() == layer
                     select gamePiece).ToList();

                LogStatement($"Game Pieces of layer {layer} found: {specifiedGamePiecesList.Count}");
                return specifiedGamePiecesList;
            }
            else
            {
                STKDebugLogger.LogWarning("Board isn't initialized. Returning null value");
                return null;
            }
        }

        public List<GamePiece> GetPiecesOnPosition((int,int) xyPosition)
        {
            if (_isBoardInitialized)
            {
                LogStatement($"Fetching all actve gamePieces on position {xyPosition.Item1},{xyPosition.Item2}...");
                List<GamePiece> querydPieces =
                    (from gamePiece in _gamePiecesInPlay
                     where gamePiece.GetGridPosition() == xyPosition
                     select gamePiece).ToList();

                LogStatement($"GamePieces at position {xyPosition.Item1},{xyPosition.Item2} found: {querydPieces.Count}");
                return querydPieces;
            }
            else
            {
                STKDebugLogger.LogWarning("Board isn't initialized. Returning null value");
                return null;
            }
            
        }

        public void AddGamePiece(GamePiece newGamePiece, GameBoardLayer deseiredLayer, (int, int) xyDesiredPosition)
        {
            if (_isBoardInitialized)
            {
                LogStatement($"Checking if adding {newGamePiece.gameObject.name} to position ({xyDesiredPosition.Item1},{xyDesiredPosition.Item2}) is a valid");
                bool _doesPositionExistOnBoard = _boardGrid.IsCellInGrid(xyDesiredPosition.Item1, xyDesiredPosition.Item2);
                bool _doesPieceAlreadyExistOnBoard = DoesGamePieceExistOnBoard(newGamePiece);
                bool _isPositionAlreadyOccupiedOnLayer = IsPositionOccupied(xyDesiredPosition, deseiredLayer);

                if (!_doesPieceAlreadyExistOnBoard && !_isPositionAlreadyOccupiedOnLayer && _doesPositionExistOnBoard)
                {
                    LogStatement($"Attempting to Add {newGamePiece.gameObject.name} to gameBoard...");
                    SetGamePieceAsChild(newGamePiece);
                    newGamePiece.SetGameBoard(this);
                    newGamePiece.SetBoardLayer(deseiredLayer);
                    newGamePiece.MoveIntoPlay(xyDesiredPosition);
                    SubscribePieceToTurnSystem(newGamePiece);

                    if (newGamePiece.gameObject.activeSelf == false)
                        newGamePiece.gameObject.SetActive(true);

                    _gamePiecesInPlay.Add(newGamePiece);

                }
                else
                    LogStatement($"Cannot add {newGamePiece.gameObject.name} to position ({xyDesiredPosition.Item1},{xyDesiredPosition.Item2})");
            }
            else
                STKDebugLogger.LogWarning("Board isn't initialized. Ignoring 'AddGamePiece' command");

        }

        public void RemoveGamePieceFromBoard(GamePiece gamePiece)
        {
            if (_isBoardInitialized)
            {
                LogStatement($"Attempting Removal of {gamePiece.gameObject.name}...");
                if (_gamePiecesInPlay.Contains(gamePiece))
                {
                    _gamePiecesInPlay.Remove(gamePiece);
                    gamePiece.RemoveFromPlay();
                    UnsubscribePieceFromTurnSystem(gamePiece);
                    LogStatement($"{ gamePiece.gameObject.name} removal successful");
                }
                else
                    LogStatement($"{gamePiece.gameObject.name} not found on Gameboard");
            }
            else
                STKDebugLogger.LogWarning("Board not initialized. Ignoring remove command.");
            
        }

        public bool IsPositionOccupied((int,int) xyPosition, GameBoardLayer layer)
        {
            if (_isBoardInitialized)
            {
                LogStatement($"Checking Cell ({xyPosition.Item1},{xyPosition.Item2}) Occupancy...");
                List<GamePiece> possiblePieces = GetPiecesInLayer(layer);

                var occupancyQuery =
                    from gamePiece in possiblePieces
                    where gamePiece.GetGridPosition() == xyPosition
                    select gamePiece;

                LogStatement($"Occupancies found: {occupancyQuery.Count()}");
                if (occupancyQuery.Count() > 0)
                    return true;
                else return false;
            }
            else
            {
                STKDebugLogger.LogWarning("Board not initialized. No positions exist. returning false.");
                return false;
            }

        }

        public bool DoesGamePieceExistOnBoard(GamePiece gamePiece)
        {
            if (_isBoardInitialized)
            {
                LogStatement($"Does {gamePiece.gameObject.name} Exist On Board: { _gamePiecesInPlay.Contains(gamePiece)}");
                return _gamePiecesInPlay.Contains(gamePiece);
            }
            else
            {
                STKDebugLogger.LogWarning("Board not initialized. No pieces exist on board. Returning false");
                return false;
            }
            
        }



        //Debug Utils
        private void LogStatement(string statement)
        {
            if (_isDebugActive)
                Debug.Log($"{statement}");
        }
    }
}

