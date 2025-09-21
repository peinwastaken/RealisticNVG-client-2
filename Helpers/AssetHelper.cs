using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using System.Reflection;
using System;
using BorkelRNVG.Enum;
using BorkelRNVG.Data;

namespace BorkelRNVG.Helpers
{
    public class AssetHelper
    {
        public static readonly string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly string assetsDirectory = $"{directory}\\Assets";

        public static Shader pixelationShader; // Assets/Systems/Effects/Pixelation/Pixelation.shader
        public static Shader nightVisionShader; // Assets/Shaders/CustomNightVision.shader
        public static Shader contrastShader;
        public static Shader additiveBlendShader;
        public static Shader blurShader;
        public static Shader exposureShader;
        public static Shader maskShader;

        public static Texture2D noiseTexture = LoadPNG($"{assetsDirectory}\\MaskTextures\\Noise.png", TextureWrapMode.Repeat);

        public static Dictionary<string, AudioClip> LoadedAudioClips = new Dictionary<string, AudioClip>();

        // better! :)
        public static Dictionary<ENVGTexture, NVGTextureData> NightVisionTextures = new Dictionary<ENVGTexture, NVGTextureData>()
        {
            { ENVGTexture.Anvis, new NVGTextureData($"{assetsDirectory}\\MaskTextures\\mask_anvis.png", $"{assetsDirectory}\\LensTextures\\lens_anvis.png") },
            { ENVGTexture.Binocular, new NVGTextureData($"{assetsDirectory}\\MaskTextures\\mask_binocular.png", $"{assetsDirectory}\\LensTextures\\lens_binocular.png") },
            { ENVGTexture.Monocular, new NVGTextureData($"{assetsDirectory}\\MaskTextures\\mask_old_monocular.png", $"{assetsDirectory}\\LensTextures\\lens_old_monocular.png") },
            { ENVGTexture.Pnv, new NVGTextureData($"{assetsDirectory}\\MaskTextures\\mask_pnv.png", $"{assetsDirectory}\\LensTextures\\lens_pnv.png") },
            { ENVGTexture.Thermal, new NVGTextureData($"{assetsDirectory}\\MaskTextures\\mask_thermal.png", $"{assetsDirectory}\\LensTextures\\lens_pnv.png") },
            { ENVGTexture.Pixel, new NVGTextureData($"{assetsDirectory}\\MaskTextures\\pixel_mask1.png", $"{assetsDirectory}\\LensTextures\\lens_old_monocular.png") }
        };

        public static Texture MaskToLens(Texture maskTex)
        {
            foreach (var data in NightVisionTextures)
            {
                if (data.Value.Mask == maskTex) // sigh..
                {
                    return data.Value.Lens;
                }
            }
            return null;
        }

        public static void LoadShaders()
        {
            string eftShaderPath = Path.Combine(Environment.CurrentDirectory, "EscapeFromTarkov_Data", "StreamingAssets", "Windows", "shaders");
            string nightVisionShaderPath = $"{assetsDirectory}\\Shaders\\borkel_realisticnvg_shaders";

            pixelationShader = LoadShader("Assets/Systems/Effects/Pixelation/Pixelation.shader", eftShaderPath); // T-7 pixelation
            nightVisionShader = LoadShader("Assets/Shaders/CustomNightVision.shader", nightVisionShaderPath);
            contrastShader = LoadShader("assets/shaders/pein/shaders/contrastshader.shader", $"{assetsDirectory}\\Shaders\\pein_shaders");
            additiveBlendShader = LoadShader("assets/shaders/pein/shaders/additiveblendshader.shader", $"{assetsDirectory}\\Shaders\\pein_shaders");
            blurShader = LoadShader("assets/shaders/pein/shaders/blurshader.shader", $"{assetsDirectory}\\Shaders\\pein_shaders");
            exposureShader = LoadShader("assets/shaders/pein/shaders/exposureshader.shader", $"{assetsDirectory}\\Shaders\\pein_shaders");
            maskShader = LoadShader("assets/shaders/pein/shaders/maskshader.shader", $"{assetsDirectory}\\Shaders\\pein_shaders");
        }

        public static Texture2D LoadPNG(string filePath, TextureWrapMode wrapMode)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (File.Exists(filePath))
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
                tex.wrapMode = wrapMode; //otherwise the mask will repeat itself around screen borders
            }
            else
            {
                string relativePath = filePath.Replace(assetsDirectory + "\\", "");
                Plugin.Log.LogError($"BRNVG Mod: Failed to load PNG: {relativePath}");
                return null;
            }

            return tex;
        }

        public static Texture2D LoadPNG(string filePath)
        {
            return LoadPNG(filePath, TextureWrapMode.Clamp);
        }

        public static Shader LoadShader(string shaderName, string bundlePath)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            Shader shader = assetBundle.LoadAsset<Shader>(shaderName);
            assetBundle.Unload(false);
            return shader;
        }

        public static ComputeShader LoadComputeShader(string shaderName, string bundlePath)
        {
            AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
            ComputeShader computeShader = assetBundle.LoadAsset<ComputeShader>(shaderName);
            assetBundle.Unload(false);
            return computeShader;
        }

        public static void LoadAudioClips()
        {
            string[] audioFilesDir = Directory.GetFiles($"{assetsDirectory}\\Sounds");
            LoadedAudioClips.Clear();

            foreach (string fileDir in audioFilesDir)
            {
                LoadAudioClip(fileDir);
            }
        }

        public static async void LoadAudioClip(string path)
        {
            LoadedAudioClips[Path.GetFileName(path)] = await RequestAudioClip(path);
        }

        public static async Task<AudioClip> RequestAudioClip(string path)
        {
            string extension = Path.GetExtension(path);
            AudioType audioType = AudioType.WAV;

            switch (extension)
            {
                case ".wav":
                    audioType = AudioType.WAV;
                    break;
                case ".ogg":
                    audioType = AudioType.OGGVORBIS;
                    break;
            }

            UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(path, audioType);
            UnityWebRequestAsyncOperation sendWeb = uwr.SendWebRequest();

            while (!sendWeb.isDone)
                await Task.Yield();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Plugin.Log.LogWarning("BRNVG Mod: Failed To Fetch Audio Clip");
                return null;
            }
            else
            {
                AudioClip audioclip = DownloadHandlerAudioClip.GetContent(uwr);
                return audioclip;
            }
        }
    }
}
