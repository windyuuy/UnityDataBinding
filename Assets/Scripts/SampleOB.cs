using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DataBinding;

[Observable]
public class SampleOB: IObservableAttrs
{
    public double KKK { get; set; } = 234;
}
