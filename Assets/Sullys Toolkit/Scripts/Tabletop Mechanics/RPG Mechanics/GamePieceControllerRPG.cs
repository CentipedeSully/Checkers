using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SullysToolkit.TableTop.RPG
{
    public class GamePieceControllerRPG : MonoBehaviour, ITurnListener, ISelectionCache<GameObject>
    {
        //Declarations
        [Header("Controller Settings")]
        [SerializeField] private TurnPhase _controlsUnlockedPhase;
        [SerializeField] private bool _isControlAvailable = false;

        [Header("Selection Settings")]
        [SerializeField] private bool _isSelectorReady = true;
        [SerializeField] private float _selectionCooldown = .35f;
        [Tooltip("The first element takes priority over what will get shown. " +
            "If the gamepiece of that type doesn't exist, then the gamepiece of the next proiority will be displayed instead.")]
        [SerializeField] private List<GamePieceType> _displayPriorityList;
        [SerializeField] [Min(-1)] private int _selectedPositionX = -1;
        [SerializeField] [Min(-1)] private int _selectedPositionY = -1;
        [SerializeField] private GamePiece _unitOnSelectedPosition;
        [SerializeField] private GamePiece _poiOnSelectedPosition;
        [SerializeField] private GamePiece _terrainOnSelectedPosition;




        [Header("References")]
        [SerializeField] private TurnSystem _turnSystem;
        [SerializeField] private GameBoard _gameBoard;
        [SerializeField] private MouseToWorld2D _mouseToWorldTrackerRef;

        [Header("Debugging Utils")]
        [SerializeField] private bool _isDebugActive = false;


        //Events
        public delegate void SelectionEventWithContext(GamePiece selectedPiece);
        public delegate void SelectionEvent();
        public event SelectionEventWithContext OnSelectionCaptured;
        public event SelectionEvent OnSelectionCleared;
        public event SelectionEvent OnActionPerformed;



        //Monobehaviours
        private void Awake()
        {
            
        }

        private void Update()
        {
            ListenForSelectionCommands();
        }


        //Internal Utils
        private void ListenForSelectionCommands()
        {
            //Left Click -> 0
            //Right Click -> 1
            //Middle Click -> 2

            if (Input.GetMouseButtonDown(0) && _isSelectorReady)
            {
                CooldownSelection();
                STKDebugLogger.LogStatement(_isDebugActive, $"Left-click detected. Determining contextual selection request...");
                (int, int) xySelectedPosition = _gameBoard.GetGrid().GetCellFromPosition(_mouseToWorldTrackerRef.GetWorldPosition());

                if (xySelectedPosition == (-1,-1))
                {
                    STKDebugLogger.LogStatement(_isDebugActive, $"Clearing selection data due to clicking off of game grid");
                    ClearSelection();
                    OnSelectionCleared?.Invoke();
                }

                else
                {
                    STKDebugLogger.LogStatement(_isDebugActive, $"Capturing new Selection data...");
                    ClearSelection();

                    CaptureSelectionViaMousePosition();
                    DisplaySelectedGamePieceOfHighestPriority();
                    OnSelectionCaptured?.Invoke(GetHighestPrioritySelection());
                }
            }

            else if (Input.GetMouseButtonDown(1) && _isSelectorReady && IsSelectionAvailable() && _isControlAvailable)
            {
                CooldownSelection();
                STKDebugLogger.LogStatement(_isDebugActive, $"Right-click detected. Determining contextual action request...");
                (int, int) xySelectedPosition = _gameBoard.GetGrid().GetCellFromPosition(_mouseToWorldTrackerRef.GetWorldPosition());

                if (xySelectedPosition == (-1, -1))
                    STKDebugLogger.LogStatement(_isDebugActive, $"Ignoring contextual action request due to target being off game grid");

                else
                {
                    STKDebugLogger.LogStatement(_isDebugActive, $"Right Click on good position. Unimplemented, but good ^_^");
                    OnActionPerformed?.Invoke();
                }
            }
        }



        private void CaptureSelectionViaMousePosition()
        {
            (int, int) xySelection = GetCellPositionFromMouse();
            _selectedPositionX = xySelection.Item1;
            _selectedPositionY = xySelection.Item2;

            _unitOnSelectedPosition = GetUnitOnPosition(_selectedPositionX, _selectedPositionY);
            _poiOnSelectedPosition = GetPoiOnPosition(_selectedPositionX, _selectedPositionY);
            _terrainOnSelectedPosition = GetTerrainOnPosition(_selectedPositionX, _selectedPositionY);
        }

        private void DisplaySelectedGamePieceOfHighestPriority()
        {
            STKDebugLogger.LogStatement(_isDebugActive, $"Selecting a gamePiece to display based on the current selection data & priority list...");
            for (int i = 0; i < _displayPriorityList.Count; i++)
            {

                if (_displayPriorityList[i] == GamePieceType.Unit && _unitOnSelectedPosition != null)
                {
                    DisplayGamePieceData(_unitOnSelectedPosition);
                    return;
                }

                else if (_displayPriorityList[i] == GamePieceType.PointOfInterest && _poiOnSelectedPosition != null)
                {
                    DisplayGamePieceData(_poiOnSelectedPosition);
                    return;
                }

                else if (_displayPriorityList[i] == GamePieceType.Terrain && _terrainOnSelectedPosition != null)
                {
                    DisplayGamePieceData(_terrainOnSelectedPosition);
                    return;
                }
            }

            STKDebugLogger.LogStatement(_isDebugActive, $"No selection exists to display");
        }

        private GamePiece GetHighestPrioritySelection()
        {
            STKDebugLogger.LogStatement(_isDebugActive, $"Determining the non-null selection of highest priority...");
            for (int i = 0; i < _displayPriorityList.Count; i++)
            {

                if (_displayPriorityList[i] == GamePieceType.Unit && _unitOnSelectedPosition != null)
                    return _unitOnSelectedPosition;

                else if (_displayPriorityList[i] == GamePieceType.PointOfInterest && _poiOnSelectedPosition != null)
                    return _poiOnSelectedPosition;

                else if (_displayPriorityList[i] == GamePieceType.Terrain && _terrainOnSelectedPosition != null)
                    return _terrainOnSelectedPosition;
            }

            STKDebugLogger.LogStatement(_isDebugActive, $"No Selection exists. Returning null");
            return null;
        }

        private (int,int) GetCellPositionFromMouse()
        {
            STKDebugLogger.LogStatement(_isDebugActive, "Capturing Grid Selection via Mouse Position...");
            Vector3 mousePosition = _mouseToWorldTrackerRef.GetWorldPosition();
            if (_gameBoard.GetGrid().IsPositionOnGrid(mousePosition))
            {
                (int, int) xySelectedPosition = _gameBoard.GetGrid().GetCellFromPosition(mousePosition);

                STKDebugLogger.LogStatement(_isDebugActive, $"Selected Position ({xySelectedPosition.Item1},{xySelectedPosition.Item2})");
                return xySelectedPosition;
            }

            else
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"No Position found. Mouse isn't on Game Grid. Returning (-1,-1)");
                return (-1, -1);
            }
        }

        private GamePiece GetUnitOnPosition(int x, int y)
        {
            STKDebugLogger.LogStatement(_isDebugActive, $"Getting unit on position ({x},{y})...");
            List<GamePiece> allPiecesOnPosition = _gameBoard.GetPiecesOnPosition((x,y));

            IEnumerable<GamePiece> unitQuery =
                from piece in allPiecesOnPosition
                where piece.GetGamePieceType() == GamePieceType.Unit
                select piece;

            if (unitQuery.Any())
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"Unit Detected: {unitQuery.First().name}, ID: {unitQuery.First().GetInstanceID()}");
                return unitQuery.First();
            }
                
            else
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"No unit Detected");
                return null;
            }
        }

        private GamePiece GetPoiOnPosition(int x, int y)
        {
            STKDebugLogger.LogStatement(_isDebugActive, $"Getting PoI on position ({x},{y})...");
            List<GamePiece> allPiecesOnPosition = _gameBoard.GetPiecesOnPosition((x, y));

            IEnumerable<GamePiece> poiQuery =
                from piece in allPiecesOnPosition
                where piece.GetGamePieceType() == GamePieceType.PointOfInterest
                select piece;

            if (poiQuery.Any())
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"PoI Detected: {poiQuery.First().name}, ID: {poiQuery.First().GetInstanceID()}");
                return poiQuery.First();
            }

            else
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"No PoI Detected");
                return null;
            }
        }

        private GamePiece GetTerrainOnPosition(int x, int y)
        {
            STKDebugLogger.LogStatement(_isDebugActive, $"Getting Terrain on position ({x},{y})...");
            List<GamePiece> allPiecesOnPosition = _gameBoard.GetPiecesOnPosition((x, y));

            IEnumerable<GamePiece> terrainQuery =
                from piece in allPiecesOnPosition
                where piece.GetGamePieceType() == GamePieceType.Terrain
                select piece;

            if (terrainQuery.Any())
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"Terrain Detected: {terrainQuery.First().name}, ID: {terrainQuery.First().GetInstanceID()}");
                return terrainQuery.First();
            }

            else
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"No Terrain Detected");
                return null;
            }
        }

        private void MoveGamePieceInDirection(GamePiece movingPiece,(int,int) xyDirection)
        {
            STKDebugLogger.LogStatement(_isDebugActive, $"Attempting to move gamePiece ({movingPiece?.name},ID: {movingPiece?.GetInstanceID()})");
            IMoveableRPGPiece validatedMoveablePiece = movingPiece?.GetComponent<IMoveableRPGPiece>();
            if (validatedMoveablePiece != null)
                validatedMoveablePiece.MoveToNeighborCell(xyDirection);
            else
                STKDebugLogger.LogStatement(_isDebugActive,$"GamePiece Controller attempted to move a null Piece. Ignoring Command");
        }

        private void PerformInteractionBtwnGamePieces(GamePiece actorPiece, GamePiece subjectPiece)
        {
            STKDebugLogger.LogStatement(_isDebugActive, $"Attempting to perform an interaction btwn gamePieces: \n" +
                $"Actor ({actorPiece?.name}, ID: {actorPiece?.GetInstanceID()})\n" +
                $"Subject ({subjectPiece?.name}, ID: {subjectPiece?.GetInstanceID()})");

            IRPGInteractablePiece validatedInteractablePiece = subjectPiece?.GetComponent<IRPGInteractablePiece>();

            if (validatedInteractablePiece != null)
                validatedInteractablePiece.TriggerInteractionEvent(actorPiece);

            else
                STKDebugLogger.LogStatement(_isDebugActive, $"GamePiece Controller attempted to perform an interact onto a null piece. Ignoring Command");
        }

        private void CooldownSelection()
        {
            _isSelectorReady = false;
            Invoke("ReadySelector", _selectionCooldown);
        }

        private void ReadySelector()
        {
            _isSelectorReady = true;
        }

        private void DisplayGamePieceData(GamePiece piece)
        {
            IUIDisplayController displayController = piece?.GetComponent<IUIDisplayController>();

            if (displayController != null)
            {
                if (displayController.IsDataOnDisplay() == false)
                    displayController.DisplayData();

                displayController.UpdateData();
            }
                
        }

        private void HideGamePieceData(GamePiece piece)
        {
            IUIDisplayController displayController = piece?.GetComponent<IUIDisplayController>();

            if (displayController != null)
            {
                if (displayController.IsDataOnDisplay())
                    displayController.HideData();

                displayController.UpdateData();
            }
                
        }





        //Getters, Setters, & Commands
        public bool IsDebugActive()
        {
            return _isDebugActive;
        }

        public bool IsControlAvailable()
        {
            return _isControlAvailable;
        }

        public int GetResponsePhase()
        {
            return (int)_controlsUnlockedPhase;
        }

        public bool IsTurnListenerReadyToPassPhase()
        {
            return true;
        }

        public void RespondToNotification(int turnNumber)
        {
            _isControlAvailable = true;
        }

        public void ResetResponseFlag()
        {
            _isControlAvailable = false;
        }

        public ITurnBroadcaster GetTurnBroadcaster()
        {
            return _turnSystem;
        }

        public string GetConcreteListenerNameForDebugging()
        {
            return ToString() + ", ID:" + GetInstanceID();
        }

        public void ResetUtilsOnTurnSystemInterruption()
        {
            _isControlAvailable = false;
        }

        public void ClearSelection()
        {
            if (_unitOnSelectedPosition != null)
            {
                HideGamePieceData(_unitOnSelectedPosition);
                _unitOnSelectedPosition = null;
            }

            if (_poiOnSelectedPosition != null)
            {
                HideGamePieceData(_poiOnSelectedPosition);
                _poiOnSelectedPosition = null;
            }

            if (_terrainOnSelectedPosition != null)
            {
                HideGamePieceData(_terrainOnSelectedPosition);
                _terrainOnSelectedPosition = null;
            }
        }


        public GameObject GetSelection()
        {
            return GetHighestPrioritySelection().gameObject;
        }

        public void SetSelection(GameObject newSelection)
        {
            STKDebugLogger.LogWarning("Only selection via mouse are supported, currently");
        }

        public List<GameObject> GetSelectionCollection()
        {
            List<GameObject> returnList = new List<GameObject>();

            if (_unitOnSelectedPosition != null)
                returnList.Add(_unitOnSelectedPosition.gameObject);

            if (_poiOnSelectedPosition != null)
                returnList.Add(_poiOnSelectedPosition.gameObject);

            if (_terrainOnSelectedPosition != null)
                returnList.Add(_terrainOnSelectedPosition.gameObject);

            return returnList;
           
        }

        public void AddSelection(GameObject newSelection)
        {
            SetSelection(newSelection);
        }

        public void RemoveSelection(GameObject existingSelection)
        {
            ClearSelection();
        }

        public bool IsSelectionAvailable()
        {
            bool unitExists = _unitOnSelectedPosition != null;
            bool poiExists = _poiOnSelectedPosition != null;
            bool terrainExists = _terrainOnSelectedPosition != null;

            return unitExists || poiExists || terrainExists;
        }
    }
}

