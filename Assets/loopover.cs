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
    public String[] letters;
    public TextMesh[] tileLetters;
    private string[] solveState = new string[25] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y" };
    private string[] currentState;
    public KMSelectable[] negvertibuttons;
    public KMSelectable[] posvertibuttons;
    public KMSelectable[] poshoributtons;
    public KMSelectable[] neghoributtons;

    KMSelectable.OnInteractHandler buttonHandler(KMSelectable arrow, Action<KMSelectable> method)
    {
        return delegate ()
        {
            Audio.PlaySoundAtTransform("tick", arrow.transform);
            if (!moduleSolved)
            {
                method(arrow);
                tileState();
            }
            return false;
        };
    }

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable arrow in negvertibuttons)
        {
            arrow.OnInteract += buttonHandler(arrow, negvertiPress);
        }
        foreach (KMSelectable arrow in posvertibuttons)
        {
            arrow.OnInteract += buttonHandler(arrow, posvertiPress);
        }
        foreach (KMSelectable arrow in poshoributtons)
        {
            arrow.OnInteract += buttonHandler(arrow, poshoriPress);
        }
        foreach (KMSelectable arrow in neghoributtons)
        {
            arrow.OnInteract += buttonHandler(arrow, neghoriPress);
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
        tileState();
    }

    void tileState()
    {
        for (int i = 0; i <= 24; i++)
        {
            var letter = solveState[i];
            var m = Array.IndexOf(currentState, letter);
            tiles[m].material = tileColors[i];
            tileLetters[m].text = solveState[i];
        }
        if (currentState.SequenceEqual(solveState))
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
