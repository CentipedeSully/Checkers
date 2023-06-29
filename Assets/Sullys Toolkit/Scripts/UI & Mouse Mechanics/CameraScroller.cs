using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


namespace SullysToolkit
{
    public class CameraScroller : MonoBehaviour
    {
        //Declarations
        [Header("Scroller Settings")]
        [SerializeField] private CinemachineVirtualCamera _UICameraReference;
        [SerializeField] private Transform _mapOrigin;
        [SerializeField] private Transform _cameraFollowTarget;
        [SerializeField] [Min(0)] private float _horizontalBoundDistance;
        [SerializeField] [Min(0)] private float _verticalBoundDistance;
        [SerializeField] [Min(0)] private float _scrollSpeed;
        [SerializeField] private bool _isScrollingEnabled = true;
        [SerializeField] private bool _isDebugActive = false;

        [Header("Debugging")]
        [SerializeField] [Range(-1,1)] private int _horizontalInput;
        [SerializeField] [Range(-1, 1)] private int _verticalInput;
        [SerializeField] private Color _gizmoColor = Color.white;



        //Monobehaviours
        private void Awake()
        {
            SetVirutalCameraToFollowScroller();
        }

        private void Update()
        {
            if (_isDebugActive)
                ListenForDebugCommands();

            if (_isScrollingEnabled)
                MoveScrollerIfWithinBounds();
        }


        private void OnDrawGizmosSelected()
        {
            if (IsDebugActive())
            {
                SetGizmoColor();
                DrawBoundaries();
                DrawCameraPosition();
                DrawMapOrigin();
            }
        }


        //Utils
        public void SetDebug(bool newValue)
        {
            _isDebugActive = newValue;
        }

        public bool IsDebugActive()
        {
            return _isDebugActive;
        }

        public void SetHorizontalInput(int newValue)
        {
            _horizontalInput = Mathf.Clamp(newValue, -1, 1);
        }

        public void SetVerticalInput(int newValue)
        {
            _verticalInput = Mathf.Clamp(newValue, -1, 1);
        }

        private void SetVirutalCameraToFollowScroller()
        {
            if (_UICameraReference != null)
            {
                if (_cameraFollowTarget == null)
                    _cameraFollowTarget = this.transform;

                _UICameraReference.Follow = _cameraFollowTarget;
            }
                
        }

        private void MoveScrollerIfWithinBounds()
        {
            float upperBound = _mapOrigin.position.y + _verticalBoundDistance;
            float lowerBound = _mapOrigin.position.y - _verticalBoundDistance;
            float rightBound = _mapOrigin.position.x + _horizontalBoundDistance;
            float leftBound = _mapOrigin.position.x - _horizontalBoundDistance;


            //positive up:  check if input == up and current position < upperbound
            if (_verticalInput > 0 && _cameraFollowTarget.position.y < upperBound)
                MoveScrollerVertically();

            //negative down
            else if (_verticalInput < 0 && _cameraFollowTarget.position.y > lowerBound)
                MoveScrollerVertically();

            //negative left
            if (_horizontalInput < 0 && _cameraFollowTarget.position.x > leftBound)
                MoveScrollerHorizontally();

            //positive right
            else if (_horizontalInput > 0 && _cameraFollowTarget.position.x < rightBound)
                MoveScrollerHorizontally();

        }

        private void MoveScrollerVertically()
        {
            Vector2 moveDirection = new Vector2(0, _verticalInput * _scrollSpeed * Time.deltaTime);
            _cameraFollowTarget.transform.Translate(moveDirection);
        }

        private void MoveScrollerHorizontally()
        {
            Vector2 moveDirection = new Vector2(_horizontalInput * _scrollSpeed * Time.deltaTime, 0);
            _cameraFollowTarget.transform.Translate(moveDirection);
        }




        //Debugging
        private void SetGizmoColor()
        {
            Gizmos.color = _gizmoColor;
        }

        private void DrawBoundaries()
        {
            if (_mapOrigin != null)
            {
                //calculate the four corners
                Vector2 bottomLeft = new Vector3(_mapOrigin.position.x - _horizontalBoundDistance, _mapOrigin.position.y - _verticalBoundDistance);
                Vector2 topLeft = new Vector3(_mapOrigin.position.x - _horizontalBoundDistance, _mapOrigin.position.y + _verticalBoundDistance);
                Vector2 bottomRight = new Vector3(_mapOrigin.position.x + _horizontalBoundDistance, _mapOrigin.position.y - _verticalBoundDistance);
                Vector2 topRight = new Vector3(_mapOrigin.position.x + _horizontalBoundDistance, _mapOrigin.position.y + _verticalBoundDistance);

                //Draw the lines
                Gizmos.DrawLine(bottomRight, topRight); //Right side
                Gizmos.DrawLine(bottomLeft, topLeft); //Left side
                Gizmos.DrawLine(topLeft, topRight); //Top
                Gizmos.DrawLine(bottomRight, bottomLeft); //Bottom
            }
        }

        private void DrawMapOrigin()
        {
            if (_mapOrigin != null)
                Gizmos.DrawWireSphere(_mapOrigin.position, .2f);
        }

        private void DrawCameraPosition()
        {
            if (_cameraFollowTarget != null)
                Gizmos.DrawWireSphere(_cameraFollowTarget.position, .15f);
        }

        private void ListenForDebugCommands()
        {
            if (Input.GetKey(KeyCode.A))
                _horizontalInput -= 1;
            else if (Input.GetKey(KeyCode.D))
                _horizontalInput += 1;
            else _horizontalInput = 0;

            if (Input.GetKey(KeyCode.W))
                _verticalInput += 1;
            else if (Input.GetKey(KeyCode.S))
                _verticalInput -= 1;
            else _verticalInput = 0;

           _horizontalInput = Mathf.Clamp(_horizontalInput, -1, 1);
           _verticalInput = Mathf.Clamp(_verticalInput, -1, 1);
        }
    }
}

