using System.Collections;
using UnityEngine;

public class BlinkingSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private Color initialColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        initialColor = spriteRenderer.color;
    }

    private void OnEnable()
    {
        StartCoroutine(Blink());
    }

    private void OnDisable()
    {
        StopCoroutine(Blink());
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            SetAlpha(0f);
            float seconds = Random.Range(2f, 5f);
            yield return new WaitForSeconds(seconds);
            SetAlpha(1f);
            yield return new WaitForSeconds(0.2f);
            SetAlpha(0f);
        }
    }

    private void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}