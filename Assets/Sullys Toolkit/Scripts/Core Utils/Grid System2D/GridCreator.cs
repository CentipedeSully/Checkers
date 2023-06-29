using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit;


public class GridCreator : MonoBehaviour
{
    //Declarations
    [SerializeField] private GridSystem<bool> _grid;
    [SerializeField] private Vector2 _gridOffset;
    [SerializeField] private float _cellSize = 1;
    [SerializeField] private int _gridSize = 15;


    //Monos
    private void Awake()
    {
        _grid = new GridSystem<bool>(_gridSize, _gridSize, _cellSize, _gridOffset, () => { return false; });
        _grid.SetDebugDrawDuration(999);
        _grid.SetDebugDrawing(true);
    }



    //Utils
    public GridSystem<bool> GetGrid()
    {
        return _grid;
    }

    public float GetCellSize()
    {
        return _cellSize;
    }

    public Vector3 GetGridOffset()
    {
        return _gridOffset;
    }

    public int GetGridSize()
    {
        return _gridSize;
    }
}
