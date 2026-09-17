using UnityEngine;
using UnityEngine.UI;

namespace PixelPlatformer
{
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Button _guideButton;
        [SerializeField] private GuideUIManager _guideUIManager;
        private Vector3 _guideButtonAnchorPos;

        private void Awake()
        {
            _guideButtonAnchorPos = _guideButton.GetComponent<RectTransform>().anchoredPosition;
        }

        private void OnEnable()
        {
            _guideButton.onClick.AddListener(HandleGuideClick);
        }

        private void OnDisable()
        {
            _guideButton.onClick.RemoveListener(HandleGuideClick);
        }

        private void HandleGuideClick()
        {
            _guideUIManager.ToggleGuide(_guideButtonAnchorPos);
        }
    }
}