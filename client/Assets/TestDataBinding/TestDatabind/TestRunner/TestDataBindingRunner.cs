using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestUIDataBinding
{
	public class TestDataBindingRunner : MonoBehaviour
	{
		[Test]
		public void RunScene1()
		{
			EditorSceneManager.LoadSceneInPlayMode("Assets/TestDataBinding/TestDatabind/TestScene.unity",
				new LoadSceneParameters(LoadSceneMode.Additive));
		}
	}
}