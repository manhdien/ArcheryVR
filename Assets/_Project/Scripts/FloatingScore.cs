using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Hiển thị điểm số bay (+10, HIT!) tại vị trí bắn trúng và mờ dần biến mất.
/// Task 6: Chỉ hiện +10 với màu vàng và "HIT!" — không phân zone nữa.
/// </summary>
public class FloatingScore : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private float floatSpeed = 60f;
    [SerializeField] private float duration = 0.9f;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (scoreText == null) scoreText = GetComponentInChildren<TMP_Text>();
    }

    /// <summary>
    /// Khởi tạo nội dung, màu sắc và vị trí hiển thị điểm số.
    /// </summary>
    public void Init(int score, string hitType, Vector3 worldHitPoint)
    {
        if (scoreText != null)
        {
            scoreText.text = $"+{score}\n<size=70%>HIT!</size>";
            scoreText.color = new Color(1f, 0.9f, 0.2f, 1f); // Vàng tươi cho mọi phát trúng
        }

        // Định vị trên màn hình từ tọa độ thế giới
        Camera cam = Camera.main;
        if (cam != null && worldHitPoint != Vector3.zero)
        {
            Vector3 screenPos = cam.WorldToScreenPoint(worldHitPoint);
            if (screenPos.z > 0)
            {
                if (rectTransform != null)
                {
                    rectTransform.position = screenPos;
                }
            }
        }

        StartCoroutine(AnimateAndDestroy());
    }

    private IEnumerator AnimateAndDestroy()
    {
        float timer = 0f;
        Color initialColor = scoreText != null ? scoreText.color : Color.white;

        while (timer < duration)
        {
            float dt = Time.unscaledDeltaTime;
            timer += dt;
            float progress = timer / duration;

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition += Vector2.up * (floatSpeed * dt);
            }

            if (scoreText != null)
            {
                float alpha = Mathf.Lerp(1f, 0f, progress);
                scoreText.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
