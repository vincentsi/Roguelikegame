using UnityEngine;
using UnityEditor;
using ProjectRoguelike.Procedural;

namespace ProjectRoguelike.Editor
{
    /// <summary>
    /// Utilitaire d'éditeur pour créer des RoomData ScriptableObjects rapidement.
    /// </summary>
    public static class RoomDataCreator
    {
        [MenuItem("Assets/Create/Roguelike/Rooms/Combat Room")]
        public static void CreateCombatRoom()
        {
            CreateRoom(RoomType.Combat, "CombatRoom");
        }

        [MenuItem("Assets/Create/Roguelike/Rooms/Elite Room")]
        public static void CreateEliteRoom()
        {
            CreateRoom(RoomType.Elite, "EliteRoom");
        }

        [MenuItem("Assets/Create/Roguelike/Rooms/Shop Room")]
        public static void CreateShopRoom()
        {
            CreateRoom(RoomType.Shop, "ShopRoom");
        }

        [MenuItem("Assets/Create/Roguelike/Rooms/Event Room")]
        public static void CreateEventRoom()
        {
            CreateRoom(RoomType.Event, "EventRoom");
        }

        [MenuItem("Assets/Create/Roguelike/Rooms/Boss Room")]
        public static void CreateBossRoom()
        {
            CreateRoom(RoomType.Boss, "BossRoom");
        }

        [MenuItem("Assets/Create/Roguelike/Rooms/Loot Room")]
        public static void CreateLootRoom()
        {
            CreateRoom(RoomType.Loot, "LootRoom");
        }

        private static void CreateRoom(RoomType roomType, string baseName)
        {
            var roomData = ScriptableObject.CreateInstance<RoomData>();
            roomData.name = baseName;

            // Utiliser SerializedObject pour modifier les champs privés
            var serializedObject = new SerializedObject(roomData);

            // Configuration par défaut
            serializedObject.FindProperty("roomName").stringValue = baseName;
            serializedObject.FindProperty("roomType").enumValueIndex = (int)roomType;

            // Taille par défaut
            serializedObject.FindProperty("size").vector2IntValue = new Vector2Int(10, 10);

            // Portes par défaut (4 portes dans toutes les directions)
            serializedObject.FindProperty("hasNorthDoor").boolValue = true;
            serializedObject.FindProperty("hasSouthDoor").boolValue = true;
            serializedObject.FindProperty("hasEastDoor").boolValue = true;
            serializedObject.FindProperty("hasWestDoor").boolValue = true;

            // Profondeur selon le type
            switch (roomType)
            {
                case RoomType.Combat:
                    serializedObject.FindProperty("minDepth").intValue = 0;
                    serializedObject.FindProperty("maxDepth").intValue = 100;
                    serializedObject.FindProperty("spawnWeight").floatValue = 1.0f;
                    break;

                case RoomType.Elite:
                    serializedObject.FindProperty("minDepth").intValue = 2;
                    serializedObject.FindProperty("maxDepth").intValue = 100;
                    serializedObject.FindProperty("spawnWeight").floatValue = 0.5f;
                    break;

                case RoomType.Shop:
                    serializedObject.FindProperty("minDepth").intValue = 1;
                    serializedObject.FindProperty("maxDepth").intValue = 100;
                    serializedObject.FindProperty("spawnWeight").floatValue = 0.3f;
                    break;

                case RoomType.Event:
                    serializedObject.FindProperty("minDepth").intValue = 1;
                    serializedObject.FindProperty("maxDepth").intValue = 100;
                    serializedObject.FindProperty("spawnWeight").floatValue = 0.4f;
                    break;

                case RoomType.Boss:
                    serializedObject.FindProperty("minDepth").intValue = 4;
                    serializedObject.FindProperty("maxDepth").intValue = 100;
                    serializedObject.FindProperty("spawnWeight").floatValue = 1.0f;
                    break;

                case RoomType.Loot:
                    serializedObject.FindProperty("minDepth").intValue = 0;
                    serializedObject.FindProperty("maxDepth").intValue = 100;
                    serializedObject.FindProperty("spawnWeight").floatValue = 0.6f;
                    break;
            }

            // Appliquer les modifications
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            // Sauvegarder
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Room Data",
                baseName,
                "asset",
                "Choose where to save the room data",
                "Assets/_Project/ScriptableObjects/Rooms"
            );

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(roomData, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorUtility.FocusProjectWindow();
                Selection.activeObject = roomData;

                Debug.Log($"[RoomDataCreator] Created {roomType} room at {path}");
            }
        }
    }
}
