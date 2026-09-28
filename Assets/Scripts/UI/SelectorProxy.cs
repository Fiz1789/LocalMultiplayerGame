using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RowdyRobots
{
public class SelectorProxy : MonoBehaviour
{

    [HideInInspector]
    public PlayerSelector playerSelector;
    public int selectorId;
    bool m_canSubmit = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnNavigate(InputValue value)
    {
        if (playerSelector != null)
        {
            playerSelector.OnChooseCharacter(value);
        }
    }

    public void OnStartGame(InputValue value)
    {
        if (value.isPressed)
        {
            playerSelector.OnStartGame(value);
        }  
    }


    public void OnSubmit(InputValue value)
    {
        if (!m_canSubmit) return;

        if (value.isPressed)
        {
            playerSelector.OnConfirm(value);
        }
    }

    public void OnCancel(InputValue value)
    {
        if (value.isPressed)

        {
            playerSelector.OnCancel(value);
        }

    }

   public void SetSelectorId(int _selectorId)
    {
        selectorId = _selectorId;
        playerSelector.selectorId = _selectorId;
        playerSelector.SetPlayerInput(GetComponent<PlayerInput>());
        StartCoroutine(EnableSubmitNextFrame());
    }

    IEnumerator EnableSubmitNextFrame()
    {
        m_canSubmit = false;
        yield return null;
        m_canSubmit = true;
    }
}
}