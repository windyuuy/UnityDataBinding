using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RTestLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        var root = GameObject.Find("Canvas");
        var rootPrefab=Resources.Load("RRoot");
        var rootNode=Instantiate(rootPrefab,root.transform);
    }

}
