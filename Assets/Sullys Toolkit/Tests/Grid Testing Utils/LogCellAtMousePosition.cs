using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit
{
    public class LogCellAtMousePosition : MonoBehaviour
    {
        //Declarations
        [Header("Required References")]
        [SerializeField] private MouseToWorld2D _mouseTracker;
        [SerializeField] private GridSystem<bool> _referenceGrid;

        [Header("Logging Settings")]
        [SerializeField] private bool _logResults = true;

        [Header("Test Results")]
        [SerializeField] private bool _isMouseOnGrid = false;
        [SerializeField] private Vector3 _currentMousePosition;
        [SerializeField] private (int, int) _cellPosition;
    

        //Monobehaviours
        private void Update()
        {
            if (_logResults)
                LogPositionalData();
        }



        //Internal Utils
        private void LogPositionalData()
        {
            if (_mouseTracker != null && _referenceGrid != null)
            {
                _currentMousePosition = _mouseTracker.GetWorldPosition();
                _isMouseOnGrid = _referenceGrid.IsPositionOnGrid(_currentMousePosition);
                if (_isMouseOnGrid)
                    _cellPosition = _referenceGrid.GetCellFromPosition(_currentMousePosition);
                else _cellPosition = (-1, -1);

                //Debug.Log($"Mouse Position: {_currentMousePosition}");
                Debug.Log($"Is Mouse On Grid: {_isMouseOnGrid}");
                Debug.Log($"Cell Position: ({_cellPosition.Item1}, {_cellPosition.Item2})");
            }
        }



        //Getters, Setters, & Commands
        public GridSystem<bool> GetReferenceGrid()
        {
            return _referenceGrid;
        }

        public void SetReferenceGrid(GridSystem<bool> newGrid)
        {
            if (newGrid != null)
                _referenceGrid = newGrid;
        }




    }

}

