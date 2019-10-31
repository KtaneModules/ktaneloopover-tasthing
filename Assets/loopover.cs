using System;
﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

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

		void Awake()
		{
        moduleId = moduleIdCounter++;
		}

		void Start ()
		{
      currentState = solveState.ToArray().Shuffle();
      tileState();
		}

    void tileState()
    {
      for(int i = 0; i <= 24; i++)
      {
        var letter = solveState[i];
        var m = Array.IndexOf(currentState, letter);
        tiles[i].material = tileColors[m];
        tileLetters[i].text = solveState[m];
      }
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
