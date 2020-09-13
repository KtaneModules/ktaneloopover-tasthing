using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using rnd = UnityEngine.Random;

public class loopover : MonoBehaviour
{
    public new KMAudio audio;
    public KMBombInfo bomb;
    public KMBombModule module;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    public Renderer[] tiles;
    public Material[] tileColors;
    public string[] letters;
    public TextMesh[] tileLetters;
    private static readonly string[] solveState = new string[25] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y" };
    private string[] currentState;
    public KMSelectable[] negvertibuttons;
    public KMSelectable[] posvertibuttons;
    public KMSelectable[] poshoributtons;
    public KMSelectable[] neghoributtons;

    sealed class Shift
    {
        public string[] NextState;
        public string[] PreviousState;
        public bool Row;
        public int Index;
        public int Direction;
    }

    private readonly Queue<Shift> _animationQueue = new Queue<Shift>();
    private List<Shift> allMoves = new List<Shift>();

    KMSelectable.OnInteractHandler buttonHandler(bool row, int index, int direction, KMSelectable arrow, Action<KMSelectable, string[]> method)
    {
        return delegate ()
        {
            if (!moduleSolved)
            {
                audio.PlaySoundAtTransform("tick", arrow.transform);
                var prevState = currentState.ToArray();
                method(arrow, currentState);
                var currentShift = new Shift { Row = row, Index = index, Direction = direction, NextState = currentState.ToArray(), PreviousState = prevState };
                _animationQueue.Enqueue(currentShift);
                allMoves.Add(currentShift);
            }
            return false;
        };
    }

    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int i = 0; i < negvertibuttons.Length; i++)
            negvertibuttons[i].OnInteract += buttonHandler(false, i, 1, negvertibuttons[i], negvertiPress);
        for (int i = 0; i < posvertibuttons.Length; i++)
            posvertibuttons[i].OnInteract += buttonHandler(false, i, -1, posvertibuttons[i], posvertiPress);
        for (int i = 0; i < poshoributtons.Length; i++)
            poshoributtons[i].OnInteract += buttonHandler(true, i, 1, poshoributtons[i], poshoriPress);
        for (int i = 0; i < neghoributtons.Length; i++)
            neghoributtons[i].OnInteract += buttonHandler(true, i, -1, neghoributtons[i], neghoriPress);
        StartCoroutine(animate());
    }

    IEnumerator animate()
    {
        while (!moduleSolved)
        {
            while (_animationQueue.Count == 0)
                yield return null;

            var item = _animationQueue.Dequeue();

            const float duration = .15f;
            float elapsed = 0;
            if (item.Direction == -1)
            {
                setBoard(item.NextState);
                while (elapsed < duration)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var tileToMove = tiles[item.Row ? (5 * item.Index + i) : (item.Index + 5 * i)];
                        var newPosition = new Vector3(
                            -4f + (item.Row ? i : item.Index) * 2.2f + (item.Row ? 2.2f * elapsed / duration - 2.2f : 0) * item.Direction,
                            2.05f - (item.Row ? item.Index : i) * 2.2f + (item.Row ? 0 : 2.2f * elapsed / duration - 2.2f) * item.Direction,
                            0);
                        tileToMove.transform.localPosition = newPosition;
                    }
                    yield return null;
                    elapsed += Time.deltaTime;
                }
            }
            else
            {
                while (elapsed < duration)
                {
                    for (var i = 0; i < 5; i++)
                    {
                        var tileToMove = tiles[item.Row ? (5 * item.Index + i) : (item.Index + 5 * i)];
                        var newPosition = new Vector3(
                            -4f + (item.Row ? i : item.Index) * 2.2f + (item.Row ? 2.2f * elapsed / duration : 0) * item.Direction,
                            2.05f - (item.Row ? item.Index : i) * 2.2f + (item.Row ? 0 : 2.2f * elapsed / duration) * item.Direction,
                            0);
                        tileToMove.transform.localPosition = newPosition;
                    }
                    yield return null;
                    elapsed += Time.deltaTime;
                }
                setBoard(item.NextState);
            }

            for (var i = 0; i < 25; i++)
                tiles[i].transform.localPosition = new Vector3(-4f + 2.2f * (i % 5), 2.05f - 2.2f * (i / 5), 0);
        }
    }

    void Start()
    {
        currentState = solveState.ToArray();
        for (int i = 0; i < 1000; i++)
        {
            var movement = rnd.Range(0, 4);
            var movement2 = rnd.Range(0, 5);
            var prevState = currentState.ToArray();
            switch (movement)
            {
                case 0:
                    negvertiPress(negvertibuttons[movement2], currentState);
                    allMoves.Add(new Shift { Row = false, Index = movement2, Direction = 1, NextState = currentState.ToArray(), PreviousState = prevState });
                    break;
                case 1:
                    posvertiPress(posvertibuttons[movement2], currentState);
                    allMoves.Add(new Shift { Row = false, Index = movement2, Direction = -1, NextState = currentState.ToArray(), PreviousState = prevState });
                    break;
                case 2:
                    neghoriPress(neghoributtons[movement2], currentState);
                    allMoves.Add(new Shift { Row = true, Index = movement2, Direction = -1, NextState = currentState.ToArray(), PreviousState = prevState });
                    break;
                default:
                    poshoriPress(poshoributtons[movement2], currentState);
                    allMoves.Add(new Shift { Row = true, Index = movement2, Direction = 1, NextState = currentState.ToArray(), PreviousState = prevState });
                    break;
            }
        }
        setBoard(currentState);
    }

    void setBoard(string[] state)
    {
        for (int i = 0; i <= 24; i++)
        {
            var letter = solveState[i];
            var m = Array.IndexOf(state, letter);
            tiles[m].material = tileColors[i];
            tileLetters[m].text = solveState[i];
        }
        if (state.SequenceEqual(solveState))
        {
            module.HandlePass();
            Debug.LogFormat("[Loopover #{0}] Module solved!", moduleId);
            audio.PlaySoundAtTransform("solve", transform);
            moduleSolved = true;
        }
    }

    void negvertiPress(KMSelectable arrow, string[] state)
    {
        var ix = Array.IndexOf(negvertibuttons, arrow);
        var s1 = state[0 + ix];
        var s2 = state[5 + ix];
        var s3 = state[10 + ix];
        var s4 = state[15 + ix];
        var s5 = state[20 + ix];
        state[0 + ix] = s2;
        state[5 + ix] = s3;
        state[10 + ix] = s4;
        state[15 + ix] = s5;
        state[20 + ix] = s1;
    }

    void posvertiPress(KMSelectable arrow, string[] state)
    {
        var ix = Array.IndexOf(posvertibuttons, arrow);
        var s1 = state[0 + ix];
        var s2 = state[5 + ix];
        var s3 = state[10 + ix];
        var s4 = state[15 + ix];
        var s5 = state[20 + ix];
        state[0 + ix] = s5;
        state[5 + ix] = s1;
        state[10 + ix] = s2;
        state[15 + ix] = s3;
        state[20 + ix] = s4;
    }

    void neghoriPress(KMSelectable arrow, string[] state)
    {
        var ix = Array.IndexOf(neghoributtons, arrow);
        var s1 = state[0 + ix * 5];
        var s2 = state[1 + ix * 5];
        var s3 = state[2 + ix * 5];
        var s4 = state[3 + ix * 5];
        var s5 = state[4 + ix * 5];
        state[0 + ix * 5] = s2;
        state[1 + ix * 5] = s3;
        state[2 + ix * 5] = s4;
        state[3 + ix * 5] = s5;
        state[4 + ix * 5] = s1;
    }

    void poshoriPress(KMSelectable arrow, string[] state)
    {
        var ix = Array.IndexOf(poshoributtons, arrow);
        var s1 = state[0 + ix * 5];
        var s2 = state[1 + ix * 5];
        var s3 = state[2 + ix * 5];
        var s4 = state[3 + ix * 5];
        var s5 = state[4 + ix * 5];
        state[0 + ix * 5] = s5;
        state[1 + ix * 5] = s1;
        state[2 + ix * 5] = s2;
        state[3 + ix * 5] = s3;
        state[4 + ix * 5] = s4;
    }

    // Twitch Plays
    private bool paramsValid(string[] prms)
    {
        string[] validsRows = { "row1", "row2", "row3", "row4", "row5" };
        string[] validsCols = { "col1", "col2", "col3", "col4", "col5" };
        string[] validsLeftsRights = { "l1", "l2", "l3", "l4", "l5", "r1", "r2", "r3", "r4", "r5" };
        string[] validsUpsDowns = { "u1", "u2", "u3", "u4", "u5", "d1", "d2", "d3", "d4", "d5" };
        if (prms.Length % 2 != 0)
        {
            return false;
        }
        for (int i = 1; i < prms.Length; i += 2)
        {
            if (!validsCols.Contains(prms[i - 1]) && !validsRows.Contains(prms[i - 1]))
            {
                return false;
            }
            if (validsCols.Contains(prms[i - 1]))
            {
                if (!validsUpsDowns.Contains(prms[i]))
                {
                    return false;
                }
            }
            else if (validsRows.Contains(prms[i - 1]))
            {
                if (!validsLeftsRights.Contains(prms[i]))
                {
                    return false;
                }
            }
        }
        return true;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} row<#> l/r<#2> [Moves left/right '#2' times in row '#'] | !{0} col<#> u/d<#2> [Moves up/down '#2' times in column '#'] | Commands are chainable, for ex: !{0} row1 l3 col1 d1";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (paramsValid(parameters))
        {
            yield return null;
            for (int i = 0; i < parameters.Length - 1; i++)
            {
                if (parameters[i].EqualsIgnoreCase("col1"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("u"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            negvertibuttons[0].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("d"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            posvertibuttons[0].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else if (parameters[i].EqualsIgnoreCase("col2"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("u"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            negvertibuttons[1].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("d"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            posvertibuttons[1].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else if (parameters[i].EqualsIgnoreCase("col3"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("u"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            negvertibuttons[2].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("d"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            posvertibuttons[2].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else if (parameters[i].EqualsIgnoreCase("col4"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("u"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            negvertibuttons[3].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("d"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            posvertibuttons[3].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else if (parameters[i].EqualsIgnoreCase("col5"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("u"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            negvertibuttons[4].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("d"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            posvertibuttons[4].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else if (parameters[i].EqualsIgnoreCase("row1"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("l"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            neghoributtons[0].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("r"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            poshoributtons[0].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else if (parameters[i].EqualsIgnoreCase("row2"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("l"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            neghoributtons[1].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("r"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            poshoributtons[1].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else if (parameters[i].EqualsIgnoreCase("row3"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("l"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            neghoributtons[2].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("r"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            poshoributtons[2].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else if (parameters[i].EqualsIgnoreCase("row4"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("l"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            neghoributtons[3].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("r"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            poshoributtons[3].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
                else if (parameters[i].EqualsIgnoreCase("row5"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i + 1].Substring(1, 1), out temp);
                    if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("l"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            neghoributtons[4].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("r"))
                    {
                        for (int j = 0; j < temp; j++)
                        {
                            poshoributtons[4].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                }
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        if (moduleSolved)
            yield break;

        // First part: move all the tiles in the top-left 4×4 into their correct positions
        while (true)
        {
            // Find the first tile within the top-left 4×4 that is not in the right place
            var wrongTile = Enumerable.Range(0, 25).Where(i => i % 5 != 4 && i / 5 != 4).Concat(new[] { -1 }).First(i => i == -1 || currentState[i] != solveState[i]);

            if (wrongTile == -1)
                goto lastPart;

            var targetCol = wrongTile % 5;
            var targetRow = wrongTile / 5;
            var curPos = Array.IndexOf(currentState, solveState[wrongTile]);
            var curCol = curPos % 5;
            var curRow = curPos / 5;

            // If the desired tile is in the rightmost column and above the target row, we need to move it below the target row.
            if (curCol == 4 && curRow <= targetRow)
            {
                for (var i = 0; i < targetRow - curRow + 1; i++)
                {
                    posvertibuttons[4].OnInteract();
                    yield return new WaitForSeconds(.1f);
                }
                curRow = targetRow + 1;
            }

            // Three cases!
            // Case 1: the desired tile is in the wrong row
            if (curRow != targetRow)
            {
                // If it’s in the correct column, shift it left one
                if (curCol == targetCol)
                {
                    neghoributtons[curRow].OnInteract();
                    curCol = (curCol + 4) % 5;
                    yield return new WaitForSeconds(.1f);
                }

                // Move the target column down
                // (We don’t need to do that if the target row is 0)
                if (targetRow > 0)
                {
                    for (var i = 0; i < curRow - targetRow; i++)
                    {
                        posvertibuttons[targetCol].OnInteract();
                        yield return new WaitForSeconds(.1f);
                    }
                }
                // Move the tile into the right column by shifting its row
                for (var i = 0; i < (targetCol - curCol + 5) % 5; i++)
                {
                    poshoributtons[curRow].OnInteract();
                    yield return new WaitForSeconds(.1f);
                }
                // Move the target column back up
                for (var i = 0; i < curRow - targetRow; i++)
                {
                    negvertibuttons[targetCol].OnInteract();
                    yield return new WaitForSeconds(.1f);
                }
            }
            // Case 2: the desired tile is already in the correct row, but wrong column
            else
            {
                // Shift the current and target column down 1
                posvertibuttons[curCol].OnInteract();
                yield return new WaitForSeconds(.1f);
                posvertibuttons[targetCol].OnInteract();
                yield return new WaitForSeconds(.1f);

                // Move the tile into the right column by shifting its row
                for (var i = 0; i < (targetCol - curCol + 5) % 5; i++)
                {
                    poshoributtons[curRow + 1].OnInteract();
                    yield return new WaitForSeconds(.1f);
                }

                // Shift the columns back up
                negvertibuttons[curCol].OnInteract();
                yield return new WaitForSeconds(.1f);
                negvertibuttons[targetCol].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }

        // Second part: use Dijkstra’s algorithm to solve the bottom row and right column
        lastPart:

        var already = new Dictionary<string, SolverQueueItem>();
        var threadDone = false;

        var thread = new Thread(() =>
        {
            var q = new Queue<SolverQueueItem>();
            q.Enqueue(new SolverQueueItem { State = currentState });
            while (q.Count > 0)
            {
                var item = q.Dequeue();
                var key = item.State.Join("");
                if (already.ContainsKey(key))
                    continue;
                already[key] = item;
                if (item.State.SequenceEqual(solveState))
                {
                    threadDone = true;
                    return;
                }

                var newState = item.State.ToArray();
                poshoriPress(poshoributtons[4], newState);
                q.Enqueue(new SolverQueueItem { PrevState = item.State, Button = poshoributtons[4], State = newState });
                newState = item.State.ToArray();
                neghoriPress(neghoributtons[4], newState);
                q.Enqueue(new SolverQueueItem { PrevState = item.State, Button = neghoributtons[4], State = newState });
                newState = item.State.ToArray();
                posvertiPress(posvertibuttons[4], newState);
                q.Enqueue(new SolverQueueItem { PrevState = item.State, Button = posvertibuttons[4], State = newState });
                newState = item.State.ToArray();
                negvertiPress(negvertibuttons[4], newState);
                q.Enqueue(new SolverQueueItem { PrevState = item.State, Button = negvertibuttons[4], State = newState });
            }
            throw new InvalidOperationException();
        });
        thread.Start();

        while (!threadDone)
            yield return true;

        var buttonPresses = new List<KMSelectable>();
        var state = solveState;
        while (state != null)
        {
            var item = already[state.Join("")];
            buttonPresses.Add(item.Button);
            state = item.PrevState;
        }
        for (int i = buttonPresses.Count - 2; i >= 0; i--)
        {
            buttonPresses[i].OnInteract();
            yield return new WaitForSeconds(.1f);
        }

        while (!moduleSolved)
            yield return true;
    }

    struct SolverQueueItem
    {
        public string[] State;
        public string[] PrevState;
        public KMSelectable Button;
    }
}
