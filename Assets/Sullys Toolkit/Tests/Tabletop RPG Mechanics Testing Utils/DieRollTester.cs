using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit.TableTop;

public class DieRollTester : MonoBehaviour
{
    //Declarations
    [Header("Test Parameters")]
    [SerializeField] private int _dieSize = 4;

    [Header("Commands")]
    [SerializeField] private bool _clearResults;
    [SerializeField] private bool _performTest;

    [Header("Test Results")]
    [SerializeField] private bool _errorFlag;
    [SerializeField] private int _latestErrorIndex = -1;
    [SerializeField] private int[] _resultsArray;



    //Monobehavours
    void Start()
    {
        _resultsArray = new int[1000];
    }

    private void Update()
    {
        ListenForCommands();
    }



    //Internal Utils
    private void ClearTestResults()
    {
        _resultsArray = new int[1000];
        _latestErrorIndex = -1;
        _errorFlag = false;
    }

    private void RollaThousandDice()
    {
        ClearTestResults();

        for (int i = 0; i < 1000; i++)
        {
            _resultsArray[i] = DieRoller.RollDie(_dieSize);
            if (_resultsArray[i] < 1 || _resultsArray[i] > _dieSize)
            {
                _errorFlag = true;
                _latestErrorIndex = i;
            }
        }
            
    }

    private void ListenForCommands()
    {
        if (_clearResults)
        {
            _clearResults = false;
            ClearTestResults();
        }

        if (_performTest)
        {
            _performTest = false;
            RollaThousandDice();
        }
    }

    public bool IsErrorFlagRaised()
    {
        return _errorFlag;
    }
}
