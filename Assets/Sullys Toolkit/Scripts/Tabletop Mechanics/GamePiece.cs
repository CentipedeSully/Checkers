using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit.TableTop
{
    public enum GamePieceType
    {
        Terrain,
        PointOfInterest,
        Unit
    }

    public class GamePiece : MonoBehaviour
    {
        //Declarations
        [Header("Play Data")]
        [SerializeField] private GamePieceType _typeOfGamepiece;
        [SerializeField] private bool _isInPlay;
        [SerializeField] private (int, int) _currentGridPosition = (-1, -1);

        [Header("GamePiece Metadata")]
        [Tooltip("Where will the gamePiece be stored when out of play")]
        [SerializeField] private Transform _bagOfHolding;
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private GameBoardLayer _boardLayer = GameBoardLayer.Undefined;









        //Monobehaviours





        //Internal Utils
        private void MoveToPositionOnBoard((int,int) xyBoardPosition)
        {
            transform.position = _gameBoard.GetGrid().GetPositionFromCell(xyBoardPosition.Item1, xyBoardPosition.Item2);
        }

        private void MoveToPositionOutOfPlay()
        {
            transform.position = _bagOfHolding.position;
        }

        private void ReparentGamePieceToHoldingTransform()
        {
            transform.SetParent(_bagOfHolding);
        }



        //Getters, Setters, & Commands
        public bool IsInPlay()
        {
            return _isInPlay;
        }

        public GameBoard GetGameBoard()
        {
            return _gameBoard;
        }

        public void SetGameBoard(GameBoard newBoard)
        {
            if (newBoard != null)
                _gameBoard = newBoard;
        }

        public GamePieceType GetGamePieceType()
        {
            return _typeOfGamepiece;
        }

        public void ClearGamePieceBoardData()
        {
            _gameBoard = null;
            _currentGridPosition = (-1, -1);
        }

        public GameBoardLayer GetBoardLayer()
        {
            return _boardLayer;
        }

        public void SetBoardLayer(GameBoardLayer newLayer)
        {
            _boardLayer = newLayer;
        }

        public (int,int) GetGridPosition()
        {
            return _currentGridPosition;
        }

        public void SetGridPosition((int,int) newPosition)
        {
            if (_gameBoard.GetGrid().IsCellInGrid(newPosition.Item1, newPosition.Item2))
            {
                _currentGridPosition = newPosition;
                MoveToPositionOnBoard(newPosition);
            }
        }

        public Transform GetOutOfPlayHoldingLocation()
        {
            return _bagOfHolding;
        }

        public void SetOutOfPlayHoldingLocation(Transform newTransform)
        {
            _bagOfHolding = newTransform;
        }

        public void RemoveFromPlay()
        {
            _isInPlay = false;
            gameObject.SetActive(false);
            ClearGamePieceBoardData();

            ReparentGamePieceToHoldingTransform();
            MoveToPositionOutOfPlay();
        }

        public void MoveIntoPlay((int,int) newPosition)
        {
            if (_isInPlay == false)
            {
                _isInPlay = true;
                SetGridPosition(newPosition);
            }
        }

    }
}

