using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SullysToolkit.TableTop.RPG
{
    public class TriggerConflict : MonoBehaviour, IRPGInteractablePiece
    {
        //Declarations
        [SerializeField] private List<string> _enemyFactions;
        [SerializeField] private GamePiece _gamePieceRef;
        [SerializeField] private bool _isDebugActive = true;
        private IRPGIdentityDefinition _identityRef;
        private IRPGAttributes _attributesRef;

        //Events
        public delegate void ConflictTriggeredEvent(GamePiece attacker, GamePiece defender);
        public event ConflictTriggeredEvent OnConflictTriggered;

        //Monobehaviours
        private void Awake()
        {
            InitializeReferences();
        }



        //Internal utils
        private void InitializeReferences()
        {
            _gamePieceRef = GetComponent<GamePiece>();
            _identityRef = _gamePieceRef.GetComponent<IRPGIdentityDefinition>();
            _attributesRef = _gamePieceRef.GetComponent<IRPGAttributes>();
        }


        //Getters, Setters, & Commands
        public GamePiece GetGamePiece()
        {
            return _gamePieceRef;
        }

        public void TriggerInteractionEvent(GamePiece performer)
        {
            IRPGAttributes attackerAttributes = performer?.GetComponent<IRPGAttributes>();
            if (attackerAttributes == null)
            {
                STKDebugLogger.LogError($"gamePiece {performer} has no attributes to deduct Ap from");
                return;
            }

            else if (attackerAttributes.GetCurrentActionPoints()  > 0)
            {
                STKDebugLogger.LogStatement(_isDebugActive, $"Comparing the Identities of self(defender) and Attacker({performer})...");
                IRPGIdentityDefinition identifiedGamePiece = performer.GetComponent<IRPGIdentityDefinition>();

                if (_enemyFactions.Contains(identifiedGamePiece.GetFaction()))
                {
                    STKDebugLogger.LogStatement(_isDebugActive, $"Interaction with Hostile verified! Determining Conflict type");
                    if (_attributesRef.GetCurrentActionPoints() < 1)
                    {
                        STKDebugLogger.LogStatement(_isDebugActive, $"Entering OneSided Conflict due to defender having insufficient AP. Deducting Ap from attacker");
                        ConflictResolver.ResolveOneSidedConflict(performer, _gamePieceRef);
                    }
                    else
                    {
                        STKDebugLogger.LogStatement(_isDebugActive, $"Entering Two-Sided Conflict and Deducting AP fom both parties.");
                        ConflictResolver.ResolveTwoSidedConflict(performer, _gamePieceRef);
                    }

                    OnConflictTriggered?.Invoke(performer, _gamePieceRef);
                }
                else
                    STKDebugLogger.LogStatement(_isDebugActive, $"The interaction Performer isn't an enemy of this gamePiece. Ignoring Conflict.");

            }

            else
                STKDebugLogger.LogStatement(_isDebugActive,$"Interaction Failed. {performer} has no AP");

        }
    }
}

