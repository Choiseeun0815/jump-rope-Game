using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public UnityEvent onDown;
    public UnityEvent onUp;

    private bool _isDown;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_isDown) return;
        _isDown = true;
        onDown?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDown) return;
        _isDown = false;
        onUp?.Invoke();
    }

    // 버튼 누른 채로 밖으로 드래그하면 “뗀 것” 처리
    public void OnPointerExit(PointerEventData eventData)
    {
        OnPointerUp(eventData);
    }
}