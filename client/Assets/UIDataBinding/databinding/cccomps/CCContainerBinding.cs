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

		public virtual ContainerBind containerBind { get; set; } = new ContainerBind();

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public virtual void integrate()
		{
			this.containerBind.rawObj = this;
			DataBindHubHelper.onAddContainerBind(this);
		}

		protected bool isRelate = false;
		public virtual void relate()
		{
			this.isRelate = true;
			DataBindHubHelper.onRelateContainerBind(this);
		}

		public virtual void derelate()
		{
			DataBindHubHelper.onDerelateContainerBind(this);
		}

		protected override void onPreload()
		{
			this.integrate();
		}
		protected override void onPreDestroy()
		{
			this.derelate();
		}

		protected override void onAttach()
		{
			this.relate();
		}

		protected override void onDeattach()
		{
			this.derelate();
		}

	}
}
