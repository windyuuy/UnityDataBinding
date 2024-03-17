using System;
using System.Collections.Generic;
using System.Linq;
using DataBind.UIBind;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public class DataBindHubEditorWindow : EditorWindow
{
	[SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

	[MenuItem("Window/UI Toolkit/DataBindHubEditorWindow")]
	public static void ShowExample()
	{
		DataBindHubEditorWindow wnd = GetWindow<DataBindHubEditorWindow>();
		wnd.titleContent = new GUIContent("DataBindHubEditorWindow");
		wnd.Show();
	}

	public static void ShowBindRelation(IRawObjObservable bindHubTree)
	{
		DataBindHubEditorWindow wnd = GetWindow<DataBindHubEditorWindow>();
		wnd.titleContent = new GUIContent("DataBindHubEditorWindow");
		wnd.UpdateView(bindHubTree);
		wnd.Show();
	}

	private IRawObjObservable _bindHubTree;

	private int _idAcc;

	public struct DataBindWrap
	{
		public IRawObjObservable Bind;
		public string Key;
		public bool IsRecursive;

		public DataBindWrap(IRawObjObservable bind, string key, bool isRecursive)
		{
			Bind = bind;
			Key = key;
			IsRecursive = isRecursive;
		}
	}

	private void UpdateView(IRawObjObservable bindHubTree)
	{
		if (bindHubTree == null)
		{
			return;
		}

		_bindHubTree = bindHubTree;

		_idAcc = 0;
		_rootItems.Clear();

		var referedMap = new Dictionary<(IRawObjObservable bindComp, string key), int>();

		void UpdateItem(List<TreeViewItemData<DataBindWrap>> parent, (IRawObjObservable bindComp, string key) wrapInfo)
		{
			var itemId = _idAcc++;
			var (bindComp, keyInput) = wrapInfo;
			if (referedMap.ContainsKey(wrapInfo))
			{
				var item = new TreeViewItemData<DataBindWrap>(itemId, new(bindComp, keyInput, true));
				parent.Add(item);
				return;
			}

			var subList = new List<TreeViewItemData<DataBindWrap>>();
			referedMap.Add(wrapInfo, itemId);

			if (bindComp is IDataBindRepeater bindHub)
			{
				// if (bindHub.BindHubs.Count > 0)
				// {
				// 	foreach (var subHub in bindHub.BindHubs)
				// 	{
				// 		UpdateItem(subList, subHub);
				// 	}
				// }

				// foreach (var dataBindPump in bindHub.GetBindPumps())
				// {
				// 	UpdateItem(subList, (dataBindPump, null));
				// }

				foreach (var peekBindEvent in bindHub.PeekBindEvents())
				{
					UpdateItem(subList, (peekBindEvent.target, peekBindEvent.key));
				}
			}

			if (bindComp is IDataSourcePump dataSource)
			{
				// if (dataSource.BindHub != null)
				// {
				// 	UpdateItem(subList, (dataSource.BindHub, null));
				// }
				foreach (var rawObjObservable in dataSource.PeekSourceBindEvents<IRawObjObservable>())
				{
					UpdateItem(subList, (rawObjObservable, null));
				}
			}

			// if (bindComp is IDataBindPump bindPump)
			// {
			// 	UpdateItem(subList, bindPump.BindHub);
			// }

			if (bindComp is IDataSourceDispatcher dispatcher)
			{
				foreach (var dataSourceHub in dispatcher.GetDataSourceHubs())
				{
					UpdateItem(subList, (dataSourceHub, null));
				}
			}

			if (subList.Count > 0)
			{
				var item = new TreeViewItemData<DataBindWrap>(itemId, new(bindComp, keyInput, false), subList);
				parent.Add(item);
			}
			else
			{
				var item = new TreeViewItemData<DataBindWrap>(itemId, new(bindComp, keyInput, false));
				parent.Add(item);
			}
		}

		var parents = FindParents(bindHubTree);
		foreach (var dataBind in parents)
		{
			UpdateItem(_rootItems, (dataBind, null));
		}

		var item = referedMap.First(refer => refer.Key.bindComp == bindHubTree);
		var selectedIndex = item.Value;
		CurMarkIndex = selectedIndex;

		var treeView = rootVisualElement.Q<TreeView>();
		treeView.SetRootItems(_rootItems);
		treeView.RefreshItems();
		treeView.ExpandAll();
		treeView.selectedIndex = selectedIndex;
	}

	protected static List<IRawObjObservable> FindParents(IRawObjObservable dataBindHub0)
	{
		var parents = new List<IRawObjObservable>();
		var tempParents = new HashSet<IRawObjObservable>();

		void FindParentsInternal(IRawObjObservable dataBindHub)
		{
			if (!tempParents.Add(dataBindHub))
			{
				return;
			}

			if (dataBindHub is IDataBindHubSubTree subTree)
			{
				if (subTree.Parents.Count == 0)
				{
					parents.Add(dataBindHub);
				}
				else
				{
					foreach (var parent in subTree.Parents)
					{
						if (parent is IRawObjObservable parent1)
						{
							FindParentsInternal(parent1);
						}
					}
				}
			}
			else
			{
				parents.Add(dataBindHub);
			}
		}

		FindParentsInternal(dataBindHub0);
		tempParents.Clear();
		return parents;
	}

	protected int CurMarkIndex = 0;

	private readonly List<TreeViewItemData<DataBindWrap>> _rootItems = new();

	public void CreateGUI()
	{
		// Each editor window contains a root VisualElement object
		VisualElement root = rootVisualElement;

		// Instantiate UXML
		VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
		root.Add(labelFromUXML);

		var treeView = rootVisualElement.Q<TreeView>();

		treeView.SetRootItems(_rootItems);

		treeView.makeItem = () => { return new Label(); };

		treeView.bindItem = (VisualElement element, int index) =>
		{
			var wrapInfo = treeView.GetItemDataForIndex<DataBindWrap>(index);
			var bindHub = wrapInfo.Bind;
			if (bindHub != null)
			{
				var obj = (MonoBehaviour)bindHub.RawObj;
				var labelElement = (Label)element;
				if (obj == null)
				{
					labelElement.text = "*invalid listener not released";
				}
				else
				{
					var referMark = wrapInfo.IsRecursive ? "&" : "";
					var nameMark = string.IsNullOrEmpty(wrapInfo.Key) ? "" : $" - {wrapInfo.Key}";
					var curMark = index == CurMarkIndex ? "\u27BD" : "";
					labelElement.text =
						$"{curMark}{referMark}{obj.name}<{obj.GetType().Name}>{nameMark}";
				}

				element.userData = bindHub;
				// Debug.Log($"RegisterCallback");
				element.RegisterCallback<PointerUpEvent, VisualElement>(HandleClick, element, TrickleDown.TrickleDown);
			}
		};

		treeView.unbindItem = ((element, i) =>
		{
			// Debug.Log($"UnregisterCallback");
			element.UnregisterCallback<PointerUpEvent, VisualElement>(HandleClick, TrickleDown.TrickleDown);
		});
	}

	protected void HandleClick(PointerUpEvent evt, VisualElement asset)
	{
		Debug.Log($"click {evt.target}");
		try
		{
			// Only perform this action at the target, not in a parent
			if (evt.propagationPhase != PropagationPhase.AtTarget)
			{
				return;
			}

			// if (evt.clickCount < 2)
			// {
			// 	return;
			// }

			// Assign a random new color
			var targetBox = evt.target as VisualElement;
			var bindHub = (IRawObjObservable)targetBox.userData;
			var obj = (Component)bindHub.RawObj;
			if (obj != null)
			{
				Selection.activeObject = (Object)bindHub.RawObj;
			}
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}
}