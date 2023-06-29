using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit.TableTop.RPG
{
    public class GamePieceDataDisplayController : MonoBehaviour, IUIDisplayController
    {
        //Declarations
        [Header("UI Display Settings")]
        [SerializeField] private GamePieceType _displayType;
        [SerializeField] private GameObject _displayObject;
        [SerializeField] private bool _hideDisplayOnStart = true;
        [SerializeField] private bool _isDebugActive = false;
        [SerializeField] private bool _isDisplayReady = false;


        [Header("Debugging Utilities")]
        [SerializeField] private bool _updateDataCmd = false;
        [SerializeField] private IRPGAttributes _attributeRef;
        [SerializeField] private IRPGIdentityDefinition _identityRef;
        [SerializeField] private IHealthManager _healthRef;
        [SerializeField] private GamePiece _gamePieceRef;
        [SerializeField] private IMoveableRPGPiece _movementRef;
        [SerializeField] private IRPGExperienceProvider _expProviderRef;

        //Monobehaviours
        private void Awake()
        {
            InitializeReferences();
        }

        private void Start()
        {
            SetupDisplayOnStart();
        }

        private void Update()
        {
            ListenForDebugCommands();
        }



        //Internal Utils
        private void InitializeReferences()
        {
            _gamePieceRef = GetComponent<GamePiece>();
            _attributeRef = _gamePieceRef?.GetComponent<IRPGAttributes>();
            _identityRef = _gamePieceRef?.GetComponent<IRPGIdentityDefinition>();
            _healthRef = _gamePieceRef?.GetComponent<IHealthManager>();
            _movementRef = _gamePieceRef?.GetComponent<IMoveableRPGPiece>();
            _expProviderRef = _gamePieceRef?.GetComponent<IRPGExperienceProvider>();
        }

        private void ListenForDebugCommands()
        {
            if (_isDebugActive)
            {
                if (_updateDataCmd)
                {
                    _updateDataCmd = false;
                    UpdateData();
                }
            }
        }


        //Getters, Setters, & Commands
        public GamePieceType GetDisplayType()
        {
            return _displayType;
        }

        public GameObject GetDisplayObject()
        {
            return _displayObject;
        }

        public void SetDisplayObject(GameObject newDisplay)
        {
            if (newDisplay != null)
            { 
                _displayObject = newDisplay;
                UpdateData();
            }
            else 
                STKDebugLogger.LogWarning($"Attempted to assign a null Display Object onto {gameObject.name}, ID:{GetInstanceID()}");
        }

        public void DisplayData()
        {
            if (_displayObject != null)
            {
                _displayObject.SetActive(true);
                STKDebugLogger.LogStatement(_isDebugActive, $"Displaying Data for {gameObject.name}, ID:{GetInstanceID()}");
            }
            else 
                STKDebugLogger.LogWarning($"No Display Object exists to Display for {gameObject.name}, ID:{GetInstanceID()}");
        }

        public void HideData()
        {
            if (_displayObject != null)
            {
                _displayObject.SetActive(false);
                STKDebugLogger.LogStatement(_isDebugActive, $"Hiding Data for {gameObject.name}, ID:{GetInstanceID()}");
            }
            else 
                STKDebugLogger.LogWarning($"No Display Object exists to Hide for {gameObject.name}, ID:{GetInstanceID()}");
        }

        public bool IsDataOnDisplay()
        {
            return _displayObject.activeSelf;
        }

        public void SetupDisplayOnStart()
        {
            if (_displayObject == null)
            {
                switch (_displayType)
                {
                    case GamePieceType.Unit:
                        _displayObject = GamePieceDisplayerRPG.Instance.GetUnitDisplay();
                        break;
                    case GamePieceType.PointOfInterest:
                        _displayObject = GamePieceDisplayerRPG.Instance.GetPOIDisplay();
                        break;
                    case GamePieceType.Terrain:
                        _displayObject = GamePieceDisplayerRPG.Instance.GetTerrainDisplay();
                        break;
                }
            }

            _isDisplayReady = true;
            UpdateData();

            if (_hideDisplayOnStart == true)
                HideData();
            else DisplayData();
        }

        public void UpdateData()
        {
            if (_isDisplayReady)
            {
                if (_displayType == GamePieceType.Unit)
                    GamePieceDisplayerRPG.Instance.UpdateDisplayData(_identityRef.GetName(), _healthRef.GetCurrentHealth(), _attributeRef.GetAtkModifier(),
                        _attributeRef.GetDef(), _attributeRef.GetDamageDie(), _attributeRef.GetDamageModifier(), _attributeRef.GetCurrentActionPoints(),
                        _movementRef.GetCurrentMovePoints());

                else if (_displayType == GamePieceType.PointOfInterest)
                    GamePieceDisplayerRPG.Instance.UpdateDisplayData(_identityRef.GetName(), _identityRef.GetDescription(), _expProviderRef.GetExpValue());

                else if (_displayType == GamePieceType.Terrain)
                    GamePieceDisplayerRPG.Instance.UpdateDisplayData(_identityRef.GetName(), _identityRef.GetDescription());
            }
        }
    }
}

