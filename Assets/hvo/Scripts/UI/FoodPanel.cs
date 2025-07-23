using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace HvO.UI
{
    public class FoodPanel : MonoBehaviour
    {
        
        [SerializeField] private Button m_CloseButton;

        
        public void Show()
        {
            gameObject.SetActive(true);
        }
        
        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void SetupHooks(UnityAction onClose)
        {
            UnsubscribeAll();
            m_CloseButton.onClick.AddListener(onClose);
        }

        void UnsubscribeAll()
        {
            m_CloseButton.onClick.RemoveAllListeners();
        }
        
        void OnDisable()
        {
            UnsubscribeAll();
        }
    }
}