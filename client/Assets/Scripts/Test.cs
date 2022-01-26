using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public Test() : base()
    {
        Debug.Log("Cre");
    }
    void Awake()
    {
        Debug.Log("OnLoad");
    }
    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }
    public void OnTransformParentChanged()
    {
        Debug.Log("OnTransformParentChanged:"+this.name);
    }
    public void OnTransformChildrenChanged()
    {
        Debug.Log("OnTransformChildrenChanged:"+this.name);
    }

    //public void OnAfterDeserialize()
    //{
    //}

    //public void OnBeforeSerialize()
    //{
    //    console.log(this.transform.parent);
    //}
}

