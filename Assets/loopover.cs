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

}
