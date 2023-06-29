using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SullysToolkit
{
    public interface IRegenerateable
    {
        void RegenerateAttributes();
    }

    public class TurnRolloverRegenerator : MonoBehaviour, ITurnListener
    {
        //Declarations
        [Header("Regeneration Timing Utils")]
        [SerializeField] private TurnSystem _turnSystem;
        [SerializeField] private TurnPhase _regenPhase;
        [SerializeField] private IRegenerateable[] _regenerateableReferences;
        [SerializeField] private bool _readyToPassTurn = false;
        [SerializeField] private bool _isReferencesInitialized = false;

        [Header("Debugging Utilities")]
        [SerializeField] private bool _isDebugActive = false;


        //Monos




        //Internals
        public void InitializeReferences()
        {
            if (_turnSystem != null)
            {
                _regenerateableReferences = GetComponents<IRegenerateable>();
                _turnSystem.AddTurnListener(this);
                _isReferencesInitialized = true;
            }
            else
                STKDebugLogger.LogWarning($"Turn Rollover REgenerator {gameObject.name} Attempted to init references without it's turnSystem Being setup");
        }



        // Getters, Setters, && Commands
        public bool IsDebugActive()
        {
            return _isDebugActive;
        }

        public void TriggerRegenerationInChildReferences()
        {
            foreach (IRegenerateable reference in _regenerateableReferences)
            {
                if (reference != null)
                    reference.RegenerateAttributes();
            }
                
        }

        public bool IsReferencesInitialized()
        {
            return _isReferencesInitialized;
        }

        public void SetTurnBroadcaster(TurnSystem turnSystem)
        {
            _turnSystem = turnSystem;
        }

        public int GetResponsePhase()
        {
            return (int)_regenPhase;
        }

        public bool IsTurnListenerReadyToPassPhase()
        {
            return _readyToPassTurn;
        }

        public void RespondToNotification(int turnNumber)
        {
            TriggerRegenerationInChildReferences();
            _readyToPassTurn = true;
        }

        public void ResetResponseFlag()
        {
            _readyToPassTurn = false;
        }

        public ITurnBroadcaster GetTurnBroadcaster()
        {
            return _turnSystem;
        }

        public string GetConcreteListenerNameForDebugging()
        {
            return this.ToString() + ", ID:" + GetInstanceID();
        }

        public void ResetUtilsOnTurnSystemInterruption()
        {
            ResetResponseFlag();
        }
    }
}

