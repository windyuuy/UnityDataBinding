using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var A = Resources.Load("A");
        Debug.Log("jhgjhg");
        var AC = Instantiate(A, this.transform) as GameObject;
        Debug.Log("bbbbb");
        var AC2 = Instantiate(A, AC.transform) as GameObject;
        Debug.Log("cccc");
        Destroy(AC2);
        //AC2.transform.parent = null;
        //AC2.transform.parent = AC.transform;
        //AC.transform.parent = this.transform;
    }

}
