using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit.TableTop.RPG
{
    public static class ConflictResolver
    {
        //Declarations
        private static IRPGConflictLogger _conflictLogger;
        private static int _unusedFieldCode = -999;
        private static int _lastAttackerAtkRoll;
        private static int _lastAttackerDef;
        private static int _lastAttackerDmgRoll;
        private static int _lastDefenderAtkRoll;
        private static int _lastDefenderDef;
        private static int _lastDefenderDmgRoll;


        //Internal Utils
        private static void DeductApCost(IRPGAttributes unit)
        {
            if (unit != null)
            {
                if (unit.GetCurrentActionPoints() > 0)
                    unit.SetCurrentActionPoints(unit.GetCurrentActionPoints() - 1);
            }
            else
                STKDebugLogger.LogWarning("Attempted Action Point Deduction from null unit attribute");
        }

        private static int CalculateAttackRoll(IRPGAttributes unit)
        {
            if (unit != null)
                return DieRoller.RollDieWithModifier(unit.GetAtkDie(), unit.GetAtkModifier());
            else
            {
                STKDebugLogger.LogWarning($"Null unit attribute tried to make an atkRoll. Returning {_unusedFieldCode}");
                return _unusedFieldCode;
            }
        }

        private static int CalculateDamageRoll(IRPGAttributes unit)
        {
            if (unit != null)
                return DieRoller.RollDieWithModifier(unit.GetDamageDie(), unit.GetDamageModifier());
            else
            {
                STKDebugLogger.LogWarning($"Null unit attribute tried to roll for Damage. Returning {_unusedFieldCode}");
                return -_unusedFieldCode;
            }
        }

        private static int GetDefence(IRPGAttributes unit)
        {
            if (unit != null)
                return unit.GetDef();
            else
            {
                STKDebugLogger.LogWarning($"Attempted to get Defence for null unit Attribute. Returning {_unusedFieldCode}");
                return -_unusedFieldCode;
            }
        }

        private static void DamageUnit(IDamageableRPGPiece unit, int damage)
        {
            if (unit != null)
                unit.RecieveDamage(damage);
            else
                STKDebugLogger.LogWarning("Attempted to damage a null Damageable unit");
        }

        private static void LogConflict()
        {
            _conflictLogger.LogConflict(_lastAttackerAtkRoll, _lastAttackerDmgRoll, _lastAttackerDef, _lastDefenderAtkRoll, _lastDefenderDmgRoll, _lastDefenderDef);
        }


        //Commands
        public static void ResolveOneSidedConflict(GamePiece attackerGamePiece, GamePiece defenderGamePiece)
        {
            //pay AP cost
            DeductApCost(attackerGamePiece.GetComponent<IRPGAttributes>());

            //Calculate current Conflict stats
            _lastAttackerAtkRoll = CalculateAttackRoll(attackerGamePiece.GetComponent<IRPGAttributes>());
            _lastAttackerDef = GetDefence(attackerGamePiece.GetComponent<IRPGAttributes>());
            _lastAttackerDmgRoll = CalculateDamageRoll(attackerGamePiece.GetComponent<IRPGAttributes>());

            _lastDefenderAtkRoll = _unusedFieldCode;
            _lastDefenderDef = GetDefence(defenderGamePiece.GetComponent<IRPGAttributes>());
            _lastDefenderDmgRoll = _unusedFieldCode;

            LogConflict();

            if (_lastAttackerAtkRoll >= _lastDefenderDef)
                DamageUnit(defenderGamePiece.GetComponent<IDamageableRPGPiece>(), _lastAttackerDmgRoll);
        }

        public static void ResolveTwoSidedConflict(GamePiece attackerGamePiece, GamePiece defenderGamePiece)
        {
            //pay AP cost
            DeductApCost(attackerGamePiece.GetComponent<IRPGAttributes>());
            DeductApCost(defenderGamePiece.GetComponent<IRPGAttributes>());

            //Calculate current Conflict stats
            _lastAttackerAtkRoll = CalculateAttackRoll(attackerGamePiece.GetComponent<IRPGAttributes>());
            _lastAttackerDef = GetDefence(attackerGamePiece.GetComponent<IRPGAttributes>());
            _lastAttackerDmgRoll = CalculateDamageRoll(attackerGamePiece.GetComponent<IRPGAttributes>());

            _lastDefenderAtkRoll = CalculateAttackRoll(defenderGamePiece.GetComponent<IRPGAttributes>());
            _lastDefenderDef = GetDefence(defenderGamePiece.GetComponent<IRPGAttributes>());
            _lastDefenderDmgRoll = CalculateDamageRoll(defenderGamePiece.GetComponent<IRPGAttributes>());

            LogConflict();

            if (_lastAttackerAtkRoll >= _lastDefenderDef)
                DamageUnit(defenderGamePiece.GetComponent<IDamageableRPGPiece>(), _lastAttackerDmgRoll);
            
            if (_lastDefenderAtkRoll >= _lastAttackerDef)
                DamageUnit(attackerGamePiece.GetComponent<IDamageableRPGPiece>(), _lastDefenderDmgRoll);
        }

        public static IRPGConflictLogger GetConflictLogger()
        {
            return _conflictLogger;
        }

        public static void SetConflictLogger(IRPGConflictLogger newConflictLogger)
        {
            if (newConflictLogger != null)
                _conflictLogger = newConflictLogger;
        }
    }
}
