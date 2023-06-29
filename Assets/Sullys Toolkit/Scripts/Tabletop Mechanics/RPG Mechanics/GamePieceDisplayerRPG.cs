using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace SullysToolkit.TableTop.RPG
{
    public class GamePieceDisplayerRPG : MonoSingleton<GamePieceDisplayerRPG>
    {
        //Declarations
        [Header("Overall Display Settings")]
        [SerializeField] private bool _hideAllDisplaysOnAwake = true;

        [Header("Unit Display Settings")]
        [SerializeField] private GameObject _unitDisplayObject;
        [SerializeField] private TextMeshProUGUI _unitName;
        [SerializeField] private TextMeshProUGUI _hpValue;
        [SerializeField] private TextMeshProUGUI _apValue;
        [SerializeField] private TextMeshProUGUI _mpValue;
        [SerializeField] private TextMeshProUGUI _atkValue;
        [SerializeField] private TextMeshProUGUI _dmgValue;
        [SerializeField] private TextMeshProUGUI _defValue;

        [Header ("Point Of Interest Display Settings")]
        [SerializeField] private GameObject _pointOfInterestDisplayObject;
        [SerializeField] private TextMeshProUGUI _poiName;
        [SerializeField] private TextMeshProUGUI _poiDescription;
        [SerializeField] private TextMeshProUGUI _poiReward;

        [Header ("Terrain Display Settings")]
        [SerializeField] private GameObject _terrainDisplayObject;
        [SerializeField] private TextMeshProUGUI _terrainName;
        [SerializeField] private TextMeshProUGUI _terrainDescription;





        //Monobehavours
        //...


        //Internal Utils
        protected override void InitializeAdditionalFields()
        {
            SetupDisplays();
        }

        private void SetupDisplays()
        {
            if (_hideAllDisplaysOnAwake)
            {
                _unitDisplayObject.SetActive(false);
                _pointOfInterestDisplayObject.SetActive(false);
                _terrainDisplayObject.SetActive(false);
            }
        }



        //Getters Setters, & Commands
        public GameObject GetUnitDisplay()
        {
            return _unitDisplayObject;
        }

        public GameObject GetPOIDisplay()
        {
            return _pointOfInterestDisplayObject;
        }

        public GameObject GetTerrainDisplay()
        {
            return _terrainDisplayObject;
        }

        public void UpdateDisplayData(string name, int hp, int atk, int def, int dmgDie, int dmgModifier, int ap, int mp)
        {
            _unitName.text = name;
            _hpValue.text = hp.ToString();
            _atkValue.text = atk.ToString();
            _defValue.text = def.ToString();
            _dmgValue.text = Mathf.Max(0, dmgModifier + 1) + " - " + Mathf.Max(0, dmgDie + dmgModifier);
            _apValue.text = ap.ToString();
            _mpValue.text = mp.ToString();
        }

        public void UpdateDisplayData(string name, string desc, int reward)
        {
            _poiName.text = name;
            _poiDescription.text = desc;
            _poiReward.text = reward + " xp"; 
        }

        public void UpdateDisplayData(string name, string desc)
        {
            _terrainName.text = name;
            _terrainDescription.text = desc;
        }
    }
}

