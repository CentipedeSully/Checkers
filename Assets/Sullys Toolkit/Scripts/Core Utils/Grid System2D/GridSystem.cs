using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace SullysToolkit
{
    public class GridSystem<T>
    {
        //Delcarations
        private T[,] _gridData;

        public int Width { get; }

        public int Height { get; }

        public float CellSize { get; }

        public Vector3 Origin { get; }

        private bool _isDebugDrawingActive = false;

        private float _debugDrawDuration = 60;

        
        //Constructor
        public GridSystem(int x, int y, float cellSize, Vector3 origin, Func<T> createGridObject)
        {
            this.Width = Mathf.Max(1,x);
            this.Height = Mathf.Max(1, y); ;
            this.CellSize = Mathf.Max(.1f, cellSize);
            this.Origin = origin;

            _gridData = new T[Width, Height];
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                    _gridData[i, j] = createGridObject();
            }
        }


        //Utils
        public bool IsCellInGrid(int x, int y)
        {
            if ((x >= 0 && x < Width) && (y >= 0 && y < Height))
                return true;
            else return false;
        }

        public T GetValueAtCell(int x, int y)
        {
            if (IsCellInGrid(x, y))
                return _gridData[x, y];
            else
            {
                Debug.LogWarning($"{this} Error: CellOutOfBounds. Cant get position of cell. returning default value");
                return default;
            }
        }

        public void SetValueAtCell(int x, int y, T value)
        {
            if (IsCellInGrid(x, y))
                _gridData[x, y] = value;
        }

        public Vector3 GetPositionFromCell(int x, int y)
        {
            if (IsCellInGrid(x,y))
                return new Vector3( (x * CellSize) + (CellSize/2) + Origin.x, (y * CellSize) + (CellSize/2) + Origin.y, 0) ;

            else
            {
                Debug.LogWarning($"{this} Error: CellOutOfBounds. Cant get position of cell. returning zero");
                return Vector3.zero;
            }
        }

        public bool IsPositionOnGrid(Vector3 position)
        {
            //establish grid bounds
            float minX = 0 + Origin.x;
            float maxX = (Width * CellSize) + Origin.x;
            float minY = 0 + Origin.y;
            float maxY = (Height * CellSize) + Origin.y;

            //return if position beyond bounds
            if (position.x > maxX || position.x < minX || position.y > maxY || position.y < minY)
                return false;

            else return true;
        }

        public (int x, int y) GetCellFromPosition(Vector3 position)
        {
            if ( IsPositionOnGrid(position))
            {
                int xCellPositon = Mathf.FloorToInt((position.x / CellSize) - Origin.x);
                int yCellPosition = Mathf.FloorToInt((position.y / CellSize) - Origin.y);
                return (xCellPositon, yCellPosition);
            }

            else
            {
                Debug.LogWarning($"{this} Warning: Position {position} not on Grid. Returning (-1,-1) cell position");
                return (-1, -1);
            }
        }

        public bool IsDebugDrawingActive()
        {
            return _isDebugDrawingActive;
        }

        public void SetDebugDrawing(bool newValue)
        {
            _isDebugDrawingActive = newValue;

            if (_isDebugDrawingActive)
                DebugDrawGrid();
        }

        public void SetDebugDrawDuration(float newValue)
        {
            if (newValue > 0)
                _debugDrawDuration = newValue;
        }

        public float GetDebugDrawDuration()
        {
            return _debugDrawDuration;
        }

        private void DebugDrawGrid()
        {
            //Draw Southern line
            Debug.DrawLine(Origin, new Vector3(Origin.x + (Width * CellSize), Origin.y),Color.magenta, _debugDrawDuration);

            //Draw Western line
            Debug.DrawLine(Origin, new Vector3(Origin.x, Origin.y + (Height * CellSize)), Color.magenta, _debugDrawDuration);

            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    //Draw northern line
                    Debug.DrawLine(new Vector3(Origin.x + (i * CellSize),Origin.y + (j * CellSize) + CellSize), 
                                   new Vector3(Origin.x + (i * CellSize) + CellSize, Origin.y + (j * CellSize) + CellSize),
                                   Color.magenta, _debugDrawDuration);

                    //Draw Eastern line
                    Debug.DrawLine(new Vector3(Origin.x + (i * CellSize) + CellSize, Origin.y + (j * CellSize)), 
                                   new Vector3(Origin.x + (i * CellSize) + CellSize, Origin.y + (j * CellSize) + CellSize), 
                                   Color.magenta, _debugDrawDuration);
                }
        }
    }





}

