using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SullysToolkit;
using UnityEngine.Events;


namespace SullysToolkit
{
    public interface IGameLoopSystem
    {
        void InitializeAwakeSettings();

        void InitializeStartSettings();

        void EnterGameloop();

        void ExitGameLoop();

        bool IsGameLoopEntered();

        void TogglePause();

        bool IsGamePaused();
    }


    public abstract class GameLoopSystem : MonoSingleton<GameLoopSystem>, IGameLoopSystem
    {
        //Declarations
        [Header("Core Gameloop Status")]
        [SerializeField] protected bool _isGameLoopEntered;
        [SerializeField] protected bool _isGamePaused;

        //events
        public delegate void GameLoopEvent(bool value);
        public event GameLoopEvent OnGameLoopEntered;
        public event GameLoopEvent OnGameLoopPaused;



        //Monobehaviours

        // "InitializeAwakeSettings()" is called within the inherited monosingleton's awake state.

        private void Start()
        {
            InitializeStartSettings();
        }




        //Interface Utils
        public abstract void InitializeAwakeSettings();

        public abstract void InitializeStartSettings();

        public virtual void EnterGameloop()
        {
            if (!_isGameLoopEntered)
            {
                _isGameLoopEntered = true;
                OnGameLoopEntered?.Invoke(true);
            }
        }

        public virtual void ExitGameLoop()
        {
            if (_isGameLoopEntered)
            {

                if (_isGamePaused)
                    TogglePause();

                _isGameLoopEntered = false;
                OnGameLoopEntered?.Invoke(false);
            }
        }

        public virtual bool IsGameLoopEntered()
        {
            return _isGameLoopEntered;
        }

        public virtual void TogglePause()
        {
            if (_isGameLoopEntered)
            {
                if (!_isGamePaused)
                {
                    _isGamePaused = true;
                    Time.timeScale = 0;
                    AudioListener.pause = true;
                    OnGameLoopPaused?.Invoke(true);
                }

                else
                {
                    _isGamePaused = false;
                    Time.timeScale = 1;
                    AudioListener.pause = false;
                    OnGameLoopPaused?.Invoke(false);
                }
            }
        }

        public virtual bool IsGamePaused()
        {
            return _isGamePaused;
        }



        //Inherited, overridden Utils
        protected override void InitializeAdditionalFields()
        {
            InitializeAwakeSettings();
        }




        //Utils
        //...

    }


}


