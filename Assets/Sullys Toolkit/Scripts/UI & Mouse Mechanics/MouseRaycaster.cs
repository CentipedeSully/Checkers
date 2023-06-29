using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SullysToolkit
{
    //Interfaces
    public interface IMouseInteractable
    {
        void LeftClick();

        void RightClick();

        void MiddleClick();

        void OnHover();

        void OnHoverAfterDelay();

        void OnHoverExit();

    }


    public class MouseRaycaster : MonoBehaviour
    {
        //Declarations
        [Header("Raycaster Settings")]
        [SerializeField] private string _RaycasterName = "Unnamed Selector";
        [SerializeField] private float _raycastDistance = 50;
        [SerializeField] private Vector3 _castDirection = Vector3.back;
        [SerializeField] private LayerMask _selectableLayers;
        [SerializeField] private ISelectionCache<GameObject> _selectionCache;
        [SerializeField] private MouseToWorld2D _mouseToWorld2DReference;

        //Debugging
        [Header("Debugging Utils")]
        [SerializeField] private bool _isDebugActive;
        [SerializeField] private Color _gizmoColor;
        [SerializeField] private bool _clearSelectionCmd;


        //Monos
        private void Awake()
        {
            _selectionCache = GetComponent<ISelectionCache<GameObject>>();
        }

        private void Update()
        {
            ListenforDebugCommandsIfDebugActive();
            ControlRaycasterViaMouseClicks();
        }

        private void OnDrawGizmosSelected()
        {
            DrawMousePointer();
        }





        //Internal Utils
        private RaycastHit2D CastRayFromMouse()
        {
            Vector2 castOrigin = _mouseToWorld2DReference.GetWorldPosition();
            return Physics2D.Raycast(castOrigin, _castDirection,_raycastDistance,_selectableLayers);
        }

        private void SetSelectionViaRaycast()
        {
            RaycastHit2D raycastData = CastRayFromMouse();
            if (raycastData.collider != null)
                _selectionCache.SetSelection(raycastData.collider.gameObject);
            else ClearCurrentSelection();
            
        }

        private void AddSelectionViaRaycast()
        {
            RaycastHit2D raycastData = CastRayFromMouse();
            if (raycastData.collider != null)
                _selectionCache.AddSelection(raycastData.collider.gameObject);
            else ClearCurrentSelection();
        }

        private void ClearCurrentSelection()
        {
            _selectionCache.ClearSelection();
        }



        //Getters, Setters, and Commands
        public string GetRaycasterName()
        {
            return _RaycasterName;
        }

        public ISelectionCache<GameObject> GetSelectionCache()
        {
            return _selectionCache;
        }

        public void SetSelectionCache(ISelectionCache<GameObject> newCache)
        {
            if (newCache != null)
                _selectionCache = newCache;
        }

        public LayerMask GetSelectableLayers()
        {
            return _selectableLayers;
        }

        public void SetSelectableLayers(LayerMask newLayerMask)
        {
            _selectableLayers = newLayerMask;
        }


        public bool IsDebugActive()
        {
            return _isDebugActive;
        }

        public void SetDebugMode(bool newValue)
        {
            _isDebugActive = newValue;
        }


        //Debugging
        private void DrawMousePointer()
        {
            Gizmos.color = _gizmoColor;
            Vector3 mouseWorldPosition = _mouseToWorld2DReference.GetWorldPosition();
            Vector3 debugLineStart = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, Camera.main.transform.position.z);
            Vector3 debugLineEnd = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, _raycastDistance);
            Gizmos.DrawLine(debugLineStart, debugLineEnd);
        }

        private void ListenforDebugCommandsIfDebugActive()
        {
            if (_isDebugActive)
            {
                if (_clearSelectionCmd)
                {
                    _clearSelectionCmd = false;
                    if (_selectionCache != null)
                        _selectionCache.ClearSelection();
                }
            }
        }

        private void ControlRaycasterViaMouseClicks()
        {
            if (Input.GetKey(KeyCode.Mouse0))
                SetSelectionViaRaycast();
            else if (Input.GetKey(KeyCode.Mouse1))
                AddSelectionViaRaycast();

        }

    }
}

