using System;
using TMPro;
using UnityEngine;

namespace hvo.Scripts.Utils
{
    public class BattleTimer : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI m_TimerText;
        [SerializeField] private float m_Duration = 90f;

        public event Action OnTimerExpired;

        private float m_Remaining;
        private bool m_Running;

        public float Elapsed => m_Duration - m_Remaining;

        public void StartTimer()
        {
            m_Remaining = m_Duration;
            m_Running = true;
        }

        public void StopTimer()
        {
            m_Running = false;
        }

        private void Update()
        {
            if (!m_Running) return;

            m_Remaining -= Time.deltaTime;
            UpdateDisplay();

            if (m_Remaining <= 0f)
            {
                m_Remaining = 0f;
                m_Running = false;
                OnTimerExpired?.Invoke();
            }
        }

        private void UpdateDisplay()
        {
            if (m_TimerText == null) return;
            int seconds = Mathf.CeilToInt(m_Remaining);
            m_TimerText.text = $"{seconds / 60:00}:{seconds % 60:00}";
            m_TimerText.color = m_Remaining <= 10f ? Color.red : Color.white;
        }
    }
}
