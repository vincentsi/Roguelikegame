using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ProjectRoguelike.Procedural;

namespace ProjectRoguelike.Editor.Tools
{
    /// <summary>
    /// Editor tool to create the Run scene for procedural dungeon generation.
    /// </summary>
    public static class CreateRunScene
    {
        [MenuItem("Tools/Create Run Scene", priority = 1)]
        public static void CreateRunSceneMenu()
        {
            string scenePath = "Assets/_Project/Scenes/Run/Run.unity";
            
            // Check if scene already exists
            if (System.IO.File.Exists(scenePath))
            {
                if (!EditorUtility.DisplayDialog("Scene Exists", 
                    "Run scene already exists. Do you want to open it?", 
                    "Open", "Cancel"))
                {
                    return;
                }
                
                EditorSceneManager.OpenScene(scenePath);
                return;
            }

            // Create directory if it doesn't exist
            string directory = System.IO.Path.GetDirectoryName(scenePath);
            if (!System.IO.Directory.Exists(directory))
            {
                System.IO.Directory.CreateDirectory(directory);
            }

            // Create new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Remove default objects we don't need
            GameObject mainCamera = GameObject.Find("Main Camera");
            if (mainCamera != null)
            {
                Object.DestroyImmediate(mainCamera);
            }
            
            GameObject directionalLight = GameObject.Find("Directional Light");
            if (directionalLight != null)
            {
                Object.DestroyImmediate(directionalLight);
            }

            // Create DungeonManager
            GameObject dungeonManagerObj = new GameObject("DungeonManager");
            DungeonManager dungeonManager = dungeonManagerObj.AddComponent<DungeonManager>();
            
            // Create DungeonRoot
            GameObject dungeonRoot = new GameObject("DungeonRoot");
            dungeonRoot.transform.SetParent(dungeonManagerObj.transform);
            
            // Set DungeonRoot reference
            var serializedDungeonManager = new SerializedObject(dungeonManager);
            serializedDungeonManager.FindProperty("dungeonRoot").objectReferenceValue = dungeonRoot.transform;
            serializedDungeonManager.ApplyModifiedProperties();

            // Create a basic ground plane for NavMesh
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = Vector3.one * 100f; // Large ground for dungeon
            
            // Set ground layer to Default (for NavMesh)
            ground.layer = 0;

            // Create a simple directional light
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.8f);
            light.intensity = 1f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // Save scene
            bool saveSuccess = EditorSceneManager.SaveScene(newScene, scenePath);
            
            if (saveSuccess)
            {
                // Add scene to Build Settings
                AddSceneToBuildSettings(scenePath);
                
                Debug.Log($"[CreateRunScene] Run scene created successfully at {scenePath}");
                Debug.Log("[CreateRunScene] Scene added to Build Settings automatically.");
                Debug.Log("[CreateRunScene] Don't forget to:");
                Debug.Log("  1. Bake NavMesh in the Run scene (Window > AI > Navigation)");
                Debug.Log("  2. Assign RoomData prefabs to DungeonManager's Available Rooms list");
                Debug.Log("  3. Assign an enemy prefab to DungeonManager if you want auto-spawning");
            }
            else
            {
                Debug.LogError("[CreateRunScene] Failed to save Run scene!");
            }
        }
        
        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = UnityEditor.EditorBuildSettings.scenes;
            var sceneList = new System.Collections.Generic.List<UnityEditor.EditorBuildSettingsScene>(scenes);
            
            // Check if scene is already in build settings
            bool alreadyAdded = false;
            foreach (var scene in sceneList)
            {
                if (scene.path == scenePath)
                {
                    alreadyAdded = true;
                    if (!scene.enabled)
                    {
                        scene.enabled = true;
                        Debug.Log($"[CreateRunScene] Run scene was in Build Settings but disabled. Enabled it.");
                    }
                    break;
                }
            }
            
            if (!alreadyAdded)
            {
                sceneList.Add(new UnityEditor.EditorBuildSettingsScene(scenePath, true));
                Debug.Log($"[CreateRunScene] Added Run scene to Build Settings.");
            }
            
            UnityEditor.EditorBuildSettings.scenes = sceneList.ToArray();
        }
    }
}

