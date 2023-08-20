using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ChaCustom;
using HarmonyLib;
using KKAPI;
using KKAPI.Chara;
using KKAPI.Studio;
using KKAPI.Studio.SaveLoad;
using KKAPI.Utilities;
using Studio;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityStandardAssets.ImageEffects;
using static GameCursor;

namespace PostProcessingEffectsV3
{
    [BepInDependency("org.bepinex.plugins.KKS_PostProcessingRuntime", "1.0.0.0")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class PostProcessingEffectsV3 : BaseUnityPlugin
    {
        public const string PluginGUID = "org.bepinex.plugins.KKS_PostProcessingEffectsV3";
        public const string PluginName = "KKS_PostProcessingEffectsV3";
        public const string PluginVersion = "2.0";

        internal static new ManualLogSource Logger;

        #region Constants
        private AmbientOcclusionQuality[] AOq = new AmbientOcclusionQuality[5]
        {
            AmbientOcclusionQuality.Lowest,
            AmbientOcclusionQuality.Low,
            AmbientOcclusionQuality.Medium,
            AmbientOcclusionQuality.High,
            AmbientOcclusionQuality.Ultra
        };

        private string[] AOq2 = new string[5] { "Lowest", "Low", "Medium", "High", "Ultra" };

        private PostProcessLayer.Antialiasing[] AAm = new PostProcessLayer.Antialiasing[4]
        {
            PostProcessLayer.Antialiasing.None,
            PostProcessLayer.Antialiasing.FastApproximateAntialiasing,
            PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing,
            PostProcessLayer.Antialiasing.TemporalAntialiasing
        };

        private string[] AAm2 = new string[4] { "None", "FXAA", "SMAA", "TAA" };

        private SubpixelMorphologicalAntialiasing.Quality[] AAq = new SubpixelMorphologicalAntialiasing.Quality[3]
        {
            SubpixelMorphologicalAntialiasing.Quality.Low,
            SubpixelMorphologicalAntialiasing.Quality.Medium,
            SubpixelMorphologicalAntialiasing.Quality.High
        };

        private string[] AAq2 = new string[3] { "Low", "Medium", "High" };

        private GradingMode[] CGm = new GradingMode[2]
        {
            GradingMode.LowDefinitionRange,
            GradingMode.HighDefinitionRange
        };

        private string[] CGm2 = new string[2] { "LowDefinitionRange", "HighDefinitionRange" };

        private Tonemapper[] CGt = new Tonemapper[3]
        {
            Tonemapper.None,
            Tonemapper.ACES,
            Tonemapper.Neutral
        };

        private string[] CGt2 = new string[3] { "None", "ACES", "Neutral" };

        private KernelSize[] DOFm = new KernelSize[4]
        {
            KernelSize.Small,
            KernelSize.Medium,
            KernelSize.Large,
            KernelSize.VeryLarge
        };

        private string[] DOFm2 = new string[4] { "Small", "Medium", "Large", "VeryLarge" };

        private FogMode[] fogModes = new FogMode[3]
        {
            FogMode.Linear,
            FogMode.Exponential,
            FogMode.ExponentialSquared
        };

        private string[] fogModes2 = new string[3] { "Linear", "Exponential", "ExponentialSquared" };
        #endregion

        #region Post Process Effects Variables
        private PostProcessVolume postProcessVolume;
        public PostProcessResources postProcessResources;
        public PostProcessLayer postProcessLayer;
        public Shader SSAOshader;
        public Texture2D noiseTex;
        private bool oAOenable;
        public GameObject camtarget;
        public AssetBundle ab;
        public Camera cam;
        public bool onoff_post;
        public Shader depthnormals;
        private bool nAOenable;
        public SSAOProUtils.SSAOPro sAOPro;
        public Shader SobelShader;
        public global::SobelOutline.SobelOutline sobel;
        private Posterize posterize;
        private LensDistortion lensDistortion;
        public Shader posShader;
        private SengaEffect sengaEffect;
        public Shader sengaShader;
        public Texture2D sengaToneTex;
        private Transform charapos = null;
        private AmbientOcclusion AO;
        private UnityEngine.Rendering.PostProcessing.Bloom bloom;
        private ColorGrading CG;
        private UnityEngine.Rendering.PostProcessing.MotionBlur MB;
        private UnityEngine.Rendering.PostProcessing.DepthOfField DOF;
        private Vignette VG;
        private ChromaticAberration CA;
        private GlobalFog globalFog;
        private Grain grain;
        #endregion

        #region Setup

        private global::Studio.Studio studio;

        private Dictionary<ChaControl, Transform> CharaList = new Dictionary<ChaControl, Transform>();

        private void Awake()
        {
            Logger = base.Logger;
        }

        private void OnEnable()
        {
            ab = AssetBundle.LoadFromFile(Path.Combine(Paths.BepInExRootPath, "plugins/KKS_postprocessresources.asset"));
            if (!(ab == null))
            {
                depthnormals = ab.LoadAsset<Shader>("Internal-DepthNormalsTexturemod");
                postProcessResources = ScriptableObject.CreateInstance<PostProcessResources>();
                postProcessResources.shaders = new PostProcessResources.Shaders();
                postProcessResources.computeShaders = new PostProcessResources.ComputeShaders();
                postProcessResources.smaaLuts = new PostProcessResources.SMAALuts();
                postProcessResources.shaders.bloom = ab.LoadAsset<Shader>("bloom");
                postProcessResources.shaders.copy = ab.LoadAsset<Shader>("copy");
                postProcessResources.shaders.copyStd = ab.LoadAsset<Shader>("copyStd");
                postProcessResources.shaders.copyStdFromTexArray = ab.LoadAsset<Shader>("copyStdFromTexArray");
                postProcessResources.shaders.copyStdFromDoubleWide = ab.LoadAsset<Shader>("copyStdFromDoubleWide");
                postProcessResources.shaders.discardAlpha = ab.LoadAsset<Shader>("discardAlpha");
                postProcessResources.shaders.depthOfField = ab.LoadAsset<Shader>("depthOfField");
                postProcessResources.shaders.finalPass = ab.LoadAsset<Shader>("finalPass");
                postProcessResources.shaders.grainBaker = ab.LoadAsset<Shader>("grainBaker");
                postProcessResources.shaders.motionBlur = ab.LoadAsset<Shader>("motionBlur");
                postProcessResources.shaders.temporalAntialiasing = ab.LoadAsset<Shader>("temporalAntialiasing");
                postProcessResources.shaders.subpixelMorphologicalAntialiasing = ab.LoadAsset<Shader>("subpixelMorphologicalAntialiasing");
                postProcessResources.shaders.texture2dLerp = ab.LoadAsset<Shader>("texture2dLerp");
                postProcessResources.shaders.uber = ab.LoadAsset<Shader>("uber");
                postProcessResources.shaders.lut2DBaker = ab.LoadAsset<Shader>("lut2DBaker");
                postProcessResources.shaders.lightMeter = ab.LoadAsset<Shader>("lightMeter");
                postProcessResources.shaders.gammaHistogram = ab.LoadAsset<Shader>("gammaHistogram");
                postProcessResources.shaders.waveform = ab.LoadAsset<Shader>("waveform");
                postProcessResources.shaders.vectorscope = ab.LoadAsset<Shader>("vectorscope");
                postProcessResources.shaders.debugOverlays = ab.LoadAsset<Shader>("debugOverlays");
                postProcessResources.shaders.deferredFog = ab.LoadAsset<Shader>("deferredFog");
                postProcessResources.shaders.scalableAO = ab.LoadAsset<Shader>("scalableAO");
                postProcessResources.shaders.multiScaleAO = ab.LoadAsset<Shader>("multiScaleAO");
                postProcessResources.shaders.screenSpaceReflections = ab.LoadAsset<Shader>("screenSpaceReflections");
                postProcessResources.computeShaders.autoExposure = ab.LoadAsset<ComputeShader>("AutoExposure.compute");
                postProcessResources.computeShaders.exposureHistogram = ab.LoadAsset<ComputeShader>("ExposureHistogram.compute");
                postProcessResources.computeShaders.lut3DBaker = ab.LoadAsset<ComputeShader>("Lut3DBaker.compute");
                postProcessResources.computeShaders.texture3dLerp = ab.LoadAsset<ComputeShader>("Texture3DLerp.compute");
                postProcessResources.computeShaders.multiScaleAODownsample1 = ab.LoadAsset<ComputeShader>("MultiScaleVODownsample1.compute.");
                postProcessResources.computeShaders.multiScaleAODownsample2 = ab.LoadAsset<ComputeShader>("MultiScaleVODownsample2.compute");
                postProcessResources.computeShaders.multiScaleAORender = ab.LoadAsset<ComputeShader>("MultiScaleVORender.compute");
                postProcessResources.computeShaders.multiScaleAOUpsample = ab.LoadAsset<ComputeShader>("MultiScaleVOUpsample.compute");
                postProcessResources.computeShaders.gaussianDownsample = ab.LoadAsset<ComputeShader>("GaussianDownsample.compute");
                postProcessResources.smaaLuts.area = ab.LoadAsset<Texture2D>("areaTex");
                postProcessResources.smaaLuts.search = ab.LoadAsset<Texture2D>("searchTex");
                postProcessResources.blueNoise64 = new Texture2D[64];
                for (int i = 0; i < 64; i++)
                {
                    postProcessResources.blueNoise64[i] = ab.LoadAsset<Texture2D>("LDR_LLL1_" + i + ".png");
                }
                postProcessResources.blueNoise256 = new Texture2D[8];
                for (int j = 0; j < 8; j++)
                {
                    postProcessResources.blueNoise256[j] = ab.LoadAsset<Texture2D>("LDR_LLL2_" + j + ".png");
                }
                SSAOshader = ab.LoadAsset<Shader>("SSAOPro_v2");
                noiseTex = ab.LoadAsset<Texture2D>("noise");
                SobelShader = ab.LoadAsset<Shader>("RealToon_Sobel_Outline_FX.shader");
                posShader = ab.LoadAsset<Shader>("Posterize");
                sengaShader = ab.LoadAsset<Shader>("senga");
                sengaToneTex = ab.LoadAsset<Texture2D>("dot_03.bmp");
                ab.Unload(false);
                BindConfig();
                SceneManager.sceneLoaded += OnSceneLoaded;
                base.Config.SettingChanged += OnSettingChanged;
                CharacterApi.CharacterReloaded += CharacterReloaded;
                StudioSaveLoadApi.ObjectsSelected += ObjectsSelected;
                onoff_post = onoff.Value;
            }
        }

        private void Start()
        {
            if (onoff.Value)
            {
                GraphicsSettings.SetShaderMode(BuiltinShaderType.DepthNormals, BuiltinShaderMode.UseCustom);
                GraphicsSettings.SetCustomShader(BuiltinShaderType.DepthNormals, depthnormals);
            }
            Harmony.CreateAndPatchAll(typeof(Patch), (string)null);
        }

        private void ObjectsSelected(object sender, ObjectsSelectedEventArgs e)
        {
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                ObjectCtrlInfo objectCtrlInfo = e.SelectedObjects.First();
                try
                {
                    OCIChar ociChar = (OCIChar)objectCtrlInfo;
                    charapos = ociChar.GetChaControl().transform.Find("BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips");
                }
                catch
                {
                }
            }
        }

        private void Update()
        {
            if (onoff.Value != onoff_post)
            {
                if (onoff.Value)
                {
                    GraphicsSettings.SetShaderMode(BuiltinShaderType.DepthNormals, BuiltinShaderMode.UseCustom);
                    GraphicsSettings.SetCustomShader(BuiltinShaderType.DepthNormals, depthnormals);
                    Setup();
                }
                else
                {
                    GraphicsSettings.SetShaderMode(BuiltinShaderType.DepthNormals, BuiltinShaderMode.UseBuiltin);
                    RuntimeUtilities.DestroyVolume(postProcessVolume, true, true);
                    UnityEngine.Object.Destroy(postProcessLayer);
                    UnityEngine.Object.Destroy(sAOPro);
                    UnityEngine.Object.Destroy(sobel);
                    UnityEngine.Object.Destroy(posterize);
                    UnityEngine.Object.Destroy(sengaEffect);
                }
            }
            onoff_post = onoff.Value;
            if (onoff.Value)
            {
                if (DOFautofocus.Value && DOFenable.Value)
                {
                    if (cam == null)
                    {
                        return;
                    }
                    if (DOFAFmode.Value == 0)
                    {
                        DOF.focusDistance.Override(Vector3.Distance(camtarget.transform.position, cam.transform.position));
                    }
                    else if (DOFAFmode.Value == 1 && KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
                    {
                        if (charapos != null)
                        {
                            float x = Vector3.Distance(charapos.position, cam.transform.position);
                            DOF.focusDistance.Override(x);
                        }
                    }
                    else if (DOFAFmode.Value == 2 && CharaList.Keys.Count() != 0)
                    {
                        Dictionary<float, Transform> dictionary = new Dictionary<float, Transform>();
                        foreach (ChaControl key3 in CharaList.Keys)
                        {
                            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
                            {
                                if (key3.GetOCIChar().treeNodeObject.visible)
                                {
                                    Vector3 vector = cam.WorldToScreenPoint(CharaList[key3].gameObject.transform.position);
                                    float key = Vector2.Distance(new Vector2(vector.x, vector.y), new Vector2(Screen.width / 2, Screen.height / 2));
                                    dictionary.Add(key, CharaList[key3]);
                                }
                            }
                            else
                            {
                                Vector3 vector2 = cam.WorldToScreenPoint(CharaList[key3].gameObject.transform.position);
                                float key2 = Vector2.Distance(new Vector2(vector2.x, vector2.y), new Vector2(Screen.width / 2, Screen.height / 2));
                                dictionary.Add(key2, CharaList[key3]);
                            }
                        }
                        DOF.focusDistance.Override(Vector3.Distance(dictionary[dictionary.Keys.Min()].position, cam.transform.position));
                    }
                }
                if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker || KoikatuAPI.GetCurrentGameMode() == GameMode.Unknown)
                {
                    cam.allowMSAA = false;
                }
            }
            if (MasterSwitch.Value.IsDown())
            {
                onoff.Value = !onoff.Value;
            }
            if (OpenGUI.Value.IsDown())
            {
                mainwin = !mainwin;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            cam = Camera.main;
            camtarget = GameObject.Find("CameraTarget");
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                studio = Singleton<global::Studio.Studio>.Instance;
            }
            if (onoff.Value)
            {
                Setup();
            }
            CharaList = new Dictionary<ChaControl, Transform>();
            ChaControl[] array = UnityEngine.Object.FindObjectsOfType<ChaControl>();
            foreach (ChaControl chaControl in array)
            {
                CharaList.Add(chaControl, chaControl.transform.Find("BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips"));
            }
        }

        private void CharacterReloaded(object sender, CharaReloadEventArgs e)
        {
            CharaList = new Dictionary<ChaControl, Transform>();
            ChaControl[] array = UnityEngine.Object.FindObjectsOfType<ChaControl>();
            foreach (ChaControl chaControl in array)
            {
                CharaList.Add(chaControl, chaControl.transform.Find("BodyTop/p_cf_body_bone/cf_j_root/cf_n_height/cf_j_hips"));
            }
        }

        private void Setup()
        {
            if ((bool)cam)
            {
                GameObject gameObject = cam.gameObject;
                postProcessLayer = gameObject.GetComponent<PostProcessLayer>();
                if (postProcessLayer == null)
                {
                    postProcessLayer = gameObject.AddComponent<PostProcessLayer>();
                }
                globalFog = gameObject.GetComponent<GlobalFog>();
                if (postProcessLayer == null)
                    globalFog = gameObject.AddComponent<GlobalFog>();
                postProcessLayer.Init(postProcessResources);
                postProcessLayer.InitBundles();
                postProcessLayer.volumeLayer = LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer));
                postProcessLayer.volumeTrigger = gameObject.transform;
                postProcessVolume = UnityEngine.Object.FindObjectOfType<PostProcessVolume>();
                if (!postProcessVolume)
                {
                    postProcessVolume = new GameObject("PostProcessVolume").AddComponent<PostProcessVolume>();
                }
                postProcessVolume.isGlobal = true;
                postProcessVolume.gameObject.layer = gameObject.layer;
                if (!postProcessVolume.profile.HasSettings<AmbientOcclusion>())
                {
                    AO = postProcessVolume.profile.AddSettings<AmbientOcclusion>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<AmbientOcclusion>(out AO);
                }
                if (!postProcessVolume.profile.HasSettings<UnityEngine.Rendering.PostProcessing.Bloom>())
                {
                    bloom = postProcessVolume.profile.AddSettings<UnityEngine.Rendering.PostProcessing.Bloom>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<UnityEngine.Rendering.PostProcessing.Bloom>(out bloom);
                }
                if (!postProcessVolume.profile.HasSettings<ColorGrading>())
                {
                    CG = postProcessVolume.profile.AddSettings<ColorGrading>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<ColorGrading>(out CG);
                }
                if (!postProcessVolume.profile.HasSettings<UnityEngine.Rendering.PostProcessing.MotionBlur>())
                {
                    MB = postProcessVolume.profile.AddSettings<UnityEngine.Rendering.PostProcessing.MotionBlur>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<UnityEngine.Rendering.PostProcessing.MotionBlur>(out MB);
                }
                if (!postProcessVolume.profile.HasSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>())
                {
                    DOF = postProcessVolume.profile.AddSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<UnityEngine.Rendering.PostProcessing.DepthOfField>(out DOF);
                }
                if (!postProcessVolume.profile.HasSettings<Vignette>())
                {
                    VG = postProcessVolume.profile.AddSettings<Vignette>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<Vignette>(out VG);
                }
                if (!postProcessVolume.profile.HasSettings<ChromaticAberration>())
                {
                    CA = postProcessVolume.profile.AddSettings<ChromaticAberration>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<ChromaticAberration>(out CA);
                }
                if (!postProcessVolume.profile.HasSettings<Grain>())
                {
                    grain = postProcessVolume.profile.AddSettings<Grain>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<Grain>(out grain);
                }
                lensDistortion = ScriptableObject.CreateInstance<LensDistortion>();
                lensDistortion = (LensDistortion)postProcessVolume.profile.AddSettings(lensDistortion);

                sAOPro = cam.GetComponent<SSAOProUtils.SSAOPro>();
                if (sAOPro == null)
                {
                    sAOPro = gameObject.AddComponent<SSAOProUtils.SSAOPro>();
                }
                sAOPro.enabled = false;
                sAOPro.NoiseTexture = noiseTex;
                sAOPro.ShaderSSAO = SSAOshader;
                if (cam.gameObject.GetComponent<Posterize>() == null)
                {
                    posterize = cam.gameObject.AddComponent<Posterize>();
                }
                posterize.enabled = false;
                posterize.shader = posShader;
                if (cam.gameObject.GetComponent<global::SobelOutline.SobelOutline>() == null)
                {
                    sobel = cam.gameObject.AddComponent<global::SobelOutline.SobelOutline>();
                }
                sobel.enabled = false;
                sobel.shader = SobelShader;
                if (cam.gameObject.GetComponent<SengaEffect>() == null)
                {
                    sengaEffect = cam.gameObject.AddComponent<SengaEffect>();
                }
                sengaEffect.enabled = false;
                sengaEffect.shader = sengaShader;
                sengaEffect.ToneTex = sengaToneTex;
                Settings();
            }
        }
        #endregion

        protected void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            if (onoff.Value)
            {
                Settings();
                UpdateBuffers();
            }
        }

        private void Settings()
        {
            if (AOenable.Value)
            {
                if (AOmodesel.Value)
                {
                    oAOenable = false;
                    nAOenable = true;
                }
                else
                {
                    oAOenable = true;
                    nAOenable = false;
                }
            }
            else
            {
                oAOenable = false;
                nAOenable = false;
            }
            if ((bool)postProcessLayer && (bool)postProcessVolume && (bool)sAOPro)
            {
                postProcessLayer.antialiasingMode = AAmode.Value;
                postProcessLayer.subpixelMorphologicalAntialiasing.quality = AAsmaaq.Value;
                postProcessLayer.fastApproximateAntialiasing.fastMode = AAfxaafm.Value;
                postProcessLayer.fastApproximateAntialiasing.keepAlpha = AAfxaakpa.Value;
                postProcessLayer.temporalAntialiasing.jitterSpread = TAAjittetSpeed.Value;
                postProcessLayer.temporalAntialiasing.motionBlending = TAAmotionBlending.Value;
                postProcessLayer.temporalAntialiasing.stationaryBlending = TAAstationaryBlending.Value;
                postProcessLayer.temporalAntialiasing.sharpness = TAAsharpen.Value;

                postProcessLayer.fog.enabled = FogEnable.Value;
                RenderSettings.fog = FogEnable.Value;
                RenderSettings.fogDensity = FogDensity.Value;
                RenderSettings.fogStartDistance = FogStart.Value;
                RenderSettings.fogEndDistance = FogEnd.Value;
                RenderSettings.fogMode = FogModeSelected.Value;
                RenderSettings.fogColor = FogColor.Value;

                if (studio != null && studio.sceneInfo != null && StudioAPI.InsideStudio)
                {
                    studio.sceneInfo.enableFog = FogEnable.Value;
                    if (FogModeSelected.Value == FogMode.Linear)
                        studio.sceneInfo.fogStartDistance = FogStart.Value;
                    else
                        studio.sceneInfo.fogStartDistance = 0f;
                    studio.sceneInfo.fogColor = FogColor.Value;
                    studio.sceneInfo.fogHeight = FogHeight.Value;
                    globalFog.height = FogHeight.Value;
                    globalFog.enabled = FogEnable.Value;
                }

                bloom.enabled.Override(Bloomenable.Value);
                bloom.intensity.Override(Bloomintensity.Value);
                bloom.anamorphicRatio.Override(Bloomanamor.Value);
                bloom.clamp.Override(Bloomclamp.Value);
                bloom.color.Override(Bloomcolor.Value);
                bloom.diffusion.Override(Bloomdiffusion.Value);
                bloom.fastMode.Override(Bloomfsmd.Value);
                bloom.softKnee.Override(BloomsoftKnee.Value);
                bloom.threshold.Override(Bloomthreshold.Value);

                AO.enabled.Override(oAOenable);
                AO.mode.Override(AOmode.Value);
                AO.intensity.Override(AOintensity.Value);
                AO.color.Override(AOcolor.Value);
                AO.radius.Override(AOradius.Value);
                AO.quality.Override(AOquality.Value);

                CG.enabled.Override(CGenable.Value);
                CG.tonemapper.Override(CGtoneMapper.Value);
                CG.gradingMode.Override(CGgradingmode.Value);
                CG.temperature.Override(CGtemp.Value);
                CG.tint.Override(CGtint.Value);
                CG.postExposure.Override(CGposte.Value);
                CG.colorFilter.Override(CGcolfilter.Value);
                CG.hueShift.Override(CGhueShift.Value);
                CG.saturation.Override(CGsaturation.Value);
                CG.contrast.Override(CGcontrast.Value);
                CG.lift.Override(CGlift.Value);
                CG.gamma.Override(CGgamma.Value);
                CG.gain.Override(CGgain.Value);

                MB.enabled.Override(MBenable.Value);
                MB.shutterAngle.Override(MBshutter.Value);
                MB.sampleCount.Override(MBsamplecnt.Value);

                DOF.enabled.Override(DOFenable.Value);
                if (!DOFautofocus.Value)
                {
                    DOF.focusDistance.Override(DOFfocusd.Value);
                }
                DOF.aperture.Override(DOFaperture.Value);
                DOF.focalLength.Override(DOFfocall.Value);
                DOF.kernelSize.Override(DOFmaxblur.Value);

                VG.enabled.Override(VGenable.Value);
                VG.center.Override(VGcenter.Value);
                VG.mode.Override(VGmode.Value);
                VG.color.Override(VGcol.Value);
                VG.intensity.Override(VGintensity.Value);
                VG.smoothness.Override(VGsmoothness.Value);
                VG.roundness.Override(VGroundness.Value);
                VG.rounded.Override(VGrounded.Value);
                VG.opacity.Override(VGopacity.Value);

                lensDistortion.enabled.Override(DistortionEnable.Value);
                lensDistortion.intensity.Override(DistortionIntensity.Value);
                lensDistortion.intensityX.Override(DistortionIntensityX.Value);
                lensDistortion.intensityY.Override(DistortionIntensityY.Value);
                lensDistortion.centerX.Override(DistortionCenterX.Value);
                lensDistortion.centerY.Override(DistortionCenterY.Value);
                lensDistortion.scale.Override(DistortionScale.Value);

                grain.enabled.Override(GrainEnable.Value);
                grain.colored.Override(GrainColored.Value);
                grain.intensity.Override(GrainIntensity.Value);
                grain.size.Override(GrainSize.Value);
                grain.lumContrib.Override(GrainLumContrib.Value);

                CA.enabled.Override(CAenable.Value);
                CA.intensity.Override(CAintensity.Value);
                sAOPro.enabled = nAOenable;
                sAOPro.Bias = cBias.Value;
                sAOPro.Blur = cBlurType.Value;
                sAOPro.BlurBilateralThreshold = cThres.Value;
                sAOPro.BlurDownsampling = cBlurDownS.Value;
                sAOPro.BlurPasses = cBlurPasses.Value;
                sAOPro.CutoffDistance = cMaxDistance.Value;
                sAOPro.CutoffFalloff = cFalloff.Value;
                sAOPro.Distance = cDistance.Value;
                sAOPro.Downsampling = cDownsampling.Value;
                sAOPro.Intensity = cIntensity.Value;
                sAOPro.LumContribution = cLithingCont.Value;
                sAOPro.OcclusionColor = cOccColor.Value;
                sAOPro.Samples = cSampleCount.Value;
                sAOPro.Radius = cRadius.Value;

                sobel.enabled = SoutlineEnable.Value;
                sobel.ColorPower = ColorPower.Value;
                sobel.OutlineColor = OutlineColor.Value;
                sobel.OutlineWidth = OutlineWidth.Value;

                posterize.enabled = PosEnable.Value;
                posterize._div = PosDiv.Value;
                posterize._HSV = PosHSV.Value;

                sengaEffect.enabled = SengaEnable.Value;
                sengaEffect.SengaOnly = SengaOnly.Value;
                sengaEffect.ColorEdge = SengaColorEdge.Value;
                sengaEffect.ColorThreshold = SengaColorThres.Value;
                sengaEffect.DepthEdge = SengaDepthEdge.Value;
                sengaEffect.DepthThreshold = SengaDepthThres.Value;
                sengaEffect.NormalEdge = SengaNormalEdge.Value;
                sengaEffect.NormalThreshold = SengaNomalThes.Value;
                sengaEffect.SampleDistance = SengaSampleDistance.Value;
                sengaEffect.SobelEdge = SengaSobelEdge.Value;
                sengaEffect.SobelThreshold = SengaSobelThres.Value;
                sengaEffect.kosa = SengaColBlend.Value;
                sengaEffect.toneThres = SengaToneThres.Value;
                sengaEffect.ToneOn = SengaToneEnable.Value;
                sengaEffect.ToneThick = SengaToneThick.Value;
                sengaEffect.scale = SengaToneScale.Value;
                sengaEffect.BlurOn = SengaBlurEnable.Value;
                sengaEffect.thick = SengaBlurThick.Value;
                sengaEffect.pow = SengaBlurPow.Value;
                sengaEffect.SampleCount = SengaBlurSample.Value;
                sengaEffect.dir = SengaBlurDir.Value;
            }
        }


        #region UI
        private bool mainwin = false;
        private readonly int uiWindowHash = ('P' << 24) | ('P' << 16) | ('E' << 8);
        private bool exitOnFocusLoss = true;
        public Rect Rect1 = new Rect(220f, 50f, 520, 420f);

        private Canvas myCanvas;
        private GameObject myGO;

        private bool AOb = false;
        private bool AA = false;
        private bool bloomb = false;
        private bool CGb = false;
        private bool MBb = false;
        private bool VGb = false;
        private bool DOFb = false;
        private bool CAb = false;
        private bool SCOb = false;
        private bool Posb = false;
        private bool Sengab = false;
        private bool distortion = false;
        private bool fog = false;
        private bool grainShown = false;

        #region Buffers
        private string DistortionIntensityBuffer;
        private string DistortionIntensityXBuffer;
        private string DistortionIntensityYBuffer;
        private string DistortionCenterXBuffer;
        private string DistortionCenterYBuffer;
        private string DistortionScaleBuffer;
        private string cDownsamplingBuffer;
        private string cIntensityBuffer;
        private string cRadiusBuffer;
        private string cDistanceBuffer;
        private string cBiasBuffer;
        private string cLithingContBuffer;
        private string cBlurPassesBuffer;
        private string cThresBuffer;
        private string AOintensityBuffer;
        private string AOradiusBuffer;
        private string TAAjittetSpeedBuffer;
        private string TAAstationaryBlendingBuffer;
        private string TAAmotionBlendingBuffer;
        private string TAAsharpenBuffer;
        private string BloomintensityBuffer;
        private string BloomthresholdBuffer;
        private string BloomsoftKneeBuffer;
        private string BloomclampBuffer;
        private string BloomdiffusionBuffer;
        private string BloomanamorBuffer;
        private string CAintensityBuffer;
        private string CGtempBuffer;
        private string CGtintBuffer;
        private string CGposteBuffer;
        private string CGhueShiftBuffer;
        private string CGsaturationBuffer;
        private string CGcontrastBuffer;
        private string DOFfocusdBuffer;
        private string DOFapertureBuffer;
        private string DOFfocallBuffer;
        private string MBshutterBuffer;
        private string MBsamplecntBuffer;
        private string VGcenterXBuffer;
        private string VGcenterYBuffer;
        private string VGintensityBuffer;
        private string VGsmoothnessBuffer;
        private string VGroundnessBuffer;
        private string OutlineWidthBuffer;
        private string ColorPowerBuffer;
        private string PosDivBuffer;
        private string SengaSampleDistanceBuffer;
        private string SengaNomalThesBuffer;
        private string SengaDepthThresBuffer;
        private string SengaColorThresBuffer;
        private string SengaSobelThresBuffer;
        private string SengaNormalEdgeBuffer;
        private string SengaDepthEdgeBuffer;
        private string SengaColorEdgeBuffer;
        private string SengaSobelEdgeBuffer;
        private string SengaColBlendBuffer;
        private string SengaToneScaleBuffer;
        private string SengaToneThickBuffer;
        private string SengaToneThresBuffer;
        private string SengaBlurDirBuffer;
        private string SengaBlurPowBuffer;
        private string SengaBlurThickBuffer;
        private string SengaBlurSampleBuffer;
        private string FogDensityBuffer;
        private string FogStartBuffer;
        private string FogEndBuffer;
        private string FogHeightBuffer;
        private string GrainIntensityBuffer;
        private string GrainSizeBuffer;
        private string GrainLumContribBuffer;

        private void UpdateBuffers()
        {
            TAAjittetSpeedBuffer = TAAjittetSpeed.Value.ToString();
            TAAsharpenBuffer = TAAsharpen.Value.ToString();
            TAAstationaryBlendingBuffer = TAAstationaryBlending.Value.ToString();
            TAAmotionBlendingBuffer = TAAmotionBlending.Value.ToString();
            AOintensityBuffer = AOintensity.Value.ToString();
            AOradiusBuffer = AOradius.Value.ToString();
            cIntensityBuffer = cIntensity.Value.ToString();
            cRadiusBuffer = cRadius.Value.ToString();
            cDownsamplingBuffer = cDownsampling.Value.ToString();
            cDistanceBuffer = cDistance.Value.ToString();
            cBiasBuffer = cBias.Value.ToString();
            cLithingContBuffer = cLithingCont.Value.ToString();
            cBlurPassesBuffer = cBlurPasses.Value.ToString();
            cThresBuffer = cThres.Value.ToString();
            BloomintensityBuffer = Bloomintensity.Value.ToString();
            BloomanamorBuffer = Bloomanamor.Value.ToString();
            BloomsoftKneeBuffer = BloomsoftKnee.Value.ToString();
            BloomthresholdBuffer = Bloomthreshold.Value.ToString();
            BloomclampBuffer = Bloomclamp.Value.ToString();
            BloomdiffusionBuffer = Bloomdiffusion.Value.ToString();
            CGposteBuffer = CGposte.Value.ToString();
            CGtempBuffer = CGtemp.Value.ToString();
            CGtintBuffer = CGtint.Value.ToString();
            CGhueShiftBuffer = CGhueShift.Value.ToString();
            CGsaturationBuffer = CGsaturation.Value.ToString();
            CGcontrastBuffer = CGcontrast.Value.ToString();
            CAintensityBuffer = CAintensity.Value.ToString();
            MBshutterBuffer = MBshutter.Value.ToString();
            MBsamplecntBuffer = MBsamplecnt.Value.ToString();
            DOFfocusdBuffer = DOFfocusd.Value.ToString();
            DOFapertureBuffer = DOFaperture.Value.ToString();
            DOFfocallBuffer = DOFfocall.Value.ToString();
            VGintensityBuffer = VGintensity.Value.ToString();
            VGsmoothnessBuffer = VGsmoothness.Value.ToString();
            VGroundnessBuffer = VGroundness.Value.ToString();
            VGcenterXBuffer = VGcenter.Value.x.ToString();
            VGcenterYBuffer = VGcenter.Value.y.ToString();
            OutlineWidthBuffer = OutlineWidth.Value.ToString();
            ColorPowerBuffer = ColorPower.Value.ToString();
            PosDivBuffer = PosDiv.Value.ToString();
            SengaNomalThesBuffer = SengaNomalThes.Value.ToString();
            SengaDepthThresBuffer = SengaDepthThres.Value.ToString();
            SengaColorThresBuffer = SengaColorThres.Value.ToString();
            SengaSobelThresBuffer = SengaSobelThres.Value.ToString();
            SengaNormalEdgeBuffer = SengaNormalEdge.Value.ToString();
            SengaDepthEdgeBuffer = SengaDepthEdge.Value.ToString();
            SengaColorEdgeBuffer = SengaColorEdge.Value.ToString();
            SengaSobelEdgeBuffer = SengaSobelEdge.Value.ToString();
            SengaColBlendBuffer = SengaColBlend.Value.ToString();
            SengaToneScaleBuffer = SengaToneScale.Value.ToString();
            SengaToneThickBuffer = SengaToneThick.Value.ToString();
            SengaToneThresBuffer = SengaToneThres.Value.ToString();
            SengaBlurDirBuffer = SengaBlurDir.Value.ToString();
            SengaBlurPowBuffer = SengaBlurPow.Value.ToString();
            SengaBlurThickBuffer = SengaBlurThick.Value.ToString();
            SengaBlurSampleBuffer = SengaBlurSample.Value.ToString();
            SengaSampleDistanceBuffer = SengaSampleDistance.Value.ToString();
            DistortionIntensityBuffer = DistortionIntensity.Value.ToString();
            DistortionIntensityXBuffer = DistortionIntensityX.Value.ToString();
            DistortionIntensityYBuffer = DistortionIntensityY.Value.ToString();
            DistortionCenterXBuffer = DistortionCenterX.Value.ToString();
            DistortionCenterYBuffer = DistortionCenterY.Value.ToString();
            DistortionScaleBuffer = DistortionScale.Value.ToString();
            FogDensityBuffer = FogDensity.Value.ToString();
            FogStartBuffer = FogStart.Value.ToString();
            FogEndBuffer = FogEnd.Value.ToString();
            FogHeightBuffer = FogHeight.Value.ToString();
            GrainIntensityBuffer = GrainIntensity.Value.ToString();
            GrainSizeBuffer = GrainSize.Value.ToString();
            GrainLumContribBuffer = GrainLumContrib.Value.ToString();
        }
        #endregion


        private void OnGUI()
        {
            if (mainwin)
            {
                if (exitOnFocusLoss)
                    if (GUI.Button(new Rect(0f, 0f, Screen.width, Screen.height), "", GUI.skin.label))
                    {
                        mainwin = false;
                        UnityEngine.Object.Destroy(myGO);
                    }
                Rect1 = GUILayout.Window(uiWindowHash, Rect1, mainwindow, "PostProcessingEffects");
                IMGUIUtils.EatInputInRect(Rect1);
            }
        }

        private float DrawSliderTextBoxCombo(string label, float min, float max, ref string buffer, float value, float valueDefault, bool isIntSlider = false)
        {
            float newValue = value;
            string focused = GUI.GetNameOfFocusedControl();
            if (focused == label && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
                GUI.FocusControl(null);

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(120f));

            float sliderBuffer = GUILayout.HorizontalSlider(value, min, max);
            if (isIntSlider) sliderBuffer = (int)sliderBuffer;

            GUI.SetNextControlName(label);
            buffer = GUILayout.TextField(buffer.ToString(), GUILayout.Width(50));

            if (focused != label)
            {
                if (!float.TryParse(buffer, out float valueBufferFloat))
                    valueBufferFloat = value;
                if (valueBufferFloat != value)
                    newValue = valueBufferFloat;
                else if (sliderBuffer != value)
                    newValue = sliderBuffer;
            }

            if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                newValue = valueDefault;

            GUILayout.EndHorizontal();

            if (newValue.CompareTo(min) < 0) return min;
            else if (newValue.CompareTo(max) > 0) return max;
            else return newValue;
        }

        private void mainwindow(int windowID)
        {
            GUILayout.BeginHorizontal();
            onoff.Value = GUILayout.Toggle(onoff.Value, "Enable/Disable All");
            exitOnFocusLoss = GUILayout.Toggle(exitOnFocusLoss, "Close on focus loss");
            GUILayout.EndHorizontal();

            #region Ambient Occulusion
            AOb = GUILayout.Toggle(AOb, "AmbientOcculusion ", GUI.skin.button);
            if (AOb)
            {
                GUILayout.BeginVertical();
                AOenable.Value = GUILayout.Toggle(AOenable.Value, "Enable");
                AOmodesel.Value = GUILayout.Toggle(AOmodesel.Value, "New AO Mode");
                if (AOmodesel.Value)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("SampleCount  ", GUILayout.Width(120f));
                    cSampleCount.Value = (SSAOProUtils.SSAOPro.SampleCount)GUILayout.SelectionGrid((int)cSampleCount.Value, Enum.GetNames(typeof(SSAOProUtils.SSAOPro.SampleCount)), 3, GUI.skin.toggle);
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cSampleCount.Value = (SSAOProUtils.SSAOPro.SampleCount)cSampleCount.DefaultValue;
                    }
                    GUILayout.EndHorizontal();

                    cDownsampling.Value = (int)DrawSliderTextBoxCombo(
                        "Downsampling", 1f, 4f, ref cDownsamplingBuffer, cDownsampling.Value, (int)cDownsampling.DefaultValue, true
                    );
                    cIntensity.Value = DrawSliderTextBoxCombo(
                        "Intensity", 0f, 16f, ref cIntensityBuffer, cIntensity.Value, (float)cIntensity.DefaultValue
                    );
                    cRadius.Value = DrawSliderTextBoxCombo(
                        "Radius   ", 0.01f, 1.25f, ref cRadiusBuffer, cRadius.Value, (float)cRadius.DefaultValue
                    );
                    cDistance.Value = DrawSliderTextBoxCombo(
                        "Distance   ", 0f, 10f, ref cDistanceBuffer, cDistance.Value, (float)cDistance.DefaultValue
                    );
                    cBias.Value = DrawSliderTextBoxCombo(
                        "Bias   ", 0f, 1f, ref cBiasBuffer, cBias.Value, (float)cBias.DefaultValue
                    );
                    cLithingCont.Value = DrawSliderTextBoxCombo(
                        "LightingContribution   ", 0f, 1f, ref cLithingContBuffer, cLithingCont.Value, (float)cLithingCont.DefaultValue
                    );

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color   ");
                    if (GUILayout.Button("", colorbutton(cOccColor.Value)) && (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio || KoikatuAPI.GetCurrentGameMode() != GameMode.Maker))
                    {
                        Action<Color> act = delegate (Color c)
                        {
                            cOccColor.Value = c;
                        };
                        ColorPicker(cOccColor.Value, act);
                    }
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cOccColor.Value = (Color)cOccColor.DefaultValue;
                    }

                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("BlurType  ", GUILayout.Width(120f));
                    cBlurType.Value = (SSAOProUtils.SSAOPro.BlurMode)GUILayout.SelectionGrid((int)cBlurType.Value, Enum.GetNames(typeof(SSAOProUtils.SSAOPro.BlurMode)), 3, GUI.skin.toggle);
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cBlurType.Value = (SSAOProUtils.SSAOPro.BlurMode)cBlurType.DefaultValue;
                    }
                    GUILayout.EndHorizontal();

                    cBlurDownS.Value = GUILayout.Toggle(cBlurDownS.Value, "BlurDownsampling");

                    cBlurPasses.Value = (int)DrawSliderTextBoxCombo(
                        "BlurPasses", 1f, 4f, ref cBlurPassesBuffer, cBlurPasses.Value, (int)cBlurPasses.DefaultValue, true
                    );
                    cThres.Value = DrawSliderTextBoxCombo(
                        "BlurThreshold", 1f, 20f, ref cThresBuffer, cThres.Value, (float)cThres.DefaultValue
                    );

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("MaxDistance", GUILayout.Width(120f));
                    cMaxDistance.Value = float.Parse(GUILayout.TextField(cMaxDistance.Value.ToString("F")));
                    GUILayout.Label(cMaxDistance.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cMaxDistance.Value = (float)cMaxDistance.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Falloff", GUILayout.Width(120f));
                    cFalloff.Value = float.Parse(GUILayout.TextField(cFalloff.Value.ToString("F")));
                    GUILayout.Label(cFalloff.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cFalloff.Value = (float)cFalloff.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                }
                else
                {
                    AOintensity.Value = DrawSliderTextBoxCombo(
                        "Intensity", 0f, 4f, ref AOintensityBuffer, AOintensity.Value, (float)AOintensity.DefaultValue
                    );
                    AOradius.Value = DrawSliderTextBoxCombo(
                        "Radius   ", 00001f, 3f, ref AOradiusBuffer, AOradius.Value, (float)AOradius.DefaultValue
                    );

                    int selected = Array.IndexOf(AOq, AOquality.Value);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Quality  ", GUILayout.Width(120f));
                    selected = GUILayout.SelectionGrid(selected, AOq2, 3, GUI.skin.toggle);
                    AOquality.Value = AOq[selected];
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        AOquality.Value = (AmbientOcclusionQuality)AOquality.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Color   ");
                    if (GUILayout.Button("", colorbutton(AOcolor.Value)) && (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio || KoikatuAPI.GetCurrentGameMode() != GameMode.Maker))
                    {
                        Action<Color> act2 = delegate (Color c)
                        {
                            AOcolor.Value = c;
                        };
                        ColorPicker(AOcolor.Value, act2);
                    }
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        AOcolor.Value = (Color)AOcolor.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            #endregion

            #region Anti Aliasing
            AA = GUILayout.Toggle(AA, "AntiAliasing ", GUI.skin.button);
            if (AA)
            {
                GUILayout.BeginVertical();
                int selected2 = Array.IndexOf(AAm, AAmode.Value);
                GUILayout.BeginHorizontal();
                GUILayout.Label("AAmode  ", GUILayout.Width(120f));
                selected2 = GUILayout.SelectionGrid(selected2, AAm2, 4, GUI.skin.toggle);
                AAmode.Value = AAm[selected2];
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    AAmode.Value = (PostProcessLayer.Antialiasing)AAmode.DefaultValue;
                }
                GUILayout.EndHorizontal();
                if (AAmode.Value == PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing)
                {
                    int selected3 = Array.IndexOf(AAq, AAsmaaq.Value);
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Quality  ", GUILayout.Width(120f));
                    selected3 = GUILayout.SelectionGrid(selected3, AAq2, 3, GUI.skin.toggle);
                    AAsmaaq.Value = AAq[selected3];
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        AAsmaaq.Value = (SubpixelMorphologicalAntialiasing.Quality)AAsmaaq.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                }
                if (AAmode.Value == PostProcessLayer.Antialiasing.TemporalAntialiasing)
                {
                    TAAjittetSpeed.Value = DrawSliderTextBoxCombo(
                        "JitterSpread", 0.1f, 1f, ref TAAjittetSpeedBuffer, TAAjittetSpeed.Value, (float)TAAjittetSpeed.DefaultValue
                    );
                    TAAstationaryBlending.Value = DrawSliderTextBoxCombo(
                        "StationaryBlending", 0.1f, 1f, ref TAAstationaryBlendingBuffer, TAAstationaryBlending.Value, (float)TAAstationaryBlending.DefaultValue
                    );
                    TAAmotionBlending.Value = DrawSliderTextBoxCombo(
                        "MotionBlending", 0.1f, 1f, ref TAAmotionBlendingBuffer, TAAmotionBlending.Value, (float)TAAmotionBlending.DefaultValue
                    );
                    TAAsharpen.Value = DrawSliderTextBoxCombo(
                        "Sharpen", 0.1f, 1f, ref TAAsharpenBuffer, TAAsharpen.Value, (float)TAAsharpen.DefaultValue
                    );
                }
                GUILayout.EndVertical();
            }
            #endregion

            #region Bloom
            bloomb = GUILayout.Toggle(bloomb, "Bloom ", GUI.skin.button);
            if (bloomb)
            {
                GUILayout.BeginVertical();
                Bloomenable.Value = GUILayout.Toggle(Bloomenable.Value, "Enable");

                Bloomintensity.Value = DrawSliderTextBoxCombo(
                    "Intensity  ", 0f, 10f, ref BloomintensityBuffer, Bloomintensity.Value, (float)Bloomintensity.DefaultValue
                );
                Bloomthreshold.Value = DrawSliderTextBoxCombo(
                    "Threshold", 0f, 5f, ref BloomthresholdBuffer, Bloomthreshold.Value, (float)Bloomthreshold.DefaultValue
                );
                BloomsoftKnee.Value = DrawSliderTextBoxCombo(
                    "SoftKnee", 0f, 1f, ref BloomsoftKneeBuffer, BloomsoftKnee.Value, (float)BloomsoftKnee.DefaultValue
                );
                Bloomclamp.Value = DrawSliderTextBoxCombo(
                    "Clamp", 0f, 65472f, ref BloomclampBuffer, Bloomclamp.Value, (float)Bloomclamp.DefaultValue
                );
                Bloomdiffusion.Value = DrawSliderTextBoxCombo(
                    "Diffusion", 1f, 10f, ref BloomdiffusionBuffer, Bloomdiffusion.Value, (float)Bloomdiffusion.DefaultValue
                );
                Bloomanamor.Value = DrawSliderTextBoxCombo(
                    "AnamorphicPatio", -1f, 1f, ref BloomanamorBuffer, Bloomanamor.Value, (float)Bloomanamor.DefaultValue
                );

                GUILayout.BeginHorizontal();
                GUILayout.Label("Color   ");
                if (GUILayout.Button("", colorbutton(Bloomcolor.Value)) && (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio || KoikatuAPI.GetCurrentGameMode() != GameMode.Maker))
                {
                    Action<Color> act3 = delegate (Color c)
                    {
                        Bloomcolor.Value = c;
                    };
                    ColorPicker(Bloomcolor.Value, act3);
                }
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    Bloomcolor.Value = (Color)Bloomcolor.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            #endregion

            #region Chromatic Aberration
            CAb = GUILayout.Toggle(CAb, "ChromaticAberration", GUI.skin.button);
            if (CAb)
            {
                GUILayout.BeginVertical();
                CAenable.Value = GUILayout.Toggle(CAenable.Value, "Enable");

                CAintensity.Value = DrawSliderTextBoxCombo(
                    "Intensity  ", 0f, 1f, ref CAintensityBuffer, CAintensity.Value, (float)CAintensity.DefaultValue
                );
                GUILayout.EndVertical();
            }
            #endregion

            #region Color Grading
            CGb = GUILayout.Toggle(CGb, "ColorGrading ", GUI.skin.button);
            if (CGb)
            {
                GUILayout.BeginVertical();
                CGenable.Value = GUILayout.Toggle(CGenable.Value, "Enable");
                int selected4 = Array.IndexOf(CGm, CGgradingmode.Value);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Mode  ", GUILayout.Width(120f));
                selected4 = GUILayout.SelectionGrid(selected4, CGm2, 2, GUI.skin.toggle);
                CGgradingmode.Value = CGm[selected4];
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CGgradingmode.Value = (GradingMode)CGgradingmode.DefaultValue;
                }
                //TODO add custom tonemapping
                GUILayout.EndHorizontal();
                selected4 = Array.IndexOf(CGt, CGtoneMapper.Value);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Tonemapping  ", GUILayout.Width(120f));
                selected4 = GUILayout.SelectionGrid(selected4, CGt2, 3, GUI.skin.toggle);
                CGtoneMapper.Value = CGt[selected4];
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CGtoneMapper.Value = (Tonemapper)CGtoneMapper.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("WhiteBalance  ", GUILayout.Width(120f));

                CGtemp.Value = DrawSliderTextBoxCombo(
                    "Temperature  ", -100f, 100f, ref CGtempBuffer, CGtemp.Value, (float)CGtemp.DefaultValue
                );
                CGtint.Value = DrawSliderTextBoxCombo(
                    "Tint", -100f, 100f, ref CGtintBuffer, CGtint.Value, (float)CGtint.DefaultValue
                );

                GUILayout.Label("Tone  ", GUILayout.Width(120f));

                CGposte.Value = DrawSliderTextBoxCombo(
                    "PostExposure", 0f, 1f, ref CGposteBuffer, CGposte.Value, (float)CGposte.DefaultValue
                );
                //TODO Add brightness slider support

                GUILayout.BeginHorizontal();
                GUILayout.Label("Color   ");
                if (GUILayout.Button("", colorbutton(CGcolfilter.Value)) && (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio || KoikatuAPI.GetCurrentGameMode() != GameMode.Maker))
                {
                    Action<Color> act4 = delegate (Color c)
                    {
                        CGcolfilter.Value = c;
                    };
                    ColorPicker(CGcolfilter.Value, act4);
                }
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CGcolfilter.Value = (Color)CGcolfilter.DefaultValue;
                }
                GUILayout.EndHorizontal();

                CGhueShift.Value = DrawSliderTextBoxCombo(
                    "HueShift", -180f, 180f, ref CGhueShiftBuffer, CGhueShift.Value, (float)CGhueShift.DefaultValue
                );
                CGsaturation.Value = DrawSliderTextBoxCombo(
                    "Saturation", -100f, 100f, ref CGsaturationBuffer, CGsaturation.Value, (float)CGsaturation.DefaultValue
                );
                CGcontrast.Value = DrawSliderTextBoxCombo(
                    "Contrast", -100f, 100f, ref CGcontrastBuffer, CGcontrast.Value, (float)CGcontrast.DefaultValue
                );

                //TODO add channel mixer
                //TODO add trackballs support
                GUILayout.EndVertical();
            }
            #endregion

            #region Depth of Field
            DOFb = GUILayout.Toggle(DOFb, "DepthOfField ", GUI.skin.button);
            if (DOFb)
            {
                GUILayout.BeginVertical();
                DOFenable.Value = GUILayout.Toggle(DOFenable.Value, "Enable");

                DOFfocusd.Value = DrawSliderTextBoxCombo(
                    "FocusDistance  ", 0.1f, 50f, ref DOFfocusdBuffer, DOFfocusd.Value, (float)DOFfocusd.DefaultValue
                );
                DOFaperture.Value = DrawSliderTextBoxCombo(
                    "Aperture", 0.05f, 32f, ref DOFapertureBuffer, DOFaperture.Value, (float)DOFaperture.DefaultValue
                );
                DOFfocall.Value = DrawSliderTextBoxCombo(
                    "FocalLength", 1f, 300f, ref DOFfocallBuffer, DOFfocall.Value, (float)DOFfocall.DefaultValue
                );

                int selected5 = Array.IndexOf(DOFm, DOFmaxblur.Value);
                GUILayout.BeginHorizontal();
                GUILayout.Label("MaxBlurSize  ", GUILayout.Width(120f));
                selected5 = GUILayout.SelectionGrid(selected5, DOFm2, 4, GUI.skin.toggle);
                DOFmaxblur.Value = DOFm[selected5];
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    DOFmaxblur.Value = (KernelSize)DOFmaxblur.DefaultValue;
                }
                GUILayout.EndHorizontal();
                DOFautofocus.Value = GUILayout.Toggle(DOFautofocus.Value, "AutoFocus");
                if (DOFautofocus.Value)
                {
                    DOFAFmode.Value = GUILayout.SelectionGrid(DOFAFmode.Value, new string[3] { "CamTarget", "SelectedChara", "CharaNearestScreenCenter" }, 3, GUI.skin.toggle);
                }
                GUILayout.EndVertical();
            }
            #endregion

            #region Motion Blur
            MBb = GUILayout.Toggle(MBb, "MotionBlur", GUI.skin.button);
            if (MBb)
            {
                GUILayout.BeginVertical();
                MBenable.Value = GUILayout.Toggle(MBenable.Value, "Enable");

                MBshutter.Value = DrawSliderTextBoxCombo(
                    "ShutterAngle  ", 0f, 360f, ref MBshutterBuffer, MBshutter.Value, (float)MBshutter.DefaultValue
                );
                MBsamplecnt.Value = (int)DrawSliderTextBoxCombo(
                    "ShutterAngle  ", 4f, 32f, ref MBsamplecntBuffer, MBsamplecnt.Value, (int)MBsamplecnt.DefaultValue, true
                );
                GUILayout.EndVertical();
            }
            #endregion

            #region Vignette
            VGb = GUILayout.Toggle(VGb, "Vignette", GUI.skin.button);
            if (VGb)
            {
                GUILayout.BeginVertical();
                VGenable.Value = GUILayout.Toggle(VGenable.Value, "Enable");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Color   ");
                if (GUILayout.Button("", colorbutton(VGcol.Value)) && (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio || KoikatuAPI.GetCurrentGameMode() != GameMode.Maker))
                {
                    Action<Color> act5 = delegate (Color c)
                    {
                        VGcol.Value = c;
                    };
                    ColorPicker(VGcol.Value, act5);
                }
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    VGcol.Value = (Color)VGcol.DefaultValue;
                }
                GUILayout.EndHorizontal();

                GUILayout.Label("Center  ", GUILayout.Width(120f));
                float x = DrawSliderTextBoxCombo(
                    "X: ", 0f, 1f, ref VGcenterXBuffer, VGcenter.Value.x, 0.5f
                );
                float y = DrawSliderTextBoxCombo(
                    "Y: ", 0f, 1f, ref VGcenterYBuffer, VGcenter.Value.y, 0.5f
                );
                VGcenter.Value = new Vector2(x, y);

                GUILayout.Label("Settings", GUILayout.Width(120f));

                VGintensity.Value = DrawSliderTextBoxCombo(
                    "Intensity", 0f, 1f, ref VGintensityBuffer, VGintensity.Value, (float)VGintensity.DefaultValue
                );
                VGsmoothness.Value = DrawSliderTextBoxCombo(
                    "Smoothness", 0f, 1f, ref VGsmoothnessBuffer, VGsmoothness.Value, (float)VGsmoothness.DefaultValue
                );

                VGrounded.Value = GUILayout.Toggle(VGrounded.Value, "Rounded");

                VGroundness.Value = DrawSliderTextBoxCombo(
                    "Roundness", 0f, 1f, ref VGroundnessBuffer, VGroundness.Value, (float)VGroundness.DefaultValue
                );
                GUILayout.EndVertical();
            }
            #endregion

            #region Sobel Color Outline
            SCOb = GUILayout.Toggle(SCOb, "SobelColorOutline", GUI.skin.button);
            if (SCOb)
            {
                GUILayout.BeginVertical();
                SoutlineEnable.Value = GUILayout.Toggle(SoutlineEnable.Value, "Enable");
                GUILayout.BeginHorizontal();
                GUILayout.Label("OutlineColor   ");
                if (GUILayout.Button("", colorbutton(OutlineColor.Value)) && (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio || KoikatuAPI.GetCurrentGameMode() != GameMode.Maker))
                {
                    Action<Color> act6 = delegate (Color c)
                    {
                        OutlineColor.Value = c;
                    };
                    ColorPicker(OutlineColor.Value, act6);
                }
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    OutlineColor.Value = (Color)OutlineColor.DefaultValue;
                }
                GUILayout.EndHorizontal();

                OutlineWidth.Value = DrawSliderTextBoxCombo(
                    "OutlineWidth", 0f, 1f, ref OutlineWidthBuffer, OutlineWidth.Value, (float)OutlineWidth.DefaultValue
                );
                ColorPower.Value = DrawSliderTextBoxCombo(
                    "ColorPower", 0f, 1f, ref ColorPowerBuffer, ColorPower.Value, (float)ColorPower.DefaultValue
                );
                GUILayout.EndVertical();
            }
            #endregion

            #region Posterize
            Posb = GUILayout.Toggle(Posb, "Posterize", GUI.skin.button);
            if (Posb)
            {
                GUILayout.BeginVertical();
                PosEnable.Value = GUILayout.Toggle(PosEnable.Value, "Enable");
                PosHSV.Value = GUILayout.Toggle(PosHSV.Value, "HSV Transfer");

                PosDiv.Value = (int)DrawSliderTextBoxCombo(
                    "DivisionNum", 1f, 64f, ref PosDivBuffer, PosDiv.Value, (int)PosDiv.DefaultValue, true
                );
                GUILayout.EndVertical();
            }
            #endregion

            #region Customizaable Outline
            Sengab = GUILayout.Toggle(Sengab, "CustomizaableOutline", GUI.skin.button);
            if (Sengab)
            {
                GUILayout.BeginVertical();
                SengaEnable.Value = GUILayout.Toggle(SengaEnable.Value, "Enable");
                SengaOnly.Value = GUILayout.Toggle(SengaOnly.Value, "SengaMode");

                SengaSampleDistance.Value = DrawSliderTextBoxCombo(
                    "SampleDistance", 0f, 3f, ref SengaSampleDistanceBuffer, SengaSampleDistance.Value, (float)SengaSampleDistance.DefaultValue
                );
                SengaNomalThes.Value = DrawSliderTextBoxCombo(
                    "NormalThreshold", 0.01f, 1f, ref SengaNomalThesBuffer, SengaNomalThes.Value, (float)SengaNomalThes.DefaultValue
                );
                SengaDepthThres.Value = DrawSliderTextBoxCombo(
                    "DepthThreshold", 0.01f, 10f, ref SengaDepthThresBuffer, SengaDepthThres.Value, (float)SengaDepthThres.DefaultValue
                );
                SengaColorThres.Value = DrawSliderTextBoxCombo(
                    "ColorThreshold", 0.001f, 1f, ref SengaColorThresBuffer, SengaColorThres.Value, (float)SengaColorThres.DefaultValue
                );
                SengaSobelThres.Value = DrawSliderTextBoxCombo(
                    "SobelThreshold", 0.01f, 10f, ref SengaSobelThresBuffer, SengaSobelThres.Value, (float)SengaSobelThres.DefaultValue
                );
                SengaNormalEdge.Value = DrawSliderTextBoxCombo(
                    "NormalEdge", 0f, 1f, ref SengaNormalEdgeBuffer, SengaNormalEdge.Value, (float)SengaNormalEdge.DefaultValue
                );
                SengaDepthEdge.Value = DrawSliderTextBoxCombo(
                    "DepthEdge", 0f, 1f, ref SengaDepthEdgeBuffer, SengaDepthEdge.Value, (float)SengaDepthEdge.DefaultValue
                );
                SengaColorEdge.Value = DrawSliderTextBoxCombo(
                    "ColorEdge", 0f, 1f, ref SengaColorEdgeBuffer, SengaColorEdge.Value, (float)SengaColorEdge.DefaultValue
                );
                SengaSobelEdge.Value = DrawSliderTextBoxCombo(
                    "SobelEdge", 0f, 1f, ref SengaSobelEdgeBuffer, SengaSobelEdge.Value, (float)SengaSobelEdge.DefaultValue
                );
                SengaColBlend.Value = DrawSliderTextBoxCombo(
                    "ColorBlend", 0f, 20f, ref SengaColBlendBuffer, SengaColBlend.Value, (float)SengaColBlend.DefaultValue
                );

                GUILayout.Label("Tone", GUILayout.Width(120f));
                SengaToneEnable.Value = GUILayout.Toggle(SengaToneEnable.Value, "PasteTone");

                SengaToneScale.Value = DrawSliderTextBoxCombo(
                    "ToneScale", 0.1f, 10f, ref SengaToneScaleBuffer, SengaToneScale.Value, (float)SengaToneScale.DefaultValue
                );
                SengaToneThick.Value = DrawSliderTextBoxCombo(
                    "ToneThickness", -1f, 1f, ref SengaToneThickBuffer, SengaToneThick.Value, (float)SengaToneThick.DefaultValue
                );
                SengaToneThres.Value = DrawSliderTextBoxCombo(
                    "ToneThreshold", -1f, 1f, ref SengaToneThresBuffer, SengaToneThres.Value, (float)SengaToneThres.DefaultValue
                );

                GUILayout.Label("Blur", GUILayout.Width(120f));
                SengaBlurEnable.Value = GUILayout.Toggle(SengaBlurEnable.Value, "BlurEnable");

                SengaBlurDir.Value = DrawSliderTextBoxCombo(
                    "BlurDirection", 0f, 10f, ref SengaBlurDirBuffer, SengaBlurDir.Value, (float)SengaBlurDir.DefaultValue
                );
                SengaBlurPow.Value = DrawSliderTextBoxCombo(
                    "BlurPower", 0f, 5f, ref SengaBlurPowBuffer, SengaBlurPow.Value, (float)SengaBlurPow.DefaultValue
                );
                SengaBlurThick.Value = DrawSliderTextBoxCombo(
                    "BlurThickness", 0f, 1f, ref SengaBlurThickBuffer, SengaBlurThick.Value, (float)SengaBlurThick.DefaultValue
                );
                SengaBlurSample.Value = (int)DrawSliderTextBoxCombo(
                    "BlurSampleCount", 2f, 64f, ref SengaBlurSampleBuffer, SengaBlurSample.Value, (int)SengaBlurSample.DefaultValue, true
                );
                GUILayout.EndVertical();
            }
            #endregion

            #region Distortion
            distortion = GUILayout.Toggle(distortion, "Lens Distortion ", GUI.skin.button);
            if (distortion)
            {
                GUILayout.BeginVertical();
                DistortionEnable.Value = GUILayout.Toggle(DistortionEnable.Value, "Enable");

                DistortionIntensity.Value = DrawSliderTextBoxCombo(
                    "Intensity", -100f, 100f, ref DistortionIntensityBuffer, DistortionIntensity.Value, (float)DistortionIntensity.DefaultValue
                );
                DistortionIntensityX.Value = DrawSliderTextBoxCombo(
                    "X Multiplier", 0f, 1f, ref DistortionIntensityXBuffer, DistortionIntensityX.Value, (float)DistortionIntensityX.DefaultValue
                );
                DistortionIntensityY.Value = DrawSliderTextBoxCombo(
                    "Y Multiplier", 0f, 1f, ref DistortionIntensityYBuffer, DistortionIntensityY.Value, (float)DistortionIntensityY.DefaultValue
                );
                DistortionCenterX.Value = DrawSliderTextBoxCombo(
                   "Center X", -1f, 1f, ref DistortionCenterXBuffer, DistortionCenterX.Value, (float)DistortionCenterX.DefaultValue
               );
                DistortionCenterY.Value = DrawSliderTextBoxCombo(
                   "Center Y", -1f, 1f, ref DistortionCenterYBuffer, DistortionCenterY.Value, (float)DistortionCenterY.DefaultValue
               );
                DistortionScale.Value = DrawSliderTextBoxCombo(
                   "Scale", 0.01f, 5f, ref DistortionScaleBuffer, DistortionScale.Value, (float)DistortionScale.DefaultValue
               );
                GUILayout.EndVertical();
            }
            #endregion

            #region Deferred Fog
            fog = GUILayout.Toggle(fog, "Deferred Fog", GUI.skin.button);
            if (fog)
            {
                GUILayout.BeginVertical();
                FogEnable.Value = GUILayout.Toggle(FogEnable.Value, "Enable");

                int selectedFogMode = Array.IndexOf(fogModes, FogModeSelected.Value);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Mode  ", GUILayout.Width(120f));
                selectedFogMode = GUILayout.SelectionGrid(selectedFogMode, fogModes2, 3, GUI.skin.toggle);
                FogModeSelected.Value = fogModes[selectedFogMode];
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    FogModeSelected.Value = (FogMode)FogModeSelected.DefaultValue;
                }
                GUILayout.EndHorizontal();

                FogHeight.Value = DrawSliderTextBoxCombo(
                   "Height", 0f, 100f, ref FogHeightBuffer, FogHeight.Value, (float)FogHeight.DefaultValue
               );
                if (FogModeSelected.Value == FogMode.Linear)
                {
                    FogStart.Value = DrawSliderTextBoxCombo(
                       "Start", 0f, 100f, ref FogStartBuffer, FogStart.Value, (float)FogStart.DefaultValue
                   );
                    FogEnd.Value = DrawSliderTextBoxCombo(
                       "End", 0f, 100f, ref FogEndBuffer, FogEnd.Value, (float)FogEnd.DefaultValue
                   );
                }
                FogDensity.Value = DrawSliderTextBoxCombo(
                    "Density", 0f, 100f, ref FogDensityBuffer, FogDensity.Value, (float)FogDensity.DefaultValue
                );

                GUILayout.BeginHorizontal();
                GUILayout.Label("Color   ");
                if (GUILayout.Button("", colorbutton(FogColor.Value)) && (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio || KoikatuAPI.GetCurrentGameMode() != GameMode.Maker))
                {
                    Action<Color> act3 = delegate (Color c)
                    {
                        FogColor.Value = c;
                    };
                    ColorPicker(FogColor.Value, act3);
                }
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    FogColor.Value = (Color)FogColor.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            #endregion

            #region Grain
            grainShown = GUILayout.Toggle(grainShown, "Grain", GUI.skin.button);
            if (grainShown)
            {
                GUILayout.BeginVertical();
                GrainEnable.Value = GUILayout.Toggle(GrainEnable.Value, "Enable");
                GrainColored.Value = GUILayout.Toggle(GrainColored.Value, "Colored");

                GrainIntensity.Value = DrawSliderTextBoxCombo(
                    "Intensity", 0f, 1f, ref GrainIntensityBuffer, GrainIntensity.Value, (float)GrainIntensity.DefaultValue
                );
                GrainSize.Value = DrawSliderTextBoxCombo(
                    "Size", 0.3f, 3f, ref GrainSizeBuffer, GrainSize.Value, (float)GrainSize.DefaultValue
                );
                GrainLumContrib.Value = DrawSliderTextBoxCombo(
                    "Luminance Contribution", 0f, 1f, ref GrainLumContribBuffer, GrainLumContrib.Value, (float)GrainLumContrib.DefaultValue
                );
                GUILayout.EndVertical();
            }
            #endregion

            GUI.DragWindow();
        }

        private GUIStyle colorbutton(Color col)
        {
            GUIStyle gUIStyle = new GUIStyle();
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);
            texture2D.SetPixel(0, 0, col);
            texture2D.Apply();
            gUIStyle.normal.background = texture2D;
            return gUIStyle;
        }

        public void ColorPicker(Color col, Action<Color> act)
        {
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Studio)
            {
                if (studio.colorPalette.visible)
                {
                    studio.colorPalette.visible = false;
                }
                else
                {
                    studio.colorPalette.Setup("ColorPicker", col, act, true);
                    studio.colorPalette.visible = true;
                }
            }
            if (KoikatuAPI.GetCurrentGameMode() == GameMode.Maker)
            {
                CvsColor component = GameObject.Find("CustomScene/CustomRoot/FrontUIGroup/CustomUIGroup/CvsColor/Top").GetComponent<CvsColor>();
                if (component.isOpen)
                {
                    component.Close();
                }
                else
                {
                    component.Setup("ColorPicker", CvsColor.ConnectColorKind.None, col, act, true);
                }
            }
        }
        #endregion

        #region Config

        #region Define Configs
        private ConfigEntry<bool> onoff { get; set; }
        private ConfigEntry<KeyboardShortcut> OpenGUI { get; set; }
        private static ConfigEntry<KeyboardShortcut> MasterSwitch { get; set; }

        #region Anti-Aliasing
        private ConfigEntry<PostProcessLayer.Antialiasing> AAmode { get; set; }
        private ConfigEntry<SubpixelMorphologicalAntialiasing.Quality> AAsmaaq { get; set; }
        private ConfigEntry<bool> AAfxaafm { get; set; }
        private ConfigEntry<bool> AAfxaakpa { get; set; }
        private ConfigEntry<float> TAAjittetSpeed { get; set; }
        private ConfigEntry<float> TAAsharpen { get; set; }
        private ConfigEntry<float> TAAstationaryBlending { get; set; }
        private ConfigEntry<float> TAAmotionBlending { get; set; }
        #endregion

        #region Ambient Occlusion
        private ConfigEntry<bool> AOenable { get; set; }
        private ConfigEntry<bool> AOmodesel { get; set; }
        private ConfigEntry<AmbientOcclusionMode> AOmode { get; set; }
        private ConfigEntry<float> AOintensity { get; set; }
        private ConfigEntry<Color> AOcolor { get; set; }
        private ConfigEntry<float> AOradius { get; set; }
        private ConfigEntry<AmbientOcclusionQuality> AOquality { get; set; }

        private ConfigEntry<SSAOProUtils.SSAOPro.SampleCount> cSampleCount { get; set; }
        private ConfigEntry<int> cDownsampling { get; set; }
        private ConfigEntry<float> cIntensity { get; set; }
        private ConfigEntry<float> cRadius { get; set; }
        private ConfigEntry<float> cDistance { get; set; }
        private ConfigEntry<float> cBias { get; set; }
        private ConfigEntry<float> cLithingCont { get; set; }
        private ConfigEntry<Color> cOccColor { get; set; }
        private ConfigEntry<SSAOProUtils.SSAOPro.BlurMode> cBlurType { get; set; }
        private ConfigEntry<bool> cBlurDownS { get; set; }
        private ConfigEntry<int> cBlurPasses { get; set; }
        private ConfigEntry<float> cThres { get; set; }
        private ConfigEntry<float> cMaxDistance { get; set; }
        private ConfigEntry<float> cFalloff { get; set; }
        #endregion

        #region Color Grading
        private ConfigEntry<bool> CGenable { get; set; }
        private ConfigEntry<Tonemapper> CGtoneMapper { get; set; }
        private ConfigEntry<GradingMode> CGgradingmode { get; set; }
        private ConfigEntry<float> CGposte { get; set; }
        private ConfigEntry<Color> CGcolfilter { get; set; }
        private ConfigEntry<float> CGtemp { get; set; }
        private ConfigEntry<float> CGtint { get; set; }
        private ConfigEntry<float> CGhueShift { get; set; }
        private ConfigEntry<float> CGsaturation { get; set; }
        private ConfigEntry<float> CGcontrast { get; set; }
        private ConfigEntry<Vector4> CGlift { get; set; }
        private ConfigEntry<Vector4> CGgain { get; set; }
        private ConfigEntry<Vector4> CGgamma { get; set; }
        #endregion

        #region Bloom
        private ConfigEntry<bool> Bloomenable { get; set; }
        private ConfigEntry<float> Bloomintensity { get; set; }
        private ConfigEntry<float> Bloomdiffusion { get; set; }
        private ConfigEntry<float> BloomsoftKnee { get; set; }
        private ConfigEntry<float> Bloomthreshold { get; set; }
        private ConfigEntry<float> Bloomclamp { get; set; }
        private ConfigEntry<float> Bloomanamor { get; set; }
        private ConfigEntry<Color> Bloomcolor { get; set; }
        private ConfigEntry<bool> Bloomfsmd { get; set; }
        private ConfigEntry<bool> MBenable { get; set; }
        private ConfigEntry<float> MBshutter { get; set; }
        private ConfigEntry<int> MBsamplecnt { get; set; }
        #endregion

        #region Depth of Field
        private ConfigEntry<bool> DOFenable { get; set; }
        private ConfigEntry<float> DOFfocall { get; set; }
        private ConfigEntry<float> DOFfocusd { get; set; }
        private ConfigEntry<float> DOFaperture { get; set; }
        private ConfigEntry<KernelSize> DOFmaxblur { get; set; }
        private ConfigEntry<bool> DOFautofocus { get; set; }
        private ConfigEntry<int> DOFAFmode { get; set; }
        #endregion

        #region Vignette
        private ConfigEntry<bool> VGenable { get; set; }
        private ConfigEntry<Color> VGcol { get; set; }
        private ConfigEntry<VignetteMode> VGmode { get; set; }
        private ConfigEntry<float> VGopacity { get; set; }
        private ConfigEntry<bool> VGrounded { get; set; }
        private ConfigEntry<float> VGroundness { get; set; }
        private ConfigEntry<float> VGsmoothness { get; set; }
        private ConfigEntry<float> VGintensity { get; set; }
        private ConfigEntry<Vector2> VGcenter { get; set; }
        #endregion

        #region Chromatic Aberration
        private ConfigEntry<float> CAintensity { get; set; }
        private ConfigEntry<bool> CAenable { get; set; }
        #endregion

        #region Outline
        private ConfigEntry<float> OutlineWidth { get; set; }
        private ConfigEntry<Color> OutlineColor { get; set; }
        private ConfigEntry<float> ColorPower { get; set; }
        private ConfigEntry<bool> SoutlineEnable { get; set; }

        private ConfigEntry<bool> SengaEnable { get; set; }
        private ConfigEntry<bool> SengaOnly { get; set; }
        private ConfigEntry<float> SengaSampleDistance { get; set; }
        private ConfigEntry<float> SengaNomalThes { get; set; }
        private ConfigEntry<float> SengaDepthThres { get; set; }
        private ConfigEntry<float> SengaColorThres { get; set; }
        private ConfigEntry<float> SengaSobelThres { get; set; }
        private ConfigEntry<float> SengaNormalEdge { get; set; }
        private ConfigEntry<float> SengaDepthEdge { get; set; }
        private ConfigEntry<float> SengaColorEdge { get; set; }
        private ConfigEntry<float> SengaSobelEdge { get; set; }
        private ConfigEntry<float> SengaColBlend { get; set; }
        private ConfigEntry<float> SengaToneThres { get; set; }
        private ConfigEntry<float> SengaToneScale { get; set; }
        private ConfigEntry<float> SengaToneThick { get; set; }
        private ConfigEntry<bool> SengaToneEnable { get; set; }
        private ConfigEntry<float> SengaBlurDir { get; set; }
        private ConfigEntry<float> SengaBlurPow { get; set; }
        private ConfigEntry<float> SengaBlurThick { get; set; }
        private ConfigEntry<int> SengaBlurSample { get; set; }
        private ConfigEntry<bool> SengaBlurEnable { get; set; }
        #endregion

        #region Posterize
        private ConfigEntry<int> PosDiv { get; set; }
        private ConfigEntry<bool> PosEnable { get; set; }
        private ConfigEntry<bool> PosHSV { get; set; }
        #endregion

        #region Distortion
        private ConfigEntry<bool> DistortionEnable { get; set; }
        private ConfigEntry<float> DistortionIntensity { get; set; }
        private ConfigEntry<float> DistortionIntensityX { get; set; }
        private ConfigEntry<float> DistortionIntensityY { get; set; }
        private ConfigEntry<float> DistortionCenterX { get; set; }
        private ConfigEntry<float> DistortionCenterY { get; set; }
        private ConfigEntry<float> DistortionScale { get; set; }
        #endregion

        #region Deferred Fog

        private ConfigEntry<bool> FogEnable { get; set; }
        private ConfigEntry<FogMode> FogModeSelected { get; set; }
        private ConfigEntry<float> FogDensity { get; set; }
        private ConfigEntry<float> FogStart { get; set; }
        private ConfigEntry<float> FogEnd { get; set; }
        private ConfigEntry<float> FogHeight { get; set; }
        private ConfigEntry<Color> FogColor { get; set; }


        #endregion

        #region Grain

        private ConfigEntry<bool> GrainEnable { get; set; }
        private ConfigEntry<bool> GrainColored { get; set; }
        private ConfigEntry<float> GrainIntensity { get; set; }
        private ConfigEntry<float> GrainSize { get; set; }
        private ConfigEntry<float> GrainLumContrib { get; set; }

        #endregion
        #endregion

        private void BindConfig()
        {
            onoff = base.Config.Bind("_MasterSwitch", "OnOff", false, "");
            AAmode = base.Config.Bind("AntiAliasing", "AntiAliasing Mode", PostProcessLayer.Antialiasing.None, "");
            AAsmaaq = base.Config.Bind("AntiAliasing", "SMAA Quality", SubpixelMorphologicalAntialiasing.Quality.Medium, "");
            AAfxaafm = base.Config.Bind("AntiAliasing", "FXAA FastMode", false, "");
            AAfxaakpa = base.Config.Bind("AntiAliasing", "FXAA KeepAlpha", false, "");
            TAAjittetSpeed = base.Config.Bind("AntiAliasing", "TAA JitterSpeed", 0.75f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 1f)));
            TAAsharpen = base.Config.Bind("AntiAliasing", "TAA Sharpen", 0.3f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 3f)));
            TAAstationaryBlending = base.Config.Bind("AntiAliasing", "TAA StationaryBlending", 0.95f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 0.99f)));
            TAAmotionBlending = base.Config.Bind("AntiAliasing", "TAA MotionBlending", 0.85f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 0.99f)));
            AOmode = base.Config.Bind("Ambient Occulusion", "Ambient Occulusion Mode", AmbientOcclusionMode.ScalableAmbientObscurance, "");
            AOenable = base.Config.Bind("Ambient Occulusion", "Ambient Occulusion Enable", false, "");
            AOintensity = base.Config.Bind("Ambient Occulusion", "AOIntensity", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 4f)));
            AOcolor = base.Config.Bind("Ambient Occulusion", "Color", Color.black, "");
            AOradius = base.Config.Bind("Ambient Occulusion", "Radius", 0.25f, new ConfigDescription("", new AcceptableValueRange<float>(0.0001f, 3f)));
            AOquality = base.Config.Bind("Ambient Occulusion", "Quality", AmbientOcclusionQuality.Medium, "");
            AOmodesel = base.Config.Bind("Ambient Occulusion", "UseNewMode", true, "");
            cIntensity = base.Config.Bind("Ambient Occulusion", "Intensity", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 16f)));
            cOccColor = base.Config.Bind("Ambient Occulusion", "Color", Color.black, "");
            cRadius = base.Config.Bind("Ambient Occulusion", "Radius", 0.25f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 1.25f)));
            cSampleCount = base.Config.Bind("Ambient Occulusion", "SampleCount", SSAOProUtils.SSAOPro.SampleCount.Medium, "");
            cBlurDownS = base.Config.Bind("Ambient Occulusion", "BlurDownsampling", false, "");
            cDownsampling = base.Config.Bind("Ambient Occulusion", "DownSampling", 1, new ConfigDescription("", new AcceptableValueRange<int>(1, 4)));
            cDistance = base.Config.Bind("Ambient Occulusion", "Distance", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 10f)));
            cBias = base.Config.Bind("Ambient Occulusion", "Bias", 0.1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            cLithingCont = base.Config.Bind("Ambient Occulusion", "LightingContribution", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            cBlurType = base.Config.Bind("Ambient Occulusion", "BlurType", SSAOProUtils.SSAOPro.BlurMode.HighQualityBilateral, "");
            cBlurPasses = base.Config.Bind("Ambient Occulusion", "BlurPasses", 1, new ConfigDescription("", new AcceptableValueRange<int>(1, 4)));
            cThres = base.Config.Bind("Ambient Occulusion", "Threshold", 10f, new ConfigDescription("", new AcceptableValueRange<float>(1f, 20f)));
            cMaxDistance = base.Config.Bind("Ambient Occulusion", "MaxDistance", 150f, "");
            cFalloff = base.Config.Bind("Ambient Occulusion", "Falloff", 50f, "");
            Bloomenable = base.Config.Bind("Bloom", "_Bloom Enable", false, "");
            Bloomintensity = base.Config.Bind("Bloom", "Intensity", 3f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 10f)));
            Bloomanamor = base.Config.Bind("Bloom", "AnamorphicRatio", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            BloomsoftKnee = base.Config.Bind("Bloom", "Softknee", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            Bloomthreshold = base.Config.Bind("Bloom", "Threshold", 1.1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 10f)));
            Bloomclamp = base.Config.Bind("Bloom", "Clamp", 65472f, "");
            Bloomdiffusion = base.Config.Bind("Bloom", "Diffusion", 7f, new ConfigDescription("", new AcceptableValueRange<float>(1f, 10f)));
            Bloomcolor = base.Config.Bind("Bloom", "Color", Color.white, "");
            Bloomfsmd = base.Config.Bind("Bloom", "FastMode", false, "");
            CGenable = base.Config.Bind("Color Grading", "_Color Grading Enable", false, "");
            CGtoneMapper = base.Config.Bind("Color Grading", "ToneMapper", Tonemapper.None, "");
            CGgradingmode = base.Config.Bind("Color Grading", "GradingMode", GradingMode.LowDefinitionRange, "");
            CGposte = base.Config.Bind("Color Grading", "Tone PostExposure", 0f, "");
            CGtemp = base.Config.Bind("Color Grading", "WhiteBalance Temperature", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-100f, 100f)));
            CGtint = base.Config.Bind("Color Grading", "WhiteBalance Tint", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-100f, 100f)));
            CGhueShift = base.Config.Bind("Color Grading", "Tone HueShift", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-180f, 180f)));
            CGsaturation = base.Config.Bind("Color Grading", "Tone Saturation", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-100f, 100f)));
            CGcontrast = base.Config.Bind("Color Grading", "Tone Contrast", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-100f, 100f)));
            CGlift = base.Config.Bind("Color Grading", "Trackballs Lift", new Vector4(1f, 1f, 1f, 0f), "");
            CGgamma = base.Config.Bind("Color Grading", "Trackballs Gammma", new Vector4(1f, 1f, 1f, 0f), "");
            CGgain = base.Config.Bind("Color Grading", "Trackballs Gain", new Vector4(1f, 1f, 1f, 0f), "");
            CGcolfilter = base.Config.Bind("Color Grading", "Tone ColorFilter", Color.white, "");
            CAenable = base.Config.Bind("Chromatic Aberration", "_Chromatic Aberration Enable", false, "");
            CAintensity = base.Config.Bind("Chromatic Aberration", "Intensity", 0f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            MBenable = base.Config.Bind("Motion Blur", "_MotionBlur Enable", false, "");
            MBshutter = base.Config.Bind("Motion Blur", "ShutterAngle", 270f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 360f)));
            MBsamplecnt = base.Config.Bind("Motion Blur", "SampleCount", 10, new ConfigDescription("", new AcceptableValueRange<int>(4, 32)));
            DOFenable = base.Config.Bind("Depth of Field", "_DOF Enable", false, "");
            DOFfocusd = base.Config.Bind("Depth of Field", "FocusDistance", 10f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 100f)));
            DOFaperture = base.Config.Bind("Depth of Field", "Aperture", 5.6f, new ConfigDescription("", new AcceptableValueRange<float>(0.05f, 32f)));
            DOFfocall = base.Config.Bind("Depth of Field", "FoculLength", 50f, new ConfigDescription("", new AcceptableValueRange<float>(1f, 300f)));
            DOFmaxblur = base.Config.Bind("Depth of Field", "MaxBlurSize", KernelSize.Medium, "");
            VGenable = base.Config.Bind("Vignette", "_Vignette Enable", false, "");
            VGcenter = base.Config.Bind("Vignette", "Canter", new Vector2(0.5f, 0.5f), "");
            VGmode = base.Config.Bind("Vignette", "Mode", VignetteMode.Classic, "");
            VGcol = base.Config.Bind("Vignette", "Color", new Color(0f, 0f, 0f, 1f), "");
            VGintensity = base.Config.Bind("Vignette", "Intensity", 0f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            VGsmoothness = base.Config.Bind("Vignette", "Smoothness", 0.2f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 1f)));
            VGroundness = base.Config.Bind("Vignette", "Roundness", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            VGrounded = base.Config.Bind("Vignette", "Rounded", false, "");
            VGopacity = base.Config.Bind("Vignette", "Opacity", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            DOFautofocus = base.Config.Bind("Depth of Field", "AutoFocus", false, "");
            DOFAFmode = base.Config.Bind("Depth of Field", "AutoFocusMode", 0, new ConfigDescription("", new AcceptableValueRange<int>(0, 2)));
            MasterSwitch = base.Config.Bind("_MasterSwitch", "_Switch", default(KeyboardShortcut), "");
            OpenGUI = base.Config.Bind("_OpenGUI", "_OpenGUI", default(KeyboardShortcut), "");
            SoutlineEnable = base.Config.Bind("SobelColorOutline", "_Enable", false, "");
            OutlineWidth = base.Config.Bind("SobelColorOutline", "OutlineWidth", 0.02f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            OutlineColor = base.Config.Bind("SobelColorOutline", "OutlineColor", Color.white, "");
            ColorPower = base.Config.Bind("SobelColorOutline", "ColorPower", 2f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 10f)));
            PosEnable = base.Config.Bind("Posterize", "_Enable", false, "");
            PosDiv = base.Config.Bind("Posterize", "DivisionNum", 8, new ConfigDescription("", new AcceptableValueRange<int>(1, 64)));
            PosHSV = base.Config.Bind("Posterize", "UseHSVtrans", true, "");
            SengaEnable = base.Config.Bind("CustomizableOutline", "_Enable", false, "");
            SengaOnly = base.Config.Bind("CustomizableOutline", "OutlineOnly", false, "");
            SengaNomalThes = base.Config.Bind("CustomizableOutline", "NormalThreshold", 0.1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 1f)));
            SengaDepthThres = base.Config.Bind("CustomizableOutline", "DepthThreshold", 5f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 10f)));
            SengaColorThres = base.Config.Bind("CustomizableOutline", "ColorThreshold", 0.01f, new ConfigDescription("", new AcceptableValueRange<float>(0.001f, 1f)));
            SengaSobelThres = base.Config.Bind("CustomizableOutline", "SobelThreshold", 5f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 10f)));
            SengaNormalEdge = base.Config.Bind("CustomizableOutline", "NormalEdge", 0.5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            SengaDepthEdge = base.Config.Bind("CustomizableOutline", "DepthEdge", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            SengaColorEdge = base.Config.Bind("CustomizableOutline", "ColorEdge", 0.3f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            SengaSobelEdge = base.Config.Bind("CustomizableOutline", "SobelEdge", 0.3f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            SengaSampleDistance = base.Config.Bind("CustomizableOutline", "SampleDistance", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 3f)));
            SengaColBlend = base.Config.Bind("CustomizableOutline", "ColorBlend", 5f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 20f)));
            SengaToneEnable = base.Config.Bind("CustomizableOutline", "PasteTone", false, "");
            SengaToneScale = base.Config.Bind("CustomizableOutline", "ToneScale", 5f, new ConfigDescription("", new AcceptableValueRange<float>(0.1f, 10f)));
            SengaToneThick = base.Config.Bind("CustomizableOutline", "ToneThickness", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            SengaToneThres = base.Config.Bind("CustomizableOutline", "ToneThreshold", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            SengaBlurEnable = base.Config.Bind("CustomizableOutline", "BlurOn", false, "");
            SengaBlurDir = base.Config.Bind("CustomizableOutline", "BlurDirection", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 10f)));
            SengaBlurPow = base.Config.Bind("CustomizableOutline", "BlurPower", 0.6f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 5f)));
            SengaBlurThick = base.Config.Bind("CustomizableOutline", "BlurThickness", 0.3f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            SengaBlurSample = base.Config.Bind("CustomizableOutline", "BlurSampleCount", 8, new ConfigDescription("", new AcceptableValueRange<int>(2, 64)));
            DistortionEnable = base.Config.Bind("Lens Distortion", "_Enable", false, "");
            DistortionIntensity = base.Config.Bind("Lens Distortion", "Intensity", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-100f, 100f)));
            DistortionIntensityX = base.Config.Bind("Lens Distortion", "X Multiplier", 0f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1)));
            DistortionIntensityY = base.Config.Bind("Lens Distortion", "Y Mulitplier", 0f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            DistortionCenterX = base.Config.Bind("Lens Distortion", "X Center", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            DistortionCenterY = base.Config.Bind("Lens Distortion", "Y Center", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            DistortionScale = base.Config.Bind("Lens Distortion", "Scale", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 5f)));
            FogEnable = base.Config.Bind("Fog", "_Enable", false, "");
            FogModeSelected = base.Config.Bind("Fog", "mode", FogMode.Exponential);
            FogDensity = base.Config.Bind("Fog", "Density", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 100f)));
            FogStart = base.Config.Bind("Fog", "Start", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 100f)));
            FogEnd = base.Config.Bind("Fog", "End", 20f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 100f)));
            FogHeight = base.Config.Bind("Fog", "Height", 20f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 100f)));
            FogColor = base.Config.Bind("Fog", "Color", Color.white, "");
            GrainEnable = base.Config.Bind("Grain", "_Enable", false, "");
            GrainColored = base.Config.Bind("Grain", "Colored", false, "");
            GrainIntensity = base.Config.Bind("Grain", "Intensity", 0f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            GrainSize = base.Config.Bind("Grain", "Size", 1f, new ConfigDescription("", new AcceptableValueRange<float>(0.3f, 3f)));
            GrainLumContrib = base.Config.Bind("Grain", "Luminance Contribution", 0.8f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            UpdateBuffers();
        }
        #endregion
    }
}
