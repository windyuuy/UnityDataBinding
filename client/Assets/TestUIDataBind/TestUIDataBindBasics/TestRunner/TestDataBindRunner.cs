using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TestUIDataBind
{
	public class TestDataBindRunner : MonoBehaviour
	{
		[Test]
		public void RunScene1()
		{
			EditorSceneManager.LoadSceneInPlayMode("Assets/TestDataBind/TestDataBind/TestScene.unity",
				new LoadSceneParameters(LoadSceneMode.Additive));
		}
	}
}