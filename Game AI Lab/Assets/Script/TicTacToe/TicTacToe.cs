using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum GameMode
{
    TwoPlayers,
    EasyAI,
    NormalAI,
    HardAI
}
public class TicTacToe : MonoBehaviour
{
    [Header("設置")]
    public Button[] buttons = new Button[9];
    private Text[] buttonTexts = new Text[9];
    public Button restart;
    public GameObject currentPlayer;
    public GameObject result;

    [Header("狀態查看")]
    public string currentText;
    public Color currentColor;
    private bool gameEnded = false;

    [Header("模式選單")]
    public GameModeSelector gameModeSelector;
    public GameMode currentGameMode = GameMode.TwoPlayers;

    [Header("AI對手")]
    private TicTacToeAI ai;
    public float aiDelay = 1f;
    private bool isPlayerTurn;
    public static readonly int[][] WinningLines = new int[][]
    {
        new int[] {0, 1, 2}, // 第一行
        new int[] {3, 4, 5}, // 第二行
        new int[] {6, 7, 8}, // 第三行
        new int[] {0, 3, 6}, // 第一列
        new int[] {1, 4, 7}, // 第二列
        new int[] {2, 5, 8}, // 第三列
        new int[] {0, 4, 8}, // 主對角線
        new int[] {2, 4, 6}  // 副對角線
    };

    private void Start()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttonTexts[i] = buttons[i].GetComponentInChildren<Text>();
        }

        gameModeSelector.OnGameModeSelected += OnGameModeSelected;

        ai = new TicTacToeAI(this);

        result.SetActive(false);
        gameModeSelector.ShowSelector();
    }

    private void InitializeGame()
    {
        gameEnded = false;

        isPlayerTurn = Random.value > 0.5f;
        currentText = isPlayerTurn ? "O" : "X";
        currentColor = isPlayerTurn ? Color.red : Color.blue;

        UpdatePlayerUI();

        // 為每個按鈕添加點擊監聽器
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // 創建一個局部變量來捕獲當前的i值
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
            buttons[i].interactable = true;
            buttonTexts[i].text = "";
        }

        if (!isPlayerTurn && currentGameMode != GameMode.TwoPlayers)
        {
            StartCoroutine(AITurnWithDelay());
        }

        restart.onClick.AddListener(() => RestartGame());
    }

    private void RestartGame()
    {
        gameModeSelector.ShowSelector();
        result.SetActive(false);
    }

    private void OnGameModeSelected(GameMode mode)
    {
        currentGameMode = mode;
        Debug.Log($"當前模式: {GetGameModeDescription(currentGameMode)}");
        InitializeGame();
    }

    private string GetGameModeDescription(GameMode mode)
    {
        return mode switch
        {
            GameMode.TwoPlayers => "雙人模式",
            GameMode.EasyAI => "簡單電腦",
            GameMode.NormalAI => "普通電腦",
            GameMode.HardAI => "困難電腦",
            _ => "???",
        };
    }

    void OnButtonClick(int buttonIndex)
    {
        if (gameEnded || (currentGameMode != GameMode.TwoPlayers && !isPlayerTurn)) return;

        MakeMove(buttonIndex);

        if (!gameEnded && currentGameMode != GameMode.TwoPlayers)
        {
            isPlayerTurn = false;
            StartCoroutine(AITurnWithDelay()); // 使用協程來延遲 AI 的移動
        }
    }

    IEnumerator AITurnWithDelay()
    {
        yield return new WaitForSeconds(aiDelay);

        int aiMove = currentGameMode switch
        {
            GameMode.EasyAI => ai.EasyAI(),
            GameMode.NormalAI => ai.NormalAI(),
            GameMode.HardAI => ai.HardAI(),
            _ => -1,
        };

        if (aiMove != -1)
        {
            MakeMove(aiMove);
            isPlayerTurn = true;
        }
        else
        {
            Debug.LogError("AI移動出現錯誤");
        }
    }

    IEnumerator AITurnWithDelay100T()
    {
        OnGameModeSelected(GameMode.HardAI);
        int i = 0;

        while (i<100)
        {
            yield return new WaitForSeconds(aiDelay);

            int aiMove = currentGameMode switch
            {
                GameMode.EasyAI => ai.EasyAI(),
                GameMode.NormalAI => ai.NormalAI(),
                GameMode.HardAI => ai.HardAI(),
                _ => -1,
            };

            if (aiMove != -1)
            {
                MakeMove(aiMove);
                //isPlayerTurn = true;
            }

            if (gameEnded)
            {
                RestartGame();
                OnGameModeSelected(GameMode.HardAI);
                i++;
                Debug.Log("End");
            }
        }
    }

    public void MakeAIMove(int buttonIndex, string playerSymbol)
    {
        buttonTexts[buttonIndex].text = playerSymbol;
        buttonTexts[buttonIndex].color = playerSymbol == "O" ? Color.red : Color.blue;
        buttons[buttonIndex].interactable = false;
    }

    public void UndoAIMove(int buttonIndex)
    {
        buttonTexts[buttonIndex].text = "";
        buttons[buttonIndex].interactable = true;
    }

    public void MakeMove(int buttonIndex)
    {
        buttonTexts[buttonIndex].text = currentText;
        buttonTexts[buttonIndex].color = currentColor;
        buttons[buttonIndex].interactable = false;

        if (CheckWinCondition())
        {
            EndGame(GetWinnerText());
            Debug.Log(isPlayerTurn ? "紅方獲勝" : "藍方獲勝");
        }
        else if (CheckDrawCondition())
        {
            EndGame("平局");
            Debug.Log("平局");
        }
        else
        {
            SwitchPlayer();
        }
    }

    private void EndGame(string resultText)
    {
        gameEnded = true;
        result.GetComponentInChildren<Text>().text = resultText;
        result.SetActive(true);
    }
    private string GetWinnerText()
    {
        if (currentGameMode == GameMode.TwoPlayers)
        {
            return isPlayerTurn ? "紅方獲勝" : "藍方獲勝";
        }
        else // 單人模式
        {
            return isPlayerTurn ? "玩家獲勝" : "電腦獲勝";
        }
    }

    public bool CheckWinCondition()
    {
        // 檢查全部組合
        foreach (var line in WinningLines)
        {
            if (CheckLine(line[0], line[1], line[2]))
                return true;
        }

        return false;
    }
    public bool CheckDrawCondition()
    {
        // 檢查是否所有按鈕都被點擊
        foreach (Button button in buttons)
        {
            if (button.interactable)
                return false; // 如果有任何按鈕仍可點擊,則不是平局
        }
        // 如果所有按鈕都被點擊,且沒有勝利者,則為平局
        return !CheckWinCondition();
    }

    private bool CheckLine(int a, int b, int c)
    {
        return GetButtonText(a) != "" &&
           GetButtonText(a) == GetButtonText(b) &&
           GetButtonText(b) == GetButtonText(c);
    }

    private void SwitchPlayer()
    {
        isPlayerTurn = !isPlayerTurn;
        currentText = isPlayerTurn ? "O" : "X";
        currentColor = isPlayerTurn ? Color.red : Color.blue;

        UpdatePlayerUI();
    }

    public bool IsButtonAvailable(int index)
    {
        return buttons[index].interactable;
    }

    private void UpdatePlayerUI()
    {
        currentPlayer.GetComponentInChildren<Text>().text = currentText;
        currentPlayer.GetComponentInChildren<Text>().color = currentColor;
    }

    public string GetButtonText(int index)
    {
        return buttonTexts[index].text;
    }
}