using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SullysToolkit.TableTop.RPG
{
    public class ExpPool : MonoBehaviour, IRPGInteractablePiece, IRPGExperienceProvider, IDisplayableRPGAttribute
    {
        //Declarations
        [Header("Settings")]
        [SerializeField] [Min(0)] private int _expValue = 5;
        [SerializeField] private bool _isPoolAvailable = false;
        [SerializeField] private GamePiece _gamePieceRef;
        [SerializeField] private bool _isDebugActive = false;
        private IUIDisplayController _displayControllerRef;
        

        //Events
        public delegate void InteractionEvent(GamePiece eventSource, GamePiece whatTriggeredThisEvent);
        public event InteractionEvent OnEventTriggered;

        //Monobehaviours
        private void Awake()
        {
            InitializeReferences();
        }


        //Internal Utils
        private void InitializeReferences()
        {
            _gamePieceRef = GetComponent<GamePiece>();
            _displayControllerRef = GetComponent<IUIDisplayController>();
        }

        private void GrantExpToPerformerAndTriggerEvent(ILevelableRPGPiece gamePiece)
        {
            if (gamePiece == null)
            {
                STKDebugLogger.LogStatement(_isDebugActive,$"Performer {gamePiece} of ExpPool Interaction is null. Ignoring interaction");
                return;
            }


            IRPGAttributes gPieceAtrtibutes = gamePiece.GetGamePiece().GetComponent<IRPGAttributes>();
            if (gPieceAtrtibutes == null)
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"Performer {gamePiece} of ExpPool Interaction has No Attributes. Ignoring interaction");
                return;
            }


            if (gPieceAtrtibutes.GetCurrentActionPoints() > 1)
            {
                //Deduct AP
                gPieceAtrtibutes.SetCurrentActionPoints(gPieceAtrtibutes.GetCurrentActionPoints() - 1);

                STKDebugLogger.LogStatement(_isDebugActive,$"Granting {_expValue}(XP) to {gamePiece} and Deducting Ap");
                _isPoolAvailable = false;
                gamePiece.GainExp(_expValue);
                OnEventTriggered?.Invoke(_gamePieceRef, gamePiece.GetGamePiece());
            }
            else
                STKDebugLogger.LogStatement(_isDebugActive, $"{gamePiece} has no Ap for this interaction. Ignoring Interaction");

        }


        //Getters, Setters, & Commands
        public GamePiece GetGamePiece()
        {
            return _gamePieceRef;
        }

        public void TriggerInteractionEvent(GamePiece performer)
        {
            ILevelableRPGPiece levelableGamePiece = performer?.GetComponent<ILevelableRPGPiece>();
            GrantExpToPerformerAndTriggerEvent(levelableGamePiece);
        }

        public int GetExpValue()
        {
            return _expValue;
        }

        public void SetExpValue(int value)
        {
            _expValue = Mathf.Max(0, value);
            UpdateAttributeInDisplay(_displayControllerRef);
        }

        public void ResetPool()
        {
            _isPoolAvailable = true;
        }

        public bool IsPoolAvailable()
        {
            return _isPoolAvailable;
        }

        public void UpdateAttributeInDisplay(IUIDisplayController displayController)
        {
            if (displayController != null)
                displayController.UpdateData();
        }
    }
}


