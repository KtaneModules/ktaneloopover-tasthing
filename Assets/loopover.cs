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

		void Awake()
		{
        moduleId = moduleIdCounter++;
		}

		void Start ()
		{

		}

}
