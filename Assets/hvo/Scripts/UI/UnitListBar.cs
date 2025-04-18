using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Serialization;

namespace HvO.UI
{
    public class UnitListBar : MonoBehaviour
    {
        private UnitElementBar m_UnitElementPrefab;
        private Transform m_PanelContainer;
        private HorizontalLayoutGroup m_HorizontalLayout;

        public void Initialize(UnitElementBar unitElementPrefab)
        {
            if (unitElementPrefab == null)
            {
                Debug.LogError("Button prefab is null!");
                return;
            }
            
            m_UnitElementPrefab = unitElementPrefab;

            // Usar el panel existente
            m_PanelContainer = transform;
            m_HorizontalLayout = GetComponent<HorizontalLayoutGroup>();

            if (m_HorizontalLayout == null)
            {
                Debug.LogError("No se encontró HorizontalLayoutGroup en el panel!");
                return;
            }
        }

        public void AddUnit(GameObject unitPrefab)
        {
            if (unitPrefab == null)
            {
                Debug.LogError("Unit prefab is null!");
                return;
            }

            CreateUnitButton(unitPrefab);
        }

        private void CreateUnitButton(GameObject unitPrefab)
        {
            if (!ValidatePrefabs(unitPrefab)) return;

            GameObject buttonObj = Instantiate(m_UnitElementPrefab.transform.gameObject, m_PanelContainer);
            UnitElementBar unitButton = GetUnitElementBar(buttonObj);
            if (unitButton == null) return;

            SetupButtonUI(buttonObj);
            GameObject imageContainer = CreateImageContainer(buttonObj.transform);
            CreateUnitImage(imageContainer.transform, unitPrefab);
            SetupButtonText(buttonObj.transform);

            unitButton.Initialize(unitPrefab);
        }

        private bool ValidatePrefabs(GameObject unitPrefab)
        {
            if (m_UnitElementPrefab == null)
            {
                Debug.LogError("UnitButton prefab is not assigned!");
                return false;
            }

            if (unitPrefab == null)
            {
                Debug.LogError("Unit prefab is null!");
                return false;
            }

            return true;
        }

        private UnitElementBar GetUnitElementBar(GameObject buttonObj)
        {
            UnitElementBar unitButton = buttonObj.GetComponent<UnitElementBar>();
            if (unitButton == null)
            {
                Debug.LogError("UnitButton component not found on button prefab!");
            }

            return unitButton;
        }

        private void SetupButtonUI(GameObject buttonObj)
        {
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100, 100);

            if (buttonObj.GetComponent<CanvasGroup>() == null)
                buttonObj.AddComponent<CanvasGroup>();

            Image buttonImage = buttonObj.GetComponent<Image>() ?? buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.25f, 0.25f, 0.25f, 0.95f);
        }

        private GameObject CreateImageContainer(Transform parent)
        {
            GameObject imageContainer = new GameObject("ImageContainer");
            imageContainer.transform.SetParent(parent, false);

            RectTransform containerRect = imageContainer.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(0.1f, 0.2f);
            containerRect.anchorMax = new Vector2(0.9f, 0.9f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            return imageContainer;
        }

        private void CreateUnitImage(Transform parent, GameObject unitPrefab)
        {
            GameObject unitImageObj = new GameObject("UnitImage");
            unitImageObj.transform.SetParent(parent, false);

            Image unitImage = unitImageObj.AddComponent<Image>();
            RectTransform unitImageRect = unitImage.GetComponent<RectTransform>();
            unitImageRect.anchorMin = Vector2.zero;
            unitImageRect.anchorMax = Vector2.one;
            unitImageRect.sizeDelta = Vector2.zero;
            unitImageRect.anchoredPosition = Vector2.zero;

            SpriteRenderer spriteRenderer = unitPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                unitImage.sprite = spriteRenderer.sprite;
                unitImage.preserveAspect = true;
                unitImage.raycastTarget = false;
            }
        }

        private void SetupButtonText(Transform parent)
        {
            TextMeshProUGUI buttonText = parent.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null) return;

            GameObject textObj = new GameObject("Text (TMP)");
            textObj.transform.SetParent(parent, false);

            buttonText = textObj.AddComponent<TextMeshProUGUI>();
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 0.2f);
            textRect.offsetMin = new Vector2(2, 2);
            textRect.offsetMax = new Vector2(-2, 0);

            buttonText.fontSize = 14;
            buttonText.alignment = TextAlignmentOptions.Center;
            buttonText.color = new Color(1, 1, 1, 0.9f);
            buttonText.overflowMode = TextOverflowModes.Truncate;
            buttonText.raycastTarget = false;
        }
    }
}