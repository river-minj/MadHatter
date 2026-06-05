using System.Collections;
using Spine.Unity;
using UnityEngine;

public class HitFlashEffect : MonoBehaviour
{
    [SerializeField] private Color _flashColor = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float _flashDuration = 0.12f;

    private SpriteRenderer _spriteRenderer;
    private SkeletonAnimation _skeletonAnimation;
    private Color _originalColor;
    private Coroutine _flashCoroutine;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _skeletonAnimation = GetComponent<SkeletonAnimation>();

        if (_spriteRenderer != null)
            _originalColor = _spriteRenderer.color;
        else if (_skeletonAnimation != null)
            _originalColor = new Color(_skeletonAnimation.Skeleton.R, _skeletonAnimation.Skeleton.G,
                                       _skeletonAnimation.Skeleton.B, _skeletonAnimation.Skeleton.A);
        else
            _originalColor = Color.white;
    }

    public void Flash()
    {
        if (_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);
        _flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        SetColor(_flashColor);
        yield return new WaitForSeconds(_flashDuration);
        SetColor(_originalColor);
        _flashCoroutine = null;
    }

    private void SetColor(Color color)
    {
        if (_spriteRenderer != null)
            _spriteRenderer.color = color;
        else if (_skeletonAnimation != null)
            _skeletonAnimation.Skeleton.SetColor(color);
    }
}
