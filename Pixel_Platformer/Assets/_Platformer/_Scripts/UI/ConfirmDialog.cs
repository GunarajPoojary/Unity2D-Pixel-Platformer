using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialog : MonoBehaviour
{
    [SerializeField] private TMP_Text _messageLabel;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;
    [SerializeField] private CanvasGroup _group;
    [SerializeField] private RectTransform _window;
    [SerializeField] private Ease _tweenEase = Ease.OutBack;

    [SerializeField] private float _tweenScale = 0.8f;
    [SerializeField] private float _fadeTime = 0.2f;

    private Tween _tween;

    private Action _confirmAction;

    private void Awake()
    {
        SetVisible(false);
    }

    private void OnEnable()
    {
        _noButton.onClick.AddListener(Close);
        _yesButton.onClick.AddListener(Confirm);
    }

    private void OnDisable()
    {
        _noButton.onClick.RemoveListener(Close);
        _yesButton.onClick.RemoveListener(Confirm);
    }

    private void OnDestroy()
    {
        _tween?.Kill();
    }

    public void Open(string message, Action onConfirm)
    {
        _confirmAction = onConfirm;
        _messageLabel.text = message;

        _tween?.Kill();
        SetVisible(true);
        _window.localScale = Vector3.one * _tweenScale;

        _tween = DOTween.Sequence()
                        .Append(_group.DOFade(1f, _fadeTime))
                        .Join(_window.DOScale(1f, _fadeTime).SetEase(_tweenEase))
                        .SetUpdate(true)
                        .SetLink(gameObject);
    }

    public void Close()
    {
        _tween?.Kill();
        _group.interactable = false;
        _group.blocksRaycasts = false;

        _tween = _group.DOFade(0f, _fadeTime)
                       .SetUpdate(true)
                       .SetLink(gameObject);
    }

    private void SetVisible(bool visible)
    {
        _group.alpha = visible ? 1f : 0f;
        _group.interactable = visible;
        _group.blocksRaycasts = visible;
    }

    private void Confirm()
    {
        Close();
        _confirmAction?.Invoke();
        _confirmAction = null;
    }
}
