using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit
{
    public interface ISelectionCache<T>
    {
        T GetSelection();

        void SetSelection(T newSelection);

        List<T> GetSelectionCollection();

        void AddSelection(T newSelection);

        void RemoveSelection(T existingSelection);

        void ClearSelection();

        bool IsSelectionAvailable();
    }




    //classes
    public class SelectionManager : MonoBehaviour, ISelectionCache<GameObject>
    {
        //Declarations
        [SerializeField] private List<GameObject> _selectionList;
        [SerializeField] private bool _isMultiSelectAvaialable = false;
        [SerializeField] private bool _isDebugActive = false;



        //Monobehaviours
        private void Awake()
        {
            _selectionList = new List<GameObject>();
        }




        //Interface Utils
        public void AddSelection(GameObject newSelection)
        {
            if (newSelection != null)
            {
                if (_isMultiSelectAvaialable)
                {
                    if (!_selectionList.Contains(newSelection))
                    {
                        _selectionList.Add(newSelection);
                        STKDebugLogger.LogStatement(_isDebugActive, $"Added new object ({newSelection.gameObject.name}, ID:{GetInstanceID()}) to the 'selection' Collection");
                    }
                    else
                        STKDebugLogger.LogStatement(_isDebugActive, $"Object ({newSelection.gameObject.name}, ID:{GetInstanceID()}) already exists in 'selection' Collection");
                }
                else SetSelection(newSelection);

            }

        }

        public void ClearSelection()
        {
            _selectionList = new List<GameObject>();
            STKDebugLogger.LogStatement(_isDebugActive, "Selection Collection Cleared");
        }

        public GameObject GetSelection()
        {
            if (_selectionList.Count > 1)
                return _selectionList[0];
            else
            {
                STKDebugLogger.LogWarning($"Attempted to Fetch a selection that doesn't exist in {name}. Return a default value...");
                return default;
            }
        }

        public List<GameObject> GetSelectionCollection()
        {
            return _selectionList;
        }

        public bool IsSelectionAvailable()
        {
            return _selectionList.Count > 0;
        }

        public void RemoveSelection(GameObject existingSelection)
        {
            if (existingSelection != null)
            {
                if (_selectionList.Contains(existingSelection))
                {
                    _selectionList.Remove(existingSelection);
                    STKDebugLogger.LogStatement(_isDebugActive, $"object ({existingSelection.name}, ID:{GetInstanceID()}) removed from selection collection");
                }

                else
                    STKDebugLogger.LogStatement(_isDebugActive, $"object ({existingSelection.name}, ID:{GetInstanceID()}) doesn't exist in selection collection");
            }

        }

        public void SetSelection(GameObject newSelection)
        {
            if (newSelection != null)
            {
                _selectionList = new List<GameObject>();
                _selectionList.Add(newSelection);
            }
        }




        //Internal Utils
        //...



        //Getters, Setters, & Commands
        public bool IsDebugActive()
        {
            return _isDebugActive;
        }

        public void SetDebugMode(bool newValue)
        {
            _isDebugActive = newValue;
        }



        //Debugging
        //...

    }
}
