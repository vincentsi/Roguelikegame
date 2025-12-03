using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;
using System;
using ProjectRoguelike.Systems.Hub;
using ProjectRoguelike.Systems.Meta;
using ProjectRoguelike.UI.Hub;

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
            CreateHubSceneInternal();
        }

        [MenuItem("Tools/Create Hub UI")]
        public static void CreateHubUIOnly()
        {
            var hubManager = UnityEngine.Object.FindObjectOfType<HubManager>();
            if (hubManager == null)
            {
                EditorUtility.DisplayDialog("Error", "HubManager not found in the current scene. Please open the Hub scene first.", "OK");
                return;
            }

            EnsureEventSystem();
            CreateHubUI(hubManager);
            EditorUtility.DisplayDialog("Success", "Hub UI created and assigned to HubManager!", "OK");
        }

        [MenuItem("Tools/Create EventSystem")]
        public static void CreateEventSystemOnly()
        {
            EnsureEventSystem();
            EditorUtility.DisplayDialog("Success", "EventSystem created!", "OK");
        }

        private static void CreateHubSceneInternal()
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
            var existingLight = UnityEngine.Object.FindObjectOfType<Light>();
            if (existingLight == null)
            {
                var lightObj = new GameObject("Directional Light");
                var light = lightObj.AddComponent<Light>();
                light.type = LightType.Directional;
                lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            // Ground will be created as part of the Hub design

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

            // Create CollectionSystem
            var collectionSystemObj = new GameObject("CollectionSystem");
            var collectionSystem = collectionSystemObj.AddComponent<CollectionSystem>();

            // Create SaveSystem
            var saveSystemObj = new GameObject("SaveSystem");
            var saveSystem = saveSystemObj.AddComponent<SaveSystem>();

            // Create Player Spawn Point (center of the Hub)
            var spawnPointObj = new GameObject("PlayerSpawnPoint");
            spawnPointObj.transform.position = new Vector3(0f, 1f, 0f); // Center of the room, 1 unit above floor
            
            // Interactable zones will be positioned automatically after design is created
            // Create them with temporary positions first
            CreateInteractableZone("ShopZone", Vector3.zero, InteractableZone.InteractableType.Shop);
            CreateInteractableZone("CharacterSelectionZone", Vector3.zero, InteractableZone.InteractableType.CharacterSelection);
            CreateInteractableZone("NarrativeLabZone", Vector3.zero, InteractableZone.InteractableType.NarrativeLab);
            CreateInteractableZone("CollectionZone", Vector3.zero, InteractableZone.InteractableType.Collection);
            
            // Create 3 Run Doors (Left, Right, Front) - will be positioned at doors
            CreateInteractableZone("RunDoor_Left", Vector3.zero, InteractableZone.InteractableType.StartRunLeft);
            CreateInteractableZone("RunDoor_Right", Vector3.zero, InteractableZone.InteractableType.StartRunRight);
            CreateInteractableZone("RunDoor_Front", Vector3.zero, InteractableZone.InteractableType.StartRunFront);

            // Configure Run Doors in HubManager
            var serializedHubManager = new SerializedObject(hubManager);
            
            // Assign spawn point
            var spawnPointProperty = serializedHubManager.FindProperty("playerSpawnPoint");
            if (spawnPointProperty != null)
            {
                spawnPointProperty.objectReferenceValue = spawnPointObj.transform;
            }
            
            // Configure run doors (left, right, front)
            var leftDoorProperty = serializedHubManager.FindProperty("leftDoor");
            var rightDoorProperty = serializedHubManager.FindProperty("rightDoor");
            var frontDoorProperty = serializedHubManager.FindProperty("frontDoor");
            
            if (leftDoorProperty != null)
            {
                leftDoorProperty.FindPropertyRelative("doorName").stringValue = "Left Door";
                leftDoorProperty.FindPropertyRelative("levelName").stringValue = "Level 1";
                leftDoorProperty.FindPropertyRelative("difficulty").intValue = 1;
                leftDoorProperty.FindPropertyRelative("sceneName").stringValue = "Run";
                leftDoorProperty.FindPropertyRelative("isUnlocked").boolValue = true;
            }
            
            if (rightDoorProperty != null)
            {
                rightDoorProperty.FindPropertyRelative("doorName").stringValue = "Right Door";
                rightDoorProperty.FindPropertyRelative("levelName").stringValue = "Level 2";
                rightDoorProperty.FindPropertyRelative("difficulty").intValue = 2;
                rightDoorProperty.FindPropertyRelative("sceneName").stringValue = "Run";
                rightDoorProperty.FindPropertyRelative("isUnlocked").boolValue = true;
            }
            
            if (frontDoorProperty != null)
            {
                frontDoorProperty.FindPropertyRelative("doorName").stringValue = "Front Door";
                frontDoorProperty.FindPropertyRelative("levelName").stringValue = "Level 3";
                frontDoorProperty.FindPropertyRelative("difficulty").intValue = 3;
                frontDoorProperty.FindPropertyRelative("sceneName").stringValue = "Run";
                frontDoorProperty.FindPropertyRelative("isUnlocked").boolValue = true;
            }
            
            serializedHubManager.ApplyModifiedProperties();

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

            // Generate UI meta files
            GenerateUIMetaFiles();

            // Create UI elements
            CreateHubUI(hubManager);

            // Create Hub design (walls, doors, floor, etc.)
            CreateHubDesign();

            // Position interactable zones near props
            PositionInteractableZones();

            // Save scene with all changes
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorSceneManager.MarkSceneDirty(scene);

            // Focus on the scene
            EditorSceneManager.OpenScene(scenePath);
        }

        private static void CreateHubUI(HubManager hubManager)
        {
            // Ensure EventSystem exists
            EnsureEventSystem();

            // Check if Canvas already exists
            Canvas existingCanvas = UnityEngine.Object.FindObjectOfType<Canvas>();
            Transform canvasTransform;
            
            if (existingCanvas != null)
            {
                Debug.Log("[HubSceneCreator] Canvas already exists, creating missing UI panels.");
                canvasTransform = existingCanvas.transform;
            }
            else
            {
                // Create Canvas
                GameObject canvasObj = new GameObject("HubCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                canvasTransform = canvasObj.transform;
            }

            // Find or create Shop UI
            GameObject shopPanel = GameObject.Find("ShopPanel");
            if (shopPanel == null)
            {
                shopPanel = CreateShopUI(canvasTransform);
            }
            
            // Find or create Character Selection UI
            GameObject characterSelectionPanel = GameObject.Find("CharacterSelectionPanel");
            if (characterSelectionPanel == null)
            {
                characterSelectionPanel = CreateCharacterSelectionUI(canvasTransform);
            }
            
            // Find or create Narrative Lab UI
            GameObject narrativeLabPanel = GameObject.Find("NarrativeLabPanel");
            if (narrativeLabPanel == null)
            {
                narrativeLabPanel = CreateNarrativeLabUI(canvasTransform);
            }
            
            // Find or create Collection UI
            GameObject collectionPanel = GameObject.Find("CollectionPanel");
            if (collectionPanel == null)
            {
                collectionPanel = CreateCollectionUI(canvasTransform);
            }

            // Assign UI to HubManager
            var serializedHubManager = new SerializedObject(hubManager);
            serializedHubManager.FindProperty("shopUI").objectReferenceValue = shopPanel;
            serializedHubManager.FindProperty("characterSelectionUI").objectReferenceValue = characterSelectionPanel;
            serializedHubManager.FindProperty("narrativeLabUI").objectReferenceValue = narrativeLabPanel;
            serializedHubManager.FindProperty("collectionUI").objectReferenceValue = collectionPanel;
            serializedHubManager.ApplyModifiedProperties();

            Debug.Log("[HubSceneCreator] UI created and assigned to HubManager!");
        }

        private static GameObject CreateShopUI(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "ShopPanel", new Vector2(800, 600));
            panel.SetActive(false);

            // Title
            CreateText(panel.transform, "TitleText", "SHOP", 48, new Vector2(0, 250));

            // Currency Text
            GameObject currencyText = CreateText(panel.transform, "CurrencyText", "Currency: 0", 24, new Vector2(0, 200));
            
            // Close Button
            GameObject closeButton = CreateButton(panel.transform, "CloseButton", "X", new Vector2(350, 250), new Vector2(50, 50));

            // ScrollView for unlocks
            GameObject scrollView = CreateScrollView(panel.transform, "UnlockListScrollView", new Vector2(750, 400), new Vector2(0, -50));
            GameObject content = scrollView.transform.Find("Viewport/Content").gameObject;

            // Add ShopUI component
            ShopUI shopUI = panel.AddComponent<ShopUI>();
            var serializedShopUI = new SerializedObject(shopUI);
            serializedShopUI.FindProperty("currencyText").objectReferenceValue = currencyText.GetComponent<TextMeshProUGUI>();
            serializedShopUI.FindProperty("closeButton").objectReferenceValue = closeButton.GetComponent<Button>();
            serializedShopUI.FindProperty("unlockListParent").objectReferenceValue = content.transform;
            serializedShopUI.ApplyModifiedProperties();

            return panel;
        }

        private static GameObject CreateCharacterSelectionUI(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "CharacterSelectionPanel", new Vector2(800, 600));
            panel.SetActive(false);

            // Title
            CreateText(panel.transform, "TitleText", "SELECT CHARACTER", 48, new Vector2(0, 250));

            // Character Name
            GameObject nameText = CreateText(panel.transform, "CharacterNameText", "Character Name", 32, new Vector2(0, 150));

            // Character Description
            GameObject descText = CreateText(panel.transform, "CharacterDescriptionText", "Description", 20, new Vector2(0, 50));
            RectTransform descRect = descText.GetComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(600, 100);

            // Character Icon (Image placeholder)
            GameObject iconImage = new GameObject("CharacterIconImage");
            iconImage.transform.SetParent(panel.transform);
            Image image = iconImage.AddComponent<Image>();
            image.color = Color.gray;
            RectTransform iconRect = iconImage.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(100, 100);
            iconRect.anchoredPosition = new Vector2(0, 100);

            // Navigation Buttons
            GameObject prevButton = CreateButton(panel.transform, "PreviousButton", "<", new Vector2(-200, -150), new Vector2(100, 50));
            GameObject nextButton = CreateButton(panel.transform, "NextButton", ">", new Vector2(200, -150), new Vector2(100, 50));
            GameObject selectButton = CreateButton(panel.transform, "SelectButton", "SELECT", new Vector2(0, -200), new Vector2(200, 50));
            GameObject closeButton = CreateButton(panel.transform, "CloseButton", "X", new Vector2(350, 250), new Vector2(50, 50));

            // Add CharacterSelectionUI component
            CharacterSelectionUI charUI = panel.AddComponent<CharacterSelectionUI>();
            var serializedCharUI = new SerializedObject(charUI);
            serializedCharUI.FindProperty("characterNameText").objectReferenceValue = nameText.GetComponent<TextMeshProUGUI>();
            serializedCharUI.FindProperty("characterDescriptionText").objectReferenceValue = descText.GetComponent<TextMeshProUGUI>();
            serializedCharUI.FindProperty("characterIconImage").objectReferenceValue = image;
            serializedCharUI.FindProperty("previousButton").objectReferenceValue = prevButton.GetComponent<Button>();
            serializedCharUI.FindProperty("nextButton").objectReferenceValue = nextButton.GetComponent<Button>();
            serializedCharUI.FindProperty("selectButton").objectReferenceValue = selectButton.GetComponent<Button>();
            serializedCharUI.FindProperty("closeButton").objectReferenceValue = closeButton.GetComponent<Button>();
            serializedCharUI.ApplyModifiedProperties();

            return panel;
        }

        private static GameObject CreateNarrativeLabUI(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "NarrativeLabPanel", new Vector2(800, 600));
            panel.SetActive(false);

            // Title
            CreateText(panel.transform, "TitleText", "NARRATIVE LAB", 48, new Vector2(0, 250));

            // Selected Story Display
            GameObject selectedTitle = CreateText(panel.transform, "SelectedTitleText", "Story Title", 32, new Vector2(0, 150));
            GameObject selectedContent = CreateText(panel.transform, "SelectedContentText", "Story content...", 20, new Vector2(0, 0));
            RectTransform contentRect = selectedContent.GetComponent<RectTransform>();
            contentRect.sizeDelta = new Vector2(600, 200);

            // ScrollView for story list
            GameObject scrollView = CreateScrollView(panel.transform, "StoryListScrollView", new Vector2(750, 300), new Vector2(0, -100));
            GameObject content = scrollView.transform.Find("Viewport/Content").gameObject;

            // Close Button
            GameObject closeButton = CreateButton(panel.transform, "CloseButton", "X", new Vector2(350, 250), new Vector2(50, 50));

            // Add NarrativeLabUI component
            NarrativeLabUI narrativeUI = panel.AddComponent<NarrativeLabUI>();
            var serializedNarrativeUI = new SerializedObject(narrativeUI);
            serializedNarrativeUI.FindProperty("selectedTitleText").objectReferenceValue = selectedTitle.GetComponent<TextMeshProUGUI>();
            serializedNarrativeUI.FindProperty("selectedContentText").objectReferenceValue = selectedContent.GetComponent<TextMeshProUGUI>();
            serializedNarrativeUI.FindProperty("storyListParent").objectReferenceValue = content.transform;
            serializedNarrativeUI.FindProperty("closeButton").objectReferenceValue = closeButton.GetComponent<Button>();
            serializedNarrativeUI.ApplyModifiedProperties();

            return panel;
        }

        private static GameObject CreateCollectionUI(Transform parent)
        {
            GameObject panel = CreatePanel(parent, "CollectionPanel", new Vector2(1000, 700));
            panel.SetActive(false);

            // Title
            CreateText(panel.transform, "TitleText", "COLLECTION", 48, new Vector2(0, 300));

            // Category Tabs (horizontal layout)
            GameObject categoryTabsContainer = new GameObject("CategoryTabsContainer");
            categoryTabsContainer.transform.SetParent(panel.transform);
            HorizontalLayoutGroup tabsLayout = categoryTabsContainer.AddComponent<HorizontalLayoutGroup>();
            tabsLayout.spacing = 10f;
            tabsLayout.padding = new RectOffset(10, 10, 10, 10);
            tabsLayout.childAlignment = TextAnchor.MiddleCenter;
            RectTransform tabsRect = categoryTabsContainer.GetComponent<RectTransform>();
            tabsRect.anchorMin = new Vector2(0.5f, 1f);
            tabsRect.anchorMax = new Vector2(0.5f, 1f);
            tabsRect.sizeDelta = new Vector2(900, 60);
            tabsRect.anchoredPosition = new Vector2(0, -80);

            // Item List (left side)
            GameObject itemListScrollView = CreateScrollView(panel.transform, "ItemListScrollView", new Vector2(400, 500), new Vector2(-250, -50));
            GameObject itemListContent = itemListScrollView.transform.Find("Viewport/Content").gameObject;

            // Item Details (right side)
            GameObject detailsPanel = new GameObject("ItemDetailsPanel");
            detailsPanel.transform.SetParent(panel.transform);
            Image detailsImage = detailsPanel.AddComponent<Image>();
            detailsImage.color = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            RectTransform detailsRect = detailsPanel.GetComponent<RectTransform>();
            detailsRect.anchorMin = new Vector2(0.5f, 0.5f);
            detailsRect.anchorMax = new Vector2(0.5f, 0.5f);
            detailsRect.sizeDelta = new Vector2(450, 500);
            detailsRect.anchoredPosition = new Vector2(250, -50);

            // Item Name
            GameObject itemNameText = CreateText(detailsPanel.transform, "ItemNameText", "Item Name", 32, new Vector2(0, 200));
            
            // Item Icon
            GameObject iconImage = new GameObject("ItemIconImage");
            iconImage.transform.SetParent(detailsPanel.transform);
            Image icon = iconImage.AddComponent<Image>();
            icon.color = Color.gray;
            RectTransform iconRect = iconImage.GetComponent<RectTransform>();
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(150, 150);
            iconRect.anchoredPosition = new Vector2(0, 50);

            // Item Description
            GameObject itemDescText = CreateText(detailsPanel.transform, "ItemDescriptionText", "Description", 18, new Vector2(0, -100));
            RectTransform descRect = itemDescText.GetComponent<RectTransform>();
            descRect.sizeDelta = new Vector2(400, 200);

            // Item Status
            GameObject itemStatusText = CreateText(detailsPanel.transform, "ItemStatusText", "Status", 24, new Vector2(0, -200));

            // Close Button
            GameObject closeButton = CreateButton(panel.transform, "CloseButton", "X", new Vector2(450, 300), new Vector2(50, 50));

            // Create category tab prefab (simple button)
            GameObject categoryTabPrefab = CreateButton(categoryTabsContainer.transform, "CategoryTabPrefab", "Tab", Vector2.zero, new Vector2(120, 50));
            categoryTabPrefab.SetActive(false); // Hide prefab, will be used as template

            // Create collection item prefab (simple button with text)
            GameObject collectionItemPrefab = CreateButton(itemListContent.transform, "CollectionItemPrefab", "Item", Vector2.zero, new Vector2(350, 60));
            collectionItemPrefab.SetActive(false); // Hide prefab, will be used as template

            // Add CollectionUI component
            CollectionUI collectionUI = panel.AddComponent<CollectionUI>();
            var serializedCollectionUI = new SerializedObject(collectionUI);
            serializedCollectionUI.FindProperty("categoryTabsParent").objectReferenceValue = categoryTabsContainer.transform;
            serializedCollectionUI.FindProperty("itemListParent").objectReferenceValue = itemListContent.transform;
            serializedCollectionUI.FindProperty("categoryTabPrefab").objectReferenceValue = categoryTabPrefab;
            serializedCollectionUI.FindProperty("collectionItemPrefab").objectReferenceValue = collectionItemPrefab;
            serializedCollectionUI.FindProperty("itemNameText").objectReferenceValue = itemNameText.GetComponent<TextMeshProUGUI>();
            serializedCollectionUI.FindProperty("itemDescriptionText").objectReferenceValue = itemDescText.GetComponent<TextMeshProUGUI>();
            serializedCollectionUI.FindProperty("itemIconImage").objectReferenceValue = icon;
            serializedCollectionUI.FindProperty("itemStatusText").objectReferenceValue = itemStatusText.GetComponent<TextMeshProUGUI>();
            serializedCollectionUI.FindProperty("closeButton").objectReferenceValue = closeButton.GetComponent<Button>();
            serializedCollectionUI.ApplyModifiedProperties();

            return panel;
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 size)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            return panel;
        }

        private static GameObject CreateText(Transform parent, string name, string text, int fontSize, Vector2 position)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(400, 50);
            rect.anchoredPosition = position;
            return textObj;
        }

        private static GameObject CreateButton(Transform parent, string name, string text, Vector2 position, Vector2 size)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent);
            Image image = buttonObj.AddComponent<Image>();
            image.color = Color.gray;
            Button button = buttonObj.AddComponent<Button>();
            
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = 24;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            return buttonObj;
        }

        private static GameObject CreateScrollView(Transform parent, string name, Vector2 size, Vector2 position)
        {
            GameObject scrollView = new GameObject(name);
            scrollView.transform.SetParent(parent);
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.sizeDelta = size;
            scrollRect.anchoredPosition = position;

            Image image = scrollView.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            ScrollRect scroll = scrollView.AddComponent<ScrollRect>();

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform);
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.anchoredPosition = Vector2.zero;
            Image viewportImage = viewport.AddComponent<Image>();
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 0);
            contentRect.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.content = contentRect;
            scroll.viewport = viewportRect;
            scroll.vertical = true;
            scroll.horizontal = false;

            return scrollView;
        }

        private static void GenerateUIMetaFiles()
        {
            string uiHubPath = "Assets/_Project/UI/Hub";
            
            // Ensure UI/Hub folder exists
            if (!AssetDatabase.IsValidFolder(uiHubPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Project/UI"))
                {
                    AssetDatabase.CreateFolder("Assets/_Project", "UI");
                }
                AssetDatabase.CreateFolder("Assets/_Project/UI", "Hub");
            }

            // Scripts to create meta files for
            string[] scripts = new string[]
            {
                "ShopUI.cs",
                "CharacterSelectionUI.cs",
                "NarrativeLabUI.cs"
            };

            // Folder meta file
            CreateFolderMetaFile(uiHubPath);

            // Script meta files
            foreach (var script in scripts)
            {
                string scriptPath = Path.Combine(uiHubPath, script);
                if (File.Exists(scriptPath))
                {
                    CreateScriptMetaFile(scriptPath);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("[HubSceneCreator] UI meta files generated!");
        }

        private static void CreateFolderMetaFile(string folderPath)
        {
            string metaPath = folderPath + ".meta";
            
            if (File.Exists(metaPath))
            {
                return; // Already exists
            }

            string guid = Guid.NewGuid().ToString("N");
            string metaContent = $"fileFormatVersion: 2\r\nguid: {guid}\r\nfolderAsset: yes\r\nDefaultImporter:\r\n  externalObjects: {{}}\r\n  userData: \r\n  assetBundleName: \r\n  assetBundleVariant: \r\n";

            File.WriteAllText(metaPath, metaContent);
        }

        private static void CreateScriptMetaFile(string scriptPath)
        {
            string metaPath = scriptPath + ".meta";
            
            if (File.Exists(metaPath))
            {
                return; // Already exists
            }

            string guid = Guid.NewGuid().ToString("N");
            string metaContent = $"fileFormatVersion: 2\r\nguid: {guid}\r\nMonoImporter:\r\n  externalObjects: {{}}\r\n  serializedVersion: 2\r\n  defaultReferences: []\r\n  executionOrder: 0\r\n  icon: {{instanceID: 0}}\r\n  userData: \r\n  assetBundleName: \r\n  assetBundleVariant: \r\n";

            File.WriteAllText(metaPath, metaContent);
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
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void EnsureEventSystem()
        {
            // Check if EventSystem already exists
            EventSystem existingEventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            if (existingEventSystem != null)
            {
                return;
            }

            // Create EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            
            Debug.Log("[HubSceneCreator] EventSystem created for UI interactions.");
        }

        private static void CreateHubDesign()
        {
            // Remove old Ground if it exists
            GameObject oldGround = GameObject.Find("Ground");
            if (oldGround != null)
            {
                UnityEngine.Object.DestroyImmediate(oldGround);
            }

            // Check if HubStructure already exists
            GameObject hubStructure = GameObject.Find("HubStructure");
            if (hubStructure != null)
            {
                Debug.Log("[HubSceneCreator] HubStructure already exists, deleting and recreating.");
                UnityEngine.Object.DestroyImmediate(hubStructure);
            }

            hubStructure = new GameObject("HubStructure");

            // Create floor
            CreateFloor(hubStructure.transform);

            // Create walls
            CreateWalls(hubStructure.transform);

            // Create doors
            CreateDoors(hubStructure.transform);

            // Create ceiling
            CreateCeiling(hubStructure.transform);

            // Create lighting
            CreateLighting();

            // Create props/decoration
            CreateProps(hubStructure.transform);

            Debug.Log("[HubSceneCreator] Hub design created!");
        }

        private static void CreateFloor(Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetParent(parent);
            floor.transform.localScale = new Vector3(6f, 1f, 6f); // 60x60 units (reduced from 200)
            floor.transform.position = Vector3.zero;

            var renderer = floor.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material floorMat = new Material(Shader.Find("Standard"));
                floorMat.color = new Color(0.3f, 0.3f, 0.35f);
                floorMat.SetFloat("_Metallic", 0.2f);
                floorMat.SetFloat("_Glossiness", 0.3f);
                renderer.material = floorMat;
            }
        }

        private static void CreateWalls(Transform parent)
        {
            float wallHeight = 5f;
            float wallThickness = 0.5f;
            float roomSize = 60f; // Reduced from 200
            float doorWidth = 6f;
            float doorGap = doorWidth + 2f;

            GameObject wallsContainer = new GameObject("Walls");
            wallsContainer.transform.SetParent(parent);

            // Back wall (behind spawn point) - full wall
            CreateWall(wallsContainer.transform, "BackWall", 
                new Vector3(0, wallHeight / 2, roomSize / 2), 
                new Vector3(roomSize, wallHeight, wallThickness));

            // Front wall (with door) - split into two parts
            float frontWallHalf = (roomSize - doorGap) / 2f;
            CreateWall(wallsContainer.transform, "FrontWall_Left", 
                new Vector3(-(doorGap / 2f + frontWallHalf / 2f), wallHeight / 2, -roomSize / 2), 
                new Vector3(frontWallHalf, wallHeight, wallThickness));
            CreateWall(wallsContainer.transform, "FrontWall_Right", 
                new Vector3((doorGap / 2f + frontWallHalf / 2f), wallHeight / 2, -roomSize / 2), 
                new Vector3(frontWallHalf, wallHeight, wallThickness));

            // Left wall (with door) - split into two parts
            CreateWall(wallsContainer.transform, "LeftWall_Front", 
                new Vector3(-roomSize / 2, wallHeight / 2, (doorGap / 2f + frontWallHalf / 2f)), 
                new Vector3(wallThickness, wallHeight, frontWallHalf));
            CreateWall(wallsContainer.transform, "LeftWall_Back", 
                new Vector3(-roomSize / 2, wallHeight / 2, -(doorGap / 2f + frontWallHalf / 2f)), 
                new Vector3(wallThickness, wallHeight, frontWallHalf));

            // Right wall (with door) - split into two parts
            CreateWall(wallsContainer.transform, "RightWall_Front", 
                new Vector3(roomSize / 2, wallHeight / 2, (doorGap / 2f + frontWallHalf / 2f)), 
                new Vector3(wallThickness, wallHeight, frontWallHalf));
            CreateWall(wallsContainer.transform, "RightWall_Back", 
                new Vector3(roomSize / 2, wallHeight / 2, -(doorGap / 2f + frontWallHalf / 2f)), 
                new Vector3(wallThickness, wallHeight, frontWallHalf));
        }

        private static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
        {
            // Create Unity primitive cube for walls
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material wallMat = new Material(Shader.Find("Standard"));
                wallMat.color = new Color(0.4f, 0.4f, 0.45f);
                wallMat.SetFloat("_Metallic", 0.1f);
                wallMat.SetFloat("_Glossiness", 0.2f);
                renderer.material = wallMat;
            }

            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.position = position;
            wall.transform.localScale = scale;
        }

        private static void CreateDoors(Transform parent)
        {
            float doorWidth = 6f;
            float doorHeight = 4f;
            float roomSize = 60f;

            GameObject doorsContainer = new GameObject("Doors");
            doorsContainer.transform.SetParent(parent);

            // Front door (Start Run)
            CreateDoor(doorsContainer.transform, "FrontDoor", 
                new Vector3(0, doorHeight / 2, -roomSize / 2), 
                new Vector3(doorWidth, doorHeight, 0.1f),
                InteractableZone.InteractableType.StartRunFront);

            // Left door (Start Run Left)
            CreateDoor(doorsContainer.transform, "LeftDoor", 
                new Vector3(-roomSize / 2, doorHeight / 2, 0), 
                new Vector3(0.1f, doorHeight, doorWidth),
                InteractableZone.InteractableType.StartRunLeft);

            // Right door (Start Run Right)
            CreateDoor(doorsContainer.transform, "RightDoor", 
                new Vector3(roomSize / 2, doorHeight / 2, 0), 
                new Vector3(0.1f, doorHeight, doorWidth),
                InteractableZone.InteractableType.StartRunRight);
        }

        private static void CreateDoor(Transform parent, string name, Vector3 position, Vector3 scale, InteractableZone.InteractableType doorType)
        {
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
            door.name = name;
            door.transform.SetParent(parent);
            door.transform.localPosition = position;
            door.transform.localScale = scale;

            var renderer = door.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material doorMat = new Material(Shader.Find("Standard"));
                doorMat.color = new Color(0.2f, 0.6f, 0.8f);
                doorMat.SetFloat("_Metallic", 0.5f);
                doorMat.SetFloat("_Glossiness", 0.7f);
                doorMat.EnableKeyword("_EMISSION");
                doorMat.SetColor("_EmissionColor", new Color(0.1f, 0.3f, 0.4f));
                renderer.material = doorMat;
            }
        }

        private static void CreateCeiling(Transform parent)
        {
            GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ceiling.name = "Ceiling";
            ceiling.transform.SetParent(parent);
            ceiling.transform.localScale = new Vector3(6f, 1f, 6f);
            ceiling.transform.position = new Vector3(0, 5f, 0);
            ceiling.transform.rotation = Quaternion.Euler(180f, 0, 0);

            var renderer = ceiling.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material ceilingMat = new Material(Shader.Find("Standard"));
                ceilingMat.color = new Color(0.25f, 0.25f, 0.3f);
                renderer.material = ceilingMat;
            }
        }

        private static void CreateLighting()
        {
            var existingLights = UnityEngine.Object.FindObjectsOfType<Light>();
            if (existingLights.Length > 0)
            {
                return;
            }

            GameObject mainLight = new GameObject("MainLight");
            Light light = mainLight.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.9f);
            light.intensity = 1f;
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            CreatePointLight(new Vector3(-15f, 4f, -15f), "Light_Shop");
            CreatePointLight(new Vector3(15f, 4f, -15f), "Light_CharacterSelection");
            CreatePointLight(new Vector3(0f, 4f, 15f), "Light_NarrativeLab");
        }

        private static void CreatePointLight(Vector3 position, string name)
        {
            GameObject lightObj = new GameObject(name);
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.8f, 0.9f, 1f);
            light.intensity = 2f;
            light.range = 20f;
            lightObj.transform.position = position;
        }

        private static void CreateProps(Transform parent)
        {
            GameObject propsContainer = new GameObject("Props");
            propsContainer.transform.SetParent(parent);

            // Shop area props (left side, back)
            CreateTable(propsContainer.transform, "Table_Shop", new Vector3(-15f, 0f, -15f));
            CreateScreen(propsContainer.transform, "Screen_Shop", new Vector3(-15f, 1.5f, -15f));

            // Character Selection area props (right side, back)
            CreateTable(propsContainer.transform, "Table_CharacterSelection", new Vector3(15f, 0f, -15f));
            CreateScreen(propsContainer.transform, "Screen_CharacterSelection", new Vector3(15f, 1.5f, -15f));

            // Narrative Lab area props (center, front)
            CreateTable(propsContainer.transform, "Table_NarrativeLab", new Vector3(0f, 0f, 15f));
            CreateScreen(propsContainer.transform, "Screen_NarrativeLab", new Vector3(0f, 1.5f, 15f));

            // Collection area props (left side, front)
            CreateTable(propsContainer.transform, "Table_Collection", new Vector3(-15f, 0f, 15f));
            CreateScreen(propsContainer.transform, "Screen_Collection", new Vector3(-15f, 1.5f, 15f));
        }

        private static void CreateTable(Transform parent, string name, Vector3 position)
        {
            GameObject table = GameObject.CreatePrimitive(PrimitiveType.Cube);
            table.name = name;
            table.transform.SetParent(parent);
            table.transform.position = position;
            table.transform.localScale = new Vector3(3f, 0.1f, 1.5f);

            var renderer = table.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.2f, 0.2f, 0.25f);
                renderer.material = mat;
            }
        }

        private static void CreateScreen(Transform parent, string name, Vector3 position)
        {
            GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = name;
            screen.transform.SetParent(parent);
            screen.transform.position = position;
            screen.transform.localScale = new Vector3(1.5f, 1f, 1f);
            screen.transform.rotation = Quaternion.Euler(0, 180f, 0);

            var renderer = screen.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.color = new Color(0.1f, 0.3f, 0.5f);
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", new Color(0.05f, 0.15f, 0.25f));
                renderer.material = mat;
            }
        }

        private static void PositionInteractableZones()
        {
            float roomSize = 60f;

            // Position ShopZone near Shop props (left side, back)
            GameObject shopZone = GameObject.Find("ShopZone");
            if (shopZone != null)
            {
                shopZone.transform.position = new Vector3(-15f, 1f, -15f);
                Debug.Log($"[HubSceneCreator] ShopZone positioned at {shopZone.transform.position}");
            }
            else
            {
                Debug.LogWarning("[HubSceneCreator] ShopZone not found!");
            }

            // Position CharacterSelectionZone near Character Selection props (right side, back)
            GameObject charZone = GameObject.Find("CharacterSelectionZone");
            if (charZone != null)
            {
                charZone.transform.position = new Vector3(15f, 1f, -15f);
                Debug.Log($"[HubSceneCreator] CharacterSelectionZone positioned at {charZone.transform.position}");
            }
            else
            {
                Debug.LogWarning("[HubSceneCreator] CharacterSelectionZone not found!");
            }

            // Position NarrativeLabZone near Narrative Lab props (center, front)
            GameObject narrativeZone = GameObject.Find("NarrativeLabZone");
            if (narrativeZone != null)
            {
                narrativeZone.transform.position = new Vector3(0f, 1f, 15f);
                Debug.Log($"[HubSceneCreator] NarrativeLabZone positioned at {narrativeZone.transform.position}");
            }
            else
            {
                Debug.LogWarning("[HubSceneCreator] NarrativeLabZone not found!");
            }

            // Position CollectionZone near Collection props (left side, front)
            GameObject collectionZone = GameObject.Find("CollectionZone");
            if (collectionZone != null)
            {
                collectionZone.transform.position = new Vector3(-15f, 1f, 15f);
                Debug.Log($"[HubSceneCreator] CollectionZone positioned at {collectionZone.transform.position}");
            }
            else
            {
                Debug.LogWarning("[HubSceneCreator] CollectionZone not found!");
            }

            // Position Run Doors at door locations
            GameObject leftDoorZone = GameObject.Find("RunDoor_Left");
            if (leftDoorZone != null)
            {
                leftDoorZone.transform.position = new Vector3(-roomSize / 2, 1f, 0f);
                Debug.Log($"[HubSceneCreator] RunDoor_Left positioned at {leftDoorZone.transform.position}");
            }
            else
            {
                Debug.LogWarning("[HubSceneCreator] RunDoor_Left not found!");
            }

            GameObject rightDoorZone = GameObject.Find("RunDoor_Right");
            if (rightDoorZone != null)
            {
                rightDoorZone.transform.position = new Vector3(roomSize / 2, 1f, 0f);
                Debug.Log($"[HubSceneCreator] RunDoor_Right positioned at {rightDoorZone.transform.position}");
            }
            else
            {
                Debug.LogWarning("[HubSceneCreator] RunDoor_Right not found!");
            }

            GameObject frontDoorZone = GameObject.Find("RunDoor_Front");
            if (frontDoorZone != null)
            {
                frontDoorZone.transform.position = new Vector3(0f, 1f, -roomSize / 2);
                Debug.Log($"[HubSceneCreator] RunDoor_Front positioned at {frontDoorZone.transform.position}");
            }
            else
            {
                Debug.LogWarning("[HubSceneCreator] RunDoor_Front not found!");
            }

            Debug.Log("[HubSceneCreator] All interactable zones positioned!");
        }
    }
}

