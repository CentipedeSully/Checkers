using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinCubeViaTimer : MonoBehaviour
{
    //Delcarations
    [SerializeField] private SimpleTimer _timer;
    [SerializeField] private SpinningCubeAnimController _cubeAnimController;


    //Mons
    private void Update()
    {
        ManipulateSpinBasedOnTimer();
    }


    //Utils
    private void ManipulateSpinBasedOnTimer()
    {
        if (_timer.IsTimerTicking())
            _cubeAnimController.StartSpinning();
        else _cubeAnimController.StopSpinning();
    }



}
