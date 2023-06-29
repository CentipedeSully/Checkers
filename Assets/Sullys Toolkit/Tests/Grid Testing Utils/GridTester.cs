using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit;

public class GridTester : MonoBehaviour
{
    //Declarations
    private GridSystem<bool> _booleanGrid;
    [SerializeField] private int _width = 3;
    [SerializeField] private int _height = 3;
    [SerializeField] private float _cellSize = 1;
    [SerializeField] private Transform _origin;
    [SerializeField] private GameObject _positionMarkerPrefab;

    [Header("Live Testing Utils")]
    [SerializeField] private Transform _gridPositionTestingTranform;
    [SerializeField] private bool _isTestingTransformOnGrid = false;


    //Monobehaviors
    private void Awake()
    {
        CreateValidGrid();
        _booleanGrid.SetDebugDrawing(true);
        //LogAllCellPositionsInGrid(_booleanGrid);
        //SpawnMarkerAtAllCellPositions(_booleanGrid);
    }

    private void Update()
    {
        UpdateGridPositionTest();
        ChangeValueOfCellOnInput();
    }


    //Testing Utils
    private void AttemptInvalidGridCreation()
    {
        _booleanGrid = new GridSystem<bool>(-1, 0, -99, Vector3.zero, () => new bool());
        LogGrid(_booleanGrid);
    }

    private void CreateValidGrid()
    {
        _booleanGrid = new GridSystem<bool>(_width, _height, _cellSize, _origin.position, () => new bool()) ;
        LogGrid(_booleanGrid);
    }

    private void AttemptToChangeACellThatDoesntExistOnTheGrid<T>(GridSystem<T> grid, T newValue)
    {
        Debug.Log("Before: ");
        LogGrid(grid);

        grid.SetValueAtCell(grid.Height + 1, -1, newValue);

        Debug.Log("After: ");
        LogGrid(grid);
    }

    private void ChangeCellContents<T>(GridSystem<T> grid, int x, int y, T newValue)
    {
        Debug.Log("Before: ");
        LogGrid(grid);

        grid.SetValueAtCell(x, y, newValue);

        Debug.Log("After: ");
        LogGrid(grid);
    }

    private void SpawnMarkerAtAllCellPositions<T>(GridSystem<T> grid)
    {
        for (int i = 0; i < grid.Width; i++)
            for (int j = 0; j < grid.Height; j++)
                SpawnGridMarker(grid, i, j);
    }

    private void SpawnGridMarker<T>(GridSystem<T> grid, int x, int y)
    {
        GameObject newMarker =Instantiate(_positionMarkerPrefab, grid.GetPositionFromCell(x, y), Quaternion.identity);
        newMarker.transform.localScale = new Vector3(grid.CellSize, grid.CellSize, grid.CellSize);
    }

    private void AttemptToGetACellFromAPositionOffTheGrid<T>(GridSystem<T> grid)
    {
        grid.GetCellFromPosition(new Vector3(grid.Width * grid.CellSize + grid.CellSize, grid.Height * grid.CellSize + grid.CellSize, 0));
    }

    private void AttemptToGetAPositionFromANonexistentCell<T>(GridSystem<T> grid)
    {
        grid.GetPositionFromCell(grid.Width + 1, grid.Height + 1);
    }

    private void UpdateGridPositionTest()
    {
        _isTestingTransformOnGrid = _booleanGrid.IsPositionOnGrid(_gridPositionTestingTranform.position);
        Debug.Log($"OnGrid Status: {_isTestingTransformOnGrid}, " +
                  $"Cell Position: {_booleanGrid.GetCellFromPosition(_gridPositionTestingTranform.position)}" +
                  $"Cell Value: {_booleanGrid.GetValueAtCell(_booleanGrid.GetCellFromPosition(_gridPositionTestingTranform.position).x, _booleanGrid.GetCellFromPosition(_gridPositionTestingTranform.position).y)}");
    }

    //Logging Utils
    private void LogGridCell<T>(GridSystem<T> grid, int x, int y)
    {
        Debug.Log($"Value at Location ({x},{y}): {grid.GetValueAtCell(x, y)}");
    }

    private void LogGrid<T>(GridSystem<T> grid)
    {
        Debug.Log($"Width: {grid.Width}, Height: {grid.Height}, CellSize: {grid.CellSize}, Origin: {grid.Origin}");

        for (int i = 0; i < grid.Width; i++)
            for (int j = 0; j < grid.Height; j++)
                LogGridCell(grid, i, j);
    }

    private void LogCellPosition<T>(GridSystem<T> grid, int x, int y)
    {
        Debug.Log($"Cell ({x},{y}): {grid.GetPositionFromCell(x, y)}");
    }

    private void LogAllCellPositionsInGrid<T>(GridSystem<T> grid)
    {
        for (int i = 0; i < grid.Width; i++)
            for (int j = 0; j < grid.Height; j++)
                LogCellPosition(grid, i, j);
    }

    private void ChangeValueOfCellOnInput()
    {
        if (_isTestingTransformOnGrid && Input.GetKeyDown(KeyCode.Space))
        {
            (int, int) cellPositions = _booleanGrid.GetCellFromPosition(_gridPositionTestingTranform.position);
            _booleanGrid.SetValueAtCell(cellPositions.Item1, cellPositions.Item2, true);
        }
    }

}
