using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRR : MonoBehaviour
{
    public GameObject sample;
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Instantiate(sample, this.transform);
    }

}
