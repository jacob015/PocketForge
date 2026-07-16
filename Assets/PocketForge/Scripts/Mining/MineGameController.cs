using PocketForge.Economy;
using PocketForge.Save;
using UnityEngine;

namespace PocketForge.Mining
{
    public sealed class MineGameController : MonoBehaviour
    {
        private GameSaveData saveData;
        private float oreDurability;
        private float oreHealth;
        private bool isRareOre;
        private Transform oreVisual;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle buttonStyle;

        private void Awake()
        {
            Application.targetFrameRate = 60;
            saveData = SaveService.Load();
            SpawnOre();
            CreateOreVisual();
        }

        private void Update()
        {
            var autoPower = MiningBalance.GetAutoPowerPerSecond(saveData.drillLevel);
            if (autoPower > 0f)
            {
                DamageOre(autoPower * Time.deltaTime);
            }

            if (oreVisual != null)
            {
                var pulse = 1f + Mathf.Sin(Time.time * 2f) * 0.03f;
                oreVisual.localScale = Vector3.one * pulse;
                oreVisual.Rotate(0f, 12f * Time.deltaTime, 0f, Space.World);
            }
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                SaveService.Save(saveData);
            }
        }

        private void OnApplicationQuit() => SaveService.Save(saveData);

        private void OnGUI()
        {
            if (titleStyle == null)
            {
                CreateStyles();
            }

            var scale = Mathf.Max(0.8f, Screen.width / 1080f);
            GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);
            var width = Screen.width / scale;
            var height = Screen.height / scale;

            GUI.Label(new Rect(42, 36, width - 84, 60), "POCKET FORGE", titleStyle);
            GUI.Label(new Rect(42, 102, width - 84, 42), $"Credits  {saveData.credits:N0}     Depth  {saveData.stage}", bodyStyle);

            var progress = Mathf.Clamp01(oreHealth / oreDurability);
            GUI.Box(new Rect(42, height * 0.48f, width - 84, 32), GUIContent.none);
            GUI.Box(new Rect(46, height * 0.48f + 4, (width - 92) * progress, 24), GUIContent.none);
            GUI.Label(new Rect(42, height * 0.42f, width - 84, 42),
                $"{(isRareOre ? "RARE " : string.Empty)}ORE  {Mathf.CeilToInt(oreHealth)} / {Mathf.CeilToInt(oreDurability)}", bodyStyle);

            if (GUI.Button(new Rect(42, height * 0.54f, width - 84, 120), "MINE", buttonStyle))
            {
                DamageOre(MiningBalance.GetTapPower(saveData.pickaxeLevel));
            }

            DrawUpgradeButton(42, height * 0.72f, width - 84, UpgradeType.Pickaxe, saveData.pickaxeLevel,
                $"PICKAXE  Lv.{saveData.pickaxeLevel}  Tap +1");
            DrawUpgradeButton(42, height * 0.80f, width - 84, UpgradeType.Drill, saveData.drillLevel,
                $"DRILL  Lv.{saveData.drillLevel}  Auto +0.5/s");
            DrawUpgradeButton(42, height * 0.88f, width - 84, UpgradeType.Robot, saveData.robotLevel,
                $"ROBOT  Lv.{saveData.robotLevel}  Reward +10%");
        }

        private void DrawUpgradeButton(float x, float y, float width, UpgradeType type, int level, string label)
        {
            var cost = MiningBalance.GetUpgradeCost(type, level);
            if (GUI.Button(new Rect(x, y, width, 58), $"{label}   [{cost:N0} C]", buttonStyle))
            {
                TryUpgrade(type, cost);
            }
        }

        private void TryUpgrade(UpgradeType type, int cost)
        {
            if (saveData.credits < cost)
            {
                return;
            }

            saveData.credits -= cost;
            switch (type)
            {
                case UpgradeType.Pickaxe: saveData.pickaxeLevel++; break;
                case UpgradeType.Drill: saveData.drillLevel++; break;
                case UpgradeType.Robot: saveData.robotLevel++; break;
            }

            SaveService.Save(saveData);
        }

        private void DamageOre(float amount)
        {
            oreHealth -= amount;
            if (oreHealth > 0f)
            {
                return;
            }

            saveData.credits += MiningBalance.GetOreReward(saveData.stage, isRareOre, saveData.robotLevel);
            saveData.stage++;
            SaveService.Save(saveData);
            SpawnOre();
            UpdateOreVisual();
        }

        private void SpawnOre()
        {
            oreDurability = MiningBalance.GetOreDurability(saveData.stage);
            oreHealth = oreDurability;
            isRareOre = Random.value < 0.08f;
        }

        private void CreateOreVisual()
        {
            var ore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ore.name = "MineOre";
            ore.transform.position = new Vector3(0f, 0f, 2.8f);
            ore.transform.localScale = Vector3.one * 1.6f;
            oreVisual = ore.transform;
            UpdateOreVisual();
        }

        private void UpdateOreVisual()
        {
            if (oreVisual == null)
            {
                return;
            }

            var renderer = oreVisual.GetComponent<Renderer>();
            renderer.material.color = isRareOre ? new Color(0.4f, 0.9f, 1f) : new Color(0.85f, 0.4f, 0.12f);
        }

        private void CreateStyles()
        {
            titleStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 38,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            bodyStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold
            };
        }
    }
}
