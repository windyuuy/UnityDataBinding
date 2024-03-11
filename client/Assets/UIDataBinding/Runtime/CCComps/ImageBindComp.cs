using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DataBinding.UIBind
{
	[AddComponentMenu("DataDrive/ImageBind")]
	public class ImageBindComp : DataBindCompBase
	{
		[SerializeField] [Rename("主属性")] protected string key = "";

		[SerializeField] protected Image target;

		/**
		 * 更新显示状态
		 */
		protected override void OnBindItems()
		{
			if (!string.IsNullOrEmpty(this.key))
			{
				// 设立优先级
				var ret = this.CheckSprite();
			}
		}

		[HideInInspector] public string spriteTextureUrl;

		protected virtual bool CheckSprite()
		{
			if (target == null)
			{
				target = this.GetComponent<Image>();
			}

			if (target == null)
			{
				return false;
			}

			this.WatchValueChange<string>(this.key, (newValue, oldValue) =>
			{
				if (target != null && newValue is string sNewValue && this.spriteTextureUrl != sNewValue)
				{
					this.spriteTextureUrl = sNewValue;
					this.LoadImage(sNewValue, target);
				}
			});
			return true;
		}

		private Coroutine _loadingImageCo;

		protected IEnumerator LoadImageCo(Image target0, string url)
		{
			UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
			yield return www.SendWebRequest();
			if (www.result == UnityWebRequest.Result.ConnectionError ||
			    www.result == UnityWebRequest.Result.ProtocolError)
			{
				Debug.Log(www.error);
			}

			Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;
			var rectTransform = (RectTransform)target0.transform;
			var size = rectTransform.sizeDelta;
			Sprite sprite = Sprite.Create(tex, new Rect(0, 0, size.x, size.y),
				new Vector2(0.5f, 0.5f));
			target0.sprite = sprite; //image 是 UI Image
		}

		protected virtual void LoadImage(string url, Image target0)
		{
			if (string.IsNullOrWhiteSpace(url))
			{
				target0.sprite = null;
				return;
			}

			if (url.StartsWith("http"))
			{
				if (_loadingImageCo != null)
				{
					StopCoroutine(_loadingImageCo);
				}

				_loadingImageCo = this.StartCoroutine(LoadImageCo(target0, url));
			}
			else
			{
				Resources.LoadAsync<Sprite>(url).completed += (ret) =>
				{
					if (ret is ResourceRequest ret2)
					{
						Sprite sprite = ret2.asset as Sprite;

						var image = target0;
						if (image != null)
						{
							image.sprite = sprite;
						}
					}
				};
			}
		}
	}
}