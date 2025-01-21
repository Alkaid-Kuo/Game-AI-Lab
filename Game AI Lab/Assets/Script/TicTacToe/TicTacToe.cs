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
    [Header("�]�m")]
    public Button[] buttons = new Button[9];
    private Text[] buttonTexts = new Text[9];
    public Button restart;
    public GameObject currentPlayer;
    public GameObject result;

    [Header("���A�d��")]
    public string currentText;
    public Color currentColor;
    private bool gameEnded = false;

    [Header("�Ҧ����")]
    public GameModeSelector gameModeSelector;
    public GameMode currentGameMode = GameMode.TwoPlayers;

    [Header("AI���")]
    private TicTacToeAI ai;
    public float aiDelay = 1f;
    private bool isPlayerTurn;
    public static readonly int[][] WinningLines = new int[][]
    {
        new int[] {0, 1, 2}, // �Ĥ@��
        new int[] {3, 4, 5}, // �ĤG��
        new int[] {6, 7, 8}, // �ĤT��
        new int[] {0, 3, 6}, // �Ĥ@�C
        new int[] {1, 4, 7}, // �ĤG�C
        new int[] {2, 5, 8}, // �ĤT�C
        new int[] {0, 4, 8}, // �D�﨤�u
        new int[] {2, 4, 6}  // �ƹ﨤�u
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

        // ���C�ӫ��s�K�[�I����ť��
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // �Ыؤ@�ӧ����ܶq�Ӯ����e��i��
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
        Debug.Log($"��e�Ҧ�: {GetGameModeDescription(currentGameMode)}");
        InitializeGame();
    }

    private string GetGameModeDescription(GameMode mode)
    {
        return mode switch
        {
            GameMode.TwoPlayers => "���H�Ҧ�",
            GameMode.EasyAI => "²��q��",
            GameMode.NormalAI => "���q�q��",
            GameMode.HardAI => "�x���q��",
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
            StartCoroutine(AITurnWithDelay()); // �ϥΨ�{�ө��� AI ������
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
            Debug.LogError("AI���ʥX�{���~");
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
            Debug.Log(isPlayerTurn ? "�������" : "�Ť����");
        }
        else if (CheckDrawCondition())
        {
            EndGame("����");
            Debug.Log("����");
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
            return isPlayerTurn ? "�������" : "�Ť����";
        }
        else // ��H�Ҧ�
        {
            return isPlayerTurn ? "���a���" : "�q�����";
        }
    }

    public bool CheckWinCondition()
    {
        // �ˬd�����զX
        foreach (var line in WinningLines)
        {
            if (CheckLine(line[0], line[1], line[2]))
                return true;
        }

        return false;
    }
    public bool CheckDrawCondition()
    {
        // �ˬd�O�_�Ҧ����s���Q�I��
        foreach (Button button in buttons)
        {
            if (button.interactable)
                return false; // �p�G��������s���i�I��,�h���O����
        }
        // �p�G�Ҧ����s���Q�I��,�B�S���ӧQ��,�h������
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