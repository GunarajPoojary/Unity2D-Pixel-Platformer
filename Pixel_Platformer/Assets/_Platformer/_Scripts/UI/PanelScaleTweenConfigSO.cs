using DG.Tweening;
using UnityEngine;

namespace PixelPlatformer
{
    [CreateAssetMenu(menuName = "UI/Panel Scale Tween Config", fileName = "PanelScaleTweenConfig")]
    public class PanelScaleTweenConfigSO : ScriptableObject
    {
        [Header("Panel")]
        [field: SerializeField] public float StartScale { get; private set; } = 0.8f;
        [field: SerializeField] public float OpenDuration { get; private set; } = 0.1f;
        [field: SerializeField] public float CloseDuration { get; private set; } = 0.1f;
        [field: SerializeField] public Ease OpenEase { get; private set; } = Ease.OutBack;
        [field: SerializeField] public Ease CloseEase { get; private set; } = Ease.InBack;
    }
}