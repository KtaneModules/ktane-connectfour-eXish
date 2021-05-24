using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class ConnectFourScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;

    public KMSelectable[] buttons;
    public Material[] coinMaterials;
    public GameObject coin;

    private Vector3[][] gridPositions = { 
        new Vector3[] { new Vector3(-0.06315f, 0.0174f, 0.0336f), new Vector3(-0.04215f, 0.0174f, 0.0336f), new Vector3(-0.02115f, 0.0174f, 0.0336f), new Vector3(-0.0001f, 0.0174f, 0.0336f), new Vector3(0.021f, 0.0174f, 0.0336f), new Vector3(0.042f, 0.0174f, 0.0336f), new Vector3(0.0631f, 0.0174f, 0.0336f) },
        new Vector3[] { new Vector3(-0.06315f, 0.0174f, 0.0125f), new Vector3(-0.04215f, 0.0174f, 0.0125f), new Vector3(-0.02115f, 0.0174f, 0.0125f), new Vector3(-0.0001f, 0.0174f, 0.0125f), new Vector3(0.021f, 0.0174f, 0.0125f), new Vector3(0.042f, 0.0174f, 0.0125f), new Vector3(0.0631f, 0.0174f, 0.0125f) },
        new Vector3[] { new Vector3(-0.06315f, 0.0174f, -0.0085f), new Vector3(-0.04215f, 0.0174f, -0.0085f), new Vector3(-0.02115f, 0.0174f, -0.0085f), new Vector3(-0.0001f, 0.0174f, -0.0085f), new Vector3(0.021f, 0.0174f, -0.0085f), new Vector3(0.042f, 0.0174f, -0.0085f), new Vector3(0.0631f, 0.0174f, -0.0085f) },
        new Vector3[] { new Vector3(-0.06315f, 0.0174f, -0.0296f), new Vector3(-0.04215f, 0.0174f, -0.0296f), new Vector3(-0.02115f, 0.0174f, -0.0296f), new Vector3(-0.0001f, 0.0174f, -0.0296f), new Vector3(0.021f, 0.0174f, -0.0296f), new Vector3(0.042f, 0.0174f, -0.0296f), new Vector3(0.0631f, 0.0174f, -0.0296f) },
        new Vector3[] { new Vector3(-0.06315f, 0.0174f, -0.0506f), new Vector3(-0.04215f, 0.0174f, -0.0506f), new Vector3(-0.02115f, 0.0174f, -0.0506f), new Vector3(-0.0001f, 0.0174f, -0.0506f), new Vector3(0.021f, 0.0174f, -0.0506f), new Vector3(0.042f, 0.0174f, -0.0506f), new Vector3(0.0631f, 0.0174f, -0.0506f) },
        new Vector3[] { new Vector3(-0.06315f, 0.0174f, -0.0717f), new Vector3(-0.04215f, 0.0174f, -0.0717f), new Vector3(-0.02115f, 0.0174f, -0.0717f), new Vector3(-0.0001f, 0.0174f, -0.0717f), new Vector3(0.021f, 0.0174f, -0.0717f), new Vector3(0.042f, 0.0174f, -0.0717f), new Vector3(0.0631f, 0.0174f, -0.0717f) }
    };
    private Vector3[] startPositions = new Vector3[] { new Vector3(-0.06315f, 0.0174f, 0.05f), new Vector3(-0.04215f, 0.0174f, 0.05f), new Vector3(-0.02115f, 0.0174f, 0.05f), new Vector3(-0.0001f, 0.0174f, 0.05f), new Vector3(0.021f, 0.0174f, 0.05f), new Vector3(0.042f, 0.0174f, 0.05f), new Vector3(0.0631f, 0.0174f, 0.05f) };

    private string[] numNames = new string[] { "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
    private List<int> enemyMoves = new List<int>();
    private List<int> neededMoves = new List<int>();
    private int[] colHeights = { 5, 5, 5, 5, 5, 5, 5 };
    private int stage = 0;
    private bool evenSerial = false;
    private bool animating = false;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
    }

    void Start () {
        evenSerial = bomb.GetSerialNumberNumbers().Last() % 2 == 0;
        redo:
        colHeights = new int[]{ 5, 5, 5, 5, 5, 5, 5 };
        enemyMoves.Clear();
        neededMoves.Clear();
        // 0 = empty, 1 = enemy, 2 = player
        int[,] generatedGrid = new int[6, 7];
        int firstSpot = UnityEngine.Random.Range(0, 7);
        while (!evenSerial && firstSpot == 5)
            firstSpot = UnityEngine.Random.Range(0, 7);
        generatedGrid[5, firstSpot] = 1;
        colHeights[firstSpot]--;
        enemyMoves.Add(firstSpot);
        while (true)
        {
            if (!evenSerial)
            {
                switch (enemyMoves.Last())
                {
                    case 0:
                        neededMoves.Add(neededMoves.Count() % 7);
                        break;
                    case 1:
                        neededMoves.Add(bomb.GetSerialNumberNumbers().First() % 7);
                        break;
                    case 2:
                        neededMoves.Add(bomb.GetBatteryCount(Battery.AA) % 7);
                        break;
                    case 3:
                        neededMoves.Add(bomb.GetSerialNumberNumbers().Count());
                        break;
                    case 4:
                        neededMoves.Add(numNames[bomb.GetSerialNumberNumbers().First()].Length);
                        break;
                    case 5:
                        neededMoves.Add(neededMoves.Last());
                        break;
                    case 6:
                        neededMoves.Add((neededMoves.Count() + enemyMoves.Count()) % 7);
                        break;
                }
            }
            else
            {
                switch (enemyMoves.Last())
                {
                    case 0:
                        neededMoves.Add(enemyMoves.Count() % 7);
                        break;
                    case 1:
                        neededMoves.Add(bomb.GetSerialNumberNumbers().ToList()[1] % 7);
                        break;
                    case 2:
                        neededMoves.Add(bomb.GetBatteryCount(Battery.D) % 7);
                        break;
                    case 3:
                        neededMoves.Add(bomb.GetSerialNumberLetters().Count());
                        break;
                    case 4:
                        neededMoves.Add(numNames[bomb.GetSerialNumberNumbers().ToList()[1]].Length);
                        break;
                    case 5:
                        neededMoves.Add(enemyMoves.Last());
                        break;
                    case 6:
                        neededMoves.Add((42 - (neededMoves.Count() + enemyMoves.Count())) % 7);
                        break;
                }
            }
            bool success = false;
            for (int i = 5; i >= 0; i--)
            {
                if (generatedGrid[i, neededMoves.Last()] == 0)
                {
                    generatedGrid[i, neededMoves.Last()] = 2;
                    success = true;
                    break;
                }
            }
            if (!success)
                goto redo;
            else if (CheckFourConnected(generatedGrid, 2))
                break;

            int newSpot = UnityEngine.Random.Range(0, 7);
            enemyMoves.Add(newSpot);
            success = false;
            for (int i = 5; i >= 0; i--)
            {
                if (generatedGrid[i, enemyMoves.Last()] == 0)
                {
                    generatedGrid[i, enemyMoves.Last()] = 1;
                    success = true;
                    break;
                }
            }
            if (!success || CheckFourConnected(generatedGrid, 1))
                goto redo;
        }
        GameObject first = Instantiate(coin, gridPositions[5][firstSpot], coin.transform.rotation);
        first.transform.parent = transform;
        first.transform.localScale = coin.transform.localScale;
        first.transform.eulerAngles = new Vector3(0, 0, 0);
        first.transform.localPosition = gridPositions[5][firstSpot];
        first.GetComponent<MeshRenderer>().material = coinMaterials[0];
        Debug.LogFormat("[Connect Four #{0}] Initial Board:", moduleId);
        for (int i = 0; i < 5; i++)
            Debug.LogFormat("[Connect Four #{0}] O O O O O O O", moduleId);
        List<string> initRow = new List<string>();
        for (int i = 0; i < 7; i++)
        {
            if (i == firstSpot)
                initRow.Add("R");
            else
                initRow.Add("O");
        }
        Debug.LogFormat("[Connect Four #{0}] {1}", moduleId, initRow.Join(" "));
        Debug.LogFormat("[Connect Four #{0}] Final Board:", moduleId);
        for (int i = 0; i < 6; i++)
        {
            List<string> finalRow = new List<string>();
            for (int j = 0; j < 7; j++)
            {
                if (generatedGrid[i, j] == 1)
                    finalRow.Add("R");
                else if (generatedGrid[i, j] == 2)
                    finalRow.Add("B");
                else
                    finalRow.Add("O");
            }
            Debug.LogFormat("[Connect Four #{0}] {1}", moduleId, finalRow.Join(" "));
        }
        for (int i = 0; i < neededMoves.Count; i++)
        {
            if (i != 0)
            {
                Debug.LogFormat("[Connect Four #{0}] The enemy will place a chip in column {1}", moduleId, enemyMoves[i]);
            }
            Debug.LogFormat("[Connect Four #{0}] You must place a chip in column {1}", moduleId, neededMoves[i]);
        }
    }

    void PressButton(KMSelectable pressed)
    {
        if (moduleSolved != true && animating != true)
        {
            if (neededMoves[stage] == Array.IndexOf(buttons, pressed) && stage == (neededMoves.Count - 1))
            {
                Debug.LogFormat("[Connect Four #{0}] You placed a chip in column {1}, which is correct. Module solved!", moduleId, Array.IndexOf(buttons, pressed));
                moduleSolved = true;
                StartCoroutine(AnimateDrop(Array.IndexOf(buttons, pressed), 2, true));
            }
            else if (neededMoves[stage] == Array.IndexOf(buttons, pressed) && stage != (neededMoves.Count - 1))
            {
                Debug.LogFormat("[Connect Four #{0}] You placed a chip in column {1}, which is correct.", moduleId, Array.IndexOf(buttons, pressed));
                stage++;
                StartCoroutine(AnimateDrop(Array.IndexOf(buttons, pressed), 2, false));
            }
            else
            {
                Debug.LogFormat("[Connect Four #{0}] You placed a chip in column {1}, which is incorrect (expected {2}). Strike!", moduleId, Array.IndexOf(buttons, pressed), neededMoves[stage]);
                GetComponent<KMBombModule>().HandleStrike();
            }
        }
    }

    bool CheckFourConnected(int[,] grid, int player)
    {
        // Horizontal Check 
        for (int j = 0; j < 4; j++)
        {
            for (int i = 0; i < 6; i++)
            {
                if (grid[i, j] == player && grid[i, j + 1] == player && grid[i, j + 2] == player && grid[i, j + 3] == player)
                {
                    return true;
                }
            }
        }
        // Vertical Check
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (grid[i, j] == player && grid[i + 1, j] == player && grid[i + 2, j] == player && grid[i + 3, j] == player)
                {
                    return true;
                }
            }
        }
        // Ascending Diagonal Check 
        for (int i = 3; i < 6; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                if (grid[i, j] == player && grid[i - 1, j + 1] == player && grid[i - 2, j + 2] == player && grid[i - 3, j + 3] == player)
                    return true;
            }
        }
        // Descending Diagonal Check
        for (int i = 3; i < 6; i++)
        {
            for (int j = 3; j < 7; j++)
            {
                if (grid[i, j] == player && grid[i - 1, j - 1] == player && grid[i - 2, j - 2] == player && grid[i - 3, j - 3] == player)
                    return true;
            }
        }
        return false;
    }

    bool CoinInPosition(GameObject coin, Vector3 target)
    {
        if (!(Vector3.Distance(coin.transform.localPosition, target) < 0.001f))
        {
            return false;
        }
        return true;
    }

    IEnumerator AnimateDrop(int col, int player, bool solve)
    {
        animating = true;
        audio.PlaySoundAtTransform("drop" + UnityEngine.Random.Range(1, 4), transform);
        GameObject newCoin = Instantiate(coin, startPositions[col], coin.transform.rotation);
        newCoin.transform.parent = transform;
        newCoin.transform.localScale = coin.transform.localScale;
        newCoin.transform.localRotation = coin.transform.rotation;
        newCoin.transform.localPosition = startPositions[col];
        newCoin.GetComponent<MeshRenderer>().material = coinMaterials[player - 1];
        float t = 0f;
        while (!CoinInPosition(newCoin, gridPositions[colHeights[col]][col]))
        {
            newCoin.transform.localPosition = Vector3.Lerp(startPositions[col], gridPositions[colHeights[col]][col], t);
            t += Time.deltaTime * 3f;
            yield return null;
        }
        colHeights[col]--;
        if (solve)
        {
            GetComponent<KMBombModule>().HandlePass();
        }
        else if (player == 2)
        {
            StartCoroutine(AnimateDrop(enemyMoves[stage], 1, false));
            yield break;
        }
        animating = false;
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} column/col <#> [Places a chip in column '#']";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (animating)
        {
            yield return null;
            yield return "sendtochaterror Cannot place a chip while the module is animating!";
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*column\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(parameters[0], @"^\s*col\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                int temp;
                if (int.TryParse(parameters[1], out temp))
                {
                    if (temp >= 0 && temp <= 6)
                    {
                        buttons[temp].OnInteract();
                        if (neededMoves.Last() == temp && stage == (neededMoves.Count - 1))
                        {
                            yield return "solve";
                        }
                    }
                    else
                    {
                        yield return "sendtochaterror!f The specified column '" + parameters[1] + "' is out of range 0-6!";
                    }
                }
                else
                {
                    yield return "sendtochaterror!f The specified column '" + parameters[1] + "' is invalid!";
                }
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify a column to place a chip in!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (!moduleSolved)
        {
            while (animating) yield return true;
            int start = stage;
            for (int i = start; i < neededMoves.Count; i++)
            {
                buttons[neededMoves[stage]].OnInteract();
                while (animating) yield return true;
            }
        }
        while (animating) yield return true;
    }
}
