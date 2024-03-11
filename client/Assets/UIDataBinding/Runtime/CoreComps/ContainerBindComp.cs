using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataBinding.UIBind
{
	/// <summary>
	/// ContainerBindComp 本身并不强制所有子节点采用 container 的数据源，而是在 integrate 阶段，通过增加 DataBindHubComp 组件实现对子节点注入数据源
	/// </summary>
	[AddComponentMenu("DataDrive/ContainerBindComp")]
	public class ContainerBindComp : MyComponent, ICCContainerBinding
	{
		public virtual string BindSubExp { get => bindSubExp; set => bindSubExp = value; }
		[Rename("容器数据")]
		[SerializeField]
		protected string bindSubExp = "";

		public virtual ContainerBind ContainerBind { get; set; } = new ContainerBind();

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public virtual void Integrate()
		{
			this.ContainerBind.RawObj = this;
			DataBindHubHelper.OnAddContainerBind(this);
		}

		protected bool isRelate = false;
		public virtual void Relate()
		{
			if (this.isRelate)
			{
				return;
			}
			this.isRelate = true;
			DataBindHubHelper.OnRelateContainerBind(this);
		}

		public virtual void Derelate()
		{
			if (!this.isRelate)
			{
				return;
			}

			this.isRelate = false;
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
