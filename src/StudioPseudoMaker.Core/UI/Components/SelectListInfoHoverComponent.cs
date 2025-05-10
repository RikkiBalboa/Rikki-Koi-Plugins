using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PseudoMaker.UI
{
    internal class SelectListInfoHoverComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public Action onEnterAction;
        public Action onExitAction;
        public Action<PointerEventData> onRightClickAction;

        public void OnPointerEnter(PointerEventData eventData)
        {
            onEnterAction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onExitAction();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
                onRightClickAction(eventData);
        }
    }
}
