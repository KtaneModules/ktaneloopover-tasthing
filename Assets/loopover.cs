using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using rnd = UnityEngine.Random;

public class loopover : MonoBehaviour
{
    public KMAudio Audio;
    public KMBombInfo bomb;

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
        public bool Row;
        public int Index;
        public int Direction;
        public string[] State;
    }

    private readonly Queue<Shift> _animationQueue = new Queue<Shift>();

    KMSelectable.OnInteractHandler buttonHandler(bool row, int index, int direction, KMSelectable arrow, Action<KMSelectable> method)
    {
        return delegate ()
        {
            if (!moduleSolved)
            {
                Audio.PlaySoundAtTransform("tick", arrow.transform);
                method(arrow);
                _animationQueue.Enqueue(new Shift { Row = row, Index = index, Direction = direction, State = currentState.ToArray() });
            }
            return false;
        };
    }

    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (var i = 0; i < negvertibuttons.Length; i++)
        {
            negvertibuttons[i].OnInteract += buttonHandler(false, i, 1, negvertibuttons[i], negvertiPress);
        }
        for (var i = 0; i < posvertibuttons.Length; i++)
        {
            posvertibuttons[i].OnInteract += buttonHandler(false, i, -1, posvertibuttons[i], posvertiPress);
        }
        for (var i = 0; i < poshoributtons.Length; i++)
        {
            poshoributtons[i].OnInteract += buttonHandler(true, i, 1, poshoributtons[i], poshoriPress);
        }
        for (var i = 0; i < neghoributtons.Length; i++)
        {
            neghoributtons[i].OnInteract += buttonHandler(true, i, -1, neghoributtons[i], neghoriPress);
        }
        StartCoroutine(animate());
    }

    private IEnumerator animate()
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
                setBoard(item.State);
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
                setBoard(item.State);
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
            switch (rnd.Range(0, 4))
            {
                case 0:
                    negvertiPress(negvertibuttons[rnd.Range(0, 5)]);
                    break;
                case 1:
                    posvertiPress(posvertibuttons[rnd.Range(0, 5)]);
                    break;
                case 2:
                    neghoriPress(neghoributtons[rnd.Range(0, 5)]);
                    break;
                default:
                    poshoriPress(poshoributtons[rnd.Range(0, 5)]);
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
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Loopover #{0}] Module solved.", moduleId);
            Audio.PlaySoundAtTransform("solve", transform);
            moduleSolved = true;
        }
    }

    void negvertiPress(KMSelectable arrow)
    {
        var ix = Array.IndexOf(negvertibuttons, arrow);
        var s1 = currentState[0 + ix];
        var s2 = currentState[5 + ix];
        var s3 = currentState[10 + ix];
        var s4 = currentState[15 + ix];
        var s5 = currentState[20 + ix];
        currentState[0 + ix] = s2;
        currentState[5 + ix] = s3;
        currentState[10 + ix] = s4;
        currentState[15 + ix] = s5;
        currentState[20 + ix] = s1;
    }

    void posvertiPress(KMSelectable arrow)
    {
        var ix = Array.IndexOf(posvertibuttons, arrow);
        var s1 = currentState[0 + ix];
        var s2 = currentState[5 + ix];
        var s3 = currentState[10 + ix];
        var s4 = currentState[15 + ix];
        var s5 = currentState[20 + ix];
        currentState[0 + ix] = s5;
        currentState[5 + ix] = s1;
        currentState[10 + ix] = s2;
        currentState[15 + ix] = s3;
        currentState[20 + ix] = s4;
    }

    void neghoriPress(KMSelectable arrow)
    {
        var ix = Array.IndexOf(neghoributtons, arrow);
        var s1 = currentState[0 + ix * 5];
        var s2 = currentState[1 + ix * 5];
        var s3 = currentState[2 + ix * 5];
        var s4 = currentState[3 + ix * 5];
        var s5 = currentState[4 + ix * 5];
        currentState[0 + ix * 5] = s2;
        currentState[1 + ix * 5] = s3;
        currentState[2 + ix * 5] = s4;
        currentState[3 + ix * 5] = s5;
        currentState[4 + ix * 5] = s1;
    }

    void poshoriPress(KMSelectable arrow)
    {
        var ix = Array.IndexOf(poshoributtons, arrow);
        var s1 = currentState[0 + ix * 5];
        var s2 = currentState[1 + ix * 5];
        var s3 = currentState[2 + ix * 5];
        var s4 = currentState[3 + ix * 5];
        var s5 = currentState[4 + ix * 5];
        currentState[0 + ix * 5] = s5;
        currentState[1 + ix * 5] = s1;
        currentState[2 + ix * 5] = s2;
        currentState[3 + ix * 5] = s3;
        currentState[4 + ix * 5] = s4;
    }

    /*[UnityEditor.MenuItem("DoStuff/DoStuff")]
    public static void DoStuff()
    {
      var m = FindObjectOfType<loopover>();
      var template = m.transform.Find("tiles").Find("tile1").Find("letter1").gameObject;
      m.tileLetters = new TextMesh[25];
      m.tileLetters[0] = template.GetComponent<TextMesh>();
      for (int i = 2; i <= 25; i++)
      {
        var obj = Instantiate(template);
        obj.transform.parent = m.transform.Find("tiles").Find("tile" + i);
        m.tileLetters[i-1] = obj.GetComponent<TextMesh>();
        obj.name = "letter" + i;
        obj.transform.localPosition = template.transform.localPosition;
        obj.transform.localRotation = template.transform.localRotation;
        obj.transform.localScale = template.transform.localScale;
        obj.GetComponent<TextMesh>().text = ((char)('A'+i-1)).ToString();
      }
    }*/

    //twitch plays
    private bool paramsValid(string[] prms)
    {
        string[] validsRows = { "row1", "row2", "row3", "row4", "row5" };
        string[] validsCols = { "col1", "col2", "col3", "col4", "col5" };
        string[] validsLeftsRights = { "l1", "l2", "l3", "l4", "l5", "r1", "r2", "r3", "r4", "r5" };
        string[] validsUpsDowns = { "u1", "u2", "u3", "u4", "u5", "d1", "d2", "d3", "d4", "d5" };
        if(prms.Length % 2 != 0)
        {
            return false;
        }
        for(int i = 1; i < prms.Length; i += 2)
        {
            if (!validsCols.Contains(prms[i - 1]) && !validsRows.Contains(prms[i - 1]))
            {
                return false;
            }
            if (validsCols.Contains(prms[i - 1]))
            {
                if (!validsUpsDowns.Contains(prms[i])){
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
            for(int i = 0; i < parameters.Length-1; i++)
            {
                if (parameters[i].EqualsIgnoreCase("col1"))
                {
                    int temp = 0;
                    int.TryParse(parameters[i+1].Substring(1, 1), out temp);
                    if(parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("u"))
                    {
                        for(int j = 0; j < temp; j++)
                        {
                            negvertibuttons[0].OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                    }else if (parameters[i + 1].Substring(0, 1).EqualsIgnoreCase("d"))
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

}
