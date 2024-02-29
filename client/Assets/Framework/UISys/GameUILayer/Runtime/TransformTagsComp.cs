using System;
using System.Collections.Generic;
using UnityEngine;

namespace UISys.Runtime
{
	[AddComponentMenu("UISys/Tags")]
	public class TransformTagsComp : MonoBehaviour
	{
		[SerializeField] protected LayerTagsConfig config;

		public string[] PresetTags => config == null ? Array.Empty<string>() : config.presetTags;

		[SerializeField] protected string[] tags;

		public virtual string[] Tags
		{
			get => tags;
			set
			{
				tags = value;
				EmitTagsChanged();
			}
		}

		public void EmitTagsChanged()
		{
			if (this.transform.parent != null)
			{
				this.transform.parent.SendMessage("OnTransformChildrenTagsChanged",
					SendMessageOptions.DontRequireReceiver);
			}
		}

		private void OnEnable()
		{
			if (this.transform.parent != null)
			{
				this.transform.parent.SendMessage("OnTransformChildrenActive", SendMessageOptions.DontRequireReceiver);
			}
		}

		private void OnDisable()
		{
			if (this.transform.parent != null)
			{
				this.transform.parent.SendMessage("OnTransformChildrenActive", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}