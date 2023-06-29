using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace SullysToolkit
{
    public enum TurnPhase
    {
        RefreshPhase,
        MainPhase,
        ReactionPhase,
        TurnOverPhase
    }

    public interface ITurnListener
    {
        int GetResponsePhase();

        bool IsTurnListenerReadyToPassPhase();

        void RespondToNotification(int turnNumber);

        void ResetResponseFlag();

        ITurnBroadcaster GetTurnBroadcaster();

        string GetConcreteListenerNameForDebugging();

        void ResetUtilsOnTurnSystemInterruption();


    }

    public interface ITurnBroadcaster
    {
        int GetCurrentTurn();

        int GetCurrentPhase();

        int GetMaxTurnLimit();

        void NotifyTurnListenersOfPhaseChange();

        void AddTurnListener(ITurnListener listener);

        void RemoveTurnListener(ITurnListener listener);

        void StartTurnSystem();

        void StopTurnSystem();

        bool IsTurnSystemActive();
    }

    public class TurnSystem : MonoBehaviour, ITurnBroadcaster 
    {
        //Declarations
        [Header("Turn System Settings")]
        [SerializeField] private bool _isTurnSystemActive = false;
        [SerializeField] private int _currentTurnCount = 0;
        [SerializeField] private int _maxTurnCount = 0;
        [SerializeField] private TurnPhase _currentPhase = 0;
        private List<List<ITurnListener>> _listenersList;
        private List<ITurnListener> _currentPhaseListenerAdditionsBuffer;
        private List<ITurnListener> _currentPhaseListenerRemovalsBuffer;
        private IEnumerator _turnManager;

        [Header("Debugging Utilities")]
        [SerializeField] List<int> _listenersPerPhaseList;
        [SerializeField] private bool _isDebugActive;


        //Events
        public delegate void TurnSystemEvent();
        public event TurnSystemEvent OnTurnSystemInterrupted;
        public event TurnSystemEvent OnMaxTurnCountReached;


        //Monobehaviours
        private void Awake()
        {
            InitializeListenerLists();
        }

        private void OnDestroy()
        {
            FreeSubscriptionsFromMemory();
        }



        //Internal utils
        private void SubscribeListenerToInterruptionEvent(ITurnListener validListener)
        {
            OnTurnSystemInterrupted += validListener.ResetUtilsOnTurnSystemInterruption;
        }

        private void UnsubscribeListenerFromInterruptionEvent(ITurnListener validListener)
        {
            OnTurnSystemInterrupted -= validListener.ResetUtilsOnTurnSystemInterruption;
        }

        private void FreeSubscriptionsFromMemory()
        {
            if (_listenersList != null)
            {
                foreach (List<ITurnListener> phaseList in _listenersList)
                {
                    foreach (ITurnListener listener in phaseList)
                        UnsubscribeListenerFromInterruptionEvent(listener);
                }
            }
        }


        //Interface Utils
        public int GetCurrentPhase()
        {
            return (int)_currentPhase;
        }

        public int GetCurrentTurn()
        {
            return _currentTurnCount;
        }

        public int GetMaxTurnLimit()
        {
            return _maxTurnCount;
        }

        public int GetListenerCount(int phase)
        {
            if (phase < System.Enum.GetNames(typeof(TurnPhase)).Length)
                return _listenersList[phase].Count;
            else return 0;
        }

        public List<ITurnListener> GetListenersInPhase(int phase)
        {
            if (phase < System.Enum.GetNames(typeof(TurnPhase)).Length)
                return _listenersList[phase];
            else return new List<ITurnListener>();
        }

        public void NotifyTurnListenersOfPhaseChange()
        {
            //Make all listeners of the curent phase respond to this current phase
            foreach (ITurnListener listener in _listenersList[(int)_currentPhase])
                listener.RespondToNotification(_currentTurnCount);
        }

        public void AddTurnListener(ITurnListener listener)
        {
            //check if the response phase of the listener is a valid phase for the turn system
            if (listener.GetResponsePhase() < _listenersList.Count)
            {
                //Check if the listener already exists in mainBuffer
                foreach (ITurnListener preExistingListener in _listenersList[listener.GetResponsePhase()])
                {
                    //if the names match, then the listener already exists. State a warning and return.
                    if (listener.GetConcreteListenerNameForDebugging() == preExistingListener.GetConcreteListenerNameForDebugging())
                    {
                        Debug.LogWarning($"Attempting to add preExisting Listener ({listener.GetConcreteListenerNameForDebugging()}) " +
                                         $"to the Turn System. Ignoring Command.");
                        return;
                    }
                }

                //If both the following are true, then add the unique listener to the additiveBuffer:
                //1) turn systems is active
                //2) the listener's phase matches the currently waiting phase of the turn
                //reasoning: C# lists can't be modified while being iterated through. To get around this,
                //the buffer will be checked and added to the active-listeners list before all active listeners are checked for competion of their tasks.
                //This handles the case of new additions being added to the current phase while the manager is awaiting completion of the already running listeners.
                if (_isTurnSystemActive && listener.GetResponsePhase() == (int)_currentPhase)
                {
                    STKDebugLogger.LogStatement(_isDebugActive,$"Detected Listener {listener.GetConcreteListenerNameForDebugging()}" +
                        $" requesting subscription to current phase. Adding listener to Subscription buffer...");
                    MarkListenerToBeAddedToCurrentPhase(listener);

                    return;
                }
                    
                else
                    IntegrateListenerIntoTurnSystem(listener);
            }
                
        }

        public void RemoveTurnListener(ITurnListener listener)
        {
            //if the response phase of the listener is a valid phase for the turn system, then remove it to the respective phase's list
            if (listener.GetResponsePhase() < _listenersList.Count)
            {
                //If both the following are true, then add the listener from the removal Buffer:
                //1) turn systems is active
                //2) the listener's phase matches the currently waiting phase of the turn
                //reasoning: C# lists can't be modified while being iterated through. To get around this,
                //the buffer will be checked and each listner will be reset and removed before all active listeners are checked for competion of their tasks.
                //This handles the case of removals being requested within current phase while the manager is awaiting completion of the already running listeners.
                if (listener.GetResponsePhase() == (int)_currentPhase && _isTurnSystemActive)
                {
                    STKDebugLogger.LogStatement(_isDebugActive, $"Detected Listener '{listener.GetConcreteListenerNameForDebugging()}'" +
                        $" requesting removal from current phase. Adding listener to removal buffer...");
                    MarkListenerForRemovalFromCurrentPhase(listener);
                }

                else
                    RemoveListenerFromTurnSystem(listener);
                
                
                    
            }
        }

        public void StartTurnSystem()
        {
            if (_isTurnSystemActive == false)
            {
                _isTurnSystemActive = true;
                STKDebugLogger.LogStatement(_isDebugActive, "Starting TurnSystem...");
                _turnManager = ManageTurnLifecycles();
                StartCoroutine(_turnManager);
            }
        }

        public void StopTurnSystem()
        {
            if (_isTurnSystemActive)
            {
                _isTurnSystemActive = false;
                StopCoroutine(_turnManager);
                _turnManager = null;
                OnTurnSystemInterrupted?.Invoke();
                _currentPhase = 0;
                _currentTurnCount = 0;
            }
        }

        public bool IsTurnSystemActive()
        {
            return _isTurnSystemActive;
        }


        //Utils
        private void InitializeListenerLists()
        {
            _listenersList = new List<List<ITurnListener>>();
            _listenersPerPhaseList = new List<int>();

            _currentPhaseListenerAdditionsBuffer = new List<ITurnListener>();
            _currentPhaseListenerRemovalsBuffer = new List<ITurnListener>();
;
            //Create a list for each phase
            for (int i = 0; i < System.Enum.GetNames(typeof(TurnPhase)).Length; i++)
            {
                _listenersList.Add(new List<ITurnListener>());
                _listenersPerPhaseList.Add(0);
            }
                
        }

        private void MarkListenerToBeAddedToCurrentPhase(ITurnListener newListener)
        {
            if (_currentPhaseListenerAdditionsBuffer.Contains(newListener) == false)
            {
                _currentPhaseListenerAdditionsBuffer.Add(newListener);
                STKDebugLogger.LogStatement(_isDebugActive, $"'{newListener.GetConcreteListenerNameForDebugging()}' added to subscription buffer");
            }
            else
                STKDebugLogger.LogStatement(_isDebugActive, $"'{newListener.GetConcreteListenerNameForDebugging()}' is already in the Subscription Buffer");
        }

        private void MarkListenerForRemovalFromCurrentPhase(ITurnListener listener)
        {
            //Simply remove the listener from the addBuffer if it hasn't been added yet
            if (_currentPhaseListenerAdditionsBuffer.Contains(listener))
            {
                _currentPhaseListenerAdditionsBuffer.Remove(listener);
                STKDebugLogger.LogStatement(_isDebugActive, $"'{listener.GetConcreteListenerNameForDebugging()}' Detected in Subscription buffer. " +
                    $"Removed listener from Subscription Buffer");
            }
               

            //otherwise, add it to the remove buffer
            else if (_currentPhaseListenerRemovalsBuffer.Contains(listener) == false)
            {
                _currentPhaseListenerRemovalsBuffer.Add(listener);
                STKDebugLogger.LogStatement(_isDebugActive, $"'{listener.GetConcreteListenerNameForDebugging()}' added to the Removal Buffer");
            }
            else
                STKDebugLogger.LogStatement(_isDebugActive, $"'{listener.GetConcreteListenerNameForDebugging()}' is already in the Removal Buffer");

        }

        private void IntegrateListenerIntoTurnSystem(ITurnListener listener)
        {
            //Add listener to the appropriate list, if it doesn't exist already
            _listenersList[listener.GetResponsePhase()].Add(listener);
            STKDebugLogger.LogStatement(_isDebugActive, $"Added newListener '{listener.GetConcreteListenerNameForDebugging()}' " +
                $"to the '{(TurnPhase)listener.GetResponsePhase()}' phase of the Turn System.");

            //Increment count
            _listenersPerPhaseList[listener.GetResponsePhase()] += 1;

            //Subscribe Listener to the OnInterrupt event
            SubscribeListenerToInterruptionEvent(listener);
        }

        private void RemoveListenerFromTurnSystem(ITurnListener listener)
        {
            bool listenerFound = _listenersList[listener.GetResponsePhase()].Remove(listener);
            STKDebugLogger.LogStatement(_isDebugActive, $"Removed listener '{listener.GetConcreteListenerNameForDebugging()}' " +
                    $"of the '{(TurnPhase)listener.GetResponsePhase()}' phase from the Turn System.");

            if (listenerFound)
            {
                UnsubscribeListenerFromInterruptionEvent(listener);

                //Decrement the count
                _listenersPerPhaseList[listener.GetResponsePhase()] -= 1;
            }
        }

        private void FulfillNewSubscribeRequestsForListenersAwaitingSubscriptionToCurrentPhase()
        {
            if (_currentPhaseListenerAdditionsBuffer.Count > 0)
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"Integrating SubscriptionBuffer into current phase...");
                foreach (ITurnListener listener in _currentPhaseListenerAdditionsBuffer)
                {
                    IntegrateListenerIntoTurnSystem(listener);
                    listener.RespondToNotification(_currentTurnCount);
                }

                STKDebugLogger.LogStatement(_isDebugActive, $"Integration Completed. Clearing Subscriptions Buffer...");
                _currentPhaseListenerAdditionsBuffer.Clear();
            }
        }

        private void FulfillNewRemoveRequestsForListenersAwaitingRemovalFromCurrentPhase()
        {
            if (_currentPhaseListenerRemovalsBuffer.Count > 0)
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"Fulfilling removal requests in Removal Buffer...");
                foreach (ITurnListener listener in _currentPhaseListenerRemovalsBuffer)
                {
                    RemoveListenerFromTurnSystem(listener);
                    listener.ResetUtilsOnTurnSystemInterruption();
                }

                STKDebugLogger.LogStatement(_isDebugActive, $"Removals Completed. Clearing Removals Buffer...");
                _currentPhaseListenerRemovalsBuffer.Clear();
            }
        }

        private IEnumerator ManageTurnLifecycles()
        {
            //decide whether or not this turn system is endless
            bool isMaxTurnLimitInfinite = false;
            if (_maxTurnCount < 1)
                isMaxTurnLimitInfinite = true;


            //init the phase metadata
            int phaseCounter;
            int numberOfPhases = System.Enum.GetNames(typeof(TurnPhase)).Length;


            //begin managing turns. Either cycle through the phases until the max turn is reached, or cycle endlessly
            while (_currentTurnCount < _maxTurnCount || isMaxTurnLimitInfinite)
            {
                //Reset the turn back to the beginning phase
                phaseCounter = 0;

                STKDebugLogger.LogStatement(_isDebugActive, $"Entering Turn {_currentTurnCount}");

                while (phaseCounter < numberOfPhases)
                {
                    //Update the turnSystem's phase and then notify all relevant IListeners
                    _currentPhase = (TurnPhase)phaseCounter;
                    STKDebugLogger.LogStatement(_isDebugActive, $"New Phase started: {_currentPhase}");
                    NotifyTurnListenersOfPhaseChange();


                    //Wait until all IListeners finish their respective response...
                    bool areAllListenersReady = false;
                    while (areAllListenersReady == false)
                    {
                        FulfillNewSubscribeRequestsForListenersAwaitingSubscriptionToCurrentPhase();
                        FulfillNewRemoveRequestsForListenersAwaitingRemovalFromCurrentPhase();

                        //now assume all are ready until one is verified to not be ready
                        areAllListenersReady = true;

                        //foreach listener of the current turn phase, (Do above comment ^)
                        foreach (ITurnListener listener in _listenersList[(int)_currentPhase])
                        {
                            if (listener.IsTurnListenerReadyToPassPhase() == false)
                            {
                                areAllListenersReady = false;
                                break;
                            }
                        }

                        //wait one frame before checking again
                        yield return null;
                    }

                    //All IListeners have completed their respective response.
                    //Reset their response flags and then Increment the phaseCounter

                    foreach (ITurnListener listener in _listenersList[(int)_currentPhase])
                        listener.ResetResponseFlag();

                    STKDebugLogger.LogStatement(_isDebugActive, $"{(TurnPhase)phaseCounter} Ended.");
                    phaseCounter++;
                }
                STKDebugLogger.LogStatement(_isDebugActive, $"End of turn {_currentTurnCount} reached.");
                _currentTurnCount++;
            }

            //Communicate turns over
            OnMaxTurnCountReached?.Invoke();
        }

    }

}

