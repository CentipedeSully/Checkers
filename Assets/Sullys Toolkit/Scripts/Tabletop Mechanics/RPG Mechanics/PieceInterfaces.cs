using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SullysToolkit.TableTop.RPG
{
 


    public interface IMoveableRPGPiece
    {
        GamePiece GetGamePiece();

        void MoveToNeighborCell((int, int) xyDirection);

        int GetCurrentMovePoints();

        void SetCurrentMovePoints(int value);

        int GetMaxMovePoints();

        void SetMaxMovePoints(int value);
    }

    public interface IDamageableRPGPiece
    {
        GamePiece GetGamePiece();

        void RecieveDamage(int value);
    }

    public interface IHealableRPGPiece
    {
        GamePiece GetGamePiece();

        void ReceiveHeals(int value);
    }

    public interface IHealthManager
    {
        int GetCurrentHealth();

        void SetCurrentHealth(int value);

        int GetMaxHealth();

        void SetMaxHealth(int value);

        void KillThisInstance();
    }

    public interface ILevelableRPGPiece
    {
        GamePiece GetGamePiece();

        int GetCurrentLv();

        int GetMaxLv();

        void SetCurrentLv(int value);

        void LvUp();

        int GetCurrentExp();

        void SetCurrentExp(int value);

        int GetExpGate();

        void GainExp(int value);

        void ClearExp();
    }

    public interface IRPGAttributes
    {
        int GetAtkDie();

        void SetAtkDie(int value);

        int GetAtkModifier();

        void SetAtkModifier(int value);

        int GetDef();

        void SetDef(int value);

        int GetDamageModifier();

        void SetDamageModifier(int value);

        int GetDamageDie();

        void SetDamageDie(int value);

        int GetCurrentActionPoints();

        void SetCurrentActionPoints(int value);

        int GetMaxActionPoints();

        void SetMaxActionPoints(int value);
    }

    public interface IRPGConflictLogger
    {
        void LogConflict(int attackerAtkScore, int attackerDmgScore, int attackerDef, int defenderAtkScore, int defenderDmgScore, int defenderDef);
    }

    public interface IRPGIdentityDefinition
    {
        GamePiece GetGamePiece();

        string GetName();

        void SetName(string name);

        string GetFaction();

        void SetFaction(string faction);

        bool IsHostile();

        void SetHostility(bool value);

        string GetDescription();

        void SetDescription(string newDescription);

    }

    public interface IRPGExperienceProvider
    {
        int GetExpValue();

        void SetExpValue(int value);
    }

    public interface IRPGInteractablePiece
    {
        GamePiece GetGamePiece();

        void TriggerInteractionEvent(GamePiece performer);
    }

    //Currently Work in Progress unused


    public interface IUIDisplayController
    {
        GameObject GetDisplayObject();

        void SetDisplayObject(GameObject newDisplay);

        void DisplayData();

        void HideData();

        void UpdateData();

        bool IsDataOnDisplay();

        void SetupDisplayOnStart();
    }

    public interface IDisplayableRPGAttribute
    {
        void UpdateAttributeInDisplay(IUIDisplayController displayController);
    }

}
