using System.Collections;
using TMPro;
using UnityEngine;

public class ToastPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private float _closeDuration = 2f;

    private Coroutine _autoCloseCoroutine;

    public void Show(string message)
    {
        Show(message, _closeDuration);
    }

    public void Show(string message, float duration)
    {
        if (_messageText != null)
            _messageText.text = message;

        if (_autoCloseCoroutine != null)
            StopCoroutine(_autoCloseCoroutine);

        _autoCloseCoroutine = StartCoroutine(AutoCloseCoroutine(duration));
    }

    private IEnumerator AutoCloseCoroutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
    }
}
