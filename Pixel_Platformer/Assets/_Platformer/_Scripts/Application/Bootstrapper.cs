using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public class Bootstrapper : MonoBehaviour
    {
        [SerializeField] private SceneReference _managerScene;
        private AsyncOperation op;
        private void Start()
        {
            // Load the PersisteneManager Scene which contains GameManager which handles flow of the game
            op = SceneManager.LoadSceneAsync(_managerScene.BuildIndex, LoadSceneMode.Additive);
            op.completed += Unload;
        }

        private void OnDestroy()
        {
            op.completed -= Unload;
        }

        private void Unload(AsyncOperation operation)
        {
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
        }
    }
}