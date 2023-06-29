using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit.TableTop;

public class CheckersBoardInitializer : MonoBehaviour
{
    //Declarations
    [SerializeField] private int _columns = 8;
    [SerializeField] private int _rows = 8;
    [SerializeField] private GamePiece _lightTerrainPiece;
    [SerializeField] private GamePiece _darkTerrainPiece;
    [SerializeField] private GamePiece _lightPlayPiece;
    [SerializeField] private GamePiece _darkPlayPiece;


    //references
    private GameBoard _gameBoardRef;



    //Monobehaviours
    private void Awake()
    {
        InitializeReferences();
    }

    private void Start()
    {
        SetupCheckersGame();
    }



    //Internal Utils
    private void InitializeReferences()
    {
        _gameBoardRef = GetComponent<GameBoard>();
    }

    private void SetupCheckersGame()
    {
        InitalizeBoard();
        AddTerrainPieces();
        AddPlayPieces();
    }

    private void InitalizeBoard()
    {
        _gameBoardRef.InitializeGameBoard(_rows, _columns);
    }

    private void AddTerrainPieces()
    {
        //Light on the right
        for (int c = 0; c < _columns; c++)
        {
            for (int r = 0; r < _rows; r++)
            {
                //Even rows start dark
                //Odd rows start light

                
            }
        }
    }

    private void AddPlayPieces()
    {

    }

    private GameObject CreateDarkTerrainPiece()
    {
        //return Instantiate(_darkTerrainPiece,)
        return null;
    }


    //Getters, Setters, & Commands





}
