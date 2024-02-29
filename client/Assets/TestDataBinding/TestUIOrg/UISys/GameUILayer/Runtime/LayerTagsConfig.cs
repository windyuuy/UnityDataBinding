using UnityEngine;

namespace UISys.Runtime
{
	[CreateAssetMenu(fileName = "LayerTagsConfig.asset", menuName = "UISys/LayerTagsConfig")]
	public class LayerTagsConfig:ScriptableObject
	{
		public string[] presetTags;
	}
}