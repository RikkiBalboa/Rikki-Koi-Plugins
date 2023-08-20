using UnityEngine;

namespace PostProcessingEffectsV3
{
	[ExecuteInEditMode]
	[ImageEffectAllowedInSceneView]
	public class SengaEffect : MonoBehaviour
	{
		public float SampleDistance = 1f;

		public float NormalThreshold = 0.1f;

		public float DepthThreshold = 5f;

		public float ColorThreshold = 1f;

		public float SobelThreshold = 5f;

		public float NormalEdge = 0.5f;

		public float DepthEdge = 1f;

		public float ColorEdge = 0.3f;

		public float SobelEdge = 0.3f;

		public float kosa = 5f;

		public float toneThres = 0.5f;

		public float scale = 1f;

		public Texture2D ToneTex;

		public bool SengaOnly = false;

		public bool ToneOn = false;

		public float ToneThick = 1f;

		public float dir = 60f;

		public float pow = 1f;

		public float thick = 1f;

		public int SampleCount = 4;

		public bool BlurOn = false;

		private Material material;

		public Shader shader;

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
			if (!shader || !material)
			{
				Graphics.Blit(source, destination);
				return;
			}
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
			RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
			material.SetFloat("_SampleDistance", SampleDistance);
			material.SetVector("_Threshold", new Vector4(NormalThreshold, DepthThreshold, ColorThreshold, SobelThreshold));
			material.SetVector("_EdgePower", new Vector4(NormalEdge, DepthEdge, ColorEdge, SobelEdge));
			material.SetTexture("_MainTex", source);
			Graphics.Blit(source, temporary, material, 0);
			if (SengaOnly)
			{
				material.EnableKeyword("_SengaOnly");
			}
			else
			{
				material.DisableKeyword("_SengaOnly");
			}
			ToneTex.filterMode = FilterMode.Point;
			ToneTex.wrapMode = TextureWrapMode.Repeat;
			ToneTex.Apply();
			material.SetTexture("_MainTex", source);
			material.SetTexture("_SengaTex", temporary);
			material.SetTexture("_ToneTex", ToneTex);
			material.SetFloat("_Thres", toneThres);
			material.SetFloat("_Scale", scale);
			material.SetFloat("_kosa", kosa);
			material.SetFloat("_ToneThick", ToneThick);
			if (ToneOn)
			{
				material.EnableKeyword("_ToneOn");
			}
			else
			{
				material.DisableKeyword("_ToneOn");
			}
			if (!BlurOn)
			{
				Graphics.Blit(source, destination, material, 1);
			}
			else
			{
				Graphics.Blit(source, temporary2, material, 1);
				material.SetTexture("_MainTex", temporary2);
				material.SetFloat("_dir", dir);
				material.SetFloat("_pow", pow);
				material.SetFloat("_thick", thick);
				material.SetInt("SampleCount", SampleCount);
				Graphics.Blit(temporary2, destination, material, 2);
			}
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
		}
	}
}
