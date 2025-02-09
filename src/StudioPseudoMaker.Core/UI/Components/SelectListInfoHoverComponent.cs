using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Plugins
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
