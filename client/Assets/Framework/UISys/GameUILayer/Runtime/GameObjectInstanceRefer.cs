using UnityEngine;

namespace UISys.Runtime
{
	// [CreateAssetMenu(fileName = "GameObjectInstanceRefer.asset", menuName = "UISys/GameObjectInstanceRefer")]
	public class GameObjectInstanceRefer : ScriptableObject
	{
		public virtual UILayerRoot Target { get;set; }
	}
}