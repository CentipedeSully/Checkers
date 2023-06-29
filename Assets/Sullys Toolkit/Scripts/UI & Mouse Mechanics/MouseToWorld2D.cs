using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit
{
    public class MouseToWorld2D : MonoBehaviour
    {
        //Declarations
        [Header("Mouse Pointer Settings")]
        [SerializeField] private Camera _cameraReferencePerspective;
        [SerializeField] private float _zOverride = 0;

        [Header("Debugging Utils")]
        [SerializeField] public bool _isDebugActive = false;
        [SerializeField] private Vector3 _mouseWorldPosition;




        //Monobehaviors
        private void Awake()
        {
            _cameraReferencePerspective = Camera.main;
        }

        private void Update()
        {
            if (_isDebugActive)
                LogMouseWorldPosition();
        }



        //Utils
        public Vector3 GetWorldPosition()
        {
            if (_cameraReferencePerspective != null)
                _mouseWorldPosition = _cameraReferencePerspective.ScreenToWorldPoint(Input.mousePosition);
            else _mouseWorldPosition = Vector2.zero;
            _mouseWorldPosition.z = _zOverride;

            return _mouseWorldPosition;
        }

        public bool IsDebugActive()
        {
            return _isDebugActive;
        }

        public void SetDebug(bool newValue)
        {
            _isDebugActive = newValue;
        }

        private void LogMouseWorldPosition()
        {
            Debug.Log(GetWorldPosition());
        }



    }
}

