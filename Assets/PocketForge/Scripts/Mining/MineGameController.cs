using System;
using PocketForge.Ads;
using PocketForge.Audio;
using PocketForge.Content;
using PocketForge.Iap;
using PocketForge.Localization;
using PocketForge.Save;
using PocketForge.Settings;
using UnityEngine;

namespace PocketForge.Mining
{
    public sealed class MineGameController : MonoBehaviour
    {
        [SerializeField] private Material oreMaterial;
        [SerializeField] private Material oreBillboardMaterial;
        [SerializeField] private Texture2D upgradeIconSheet;
        [SerializeField] private Texture2D quarryBackdrop;
        [SerializeField] private Texture2D uiKitTexture;
        [SerializeField] private Texture2D upgradeButtonTexture;
        [SerializeField] private Texture2D feedbackPanelTexture;
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip uiClickSound;
        [SerializeField] private AudioClip upgradeSuccessSound;
        [SerializeField] private AudioClip rewardSound;
        [SerializeField] private GameObject generatedOrePrefab;
        [SerializeField, Min(0.01f)] private float generatedOreScale = 1.25f;
        [SerializeField, Min(0.1f)] private float orePresentationScale = 1.5f;
        [SerializeField] private Vector3 orePresentationPosition = new(0f, 2.8f, 2.8f);
        [SerializeField] private MiningContentCatalog contentCatalog;

        private MiningGameState gameState;
        private MineHudPresenter hudPresenter;
        private MineAdCoordinator adCoordinator;
        private MineIapCoordinator iapCoordinator;
        private GameAudioController audioController;
        private Transform oreVisual;
        private Renderer[] oreRenderers;
        private Vector3 oreBaseScale = Vector3.one;
        private bool usesGeneratedOreModel;
        private GameObject activeOrePrefab;
        private Material backdropMaterial;
        private Mesh backdropMesh;
        private Transform backdropVisual;
        private float backdropAspect = -1f;

        // Keep CreatePrimitive dependencies in stripped player builds.
        private MeshFilter PrimitiveMeshFilterReference { get; set; }
        private MeshRenderer PrimitiveMeshRendererReference { get; set; }
        private BoxCollider PrimitiveBoxColliderReference { get; set; }
        private SphereCollider PrimitiveSphereColliderReference { get; set; }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            LanguageService.Initialize();
            GameSettingsService.Initialize();
            audioController = GameAudioController.Create(backgroundMusic, uiClickSound, upgradeSuccessSound, rewardSound);
            var catalog = contentCatalog != null ? contentCatalog : MiningContentCatalog.CreateRuntimeDefault();
            var gameService = new MiningGameService(catalog);
            var saveData = SaveService.Load();
            gameState = gameService.CreateInitialState(saveData, UnityEngine.Random.value);
            CreateQuarryBackdrop();
            var view = MineHudView.Create();
            view.SetTheme(upgradeIconSheet, uiKitTexture, upgradeButtonTexture, feedbackPanelTexture);
            hudPresenter = new MineHudPresenter(view, gameService, gameState);
            hudPresenter.StateChanged += UpdateOreVisual;
            hudPresenter.SaveRequested += SaveGame;
            adCoordinator = new MineAdCoordinator(
                view,
                gameService,
                gameState,
                new GoogleMobileAdsService(),
                new InterstitialAdPolicy(5, 180f));
            hudPresenter.OreBroken += adCoordinator.RecordOreBroken;
            adCoordinator.SaveRequested += SaveGame;
            iapCoordinator = new MineIapCoordinator(new UnityIapService(), gameState.Player, SaveEntitlement);
            iapCoordinator.DisplayChanged += view.SetIapState;
            view.BindIap(iapCoordinator.PurchaseRemoveAds, iapCoordinator.RestorePurchases);

            CreateOreVisual();
            hudPresenter.Render();
            adCoordinator.Initialize();
            iapCoordinator.Initialize();
            var offlineReward = gameService.ApplyOfflineReward(gameState, DateTimeOffset.UtcNow.ToUnixTimeSeconds() - saveData.lastSavedUnixSeconds);
            hudPresenter.ShowOfflineReward(offlineReward);
            if (offlineReward > 0)
            {
                SaveGame();
            }
        }

