using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace HvO.UI
{
    public class UnitListUI : MonoBehaviour
    {
        private GameObject m_UnitButtonPrefab;
        private Transform m_ButtonContainer;
        private HorizontalLayoutGroup m_HorizontalLayout;

        public void Initialize(GameObject buttonPrefab)
        {
            if (buttonPrefab == null)
            {
                Debug.LogError("Button prefab is null!");
                return;
            }

            m_UnitButtonPrefab = buttonPrefab;
            
            // Usar el panel existente
            m_ButtonContainer = transform;
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
            if (m_UnitButtonPrefab == null)
            {
                Debug.LogError("UnitButton prefab is not assigned!");
                return;
            }

            // Crear el botón
            GameObject buttonObj = Instantiate(m_UnitButtonPrefab, m_ButtonContainer);
            UnitButton unitButton = buttonObj.GetComponent<UnitButton>();
            
            if (unitButton == null)
            {
                Debug.LogError("UnitButton component not found on button prefab!");
                return;
            }

            // Configurar el RectTransform del botón
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(100, 100);

            // Añadir CanvasGroup si no existe
            CanvasGroup canvasGroup = buttonObj.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = buttonObj.AddComponent<CanvasGroup>();
            }

            // Añadir imagen de fondo al botón
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage == null)
            {
                buttonImage = buttonObj.AddComponent<Image>();
            }
            buttonImage.color = new Color(0.25f, 0.25f, 0.25f, 0.95f);

            // Crear un contenedor para la imagen con tamaño fijo
            GameObject imageContainer = new GameObject("ImageContainer");
            imageContainer.transform.SetParent(buttonObj.transform, false);
            RectTransform containerRect = imageContainer.AddComponent<RectTransform>();
            
            // Configurar el contenedor para que ocupe la parte central del botón
            containerRect.anchorMin = new Vector2(0.1f, 0.2f);
            containerRect.anchorMax = new Vector2(0.9f, 0.9f);
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;

            // Crear y configurar la imagen de la unidad
            GameObject unitImageObj = new GameObject("UnitImage");
            unitImageObj.transform.SetParent(imageContainer.transform, false);
            Image unitImage = unitImageObj.AddComponent<Image>();
            RectTransform unitImageRect = unitImage.GetComponent<RectTransform>();
            
            // Configurar la imagen para que se centre y ajuste al contenedor
            unitImageRect.anchorMin = Vector2.zero;
            unitImageRect.anchorMax = Vector2.one;
            unitImageRect.sizeDelta = Vector2.zero;
            unitImageRect.anchoredPosition = Vector2.zero;
            
            // Obtener el SpriteRenderer del prefab y asignar su sprite a la imagen
            SpriteRenderer spriteRenderer = unitPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                unitImage.sprite = spriteRenderer.sprite;
                unitImage.preserveAspect = true;
                unitImage.raycastTarget = false;
            }

            // Inicializar el botón con el prefab de la unidad
            unitButton.Initialize(unitPrefab);

            // Configurar el texto del botón
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText == null)
            {
                GameObject textObj = new GameObject("Text (TMP)");
                textObj.transform.SetParent(buttonObj.transform, false);
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
}
