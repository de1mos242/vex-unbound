using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VexUnbound.Editor
{
    public static class RuntimeMaterialSetup
    {
        private const string MaterialPath = "Assets/Resources/Materials/RuntimeUnlit.mat";

        [MenuItem("Vex Unbound/Create Runtime Material")]
        public static void CreateRuntimeMaterial()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                throw new InvalidOperationException("The URP Unlit shader is unavailable.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(MaterialPath)!);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, MaterialPath);
            }
            else
            {
                material.shader = shader;
            }

            material.SetColor("_BaseColor", Color.white);
            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created runtime material at {MaterialPath}");
        }
    }
}
