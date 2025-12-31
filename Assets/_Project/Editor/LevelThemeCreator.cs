using UnityEngine;
using UnityEditor;
using ProjectRoguelike.Levels;

namespace ProjectRoguelike.Editor
{
    /// <summary>
    /// Utilitaire d'éditeur pour créer des LevelTheme ScriptableObjects rapidement.
    /// </summary>
    public static class LevelThemeCreator
    {
        [MenuItem("Assets/Create/Roguelike/Level Themes/Laboratory Theme")]
        public static void CreateLaboratoryTheme()
        {
            CreateTheme("Laboratory", "A sterile research facility with white walls and scientific equipment");
        }

        [MenuItem("Assets/Create/Roguelike/Level Themes/Biological Theme")]
        public static void CreateBiologicalTheme()
        {
            CreateTheme("Biological", "Organic containment zones with growth chambers and specimens");
        }

        [MenuItem("Assets/Create/Roguelike/Level Themes/Containment Theme")]
        public static void CreateContainmentTheme()
        {
            CreateTheme("Containment", "High-security prison cells and reinforced barriers");
        }

        private static void CreateTheme(string themeName, string description)
        {
            var theme = ScriptableObject.CreateInstance<LevelTheme>();
            theme.name = themeName;

            // Utiliser SerializedObject pour modifier les champs privés
            var serializedObject = new SerializedObject(theme);

            // Configuration par défaut
            serializedObject.FindProperty("themeName").stringValue = themeName;
            serializedObject.FindProperty("levelIndex").intValue = themeName switch
            {
                "Laboratory" => 1,
                "Biological" => 2,
                "Containment" => 3,
                _ => 1
            };

            // Couleurs ambiantes selon le thème
            if (themeName == "Laboratory")
            {
                serializedObject.FindProperty("ambientColor").colorValue = new Color(0.9f, 0.9f, 0.95f); // Blanc bleuté
            }
            else if (themeName == "Biological")
            {
                serializedObject.FindProperty("ambientColor").colorValue = new Color(0.7f, 0.9f, 0.7f); // Vert pâle
            }
            else if (themeName == "Containment")
            {
                serializedObject.FindProperty("ambientColor").colorValue = new Color(0.3f, 0.3f, 0.35f); // Gris foncé
            }

            // Difficulté
            serializedObject.FindProperty("enemyDifficultyMultiplier").floatValue = themeName switch
            {
                "Laboratory" => 1.0f,
                "Biological" => 1.3f,
                "Containment" => 1.6f,
                _ => 1.0f
            };

            // Nombre d'ennemis de base
            serializedObject.FindProperty("baseEnemyCount").intValue = 5;

            // Appliquer les modifications
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            // Sauvegarder
            string path = "Assets/_Project/ScriptableObjects/Levels/" + themeName + "Theme.asset";
            AssetDatabase.CreateAsset(theme, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = theme;

            Debug.Log($"[LevelThemeCreator] Created {themeName} theme at {path}");
        }

        [MenuItem("Assets/Create/Roguelike/Complete Theme Set")]
        public static void CreateAllThemes()
        {
            CreateLaboratoryTheme();
            CreateBiologicalTheme();
            CreateContainmentTheme();

            Debug.Log("[LevelThemeCreator] Created all 3 level themes!");
        }
    }
}
