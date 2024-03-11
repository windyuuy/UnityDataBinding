using DataBinding.UIBind;
using UnityEngine;

namespace TestDataBinding.TestBasic
{
	public class TestBase : MonoBehaviour
	{

		protected virtual bool Assert(bool cond, string msg = "invalid cond")
		{
			if (cond == false)
			{
				throw new System.Exception(msg);
			}
			return cond;
		}

		//#region data host
		protected void IntegrateDataBind()
		{
			if (this._dataHost == null)
			{
				var ccDataHost = this.GetComponent<DataHostComp>();
				if (ccDataHost == null && this.autoAddDataHost)
				{
					ccDataHost = this.gameObject.AddComponent<DataHostComp>();
				}
				this._dataHost = ccDataHost;
			}
		}
		private DataHostComp _dataHost;
		public DataHostComp DataHost
		{
			get
			{
				return this._dataHost;
			}
		}

		protected bool autoAddDataHost = true;
		/**
		 * 观测数据
		 * @param data
		 */
		public virtual void ObserveData(object data, bool updateChildren = true)
		{
			this.autoAddDataHost = true;
			if (this.DataHost)
			{
				this.DataHost.ObserveData(data);
			}
		}
		//#endregion

		protected object _rawData;
		public object Data
		{
			get
			{
				return this._rawData;
			}
		}
		protected virtual void InitTestData()
		{

		}
		public virtual void Awake()
		{
			this.InitTestData();
			this.IntegrateDataBind();
			this.ObserveData(this.Data);
		}

		public virtual void Start()
		{
			// this.scheduleOnce(() => {
			try
			{
				Debug.Log("Begin Test <" + this.name + ">");
				this.Test();
				Debug.Log("Success Test <" + this.name + ">");
			}
			catch (System.Exception e)
			{
				Debug.LogError("Failed Test <" + this.name + ">");
				Debug.LogException(e);
			}



			if (MyComponent.EnableLazyAttach)
			{
				this.TestLazy();
			}
			// }, 0.2)
		}

		public virtual void Test()
		{
		}
		public virtual void TestLazy()
		{
		}

		public virtual void Tick()
		{
			VM.Tick.Next();
		}

		protected virtual void Update()
		{
			this.Tick();
		}

	}
}
