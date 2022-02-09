using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using DataBinding.CollectionExt;


public class myData
{
    public string myselfname;
}

public class test : MonoBehaviour
{

    //每一个UI都必须有一个数据观测对象
    //现在这个test就相当于是主的UI
    //它就需要一个datahost
    public GameObject img;
    public GameObject btn;
    public Toggle     toggle;
    public Slider     slider;
    public ScrollRect  scroll;



    //测试按钮

    public Button     Testbtn1;
    public Button     Testbtn2;
    public Button     Testbtn3;
    public Button     Testbtn4;
    public Button     Testbtn5;
    public Button     Testbtn6;
    public Button     Testbtn7;


    public Text text;

    private bool isselect = false;


    System.Random  _rand = new System.Random();

    List<string> RandomImagePath  = new List<string>();

    List<UnityAction> callbacks  = new List<UnityAction>();


    List<string> RandomTestPath  = new List<string>();

    //UI的观测数据结构,相当于是UI数据结构
     CommentOB   uiData  = new CommentOB();

     DataHostCompent _dataHost;


    void Start()
    {

        _dataHost  = transform.GetComponent<DataHostCompent>();

        uiData.intListData = new List<myData>();

        //在数据观测组件中来注册一下UI的数据对象

        _dataHost.registerData(uiData);

        // uiData = geneTestData();
        // 每一个UI都必须只能一个
        RandomImagePath.Add("bg_growup_pic_sticker_01");
        RandomImagePath.Add("bg_growup_pic_sticker_02");
        RandomImagePath.Add("bg_growup_pic_sticker_03");
        RandomImagePath.Add("bg_growup_pic_sticker_04");
        RandomImagePath.Add("bg_growup_pic_sticker_05");

        RandomTestPath.Add("11111");
        RandomTestPath.Add("22222");
        RandomTestPath.Add("33333");
        RandomTestPath.Add("44444");
        RandomTestPath.Add("55555");

        Testbtn1.onClick.AddListener(()=>{
                var zz = _rand.Next(0,5);
                uiData.imgepath =   RandomImagePath[zz];
                print("当前的纹理是 ： "+RandomImagePath[zz]);
            });


        callbacks.Add(()=>{Debug.Log("我是函数1");});
        callbacks.Add(()=>{Debug.Log("我是函数2");});
        callbacks.Add(()=>{Debug.Log("我是函数3");});
        callbacks.Add(()=>{Debug.Log("我是函数4");});
        Testbtn2.onClick.AddListener(()=>{
            var zz = _rand.Next(0,4);
            uiData.callBack =   callbacks[zz];   
        });


        Testbtn3.onClick.AddListener(()=>{
            var zz = _rand.Next(0,5);
            uiData.textname =   RandomTestPath[zz];

        });


        Testbtn4.onClick.AddListener(()=>{
            isselect = !isselect;
            uiData.isSelect = isselect;     
        });


        Testbtn5.onClick.AddListener(()=>{
            var zz = _rand.Next(0,10);
            uiData.progress = zz/10f ;     
        });


        Testbtn6.onClick.AddListener(()=>{
            var zz = _rand.Next(4,10);
            var kk = _rand.Next(0,100);
            List<myData> lisy = new List<myData>();
            for(int i=0;i<zz;i++)
            {
                myData temp = new myData();
                temp.myselfname = (i+kk).ToString();
                lisy.Add(temp);
            }
            uiData.intListData = lisy; 
        });



        Testbtn7.onClick.AddListener(()=>{
            var zz = _rand.Next(0,uiData.intListData.Count);
            var kk = _rand.Next(0,6);

            uiData.intListData[2].myselfname = (100+kk).ToString();
            //print("kkkkkkkkkkkkkkkkkkkkkkkkkk  "+"   "+uiData.intListData[zz].myselfname);

        });


        // int i = 0;
        // for (; i < 5; i++)
        // {
        //     text2 = Instantiate<GameObject>(text1.gameObject).GetComponent<Text>();
        //     text2.transform.SetParent(scroll.content,false);



        //     var zz = _rand.Next(0,5);
        //     text2.GetComponent<TextBind>().upadeTextData(RandomImagePath[zz]);
            
        // }



        // text2 = Instantiate<GameObject>(text1.gameObject).GetComponent<Text>();
        // text2.transform.SetParent(text1.transform,false);

        // Testbtn.onClick.AddListener(()=>{
        //     var zz = _rand.Next(0,5);
        //     text.GetComponent<TextBind>().upadeTextData(RandomImagePath[zz]);     
        // });

    }
}
