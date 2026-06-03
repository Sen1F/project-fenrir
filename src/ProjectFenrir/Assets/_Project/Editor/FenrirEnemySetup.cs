using Fenrir.Entities.Enemies;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Fenrir.Editor
{
    /// <summary>
    /// Fenrir → Setup → Spawn Ash Wolf
    /// Creates an Ash Wolf enemy at the scene origin, fully wired:
    /// EnemyBase, EnemyHealth, EnemyAI, EnemyCombat, EnemyTraitEmitter, NavMeshAgent, Collider.
    /// </summary>
    public static class FenrirEnemySetup
    {
        [MenuItem("Fenrir/Setup/Spawn Ash Wolf")]
        public static GameObject SpawnAshWolf()
        {
            return SpawnEnemy(
                displayName:  "AshWolf",
                spawnPos:     new Vector3(5f, 0f, 5f),
                maxHealth:    80f,
                baseAttack:   12f,
                moveSpeed:    4.5f,
                aggroRange:   8f,
                attackRange:  1.8f
            );
        }

        [MenuItem("Fenrir/Setup/Spawn Flame Sprite")]
        public static GameObject SpawnFlameSprite()
        {
            return SpawnEnemy(
                displayName:  "FlameSprite",
                spawnPos:     new Vector3(-5f, 0f, 5f),
                maxHealth:    45f,
                baseAttack:   8f,
                moveSpeed:    6f,
                aggroRange:   10f,
                attackRange:  5f
            );
        }

        // ── Internal ──────────────────────────────────────────────────────────

        private static GameObject SpawnEnemy(
            string displayName,
            Vector3 spawnPos,
            float maxHealth,
            float baseAttack,
            float moveSpeed,
            float aggroRange,
            float attackRange)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = displayName;
            go.tag  = "Enemy";
            go.transform.position = spawnPos;

            // Tint enemy red so it's visually distinct from the player
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial           = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                renderer.sharedMaterial.color      = new Color(0.8f, 0.2f, 0.1f);
            }

            // NavMeshAgent
            NavMeshAgent agent   = go.AddComponent<NavMeshAgent>();
            agent.speed          = moveSpeed;
            agent.stoppingDistance = attackRange * 0.8f;
            agent.radius         = 0.4f;
            agent.height         = 2f;

            // Rigidbody (kinematic — movement is NavMesh-driven)
            Rigidbody rb         = go.AddComponent<Rigidbody>();
            rb.isKinematic       = true;
            rb.useGravity        = false;

            // Enemy component stack
            EnemyBase       enemyBase   = go.AddComponent<EnemyBase>();
            EnemyHealth     health      = go.AddComponent<EnemyHealth>();
            EnemyCombat     combat      = go.AddComponent<EnemyCombat>();
            EnemyAI         ai          = go.AddComponent<EnemyAI>();
            EnemyTraitEmitter traitEmitter = go.AddComponent<EnemyTraitEmitter>();

            // Serialised field values via SerializedObject (survives domain reload)
            SetSerializedFields(go, health, maxHealth, combat, baseAttack, ai, aggroRange, attackRange);

            Undo.RegisterCreatedObjectUndo(go, $"Spawn {displayName}");
            Selection.activeGameObject = go;

            Debug.Log($"[FenrirEnemySetup] Spawned {displayName} at {spawnPos}. " +
                      "Bake NavMesh before entering Play mode (Window → AI → Navigation → Bake).");

            return go;
        }

        private static void SetSerializedFields(
            GameObject go,
            EnemyHealth health, float maxHp,
            EnemyCombat combat, float atk,
            EnemyAI ai, float aggro, float atkRange)
        {
            // Use SerializedObject so Undo/Redo and Prefab workflows work correctly
            SerializedObject soHealth = new SerializedObject(health);
            soHealth.FindProperty("_maxHealth")?.SetValue(maxHp);
            soHealth.ApplyModifiedProperties();

            SerializedObject soCombat = new SerializedObject(combat);
            soCombat.FindProperty("_baseAttack")?.SetValue(atk);
            soCombat.ApplyModifiedProperties();

            SerializedObject soAi = new SerializedObject(ai);
            soAi.FindProperty("_aggroRange")?.SetValue(aggro);
            soAi.FindProperty("_attackRange")?.SetValue(atkRange);
            soAi.ApplyModifiedProperties();
        }
    }

    // Extension so FindProperty().SetValue() reads cleanly
    internal static class SerializedPropertyExt
    {
        internal static void SetValue(this SerializedProperty prop, float value)
        {
            if (prop != null) prop.floatValue = value;
        }
    }
}
