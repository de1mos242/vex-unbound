using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace VexUnbound.Editor
{
    public static class GothicFortressMaterialSetup
    {
        private const string MaterialRoot = "Assets/Resources/Environment/GothicFortress/Materials";
        private const string TextureRoot = "Assets/Resources/Environment/GothicFortress/Textures";

        [MenuItem("Vex Unbound/Create Gothic Fortress Materials")]
        public static void CreateMaterials()
        {
            Directory.CreateDirectory(MaterialRoot);

            Material sides = GetOrCreateMaterial("WeatheredStoneSides", "Universal Render Pipeline/Lit");
            ConfigureStone(
                sides,
                new Color(0.47f, 0.49f, 0.58f),
                "stone_brick_wall_001_diff_1k.jpg",
                "stone_brick_wall_001_nor_gl_1k.jpg",
                "stone_brick_wall_001_ao_1k.jpg");

            Material tops = GetOrCreateMaterial("WeatheredStoneTops", "Universal Render Pipeline/Lit");
            ConfigureStone(
                tops,
                new Color(0.66f, 0.67f, 0.72f),
                "medieval_blocks_05_diff_1k.jpg",
                "medieval_blocks_05_nor_gl_1k.jpg",
                "medieval_blocks_05_ao_1k.jpg");

            ConfigureSimpleLit(
                GetOrCreateMaterial("DarkFortressStone", "Universal Render Pipeline/Lit"),
                new Color(0.32f, 0.34f, 0.43f),
                0.16f);
            ConfigureSimpleLit(
                GetOrCreateMaterial("BlackenedIron", "Universal Render Pipeline/Lit"),
                new Color(0.1f, 0.11f, 0.14f),
                0.24f);

            Material rain = GetOrCreateMaterial("Rain", "Universal Render Pipeline/Unlit");
            rain.SetColor("_BaseColor", new Color(0.55f, 0.64f, 0.85f, 0.25f));
            rain.SetFloat("_Surface", 1f);
            rain.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            rain.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            rain.SetFloat("_ZWrite", 0f);
            rain.SetOverrideTag("RenderType", "Transparent");
            rain.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            rain.renderQueue = (int)RenderQueue.Transparent;
            EditorUtility.SetDirty(rain);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Created gothic fortress materials in {MaterialRoot}");
        }

        private static Material GetOrCreateMaterial(string name, string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException($"The {shaderName} shader is unavailable.");
            }

            string path = $"{MaterialRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            return material;
        }

        private static void ConfigureStone(Material material, Color color, string diffuseName, string normalName, string occlusionName)
        {
            material.SetColor("_BaseColor", Color.Lerp(color, Color.white, 0.42f));
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0.12f);
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureRoot}/{diffuseName}"));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureRoot}/{normalName}"));
            material.SetTexture("_OcclusionMap", AssetDatabase.LoadAssetAtPath<Texture2D>($"{TextureRoot}/{occlusionName}"));
            material.SetTextureScale("_BaseMap", Vector2.one);
            material.SetTextureScale("_BumpMap", Vector2.one);
            material.SetTextureScale("_OcclusionMap", Vector2.one);
            material.SetFloat("_BumpScale", 0.8f);
            material.SetFloat("_OcclusionStrength", 0.72f);
            material.EnableKeyword("_NORMALMAP");
            material.EnableKeyword("_OCCLUSIONMAP");
            EditorUtility.SetDirty(material);
        }

        private static void ConfigureSimpleLit(Material material, Color color, float smoothness)
        {
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", smoothness);
            material.SetTexture("_BaseMap", null);
            material.SetTexture("_BumpMap", null);
            material.SetTexture("_OcclusionMap", null);
            material.DisableKeyword("_NORMALMAP");
            material.DisableKeyword("_OCCLUSIONMAP");
            EditorUtility.SetDirty(material);
        }
    }
}
