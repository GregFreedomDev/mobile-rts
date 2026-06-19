using System;
using System.Collections.Generic;
using UnityEngine;

namespace hvo.Scripts.Managers
{
    /// <summary>
    /// Single source of truth for the player's resources. Kept as a plain class (not a
    /// MonoBehaviour singleton) and owned by the game manager, so there is no scene-lookup
    /// fragility. Amounts never go below zero.
    /// </summary>
    public class ResourceManager
    {
        private readonly Dictionary<ResourceType, int> m_Amounts = new();

        /// <summary>Raised whenever any resource amount changes; UI can subscribe to refresh.</summary>
        public event Action OnChanged;

        public int GetAmount(ResourceType type)
            => m_Amounts.TryGetValue(type, out var amount) ? amount : 0;

        public bool Has(ResourceType type, int amount) => GetAmount(type) >= amount;

        public void Add(ResourceType type, int amount)
        {
            if (amount == 0) return;
            m_Amounts[type] = Mathf.Max(0, GetAmount(type) + amount);
            OnChanged?.Invoke();
        }

        /// <summary>Spends the amount only if there is enough; returns whether it succeeded.</summary>
        public bool TrySpend(ResourceType type, int amount)
        {
            if (!Has(type, amount)) return false;
            Add(type, -amount);
            return true;
        }

        /// <summary>Atomically spends several resources: spends nothing unless all are affordable.</summary>
        public bool TrySpend(IReadOnlyList<ResourceCost> costs)
        {
            if (costs == null) return true;

            for (int i = 0; i < costs.Count; i++)
                if (!Has(costs[i].Type, costs[i].Amount)) return false;

            for (int i = 0; i < costs.Count; i++)
                Add(costs[i].Type, -costs[i].Amount);

            return true;
        }
    }
}
