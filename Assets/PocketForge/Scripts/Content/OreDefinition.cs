using UnityEngine;

namespace PocketForge.Content
{
    [CreateAssetMenu(menuName = "Pocket Forge/Content/Ore Definition", fileName = "OreDefinition")]
    public sealed class OreDefinition : ScriptableObject
    {
        [SerializeField] private string contentId = "copper";
        [SerializeField, Min(1)] private int startStage = 1;
        [SerializeField, Min(1f)] private float baseDurability = 10f;
        [SerializeField, Min(0f)] private float durabilityPerStage = 5f;
        [SerializeField, Range(0f, 1f)] private float rareChance = 0.08f;
        [SerializeField, Min(1)] private int normalRewardMultiplier = 2;
        [SerializeField, Min(1)] private int rareRewardMultiplier = 5;
        [SerializeField] private Color normalVisualColor = new(0.85f, 0.4f, 0.12f);
        [SerializeField] private Color rareVisualColor = new(0.4f, 0.9f, 1f);
        [SerializeField] private GameObject visualPrefab;
        [SerializeField, Min(0.01f)] private float visualScale = 1.25f;

        public string ContentId => contentId;
        public int StartStage => startStage;
        public float RareChance => rareChance;
        public int NormalRewardMultiplier => normalRewardMultiplier;
        public int RareRewardMultiplier => rareRewardMultiplier;
        public GameObject VisualPrefab => visualPrefab;
        public float VisualScale => visualScale;

        public float GetDurability(int stage) => baseDurability + Mathf.Max(0, stage - startStage) * durabilityPerStage;

        public Color GetVisualColor(bool isRare) => isRare ? rareVisualColor : normalVisualColor;

        public static OreDefinition CreateRuntimeDefault()
        {
            var definition = CreateInstance<OreDefinition>();
            definition.name = "Runtime Copper Ore";
            return definition;
        }
    }
}
