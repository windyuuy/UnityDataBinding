using UnityEngine;

namespace UISys.Runtime
{
	[CreateAssetMenu(fileName = "PresetTagsConfig.asset", menuName = "UISys/PresetTagsConfig")]
	public class PresetTagsConfig:ScriptableObject
	{
		public string[] presetTags;
	}
}