#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DungeonSurvivor.EditorTools
{
    [InitializeOnLoad]
    public static class AutoSceneUpdater
    {
        private const string FlagFile = "Temp/DungeonCrypt_v13_CombatBalance.flag";

        static AutoSceneUpdater()
        {
            EditorApplication.delayCall += OnEditorReady;
        }

        private static void OnEditorReady()
        {
            CheckAndBuildScene();
        }

        private static void CheckAndBuildScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;

            if (!File.Exists(FlagFile))
            {

                var scene = EditorSceneManager.OpenScene("Assets/_Project/Scenes/DungeonCrypt_Main.unity");

                var allTorches = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                bool modified = false;
                for (int i = 0; i < allTorches.Length; i++)
                {
                    var t = allTorches[i];
                    if (t != null && (t.name == "Wall_Torch" || t.name.Contains("torch_mounted")))
                    {
                        Vector3 pos = t.transform.position;
                        if (pos.x > 1.2f && pos.x < 4.5f && pos.z > 14.5f && pos.z < 25.5f)
                        {
                            t.transform.position = new Vector3(2.85f, 2.2f, 11.5f);
                            t.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
                            modified = true;
                        }
                    }
                }

                var allEnemies = GameObject.FindObjectsByType<Enemy.EnemyController>(FindObjectsSortMode.None);
                foreach (var enemy in allEnemies)
                {
                    if (enemy != null)
                    {
                        var health = enemy.GetComponent<Enemy.EnemyHealth>();
                        bool isBoss = health != null && health.IsBoss;

                        var so = new SerializedObject(enemy);
                        var attProp = so.FindProperty("_attackRadius");
                        if (attProp != null)
                        {
                            attProp.floatValue = isBoss ? 2.1f : 1.45f;
                        }
                        so.ApplyModifiedProperties();

                        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
                        if (agent != null)
                        {
                            agent.stoppingDistance = isBoss ? 1.7f : 1.15f;
                        }
                        modified = true;
                    }
                }

                if (modified)
                {
                    EditorSceneManager.SaveOpenScenes();
                }

                try
                {
                    string dir = Path.GetDirectoryName(FlagFile);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    File.WriteAllText(FlagFile, System.DateTime.Now.ToString());
                }
                catch {}
            }
        }
    }
}
#endif

