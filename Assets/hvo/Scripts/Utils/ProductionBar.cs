using UnityEngine;

namespace hvo.Scripts.Utils
{
    /// <summary>
    /// Lightweight world-space progress bar built from sprite quads (no prefab/Canvas needed).
    /// Reusable by any building that produces something over time — houses, farms, mills.
    /// </summary>
    public class ProductionBar
    {
        private const float Width = 1.0f;
        private const float Height = 0.16f;

        private static Sprite s_Sprite;

        private readonly GameObject m_Root;
        private readonly Transform m_Fill;

        public ProductionBar(Transform parent, float localY)
        {
            m_Root = new GameObject("ProductionBar");
            m_Root.transform.SetParent(parent, false);
            m_Root.transform.localPosition = new Vector3(0f, localY, 0f);

            CreateQuad(m_Root.transform, new Color(0f, 0f, 0f, 0.6f), 60, out _);              // background
            CreateQuad(m_Root.transform, new Color(0.3f, 0.9f, 0.4f, 1f), 61, out m_Fill);     // fill

            SetProgress(0f);
            SetVisible(false);
        }

        public void SetVisible(bool visible)
        {
            if (m_Root != null) m_Root.SetActive(visible);
        }

        // Grows the fill from the left edge (sprites scale from center, so we also shift it).
        public void SetProgress(float t)
        {
            t = Mathf.Clamp01(t);
            float w = Width * t;
            m_Fill.localScale = new Vector3(w, Height, 1f);
            m_Fill.localPosition = new Vector3(-Width / 2f + w / 2f, 0f, 0f);
        }

        public void Destroy()
        {
            if (m_Root != null) Object.Destroy(m_Root);
        }

        private static void CreateQuad(Transform parent, Color color, int order, out Transform t)
        {
            var go = new GameObject("Bar");
            go.transform.SetParent(parent, false);
            go.transform.localScale = new Vector3(Width, Height, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSprite();
            sr.color = color;
            sr.sortingOrder = order;
            t = go.transform;
        }

        private static Sprite GetSprite()
        {
            if (s_Sprite == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                s_Sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            }
            return s_Sprite;
        }
    }
}
