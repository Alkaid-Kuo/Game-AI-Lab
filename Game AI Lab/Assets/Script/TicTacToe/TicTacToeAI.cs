using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class TicTacToeAI
{
    private TicTacToe game;
    private int[] baseWeights;
    public int[] currentWeights;
    private const int Max_Depth = 6;

    public TicTacToeAI(TicTacToe game)
    {
        this.game = game;
        InitializeWeights();
    }

    private void InitializeWeights() // 初始化權重
    {
        baseWeights = new int[] {
            2, 1, 2,
            1, 3, 1,
            2, 1, 2
        };

        currentWeights = (int[])baseWeights.Clone();
    }

    public int EasyAI()
    {
        List<int> availableNums = GetAvailablePositions();

        if (availableNums.Count > 0)
        {
            return availableNums[Random.Range(0, availableNums.Count)];
        }

        return -1;
    }

    public int NormalAI()
    {
        List<int> availableNums = GetAvailablePositions();
        if (availableNums.Count == 0) return -1;

        currentWeights = (int[])baseWeights.Clone();
        AdjustWeights();

        int highestWeight = currentWeights.Max();
        List<int> bestMoves = new List<int>();

        foreach (int pos in availableNums)
        {
            if (currentWeights[pos] == highestWeight)
            {
                bestMoves.Add(pos);
            }
        }

        // 從最佳移動中隨機選擇
        return bestMoves[Random.Range(0, bestMoves.Count)];
    }

    private void AdjustWeights()
    {
        for (int i = 0; i < 9; i++)
        {
            if (game.GetButtonText(i) != "")
            {
                currentWeights[i] = 0;
            }
        }

        foreach (var line in TicTacToe.WinningLines)
        {
            CheckWinOrBlock(line[0], line[1], line[2]);
        }
    }

    private void CheckWinOrBlock(int a, int b, int c)
    {
        string[] texts = { game.GetButtonText(a), game.GetButtonText(b), game.GetButtonText(c) };

        // 有一個空值，且有兩個值相等且不為空
        if (texts.Contains("") &&
            (texts.Count(t => t == "X") == 2 || texts.Count(t => t == "O") == 2))
        {
            int emptyIndex = texts[0] == "" ? a : (texts[1] == "" ? b : c);
            currentWeights[emptyIndex] = texts.Count(t => t == "X") == 2 ? 4 : 3;
        }
        /*
        if (game.GetButtonText(a) != "" && game.GetButtonText(a) == game.GetButtonText(b) && game.GetButtonText(c) == "")
        {
            currentWeights[c] = game.GetButtonText(a) == "X" ? 4 : 3;
        }
        else if (game.GetButtonText(b) != "" && game.GetButtonText(b) == game.GetButtonText(c) && game.GetButtonText(a) == "")
        {
            currentWeights[a] = game.GetButtonText(b) == "X" ? 4 : 3;
        }
        else if (game.GetButtonText(a) != "" && game.GetButtonText(a) == game.GetButtonText(c) && game.GetButtonText(b) == "")
        {
            currentWeights[b] = game.GetButtonText(a) == "X" ? 4 : 3;
        }
        */
    }

    public int HardAI() // Minimax
    {
        // 第一步使用開局棋庫，減少運算
        if (IsFirstMove())
        {
            return GetFirstMove();
        }


        int bestScore = int.MinValue; // 最佳分數(預設最小值)
        List<int> bestMoves = new List<int>();
        //int bestMove = -1; // 最佳移動(預設無效移動)

        // 1.考慮全部的可能
        for (int i = 0; i < 9; i++) // 遍歷棋盤
        {
            if (game.IsButtonAvailable(i)) // 如果位置可用
            {
                // 2.對每個可能評分
                game.MakeAIMove(i, "X"); // 執行移動（假設 AI 使用 "X"）
                int score = Minimax(0, false); // 調用 Minimax 方法評估這個移動(false表示下回合輪到玩家)
                game.UndoAIMove(i); // 撤銷移動，恢復棋盤狀態

                if (score > bestScore) // 更新最高分數
                {
                    bestScore = score;
                    bestMoves.Clear();
                    bestMoves.Add(i);
                }
                else if (score == bestScore)
                {
                    bestMoves.Add(i);
                }
            }
        }

        //foreach (int move in bestMoves)
        //{
        //    Debug.Log(move);
        //}


        // 3.選擇最優的可能
        return bestMoves[Random.Range(0,bestMoves.Count)];
    }

    private int Minimax(int depth, bool isMaximizing)
    {
        // 如果遊戲結束
        if (game.CheckWinCondition())
        {
            return isMaximizing ? -10 : 10;
        }
        else if (game.CheckDrawCondition())
        {
            return 0;
        }

        // 如果未結束
        if (isMaximizing) // true為AI回合
        {
            int bestScore = int.MinValue; // 初始化最佳分數(預設最小值)

            for (int i = 0; i < 9; i++)
            {
                if (game.IsButtonAvailable(i))
                {
                    game.MakeAIMove(i, "X"); // 執行移動（假設 AI 使用 "X"）
                    int score = Minimax(depth + 1, false); // 再次調用 Minimax 方法評估這個移動(false表示下回合輪到玩家)
                    game.UndoAIMove(i); // 撤銷移動，恢復棋盤狀態
                    bestScore = Math.Max(score, bestScore); // 設定最高分數
                }
            }

            return bestScore;
        }
        else // false為玩家回合
        {
            int bestScore = int.MaxValue; // 初始化最佳分數(預設最大值)

            for (int i = 0; i < 9; i++)
            {
                if (game.IsButtonAvailable(i))
                {
                    game.MakeAIMove(i, "O"); // 執行移動（假設 玩家 使用 "O"）
                    int score = Minimax(depth + 1, true); // 再次調用 Minimax 方法評估這個移動(true表示下回合輪到AI)
                    game.UndoAIMove(i); // 撤銷移動，恢復棋盤狀態
                    bestScore = Math.Min(score, bestScore); // 設定最低分數(對AI而言最低的分數，就是玩家的最優解)
                }
            }

            return bestScore;
        }
    }

    private bool IsFirstMove()
    {
        for (int i = 0; i < 9; i++)
        {
            if (!game.IsButtonAvailable(i)) // 如果有關閉的按鈕
            {
                return false;
            }
        }
        return true;
    }

    private int GetFirstMove()
    {
        // 開局策略：優先選擇角落或中心
        int[] preferredMoves = { 0, 2, 4, 6, 8 }; // 位置
        return preferredMoves[Random.Range(0, preferredMoves.Length)];
    }

    private List<int> GetAvailablePositions() // 取得可下位置
    {
        List<int> availablePositions = new List<int>();
        for (int i = 0; i < 9; i++)
        {
            if (game.IsButtonAvailable(i))
            {
                availablePositions.Add(i);
            }
        }
        return availablePositions;
    }
}
