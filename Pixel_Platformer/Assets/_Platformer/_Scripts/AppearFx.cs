using System;
using UnityEngine;

namespace PixelPlatformer
{
    public class AppearFX : MonoBehaviour
    {
        private Action onSpawn;

        public void SpawnCharacter()
        {
            onSpawn?.Invoke();
            // Debug.Break();
        }

        public void OnComplete()
        {
            gameObject.SetActive(false);
        }

        public void AddListener(Action action)
        {
            onSpawn += action;
        }
        
        public void RemoveListener(Action action)
        {
            onSpawn -= action;
        }
    }
}