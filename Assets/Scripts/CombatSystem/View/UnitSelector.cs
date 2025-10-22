using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;

namespace CombatSystem.View
{
    public class UnitSelector : MonoBehaviour, IUnitSelector
    {
        private readonly Queue<(SelectionFlags flags, IUnitSelector.SelectionUnitCallback callback)> requests = new();
        private CombatManager combatManager;
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
            combatManager = GetComponent<CombatManager>();
        }

        ICombatModel combatModel => combatManager.CombatModel;

        private bool IsValidPlayerSelection(int index)
        {
            SelectionFlags currentFlags = requests.Peek().flags;
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
            SelectionFlags currentFlags = requests.Peek().flags;
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
            if(requests.Count == 0) return;
            if (IsValidPlayerSelection(index))
            {
                players[index].Highlight();
                Debug.Log($"Player {index} hovered");
                PlayerHovered?.Invoke(index, unit);
            }
        }

        private void OnEnemyHovered(int index, IUnit unit)
        {
            if(requests.Count == 0) return;
            if (IsValidEnemySelection(index))
            {
                enemies[index].Highlight();
                Debug.Log($"Enemy {index} hovered");
                EnemyHovered?.Invoke(index, enemies[index]);
            }
        }

        private void OnUnitUnhovered(int index, IUnit unit)
        {
            if (requests.Count == 0)return;
            unit.Unhighlight();
        }

        private void OnPlayerClicked(int index, IUnit unit)
        {
            if(requests.Count == 0) return;
            if (IsValidPlayerSelection(index))
            {
                requests.Dequeue().callback(0,index);
                Debug.Log($"Selected 0 , {index}");
                unit.Focus();
            }
        }

        private void OnEnemyClicked(int index, IUnit unit)
        {
            if(requests.Count == 0) return;
            if (IsValidEnemySelection(index))
            {
                Debug.Log($"Selected 1 , {index}");
                requests.Dequeue().callback(1,index);
                unit.Focus();
            }
        }


        public event IUnitSelector.UnitHovered PlayerHovered;
        public event IUnitSelector.UnitHovered EnemyHovered;

        [SerializeField] private Unit[] players;
        [SerializeField] private EnemyUnit[] enemies;

        public async Task<(int team, int unit)> SelectOneAsync(SelectionFlags selectionFlags,
            CancellationToken token = default)
        {
            bool inSelectionMode = true;
            (int team, int unit) selection = (-1, -1);
            SelectOne(selectionFlags,(team, unit)=>
            {
                inSelectionMode = false;
                selection = (team, unit);
            });
            while (inSelectionMode && !token.IsCancellationRequested)
            {
                await Awaitable.NextFrameAsync(token);
            }
            Debug.Log($"Selected {selection.team} {selection.unit}");
            return selection;
        }

        public void SelectOne(SelectionFlags selectionFlags, IUnitSelector.SelectionUnitCallback callback)
        {
            requests.Enqueue((selectionFlags, callback));
        }

        public void ClearPlayersSelection()
        {
            foreach (var player in players)
            {
                player.Unhighlight();
            }
        }

        public void ClearEnemiesSelection()
        {
            foreach (var enemy in enemies)
            {
                enemy.Unhighlight();
            }
        }

        public Unit[] Players => players;
        public EnemyUnit[] Enemies => enemies;



        


    }
}