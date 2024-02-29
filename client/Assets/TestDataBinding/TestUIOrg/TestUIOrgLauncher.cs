using System;
using System.Collections;
using System.Collections.Generic;
using gcc.layer;
using UnityEngine;

public class TestUIOrgLauncher : MonoBehaviour
{
    private void Update()
    {
        VM.Tick.Next();
    }
}
