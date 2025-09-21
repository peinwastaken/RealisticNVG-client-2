using UnityEngine;
using BorkelRNVG.Helpers;

namespace BorkelRNVG.Data
{
    public class NVGTextureData
    {
        public string MaskPath;
        public string LensPath;
        public Texture2D Mask;
        public Texture2D Lens;

        public NVGTextureData(string maskPath, string lensPath)
        {
            if (maskPath != null)
            {
                Mask = AssetHelper.LoadPNG(maskPath, TextureWrapMode.Clamp);
            }

            if (lensPath != null)
            {
                Lens = AssetHelper.LoadPNG(lensPath, TextureWrapMode.Clamp);
            }
        }
    }
}
