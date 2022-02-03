using UnityEngine;

namespace DataBinding.UIBind
{
	public class TestBase : MonoBehaviour
	{

		protected virtual bool assert(bool cond, string msg = "invalid cond")
		{
			if (cond == false)
			{
				throw new System.Exception(msg);
			}
			return cond;
		}

		//#region data host
		protected void integrateDataBind()
		{
			if (this._dataHost == null)
			{
				var ccDataHost = this.GetComponent<CCDataHost>();
				if (ccDataHost == null && this.autoAddDataHost)
				{
					ccDataHost = this.gameObject.AddComponent<CCDataHost>();
				}
				this._dataHost = ccDataHost;
			}
		}
		private CCDataHost _dataHost;
		public CCDataHost dataHost
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
		public void observeData(object data, bool updateChildren = true)
		{
			this.autoAddDataHost = true;
			if (this.dataHost)
			{
				this.dataHost.observeData(data);
			}
		}
		//#endregion

		protected object _rawData;
		public object data
		{
			get
			{
				return this._rawData;
			}
		}
		protected virtual void initTestData()
		{

		}
		public virtual void Awake()
		{
			this.initTestData();
			this.integrateDataBind();
			this.observeData(this.data);
		}

		public virtual void Start()
		{
			// this.scheduleOnce(() => {
			try
			{
				Debug.Log("Begin Test <" + this.name + ">");
				this.test();
				Debug.Log("Success Test <" + this.name + ">");
			}
			catch (System.Exception e)
			{
				Debug.LogError("Failed Test <" + this.name + ">");
				Debug.LogException(e);
			}



			if (CCMyComponent.EnableLazyAttach)
			{
				this.testLazy();
			}
			// }, 0.2)
		}

		public virtual void test()
		{
		}
		public virtual void testLazy()
		{
		}

		public virtual void tick()
		{
			vm.Tick.next();
		}

		protected virtual void Update()
		{
			this.tick();
		}

	}
}
