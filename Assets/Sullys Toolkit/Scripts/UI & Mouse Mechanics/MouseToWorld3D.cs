using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SullysToolkit
{
    public class MouseToWorld3D : MonoBehaviour
    {
        //Declarations
        [SerializeField] private Camera _cameraReferencePerspective;
        [SerializeField] private LayerMask _validDetectionLayers;
        private Vector3 _mouseWorldPosition;
        public bool _isDebugActive = false;



        //Monobehaviors
        private void Update()
        {
            if (_isDebugActive)
                LogMouseWorldPosition();
        }



        //Utils
        public Vector3 GetWorldPosition()
        {
            RaycastHit detectedCollider;
            Ray pointerRay = _cameraReferencePerspective.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(pointerRay, out detectedCollider, float.MaxValue, _validDetectionLayers))
                _mouseWorldPosition = detectedCollider.point;
            else _mouseWorldPosition = Vector3.positiveInfinity;

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

