using PocketForge.Content;
using PocketForge.Localization;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Mining
{
    public sealed class MineGameController : MonoBehaviour
    {
        [SerializeField] private Material oreMaterial;
        [SerializeField] private Material oreBillboardMaterial;
        [SerializeField] private Texture2D upgradeIconSheet;
        [SerializeField] private MiningContentCatalog contentCatalog;

        private MiningGameState gameState;
        private MineHudPresenter hudPresenter;
        private Transform oreVisual;

        // Keep CreatePrimitive dependencies in stripped player builds.
        private MeshFilter PrimitiveMeshFilterReference { get; set; }
        private MeshRenderer PrimitiveMeshRendererReference { get; set; }
        private BoxCollider PrimitiveBoxColliderReference { get; set; }
        private SphereCollider PrimitiveSphereColliderReference { get; set; }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            LanguageService.Initialize();
            var catalog = contentCatalog != null ? contentCatalog : MiningContentCatalog.CreateRuntimeDefault();
            var gameService = new MiningGameService(catalog);
            gameState = gameService.CreateInitialState(SaveService.Load(), Random.value);
            var view = MineHudView.Create();
            view.SetTheme(upgradeIconSheet);
            hudPresenter = new MineHudPresenter(view, gameService, gameState);
            hudPresenter.StateChanged += UpdateOreVisual;
            hudPresenter.SaveRequested += SaveGame;

            CreateOreVisual();
            hudPresenter.Render();
        }

        private void Update()
        {
            hudPresenter.Tick(Time.deltaTime);
            AnimateOreVisual();
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit() => SaveGame();

        private void SaveGame()
        {
            if (gameState != null)
            {
                SaveService.Save(gameState.Player);
            }
        }

        private void CreateOreVisual()
        {
            var ore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ore.name = "MineOre";
            ore.transform.position = new Vector3(0f, 0.55f, 2.8f);
            ore.transform.localScale = Vector3.one * 1.6f;
            if (oreMaterial != null)
            {
                ore.GetComponent<Renderer>().sharedMaterial = oreMaterial;
            }

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

            oreVisual = ore.transform;
            UpdateOreVisual();
        }

        private void UpdateOreVisual()
        {
            if (oreVisual == null || gameState == null)
            {
                return;
            }

            oreVisual.GetComponent<Renderer>().material.color = gameState.Ore.Definition.GetVisualColor(gameState.Ore.IsRare);
        }

        private void AnimateOreVisual()
        {
            if (oreVisual == null)
            {
                return;
            }

            var pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.03f;
            oreVisual.localScale = Vector3.one * 1.6f * pulse;
            oreVisual.Rotate(0f, 12f * Time.deltaTime, 0f, Space.World);
        }
    }
}
