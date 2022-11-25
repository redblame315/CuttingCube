using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace PwhSoft.Glowing_Swords_V1.Scripts.Scripts
{
	[ExecuteInEditMode, ImageEffectAllowedInSceneView]
	public class CameraBloomEffect : MonoBehaviour 
	{
		#region Constants

		private const int BoxDownPrefilterPass = 0;
		private const int BoxDownPass = 1;
		private const int BoxUpPass = 2;
		private const int ApplyBloomPass = 3;
		private const int DebugBloomPass = 4;

		#endregion

		#region Private members
	
		private readonly RenderTexture[] _textures = new RenderTexture[16];
	
		[NonSerialized] 
		private Material _bloom;

		#region Cached Shader Property Ids

		private static readonly int Filter = Shader.PropertyToID("_Filter");
		private static readonly int Intensity = Shader.PropertyToID("_Intensity");
		private static readonly int SourceTex = Shader.PropertyToID("_SourceTex");

		#endregion

		#endregion

		#region Inspector

	
		[SerializeField]
		private Shader bloomShader;

		[Range(0, 10)]
		[SerializeField]
		public float intensity = 0.6f;

		[Range(1, 16)]
		[SerializeField]
		public int iterations = 8;

		[Range(0, 10)]
		[SerializeField]
		public float threshold = 0.9f;

		[Range(0, 1)]
		[SerializeField]
		public float softThreshold = 0f;

		/// <summary>
		/// If enabled the script will automatically enable the HDR mode for the current graphics tier.
		/// This option was implemented in order to be able to use this asset on mobile devices without problems,
		/// as HDR is deactivated by default.
		/// </summary>
		[FormerlySerializedAs("Force activate HDR")]
		[SerializeField]
		public bool forceActivateHdr = true;
	
		[SerializeField]
		private bool DebugMode = false;

		#endregion
	
		private void OnRenderImage (RenderTexture source, RenderTexture destination) {
			InitBloomIfNecessary();

			var thresholdProduct = threshold * softThreshold;
			var filter = CreateImageFilter(thresholdProduct);
		
			UpdateShaderFilterAndIntensity(filter);

			var width = source.width / 2;
			var height = source.height / 2;
			var format = source.format;

			var currentDestination = _textures[0] =
				RenderTexture.GetTemporary(width, height, 0, format);
			Graphics.Blit(source, currentDestination, _bloom, BoxDownPrefilterPass);
			var currentSource = currentDestination;

			var i = 1;
			for (; i < iterations; i++) {
				width /= 2;
				height /= 2;
				if (height < 2) {
					break;
				}
				currentDestination = _textures[i] =
					RenderTexture.GetTemporary(width, height, 0, format);
				Graphics.Blit(currentSource, currentDestination, _bloom, BoxDownPass);
				currentSource = currentDestination;
			}

			for (i -= 2; i >= 0; i--) {
				currentDestination = _textures[i];
				_textures[i] = null;
				Graphics.Blit(currentSource, currentDestination, _bloom, BoxUpPass);
				RenderTexture.ReleaseTemporary(currentSource);
				currentSource = currentDestination;
			}

			if (DebugMode) {
				Graphics.Blit(currentSource, destination, _bloom, DebugBloomPass);
			}
			else {
				_bloom.SetTexture(SourceTex, source);
				Graphics.Blit(currentSource, destination, _bloom, ApplyBloomPass);
			}
			RenderTexture.ReleaseTemporary(currentSource);
		}

		/// <summary>
		/// Initializes bloom if necessary.
		/// </summary>
		private void InitBloomIfNecessary()
		{
			if (_bloom == null)
			{
				_bloom = new Material(bloomShader)
				{
					hideFlags = HideFlags.HideAndDontSave
				};
			}
		}

		/// <summary>
		/// Sets the filter and intensity of the shader.
		/// </summary>
		/// <param name="filter">The vector 4 filter to set.</param>
		private void UpdateShaderFilterAndIntensity(Vector4 filter)
		{
			_bloom.SetVector(Filter, filter);
			_bloom.SetFloat(Intensity, Mathf.GammaToLinearSpace(intensity));
		}

		/// <summary>
		/// Creates a filter Vector
		/// </summary>
		/// <param name="thresholdProduct">The threshold product.</param>
		/// <returns>The filter of type vector 4.</returns>
		private Vector4 CreateImageFilter(float thresholdProduct)
		{
			Vector4 filter;
			filter.x = threshold;
			filter.y = filter.x - thresholdProduct;
			filter.z = 2f * thresholdProduct;
			var accuracy = 0.00001f;
			filter.w = 0.25f / (thresholdProduct + accuracy);
			return filter;
		}

		public void Start()
		{
			TryForceActivateHdr();
		}

		private void TryForceActivateHdr()
		{
#if UNITY_EDITOR
			if (!forceActivateHdr)
				return;
			var tierSettings = UnityEditor.Rendering.EditorGraphicsSettings.GetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, GraphicsTier.Tier1);
			tierSettings.hdr = true;
			UnityEditor.Rendering.EditorGraphicsSettings.SetTierSettings(EditorUserBuildSettings.selectedBuildTargetGroup, Graphics.activeTier, tierSettings);
#endif
		}
	}
}