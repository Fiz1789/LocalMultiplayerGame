using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RowdyRobots
{
    public class CharacterSelectionManager : MonoBehaviour
    {
        public List<PlayerSelector> characterSelectionList;
        public int currentPlayerIndex;

        public List<PlayerInput> playerPrefabs;

        public List<Sprite> playerImages;

        public List<Color> playerOuterColors;
        public List<Color> playerInnerColors;


        public List<bool> isPlayerChoiceConfirmedList;


        private PlayerInputManager playerInputManager;

        public TMP_Text pressStartText;
        bool isAllPlayerConfirmed;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            playerInputManager = GetComponent<PlayerInputManager>();

            // Clear previous game's player data so a fresh selection starts clean
            if (GameManager.instance != null)
            {
                GameManager.instance.playerCharacterPrefabs.Clear();
                GameManager.instance.playerDeviceIds.Clear();
                GameManager.instance.playerCharacterOuterColors.Clear();
                GameManager.instance.playerCharacterInnerColors.Clear();
            }

            currentPlayerIndex = 0;
            isPlayerChoiceConfirmedList.Clear();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPlayerJoined(PlayerInput input)
        {
            var device = input.devices[0];
            Debug.Log("PlayerJoined" + input.gameObject.name);

            int startIndex = GetFirstFreeCharacterIndex();

            SelectorProxy proxy = input.gameObject.GetComponent<SelectorProxy>();
            if (proxy != null)
            {
                proxy.playerSelector = characterSelectionList[currentPlayerIndex];
                proxy.playerSelector.gameObject.SetActive(true);
                proxy.SetSelectorId(currentPlayerIndex);

                PlayerSelector selector = characterSelectionList[currentPlayerIndex].gameObject.GetComponent<PlayerSelector>();
                selector.SetStartingIndex(startIndex);
                selector.characterImage.sprite = playerImages[startIndex];
                isPlayerChoiceConfirmedList.Add(false);
            }
            CheckPlayerSelection();

            GameManager.instance.playerCharacterPrefabs.Add(playerPrefabs[startIndex]);
            GameManager.instance.playerDeviceIds.Add(input.devices[0].deviceId);
            GameManager.instance.playerCharacterOuterColors.Add(playerOuterColors.Count > startIndex ? playerOuterColors[startIndex] : Color.white);
            GameManager.instance.playerCharacterInnerColors.Add(playerInnerColors.Count > startIndex ? playerInnerColors[startIndex] : Color.grey);
            currentPlayerIndex += 1;
        }

        public void CheckPlayerSelection()
        {
            for (int i = 0; i < isPlayerChoiceConfirmedList.Count; i++)
            {
                if (isPlayerChoiceConfirmedList[i] == false)
                {
                    isAllPlayerConfirmed = false;

                    pressStartText.gameObject.SetActive(false);

                    return;
                }

            }
            isAllPlayerConfirmed = true;
            pressStartText.gameObject.SetActive(true);
        }

        private int GetFirstFreeCharacterIndex()
        {
            for (int i = 0; i < playerImages.Count; i++)
            {
                bool taken = false;
                for (int p = 0; p < currentPlayerIndex; p++)
                {
                    var sel = characterSelectionList[p];
                    if (sel != null && sel.isActiveAndEnabled && sel.GetSelectedIndex() == i)
                    {
                        taken = true;
                        break;
                    }
                }
                if (!taken) return i;
            }
            return 0;
        }

        public void StartGame()
        {
            // Only allow start if all confirmed AND at least 2 players
            int confirmedCount = 0;
            foreach (bool confirmed in isPlayerChoiceConfirmedList)
                if (confirmed) confirmedCount++;

            if (GameManager.instance.gameState == GameManager.GameState.CharacterSelection && isAllPlayerConfirmed && confirmedCount >= 2)
            {
                GameManager.instance.gameState = GameManager.GameState.Battle;
                SceneManager.LoadScene("GAME");
            }
        }

    }
}


