using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit.TableTop.RPG
{
    public class GrowthBehavior : MonoBehaviour, ILevelableRPGPiece
    {
        //Declarations
        [Header("Current Lvl Stats")]
        [SerializeField] [Min(1)] private int _currentLv = 1;
        [SerializeField] [Min(1)] private int _maxLv = 3;
        [SerializeField] [Min(0)] private int _currentExp;
        [SerializeField] [Min(0)] private int _nextLvExpGate;
        [SerializeField] [Min(1)] private int _baseExpGate = 10;
        [SerializeField] [Min(1)] private float _expGateGrowthMultiplier = 2;

        [Header("Growth modifiers")]
        [SerializeField] private int _maxHealthBase = 2;
        [SerializeField] private int _healthGrowthDieSize = 4;
        [SerializeField] private int _maxMovePointBase = 30;
        [SerializeField] private int _movePointGrowthDieSize = 12;
        [SerializeField] private int _atkModifierBase = 0;
        [SerializeField] private int _atkGrowthDieSize = 2;
        [SerializeField] private int _defBase = 10;
        [SerializeField] private int _defGrowthDieSize = 2;
        [SerializeField] private int _damageModifierBase = 0; 
        [SerializeField] private int _damageModifierGrowthDieSize = 2;

        [Header("Growable Stats")]
        [SerializeField] private bool _isHealthPresent;
        [SerializeField] private bool _isMovementBehaviorPresent;
        [SerializeField] private bool _isAttributesPresent;

        [Header("References")]
        [SerializeField] private GamePiece _gamePieceReference;
        [SerializeField] private IHealthManager _healthReference;
        [SerializeField] private IMoveableRPGPiece _movementReference;
        [SerializeField] private IRPGAttributes _attributesRef;



        //Monobehaviours
        private void Awake()
        {
            InitializePersonalSettings();
        }

        private void Start()
        {
            InitializeOtherExistingGamePieceAttributes();
        }



        //Internal Utilities
        private void InitializePersonalSettings()
        {
            _gamePieceReference = GetComponent<GamePiece>();
            _healthReference = GetComponent<IHealthManager>();
            _movementReference = GetComponent<IMoveableRPGPiece>();
            _attributesRef = GetComponent<IRPGAttributes>();

            if (_healthReference != null)
                _isHealthPresent = true;
            if (_movementReference != null)
                _isMovementBehaviorPresent = true;
            if (_attributesRef != null)
                _isAttributesPresent = true;

        }

        private void InitializeOtherExistingGamePieceAttributes()
        {
            InitializeHealth();
            InitializeMovePoints();
            InitializeConflictAttributes();
            RecalculateExpGate();
        }

        private void InitializeHealth()
        {
            if (_isHealthPresent)
            {
                int maxHealth = _maxHealthBase;
                if (_currentLv > 1)
                    maxHealth += DieRoller.RollManyDice(_healthGrowthDieSize, _currentLv - 1);

                _healthReference.SetMaxHealth(maxHealth);
                _healthReference.SetCurrentHealth(maxHealth);
            }
                
        }

        private void InitializeMovePoints()
        {
            if (_isMovementBehaviorPresent)
            {
                int maxMovePoints = _maxMovePointBase;
                if (_currentLv > 1)
                    maxMovePoints += DieRoller.RollManyDice(_movePointGrowthDieSize, _currentLv - 1);

                _movementReference.SetMaxMovePoints(maxMovePoints);
            }
        }

        private void InitializeConflictAttributes()
        {
            if (_isAttributesPresent)
            {
                int atkModifier = _atkModifierBase;
                int def = _defBase;
                int damageModifier = _damageModifierBase;

                if (_currentLv > 1)
                {
                    atkModifier += DieRoller.RollManyDice(_atkGrowthDieSize, _currentLv - 1);
                    def += DieRoller.RollManyDice(_defGrowthDieSize, _currentLv - 1);
                    damageModifier += DieRoller.RollManyDice(_damageModifierGrowthDieSize, _currentLv - 1);
                }

                _attributesRef.SetAtkModifier(atkModifier);
                _attributesRef.SetDef(def);
                _attributesRef.SetDamageModifier(damageModifier);
            }
        }

        private void IncrementHealth()
        {
            if (_isHealthPresent)
            {
                int maxHealth = _healthReference.GetMaxHealth();
                maxHealth += DieRoller.RollDie(_healthGrowthDieSize);
                _healthReference.SetMaxHealth(maxHealth);
                _healthReference.SetCurrentHealth(maxHealth);
            }
        }

        private void IncrementMovePoints()
        {
            if (_isMovementBehaviorPresent)
            {
                int maxMovePoints = _movementReference.GetMaxMovePoints();
                maxMovePoints += DieRoller.RollDie(_movePointGrowthDieSize);
                _movementReference.SetMaxMovePoints(maxMovePoints);
            }
        }

        private void IncrementConflictAttributes()
        {
            if (_isAttributesPresent)
            {
                int atk = _attributesRef.GetAtkModifier();
                int def = _attributesRef.GetDef();
                int damage = _attributesRef.GetDamageModifier();

                atk += DieRoller.RollDie(_atkGrowthDieSize);
                def += DieRoller.RollDie(_defGrowthDieSize);
                damage += DieRoller.RollDie(_damageModifierGrowthDieSize);


                _attributesRef.SetAtkModifier(atk);
                _attributesRef.SetDef(def);
                _attributesRef.SetDamageModifier(damage);
            }
        }

        private void RecalculateExpGate()
        {
            if (_currentLv < _maxLv)
                _nextLvExpGate = (int)(_baseExpGate + _baseExpGate *(_currentLv - 1) * _expGateGrowthMultiplier);
        }

        private void CheckExpForLvUp()
        {
            if (_currentLv < _maxLv)
            {
                if (_currentExp >= _nextLvExpGate)
                {
                    while (_currentExp >= _nextLvExpGate)
                    {
                        _currentExp -= _nextLvExpGate;
                        LvUp();
                    }
                    
                }
            }
        }




        //Getters, Setters, & Commands

        public GamePiece GetGamePiece()
        {
            return _gamePieceReference;
        }

        public int GetCurrentLv()
        {
            return _currentLv;
        }

        public int GetMaxLv()
        {
            return _maxLv;
        }

        public void SetCurrentLv(int value)
        {
            _currentLv = Mathf.Clamp(value, 1, _maxLv);
            ClearExp();
            InitializeHealth();
            InitializeMovePoints();
            InitializeConflictAttributes();
            RecalculateExpGate();
        }

        public void LvUp()
        {
            if (_currentLv < _maxLv)
            {
                _currentLv++;
                IncrementHealth();
                IncrementMovePoints();
                IncrementConflictAttributes();
                RecalculateExpGate();
            }
        }

        public int GetCurrentExp()
        {
            return _currentExp;
        }

        public void SetCurrentExp(int value)
        {
            _currentExp = Mathf.Max(0, value);
            CheckExpForLvUp();
        }

        public int GetExpGate()
        {
            return _nextLvExpGate;
        }

        public void GainExp(int value)
        {
            _currentExp += Mathf.Max(0, value);
            CheckExpForLvUp();
        }

        public void ClearExp()
        {
            SetCurrentExp(0);
        }
    }
}

