
using UnityEngine;

namespace gcc.resloader
{
    /**
	 * 预制体动态加载工具
	 */
    public class TCCResLoader : ResLoader<GameObject>
    {
        public static readonly TCCResLoader Inst = new TCCResLoader();
    }

}
