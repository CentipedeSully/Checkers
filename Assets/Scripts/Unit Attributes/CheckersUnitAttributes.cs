using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit;
using SullysToolkit.TableTop;

public class CheckersUnitAttributes : MonoBehaviour
{
    //Declarations
    [SerializeField] private bool _isKing;
    private List<(int, int)> _legalMoveDirections;



    //Monobehaviours
    private void Awake()
    {
        InitializeMoveDirections();
    }



    //Internal Utilites
    private void InitializeMoveDirections()
    {
        _legalMoveDirections = new List<(int, int)>();

        if (IsUnitDark())
            AddUpwardsBoardMovementToUnit();

        else if (IsUnitLight())
            AddDownwardsBoardMovementToUnit();
    }

    private void AddUpwardsBoardMovementToUnit()
    {
        //right upwards diagonal
        _legalMoveDirections.Add((1, 1));
        //left upwards diagonal
        _legalMoveDirections.Add((-1, 1));
    }
    
    private void AddDownwardsBoardMovementToUnit()
    {
        //right upwards diagonal
        _legalMoveDirections.Add((1, -1));
        //left upwards diagonal
        _legalMoveDirections.Add((-1, -1));
    }

    private bool IsUnitDark()
    {
        return CompareTag("Dark");
    }

    private bool IsUnitLight()
    {
        return CompareTag("Light");
    }



    //Getters, Setters, & Commands
    public bool IsKing()
    {
        return _isKing;
    }

    public List<(int,int)> GetLegalMoveDirectionsList()
    {
        return _legalMoveDirections;
    }

    public void KingMe()
    {
        if (_isKing == false)
        {
            if (IsUnitDark())
            {
                _isKing = true;
                AddDownwardsBoardMovementToUnit();
            }

            else if (IsUnitLight())
            {
                _isKing = true;
                AddUpwardsBoardMovementToUnit();
            }    
        }
    }


}
