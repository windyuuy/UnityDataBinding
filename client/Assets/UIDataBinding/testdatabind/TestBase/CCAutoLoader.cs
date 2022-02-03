
using UnityEngine;

public class CCAutoLoader :MonoBehaviour
{
	public GameObject[] refers = new GameObject[0];

	protected virtual void Awake() {
		foreach (var r in this.refers)
		{
			var x = Instantiate(r);
			x.transform.SetParent(this.transform, false);
		}
	}
}
