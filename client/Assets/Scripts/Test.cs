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
        Debug.Log("OnLoad:"+this.name);
    }
    private void OnEnable()
    {
        Debug.Log("OnEnable");
    }
    public void OnTransformParentChanged()
    {
        Debug.Log("OnTransformParentChanged:"+this.name);
    }
    private void OnBeforeTransformParentChanged()
    {
        Debug.Log("OnBeforeTransformParentChanged:" + this.name);

    }
    private void OnRectTransformDimensionsChange()
    {
        Debug.Log("OnRectTransformDimensionsChange:" + this.name);

    }
    private void OnRectTransformRemoved()
    {
        Debug.Log("OnRectTransformRemoved:" + this.name);

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
    //    Console.Log(this.transform.parent);
    //}
}

