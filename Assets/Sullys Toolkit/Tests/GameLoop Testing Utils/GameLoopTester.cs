using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit;

public class GameLoopTester : GameLoopSystem
{
    //Declarations
    [Header("Debug Commands")]
    [SerializeField] private bool _pauseCommand;
    [SerializeField] private bool _startCommand;
    [SerializeField] private bool _endCommand;



    //Monobehaviours
    private void Update()
    {
        ListenForDebugCommands();
    }



    //Inherited, Overridden Utils
    public override void InitializeAwakeSettings()
    {
        //...
    }

    public override void InitializeStartSettings()
    {
        //...
    }



    //Utils
    private void ListenForDebugCommands()
    {
        if (_pauseCommand)
        {
            _pauseCommand = false;
            TogglePause();
        }

        if (_startCommand)
        {
            _startCommand = false;
            EnterGameloop();
        }

        if (_endCommand)
        {
            _endCommand = false;
            ExitGameLoop();
        }
    }

}

