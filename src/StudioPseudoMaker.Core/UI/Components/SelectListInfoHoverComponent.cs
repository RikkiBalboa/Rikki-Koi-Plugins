using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PseudoMaker.UI
{
    internal class SelectListInfoHoverComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public Action onEnterAction;
        public Action onExitAction;

        public void OnPointerEnter(PointerEventData eventData)
        {
            onEnterAction();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            onExitAction();
        }
    }
}
