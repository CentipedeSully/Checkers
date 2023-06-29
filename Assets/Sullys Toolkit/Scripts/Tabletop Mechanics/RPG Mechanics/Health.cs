using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SullysToolkit.TableTop.RPG
{
    public class Health : MonoBehaviour, IHealthManager, IHealableRPGPiece, IDamageableRPGPiece, IRegenerateable, IDisplayableRPGAttribute
    {

        //Declarations
        [Header("Health Settings")]
        [SerializeField] private int _currentHealth = 1;
        [SerializeField] [Min(1)] private int _maxHealth = 1;
        [SerializeField] [Min(0)] private int _regenAmount;

        [Header("References")]
        [SerializeField] private GamePiece _gamePieceReference;
        [SerializeField] private IUIDisplayController _displayControllerRef;

        //Events
        public delegate void HealthEvent(int value);
        public event HealthEvent OnHealed;
        public event HealthEvent OnDamaged;



        //Monobehaviours
        private void Awake()
        {
            InitializeSettings();
        }

        private void OnEnable()
        {
            OnHealed += UpdateDisplayWrapper;
            OnDamaged += UpdateDisplayWrapper;
        }

        private void OnDisable()
        {
            OnHealed -= UpdateDisplayWrapper;
            OnDamaged -= UpdateDisplayWrapper;
        }


        //Internal Utils
        private void InitializeSettings()
        {
            _gamePieceReference = GetComponent<GamePiece>();
            _displayControllerRef = GetComponent<IUIDisplayController>();
            _currentHealth = _maxHealth;
        }


        //Getters, Setters, & Commands
        public int GetCurrentHealth()
        {
            return _currentHealth;
        }

        public void SetCurrentHealth(int value)
        {
            _currentHealth = Mathf.Clamp(value, 0, _maxHealth);
        }

        public int GetMaxHealth()
        {
            return _maxHealth;
        }

        public void SetMaxHealth(int value)
        {
            _maxHealth = Mathf.Max(1, value);

            //Reflect the new maxiumum
            SetCurrentHealth(_currentHealth);
        }

        public GamePiece GetGamePiece()
        {
            return _gamePieceReference;
        }

        public void ReceiveHeals(int value)
        {
            int healValue = Mathf.Max(0, value);
            SetCurrentHealth(_currentHealth + healValue);

            OnHealed?.Invoke(healValue);
        }

        public void RecieveDamage(int value)
        {
            int damageValue = Mathf.Max(0, value);
            SetCurrentHealth(_currentHealth - damageValue);

            OnDamaged?.Invoke(damageValue);
        }

        public void KillThisInstance()
        {
            //...
        }

        public void RegenerateAttributes()
        {
            ReceiveHeals(_regenAmount);
        }

        public void UpdateAttributeInDisplay(IUIDisplayController displayController)
        {
            if (displayController != null)
                displayController.UpdateData();
        }

        public void UpdateDisplayWrapper(int value)
        {
            UpdateAttributeInDisplay(_displayControllerRef);
        }
    }
}


