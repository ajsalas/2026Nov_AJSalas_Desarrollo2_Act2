using UnityEngine;
using DungeonSurvivor.Core;
using DungeonSurvivor.Enemy;

namespace DungeonSurvivor.Enemy
{

    public class EnemySpawner : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInit()
        {
            var existing = FindFirstObjectByType<EnemySpawner>();
            if (existing == null)
            {
                GameObject spawnerObj = new GameObject("EnemySpawner_Runtime");
                spawnerObj.AddComponent<EnemySpawner>();
            }
        }

        private void Start()
        {
            SpawnBalancedEnemies();
        }

        public void SpawnBalancedEnemies()
        {

            var oldL2 = GameObject.Find("Skeleton_Minion_Antechamber_L2");
            if (oldL2 != null) Destroy(oldL2);
            var oldR2 = GameObject.Find("Skeleton_Minion_Antechamber_R2");
            if (oldR2 != null) Destroy(oldR2);
            var oldGuard = GameObject.Find("Skeleton_Guard");
            if (oldGuard != null) Destroy(oldGuard);

            var allEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            EnemyController skeletonTemplate = null;

            foreach (var enemy in allEnemies)
            {
                var health = enemy.GetComponent<EnemyHealth>();
                if (health != null && !health.IsBoss)
                {
                    skeletonTemplate = enemy;
                    break;
                }
            }

            if (skeletonTemplate == null) return;

            Transform charactersParent = skeletonTemplate.transform.parent;

            SpawnMinion(skeletonTemplate.gameObject, charactersParent, "Skeleton_Minion_Antechamber_L", new Vector3(-3.5f, 0f, 3.0f), Quaternion.Euler(0, 180, 0), 40);
            SpawnMinion(skeletonTemplate.gameObject, charactersParent, "Skeleton_Minion_Antechamber_R", new Vector3(3.5f, 0f, 3.0f), Quaternion.Euler(0, 180, 0), 40);

            SpawnMinion(skeletonTemplate.gameObject, charactersParent, "Skeleton_Minion_SideCrypt_1", new Vector3(15.0f, 0f, 17.5f), Quaternion.Euler(0, -90, 0), 40,
                new Vector3[] { new Vector3(13.0f, 0f, 17.5f), new Vector3(18.5f, 0f, 17.5f) });
            SpawnMinion(skeletonTemplate.gameObject, charactersParent, "Skeleton_Minion_SideCrypt_2", new Vector3(15.0f, 0f, 22.5f), Quaternion.Euler(0, -90, 0), 40,
                new Vector3[] { new Vector3(13.0f, 0f, 22.5f), new Vector3(18.5f, 0f, 22.5f) });

            SpawnMinion(skeletonTemplate.gameObject, charactersParent, "Skeleton_Boss_Add_L", new Vector3(-5.0f, 0f, 48.0f), Quaternion.Euler(0, 180, 0), 35);
            SpawnMinion(skeletonTemplate.gameObject, charactersParent, "Skeleton_Boss_Add_R", new Vector3(5.0f, 0f, 48.0f), Quaternion.Euler(0, 180, 0), 35);
        }

        private void SpawnMinion(GameObject template, Transform parent, string name, Vector3 position, Quaternion rotation, int maxHealth, Vector3[] waypoints = null)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
            {

                var exCtrl = existing.GetComponent<EnemyController>();
                if (exCtrl != null) exCtrl.EnsureAnimatorController();
                return;
            }

            GameObject clone = Instantiate(template, position, rotation, parent);
            clone.name = name;
            clone.SetActive(true);

            var templateAnim = template.GetComponentInChildren<Animator>();
            var cloneAnim = clone.GetComponentInChildren<Animator>();
            if (cloneAnim != null && templateAnim != null && templateAnim.runtimeAnimatorController != null)
            {
                cloneAnim.runtimeAnimatorController = templateAnim.runtimeAnimatorController;
            }

            var health = clone.GetComponent<EnemyHealth>();
            if (health != null)
            {
                var maxHealthField = typeof(EnemyHealth).GetField("_maxHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (maxHealthField != null) maxHealthField.SetValue(health, maxHealth);

                var curHealthField = typeof(EnemyHealth).GetField("_currentHealth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (curHealthField != null) curHealthField.SetValue(health, maxHealth);

                var bossField = typeof(EnemyHealth).GetField("_isBoss", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (bossField != null) bossField.SetValue(health, false);
            }

            var controller = clone.GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.EnsureAnimatorController();

                var detRadiusField = typeof(EnemyController).GetField("_detectionRadius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (detRadiusField != null) detRadiusField.SetValue(controller, 7.5f);

                var attRadiusField = typeof(EnemyController).GetField("_attackRadius", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (attRadiusField != null) attRadiusField.SetValue(controller, 1.45f);

                var agent = clone.GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null) agent.stoppingDistance = 1.15f;

                if (waypoints != null && waypoints.Length > 0)
                {
                    Transform[] wpTransforms = new Transform[waypoints.Length];
                    for (int i = 0; i < waypoints.Length; i++)
                    {
                        GameObject wpObj = new GameObject($"{name}_WP_{i}");
                        wpObj.transform.SetParent(clone.transform.parent);
                        wpObj.transform.position = waypoints[i];
                        wpTransforms[i] = wpObj.transform;
                    }
                    var wpField = typeof(EnemyController).GetField("_patrolWaypoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (wpField != null) wpField.SetValue(controller, wpTransforms);
                }
                else
                {
                    var wpField = typeof(EnemyController).GetField("_patrolWaypoints", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (wpField != null) wpField.SetValue(controller, new Transform[0]);
                }
            }
        }
    }
}

