using Fenrir.Entities.Enemies;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace Fenrir.Editor
{
    /// <summary>
    /// Fenrir → Setup → Spawn Ash Wolf / Flame Sprite
    /// Creates a fully-wired enemy in the active scene:
    /// NavMeshAgent, EnemyBase, EnemyHealth, EnemyAI, EnemyCombat, EnemyTraitEmitter.
    ///
    /// Bake NavMesh after spawning: Window → AI → Navigation → Bake.
    /// Ensure "Enemy" tag exists: Edit → Project Settings → Tags and Layers.
    /// </summary>
    public static class FenrirEnemySetup
    {
        [MenuItem("Fenrir/Setup/Spawn Ash Wolf")]
        public static GameObject SpawnAshWolf()
            => SpawnEnemy("AshWolf",  new Vector3( 5f, 0f,  5f), maxHp: 80f,  atk: 12f, speed: 4.5f, aggro: 8f,  atkRange: 1.8f);

        [MenuItem("Fenrir/Setup/Spawn Flame Sprite")]
        public static GameObject SpawnFlameSprite()
            => SpawnEnemy("FlameSprite", new Vector3(-5f, 0f, 5f), maxHp: 45f, atk: 8f,  speed: 6f,   aggro: 10f, atkRange: 5f);

        // ── Internal ──────────────────────────────────────────────────────────

        private static GameObject SpawnEnemy(
            string name, Vector3 pos,
            float maxHp, float atk, float speed, float aggro, float atkRange)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            go.name = name;
            go.transform.position = pos;

            // Safe tag assignment — tag must exist in Project Settings
            try   { go.tag = "Enemy"; }
            catch { Debug.LogWarning("[FenrirEnemySetup] 'Enemy' tag not found — add it in Edit → Project Settings → Tags and Layers."); }

            // Material tint (URP Lit) — with null-safe shader lookup
            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
                if (urpLit != null)
                {
                    rend.sharedMaterial       = new Material(urpLit);
                    rend.sharedMaterial.color = new Color(0.8f, 0.2f, 0.1f);
                }
            }

            // NavMeshAgent — owns locomotion; Rigidbody NOT added (conflicts with NavMesh).
            NavMeshAgent agent      = go.AddComponent<NavMeshAgent>();
            agent.speed             = speed;
            agent.stoppingDistance  = atkRange * 0.8f;
            agent.radius            = 0.4f;
            agent.height            = 2f;

            // Enemy component stack
            EnemyHealth      health  = go.AddComponent<EnemyHealth>();
            EnemyCombat      combat  = go.AddComponent<EnemyCombat>();
            EnemyAI          ai      = go.AddComponent<EnemyAI>();
            go.AddComponent<EnemyBase>();
            go.AddComponent<EnemyTraitEmitter>();

            // Write serialised fields via SerializedObject (Undo-safe, domain-reload-safe)
            WriteFloat(health, "_maxHealth",   maxHp);
            WriteFloat(combat, "_baseAttack",  atk);
            WriteFloat(ai,     "_detectionRange", aggro);
            WriteFloat(ai,     "_attackRange", atkRange);

            Undo.RegisterCreatedObjectUndo(go, $"Spawn {name}");
            Selection.activeGameObject = go;

            Debug.Log($"[FenrirEnemySetup] '{name}' spawned at {pos}. " +
                      "Bake NavMesh: Window → AI → Navigation → Bake.");
            return go;
        }

        private static void WriteFloat(Component target, string fieldName, float value)
        {
            SerializedObject so   = new SerializedObject(target);
            SerializedProperty sp = so.FindProperty(fieldName);
            if (sp == null)
            {
                Debug.LogWarning($"[FenrirEnemySetup] Field '{fieldName}' not found on {target.GetType().Name}. " +
                                 "Check that the field is [SerializeField] and the name matches.");
                return;
            }
            sp.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
