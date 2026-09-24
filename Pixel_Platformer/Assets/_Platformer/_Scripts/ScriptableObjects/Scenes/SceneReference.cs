using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SceneReference", menuName = "Scene Reference")]
public class SceneReference : ScriptableObject
{
    [HideInInspector] public string scenePath; // OnValidate only works on public and [SerializeField] private fields
                                               // for private without [SerializeField] will be set to empty strings

    public string ScenePath
    {
        get
        {
            if (string.IsNullOrEmpty(ScenePath))
            {
                Debug.LogError($"{name}: No Scene Asset assigned.", this);
            }

            return scenePath;
        }
    }

    public int BuildIndex
    {
        get
        {
            return UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(scenePath);
        }
    }

#if UNITY_EDITOR
    [SerializeField] private SceneAsset _sceneAsset;

    private void OnValidate()
    {
        if (_sceneAsset == null)
        {
            scenePath = string.Empty;
            Debug.LogWarning($"{name}: No Scene Asset assigned.", this);
            return;
        }

        scenePath = AssetDatabase.GetAssetPath(_sceneAsset);
    }
#endif
}