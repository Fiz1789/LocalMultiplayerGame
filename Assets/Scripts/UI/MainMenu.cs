using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RowdyRobots
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private string characterSelectScene = "CharacterSelect";

        private bool m_pressed = false;

        private void Start()
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayMainMenuMusic();
        }

        private void Update()
        {
            if (m_pressed) return;

            if (Gamepad.all.Count > 0)
            {
                foreach (var gamepad in Gamepad.all)
                {
                    if (gamepad.buttonSouth.wasPressedThisFrame ||
                        gamepad.buttonNorth.wasPressedThisFrame ||
                        gamepad.buttonEast.wasPressedThisFrame  ||
                        gamepad.buttonWest.wasPressedThisFrame  ||
                        gamepad.startButton.wasPressedThisFrame)
                    {
                        GoToCharacterSelect();
                        return;
                    }
                }
            }
        }

        private void GoToCharacterSelect()
        {
            m_pressed = true;
            SceneManager.LoadScene(characterSelectScene);
        }
    }
}
