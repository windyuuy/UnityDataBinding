using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataBind.UIBind
{
	public abstract class AContainerBindComp : DataBindBaseComp, ICCContainerBinding
	{
		public virtual string BindSubExp { get => bindSubExp; set => bindSubExp = value; }
		[Rename("容器数据")]
		[SerializeField]
		protected string bindSubExp = "";

		public virtual ContainerBind ContainerBind { get; set; } = new ContainerBind();
		
		public virtual void BindItem(ContainerItemComp item)
		{
			this.ContainerBind.BindItem(item.ContainerItem);
			item.ContainerItem.SetParent(this.ContainerBind);
		}

		public virtual void UnbindItem(ContainerItemComp item)
		{
			this.ContainerBind.UnbindItem(item.ContainerItem);
			item.ContainerItem.UnsetParent(this.ContainerBind);
		}

		protected override void OnBindItems()
		{
			throw new NotImplementedException();
		}

		/**
		 * 集成
		 * - 遍历所有浅层子hub, 设置父节点为自身
		 */
		public override void Integrate()
		{
			this.ContainerBind.RawObj = this;
			DataBindHubHelper.OnAddContainerBind(this);
		}

		protected bool IsRelate = false;
		public override void Relate()
		{
			if (this.IsRelate)
			{
				return;
			}
			this.IsRelate = true;
			DataBindHubHelper.OnRelateContainerBind(this);
		}

		public override void Derelate()
		{
			if (!this.IsRelate)
			{
				return;
			}

			this.IsRelate = false;
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
	
	/// <summary>
	/// ContainerBindComp 本身并不强制所有子节点采用 container 的数据源，而是在 integrate 阶段，通过增加 DataBindHubComp 组件实现对子节点注入数据源
	/// </summary>
	[AddComponentMenu("DataDrive/ContainerBind")]
	public class ContainerBindComp : AContainerBindComp
	{
		public override void Integrate()
		{
			base.Integrate();
			
			ContainerCtrl_Integrate();
		}

		public virtual void ContainerCtrl_Integrate()
		{
			var ccContainerBind = this.GetComponent<ContainerBindComp>();
			this.SendMessage("InitContainer", ccContainerBind, SendMessageOptions.DontRequireReceiver);
		}

		public override void Relate()
		{
			base.Relate();
			
			ContainerCtrl_Relate();
		}

		public virtual void ContainerCtrl_Relate()
		{
			ContainerCtrl_Watcher = ContainerBind.WatchList((object newValue, object oldValue) =>
			{
				var dataSources = ((System.Collections.IList)newValue);
				this.SendMessage("OnDataChanged", dataSources, SendMessageOptions.DontRequireReceiver);
			});
		}

		public override void Derelate()
		{
			base.Derelate();
			
			ContainerCtrl_Derelate();
		}

		protected EventHandlerMV2<object, object> ContainerCtrl_Watcher;
		public virtual void ContainerCtrl_Derelate()
		{
			ContainerBind.UnWatchList(this.ContainerCtrl_Watcher);
		}

	}
}
