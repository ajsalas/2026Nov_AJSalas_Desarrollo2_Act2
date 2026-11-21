#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using DungeonSurvivor.Core;
using DungeonSurvivor.Player;
using DungeonSurvivor.Enemy;
using DungeonSurvivor.Interactions;
using DungeonSurvivor.UI;
using DungeonSurvivor.Audio;

namespace DungeonSurvivor.EditorTools
{

    public static class DungeonSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/DungeonCrypt_Main.unity";
        private const string MatPath = "Assets/_Project/Materials/";
        private const string TexPath = "Assets/_Project/Textures/";
        private const string KayKitTexPath = "Assets/_Project/Textures/KayKit/";
        private const string KayKitCharPath = "Assets/_Project/Models/Characters/";
        private const string KayKitPropPath = "Assets/_Project/Models/Props/";
        private const string KayKitDungeonPath = "Assets/_Project/Models/Dungeon/";

        [MenuItem("Dungeon Survivor/Build Dungeon Level Scene")]
        public static void BuildDungeonLevel()
        {

            RenderPipelineAsset urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/_Project/Settings/PC_RPAsset.asset");
            if (urpAsset != null)
            {
                GraphicsSettings.defaultRenderPipeline = urpAsset;
                QualitySettings.renderPipeline = urpAsset;
            }

            EnsureDirectory(MatPath);
            EnsureDirectory(TexPath);
            EnsureDirectory(KayKitTexPath);
            AssetDatabase.Refresh();

            Texture2D texFloor = GenerateCobblestoneTexture();
            Texture2D texWall = GenerateStoneBrickTexture();
            Texture2D texIron = GenerateRustedIronTexture();
            Texture2D texWood = GenerateWoodPlankTexture();
            Texture2D texBoss = GenerateObsidianRuneTexture();
            Texture2D texPortal = GenerateArcaneRuneTexture();

            Texture2D texKKDungeon = TryLoadExistingTexture($"{KayKitTexPath}dungeon_texture.png") ?? texWall;
            Texture2D texKKKnight = TryLoadExistingTexture($"{KayKitTexPath}knight_texture.png") ?? texIron;
            Texture2D texKKSkeleton = TryLoadExistingTexture($"{KayKitTexPath}skeleton_texture.png") ?? GenerateBoneTexture();
            Texture2D texAcid = GenerateAcidLiquidTexture();

            Material floorMat = CreatePBRMaterial("M_Dungeon_Floor", texFloor, new Color(0.85f, 0.85f, 0.88f), 0.1f, 0.3f, new Vector2(8, 8));
            Material wallMat = CreatePBRMaterial("M_Dungeon_Wall", texWall, new Color(0.9f, 0.88f, 0.85f), 0.1f, 0.25f, new Vector2(4, 2));
            Material pillarMat = CreatePBRMaterial("M_Dungeon_Pillar", texWall, new Color(0.75f, 0.73f, 0.70f), 0.15f, 0.35f, new Vector2(1, 3));
            Material gateMat = CreatePBRMaterial("M_Iron_Gate", texIron, new Color(0.9f, 0.9f, 0.95f), 0.8f, 0.65f, new Vector2(2, 2));
            Material woodMat = CreatePBRMaterial("M_Chest_Wood", texWood, new Color(0.85f, 0.75f, 0.6f), 0.2f, 0.4f, new Vector2(1, 1));
            Material portalMat = CreatePBRMaterial("M_Portal_Arcane", texPortal, Color.white, 0.1f, 0.8f, new Vector2(1, 1), true, new Color(0.2f, 0.85f, 1.0f));
            Material acidMat = CreatePBRMaterial("M_Dungeon_Acid", texAcid, new Color(0.2f, 0.98f, 0.35f, 0.92f), 0.2f, 0.95f, new Vector2(2, 2), true, new Color(0.18f, 1.0f, 0.32f) * 2.4f);
            Material goldTrimMat = CreatePBRMaterial("M_Gold_Trim", null, new Color(0.85f, 0.65f, 0.18f), 0.85f, 0.75f, Vector2.one);
            Material glowingEyesMat = CreatePBRMaterial("M_Glowing_Eyes", null, new Color(1.0f, 0.4f, 0.05f), 0f, 0.5f, Vector2.one, true, new Color(1.0f, 0.45f, 0.1f) * 2f);

            Material kkDungeonMat = CreatePBRMaterial("M_KayKit_Dungeon", texKKDungeon, Color.white, 0.1f, 0.25f, Vector2.one);
            Material kkKnightMat = CreatePBRMaterial("M_KayKit_Knight", texKKKnight, Color.white, 0.35f, 0.55f, Vector2.one);
            Material kkSkeletonMat = CreatePBRMaterial("M_KayKit_Skeleton", texKKSkeleton, Color.white, 0.1f, 0.3f, Vector2.one);
            Material kkBossMat = CreatePBRMaterial("M_KayKit_Boss", texKKSkeleton, new Color(1.0f, 0.35f, 0.35f), 0.6f, 0.45f, Vector2.one, true, new Color(0.9f, 0.15f, 0.1f) * 1.5f);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject envRoot = new GameObject("Environment");

            BuildRoom(envRoot.transform, "Antechamber", new Vector3(0, 0, 0), new Vector3(16, 5, 16), floorMat, wallMat, true, false);

            BuildCorridor(envRoot.transform, "Corridor_North", new Vector3(0, 0, 22), new Vector3(6, 5, 28), floorMat, wallMat, acidMat);

            BuildConnectingHallway(envRoot.transform, "Hallway_East", new Vector3(6.5f, 0, 20), new Vector3(7f, 5f, 6f), floorMat, wallMat);

            BuildRoom(envRoot.transform, "SideCrypt", new Vector3(18, 0, 20), new Vector3(16, 5, 16), floorMat, wallMat, false, true);

            CreateHorseshoeAcidMoat(envRoot.transform, new Vector3(23.2f, 0f, 20f), acidMat, wallMat);

            BuildRoom(envRoot.transform, "GuardianSanctum", new Vector3(0, 0, 52), new Vector3(28, 7, 32), floorMat, wallMat, false, false, true);
            BuildPillars(envRoot.transform, new Vector3(0, 0, 52), pillarMat, kkDungeonMat);

            CreateAcidPit(envRoot.transform, "Sanctum_AcidPool_W", new Vector3(-9.5f, 0f, 52f), new Vector3(5.5f, 1.2f, 7.5f), acidMat, wallMat);
            CreateAcidPit(envRoot.transform, "Sanctum_AcidPool_E", new Vector3(9.5f, 0f, 52f), new Vector3(5.5f, 1.2f, 7.5f), acidMat, wallMat);

            BuildSafetyPerimeter(envRoot.transform);

            PlaceDecorProps(envRoot.transform, kkDungeonMat);

            GameObject lightsRoot = new GameObject("Lighting");
            CreateDirectionalMoonlight(lightsRoot.transform);

            CreateTorch(lightsRoot.transform, new Vector3(-7.85f, 2.2f, -3f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, 90, 0));
            CreateTorch(lightsRoot.transform, new Vector3(7.85f, 2.2f, -3f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, -90, 0));
            CreateTorch(lightsRoot.transform, new Vector3(-7.85f, 2.2f, 5f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, 90, 0));
            CreateTorch(lightsRoot.transform, new Vector3(7.85f, 2.2f, 5f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, -90, 0));

            CreateTorch(lightsRoot.transform, new Vector3(-2.85f, 2.2f, 20f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, 90, 0));
            CreateTorch(lightsRoot.transform, new Vector3(2.85f, 2.2f, 11.5f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, -90, 0));

            CreateTorch(lightsRoot.transform, new Vector3(18f, 2.4f, 27.85f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, 180, 0));
            CreateTorch(lightsRoot.transform, new Vector3(25.85f, 2.4f, 20f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, -90, 0));

            CreateTorch(lightsRoot.transform, new Vector3(-13.85f, 3.0f, 44f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, 90, 0));
            CreateTorch(lightsRoot.transform, new Vector3(13.85f, 3.0f, 44f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, -90, 0));
            CreateTorch(lightsRoot.transform, new Vector3(-13.85f, 3.0f, 60f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, 90, 0));
            CreateTorch(lightsRoot.transform, new Vector3(13.85f, 3.0f, 60f), kkDungeonMat, woodMat, goldTrimMat, Quaternion.Euler(0, -90, 0));

            CreatePostProcessingVolume(lightsRoot.transform);

            GameObject propsRoot = new GameObject("Props");

            CreateDoor(propsRoot.transform, new Vector3(0, 0f, 32f), gateMat, kkDungeonMat, KeyType.BronzeKey);

            CreateChest(propsRoot.transform, new Vector3(23.2f, 0.38f, 20f), kkDungeonMat, woodMat, goldTrimMat, KeyType.BronzeKey, 2);

            CreatePortal(propsRoot.transform, new Vector3(0f, 0.2f, 64f), portalMat, pillarMat);

            GameObject killZoneObj = new GameObject("KillZone");
            killZoneObj.transform.SetParent(propsRoot.transform);
            killZoneObj.transform.position = new Vector3(0, -15, 30);
            var kzCol = killZoneObj.AddComponent<BoxCollider>();
            kzCol.size = new Vector3(250, 5, 250);
            kzCol.isTrigger = true;
            killZoneObj.AddComponent<KillZone>();

            GameObject charRoot = new GameObject("Characters");

            GameObject player = CreatePlayerKnight(charRoot.transform, new Vector3(0, 0f, -4f), kkKnightMat, gateMat, goldTrimMat);

            CreateOrbitCamera(player.transform);

            CreateSkeletonEnemy(charRoot.transform, "Skeleton_Minion_Antechamber_L", new Vector3(-3.5f, 0f, 3f), kkSkeletonMat, gateMat, glowingEyesMat, 40, false, KeyType.None,
                new Vector3[] { new Vector3(-4.5f, 0f, 3f), new Vector3(-2.0f, 0f, 3f) });
            CreateSkeletonEnemy(charRoot.transform, "Skeleton_Minion_Antechamber_R", new Vector3(3.5f, 0f, 3f), kkSkeletonMat, gateMat, glowingEyesMat, 40, false, KeyType.None,
                new Vector3[] { new Vector3(4.5f, 0f, 3f), new Vector3(2.0f, 0f, 3f) });

            CreateSkeletonEnemy(charRoot.transform, "Skeleton_Minion_SideCrypt_1", new Vector3(15.0f, 0f, 17.5f), kkSkeletonMat, gateMat, glowingEyesMat, 40, false, KeyType.None,
                new Vector3[] { new Vector3(13.0f, 0f, 17.5f), new Vector3(18.5f, 0f, 17.5f) });
            CreateSkeletonEnemy(charRoot.transform, "Skeleton_Minion_SideCrypt_2", new Vector3(15.0f, 0f, 22.5f), kkSkeletonMat, gateMat, glowingEyesMat, 40, false, KeyType.None,
                new Vector3[] { new Vector3(13.0f, 0f, 22.5f), new Vector3(18.5f, 0f, 22.5f) });

            CreateBossGuardian(charRoot.transform, "Crypt_Guardian_Boss", new Vector3(0f, 0f, 54f), kkBossMat, gateMat, 150, KeyType.CryptMasterKey,
                new Vector3[] { new Vector3(-2f, 0f, 54f), new Vector3(2f, 0f, 54f) });

            CreateSkeletonEnemy(charRoot.transform, "Skeleton_Boss_Add_L", new Vector3(-5.0f, 0f, 48f), kkSkeletonMat, gateMat, glowingEyesMat, 35, false, KeyType.None,
                new Vector3[] { new Vector3(-6.0f, 0f, 48f), new Vector3(-3.5f, 0f, 48f) });
            CreateSkeletonEnemy(charRoot.transform, "Skeleton_Boss_Add_R", new Vector3(5.0f, 0f, 48f), kkSkeletonMat, gateMat, glowingEyesMat, 35, false, KeyType.None,
                new Vector3[] { new Vector3(6.0f, 0f, 48f), new Vector3(3.5f, 0f, 48f) });

            GameObject managersRoot = new GameObject("Managers");
            managersRoot.AddComponent<GameManager>();
            managersRoot.AddComponent<UIManager>();
            managersRoot.AddComponent<AudioManager>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"<color=green><b>[DungeonSceneBuilder] Successfully generated and saved level scene:</b> {ScenePath}</color>");

