using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    [CreateAssetMenu(menuName = "Button Feedback Config", fileName = "Configs/Button Feedback")]
    public class ButtonFeedbackConfigSO : ScriptableObject
    {
        [field: SerializeField] public AudioClip ClickClip { get; private set; }

        [Header("Scale")]
        [field: SerializeField] public float PressedScale { get; private set; } = 0.9f;
        [field: SerializeField] public float PressDuration { get; private set; } = 0.08f;
        [field: SerializeField] public float ReleaseDuration { get; private set; } = 0.1f;
        [field: SerializeField] public Ease PressEase { get; private set; } = Ease.OutQuad;
        [field: SerializeField] public Ease ReleaseEase { get; private set; } = Ease.OutBack;
    }
}