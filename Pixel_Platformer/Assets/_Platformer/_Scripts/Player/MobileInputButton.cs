using UnityEngine;
using UnityEngine.EventSystems;

public class MobileInputButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool _pressed;
    private bool _pressedThisFrame;
    private bool _releasedThisFrame;

    public bool Pressed { get { return _pressed; } }
    public bool PressedThisFrame { get { return _pressedThisFrame; } }
    public bool ReleasedThisFrame { get { return _releasedThisFrame; } }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        _pressedThisFrame = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        _releasedThisFrame = true;
    }

    private void LateUpdate()
    {
        _pressedThisFrame = false;
        _releasedThisFrame = false;
    }
}