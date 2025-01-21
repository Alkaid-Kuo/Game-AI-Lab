using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameModeSelector : MonoBehaviour
{
    public GameObject selectorPanel;
    public Button playerButton;
    public Button easyAIButton;
    public Button normalAIButton;
    public Button hardAIButton;

    public event Action<GameMode> OnGameModeSelected;

    private void Start()
    {
        playerButton.onClick.AddListener(() => SelectGameMode(GameMode.TwoPlayers));
        easyAIButton.onClick.AddListener(() => SelectGameMode(GameMode.EasyAI));
        normalAIButton.onClick.AddListener(() => SelectGameMode(GameMode.NormalAI));
        hardAIButton.onClick.AddListener(() => SelectGameMode(GameMode.HardAI));
    }

    public void ShowSelector()
    {
        selectorPanel.SetActive(true);
    }

    private void SelectGameMode(GameMode mode)
    {
        selectorPanel.SetActive(false);
        OnGameModeSelected?.Invoke(mode);
    }
}
