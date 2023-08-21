using UnityEngine;

namespace SobelOutline
{
    public class SobelOutline : MonoBehaviour
    {
        public float OutlineWidth;

        public Color OutlineColor;

        public float ColorPower;

        [Space(10f)]
        [Header("(Experimental)")]
        [Tooltip("[Experimental] Which layer/s should not be included")]
        public LayerMask excludeLayers = 0;

        private GameObject tmpCam = null;

        private Camera _camera;

        [HideInInspector]
        public Material _material;

        private GameObject go;

        private bool destroy = false;

        public Shader shader;

        private void Start()
        {
            _material = new Material(shader);
        }

        private void Reset()
        {
            _material = new Material(shader);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!(_material == null))
            {
                _material.SetFloat("_OutlineWidth", OutlineWidth);
                _material.SetFloat("_OutlineColorPower", ColorPower);
                _material.SetColor("_OutlineColor", OutlineColor);
                Graphics.Blit(source, destination, _material);
                Camera camera = null;
                if (excludeLayers.value != 0)
                {
                    camera = GetTmpCam();
                }
                if ((bool)camera && excludeLayers.value != 0)
                {
                    camera.targetTexture = destination;
                    camera.cullingMask = excludeLayers;
                    camera.Render();
                    destroy = true;
                }
                else if (destroy)
                {
                    Object.DestroyImmediate(GameObject.Find(tmpCam.name));
                    destroy = false;
                }
            }
        }

        private Camera GetTmpCam()
        {
            if (tmpCam == null)
            {
                if (_camera == null)
                {
                    _camera = GetComponent<Camera>();
                }
                string text = "_" + _camera.name + "_temp";
                go = GameObject.Find(text);
                if (go == null)
                {
                    tmpCam = new GameObject(text, typeof(Camera));
                    tmpCam.transform.parent = GameObject.Find(_camera.name).transform;
                }
                else
                {
                    tmpCam = go;
                }
            }
            tmpCam.hideFlags = HideFlags.DontSave;
            tmpCam.transform.position = _camera.transform.position;
            tmpCam.transform.rotation = _camera.transform.rotation;
            tmpCam.transform.localScale = _camera.transform.localScale;
            tmpCam.GetComponent<Camera>().CopyFrom(_camera);
            tmpCam.GetComponent<Camera>().enabled = false;
            tmpCam.GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
            tmpCam.GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;
            return tmpCam.GetComponent<Camera>();
        }
    }
}
