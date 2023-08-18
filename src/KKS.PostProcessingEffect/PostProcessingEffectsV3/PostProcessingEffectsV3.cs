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
using SobelOutline;
using SSAOProUtils;
using Studio;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PostProcessingEffectsV3
{
    [BepInDependency("org.bepinex.plugins.KKS_PostProcessingRuntime", "1.0.0.0")]
    [BepInPlugin("org.bepinex.plugins.KKS_PostProcessingEffectsV3", "KKS_PostProcessingEffectsV3", "1.4.0.0")]
    public class PostProcessingEffectsV3 : BaseUnityPlugin
    {

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
        private Bloom bloom;
        private ColorGrading CG;
        private MotionBlur MB;
        private DepthOfField DOF;
        private Vignette VG;
        private ChromaticAberration CA;
        #endregion

        #region Setup

        private global::Studio.Studio studio;

        private Dictionary<ChaControl, Transform> CharaList = new Dictionary<ChaControl, Transform>();


        private void wall()
        {
            myGO = new GameObject();
            myGO.name = "TestCanvas";
            myGO.AddComponent<Canvas>();
            myCanvas = myGO.GetComponent<Canvas>();
            myCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            myCanvas.sortingOrder = 32767;
            myGO.AddComponent<CanvasScaler>();
            myGO.AddComponent<GraphicRaycaster>();
            GameObject gameObject = new GameObject("Button");
            Image image = gameObject.AddComponent<Image>();
            image.transform.SetParent(myCanvas.transform);
            image.rectTransform.sizeDelta = new Vector2(Screen.width, Screen.height);
            image.rectTransform.anchoredPosition = Vector3.zero;
            image.color = new Color(1f, 1f, 1f, 0f);
            image.raycastTarget = true;
        }

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
                if (mainwin)
                {
                    wall();
                }
                else
                {
                    UnityEngine.Object.Destroy(myGO);
                }
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
                if (!postProcessVolume.profile.HasSettings<Bloom>())
                {
                    bloom = postProcessVolume.profile.AddSettings<Bloom>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<Bloom>(out bloom);
                }
                if (!postProcessVolume.profile.HasSettings<ColorGrading>())
                {
                    CG = postProcessVolume.profile.AddSettings<ColorGrading>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<ColorGrading>(out CG);
                }
                if (!postProcessVolume.profile.HasSettings<MotionBlur>())
                {
                    MB = postProcessVolume.profile.AddSettings<MotionBlur>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<MotionBlur>(out MB);
                }
                if (!postProcessVolume.profile.HasSettings<DepthOfField>())
                {
                    DOF = postProcessVolume.profile.AddSettings<DepthOfField>();
                }
                else
                {
                    postProcessVolume.profile.TryGetSettings<DepthOfField>(out DOF);
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
        public Rect Rect1 = new Rect(220f, 50f, 420f, 420f);

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

        #region Buffers
        private string DistortionIntensityBuffer = "";
        #endregion


        private void OnGUI()
        {
            if (mainwin)
            {
                if (GUI.Button(new Rect(0f, 0f, Screen.width, Screen.height), "", GUI.skin.label))
                {
                    mainwin = false;
                    UnityEngine.Object.Destroy(myGO);
                }
                Rect1 = GUILayout.Window(560, Rect1, mainwindow, "PostProcessingEffects");
            }
        }

        private float DrawSliderTextBoxCombo(string label, float min, float max, ref string buffer, float value, float valueDefault)
        {
            float newValue = value;
            string focused = GUI.GetNameOfFocusedControl();
            if (focused == label && (Event.current.keyCode == KeyCode.KeypadEnter || Event.current.keyCode == KeyCode.Return))
                GUI.FocusControl(null);

            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(120f));

            float sliderBuffer = GUILayout.HorizontalSlider(value, min, max);

            GUI.SetNextControlName(label);
            buffer = GUILayout.TextField(buffer.ToString(), GUILayout.Width(50));

            if (focused != label)
            {
                if (!float.TryParse(buffer, out float valueBufferFloat))
                {
                    valueBufferFloat = value;
                    buffer = value.ToString();
                }
                if (valueBufferFloat != value)
                    newValue = valueBufferFloat;
                else if (sliderBuffer != value)
                {
                    newValue = sliderBuffer;
                    buffer = newValue.ToString();
                }
            }

            if (GUILayout.Button("Reset", GUILayout.Width(60f)))
            {
                newValue = valueDefault;
                buffer = valueDefault.ToString();
            }

            GUILayout.EndHorizontal();
            if (value.CompareTo(min) < 0) return min;
            else if (value.CompareTo(max) > 0) return max;
            else return value;
        }

        private void mainwindow(int windowID)
        {
            #region Ambient Occulusion
            onoff.Value = GUILayout.Toggle(onoff.Value, "Enable/Disable All");
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
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Downsampling", GUILayout.Width(120f));
                    cDownsampling.Value = (int)GUILayout.HorizontalSlider(cDownsampling.Value, 1f, 4f);
                    GUILayout.Label(cDownsampling.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cDownsampling.Value = (int)cDownsampling.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Intensity", GUILayout.Width(120f));
                    cIntensity.Value = GUILayout.HorizontalSlider(cIntensity.Value, 0f, 16f);
                    GUILayout.Label(cIntensity.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cIntensity.Value = (float)cIntensity.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Radius   ", GUILayout.Width(120f));
                    cRadius.Value = GUILayout.HorizontalSlider(cRadius.Value, 0.01f, 1.25f);
                    GUILayout.Label(cRadius.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cRadius.Value = (float)cRadius.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Distance   ", GUILayout.Width(120f));
                    cDistance.Value = GUILayout.HorizontalSlider(cDistance.Value, 0f, 10f);
                    GUILayout.Label(cDistance.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cDistance.Value = (float)cDistance.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Bias   ", GUILayout.Width(120f));
                    cBias.Value = GUILayout.HorizontalSlider(cBias.Value, 0f, 1f);
                    GUILayout.Label(cBias.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cBias.Value = (float)cBias.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("LightingContribution   ", GUILayout.Width(120f));
                    cLithingCont.Value = GUILayout.HorizontalSlider(cLithingCont.Value, 0f, 1f);
                    GUILayout.Label(cLithingCont.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cLithingCont.Value = (float)cLithingCont.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
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
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("BlurPasses", GUILayout.Width(120f));
                    cBlurPasses.Value = (int)GUILayout.HorizontalSlider(cBlurPasses.Value, 1f, 4f);
                    GUILayout.Label(cBlurPasses.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cBlurPasses.Value = (int)cBlurPasses.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("BlurThreshold", GUILayout.Width(120f));
                    cThres.Value = GUILayout.HorizontalSlider(cThres.Value, 1f, 20f);
                    GUILayout.Label(cThres.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        cThres.Value = (float)cThres.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
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
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Intensity", GUILayout.Width(120f));
                    AOintensity.Value = GUILayout.HorizontalSlider(AOintensity.Value, 0f, 4f);
                    GUILayout.Label(AOintensity.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        AOintensity.Value = (float)AOintensity.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Radius   ", GUILayout.Width(120f));
                    AOradius.Value = GUILayout.HorizontalSlider(AOradius.Value, 0.0001f, 3f);
                    GUILayout.Label(AOradius.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        AOradius.Value = (float)AOradius.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
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
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("JitterSpread", GUILayout.Width(120f));
                    TAAjittetSpeed.Value = GUILayout.HorizontalSlider(TAAjittetSpeed.Value, 0.1f, 1f);
                    GUILayout.Label(TAAjittetSpeed.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        TAAjittetSpeed.Value = (float)TAAjittetSpeed.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("StationaryBlending", GUILayout.Width(120f));
                    TAAstationaryBlending.Value = GUILayout.HorizontalSlider(TAAstationaryBlending.Value, 0.1f, 1f);
                    GUILayout.Label(TAAstationaryBlending.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        TAAstationaryBlending.Value = (float)TAAstationaryBlending.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("MotionBlending", GUILayout.Width(120f));
                    TAAmotionBlending.Value = GUILayout.HorizontalSlider(TAAmotionBlending.Value, 0.1f, 1f);
                    GUILayout.Label(TAAmotionBlending.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        TAAmotionBlending.Value = (float)TAAmotionBlending.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Sharpen", GUILayout.Width(120f));
                    TAAsharpen.Value = GUILayout.HorizontalSlider(TAAsharpen.Value, 0.1f, 1f);
                    GUILayout.Label(TAAsharpen.Value.ToString("F"), GUILayout.Width(40f));
                    if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                    {
                        TAAsharpen.Value = (float)TAAsharpen.DefaultValue;
                    }
                    GUILayout.EndHorizontal();
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
                GUILayout.BeginHorizontal();
                GUILayout.Label("Intensity  ", GUILayout.Width(120f));
                Bloomintensity.Value = GUILayout.HorizontalSlider(Bloomintensity.Value, 0f, 10f);
                GUILayout.Label(Bloomintensity.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    Bloomintensity.Value = (float)Bloomintensity.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Threshold", GUILayout.Width(120f));
                Bloomthreshold.Value = GUILayout.HorizontalSlider(Bloomthreshold.Value, 0f, 5f);
                GUILayout.Label(Bloomthreshold.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    Bloomthreshold.Value = (float)Bloomthreshold.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("SoftKnee", GUILayout.Width(120f));
                BloomsoftKnee.Value = GUILayout.HorizontalSlider(BloomsoftKnee.Value, 0f, 1f);
                GUILayout.Label(BloomsoftKnee.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    BloomsoftKnee.Value = (float)BloomsoftKnee.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Clamp", GUILayout.Width(120f));
                Bloomclamp.Value = GUILayout.HorizontalSlider(Bloomclamp.Value, 0f, 1f);
                GUILayout.Label(Bloomclamp.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    Bloomclamp.Value = (float)Bloomclamp.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Diffusion", GUILayout.Width(120f));
                Bloomdiffusion.Value = GUILayout.HorizontalSlider(Bloomdiffusion.Value, 1f, 10f);
                GUILayout.Label(Bloomdiffusion.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    Bloomdiffusion.Value = (float)Bloomdiffusion.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("AnamorphicPatio", GUILayout.Width(120f));
                Bloomanamor.Value = GUILayout.HorizontalSlider(Bloomanamor.Value, -1f, 1f);
                GUILayout.Label(Bloomanamor.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    Bloomanamor.Value = (float)Bloomanamor.DefaultValue;
                }
                GUILayout.EndHorizontal();
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
                GUILayout.BeginHorizontal();
                GUILayout.Label("Intensity  ", GUILayout.Width(120f));
                CAintensity.Value = GUILayout.HorizontalSlider(CAintensity.Value, 0f, 1f);
                GUILayout.Label(CAintensity.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CAintensity.Value = (float)CAintensity.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
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
                GUILayout.BeginHorizontal();
                GUILayout.Label("Temperature  ", GUILayout.Width(120f));
                CGtemp.Value = GUILayout.HorizontalSlider(CGtemp.Value, -100f, 100f);
                GUILayout.Label(CGtemp.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CGtemp.Value = (float)CGtemp.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Tint", GUILayout.Width(120f));
                CGtint.Value = GUILayout.HorizontalSlider(CGtint.Value, -100f, 100f);
                GUILayout.Label(CGtint.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CGtint.Value = (float)CGtint.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("Tone  ", GUILayout.Width(120f));
                GUILayout.BeginHorizontal();
                GUILayout.Label("PostExposure", GUILayout.Width(120f));
                CGposte.Value = GUILayout.HorizontalSlider(CGposte.Value, 0f, 1f);
                GUILayout.Label(CGposte.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CGposte.Value = (float)CGposte.DefaultValue;
                }
                GUILayout.EndHorizontal();
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
                GUILayout.BeginHorizontal();
                GUILayout.Label("HueShift", GUILayout.Width(120f));
                CGhueShift.Value = GUILayout.HorizontalSlider(CGhueShift.Value, -180f, 180f);
                GUILayout.Label(CGhueShift.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CGhueShift.Value = (float)CGhueShift.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Saturation", GUILayout.Width(120f));
                CGsaturation.Value = GUILayout.HorizontalSlider(CGsaturation.Value, -100f, 100f);
                GUILayout.Label(CGsaturation.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CGsaturation.Value = (float)CGsaturation.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Contrast", GUILayout.Width(120f));
                CGcontrast.Value = GUILayout.HorizontalSlider(CGcontrast.Value, -100f, 100f);
                GUILayout.Label(CGcontrast.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    CGcontrast.Value = (float)CGcontrast.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            #endregion

            #region Depth of Field
            DOFb = GUILayout.Toggle(DOFb, "DepthOfField ", GUI.skin.button);
            if (DOFb)
            {
                GUILayout.BeginVertical();
                DOFenable.Value = GUILayout.Toggle(DOFenable.Value, "Enable");
                GUILayout.BeginHorizontal();
                GUILayout.Label("FocusDistance  ", GUILayout.Width(120f));
                DOFfocusd.Value = GUILayout.HorizontalSlider(DOFfocusd.Value, 0f, 50f);
                GUILayout.Label(DOFfocusd.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    DOFfocusd.Value = (float)DOFfocusd.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Aperture", GUILayout.Width(120f));
                DOFaperture.Value = GUILayout.HorizontalSlider(DOFaperture.Value, 0.1f, 32f);
                GUILayout.Label(DOFaperture.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    DOFaperture.Value = (float)DOFaperture.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("FocalLength", GUILayout.Width(120f));
                DOFfocall.Value = GUILayout.HorizontalSlider(DOFfocall.Value, 1f, 300f);
                GUILayout.Label(DOFfocall.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    DOFfocall.Value = (float)DOFfocall.DefaultValue;
                }
                GUILayout.EndHorizontal();
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
                GUILayout.BeginHorizontal();
                GUILayout.Label("ShutterAngle  ", GUILayout.Width(120f));
                MBshutter.Value = GUILayout.HorizontalSlider(MBshutter.Value, 0f, 360f);
                GUILayout.Label(MBshutter.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    MBshutter.Value = (float)MBshutter.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("SampleCount", GUILayout.Width(120f));
                MBsamplecnt.Value = (int)GUILayout.HorizontalSlider(MBsamplecnt.Value, 4f, 32f);
                GUILayout.Label(MBsamplecnt.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    MBsamplecnt.Value = (int)MBsamplecnt.DefaultValue;
                }
                GUILayout.EndHorizontal();
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
                GUILayout.BeginHorizontal();
                GUILayout.Label("Center  ", GUILayout.Width(120f));
                GUILayout.Label("X: ");
                float x = GUILayout.HorizontalSlider(VGcenter.Value.x, 0f, 1f);
                GUILayout.Label(x.ToString("F"), GUILayout.Width(40f));
                GUILayout.Label("", GUILayout.Width(60f));
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("", GUILayout.Width(120f));
                GUILayout.Label("Y: ");
                float y = GUILayout.HorizontalSlider(VGcenter.Value.y, 0f, 1f);
                GUILayout.Label(y.ToString("F"), GUILayout.Width(40f));
                VGcenter.Value = new Vector2(x, y);
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    VGcenter.Value = (Vector2)VGcenter.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Intensity", GUILayout.Width(120f));
                VGintensity.Value = GUILayout.HorizontalSlider(VGintensity.Value, 0f, 1f);
                GUILayout.Label(VGintensity.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    VGintensity.Value = (float)VGintensity.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("Smoothness", GUILayout.Width(120f));
                VGsmoothness.Value = GUILayout.HorizontalSlider(VGsmoothness.Value, 0f, 1f);
                GUILayout.Label(VGsmoothness.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    VGsmoothness.Value = (float)VGsmoothness.DefaultValue;
                }
                GUILayout.EndHorizontal();
                VGrounded.Value = GUILayout.Toggle(VGrounded.Value, "Rounded");
                GUILayout.BeginHorizontal();
                GUILayout.Label("Roundness", GUILayout.Width(120f));
                VGroundness.Value = GUILayout.HorizontalSlider(VGroundness.Value, 0f, 1f);
                GUILayout.Label(VGroundness.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    VGroundness.Value = (float)VGroundness.DefaultValue;
                }
                GUILayout.EndHorizontal();
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
                GUILayout.BeginHorizontal();
                GUILayout.Label("OutlineWidth", GUILayout.Width(120f));
                OutlineWidth.Value = GUILayout.HorizontalSlider(OutlineWidth.Value, 0f, 1f);
                GUILayout.Label(OutlineWidth.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    OutlineWidth.Value = (float)OutlineWidth.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ColorPower", GUILayout.Width(120f));
                ColorPower.Value = GUILayout.HorizontalSlider(ColorPower.Value, 0f, 10f);
                GUILayout.Label(ColorPower.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    ColorPower.Value = (float)ColorPower.DefaultValue;
                }
                GUILayout.EndHorizontal();
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
                GUILayout.BeginHorizontal();
                GUILayout.Label("DivisionNum", GUILayout.Width(120f));
                PosDiv.Value = (int)GUILayout.HorizontalSlider(PosDiv.Value, 1f, 64f);
                GUILayout.Label(PosDiv.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    PosDiv.Value = (int)PosDiv.DefaultValue;
                }
                GUILayout.EndHorizontal();
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
                GUILayout.BeginHorizontal();
                GUILayout.Label("SampleDistance", GUILayout.Width(120f));
                SengaSampleDistance.Value = GUILayout.HorizontalSlider(SengaSampleDistance.Value, 0f, 3f);
                GUILayout.Label(SengaSampleDistance.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaSampleDistance.Value = (float)SengaSampleDistance.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("NormalThreshold", GUILayout.Width(120f));
                SengaNomalThes.Value = GUILayout.HorizontalSlider(SengaNomalThes.Value, 0.01f, 1f);
                GUILayout.Label(SengaNomalThes.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaNomalThes.Value = (float)SengaNomalThes.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("DepthThreshold", GUILayout.Width(120f));
                SengaDepthThres.Value = GUILayout.HorizontalSlider(SengaDepthThres.Value, 0.01f, 10f);
                GUILayout.Label(SengaDepthThres.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaDepthThres.Value = (float)SengaDepthThres.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ColorThreshold", GUILayout.Width(120f));
                SengaColorThres.Value = GUILayout.HorizontalSlider(SengaColorThres.Value, 0.001f, 1f);
                GUILayout.Label(SengaColorThres.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaColorThres.Value = (float)SengaColorThres.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("SobelThreshold", GUILayout.Width(120f));
                SengaSobelThres.Value = GUILayout.HorizontalSlider(SengaSobelThres.Value, 0.01f, 10f);
                GUILayout.Label(SengaSobelThres.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaSobelThres.Value = (float)SengaSobelThres.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("NormalEdge", GUILayout.Width(120f));
                SengaNormalEdge.Value = GUILayout.HorizontalSlider(SengaNormalEdge.Value, 0f, 1f);
                GUILayout.Label(SengaNormalEdge.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaNormalEdge.Value = (float)SengaNormalEdge.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("DepthEdge", GUILayout.Width(120f));
                SengaDepthEdge.Value = GUILayout.HorizontalSlider(SengaDepthEdge.Value, 0f, 1f);
                GUILayout.Label(SengaDepthEdge.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaDepthEdge.Value = (float)SengaDepthEdge.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ColorEdge", GUILayout.Width(120f));
                SengaColorEdge.Value = GUILayout.HorizontalSlider(SengaColorEdge.Value, 0f, 1f);
                GUILayout.Label(SengaColorEdge.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaColorEdge.Value = (float)SengaColorEdge.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("SobelEdge", GUILayout.Width(120f));
                SengaSobelEdge.Value = GUILayout.HorizontalSlider(SengaSobelEdge.Value, 0f, 1f);
                GUILayout.Label(SengaSobelEdge.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaSobelEdge.Value = (float)SengaSobelEdge.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ColorBlend", GUILayout.Width(120f));
                SengaColBlend.Value = GUILayout.HorizontalSlider(SengaColBlend.Value, 0f, 20f);
                GUILayout.Label(SengaColBlend.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaColBlend.Value = (float)SengaColBlend.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("Tone", GUILayout.Width(120f));
                SengaToneEnable.Value = GUILayout.Toggle(SengaToneEnable.Value, "PasteTone");
                GUILayout.BeginHorizontal();
                GUILayout.Label("ToneScale", GUILayout.Width(120f));
                SengaToneScale.Value = GUILayout.HorizontalSlider(SengaToneScale.Value, 0.01f, 10f);
                GUILayout.Label(SengaToneScale.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaToneScale.Value = (float)SengaToneScale.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ToneThickness", GUILayout.Width(120f));
                SengaToneThick.Value = GUILayout.HorizontalSlider(SengaToneThick.Value, -1f, 1f);
                GUILayout.Label(SengaToneThick.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaToneThick.Value = (float)SengaToneThick.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("ToneThreshold", GUILayout.Width(120f));
                SengaToneThres.Value = GUILayout.HorizontalSlider(SengaToneThres.Value, -1f, 1f);
                GUILayout.Label(SengaToneThres.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaToneThres.Value = (float)SengaToneThres.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.Label("Blur", GUILayout.Width(120f));
                SengaBlurEnable.Value = GUILayout.Toggle(SengaBlurEnable.Value, "BlurEnable");
                GUILayout.BeginHorizontal();
                GUILayout.Label("BlurDirection", GUILayout.Width(120f));
                SengaBlurDir.Value = GUILayout.HorizontalSlider(SengaBlurDir.Value, 0f, 10f);
                GUILayout.Label(SengaBlurDir.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaBlurDir.Value = (float)SengaBlurDir.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("BlurPower", GUILayout.Width(120f));
                SengaBlurPow.Value = GUILayout.HorizontalSlider(SengaBlurPow.Value, 0f, 5f);
                GUILayout.Label(SengaBlurPow.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaBlurPow.Value = (float)SengaBlurPow.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("BlurThickness", GUILayout.Width(120f));
                SengaBlurThick.Value = GUILayout.HorizontalSlider(SengaBlurThick.Value, 0f, 1f);
                GUILayout.Label(SengaBlurThick.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaBlurThick.Value = (float)SengaBlurThick.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Label("BlurSampleCount", GUILayout.Width(120f));
                SengaBlurSample.Value = (int)GUILayout.HorizontalSlider(SengaBlurSample.Value, 2f, 64f);
                GUILayout.Label(SengaBlurSample.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    SengaBlurSample.Value = (int)SengaBlurSample.DefaultValue;
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            #endregion

            #region Distortion
            distortion = GUILayout.Toggle(distortion, "Lens Distortion ", GUI.skin.button);
            if (distortion)
            {
                GUILayout.BeginVertical();
                DistortionEnable.Value = GUILayout.Toggle(DistortionEnable.Value, "Enable");

                //GUILayout.BeginHorizontal();
                //GUILayout.Label("Intensity", GUILayout.Width(120f));
                //DistortionIntensity.Value = (float)GUILayout.HorizontalSlider(DistortionIntensity.Value, -100f, 100f);
                //GUILayout.Label(DistortionIntensity.Value.ToString("F"), GUILayout.Width(40f));
                //if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                //{
                //    DistortionIntensity.Value = (float)DistortionIntensity.DefaultValue;
                //}
                //GUILayout.EndHorizontal();

                DistortionIntensity.Value = DrawSliderTextBoxCombo("Intensity", -100f, 100f, ref DistortionIntensityBuffer, DistortionIntensity.Value, (float)DistortionIntensity.DefaultValue);

                GUILayout.BeginHorizontal();
                GUILayout.Label("X Multiplier", GUILayout.Width(120f));
                DistortionIntensityX.Value = (float)GUILayout.HorizontalSlider(DistortionIntensityX.Value, 0f, 1f);
                GUILayout.Label(DistortionIntensityX.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    DistortionIntensityX.Value = (float)DistortionIntensityX.DefaultValue;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Y Multiplier", GUILayout.Width(120f));
                DistortionIntensityY.Value = (float)GUILayout.HorizontalSlider(DistortionIntensityY.Value, 0f, 1f);
                GUILayout.Label(DistortionIntensityY.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    DistortionIntensityY.Value = (float)DistortionIntensityY.DefaultValue;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Center X", GUILayout.Width(120f));
                DistortionCenterX.Value = (float)GUILayout.HorizontalSlider(DistortionCenterX.Value, -1f, 1);
                GUILayout.Label(DistortionCenterX.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    DistortionCenterX.Value = (float)DistortionCenterX.DefaultValue;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Center Y", GUILayout.Width(120f));
                DistortionCenterY.Value = (float)GUILayout.HorizontalSlider(DistortionCenterY.Value, -1f, 1f);
                GUILayout.Label(DistortionCenterY.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    DistortionCenterY.Value = (float)DistortionCenterY.DefaultValue;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Scale", GUILayout.Width(120f));
                DistortionScale.Value = (float)GUILayout.HorizontalSlider(DistortionScale.Value, 001f, 5f);
                GUILayout.Label(DistortionScale.Value.ToString("F"), GUILayout.Width(40f));
                if (GUILayout.Button("Reset", GUILayout.Width(60f)))
                {
                    DistortionScale.Value = (float)DistortionScale.DefaultValue;
                }
                GUILayout.EndHorizontal();

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
            DistortionIntensityBuffer = DistortionIntensity.Value.ToString();
            DistortionIntensityX = base.Config.Bind("Lens Distortion", "X Multiplier", 0f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1)));
            DistortionIntensityY = base.Config.Bind("Lens Distortion", "Y Mulitplier", 0f, new ConfigDescription("", new AcceptableValueRange<float>(0f, 1f)));
            DistortionCenterX = base.Config.Bind("Lens Distortion", "X Center", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            DistortionCenterY = base.Config.Bind("Lens Distortion", "Y Center", 0f, new ConfigDescription("", new AcceptableValueRange<float>(-1f, 1f)));
            DistortionScale = base.Config.Bind("Lens Distortion", "Scale", 0f, new ConfigDescription("", new AcceptableValueRange<float>(0.01f, 5f)));
        }
        #endregion
    }
}
