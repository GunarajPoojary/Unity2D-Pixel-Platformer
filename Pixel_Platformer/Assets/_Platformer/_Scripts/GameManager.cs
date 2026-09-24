using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelPlatformer
{
    public enum GameState { MainMenu, Gameplay, Pause, Win }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private SceneLoader _sceneLoader;
        [SerializeField] private SceneReference _mainMenuScene;
        private GameState _currentState;


        #region Unity API Methods
        private void Start()
        {
            SwitchState(GameState.MainMenu);
        }

        private void OnDestroy()
        {
            GameEvents.Clear();
        }
        #endregion

        private void SwitchState(GameState state)
        {
            _currentState = state;

            switch (state)
            {
                case GameState.MainMenu:
                    EnterMainMenu();
                    break;
            }
        }

        private void EnterMainMenu()
        {
            Debug.Log("Open Main Menu");
            _sceneLoader.LoadScene(_mainMenuScene.BuildIndex, true, true, 1f);
        }

        public void RestartGame()
        {

        }

        public void StartGame()
        {

        }

        public void QuitGame()
        {

        }

        public void GoToMainMenu()
        {

        }

        public void ResumeGame()
        {

        }
    }
}
