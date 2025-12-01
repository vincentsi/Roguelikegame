using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using ProjectRoguelike.Systems.Hub;
using ProjectRoguelike.Systems.Meta;

namespace ProjectRoguelike.Editor.Tools
{
    /// <summary>
    /// Editor tool to create the Hub scene with all necessary GameObjects.
    /// </summary>
    public static class HubSceneCreator
    {
        [MenuItem("Tools/Create Hub Scene")]
        public static void CreateHubScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            
            // Save scene
            string scenePath = "Assets/_Project/Scenes/Hub/Hub.unity";
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Scenes/Hub"))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Scenes", "Hub");
            }
            
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[HubSceneCreator] Created scene at {scenePath}");

            // Create Directional Light (if not already present)
            var existingLight = Object.FindObjectOfType<Light>();
            if (existingLight == null)
            {
                var lightObj = new GameObject("Directional Light");
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // Create Ground Plane
            var planeObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            planeObj.name = "Ground";
            planeObj.transform.position = Vector3.zero;
            planeObj.transform.localScale = new Vector3(10f, 1f, 10f);

            // Create HubManager
            var hubManagerObj = new GameObject("HubManager");
            var hubManager = hubManagerObj.AddComponent<HubManager>();

            // Create ShopSystem
            var shopSystemObj = new GameObject("ShopSystem");
            var shopSystem = shopSystemObj.AddComponent<ShopSystem>();

            // Create CharacterSelector
            var characterSelectorObj = new GameObject("CharacterSelector");
            var characterSelector = characterSelectorObj.AddComponent<CharacterSelector>();

            // Create NarrativeLab
            var narrativeLabObj = new GameObject("NarrativeLab");
            var narrativeLab = narrativeLabObj.AddComponent<NarrativeLab>();

            // Create SaveSystem
            var saveSystemObj = new GameObject("SaveSystem");
            var saveSystem = saveSystemObj.AddComponent<SaveSystem>();

            // Create Player Spawn Point
            var spawnPointObj = new GameObject("PlayerSpawnPoint");
            spawnPointObj.transform.position = new Vector3(0f, 1f, 0f);
            
            // Create Interactable Zones
            CreateInteractableZone("ShopZone", new Vector3(5f, 0f, 0f), InteractableZone.InteractableType.Shop);
            CreateInteractableZone("CharacterSelectionZone", new Vector3(-5f, 0f, 0f), InteractableZone.InteractableType.CharacterSelection);
            CreateInteractableZone("NarrativeLabZone", new Vector3(0f, 0f, 5f), InteractableZone.InteractableType.NarrativeLab);
            CreateInteractableZone("StartRunZone", new Vector3(0f, 0f, -5f), InteractableZone.InteractableType.StartRun);

            // Assign spawn point to HubManager using SerializedObject
            var serializedHubManager = new SerializedObject(hubManager);
            var spawnPointProperty = serializedHubManager.FindProperty("playerSpawnPoint");
            if (spawnPointProperty != null)
            {
                spawnPointProperty.objectReferenceValue = spawnPointObj.transform;
                serializedHubManager.ApplyModifiedProperties();
            }

            // Save scene again with all objects
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log("[HubSceneCreator] Hub scene created with all managers!");
            Debug.Log("  - HubManager");
            Debug.Log("  - ShopSystem");
            Debug.Log("  - CharacterSelector");
            Debug.Log("  - NarrativeLab");
            Debug.Log("  - SaveSystem");

            // Add scene to Build Settings
            AddSceneToBuildSettings(scenePath);

            // Focus on the scene
            EditorSceneManager.OpenScene(scenePath);
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var scenes = EditorBuildSettings.scenes;
            var sceneList = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes);

            // Check if scene is already in build settings
            bool alreadyAdded = false;
            foreach (var buildScene in sceneList)
            {
                if (buildScene.path == scenePath)
                {
                    alreadyAdded = true;
                    break;
                }
            }

            if (!alreadyAdded)
            {
                sceneList.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = sceneList.ToArray();
                Debug.Log($"[HubSceneCreator] Added {scenePath} to Build Settings");
            }
        }

        private static void CreateInteractableZone(string name, Vector3 position, InteractableZone.InteractableType type)
        {
            var zoneObj = new GameObject(name);
            zoneObj.transform.position = position;
            var zone = zoneObj.AddComponent<InteractableZone>();
            
            // Set interactable type using SerializedObject
            var serializedZone = new SerializedObject(zone);
            var typeProperty = serializedZone.FindProperty("interactableType");
            if (typeProperty != null)
            {
                typeProperty.enumValueIndex = (int)type;
                serializedZone.ApplyModifiedProperties();
            }

            // Add a visual indicator (simple sphere)
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Visual";
            sphere.transform.SetParent(zoneObj.transform);
            sphere.transform.localPosition = Vector3.zero;
            sphere.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // Remove collider from visual (InteractableZone uses trigger)
            var collider = sphere.GetComponent<Collider>();
            if (collider != null)
            {
                Object.DestroyImmediate(collider);
            }
        }
    }
}

