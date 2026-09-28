using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.Rendering;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

namespace RowdyRobots
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager instance { get; private set; }

        // public float currentTime;

        // public float maxTime;

        // public TMP_Text timeUI;

        public TMP_Text countdownUI;

        [Header("Intro")]
        public GameObject introImage;

        // public TMP_Text gameOverUI;

        // public GameObject playerPrefab;

        // public int PlayerNumber = 2;

        public List<PlayerInput> playerList;

        public List<GameObject> spawnPoints;

        // public List<GameObject> playerUIList;


        //   public CinemachineTargetGroup targetGroup;

        // public HorizontalLayoutGroup playerUIPanel;

        // public RectTransform playerUIPrefab;

        private float currentCountdownCounter;
        public int maxCountdownCounter = 3;
        public float introDuration = 2f;
        private float m_introTimer;
        public LevelStates levelState;

        public enum LevelStates
        {
            Intro,
            Countdown,
            Battle,
            // GameOver
        }



        //void Awake()
        //{

        //}



        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            if (GameManager.instance == null)
            {
                SceneManager.LoadScene("MainMenu");
                return;
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.StopMusic();

            levelState = LevelStates.Intro;
            m_introTimer = introDuration;
            if (introImage != null) introImage.SetActive(true);
            countdownUI.gameObject.SetActive(false);
            // currentTime = maxTime;
            currentCountdownCounter = maxCountdownCounter;
            // timeUI.text = maxTime.ToString();
            //   Debug.Log("GamepadNumber" + string.Join("\n", Gamepad.all));

            for (int i = 0; i < GameManager.instance.playerDeviceIds.Count; i++)
            {
                InputDevice device = InputSystem.GetDeviceById(GameManager.instance.playerDeviceIds[i]);
                string scheme = device is Gamepad ? "Gamepad" : "Keyboard&Mouse";

                PlayerInput player = PlayerInput.Instantiate(
                    GameManager.instance.playerCharacterPrefabs[i].gameObject,
                    controlScheme: scheme,
                    pairWithDevice: device
                );

                // PlayerMo playerMovement1 = player.gameObject.GetComponent<PlayerMovement1>();
                playerList.Add(player);
                player.transform.position = spawnPoints[i].transform.position;
                player.transform.rotation = spawnPoints[i].transform.rotation;

                // Freeze movement during countdown
                PlayerWalk pw = player.GetComponent<PlayerWalk>();
                if (pw != null) pw.enabled = false;



                //GameObject ui = Instantiate(playerUIPrefab.gameObject);
                //ui.transform.parent = playerUIPanel.transform;  

                // playerUIList[i].SetActive(true);
                // PlayerUI playerUI = playerUIList[i].GetComponent<PlayerUI>();
                // playerUI.playerImage.sprite = GameManager.instance.playerCharacterSprites[i]; 
                // playerUI.playerText.text = "P"+(i+1).ToString();
                // playerUI.healthComponent = player.gameObject.GetComponent<HealthComponent>();
                // //if(MoistureComponent != null) {
                // playerUI.staminaComponent = player.gameObject.GetComponent<StaminaComponent>();

            }


        }

        IEnumerator CompleteCountdown()
        {
            countdownUI.text = "GO!";
            levelState = LevelStates.Battle;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayBattleMusic();

            foreach (var p in playerList)
            {
                PlayerWalk pw = p.GetComponent<PlayerWalk>();
                if (pw != null) pw.enabled = true;
            }
            yield return new WaitForSeconds(1);
            countdownUI.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (levelState == LevelStates.Intro)
            {
                m_introTimer -= Time.deltaTime;
                if (m_introTimer <= 0f)
                {
                    if (introImage != null) introImage.SetActive(false);
                    countdownUI.gameObject.SetActive(true);
                    levelState = LevelStates.Countdown;
                }
            }
            else if (levelState == LevelStates.Countdown)
            {
                currentCountdownCounter -= Time.deltaTime;
                currentCountdownCounter = Mathf.Max(currentCountdownCounter, 0);
                int countDownRounded = Mathf.CeilToInt(currentCountdownCounter);
                if (countDownRounded > 0)
                {
                    countdownUI.text = countDownRounded.ToString();
                }
                else
                {
                    StartCoroutine(CompleteCountdown());
                }
            }
            // else if (levelState == LevelStates.Battle)
            // {
            //     currentTime -= Time.deltaTime;
            //     currentTime = Mathf.Clamp(currentTime, 0, maxTime);


            //     int timerRounded = Mathf.CeilToInt(currentTime);
            //     timeUI.text = timerRounded.ToString();
            // if (currentTime == 0)
            // {

            //     OnTimeOut();
            // }
            // else
            // {
            //     RemainingPlayersCheck();
            // }
        }



        // }

        // void OnTimeOut()
        // {
        //     StartCoroutine(TimeOutEnumerator());

        // }

        // IEnumerator TimeOutEnumerator()
        // {

        //     levelState = LevelStates.GameOver;
        //     gameOverUI.gameObject.SetActive(true);
        //     gameOverUI.text = "Time Out!";
        //     yield return new WaitForSeconds(2);   
        //     int winnerIndex = -1;
        //     float maxHP = -1;
        //     for (int i = 0; i < playerList.Count; i++)
        //     {
        //         HealthComponent health = playerList[i].GetComponent<HealthComponent>();
        //         if (health.currentHealth > maxHP)
        //         {
        //             winnerIndex = i;
        //             maxHP = health.currentHealth;
        //         }
        //     }
        //     DisplayWinner(winnerIndex);
        //     for (int i = 0; i < playerList.Count; i++)
        //     {
        //         if (i != winnerIndex)
        //         {
        //             PlayerController playerController = playerList[i].gameObject.GetComponent<PlayerController>();
        //             playerController.OnDie();
        //         }
        //     }
        //     yield return new WaitForSeconds(5); 
        //     LoadCharacterSelection();
        // }

        public void LoadCharacterSelection()
        {
            Destroy(GameManager.instance.gameObject);
            SceneManager.LoadScene("CharacterSelection");
        }



        // public void DisplayWinner(int winnerIndex)
        // {
        //     gameOverUI.text = "Player " + (winnerIndex + 1).ToString() + " Wins!";
        // }

        // public void RemainingPlayersCheck()
        // {
        //     if(GameManager.instance.playerDeviceIds.Count > 1)
        //     {
        //         int remainingPlayers = 0;
        //         int lastRemainingPlayerIndex = -1;
        //         for (int i = 0; i < playerList.Count; i++)
        //         {
        //             PlayerState playerState = playerList[i].gameObject.GetComponent<PlayerState>();
        //             if (playerState.currentState != PlayerStateEnum.Dead)
        //             {
        //                 remainingPlayers++;
        //                 lastRemainingPlayerIndex = i;
        //             }
        //             if (remainingPlayers > 1)
        //             {
        //                 return;
        //             }


        //         }
        //         if (remainingPlayers == 1)
        //         {
        //             levelState = LevelStates.GameOver;
        //             DisplayWinner(lastRemainingPlayerIndex);
        //             StartCoroutine(LastRemainingPlayerWinCoroutine());
        //         }
        //     }

        // }

        // IEnumerator LastRemainingPlayerWinCoroutine()
        // {
        //     foreach (Gamepad g in Gamepad.all)
        //     {
        //         g.SetMotorSpeeds(0.0f, 0.0f);
        //     }
        //     yield return new WaitForSeconds(3);
        //     LoadCharacterSelection();   
        // }
    }
}