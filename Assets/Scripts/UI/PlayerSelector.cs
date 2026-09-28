using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RowdyRobots
{
    public class PlayerSelector : MonoBehaviour
    {
        public Image characterImage;
        public TMP_Text confirmUI;
        public TMP_Text playerText;

        [Tooltip("Semi-transparent grey Image sitting on top of characterImage — assign in Inspector")]
        public Image takenOverlay;


        int selectedCharacterIndex;
        public CharacterSelectionManager selectionManager;
        bool isOnSelectDelay;
        public int selectorId;
        private PlayerInput m_playerInput;

        private bool isConfirmed = false;

        void Awake() { }

        public void SetPlayerInput(PlayerInput playerInput)
        {
            m_playerInput = playerInput;
        }

        private bool IsGamepad()
        {
            return m_playerInput != null && m_playerInput.currentControlScheme == "Gamepad";
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (takenOverlay != null)
                takenOverlay.gameObject.SetActive(!isConfirmed && IsIndexTakenByOther(selectedCharacterIndex));
        }


        public void OnChooseCharacter(InputValue value)
        {
            if (!IsGamepad()) return;
            if (!isOnSelectDelay && isConfirmed == false)
                StartCoroutine(CharacterSelectWait(value.Get<Vector2>()));
        }

        IEnumerator CharacterSelectWait(Vector2 inputDirection)
        {
            isOnSelectDelay = true;
            Debug.Log("Fire" + inputDirection);
            int oldIndex = selectedCharacterIndex;
            int dir = inputDirection.x < 0 ? -1 : (inputDirection.x > 0 ? 1 : 0);
            int count = selectionManager.playerImages.Count;
            if (dir != 0)
            {
                selectedCharacterIndex = (selectedCharacterIndex + dir + count) % count;
            }

            characterImage.sprite = selectionManager.playerImages[selectedCharacterIndex];
            GameManager.instance.playerCharacterPrefabs[selectorId] = selectionManager.playerPrefabs[selectedCharacterIndex];
            if (selectionManager.playerOuterColors.Count > selectedCharacterIndex)
                GameManager.instance.playerCharacterOuterColors[selectorId] = selectionManager.playerOuterColors[selectedCharacterIndex];
            if (selectionManager.playerInnerColors.Count > selectedCharacterIndex)
                GameManager.instance.playerCharacterInnerColors[selectorId] = selectionManager.playerInnerColors[selectedCharacterIndex];
            yield return new WaitForSeconds(0.2f);
            isOnSelectDelay = false;
        }

        public void OnConfirm(InputValue value)
        {
            if (!IsGamepad()) return;
            if (isConfirmed == false && !IsIndexTakenByOther(selectedCharacterIndex))
            {
                confirmUI.gameObject.SetActive(true);
                selectionManager.isPlayerChoiceConfirmedList[selectorId] = true;
                isConfirmed = true;
                selectionManager.CheckPlayerSelection();

            }
        }

        public void OnCancel(InputValue value)
        {
            if (!IsGamepad()) return;
            if (isConfirmed == true)
            {
                confirmUI.gameObject.SetActive(false);
                selectionManager.isPlayerChoiceConfirmedList[selectorId] = false;
                isConfirmed = false;
                selectionManager.CheckPlayerSelection();
            }
        }

        public void OnStartGame(InputValue value)
        {
            if (!IsGamepad()) return;
            selectionManager.StartGame();
        }

        // Called by CharacterSelectionManager when a player joins
        public void SetStartingIndex(int index)
        {
            selectedCharacterIndex = index;
        }

        public int GetSelectedIndex() => selectedCharacterIndex;
        public bool IsConfirmed() => isConfirmed;

        private bool IsIndexTakenByOther(int index)
        {
            for (int i = 0; i < selectionManager.characterSelectionList.Count; i++)
            {
                if (i == selectorId) continue;
                var other = selectionManager.characterSelectionList[i];
                if (other != null && other.isActiveAndEnabled && other.IsConfirmed() && other.GetSelectedIndex() == index)
                    return true;
            }
            return false;
        }
    }
}