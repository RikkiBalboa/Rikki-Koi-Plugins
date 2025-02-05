using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Plugins
{
    internal class ResizableWindow : UIBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public static ResizableWindow MakeObjectResizable(RectTransform clickableDragZone, RectTransform resizableObject, Vector2 minDimensions, Vector2 referenceResolution, bool preventCameraControl = true)
        {
            ResizableWindow mv = clickableDragZone.gameObject.AddComponent<ResizableWindow>();
            mv.toDrag = resizableObject;
            mv.preventCameraControl = preventCameraControl;

            mv.minDimensions = minDimensions;
            mv.referenceResolution = referenceResolution;
            return mv;
        }

        private Vector2 _cachedDragPosition;
        private Vector2 _cachedMousePosition;
        private bool _pointerDownCalled;
        private BaseCameraControl _cameraControl;
        private BaseCameraControl.NoCtrlFunc _noControlFunctionCached;

        public event Action<PointerEventData> onPointerDown;
        public event Action<PointerEventData> onDrag;
        public event Action<PointerEventData> onPointerUp;

        public RectTransform toDrag;
        public bool preventCameraControl;

        public Vector2 minDimensions;
        public Vector2 referenceResolution;

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
            _cachedDragPosition = toDrag.position;
            _cachedMousePosition = Input.mousePosition;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, data.position, data.pressEventCamera, out previousPointerPosition);
            onPointerDown?.Invoke(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_pointerDownCalled == false)
                return;

            toDrag.offsetMin = new Vector2(toDrag.offsetMin.x, Input.mousePosition.y * (referenceResolution.y / Screen.height));
            toDrag.offsetMax = new Vector2(Input.mousePosition.x * ((float)Screen.width / Screen.height * referenceResolution.y / Screen.width), toDrag.offsetMax.y);
            onDrag?.Invoke(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_pointerDownCalled == false)
                return;
            if (preventCameraControl && _cameraControl)
                _cameraControl.NoCtrlCondition = _noControlFunctionCached;
            _pointerDownCalled = false;
            onPointerUp?.Invoke(eventData);
        }
    }
}