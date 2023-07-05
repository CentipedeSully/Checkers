using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit.TableTop;
using SullysToolkit;
using System.Linq;

public class CheckersBoardInitializer : MonoBehaviour
{
    //Declarations
    [Header("Board Settings")]
    [SerializeField] private int _columns = 8;
    [SerializeField] private int _rows = 8;

    [Header("Prefab Setup")]
    [SerializeField] private GamePiece _lightTerrainPiece;
    [SerializeField] private GamePiece _darkTerrainPiece;
    [SerializeField] private GamePiece _lightPlayPiece;
    [SerializeField] private GamePiece _darkPlayPiece;

    [Header("Storage Containers")]
    [SerializeField] private GameObject _terrainPieceContainer;
    [SerializeField] private GameObject _darkPieceContainer;
    [SerializeField] private GameObject _lightPieceContainer;
    [SerializeField] private GameObject _darkOutOfPlayLocation;
    [SerializeField] private GameObject _lightOutOfPlayLocation;
    


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
                //Starting from the bottomLeft Cell
                //Even rows start dark
                if (r % 2 == 0)
                {
                    if (c % 2 == 0)
                        _gameBoardRef.AddGamePiece(CreateDarkTerrainPiece(), GameBoardLayer.Terrain, (r, c));
                    else
                        _gameBoardRef.AddGamePiece(CreateLightTerrainPiece(), GameBoardLayer.Terrain, (r, c));
                }

                //Odd rows start light
                else
                {
                    if (c % 2 == 1)
                        _gameBoardRef.AddGamePiece(CreateDarkTerrainPiece(), GameBoardLayer.Terrain, (r, c));
                    else
                        _gameBoardRef.AddGamePiece(CreateLightTerrainPiece(), GameBoardLayer.Terrain, (r, c));
                }
                
            }
        }
    }

    private void AddPlayPieces()
    {
        for (int c = 0; c < _columns; c++)
        {
            for (int r = 0; r < _rows; r++)
            {
                //only place units on dark cells
                //first 3 rows are dark units
                // last 3 rows are light units

                if (IsTerrainDark(r, c))
                {
                    if (c < 3)
                        _gameBoardRef.AddGamePiece(CreateDarkPlayPiece(), GameBoardLayer.Units, (r, c));
                    else if (c > 4)
                        _gameBoardRef.AddGamePiece(CreateLightPlayPiece(), GameBoardLayer.Units, (r, c));
                }
            }
        }
    }

    private GamePiece CreateDarkTerrainPiece()
    {
        return Instantiate(_darkTerrainPiece.gameObject, transform.position, Quaternion.identity, _terrainPieceContainer.transform).GetComponent<GamePiece>();
    }

    private GamePiece CreateLightTerrainPiece()
    {
        return Instantiate(_lightTerrainPiece.gameObject, transform.position, Quaternion.identity, _terrainPieceContainer.transform).GetComponent<GamePiece>();
    }

    private GamePiece CreateDarkPlayPiece()
    {
        GamePiece darkPiece = Instantiate(_darkPlayPiece.gameObject, transform.position, Quaternion.identity, _darkPieceContainer.transform).GetComponent<GamePiece>();
        darkPiece.SetOutOfPlayHoldingLocation(_darkOutOfPlayLocation.transform);
        return darkPiece;
    }

    private GamePiece CreateLightPlayPiece()
    {
        GamePiece lightPiece = Instantiate(_lightPlayPiece.gameObject, transform.position, Quaternion.identity, _lightPieceContainer.transform).GetComponent<GamePiece>();
        lightPiece.SetOutOfPlayHoldingLocation(_lightOutOfPlayLocation.transform);
        return lightPiece;
    }

    private bool IsTerrainDark(int x, int y)
    {
        (int, int) xyPosition = (x, y);
        IEnumerable<GamePiece> terrainQuery =
            from terrain in _gameBoardRef.GetPiecesOnPosition(xyPosition)
            where terrain.GetGamePieceType() == GamePieceType.Terrain
            select terrain;

        if (terrainQuery.Any() == false)
        {
            STKDebugLogger.LogError($"No terrain detected at location {x},{y}. " +
                $"Can't respond accurately to terrain color if terrain doesn't exist.");
            return false;
        }
        else
            return terrainQuery.First().CompareTag("Dark");
    }

    //Getters, Setters, & Commands





}
