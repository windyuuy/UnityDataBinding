using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Linq;

public class MyLoom : MonoBehaviour
{

	public static MyLoom CreateOne()
	{
		var obj = new GameObject("MySharedLoom");
		var inst = obj.AddComponent<MyLoom>();
		GameObject.DontDestroyOnLoad(inst);
		return inst;
	}

	private readonly List<Action> _taskList = new List<Action>();

	protected Thread MainThread;
	void Awake()
	{
		MainThread = Thread.CurrentThread;
	}
	protected readonly Queue<Action> TaskListCopy = new();
	void Update()
	{
		if (_taskList.Count > 0)
		{
			lock (_taskList)
			{
				if (_taskList.Count > 0)
				{
					foreach (var task in _taskList)
					{
						TaskListCopy.Enqueue(task);
					}
					_taskList.Clear();
				}
			}
		}

		while (TaskListCopy.Count > 0)
		{
			var task = TaskListCopy.Dequeue();
			try
			{
				task.Invoke();
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}
	}

	public void AddTask(Action task)
	{
		lock (_taskList)
		{
			_taskList.Add(task);
		}
	}

	public void RunTask(Action task)
	{
		this.AddTask(task);

		if (MainThread == Thread.CurrentThread)
		{
			Update();
		}
	}
}
