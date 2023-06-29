using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit;

public class TestTurnListener : ITurnListener
{
    //Declarations
    int _responsePhase;
    bool _isListenerTaskStarted = false;
    bool _isListenerTaskCompleted = false;
    string _ListenerName = "Unnamed Listener";
    ITurnBroadcaster _broadcaster;
    SimpleTimer _taskTimer;


    //Constructors
    public TestTurnListener(int turnPhase, string name, SimpleTimer taskTimer, ITurnBroadcaster turnSystem)
    {
        if (turnPhase >= 0)
            this._responsePhase = turnPhase;

        this._ListenerName = name;

        this._taskTimer = taskTimer;

        //Make sure the timer is set to something
        if (taskTimer.GetLifespan() == 0)
            taskTimer.SetLifespan(1);

        _broadcaster = turnSystem;
    }


    //Interface Utils
    public void ResetUtilsOnTurnSystemInterruption()
    {
        _taskTimer.CancelTimer();
        ResetResponseFlag();
    }

    public int GetResponsePhase()
    {
        return _responsePhase;
    }

    public string GetConcreteListenerNameForDebugging()
    {
        return _ListenerName;
    }

    public bool IsTurnListenerReadyToPassPhase()
    {
        if (_isListenerTaskStarted && _isListenerTaskCompleted)
            return true;
        else return false;
    }

    public void RespondToNotification(int turnNumber)
    {
        StartTask();
    }

    public ITurnBroadcaster GetTurnBroadcaster()
    {
        return _broadcaster;
    }

    public void ResetResponseFlag()
    {
        //unsubscribe
        _taskTimer.OnTimerExpired -= FlagTaskAsComplete;

        _isListenerTaskCompleted = false;
        _isListenerTaskStarted = false;

        //log status
        Debug.Log($"{_ListenerName}, (phase{_responsePhase}) flags reset");
    }


    //Utils
    private void StartTask()
    {
        _isListenerTaskStarted = true;

        //subscribe
        _taskTimer.OnTimerExpired += FlagTaskAsComplete;

        //Log status
        Debug.Log($"{_ListenerName}, (phase{_responsePhase}) task started. Beginning Timer");
        _taskTimer.StartTimer();

    }

    private void FlagTaskAsComplete()
    {
        _isListenerTaskCompleted = true;
        Debug.Log($"{_ListenerName}, (phase{_responsePhase}) completed.");
    }
}

public class TurnSystemTester : MonoBehaviour
{
    //Declarations
    [SerializeField] private TurnSystem _turnSystem;
    [SerializeField] private List<SimpleTimer> _timersList;
    [SerializeField] private List<ITurnListener> _turnListeners;


    //Monos
    private void OnEnable()
    {
        _turnSystem.OnMaxTurnCountReached += LogTurnSystemCompleted;
    }

    private void OnDisable()
    {
        _turnSystem.OnMaxTurnCountReached -= LogTurnSystemCompleted;
    }

    private void Start()
    {
        //Init the test turn listeners
        _turnListeners = new List<ITurnListener>();

        int phaseIndex = 0;
        int currentPhaseListenerCount = 0;
        int phaseDistributionCount = 3;
        for (int i = 0; i < _timersList.Count; i++)
        {
            if (currentPhaseListenerCount == phaseDistributionCount)
            {
                currentPhaseListenerCount = 0;
                phaseIndex++;
            }

            _turnListeners.Add(new TestTurnListener(phaseIndex, "TestTurnListener" + i, _timersList[i], _turnSystem));
            currentPhaseListenerCount++;
        }

        //Adding Listeners to the turn System
        foreach (ITurnListener listener in _turnListeners)
            _turnSystem.AddTurnListener(listener);

        LogAllListenersInTurnSystem();

        Debug.Log("Staring Turn System");
        _turnSystem.StartTurnSystem();
        
    }


    //Utils
    private void LogTurnSystemCompleted()
    {
        Debug.Log("Final Turn Completed. TurnSystemEnded");
    }

    private void LogAllListenersInTurnSystem()
    {
        for (int i = 0; i < System.Enum.GetNames(typeof(TurnPhase)).Length; i++)
        {
            Debug.Log($"Listeners in Phase{i}: {_turnSystem.GetListenerCount(i)}");
            foreach (ITurnListener listener in _turnSystem.GetListenersInPhase(i))
                LogListenerStatus(listener);
        }
    }

    private void ToggleTimerViaInput(SimpleTimer timer)
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (timer.IsTimerTicking())
                timer.CancelTimer();
            else timer.StartTimer();
        }
    }

    private void LogListenerStatus(ITurnListener listener)
    {
        Debug.Log($"Name: {listener.GetConcreteListenerNameForDebugging()}, " +
                  $"Phase: {listener.GetResponsePhase()}, " +
                  $"IsCompleted: {listener.IsTurnListenerReadyToPassPhase()}");

    }

    private void LogListenerOnInput(ITurnListener listener)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LogListenerStatus(listener);
            if (listener.IsTurnListenerReadyToPassPhase())
            {
                Debug.Log("Resetting Listener ResponseFlags...");
                listener.ResetResponseFlag();
            }
        }
            
    }

}




