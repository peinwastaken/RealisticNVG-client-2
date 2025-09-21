using SPT.Reflection.Patching;
using BSG.CameraEffects;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using BorkelRNVG.Helpers;
using BorkelRNVG.Enum;

namespace BorkelRNVG.Patches
{
    internal class NightVisionAwakePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NightVision), "Awake");
        }

        [PatchPrefix]
        private static void PatchPrefix(NightVision __instance, ref Shader ___Shader) //___Shader is the same as __instance.Shader
        {
            //replaces the masks in the class NightVision and applies visual changes
            //Plugin.UltimateBloomInstance = __instance.GetComponent<UltimateBloom>(); //to disable it when NVG turns ON
            //Plugin.BloomAndFlaresInstance = __instance.GetComponent<BloomAndFlares>(); //to disable it when NVG turns ON

            __instance.AnvisMaskTexture = AssetHelper.NightVisionTextures[ENVGTexture.Anvis].Mask;
            __instance.BinocularMaskTexture = AssetHelper.NightVisionTextures[ENVGTexture.Binocular].Mask;
            __instance.OldMonocularMaskTexture = AssetHelper.NightVisionTextures[ENVGTexture.Monocular].Mask;
            __instance.ThermalMaskTexture = AssetHelper.NightVisionTextures[ENVGTexture.Thermal].Mask;
            __instance.Noise = AssetHelper.noiseTexture;

            // :^)
            if (__instance.Color.g > 0.9f)
            {
                ___Shader = AssetHelper.nightVisionShader;
            }
        }
    }
}