        private void Update()
        {
            hudPresenter.Tick(Time.deltaTime);
            adCoordinator.Tick(Time.unscaledDeltaTime);
            AnimateOreVisual();
            ResizeQuarryBackdrop();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                GameSettingsService.Flush();
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            GameSettingsService.Flush();
            SaveGame();
        }

        private void OnDestroy()
        {
            if (hudPresenter != null && adCoordinator != null)
            {
                hudPresenter.OreBroken -= adCoordinator.RecordOreBroken;
                adCoordinator.SaveRequested -= SaveGame;
                adCoordinator.Dispose();
            }

            if (iapCoordinator != null)
            {
                iapCoordinator.Dispose();
            }

            if (audioController != null)
            {
                Destroy(audioController.gameObject);
            }

            if (backdropMaterial != null)
            {
                Destroy(backdropMaterial);
            }

            if (backdropMesh != null)
            {
                Destroy(backdropMesh);
            }
        }

        private void SaveGame()
        {
            if (gameState != null)
            {
                SaveService.Save(gameState.Player);
            }
        }

        private bool SaveEntitlement()
        {
            return gameState != null && SaveService.Save(gameState.Player);
        }

        private void CreateOreVisual()
        {
            var orePrefab = ResolveOrePrefab();
            usesGeneratedOreModel = orePrefab != null;
            activeOrePrefab = orePrefab;
            var ore = usesGeneratedOreModel
                ? Instantiate(orePrefab)
                : new GameObject("MineOre");
            ore.name = "MineOre";
            ore.transform.position = orePresentationPosition;
            if (usesGeneratedOreModel)
            {
                ore.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
                oreBaseScale = Vector3.one * ResolveOreScale() * orePresentationScale;
                ore.transform.localScale = oreBaseScale;
            }
            else
            {
                CreateOreChunk(ore.transform, new Vector3(0f, 0f, 0.1f), new Vector3(1.45f, 1.18f, 1.22f), new Vector3(8f, 0f, -10f));
                CreateOreChunk(ore.transform, new Vector3(-0.76f, -0.2f, 0.2f), new Vector3(0.7f, 0.62f, 0.66f), new Vector3(15f, 18f, -18f));
                CreateOreChunk(ore.transform, new Vector3(0.72f, -0.26f, 0.18f), new Vector3(0.65f, 0.58f, 0.62f), new Vector3(12f, -24f, 15f));
                CreateOreChunk(ore.transform, new Vector3(0.08f, 0.56f, 0.1f), new Vector3(0.74f, 0.5f, 0.68f), new Vector3(-14f, 8f, 10f));
                CreateOreChunk(ore.transform, new Vector3(-0.35f, 0.28f, -0.82f), new Vector3(0.28f, 0.25f, 0.12f), new Vector3(0f, 24f, 0f));
                CreateOreChunk(ore.transform, new Vector3(0.38f, -0.08f, -0.86f), new Vector3(0.22f, 0.2f, 0.1f), new Vector3(0f, -18f, 0f));

                if (oreBillboardMaterial != null)
                {
                    var billboard = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    billboard.name = "ForgeOreArt";
                    billboard.transform.SetParent(ore.transform, false);
                    billboard.transform.localPosition = new Vector3(0f, 0f, -0.9f);
                    billboard.transform.localScale = Vector3.one * 2.6f;
                    billboard.GetComponent<Renderer>().sharedMaterial = oreBillboardMaterial;
                    Destroy(billboard.GetComponent<Collider>());
                }
            }

            oreVisual = ore.transform;
            oreRenderers = ore.GetComponentsInChildren<Renderer>();
            UpdateOreVisual();
        }

