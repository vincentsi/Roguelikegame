using System.Collections;
using UnityEngine;

namespace ProjectRoguelike.Systems.Hub
{
    /// <summary>
    /// Loads Hub props progressively at runtime to avoid freeze during scene load.
    /// </summary>
    public sealed class HubPropsLoader : MonoBehaviour
    {
        [Header("Props Configuration")]
        [SerializeField] private bool loadPropsAtRuntime = true;
        [SerializeField] private float delayBetweenProps = 0.1f; // Delay between each prop creation
        
        [Header("Props Prefabs")]
        [SerializeField] private GameObject tablePrefab;
        [SerializeField] private GameObject screenPrefab;
        
        [Header("Props Positions")]
        [SerializeField] private Vector3[] tablePositions = new Vector3[]
        {
            new Vector3(-15f, 0f, -15f), // Shop
            new Vector3(15f, 0f, -15f),  // Character Selection
            new Vector3(0f, 0f, 15f),    // Narrative Lab
            new Vector3(-15f, 0f, 15f)   // Collection
        };
        
        [SerializeField] private Vector3[] screenPositions = new Vector3[]
        {
            new Vector3(-15f, 1.5f, -15f), // Shop
            new Vector3(15f, 1.5f, -15f),  // Character Selection
            new Vector3(0f, 1.5f, 15f),    // Narrative Lab
            new Vector3(-15f, 1.5f, 15f)   // Collection
        };

        private void Start()
        {
            if (loadPropsAtRuntime)
            {
                StartCoroutine(LoadPropsProgressively());
            }
        }

        private IEnumerator LoadPropsProgressively()
        {
            // Wait a frame to ensure scene is fully loaded
            yield return null;
            
            // Find or create props container
            GameObject propsContainer = GameObject.Find("Props");
            if (propsContainer == null)
            {
                propsContainer = new GameObject("Props");
            }

            // Load tables
            for (int i = 0; i < tablePositions.Length; i++)
            {
                CreateTable(propsContainer.transform, $"Table_{i}", tablePositions[i]);
                yield return new WaitForSeconds(delayBetweenProps);
            }

            // Load screens
            for (int i = 0; i < screenPositions.Length; i++)
            {
                CreateScreen(propsContainer.transform, $"Screen_{i}", screenPositions[i]);
                yield return new WaitForSeconds(delayBetweenProps);
            }
        }

        private void CreateTable(Transform parent, string name, Vector3 position)
        {
            GameObject table;
            
            if (tablePrefab != null)
            {
                table = Instantiate(tablePrefab, parent);
            }
            else
            {
                // Fallback: create primitive
                table = GameObject.CreatePrimitive(PrimitiveType.Cube);
                table.transform.SetParent(parent);
                table.transform.localScale = new Vector3(3f, 0.1f, 1.5f);
                
                var renderer = table.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0.2f, 0.2f, 0.25f);
                    renderer.material = mat;
                }
            }
            
            table.name = name;
            table.transform.position = position;
        }

        private void CreateScreen(Transform parent, string name, Vector3 position)
        {
            GameObject screen;
            
            if (screenPrefab != null)
            {
                screen = Instantiate(screenPrefab, parent);
            }
            else
            {
                // Fallback: create primitive
                screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
                screen.transform.SetParent(parent);
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
            
            screen.name = name;
            screen.transform.position = position;
        }
    }
}

