using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleTimer : MonoBehaviour
{
    //Declarations'
    [SerializeField] private float _lifespan;
    [SerializeField] private float _currentCount;
    [SerializeField] private bool _isTicking = false;

    //Events
    public delegate void SimpleTimerEvent();
    public event SimpleTimerEvent OnTimerExpired;                                                                                                    



    //Monobehaviours
    private void Update()
    {
        if (_isTicking)
            CountToLifespan();
    }



    //Utils
    private void CountToLifespan()
    {
        _currentCount += Time.deltaTime;

        if (_currentCount >= _lifespan)
        {
            ResetTimer();
            OnTimerExpired?.Invoke();
        }
    }

    private void ResetTimer()
    {
        _currentCount = 0;
        _isTicking = false;
    }


    //Control Utils
    public void StartTimer(float newDuration)
    {
        if (newDuration > 0 && _isTicking == false)
        {
            _lifespan = newDuration;
            _isTicking = true;
        }
            
    }

    public void StartTimer()
    {
        if (_lifespan > 0 && _isTicking == false)
            _isTicking = true;
    }

    public void CancelTimer()
    {
        ResetTimer();
    }


    //Getters && Setters
    public bool IsTimerTicking()
    {
        return _isTicking;
    }

    public float GetLifespan()
    {
        return _lifespan;
    }

    public void SetLifespan(float newDuration)
    {
        if (newDuration > 0)
            _lifespan = newDuration;
    }

    public float GetCurrentTime()
    {
        return _currentCount;
    }


}
