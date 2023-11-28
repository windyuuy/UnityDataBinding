using System;
using System.Collections.Generic;
using System.Linq;
using Framework.GameLib.MonoUtils;
using MoreLinq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI.UISys.GameUILayer.Runtime.Input
{
	public interface IWithKeyback
	{
		void OnKeyback();
		bool isActiveAndEnabled { get; }
		int GetLayerOrder();
	}
	
	public class UIInputMG:IDisposable
	{
		public static readonly UIInputMG Shared = new();

		protected List<IWithKeyback> keybackHandlers;

		public void TryRegister(object handler)
		{
			if (handler is IWithKeyback keybackHandler)
			{
				this.RegisterOnKeyback(keybackHandler);
			}
		}
		public void RegisterOnKeyback(IWithKeyback handler)
		{
			if (keybackHandlers == null)
			{
				keybackHandlers = new();
				MonoSchedulerMG.SharedMonoScheduler.Schedule(Update);
			}

			if (!keybackHandlers.Contains(handler))
			{
				keybackHandlers.Add(handler);
			}
		}
		public void UnregisterOnKeyback(IWithKeyback handler)
		{
			if (keybackHandlers == null)
			{
				return;
			}
			keybackHandlers.Remove(handler);
		}

		public bool IsOnKeybackEnabled(IWithKeyback handler)
		{
			return keybackHandlers.Contains(handler);
		}

		protected void Update()
		{
			if (UnityEngine.Input.GetKeyUp(KeyCode.Escape))
			{
				DispatchKeyback();
			}
		}

		protected List<IWithKeyback> tempList;
		protected void DispatchKeyback()
		{
			if (tempList == null)
			{
				tempList = new();
			}

			keybackHandlers.RemoveAll(handler => !IsValidHandler(handler));
			// tempList.Clear();
			// tempList.AddRange(keybackHandlers.Where(handler => handler != null && handler.isActiveAndEnabled));
			// foreach (var handler in tempList)
			// {
			// 	try
			// 	{
			// 		handler.OnKeyback();
			// 	}
			// 	catch (Exception e)
			// 	{
			// 		Debug.LogException(e);
			// 	}
			// }
			// tempList.Clear();
			
			// 暂时只支持topui的情形
			var topHandler = GetTopKeybackHandler();
			if (topHandler != null)
			{
				try
				{
					topHandler.OnKeyback();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}
		}

		public void Dispose()
		{
			MonoSchedulerMG.SharedMonoScheduler.Unschedule(DispatchKeyback);
		}

		public IWithKeyback GetTopKeybackHandler()
		{
			if (keybackHandlers!=null && keybackHandlers.Count > 0)
			{
				var topHandler=keybackHandlers
					.Where(handler=>handler!=null&&handler.isActiveAndEnabled)
					.MaxBy(handler => handler.GetLayerOrder()).LastOrDefault();
				return topHandler;
			}

			return null;
		}
		
		public bool IsValidHandler(IWithKeyback handler){
			try
			{
				if (handler != null)
				{
					var tmp = handler.isActiveAndEnabled;
					return true;
				}
			}
			catch
			{
				return false;
			}
			return false;
		}

		/// <summary>
		/// 判断在注册响应键盘事件的UI层中, 是否排在最前
		/// </summary>
		/// <param name="handler"></param>
		/// <returns></returns>
		public bool IsTopUI(IWithKeyback handler)
		{
			if (IsValidHandler(handler)==false || handler.isActiveAndEnabled == false)
			{
				return false;
			}
			
			var layerOrder = handler.GetLayerOrder();
			foreach (var handler1 in keybackHandlers)
			{
				if (IsValidHandler(handler1)==false || handler1.isActiveAndEnabled==false)
				{
					continue;
				}
				
				if (handler1.GetLayerOrder() > layerOrder)
				{
					return false;
				}
			}

			return true;
		}
	}
}