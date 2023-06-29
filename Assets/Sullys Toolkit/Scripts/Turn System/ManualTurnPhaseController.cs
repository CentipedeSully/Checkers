using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SullysToolkit
{
    public class ManualTurnPhaseController : MonoBehaviour, ITurnListener
    {
        //Declarations
        [Header("Turn System Settings")]
        [SerializeField] private TurnSystem _turnSystem;
        [SerializeField] private TurnPhase _turnPhase;
        [SerializeField] private bool _passPhaseCommand = false;
        [SerializeField] private bool _isMyPhaseNow = false;



        //Monobehaviours




        //Internal Utils
        private void ResetUtilsOnTurnSystemEnd()
        {
            _passPhaseCommand = false;
            _isMyPhaseNow = false;

        }


        //Getters, Setters, & Commands
        public string GetConcreteListenerNameForDebugging()
        {
            return this.ToString() + ", ID:" + GetInstanceID();
        }

        public int GetResponsePhase()
        {
            return (int)_turnPhase;
        }

        public ITurnBroadcaster GetTurnBroadcaster()
        {
            return _turnSystem;
        }

        public bool IsTurnListenerReadyToPassPhase()
        {
            return _passPhaseCommand && _isMyPhaseNow;
        }

        public void ResetResponseFlag()
        {
            _isMyPhaseNow = false;
            _passPhaseCommand = false;
        }

        public void RespondToNotification(int turnNumber)
        {
            _isMyPhaseNow = true;
        }

        public void SetPassPhaseCommand(bool value)
        {
            if (_turnSystem.IsTurnSystemActive())
                _passPhaseCommand = value;
        }

        public bool GetPassPhaseCommand()
        {
            return _passPhaseCommand;
        }

        public bool IsMyPhaseNow()
        {
            return _isMyPhaseNow;
        }

        public void AddThisControllerAsTurnPhaseListener()
        {
            if (_turnSystem.IsTurnSystemActive() == false)
                _turnSystem.AddTurnListener(this);
        }

        public TurnPhase GetTurnPhase()
        {
            return _turnPhase;
        }

        public void SetTurnSystem(TurnSystem newTurnSystem)
        {
            _turnSystem = newTurnSystem;
            AddThisControllerAsTurnPhaseListener();
        }

        public void ResetUtilsOnTurnSystemInterruption()
        {
            ResetUtilsOnTurnSystemEnd();
        }
    }
}

