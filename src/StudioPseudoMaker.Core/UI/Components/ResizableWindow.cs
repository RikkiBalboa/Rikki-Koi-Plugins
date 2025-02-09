using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Plugins
{
    internal class ResizableWindow : UIBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static ResizableWindow MakeObjectResizable(RectTransform clickableDragZone, RectTransform resizableObject, Vector2 minDimensions, CanvasScaler canvasScaler, bool preventCameraControl = true, Action onResize = null)
        {
            ResizableWindow mv = clickableDragZone.gameObject.AddComponent<ResizableWindow>();
            mv.toResize = resizableObject;
            mv.preventCameraControl = preventCameraControl;

            mv.minDimensions = minDimensions;
            mv.canvasScaler = canvasScaler;
            mv.onResize = onResize;
            return mv;
        }

        private Vector2 _cachedDragPosition;
        private Vector2 _cachedMousePosition;
        private bool _pointerDownCalled;
        private BaseCameraControl _cameraControl;
        private BaseCameraControl.NoCtrlFunc _noControlFunctionCached;

        private Action onResize;

        public RectTransform toResize;
        public bool preventCameraControl;

        public Vector2 minDimensions;
        public CanvasScaler canvasScaler;

        public override void Awake()
        {
            base.Awake();
            _cameraControl = FindObjectOfType<BaseCameraControl>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (preventCameraControl && _cameraControl)
            {
                _noControlFunctionCached = _cameraControl.NoCtrlCondition;
                _cameraControl.NoCtrlCondition = () => true;
            }
            _pointerDownCalled = true;
            _cachedDragPosition = toResize.position;
            _cachedMousePosition = Input.mousePosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_pointerDownCalled == false)
                return;

            var newHeightOffset = Mathf.Clamp(Input.mousePosition.y * (canvasScaler.referenceResolution.y / Screen.height), 0, toResize.offsetMax.y - minDimensions.y);
            var newWidthOffset = Mathf.Clamp(Input.mousePosition.x * ((float)Screen.width / Screen.height * canvasScaler.referenceResolution.y / Screen.width), toResize.offsetMin.x + minDimensions.x, 9999);

            toResize.offsetMin = new Vector2(toResize.offsetMin.x, newHeightOffset);
            toResize.offsetMax = new Vector2(newWidthOffset, toResize.offsetMax.y);


            if (onResize != null)
                onResize();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointerDownCalled == false)
                return;
            if (preventCameraControl && _cameraControl)
                _cameraControl.NoCtrlCondition = _noControlFunctionCached;
            _pointerDownCalled = false;
        }
    }
}