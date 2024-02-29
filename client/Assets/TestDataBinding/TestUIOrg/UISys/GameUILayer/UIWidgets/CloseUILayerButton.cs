
using gcc.layer;
using UnityEngine;
using UnityEngine.UI;
using Node = UnityEngine.Transform;

namespace UISys.Runtime
{
    [RequireComponent(typeof(Button))]
    public class CloseUILayerButton : MonoBehaviour
    {
        public Node dialogRoot;

        /**
         * 获取当前组件所在节点的host
         */
        public Node FindLayerRoot()
        {
            Node node = this.transform;

            try
            {
                while (node != null && null == node.GetComponent<UILayer>())
                {
                    node = node.parent;
                }
            }
            catch (System.Exception err)
            {
                Debug.LogError(err);
            }

            return node;
        }

        void Awake()
        {
            if (this.dialogRoot == null)
            {
                var root = this.FindLayerRoot();
                this.dialogRoot = root;
            }

            if (this.dialogRoot)
            {
                var button = this.GetComponent<Button>();
                if (button)
                {
                    button.onClick.AddListener(() =>
                    {
                        var uiLayer = this.dialogRoot.GetComponent<UILayer>();
                        if (uiLayer != null)
                        {

                            Debug.Log("[c] 关闭对话");
                            //uiLayer.DoClose();
                            uiLayer.Dispose();
                        }
                    });
                }
            }
        }
    }
}

