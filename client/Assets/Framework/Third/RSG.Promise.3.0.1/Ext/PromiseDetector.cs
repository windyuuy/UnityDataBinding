
using System.Collections;
using System.Linq;
using RSG;
using UnityEngine;

public class PromiseDetector : System.IDisposable
{
    public void Dispose()
    {

    }

    public void Init()
    {
        LoomMG.sharedLoom.StartCoroutine(Update());
    }



    public IEnumerator Update()
    {
        while (true)
        {
            var es = Promise.CheckUncaughtExceptions();
            var counter = 0;
            foreach (var e in es)
            {
                ++counter;  
                Debug.LogException(e);
            }
            if (counter > 0)
            {
                Debug.LogError($"Uncaught Exception Count: {counter}");
            }
            Promise.ClearUncaughtPromises();
            yield return null;
#if !UNITY_EDITOR
            yield return null;
            yield return null;
            yield return null;
#endif
        }
    }

}
