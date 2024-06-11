using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollBarInteraction : Scrollbar
{
    public event Action OnSelected;
    public event Action OnDeselected;
    public override void OnSelect(BaseEventData eventData)
    {
        base.OnSelect(eventData);
        OnSelected?.Invoke();
    }
    public override void OnDeselect(BaseEventData eventData)
    {
        base.OnDeselect(eventData);
        OnDeselected?.Invoke();
    }
}
