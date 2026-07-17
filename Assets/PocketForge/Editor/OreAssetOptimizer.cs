using System;
using System.IO;
using System.Linq;
using PocketForge.Content;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;

namespace PocketForge.EditorTools
{
    public static class OreAssetOptimizer
    {
        private const string OutputRoot = "Assets/PocketForge/Art/Optimized/Ores";
        private const int TargetTriangleCount = 12000;
        private const int TextureSize = 512;

        private readonly struct OreSource
        {
            public OreSource(string id, string sourcePath, string definitionPath)
            {
                Id = id;
                SourcePath = sourcePath;
                DefinitionPath = definitionPath;
            }

            public string Id { get; }
            public string SourcePath { get; }
            public string DefinitionPath { get; }
        }

        private static readonly OreSource[] Sources =
        {
            new("Copper", "Assets/PocketForge/Art/Generated/Models/Imported/PocketForge_CopperOre_Imported.glb", "Assets/PocketForge/Content/Ores/CopperOre.asset"),
            new("Iron", "Assets/PocketForge/Art/Generated/Models/Imported/PocketForge_IronOre_Imported.glb", "Assets/PocketForge/Content/Ores/IronOre.asset"),
            new("Gold", "Assets/PocketForge/Art/Generated/Models/Imported/PocketForge_GoldOre_Imported.glb", "Assets/PocketForge/Content/Ores/GoldOre.asset"),
            new("Crystal", "Assets/PocketForge/Art/Generated/Models/Imported/PocketForge_TurquoiseCrystalOre_v2_Imported.glb", "Assets/PocketForge/Content/Ores/CrystalOre.asset")
        };

        [MenuItem("Pocket Forge/Optimize Ore Assets")]
        public static void OptimizeAll()
        {
            EnsureFolder(OutputRoot);
            foreach (var source in Sources)
            {
                Optimize(source);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Optimized {Sources.Length} ore assets to about {TargetTriangleCount:N0} triangles and {TextureSize}px ASTC textures.");
        }

        private static void Optimize(OreSource source)
        {
            var sourceAssets = AssetDatabase.LoadAllAssetsAtPath(source.SourcePath);
            var sourceMesh = sourceAssets.OfType<Mesh>().FirstOrDefault();
            var sourceTexture = sourceAssets.OfType<Texture2D>().FirstOrDefault();
            if (sourceMesh == null || sourceTexture == null)
            {
                throw new InvalidOperationException($"Missing mesh or texture in {source.SourcePath}.");
            }

            var folder = $"{OutputRoot}/{source.Id}";
            EnsureFolder(folder);
            var meshPath = $"{folder}/{source.Id}Ore_Mobile.asset";
            var texturePath = $"{folder}/{source.Id}Ore_Albedo_Mobile.png";
            var materialPath = $"{folder}/{source.Id}Ore_Mobile.mat";
            var prefabPath = $"{folder}/{source.Id}Ore_Mobile.prefab";

            DeleteGeneratedAsset(meshPath);
            DeleteGeneratedAsset(texturePath);
            DeleteGeneratedAsset(materialPath);
            DeleteGeneratedAsset(prefabPath);

            var triangleCount = Enumerable.Range(0, sourceMesh.subMeshCount)
                .Sum(index => (int)sourceMesh.GetIndexCount(index) / 3);
            var quality = Mathf.Clamp01((float)TargetTriangleCount / Mathf.Max(1, triangleCount));
            var simplifier = new MeshSimplifier
            {
                SimplificationOptions = new SimplificationOptions
                {
                    PreserveBorderEdges = true,
                    PreserveSurfaceCurvature = true,
                    PreserveUVSeamEdges = true,
                    PreserveUVFoldoverEdges = true,
                    EnableSmartLink = true,
                    VertexLinkDistance = double.Epsilon,
                    MaxIterationCount = 100,
                    Agressiveness = 7.0
                }
            };
            simplifier.Initialize(sourceMesh);
            simplifier.SimplifyMesh(quality);
            var mobileMesh = simplifier.ToMesh();
            mobileMesh.name = $"{source.Id}Ore_Mobile";
            mobileMesh.RecalculateBounds();
            MeshUtility.Optimize(mobileMesh);
            MeshUtility.SetMeshCompression(mobileMesh, ModelImporterMeshCompression.Medium);
            AssetDatabase.CreateAsset(mobileMesh, meshPath);

            WriteMobileTexture(sourceTexture, texturePath);
            var mobileTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new InvalidOperationException("URP Lit shader was not found.");
            }

            var material = new Material(shader)
            {
                name = $"{source.Id}Ore_Mobile"
            };
            material.SetTexture("_BaseMap", mobileTexture);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Metallic", 0.12f);
            material.SetFloat("_Smoothness", 0.42f);
            AssetDatabase.CreateAsset(material, materialPath);

            var prefabRoot = new GameObject($"{source.Id}Ore_Mobile");
            prefabRoot.AddComponent<MeshFilter>().sharedMesh = mobileMesh;
            prefabRoot.AddComponent<MeshRenderer>().sharedMaterial = material;
            var prefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
            UnityEngine.Object.DestroyImmediate(prefabRoot);

            var definition = AssetDatabase.LoadAssetAtPath<OreDefinition>(source.DefinitionPath);
            var serializedDefinition = new SerializedObject(definition);
            serializedDefinition.FindProperty("visualPrefab").objectReferenceValue = prefab;
            serializedDefinition.FindProperty("visualScale").floatValue = 1.25f;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();

            var outputTriangles = Enumerable.Range(0, mobileMesh.subMeshCount)
                .Sum(index => (int)mobileMesh.GetIndexCount(index) / 3);
            Debug.Log($"{source.Id}: {triangleCount:N0} -> {outputTriangles:N0} triangles.");
        }

        private static void WriteMobileTexture(Texture source, string assetPath)
        {
            var renderTexture = RenderTexture.GetTemporary(TextureSize, TextureSize, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var previous = RenderTexture.active;
            try
            {
                Graphics.Blit(source, renderTexture);
                RenderTexture.active = renderTexture;
                var readable = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false, false);
                readable.ReadPixels(new Rect(0f, 0f, TextureSize, TextureSize), 0, 0);
                readable.Apply(false, false);
                File.WriteAllBytes(ToAbsolutePath(assetPath), readable.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(readable);
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(renderTexture);
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = false;
            importer.maxTextureSize = TextureSize;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = TextureSize,
                format = TextureImporterFormat.ASTC_6x6,
                textureCompression = TextureImporterCompression.CompressedHQ,
                compressionQuality = 80
            });
            importer.SaveAndReimport();
        }

        private static void EnsureFolder(string path)
        {
            var segments = path.Split('/');
            var current = segments[0];
            for (var index = 1; index < segments.Length; index++)
            {
                var next = $"{current}/{segments[index]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, segments[index]);
                }

                current = next;
            }
        }

        private static void DeleteGeneratedAsset(string path)
        {
            if (AssetDatabase.LoadMainAssetAtPath(path) != null)
            {
                AssetDatabase.DeleteAsset(path);
            }
        }

        private static string ToAbsolutePath(string assetPath)
        {
            return Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, assetPath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
