using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

namespace CombatSystem.View
{
    public class UnitSelector : MonoBehaviour, IUnitSelector
    {
        private void Awake()
        {
            // throw new NotImplementedException();
            for (var i = 0; i < players.Length; i++)
            {
                //Prevents captured variable shenanigans
                var i1 = i;
                players[i].Hover += () => OnPlayerHovered(i1, players[i1]);
                players[i].Unhover += () => OnUnitUnhovered(i1, players[i1]);
                players[i].Click += () => OnPlayerClicked(i1, players[i1]);
                enemies[i].Hover += () => OnEnemyHovered(i1, enemies[i1]);
                enemies[i].Unhover += () => OnUnitUnhovered(i1, enemies[i1]);
                enemies[i].Click += () => OnEnemyClicked(i1, enemies[i1]);
            }
        }

        ICombatModel combatModel;

        private bool IsValidPlayerSelection(int index)
        {
            if (!currentFlags.HasFlag(SelectionFlags.Ally)) return false;
            if (currentFlags.HasFlag(SelectionFlags.Actionable))
            {
                if (combatModel.GetTeam(0).HasUnitTakenTurn(index))
                {
                    return false;
                }
            }

            if (currentFlags.HasFlag(SelectionFlags.Alive))
            {
                if (!combatModel.GetTeam(0).IsUnitAlive(index))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsValidEnemySelection(int index)
        {
            if (!currentFlags.HasFlag(SelectionFlags.Enemy)) return false;
            if (currentFlags.HasFlag(SelectionFlags.Actionable))
            {
                if (combatModel.GetTeam(0).HasUnitTakenTurn(index))
                {
                    return false;
                }
            }

            if (currentFlags.HasFlag(SelectionFlags.Alive))
            {
                if (!combatModel.GetTeam(0).IsUnitAlive(index))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnPlayerHovered(int index, IUnit unit)
        {
            if (!inSelectionMode) return;

            if (IsValidPlayerSelection(index))
            {
                players[index].Highlight();
                PlayerHovered?.Invoke(index, unit);
            }
        }

        private void OnEnemyHovered(int index, IUnit unit)
        {
            if (!inSelectionMode) return;
            if (IsValidEnemySelection(index))
            {
                enemies[index].Highlight();
                EnemyHovered?.Invoke(index, enemies[index]);
            }
        }

        private void OnUnitUnhovered(int index, IUnit unit)
        {
            if (!inSelectionMode)return;
            unit.Unhighlight();
        }

        private SelectionFlags currentFlags;
        private (int team, int unit) currentSelection;

        private void OnPlayerClicked(int index, IUnit unit)
        {
            if(!inSelectionMode) return;
            if (IsValidPlayerSelection(index))
            {
                currentSelection = (0, index);
                inSelectionMode = false;
                unit.Focus();
            }
        }

        private void OnEnemyClicked(int index, IUnit unit)
        {
            if(!inSelectionMode) return;
            if (IsValidEnemySelection(index))
            {
                currentSelection = (1, index);
                inSelectionMode = false;
                unit.Focus();
            }
        }


        public event IUnitSelector.UnitHovered PlayerHovered;
        public event IUnitSelector.UnitHovered EnemyHovered;

        [SerializeField] private Unit[] players;
        [SerializeField] private EnemyUnit[] enemies;

        private bool inSelectionMode = false;

        public async Task<(int team, int unit)> SelectOneAsync(SelectionFlags selectionFlags,
            CancellationToken token = default)
        {
            currentSelection = (-1, -1);
            inSelectionMode = true;
            currentFlags = selectionFlags;
            while (inSelectionMode && !token.IsCancellationRequested)
            {
                await Awaitable.NextFrameAsync(token);
            }

            inSelectionMode = false;
            currentFlags = SelectionFlags.None;
            Debug.Log($"Selected {currentSelection.team} {currentSelection.unit}");
            return currentSelection;
        }

        public async void SelectOne(SelectionFlags selectionFlags, IUnitSelector.SelectionUnitCallback callback)
        {
            try
            {
                var selection = await SelectOneAsync(selectionFlags);
                callback(selection.team, selection.unit);
            }
            catch
            {
                Debug.Log("Error occured during unit selection");
            }
        }

        public Unit[] Players => players;
        public EnemyUnit[] Enemies => enemies;



        


    }
}