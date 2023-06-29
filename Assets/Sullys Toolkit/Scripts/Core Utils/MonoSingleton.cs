using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                    Debug.LogError(typeof(T).ToString() + "instance is null");
                return _instance;
            }
        }

        private void Awake()
        {
            _instance = (T)this;
            InitializeAdditionalFields();
        }

        protected virtual void InitializeAdditionalFields() { }
    }
}

