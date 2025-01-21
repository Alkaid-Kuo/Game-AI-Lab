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

    private void InitializeWeights() // ��l���v��
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

        // �q�̨β��ʤ��H�����
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

        // ���@�ӪŭȡA�B����ӭȬ۵��B������
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
        // �Ĥ@�B�ϥζ}���Ѯw�A��ֹB��
        if (IsFirstMove())
        {
            return GetFirstMove();
        }


        int bestScore = int.MinValue; // �̨Τ���(�w�]�̤p��)
        List<int> bestMoves = new List<int>();
        //int bestMove = -1; // �̨β���(�w�]�L�Ĳ���)

        // 1.�Ҽ{�������i��
        for (int i = 0; i < 9; i++) // �M���ѽL
        {
            if (game.IsButtonAvailable(i)) // �p�G��m�i��
            {
                // 2.��C�ӥi�����
                game.MakeAIMove(i, "X"); // ���沾�ʡ]���] AI �ϥ� "X"�^
                int score = Minimax(0, false); // �ե� Minimax ��k�����o�Ӳ���(false��ܤU�^�X���쪱�a)
                game.UndoAIMove(i); // �M�P���ʡA��_�ѽL���A

                if (score > bestScore) // ��s�̰�����
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


        // 3.��ܳ��u���i��
        return bestMoves[Random.Range(0,bestMoves.Count)];
    }

    private int Minimax(int depth, bool isMaximizing)
    {
        // �p�G�C������
        if (game.CheckWinCondition())
        {
            return isMaximizing ? -10 : 10;
        }
        else if (game.CheckDrawCondition())
        {
            return 0;
        }

        // �p�G������
        if (isMaximizing) // true��AI�^�X
        {
            int bestScore = int.MinValue; // ��l�Ƴ̨Τ���(�w�]�̤p��)

            for (int i = 0; i < 9; i++)
            {
                if (game.IsButtonAvailable(i))
                {
                    game.MakeAIMove(i, "X"); // ���沾�ʡ]���] AI �ϥ� "X"�^
                    int score = Minimax(depth + 1, false); // �A���ե� Minimax ��k�����o�Ӳ���(false��ܤU�^�X���쪱�a)
                    game.UndoAIMove(i); // �M�P���ʡA��_�ѽL���A
                    bestScore = Math.Max(score, bestScore); // �]�w�̰�����
                }
            }

            return bestScore;
        }
        else // false�����a�^�X
        {
            int bestScore = int.MaxValue; // ��l�Ƴ̨Τ���(�w�]�̤j��)

            for (int i = 0; i < 9; i++)
            {
                if (game.IsButtonAvailable(i))
                {
                    game.MakeAIMove(i, "O"); // ���沾�ʡ]���] ���a �ϥ� "O"�^
                    int score = Minimax(depth + 1, true); // �A���ե� Minimax ��k�����o�Ӳ���(true��ܤU�^�X����AI)
                    game.UndoAIMove(i); // �M�P���ʡA��_�ѽL���A
                    bestScore = Math.Min(score, bestScore); // �]�w�̧C����(��AI�Ө��̧C�����ơA�N�O���a�����u��)
                }
            }

            return bestScore;
        }
    }

    private bool IsFirstMove()
    {
        for (int i = 0; i < 9; i++)
        {
            if (!game.IsButtonAvailable(i)) // �p�G�����������s
            {
                return false;
            }
        }
        return true;
    }

    private int GetFirstMove()
    {
        // �}�������G�u����ܨ����Τ���
        int[] preferredMoves = { 0, 2, 4, 6, 8 }; // ��m
        return preferredMoves[Random.Range(0, preferredMoves.Length)];
    }

    private List<int> GetAvailablePositions() // ���o�i�U��m
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