        private void CreateOreChunk(Transform parent, Vector3 position, Vector3 scale, Vector3 rotation)
        {
            var chunk = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            chunk.name = "OreChunk";
            chunk.transform.SetParent(parent, false);
            chunk.transform.localPosition = position;
            chunk.transform.localScale = scale;
            chunk.transform.localEulerAngles = rotation;
            var renderer = chunk.GetComponent<Renderer>();
            if (oreMaterial != null)
            {
                renderer.sharedMaterial = oreMaterial;
            }

            Destroy(chunk.GetComponent<Collider>());
        }

        private void CreateQuarryBackdrop()
        {
            if (quarryBackdrop == null || Camera.main == null)
            {
                return;
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                return;
            }

            var camera = Camera.main;
            const float distance = 14.5f;
            var backdrop = new GameObject("QuarryBackdrop", typeof(MeshFilter), typeof(MeshRenderer));
            backdrop.transform.position = camera.transform.position + camera.transform.forward * distance;
            backdrop.transform.rotation = camera.transform.rotation;
            backdropVisual = backdrop.transform;

            backdropMesh = new Mesh
            {
                name = "QuarryBackdropQuad",
                vertices = new[]
                {
                    new Vector3(-0.5f, -0.5f, 0f),
                    new Vector3(0.5f, -0.5f, 0f),
                    new Vector3(-0.5f, 0.5f, 0f),
                    new Vector3(0.5f, 0.5f, 0f)
                },
                uv = new[]
                {
                    new Vector2(0f, 0f),
                    new Vector2(1f, 0f),
                    new Vector2(0f, 1f),
                    new Vector2(1f, 1f)
                },
                triangles = new[] { 0, 2, 1, 2, 3, 1 }
            };
            backdropMesh.RecalculateBounds();
            backdrop.GetComponent<MeshFilter>().sharedMesh = backdropMesh;

            backdropMaterial = new Material(shader)
            {
                name = "QuarryBackdropRuntime"
            };
            backdropMaterial.SetTexture("_BaseMap", quarryBackdrop);
            backdropMaterial.SetColor("_BaseColor", Color.white);
            backdrop.GetComponent<MeshRenderer>().sharedMaterial = backdropMaterial;
            ResizeQuarryBackdrop(true);
        }

        private void ResizeQuarryBackdrop(bool force = false)
        {
            var camera = Camera.main;
            if (backdropVisual == null || camera == null || (!force && Mathf.Approximately(backdropAspect, camera.aspect)))
            {
                return;
            }

            const float distance = 14.5f;
            var height = 2f * distance * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.04f;
            backdropAspect = camera.aspect;
            backdropVisual.position = camera.transform.position + camera.transform.forward * distance;
            backdropVisual.rotation = camera.transform.rotation;
            backdropVisual.localScale = new Vector3(height * backdropAspect, height, 1f);
        }

        private void UpdateOreVisual()
        {
            if (oreVisual == null || gameState == null)
            {
                return;
            }

            var desiredPrefab = ResolveOrePrefab();
            if (desiredPrefab != activeOrePrefab)
            {
                Destroy(oreVisual.gameObject);
                oreVisual = null;
                CreateOreVisual();
                return;
            }

            if (usesGeneratedOreModel)
            {
                return;
            }

            var baseColor = gameState.Ore.Definition.GetVisualColor(gameState.Ore.IsRare);
            foreach (var renderer in oreRenderers)
            {
                renderer.material.color = baseColor;
            }
        }

        private void AnimateOreVisual()
        {
            if (oreVisual == null)
            {
                return;
            }

            var pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.03f;
            oreVisual.localScale = oreBaseScale * pulse;
            oreVisual.Rotate(0f, 12f * Time.deltaTime, 0f, Space.World);
        }

        private GameObject ResolveOrePrefab()
        {
            return gameState?.Ore?.Definition?.VisualPrefab != null
                ? gameState.Ore.Definition.VisualPrefab
                : generatedOrePrefab;
        }

        private float ResolveOreScale()
        {
            return gameState?.Ore?.Definition?.VisualPrefab != null
                ? gameState.Ore.Definition.VisualScale
                : generatedOreScale;
        }
    }
}
