using TMPro;
using UnityEngine;

namespace TownOfUs.Modules.CustomHats
{
    // Class to clone the mask that contains the ColorChips in Cosmetics Tabs (Hat Tab for us), and apply to TMPro texts
    public class TMPMaskCloner
    {
        private SpriteRenderer cloneMaskRenderer;

        public void CreateCloneMask(SpriteMask mask)
        {
            // Create clone sprite renderer
            var go = new GameObject("TMPCloneMask");
            cloneMaskRenderer = go.AddComponent<SpriteRenderer>();
            cloneMaskRenderer.sprite = mask.sprite;

            go.transform.SetParent(mask.transform, false);

            // Make invisible + write stencil
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.SetInt("_Stencil", 1);
            mat.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Replace);
            mat.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Always);
            mat.SetInt("_ColorMask", 0);
            cloneMaskRenderer.material = mat;

            // Match sprite mask sorting ORDER
            cloneMaskRenderer.sortingLayerID = mask.frontSortingLayerID;
            cloneMaskRenderer.sortingOrder = mask.frontSortingOrder - 1;
        }

        public void ApplyToTMP(TextMeshPro tmp)
        {
            var mat = new Material(tmp.fontMaterial);
            mat.SetInt("_Stencil", 1);
            mat.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
            tmp.fontMaterial = mat;
        }
    }
}