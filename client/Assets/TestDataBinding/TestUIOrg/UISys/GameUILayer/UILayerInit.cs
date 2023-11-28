using System.Collections;
using System.Collections.Generic;

public class UILayerInit
{

    protected UILayerRoot layerRoot;

    public void Init(UILayerRoot layerRoot)
    {
        this.layerRoot = layerRoot;

        LoomMG.Init();
        this.initLayerMG();
    }

    public void setupUILoadingHandler()
    {
        // let loadingHandler = new LoadingHandler().init();
        // LayerMG.addLoadingHandler(loadingHandler);
        // NetLoadingHandler.setDelegate(loadingHandler);
    }

    /**
	 * 创建图层群体关系
	 */
    protected void setupLayerBundles()
    {
        // layerRoot.LayerBundle.SetupBundles(new Dictionary<string, gcc.layer.LayerBundleInputItem[]>
        // {
        // 	{"BGameScene",new gcc.layer.LayerBundleInputItem[]{"DGameScene", "DGameGround",}},
        // });
        layerRoot.LayerBundle.SetupBundles(new Dictionary<string, gcc.layer.LayerBundleInputItem[]>{
            { "BHome",new gcc.layer.LayerBundleInputItem[] {
                "MainScene/DMainScene",
                "Chapter/ChapterView",
                "Mission/MissionLayer",
                "Home/DHomeView",
                "CharacterName/NameLayer",
			}},
        });

        layerRoot.LayerBundle.SetupBundles(new Dictionary<string, gcc.layer.LayerBundleInputItem[]>{
            { "BLevel",new gcc.layer.LayerBundleInputItem[] {
                "NormalScene/DNormalScene",
                "Bubble/BubbleLayer",
                "Joystick/Joystick",
                "Mission/MissionLayer",
                "Home/DHomeView",
                "CharacterName/NameLayer",
                "GuideWarnText/GuideWarnTextLayer",
			}},
        });

        layerRoot.LayerBundle.SetupBundles(new Dictionary<string, gcc.layer.LayerBundleInputItem[]>{
            { "BMain",new gcc.layer.LayerBundleInputItem[] {
                "MainScene/DMainScene",
                "Bubble/BubbleLayer",
                "Joystick/Joystick",
                "Mission/MissionLayer",
                "Home/DHomeView",
                "CharacterName/NameLayer",
			}},
        });

        layerRoot.LayerBundle.SetupBundles(new Dictionary<string, gcc.layer.LayerBundleInputItem[]>{
            { "BSpeak",new gcc.layer.LayerBundleInputItem[] {
                "Speaking/Speaking",
                "FaceExpress/FaceLayer",
                "Speaking/SpeakingSprite",
                "Speaking/SpeakingCombo",
                "Speaking/SpeakingLoudly",
                "Speaking/SpeakingFlower",
			}},
        });
        // layerRoot.LayerBundle.SetupBundles(new Dictionary<string, gcc.layer.LayerBundleInputItem[]>{
        //     { "BLogin",new gcc.layer.LayerBundleInputItem[] {
        //         "LoginFlow/UI/AgreementUI",
        //         "LoginFlow/UI/LoginSuccessUI",
        //     }},
        // });
        // layerRoot.LayerBundle.SetupBundles(new Dictionary<string, gcc.layer.LayerBundleInputItem[]>{
        // 	{ "BMainScenePages",new gcc.layer.LayerBundleInputItem[] { "BHome",}},
        // });
        // layerRoot.LayerBundle.SetRecordBundle(gcc.layer.LayerBundle.DefaultBundleName);
    }

    /**
	 * 加载图层框架并执行初始设置
	 */
    protected void initLayerMG()
    {
        Main.UILayerRoot = layerRoot.Init();
        var LayerMG = layerRoot.LayerManager;
        // 注册对话框主类
        LayerMG.RegisterLayerClass("CCLayerComp", typeof(UILayer));
        // 构建图层顺序
        // 支持填写Uri和资源Uri辅助排序
        LayerMG.LayerOrderMG.SetupTagOrders(new string[] {
            LayerTags.Back,
            LayerTags.Normal,
            LayerTags.Barrier,
            LayerTags.WorldSpaceUI,
            LayerTags.WorldSpaceUI2,
            LayerTags.Pop,
            LayerTags.Transfer,
            LayerTags.DSureDialog,
            LayerTags.Info
        });
        // 创建图层群体关系
        this.setupLayerBundles();
    }

    protected void initDebugInfo()
    {
        // if (DevInfo.showFPS)
        // {
        // 	profiler.showStats()
        // 		}
        // else
        // {
        // 	profiler.hideStats()
        // 		}
    }

}
