using UnityEngine;

namespace PostProcessingEffectsV3
{
	public class Posterize : MonoBehaviour
	{
		private Material material;

		public Shader shader;

		public int _div = 4;

		public bool _HSV = true;

		private void Start()
		{
			material = new Material(shader);
		}

		private void OnEnable()
		{
			GetComponent<Camera>().depthTextureMode |= DepthTextureMode.DepthNormals;
		}

		private void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			if (!shader)
			{
				Graphics.Blit(source, destination);
				return;
			}
			material.SetInt("_div", _div);
			if (_HSV)
			{
				material.EnableKeyword("_HSV_ON");
			}
			else
			{
				material.DisableKeyword("_HSV_ON");
			}
			Graphics.Blit(source, destination, material);
		}
	}
}
