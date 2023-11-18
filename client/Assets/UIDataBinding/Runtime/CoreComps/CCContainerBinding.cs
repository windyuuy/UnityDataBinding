using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataBinding.UIBind
{
	[AddComponentMenu("DataDrive/CCContainerBinding")]
	public class CCContainerBinding : CCMyComponent, ICCContainerBinding
	{
		public virtual string BindSubExp { get => bindSubExp; set => bindSubExp = value; }
		[Rename("容器数据")]
		[SerializeField]
		protected string bindSubExp = "";

		/**
		 * 如果子节点没有添加 DialogChild, 那么强制为所有子节点添加 DialogChild
		 */
		public bool bindChildren = true;

		public virtual ContainerBind ContainerBind { get; set; } = new ContainerBind();

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public virtual void Integrate()
		{
			this.ContainerBind.rawObj = this;
			DataBindHubHelper.OnAddContainerBind(this);
		}

		protected bool isRelate = false;
		public virtual void Relate()
		{
			this.isRelate = true;
			DataBindHubHelper.OnRelateContainerBind(this);
		}

		public virtual void Derelate()
		{
			DataBindHubHelper.OnDerelateContainerBind(this);
		}

		protected override void OnPreload()
		{
			this.Integrate();
		}
		protected override void OnPreDestroy()
		{
			this.Derelate();
		}

		protected override void OnAttach()
		{
			this.Relate();
		}

		protected override void OnDeattach()
		{
			this.Derelate();
		}

	}
}