            EnsureSceneInBuildSettings(ScenePath);
        }

        private static GameObject InstantiateFBX(string assetPath, Transform parent, Vector3 localPos, Quaternion localRot, Vector3 localScale, Material materialOverride = null)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                GameObject instance = Object.Instantiate(prefab, parent);
                instance.name = Path.GetFileNameWithoutExtension(assetPath);
                instance.transform.localPosition = localPos;
                instance.transform.localRotation = localRot;
                instance.transform.localScale = localScale;

                if (materialOverride != null)
                {
                    Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in renderers)
                    {
                        r.sharedMaterial = materialOverride;
                    }
                }
                return instance;
            }
            return null;
        }

        private static Texture2D TryLoadExistingTexture(string path)
        {
            if (File.Exists(path))
            {
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null) return tex;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return null;
        }

        private static Texture2D GenerateCobblestoneTexture()
        {
            string path = $"{TexPath}T_Cobblestone_Floor.png";
            var existing = TryLoadExistingTexture(path);
            if (existing != null) return existing;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color baseCol = new Color(0.24f, 0.24f, 0.26f);
            Color mortarCol = new Color(0.12f, 0.12f, 0.13f);

            int tileSize = 32;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int row = y / tileSize;
                    int offsetX = (row % 2 == 0) ? 0 : tileSize / 2;
                    int localX = (x + offsetX) % tileSize;
                    int localY = y % tileSize;

                    bool isBorder = localX <= 2 || localX >= tileSize - 2 || localY <= 2 || localY >= tileSize - 2;
                    float noise = Mathf.PerlinNoise(x * 0.08f, y * 0.08f) * 0.18f;
                    float fineNoise = Mathf.PerlinNoise(x * 0.35f, y * 0.35f) * 0.08f;

                    Color pixel = isBorder ? mortarCol : (baseCol + new Color(noise + fineNoise, noise + fineNoise, noise + fineNoise));
                    tex.SetPixel(x, y, pixel);
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Texture2D GenerateStoneBrickTexture()
        {
            string path = $"{TexPath}T_Dungeon_Wall.png";
            var existing = TryLoadExistingTexture(path);
            if (existing != null) return existing;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color brickCol = new Color(0.38f, 0.35f, 0.33f);
            Color mortarCol = new Color(0.15f, 0.14f, 0.13f);

            int brickW = 64;
            int brickH = 32;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int row = y / brickH;
                    int offsetX = (row % 2 == 0) ? 0 : brickW / 2;
                    int localY = y % brickH;

                    for (int x2 = 0; x2 < size; x2++)
                    {
                        int localX = (x2 + offsetX) % brickW;
                        bool isMortar = localX <= 3 || localX >= brickW - 3 || localY <= 3 || localY >= brickH - 3;
                        float noise = Mathf.PerlinNoise(x2 * 0.06f, y * 0.06f) * 0.16f;
                        float grit = Mathf.PerlinNoise(x2 * 0.25f, y * 0.25f) * 0.06f;

                        Color pixel = isMortar ? mortarCol : (brickCol + new Color(noise + grit, noise + grit, noise + grit));
                        tex.SetPixel(x2, y, pixel);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Texture2D GenerateRustedIronTexture()
        {
            string path = $"{TexPath}T_Iron_Rusted.png";
            var existing = TryLoadExistingTexture(path);
            if (existing != null) return existing;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color ironCol = new Color(0.20f, 0.21f, 0.23f);
            Color rustCol = new Color(0.48f, 0.22f, 0.08f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float rustMask = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                    float micro = Mathf.PerlinNoise(x * 0.3f, y * 0.3f) * 0.08f;
                    Color col = (rustMask > 0.65f) ? Color.Lerp(ironCol, rustCol, (rustMask - 0.65f) * 3f) : ironCol;
                    tex.SetPixel(x, y, col + new Color(micro, micro, micro));
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Texture2D GenerateWoodPlankTexture()
        {
            string path = $"{TexPath}T_Chest_Wood.png";
            var existing = TryLoadExistingTexture(path);
            if (existing != null) return existing;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color woodCol = new Color(0.42f, 0.26f, 0.12f);
            Color seamCol = new Color(0.18f, 0.10f, 0.04f);

            int plankW = 42;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isSeam = (x % plankW) <= 2;
                    float grain = Mathf.PerlinNoise(x * 0.03f, y * 0.25f) * 0.15f;
                    Color pixel = isSeam ? seamCol : (woodCol + new Color(grain, grain * 0.7f, grain * 0.3f));
                    tex.SetPixel(x, y, pixel);
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Texture2D GenerateBoneTexture()
        {
            string path = $"{TexPath}T_Enemy_Bone.png";
            var existing = TryLoadExistingTexture(path);
            if (existing != null) return existing;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color bone = new Color(0.78f, 0.74f, 0.65f);
            Color crack = new Color(0.28f, 0.25f, 0.20f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float crackNoise = Mathf.PerlinNoise(x * 0.12f, y * 0.12f);
                    bool isCrack = (crackNoise > 0.48f && crackNoise < 0.52f);
                    float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.1f;
                    tex.SetPixel(x, y, isCrack ? crack : (bone + new Color(noise, noise, noise * 0.8f)));
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Texture2D GenerateObsidianRuneTexture()
        {
            string path = $"{TexPath}T_Boss_Obsidian.png";
            var existing = TryLoadExistingTexture(path);
            if (existing != null) return existing;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color obsidian = new Color(0.10f, 0.09f, 0.11f);
            Color rune = new Color(0.95f, 0.12f, 0.10f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float runeNoise = Mathf.PerlinNoise(x * 0.07f, y * 0.07f);
                    bool isRune = (runeNoise > 0.62f && runeNoise < 0.68f) || (x % 64 == 32 && y > 40 && y < 216);
                    tex.SetPixel(x, y, isRune ? rune : obsidian);
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Texture2D GenerateArcaneRuneTexture()
        {
            string path = $"{TexPath}T_Arcane_Portal.png";
            var existing = TryLoadExistingTexture(path);
            if (existing != null) return existing;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Vector2 center = new Vector2(size / 2f, size / 2f);
            Color dark = new Color(0.04f, 0.08f, 0.15f);
            Color glow = new Color(0.18f, 0.85f, 1.0f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float ring = Mathf.Sin(dist * 0.25f);
                    Color pixel = (ring > 0.4f) ? glow : dark;
                    tex.SetPixel(x, y, pixel);
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static Texture2D GenerateAcidLiquidTexture()
        {
            string path = $"{TexPath}T_Acid_Liquid.png";
            var existing = TryLoadExistingTexture(path);
            if (existing != null) return existing;

            int size = 256;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);
            Color deepGreen = new Color(0.06f, 0.42f, 0.12f, 0.95f);
            Color brightLime = new Color(0.35f, 0.98f, 0.28f, 0.95f);
            Color bubbleCore = new Color(0.85f, 1.0f, 0.55f, 0.98f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float n1 = Mathf.PerlinNoise(x * 0.06f, y * 0.06f);
                    float n2 = Mathf.PerlinNoise((x + 64) * 0.15f, (y + 64) * 0.15f);
                    float blend = Mathf.Clamp01(n1 * 0.7f + n2 * 0.3f);

                    Color col = Color.Lerp(deepGreen, brightLime, blend);

                    float bubbleNoise = Mathf.PerlinNoise(x * 0.28f, y * 0.28f);
                    if (bubbleNoise > 0.78f)
                    {
                        col = Color.Lerp(col, bubbleCore, (bubbleNoise - 0.78f) / 0.22f);
                    }

                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        private static void SaveTextureAsPNG(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }

        private static Material CreatePBRMaterial(string name, Texture2D mainTex, Color tint, float metallic, float smoothness, Vector2 tiling, bool emissive = false, Color emissiveColor = default)
        {
            string path = $"{MatPath}{name}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            Shader shader = null;
            if (GraphicsSettings.currentRenderPipeline != null || GraphicsSettings.defaultRenderPipeline != null)
            {
                shader = Shader.Find("Universal Render Pipeline/Lit");
            }
            if (shader == null)
            {
                string shaderPath = AssetDatabase.GUIDToAssetPath("933532a4fcc9baf4fa0491de14d08ed7");
                if (!string.IsNullOrEmpty(shaderPath))
                {
                    shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                }
            }
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }
            if (shader == null)
            {
                shader = Shader.Find("Mobile/Diffuse");
            }
            if (shader == null)
            {
                shader = Shader.Find("Diffuse");
            }

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else
            {
                mat.shader = shader;
            }

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", tint);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", tint);

            if (mainTex != null)
            {
                if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", mainTex);
                if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", mainTex);
                mat.mainTextureScale = tiling;
            }

            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);

            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                Color eCol = (emissiveColor == default) ? tint * 2.0f : emissiveColor;
                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", eCol);
            }

            EditorUtility.SetDirty(mat);
            AssetDatabase.SaveAssets();
            return mat;
        }

        private static void BuildRoom(Transform parent, string name, Vector3 center, Vector3 size, Material floorMat, Material wallMat, bool hasNorthDoor, bool hasWestDoor, bool hasSouthDoor = false)
        {
            GameObject room = new GameObject(name);
            room.transform.SetParent(parent);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = $"{name}_Floor";
            floor.transform.SetParent(room.transform);
            floor.transform.position = new Vector3(center.x, center.y - 0.25f, center.z);
            floor.transform.localScale = new Vector3(size.x, 0.5f, size.z);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            float halfX = size.x / 2f;
            float halfZ = size.z / 2f;
            float h = size.y;

            if (!hasSouthDoor)
            {
                CreateWall(room.transform, $"{name}_Wall_S", new Vector3(center.x, center.y + h / 2f, center.z - halfZ), new Vector3(size.x, h, 0.6f), wallMat);
            }
            else
            {
                float doorWidth = 6f;
                float segWidth = (size.x - doorWidth) / 2f;
                CreateWall(room.transform, $"{name}_Wall_S_L", new Vector3(center.x - (halfX - segWidth / 2f), center.y + h / 2f, center.z - halfZ), new Vector3(segWidth, h, 0.6f), wallMat);
                CreateWall(room.transform, $"{name}_Wall_S_R", new Vector3(center.x + (halfX - segWidth / 2f), center.y + h / 2f, center.z - halfZ), new Vector3(segWidth, h, 0.6f), wallMat);
            }

            if (!hasNorthDoor)
            {
                CreateWall(room.transform, $"{name}_Wall_N", new Vector3(center.x, center.y + h / 2f, center.z + halfZ), new Vector3(size.x, h, 0.6f), wallMat);
            }
            else
            {
                float doorWidth = 6f;
                float segWidth = (size.x - doorWidth) / 2f;
                CreateWall(room.transform, $"{name}_Wall_N_L", new Vector3(center.x - (halfX - segWidth / 2f), center.y + h / 2f, center.z + halfZ), new Vector3(segWidth, h, 0.6f), wallMat);
                CreateWall(room.transform, $"{name}_Wall_N_R", new Vector3(center.x + (halfX - segWidth / 2f), center.y + h / 2f, center.z + halfZ), new Vector3(segWidth, h, 0.6f), wallMat);
            }

            if (!hasWestDoor)
            {
                CreateWall(room.transform, $"{name}_Wall_W", new Vector3(center.x - halfX, center.y + h / 2f, center.z), new Vector3(0.6f, h, size.z), wallMat);
            }
            else
            {
                float doorWidth = 6f;
                float segWidth = (size.z - doorWidth) / 2f;
                CreateWall(room.transform, $"{name}_Wall_W_F", new Vector3(center.x - halfX, center.y + h / 2f, center.z - (halfZ - segWidth / 2f)), new Vector3(0.6f, h, segWidth), wallMat);
                CreateWall(room.transform, $"{name}_Wall_W_B", new Vector3(center.x - halfX, center.y + h / 2f, center.z + (halfZ - segWidth / 2f)), new Vector3(0.6f, h, segWidth), wallMat);
            }

            CreateWall(room.transform, $"{name}_Wall_E", new Vector3(center.x + halfX, center.y + h / 2f, center.z), new Vector3(0.6f, h, size.z), wallMat);
        }

        private static void BuildCorridor(Transform parent, string name, Vector3 center, Vector3 size, Material floorMat, Material wallMat, Material acidMat)
        {
            GameObject corridor = new GameObject(name);
            corridor.transform.SetParent(parent);

            GameObject floorSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorSouth.name = $"{name}_Floor_South";
            floorSouth.transform.SetParent(corridor.transform);
            floorSouth.transform.position = new Vector3(center.x, center.y - 0.25f, 10.2f);
            floorSouth.transform.localScale = new Vector3(size.x, 0.5f, 4.4f);
            floorSouth.GetComponent<Renderer>().sharedMaterial = floorMat;

            CreateAcidPit(corridor.transform, "Corridor_AcidTrench", new Vector3(center.x, 0, 14f), new Vector3(size.x - 0.1f, 1.4f, 3.2f), acidMat, wallMat);

            GameObject floorNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floorNorth.name = $"{name}_Floor_North";
            floorNorth.transform.SetParent(corridor.transform);
            floorNorth.transform.position = new Vector3(center.x, center.y - 0.25f, 25.8f);
            floorNorth.transform.localScale = new Vector3(size.x, 0.5f, 20.4f);
            floorNorth.GetComponent<Renderer>().sharedMaterial = floorMat;

            float halfX = size.x / 2f;
            float h = size.y;

            CreateWall(corridor.transform, $"{name}_Wall_W", new Vector3(center.x - halfX, center.y + h / 2f, center.z), new Vector3(0.6f, h, size.z), wallMat);

            CreateWall(corridor.transform, $"{name}_Wall_E_South", new Vector3(center.x + halfX, center.y + h / 2f, 12.5f), new Vector3(0.6f, h, 9f), wallMat);

            CreateWall(corridor.transform, $"{name}_Wall_E_North", new Vector3(center.x + halfX, center.y + h / 2f, 29.5f), new Vector3(0.6f, h, 13f), wallMat);
        }

        private static void BuildConnectingHallway(Transform parent, string name, Vector3 center, Vector3 size, Material floorMat, Material wallMat)
        {
            GameObject hallway = new GameObject(name);
            hallway.transform.SetParent(parent);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = $"{name}_Floor";
            floor.transform.SetParent(hallway.transform);
            floor.transform.position = new Vector3(center.x, center.y - 0.25f, center.z);
            floor.transform.localScale = new Vector3(size.x, 0.5f, size.z);
            floor.GetComponent<Renderer>().sharedMaterial = floorMat;

            float halfZ = size.z / 2f;
            float h = size.y;

            CreateWall(hallway.transform, $"{name}_Wall_N", new Vector3(center.x, center.y + h / 2f, center.z + halfZ), new Vector3(size.x, h, 0.6f), wallMat);

            CreateWall(hallway.transform, $"{name}_Wall_S", new Vector3(center.x, center.y + h / 2f, center.z - halfZ), new Vector3(size.x, h, 0.6f), wallMat);
        }

        private static void BuildPillars(Transform parent, Vector3 center, Material pillarMat, Material kaykitMat)
        {
            float[] xOffsets = new float[] { -7f, 7f };
            float[] zOffsets = new float[] { -7f, 7f };

            foreach (var ox in xOffsets)
            {
                foreach (var oz in zOffsets)
                {
                    Vector3 pos = new Vector3(center.x + ox, 0f, center.z + oz);
                    GameObject model = InstantiateFBX($"{KayKitDungeonPath}pillar.fbx", parent, pos, Quaternion.identity, new Vector3(1.4f, 1.8f, 1.4f), kaykitMat);
                    if (model == null)
                    {
                        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        pillar.name = "Gothic_Stone_Pillar";
                        pillar.transform.SetParent(parent);
                        pillar.transform.position = new Vector3(center.x + ox, 3.5f, center.z + oz);
                        pillar.transform.localScale = new Vector3(1.8f, 7.0f, 1.8f);
                        pillar.GetComponent<Renderer>().sharedMaterial = pillarMat;
                    }
                    else
                    {
                        var col = model.AddComponent<BoxCollider>();
                        col.size = new Vector3(1.5f, 7.0f, 1.5f);
                        col.center = new Vector3(0, 3.5f, 0);
                    }
                }
            }
        }

        private static void PlaceDecorProps(Transform parent, Material kaykitMat)
        {

            InstantiateFBX($"{KayKitPropPath}barrel_large.fbx", parent, new Vector3(-6.2f, 0f, -6.2f), Quaternion.identity, Vector3.one * 1.2f, kaykitMat);
            InstantiateFBX($"{KayKitPropPath}barrel_small.fbx", parent, new Vector3(-5.2f, 0f, -6.5f), Quaternion.identity, Vector3.one * 1.2f, kaykitMat);
            InstantiateFBX($"{KayKitPropPath}barrel_large.fbx", parent, new Vector3(23.5f, 0f, 14f), Quaternion.identity, Vector3.one * 1.1f, kaykitMat);
            InstantiateFBX($"{KayKitPropPath}chair.fbx", parent, new Vector3(14f, 0f, 25f), Quaternion.Euler(0, 45, 0), Vector3.one * 1.2f, kaykitMat);
            InstantiateFBX($"{KayKitPropPath}barrel_small.fbx", parent, new Vector3(-2.2f, 0f, 15f), Quaternion.identity, Vector3.one * 1.1f, kaykitMat);
        }

        private static void BuildSafetyPerimeter(Transform parent)
        {
            GameObject safety = new GameObject("Safety_Perimeter_Colliders");
            safety.transform.SetParent(parent);

            CreateBarrier(safety.transform, "Barrier_Far_West", new Vector3(-15.5f, 5f, 30f), new Vector3(2f, 14f, 85f));
            CreateBarrier(safety.transform, "Barrier_Far_East_Sanctum", new Vector3(15.5f, 5f, 52f), new Vector3(2f, 14f, 40f));
            CreateBarrier(safety.transform, "Barrier_Far_East_SideCrypt", new Vector3(27.5f, 5f, 20f), new Vector3(2f, 14f, 24f));
            CreateBarrier(safety.transform, "Barrier_Far_North", new Vector3(0f, 5f, 69.5f), new Vector3(36f, 14f, 2f));
            CreateBarrier(safety.transform, "Barrier_Far_South", new Vector3(0f, 5f, -9.5f), new Vector3(24f, 14f, 2f));
        }

        private static void CreateBarrier(Transform parent, string name, Vector3 pos, Vector3 size)
        {
            GameObject b = new GameObject(name);
            b.transform.SetParent(parent);
            b.transform.position = pos;
            var col = b.AddComponent<BoxCollider>();
            col.size = size;
        }

        private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = pos;
            wall.transform.localScale = scale;
            wall.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void CreateDirectionalMoonlight(Transform parent)
        {
            GameObject lightObj = new GameObject("Directional_Moonlight");
            lightObj.transform.SetParent(parent);
            lightObj.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.38f, 0.48f, 0.62f);
            light.intensity = 0.35f;
            light.shadows = LightShadows.Soft;
        }

        private static void CreateTorch(Transform parent, Vector3 position, Material kaykitMat, Material woodMat, Material goldMat, Quaternion rotation = default)
        {
            GameObject torch = new GameObject("Wall_Torch");
            torch.transform.SetParent(parent);
            torch.transform.position = position;
            torch.transform.rotation = rotation;

            GameObject torchMesh = InstantiateFBX($"{KayKitPropPath}torch_mounted.fbx", torch.transform, Vector3.zero, Quaternion.identity, Vector3.one * 1.5f, kaykitMat);
            if (torchMesh == null)
            {
                torchMesh = InstantiateFBX($"{KayKitPropPath}torch.fbx", torch.transform, Vector3.zero, Quaternion.identity, Vector3.one * 1.5f, kaykitMat);
            }

            if (torchMesh == null)
            {

                GameObject bracket = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bracket.name = "Sconce_Bracket";
                bracket.transform.SetParent(torch.transform);
                bracket.transform.localPosition = new Vector3(0, 0, -0.05f);
                bracket.transform.localScale = new Vector3(0.18f, 0.28f, 0.12f);
                bracket.GetComponent<Renderer>().sharedMaterial = goldMat;
                Object.DestroyImmediate(bracket.GetComponent<Collider>());

                GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                stick.name = "Torch_Handle";
                stick.transform.SetParent(torch.transform);
                stick.transform.localPosition = new Vector3(0, 0.15f, 0.12f);
                stick.transform.localScale = new Vector3(0.1f, 0.35f, 0.1f);
                stick.transform.localRotation = Quaternion.Euler(22, 0, 0);
                stick.GetComponent<Renderer>().sharedMaterial = woodMat;
                Object.DestroyImmediate(stick.GetComponent<Collider>());
            }

            GameObject lightObj = new GameObject("TorchLight");
            lightObj.transform.SetParent(torch.transform);
            lightObj.transform.localPosition = new Vector3(0, 0.52f, 0.25f);
            var l = lightObj.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1.0f, 0.62f, 0.22f);
            l.range = 8.5f;
            l.intensity = 2.4f;
            l.shadows = LightShadows.Soft;

            lightObj.AddComponent<TorchFlicker>();
        }

        private static void CreatePostProcessingVolume(Transform parent)
        {
            GameObject volumeObj = new GameObject("Global_PostProcess_Volume");
            volumeObj.transform.SetParent(parent);
            var vol = volumeObj.AddComponent<Volume>();
            vol.isGlobal = true;
            vol.priority = 1.0f;

            string profilePath = "Assets/_Project/Settings/DefaultVolumeProfile.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile != null)
            {
                vol.sharedProfile = profile;
            }
        }

        private static void CreateDoor(Transform parent, Vector3 position, Material gateMat, Material kaykitMat, KeyType reqKey)
        {
            GameObject doorRoot = new GameObject("Reja_Levadiza_Hierro");
            doorRoot.transform.SetParent(parent);
            doorRoot.transform.position = position;

            GameObject doorMesh = InstantiateFBX($"{KayKitPropPath}wall_gated.fbx", doorRoot.transform, Vector3.zero, Quaternion.identity, new Vector3(1.5f, 1.2f, 1.5f), kaykitMat);
            if (doorMesh == null)
            {
                doorMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                doorMesh.name = "Gate_Mover";
                doorMesh.transform.SetParent(doorRoot.transform);
                doorMesh.transform.localPosition = new Vector3(0, 2.1f, 0);
                doorMesh.transform.localScale = new Vector3(5.8f, 4.2f, 0.45f);
                doorMesh.GetComponent<Renderer>().sharedMaterial = gateMat;
            }

            var solidCol = doorMesh.GetComponent<BoxCollider>();
            if (solidCol == null) solidCol = doorMesh.AddComponent<BoxCollider>();
            solidCol.size = new Vector3(5.8f, 4.2f, 0.45f);
            solidCol.center = new Vector3(0, 2.1f, 0);

            var triggerCol = doorRoot.AddComponent<BoxCollider>();
            triggerCol.size = new Vector3(6.5f, 4.5f, 4.5f);
            triggerCol.center = new Vector3(0, 2.1f, 0);
            triggerCol.isTrigger = true;

            var door = doorRoot.AddComponent<DoorInteractable>();
            SetPrivateField(door, "_requiredKey", reqKey);
            SetPrivateField(door, "_doorMover", doorMesh.transform);
            SetPrivateField(door, "_slideHeight", 4.8f);
        }

        private static void CreateChest(Transform parent, Vector3 position, Material kaykitMat, Material woodMat, Material goldMat, KeyType key, int potions)
        {
            GameObject chestRoot = new GameObject("Treasure_Chest");
            chestRoot.transform.SetParent(parent);
            chestRoot.transform.position = position;

            GameObject chestModel = InstantiateFBX($"{KayKitPropPath}chest_gold.fbx", chestRoot.transform, Vector3.zero, Quaternion.identity, Vector3.one * 1.4f, kaykitMat);
            if (chestModel == null)
            {
                chestModel = InstantiateFBX($"{KayKitPropPath}chest.fbx", chestRoot.transform, Vector3.zero, Quaternion.identity, Vector3.one * 1.4f, kaykitMat);
            }

            Transform lidTransform = null;
            if (chestModel != null)
            {

                foreach (Transform child in chestModel.GetComponentsInChildren<Transform>())
                {
                    if (child.name.ToLower().Contains("lid"))
                    {
                        lidTransform = child;
                        break;
                    }
                }
                if (lidTransform == null) lidTransform = chestModel.transform;
            }
            else
            {

                GameObject baseMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                baseMesh.name = "Chest_Base";
                baseMesh.transform.SetParent(chestRoot.transform);
                baseMesh.transform.localPosition = new Vector3(0, 0.35f, 0);
                baseMesh.transform.localScale = new Vector3(1.3f, 0.7f, 0.85f);
                baseMesh.GetComponent<Renderer>().sharedMaterial = woodMat;

                GameObject lidMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
                lidMesh.name = "Chest_Lid";
                lidMesh.transform.SetParent(chestRoot.transform);
                lidMesh.transform.localPosition = new Vector3(0, 0.75f, -0.42f);
                lidMesh.transform.localScale = new Vector3(1.32f, 0.26f, 0.88f);
                lidMesh.GetComponent<Renderer>().sharedMaterial = woodMat;
                lidTransform = lidMesh.transform;
            }

            GameObject solidTarget = chestModel != null ? chestModel : chestRoot;
            var solidCol = solidTarget.AddComponent<BoxCollider>();
            solidCol.size = new Vector3(1.3f, 0.95f, 0.95f);
            solidCol.center = new Vector3(0, 0.48f, 0);
            solidCol.isTrigger = false;

            var col = chestRoot.AddComponent<BoxCollider>();
            col.size = new Vector3(3.2f, 2.0f, 3.2f);
            col.center = new Vector3(0, 0.9f, 0);
            col.isTrigger = true;

            var chest = chestRoot.AddComponent<ChestInteractable>();
            SetPrivateField(chest, "_containedKey", key);
            SetPrivateField(chest, "_containedPotions", potions);
            SetPrivateField(chest, "_chestLid", lidTransform);
        }

        private static void CreateHorseshoeAcidMoat(Transform parent, Vector3 altarCenter, Material acidMat, Material wallMat)
        {
            GameObject moatRoot = new GameObject("SideCrypt_HorseshoeAcidMoat");
            moatRoot.transform.SetParent(parent);
            moatRoot.transform.position = altarCenter;

            GameObject altar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            altar.name = "Altar_Dais";
            altar.transform.SetParent(moatRoot.transform);
            altar.transform.localPosition = new Vector3(0, 0.19f, 0);
            altar.transform.localScale = new Vector3(3.4f, 0.38f, 3.4f);
            altar.GetComponent<Renderer>().sharedMaterial = wallMat;

            CreateMoatCanal(moatRoot.transform, "Moat_Front_W", new Vector3(-2.6f, 0, 0), new Vector3(1.8f, 1.0f, 7.2f), acidMat, wallMat, true);

            CreateMoatCanal(moatRoot.transform, "Moat_North", new Vector3(0.1f, 0, 2.7f), new Vector3(3.6f, 1.0f, 1.8f), acidMat, wallMat, false);

            CreateMoatCanal(moatRoot.transform, "Moat_South", new Vector3(0.1f, 0, -2.7f), new Vector3(3.6f, 1.0f, 1.8f), acidMat, wallMat, false);
        }

        private static GameObject CreateMoatCanal(Transform parent, string name, Vector3 localPos, Vector3 size, Material acidMat, Material wallMat, bool isFrontCanal)
        {
            GameObject canalRoot = new GameObject(name);
            canalRoot.transform.SetParent(parent);
            canalRoot.transform.localPosition = localPos;

            GameObject liquidObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            liquidObj.name = $"{name}_AcidSurface";
            liquidObj.transform.SetParent(canalRoot.transform);
            liquidObj.transform.localPosition = new Vector3(0, 0.06f, 0);
            liquidObj.transform.localScale = new Vector3(size.x, 0.12f, size.z);
            liquidObj.GetComponent<Renderer>().sharedMaterial = acidMat;
            Object.DestroyImmediate(liquidObj.GetComponent<Collider>());
            liquidObj.AddComponent<AcidLiquidAnimator>();

            float curbH = 0.24f;
            float curbW = 0.25f;

            if (isFrontCanal)
            {

                GameObject curbOuter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curbOuter.name = "Curb_Outer";
                curbOuter.transform.SetParent(canalRoot.transform);
                curbOuter.transform.localPosition = new Vector3(-size.x / 2f - curbW / 2f, curbH / 2f, 0);
                curbOuter.transform.localScale = new Vector3(curbW, curbH, size.z + curbW * 2f);
                curbOuter.GetComponent<Renderer>().sharedMaterial = wallMat;

                GameObject curbInner = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curbInner.name = "Curb_Inner";
                curbInner.transform.SetParent(canalRoot.transform);
                curbInner.transform.localPosition = new Vector3(size.x / 2f + curbW / 2f, curbH / 2f, 0);
                curbInner.transform.localScale = new Vector3(curbW, curbH, 3.4f);
                curbInner.GetComponent<Renderer>().sharedMaterial = wallMat;
            }
            else
            {

                float sign = localPos.z > 0 ? 1f : -1f;
                GameObject curbOuter = GameObject.CreatePrimitive(PrimitiveType.Cube);
                curbOuter.name = "Curb_Outer";
                curbOuter.transform.SetParent(canalRoot.transform);
                curbOuter.transform.localPosition = new Vector3(0, curbH / 2f, sign * (size.z / 2f + curbW / 2f));
                curbOuter.transform.localScale = new Vector3(size.x, curbH, curbW);
                curbOuter.GetComponent<Renderer>().sharedMaterial = wallMat;
            }

            GameObject lightObj = new GameObject("AcidLight");
            lightObj.transform.SetParent(canalRoot.transform);
            lightObj.transform.localPosition = new Vector3(0, 0.7f, 0);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.25f, 1.0f, 0.35f);
            light.range = Mathf.Max(size.x, size.z) * 1.3f;
            light.intensity = 2.4f;

            var hazardCol = canalRoot.AddComponent<BoxCollider>();
            hazardCol.isTrigger = true;
            hazardCol.size = new Vector3(size.x, 1.4f, size.z);
            hazardCol.center = new Vector3(0, 0.5f, 0);
            canalRoot.AddComponent<AcidHazardZone>();

            return canalRoot;
        }

        private static GameObject CreateAcidPit(Transform parent, string name, Vector3 center, Vector3 size, Material acidMat, Material wallMat)
        {
            GameObject pitRoot = new GameObject(name);
            pitRoot.transform.SetParent(parent);
            pitRoot.transform.position = center;

            GameObject pitFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pitFloor.name = $"{name}_PitFloor";
            pitFloor.transform.SetParent(pitRoot.transform);
            pitFloor.transform.localPosition = new Vector3(0, -0.65f, 0);
            pitFloor.transform.localScale = new Vector3(size.x, 0.2f, size.z);
            pitFloor.GetComponent<Renderer>().sharedMaterial = wallMat;

            GameObject liquidObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            liquidObj.name = $"{name}_AcidSurface";
            liquidObj.transform.SetParent(pitRoot.transform);
            liquidObj.transform.localPosition = new Vector3(0, 0.06f, 0);
            liquidObj.transform.localScale = new Vector3(size.x - 0.1f, 0.12f, size.z - 0.1f);
            liquidObj.GetComponent<Renderer>().sharedMaterial = acidMat;

            Object.DestroyImmediate(liquidObj.GetComponent<Collider>());

            float curbHeight = 0.22f;
            float curbWidth = 0.35f;

            GameObject curbN = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curbN.name = "Curb_N";
            curbN.transform.SetParent(pitRoot.transform);
            curbN.transform.localPosition = new Vector3(0, curbHeight / 2f, size.z / 2f);
            curbN.transform.localScale = new Vector3(size.x, curbHeight, curbWidth);
            curbN.GetComponent<Renderer>().sharedMaterial = wallMat;

            GameObject curbS = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curbS.name = "Curb_S";
            curbS.transform.SetParent(pitRoot.transform);
            curbS.transform.localPosition = new Vector3(0, curbHeight / 2f, -size.z / 2f);
            curbS.transform.localScale = new Vector3(size.x, curbHeight, curbWidth);
            curbS.GetComponent<Renderer>().sharedMaterial = wallMat;

            GameObject lightObj = new GameObject("AcidLight");
            lightObj.transform.SetParent(pitRoot.transform);
            lightObj.transform.localPosition = new Vector3(0, 0.7f, 0);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.25f, 1.0f, 0.35f);
            light.range = Mathf.Max(size.x, size.z) * 1.5f;
            light.intensity = 2.8f;

            liquidObj.AddComponent<AcidLiquidAnimator>();

            var hazardCol = pitRoot.AddComponent<BoxCollider>();
            hazardCol.isTrigger = true;
            hazardCol.size = new Vector3(size.x, size.y * 1.6f, size.z);
            hazardCol.center = new Vector3(0, -0.4f, 0);
            pitRoot.AddComponent<AcidHazardZone>();

            return pitRoot;
        }

        private static void CreatePortal(Transform parent, Vector3 position, Material portalMat, Material stoneMat)
        {
            GameObject portalRoot = new GameObject("Arcane_Escape_Portal");
            portalRoot.transform.SetParent(parent);
            portalRoot.transform.position = position;

            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pedestal.name = "Portal_Dais";
            pedestal.transform.SetParent(portalRoot.transform);
            pedestal.transform.localPosition = Vector3.zero;
            pedestal.transform.localScale = new Vector3(4.8f, 0.25f, 4.8f);
            pedestal.GetComponent<Renderer>().sharedMaterial = stoneMat;

            GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disc.name = "Arcane_Disc";
            disc.transform.SetParent(portalRoot.transform);
            disc.transform.localPosition = new Vector3(0, 0.15f, 0);
            disc.transform.localScale = new Vector3(3.8f, 0.1f, 3.8f);
            disc.GetComponent<Renderer>().sharedMaterial = portalMat;

            GameObject pLight = new GameObject("PortalLight");
            pLight.transform.SetParent(portalRoot.transform);
            pLight.transform.localPosition = new Vector3(0, 1.5f, 0);
            var light = pLight.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.2f, 0.85f, 1.0f);
            light.range = 9f;
            light.intensity = 3.0f;

            var col = portalRoot.AddComponent<BoxCollider>();
            col.size = new Vector3(4.5f, 3f, 4.5f);
            col.isTrigger = true;

            portalRoot.AddComponent<PortalInteractable>();
        }

        private static GameObject CreatePlayerKnight(Transform parent, Vector3 position, Material armorMat, Material steelMat, Material goldMat)
        {
            GameObject playerObj = new GameObject("Player_Knight");
            playerObj.tag = "Player";
            playerObj.transform.SetParent(parent);
            playerObj.transform.position = position;

            var cc = playerObj.AddComponent<CharacterController>();
            cc.height = 1.85f;
            cc.radius = 0.42f;
            cc.center = new Vector3(0, 0.92f, 0);

            GameObject knightModel = InstantiateFBX($"{KayKitCharPath}Knight.fbx", playerObj.transform, Vector3.zero, Quaternion.identity, Vector3.one, armorMat);

            Transform mainSword = null;
            if (knightModel != null)
            {

                foreach (Transform child in knightModel.GetComponentsInChildren<Transform>(true))
                {
                    string n = child.name;

                    if (n.Contains("Spike_Shield") || n.Contains("Rectangle_Shield") || n.Contains("Badge_Shield"))
                    {
                        child.gameObject.SetActive(false);
                    }

                    if (n.Contains("2H_Sword") || n.Contains("Offhand"))
                    {
                        child.gameObject.SetActive(false);
                    }

                    if (n.Contains("Round_Shield"))
                    {
                        child.gameObject.SetActive(true);
                    }

                    if (n.Contains("1H_Sword") && !n.Contains("Offhand"))
                    {
                        child.gameObject.SetActive(true);
                        mainSword = child;
                    }
                }

                RuntimeAnimatorController knightAnim = GetOrCreateKnightAnimatorController();
                if (knightAnim != null)
                {
                    var anim = knightModel.GetComponent<Animator>();
                    if (anim == null) anim = knightModel.AddComponent<Animator>();
                    anim.runtimeAnimatorController = knightAnim;
                    anim.applyRootMotion = false;
                }
            }

            GameObject weaponObj = mainSword != null ? mainSword.gameObject : new GameObject("Sword_Hitbox");
            if (mainSword == null)
            {
                weaponObj.transform.SetParent(playerObj.transform);
                weaponObj.transform.localPosition = new Vector3(0.48f, 0.75f, 0.55f);
                weaponObj.transform.localRotation = Quaternion.Euler(15, 10, -20);
            }

            var weaponCol = weaponObj.GetComponent<BoxCollider>();
            if (weaponCol == null) weaponCol = weaponObj.AddComponent<BoxCollider>();
            weaponCol.size = new Vector3(0.5f, 0.5f, 1.3f);
            weaponCol.center = new Vector3(0, 0, 0.4f);
            weaponCol.isTrigger = true;
            weaponCol.enabled = false;

            var weaponHitbox = weaponObj.GetComponent<WeaponHitbox>();
            if (weaponHitbox == null) weaponHitbox = weaponObj.AddComponent<WeaponHitbox>();

            playerObj.AddComponent<PlayerHealth>();
            playerObj.AddComponent<PlayerInventory>();
            playerObj.AddComponent<PlayerController>();
            playerObj.AddComponent<ProceduralCharacterAnimator>();

            var combat = playerObj.AddComponent<PlayerCombat>();
            SetPrivateField(combat, "_weaponHitbox", weaponHitbox);

            return playerObj;
        }

        private static void CreateSkeletonEnemy(Transform parent, string name, Vector3 position, Material boneMat, Material steelMat, Material eyeMat, int hp, bool isBoss, KeyType dropKey, Vector3[] waypoints)
        {
            GameObject enemyObj = new GameObject(name);
            enemyObj.transform.SetParent(parent);
            enemyObj.transform.position = position;

            GameObject skelModel = InstantiateFBX($"{KayKitCharPath}Skeleton_Minion.fbx", enemyObj.transform, Vector3.zero, Quaternion.identity, Vector3.one * 1.1f, boneMat);
            if (skelModel != null)
            {
                RuntimeAnimatorController skelAnim = GetOrCreateSkeletonAnimatorController("Minion");
                if (skelAnim != null)
                {
                    var anim = skelModel.GetComponent<Animator>();
                    if (anim == null) anim = skelModel.AddComponent<Animator>();
                    anim.runtimeAnimatorController = skelAnim;
                    anim.applyRootMotion = false;
                }
            }
            else
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Skeleton_Body";
                body.transform.SetParent(enemyObj.transform);
                body.transform.localPosition = new Vector3(0, 0.9f, 0);
                body.transform.localScale = new Vector3(0.75f, 0.9f, 0.75f);
                body.GetComponent<Renderer>().sharedMaterial = boneMat;
            }

            GameObject weaponObj = new GameObject("AttackHitbox");
            weaponObj.transform.SetParent(enemyObj.transform);
            weaponObj.transform.localPosition = new Vector3(0.45f, 0.75f, 0.5f);
            weaponObj.transform.localRotation = Quaternion.Euler(15, 0, 0);

            GameObject bladeMesh = InstantiateFBX($"{KayKitCharPath}Skeleton_Blade.fbx", weaponObj.transform, Vector3.zero, Quaternion.identity, Vector3.one * 1.1f, boneMat);
            if (bladeMesh == null)
            {
                GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                w.transform.SetParent(weaponObj.transform);
                w.transform.localPosition = Vector3.zero;
                w.transform.localScale = new Vector3(0.15f, 0.15f, 1.1f);
                w.GetComponent<Renderer>().sharedMaterial = steelMat;
                Object.DestroyImmediate(w.GetComponent<Collider>());
            }

            var col = weaponObj.AddComponent<BoxCollider>();
            col.size = new Vector3(0.6f, 0.6f, 1.3f);
            col.center = new Vector3(0, 0, 0.3f);
            col.isTrigger = true;
            col.enabled = false;
            var enemyHitbox = weaponObj.AddComponent<EnemyHitbox>();

            InstantiateFBX($"{KayKitCharPath}Skeleton_Shield_Large_A.fbx", enemyObj.transform, new Vector3(-0.45f, 0.8f, 0.2f), Quaternion.Euler(0, 80, 0), Vector3.one * 1.0f, boneMat);

            var capCol = enemyObj.AddComponent<CapsuleCollider>();
            capCol.center = new Vector3(0, 0.95f, 0);
            capCol.radius = 0.5f;
            capCol.height = 1.9f;

            var rb = enemyObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            enemyObj.tag = "Enemy";

            var health = enemyObj.AddComponent<EnemyHealth>();
            SetPrivateField(health, "_maxHealth", hp);
            SetPrivateField(health, "_dropKey", dropKey);

            var nav = enemyObj.AddComponent<UnityEngine.AI.NavMeshAgent>();
            nav.stoppingDistance = 1.15f;

            Transform[] wpTransforms = CreateWaypoints(parent, name, waypoints);

            var controller = enemyObj.AddComponent<EnemyController>();
            SetPrivateField(controller, "_patrolWaypoints", wpTransforms);
            SetPrivateField(controller, "_enemyHitbox", enemyHitbox);
            SetPrivateField(controller, "_attackRadius", 1.45f);
        }

        private static void CreateBossGuardian(Transform parent, string name, Vector3 position, Material bossMat, Material steelMat, int hp, KeyType dropKey, Vector3[] waypoints)
        {
            GameObject bossObj = new GameObject(name);
            bossObj.transform.SetParent(parent);
            bossObj.transform.position = position;

            float scale = 1.65f;

            GameObject bossModel = InstantiateFBX($"{KayKitCharPath}Skeleton_Warrior.fbx", bossObj.transform, Vector3.zero, Quaternion.identity, Vector3.one * scale, bossMat);
            if (bossModel != null)
            {
                RuntimeAnimatorController bossAnim = GetOrCreateSkeletonAnimatorController("Warrior");
                if (bossAnim != null)
                {
                    var anim = bossModel.GetComponent<Animator>();
                    if (anim == null) anim = bossModel.AddComponent<Animator>();
                    anim.runtimeAnimatorController = bossAnim;
                    anim.applyRootMotion = false;
                }
            }
            else
            {
                GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Guardian_Body";
                body.transform.SetParent(bossObj.transform);
                body.transform.localPosition = new Vector3(0, 0.9f * scale, 0);
                body.transform.localScale = new Vector3(0.95f * scale, 0.9f * scale, 0.95f * scale);
                body.GetComponent<Renderer>().sharedMaterial = bossMat;
            }

            GameObject weaponObj = new GameObject("AttackHitbox");
            weaponObj.transform.SetParent(bossObj.transform);
            weaponObj.transform.localPosition = new Vector3(0.6f * scale, 0.8f * scale, 0.7f * scale);
            weaponObj.transform.localRotation = Quaternion.Euler(20, 10, -15);

            GameObject bladeMesh = InstantiateFBX($"{KayKitCharPath}Skeleton_Blade.fbx", weaponObj.transform, Vector3.zero, Quaternion.identity, Vector3.one * (scale * 1.3f), bossMat);
            if (bladeMesh == null)
            {
                GameObject w = GameObject.CreatePrimitive(PrimitiveType.Cube);
                w.transform.SetParent(weaponObj.transform);
                w.transform.localPosition = Vector3.zero;
                w.transform.localScale = new Vector3(0.25f * scale, 0.25f * scale, 1.8f * scale);
                w.GetComponent<Renderer>().sharedMaterial = bossMat;
                Object.DestroyImmediate(w.GetComponent<Collider>());
            }

            var col = weaponObj.AddComponent<BoxCollider>();
            col.size = new Vector3(1.0f, 1.0f, 2.2f);
            col.center = new Vector3(0, 0, 0.6f);
            col.isTrigger = true;
            col.enabled = false;
            var enemyHitbox = weaponObj.AddComponent<EnemyHitbox>();
            SetPrivateField(enemyHitbox, "_damageAmount", 35);

            InstantiateFBX($"{KayKitCharPath}Skeleton_Shield_Large_A.fbx", bossObj.transform, new Vector3(-0.6f * scale, 0.85f * scale, 0.3f * scale), Quaternion.Euler(0, 80, 0), Vector3.one * (scale * 1.2f), bossMat);

            var capCol = bossObj.AddComponent<CapsuleCollider>();
            capCol.center = new Vector3(0, 0.95f * scale, 0);
            capCol.radius = 0.55f * scale;
            capCol.height = 1.9f * scale;

            var rb = bossObj.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            bossObj.tag = "Enemy";

            var health = bossObj.AddComponent<EnemyHealth>();
            SetPrivateField(health, "_maxHealth", hp);
            SetPrivateField(health, "_isBoss", true);
            SetPrivateField(health, "_dropKey", dropKey);

            var nav = bossObj.AddComponent<UnityEngine.AI.NavMeshAgent>();
            nav.stoppingDistance = 1.7f;
            nav.radius = 0.8f;
            nav.height = 2.8f;

            Transform[] wpTransforms = CreateWaypoints(parent, name, waypoints);

            var controller = bossObj.AddComponent<EnemyController>();
            SetPrivateField(controller, "_patrolWaypoints", wpTransforms);
            SetPrivateField(controller, "_enemyHitbox", enemyHitbox);
            SetPrivateField(controller, "_detectionRadius", 12f);
            SetPrivateField(controller, "_attackRadius", 2.1f);
        }

        private static Transform[] CreateWaypoints(Transform parent, string name, Vector3[] waypoints)
        {
            Transform[] wpTransforms = new Transform[waypoints.Length];
            for (int i = 0; i < waypoints.Length; i++)
            {
                var wp = new GameObject($"{name}_WP_{i}");
                wp.transform.SetParent(parent);
                wp.transform.position = waypoints[i];
                wpTransforms[i] = wp.transform;
            }
            return wpTransforms;
        }

        private static void CreateOrbitCamera(Transform playerTarget)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
                camObj.AddComponent<AudioListener>();
            }

            var orbit = cam.GetComponent<ThirdPersonOrbitCamera>();
            if (orbit == null) orbit = cam.gameObject.AddComponent<ThirdPersonOrbitCamera>();
            orbit.SetTarget(playerTarget);
        }

        private static RuntimeAnimatorController GetOrCreateKnightAnimatorController()
        {
            string animFolder = "Assets/_Project/Animations";
            if (!Directory.Exists(animFolder)) Directory.CreateDirectory(animFolder);

            string controllerPath = $"{animFolder}/Knight_Animator.controller";
            var existingController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (existingController != null)
            {
                return existingController;
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Roll", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("IsBlocking", AnimatorControllerParameterType.Bool);

            Object[] assets = AssetDatabase.LoadAllAssetsAtPath($"{KayKitCharPath}Knight.fbx");
            AnimationClip idleClip = null;
            AnimationClip walkClip = null;
            AnimationClip runClip = null;
            AnimationClip attackClip = null;
            AnimationClip jumpClip = null;
            AnimationClip blockClip = null;

            foreach (var obj in assets)
            {
                if (obj is AnimationClip clip)
                {
                    if (clip.name.Contains("__preview__")) continue;
                    string n = clip.name;
                    if (n.Equals("Idle") || (n.Contains("Idle") && idleClip == null)) idleClip = clip;
                    else if (n.Equals("Walking_A") || (n.Contains("Walk") && walkClip == null)) walkClip = clip;
                    else if (n.Equals("Running_A") || (n.Contains("Run") && runClip == null)) runClip = clip;
                    else if (n.Contains("1H_Melee_Attack_Slice_Horizontal") || (n.Contains("Attack") && attackClip == null)) attackClip = clip;
                    else if (n.Contains("Jump_Full_Short") || n.Contains("Jump_Start") || (n.Contains("Jump") && jumpClip == null)) jumpClip = clip;
                    else if (n.Equals("Blocking") || n.Equals("Block") || (n.Contains("Block") && blockClip == null)) blockClip = clip;
                }
            }

            var sm = controller.layers[0].stateMachine;

            var idleState = sm.AddState("Idle");
            idleState.motion = idleClip;

            var walkState = sm.AddState("Walk");
            walkState.motion = walkClip;

            var runState = sm.AddState("Run");
            runState.motion = runClip;

            var attackState = sm.AddState("Attack");
            attackState.motion = attackClip;

            var jumpState = sm.AddState("Jump");
            jumpState.motion = jumpClip;

            var t1 = idleState.AddTransition(walkState);
            t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            t1.hasExitTime = false;
            t1.duration = 0.12f;

            var t2 = walkState.AddTransition(idleState);
            t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            t2.hasExitTime = false;
            t2.duration = 0.12f;

            var t3 = walkState.AddTransition(runState);
            t3.AddCondition(AnimatorConditionMode.Greater, 0.7f, "Speed");
            t3.hasExitTime = false;
            t3.duration = 0.12f;

            var t4 = runState.AddTransition(walkState);
            t4.AddCondition(AnimatorConditionMode.Less, 0.7f, "Speed");
            t4.hasExitTime = false;
            t4.duration = 0.12f;

            var tAtk = sm.AddAnyStateTransition(attackState);
            tAtk.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            tAtk.hasExitTime = false;
            tAtk.duration = 0.05f;

            var tAtkEnd = attackState.AddTransition(idleState);
            tAtkEnd.hasExitTime = true;
            tAtkEnd.exitTime = 0.8f;
            tAtkEnd.duration = 0.15f;

            var tJmp = sm.AddAnyStateTransition(jumpState);
            tJmp.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            tJmp.hasExitTime = false;
            tJmp.duration = 0.05f;

            var tJmpEnd = jumpState.AddTransition(idleState);
            tJmpEnd.AddCondition(AnimatorConditionMode.If, 0, "IsGrounded");
            tJmpEnd.hasExitTime = true;
            tJmpEnd.exitTime = 0.5f;
            tJmpEnd.duration = 0.15f;

            var rollState = sm.AddState("Roll");
            rollState.motion = runClip != null ? runClip : walkClip;
            rollState.speed = 1.6f;

            var tRoll = sm.AddAnyStateTransition(rollState);
            tRoll.AddCondition(AnimatorConditionMode.If, 0, "Roll");
            tRoll.hasExitTime = false;
            tRoll.duration = 0.05f;

            var tRollEnd = rollState.AddTransition(idleState);
            tRollEnd.hasExitTime = true;
            tRollEnd.exitTime = 0.7f;
            tRollEnd.duration = 0.12f;

            var blockState = sm.AddState("Block");
            blockState.motion = blockClip != null ? blockClip : idleClip;

            var tBlock = sm.AddAnyStateTransition(blockState);
            tBlock.AddCondition(AnimatorConditionMode.If, 0, "IsBlocking");
            tBlock.hasExitTime = false;
            tBlock.duration = 0.08f;

            var tBlockEnd = blockState.AddTransition(idleState);
            tBlockEnd.AddCondition(AnimatorConditionMode.IfNot, 0, "IsBlocking");
            tBlockEnd.hasExitTime = false;
            tBlockEnd.duration = 0.12f;

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static RuntimeAnimatorController GetOrCreateSkeletonAnimatorController(string role)
        {
            string animFolder = "Assets/_Project/Animations";
            if (!Directory.Exists(animFolder)) Directory.CreateDirectory(animFolder);

            string controllerPath = $"{animFolder}/Skeleton_{role}_Animator.controller";
            var existingController = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (existingController != null)
            {
                return existingController;
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            string fbxFile = role == "Warrior" ? "Skeleton_Warrior.fbx" : "Skeleton_Minion.fbx";
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath($"{KayKitCharPath}{fbxFile}");
            AnimationClip idleClip = null;
            AnimationClip walkClip = null;
            AnimationClip attackClip = null;
            AnimationClip deathClip = null;

            foreach (var obj in assets)
            {
                if (obj is AnimationClip clip)
                {
                    if (clip.name.Contains("__preview__")) continue;
                    string n = clip.name;
                    if (n.Equals("Idle") || (n.Contains("Idle") && idleClip == null)) idleClip = clip;
                    else if (n.Equals("Walking_A") || (n.Contains("Walk") && walkClip == null)) walkClip = clip;
                    else if (n.Contains("1H_Melee_Attack_Slice_Horizontal") || (n.Contains("Attack") && attackClip == null)) attackClip = clip;
                    else if (n.Equals("Death_A") || (n.Contains("Death") && deathClip == null)) deathClip = clip;
                }
            }

            var sm = controller.layers[0].stateMachine;

            var idleState = sm.AddState("Idle");
            idleState.motion = idleClip;

            var walkState = sm.AddState("Walk");
            walkState.motion = walkClip;

            var attackState = sm.AddState("Attack");
            attackState.motion = attackClip;

            var deathState = sm.AddState("Death");
            deathState.motion = deathClip;

            var t1 = idleState.AddTransition(walkState);
            t1.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            t1.hasExitTime = false;
            t1.duration = 0.15f;

            var t2 = walkState.AddTransition(idleState);
            t2.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            t2.hasExitTime = false;
            t2.duration = 0.15f;

            var tAtk = sm.AddAnyStateTransition(attackState);
            tAtk.AddCondition(AnimatorConditionMode.If, 0, "Attack");
            tAtk.hasExitTime = false;
            tAtk.duration = 0.05f;

            var tAtkEnd = attackState.AddTransition(idleState);
            tAtkEnd.hasExitTime = true;
            tAtkEnd.exitTime = 0.85f;
            tAtkEnd.duration = 0.15f;

            var tDeath = sm.AddAnyStateTransition(deathState);
            tDeath.AddCondition(AnimatorConditionMode.If, 0, "Death");
            tDeath.hasExitTime = false;
            tDeath.duration = 0.05f;

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
            foreach (var s in existing)
            {
                if (s.path == scenePath) return;
            }

            var updated = new EditorBuildSettingsScene[existing.Length + 1];
            existing.CopyTo(updated, 0);
            updated[existing.Length] = new EditorBuildSettingsScene(scenePath, true);
            EditorBuildSettings.scenes = updated;
        }
    }
}
#endif

