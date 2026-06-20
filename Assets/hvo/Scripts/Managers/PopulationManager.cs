using System;
using UnityEngine;

namespace hvo.Scripts.Managers
{
    /// <summary>
    /// Tracks the village's maximum population (housing capacity). Current population is derived
    /// from the living villager units, so it never desyncs on death. Plain class owned by the
    /// game manager — same pattern as <see cref="ResourceManager"/>, no scene-lookup fragility.
    /// </summary>
    public class PopulationManager
    {
        public int MaxPopulation { get; private set; }

        /// <summary>Raised when the housing capacity changes (a house/castle is built or removed).</summary>
        public event Action OnChanged;

        public void IncreaseMax(int amount)
        {
            if (amount == 0) return;
            MaxPopulation = Mathf.Max(0, MaxPopulation + amount);
            OnChanged?.Invoke();
        }
    }
}
