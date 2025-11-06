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
                players[i].Unhover += () => OnUnitUnhovered(0, i1, players[i1]);
                players[i].Click += () => OnPlayerClicked(i1, players[i1]);
                enemies[i].Hover += () => OnEnemyHovered(i1, enemies[i1]);
                enemies[i].Unhover += () => OnUnitUnhovered(0, i1, enemies[i1]);
                enemies[i].Click += () => OnEnemyClicked(i1, enemies[i1]);
            }

            combatManager = GetComponent<CombatManager>();
        }

        // ICombatModel combatModel => combatManager.CombatModel;

        public void ManualSelect(int team, int unit)
        {
            if (team == 0)
            {
                OnPlayerClicked(unit, players[unit]);
            }
            else
            {
                OnEnemyClicked(unit, enemies[unit]);
            }
        }

        private bool IsValidPlayerSelection(int index, SelectionFlags selectionFlags)
        {
            return combatManager.TrySelectUnit(0, 0, index, selectionFlags, out var selected);

            /*
            if (!selectionFlags.HasFlag(SelectionFlags.Ally)) return false;
            if (selectionFlags.HasFlag(SelectionFlags.Actionable))
            {
                if (combatModel.GetTeam(0).HasUnitTakenTurn(index))
                {
                    return false;
                }
            }

            if (selectionFlags.HasFlag(SelectionFlags.Alive))
            {
                if (!combatModel.GetTeam(0).IsUnitAlive(index))
                {
                    return false;
                }
            }

            return true;
            */
        }

        private bool IsValidEnemySelection(int index, SelectionFlags selectionFlags)
        {
            return combatManager.TrySelectUnit(0, 1, index, selectionFlags, out var selected);

            /*
            if (!selectionFlags.HasFlag(SelectionFlags.Enemy)) return false;
            if (selectionFlags.HasFlag(SelectionFlags.Actionable))
            {
                if (combatModel.GetTeam(0).HasUnitTakenTurn(index))
                {
                    return false;
                }
            }

            if (selectionFlags.HasFlag(SelectionFlags.Alive))
            {
                if (!combatModel.GetTeam(1).IsUnitAlive(index))
                {
                    return false;
                }
            }

            return true;
            */
        }

        private void OnPlayerHovered(int index, IUnit unit)
        {
            PlayerHovered?.Invoke(index, unit);
            if (requests.Count == 0) return;
            if (IsValidPlayerSelection(index, requests.Peek().flags))
            {
                players[index].Highlight();
                // Debug.Log($"Player {index} hovered");
                SelectablePlayerHovered?.Invoke(index, unit);
            }
        }

        private void OnEnemyHovered(int index, IUnit unit)
        {
            EnemyHovered?.Invoke(index, unit);
            if (requests.Count == 0) return;
            if (IsValidEnemySelection(index, requests.Peek().flags))
            {
                enemies[index].Highlight();
                // Debug.Log($"Enemy {index} hovered");
                SelectableEnemyHovered?.Invoke(index, enemies[index]);
            }
        }

        private void OnUnitUnhovered(int team, int unit_index, IUnit unit)
        {
            if (requests.Count == 0) return;
            unit.Unhighlight();
            if (team == 0)
            {
                PlayerUnhovered?.Invoke(unit_index, unit);
            }
            else
            {
                EnemyUnhovered?.Invoke(unit_index, unit);
            }
        }

        private void OnPlayerClicked(int index, IUnit unit)
        {
            if (requests.Count == 0) return;
            if (IsValidPlayerSelection(index, requests.Peek().flags))
            {
                requests.Dequeue().callback(0, index);
                Debug.Log($"Selected 0 , {index}");
                unit.Focus();
            }
        }

        private void OnEnemyClicked(int index, IUnit unit)
        {
            if (requests.Count == 0) return;
            if (IsValidEnemySelection(index, requests.Peek().flags))
            {
                // Debug.Log($"Selected 1 , {index}");
                requests.Dequeue().callback(1, index);
                unit.Focus();
            }
        }


        public event IUnitSelector.UnitHovered SelectablePlayerHovered;
        public event IUnitSelector.UnitHovered PlayerHovered;
        public event IUnitSelector.UnitHovered PlayerUnhovered;
        public event IUnitSelector.UnitHovered SelectableEnemyHovered;
        public event IUnitSelector.UnitHovered EnemyHovered;
        public event IUnitSelector.UnitHovered EnemyUnhovered;

        [SerializeField] private Unit[] players;
        [SerializeField] private EnemyUnit[] enemies;

        public async Task<(int team, int unit)> SelectOneAsync(SelectionFlags selectionFlags,
            CancellationToken token = default)
        {
            bool inSelectionMode = true;
            (int team, int unit) selection = (-1, -1);
            SelectOne(selectionFlags, (team, unit) =>
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

        public List<(int team, int unit)> SelectAll(SelectionFlags selectionFlags)
        {
            List<(int team, int unit)> result = new List<(int team, int unit)>();
            for (int i = 0; i < players.Length; i++)
            {
                if (!IsValidPlayerSelection(i, selectionFlags))
                {
                    continue;
                }

                players[i].Focus();
                result.Add((0, i));
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                if (!IsValidEnemySelection(i, selectionFlags))
                {
                    continue;
                }

                enemies[i].Focus();
                result.Add((1, i));
            }

            return result;
        }


        public void HighlightAll(SelectionFlags selectionFlags)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (!IsValidPlayerSelection(i, selectionFlags))
                {
                    continue;
                }

                players[i].Highlight();
            }

            for (int i = 0; i < enemies.Length; i++)
            {
                if (!IsValidEnemySelection(i, selectionFlags))
                {
                    continue;
                }

                enemies[i].Highlight();
            }
        }

        public void ClearRequests()
        {
            while (requests.Count > 0)
            {
                requests.Dequeue().callback(-1, -1);
            }
        }

        public void ClearPlayersSelection()
        {
            foreach (var player in players)
            {
                player.Unselect();
            }
        }

        public void ClearEnemiesSelection()
        {
            foreach (var enemy in enemies)
            {
                enemy.Unselect();
            }
        }

        public Unit[] Players => players;
        public EnemyUnit[] Enemies => enemies;
    }
}