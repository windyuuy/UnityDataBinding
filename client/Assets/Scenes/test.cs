using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DataBinding.CollectionExt;
using System;


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
    public Button     Testbtn8;
    public Button     Testbtn9;
    public Button     Testbtn10;
    public Button     Testbtn11;
     public Button     Testbtn12;

    public Button     Testbtn13;
    public Button     Testbtn14;


    public Text text;

    private bool isselect = false;


    System.Random  _rand = new System.Random();

    List<string> RandomImagePath  = new List<string>();

    List<Action> callbacks  = new List<Action>();


    List<string> RandomTestPath  = new List<string>();

    List<string> names  = new List<string>();
    List<float> progress  = new List<float>();
    List<bool> togglse  = new List<bool>();


    //UI的观测数据结构,相当于是UI数据结构
     CommentOB   uiData;

     DataHostCompent _dataHost;


    public void createUITestData()
    {
        uiData   = new CommentOB();
        uiData.imgepath = "bg_growup_pic_sticker_01";
        uiData.textname = "test";
        uiData.age = 10;
        uiData.progress = 0.1f;
        uiData.callBack = ()=>{print("测试的初始化函数");};
        uiData.isSelect = false;
        uiData.intListData = new List<myData>();
        uiData.listDictionary = new  Dictionary<int, myData>();

        //测试的随机化数据

        RandomImagePath.Add("bg_growup_pic_sticker_01");
        RandomImagePath.Add("bg_growup_pic_sticker_02");
        RandomImagePath.Add("bg_growup_pic_sticker_03");
        RandomImagePath.Add("bg_growup_pic_sticker_04");
        RandomImagePath.Add("bg_growup_pic_sticker_05");
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
        RandomTestPath.Add("66666");
        RandomTestPath.Add("77777");
        RandomTestPath.Add("88888");
        RandomTestPath.Add("99999");
        RandomTestPath.Add("101010");

        callbacks.Add(()=>{Debug.Log("我是函数1");});
        callbacks.Add(()=>{Debug.Log("我是函数2");});
        callbacks.Add(()=>{Debug.Log("我是函数3");});
        callbacks.Add(()=>{Debug.Log("我是函数4");});
        callbacks.Add(()=>{Debug.Log("我是函数5");});
        callbacks.Add(()=>{Debug.Log("我是函数6");});
        callbacks.Add(()=>{Debug.Log("我是函数7");});
        callbacks.Add(()=>{Debug.Log("我是函数8");});
        callbacks.Add(()=>{Debug.Log("我是函数9");});
        callbacks.Add(()=>{Debug.Log("我是函数10");});

        names.Add("王一");
        names.Add("王二");
        names.Add("王三");
        names.Add("王四");
        names.Add("王五");
        names.Add("王六");
        names.Add("王七");
        names.Add("王八");
        names.Add("王九");
        names.Add("王十");


        progress.Add(0.1f);
        progress.Add(0.5f);
        progress.Add(0.6f);
        progress.Add(0.7f);
        progress.Add(0.8f);
        progress.Add(0.9f);
        progress.Add(0.15f);
        progress.Add(0.55f);
        progress.Add(0.65f);
        progress.Add(0.75f);

        togglse.Add(false);
        togglse.Add(true);
        togglse.Add(false);
        togglse.Add(true);
        togglse.Add(false);
        togglse.Add(true);
        togglse.Add(false);
        togglse.Add(true);
        togglse.Add(false);
        togglse.Add(true);

        //注意初始化时UI的值一般也都是有值的
        var num = 1;
        for(int i=0;i<num;i++)
        {
            myData temp = new myData();
            temp.myheadList = new List<myShowData>();
            temp.myselfname = i.ToString();
            for(int j=0;j<num;j++)
            {
                myShowData temp_1 = new myShowData();
                temp_1.myheadimg = RandomImagePath[j];
                temp_1.myheadname = names[j];
                temp_1.myheadprogress = progress[j];
                temp_1.myheadcallBack = callbacks[j];
                temp_1.myheadhaveselect = togglse[j];
                temp.myheadList.Add(temp_1);
            }
            uiData.listDictionary.Add(i,temp);
        }
    }

    void Start()
    {

        createUITestData();
        _dataHost  = transform.GetComponent<DataHostCompent>();
        //将UI数据注册进UI的数据观测者组件中
        _dataHost.registerData(uiData);




        //--------------下面是非容器的测试------------------------------------
        Testbtn1.onClick.AddListener(()=>{
                var zz = _rand.Next(0,5);
                uiData.imgepath =   RandomImagePath[zz];
                print("当前的纹理是 ： "+RandomImagePath[zz]);
            });


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



        //--------------下面是简单容器的测试------------------------------------{object}
        Testbtn6.onClick.AddListener(()=>{
            var zz = _rand.Next(2,6);
            var kk = _rand.Next(0,100);
            List<myData> lisy = new List<myData>();
            print("zhe changdu is :"+zz);
            for(int i=0;i<zz;i++)
            {
                myData temp = new myData();
                temp.myselfname = (i).ToString();
                lisy.Add(temp);
            }
            uiData.intListData = lisy; 
            //_dataHost.registerData(uiData);
        });



        Testbtn7.onClick.AddListener(()=>{
            var zz = _rand.Next(0,uiData.intListData.Count);
            var kk = _rand.Next(0,6);

            uiData.intListData[1].myselfname = (100+kk).ToString();
            //print("kkkkkkkkkkkkkkkkkkkkkkkkkk  "+"   "+uiData.intListData[zz].myselfname);
        });


        //--------------下面是复合容器的测试------------------------------------{{object},{object}……}
        //产生随机长度的容器数据
        Testbtn8.onClick.AddListener(()=>{
            var zz =_rand.Next(2,4);
            var kk = _rand.Next(1,3);
            Dictionary<int, myData> lisy = new Dictionary<int, myData>();
            print("字典的长度为 :"+zz);
            print("子列表的长度为 :"+kk);
            for(int i=0;i<zz;i++)
            {
                myData temp = new myData();
                temp.myheadList = new List<myShowData>();
                temp.myselfname = i.ToString();
                for(int j=0;j<kk;j++)
                {
                    myShowData temp_1 = new myShowData();
                    temp_1.myheadimg = RandomImagePath[j];
                    temp_1.myheadname = names[j];
                    temp_1.myheadprogress = progress[j];
                    temp_1.myheadcallBack = callbacks[j];
                    temp_1.myheadhaveselect = togglse[j];
                    temp.myheadList.Add(temp_1);
                }
                lisy.Add(i,temp);
            }
            uiData.listDictionary = lisy;
        });

        //观测复合列表中的某一项的内容
        Testbtn9.onClick.AddListener(()=>{
            var zz = 0;
            var kk = _rand.Next(0,10);
            uiData.listDictionary[zz].myheadList[zz].myheadimg = RandomImagePath[kk];
            uiData.listDictionary[zz].myheadList[zz].myheadname = names[kk];
            uiData.listDictionary[zz].myheadList[zz].myheadprogress = progress[kk];
            uiData.listDictionary[zz].myheadList[zz].myheadcallBack = callbacks[kk];
            uiData.listDictionary[zz].myheadList[zz].myheadhaveselect = togglse[kk];
            //print("kkkkkkkkkkkkkkkkkkkkkkkkkk  "+"   "+uiData.intListData[zz].myselfname);
        });


        //观测列表中的所有项目
        Testbtn10.onClick.AddListener(()=>{
            print("kkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkkk---> "+uiData.listDictionary.Count);
            for(int i= 0;i<uiData.listDictionary.Count;i++)
            {
                for(int j= 0;j<uiData.listDictionary[i].myheadList.Count;j++)
                {
                    var kk = _rand.Next(0,10);
                    uiData.listDictionary[i].myheadList[j].myheadimg = RandomImagePath[kk];
                    uiData.listDictionary[i].myheadList[j].myheadname = names[kk];
                    uiData.listDictionary[i].myheadList[j].myheadprogress = progress[kk];
                    uiData.listDictionary[i].myheadList[j].myheadcallBack = callbacks[kk];
                    uiData.listDictionary[i].myheadList[j].myheadhaveselect = togglse[kk];
                }
            }
        });

        //删除容器中的一项[一般的删除和增加都是在列表的尾部]
        Testbtn11.onClick.AddListener(()=>{
            var kk = _rand.Next(0,uiData.listDictionary.Count);
            if(uiData.listDictionary[kk]!=null && uiData.listDictionary[kk].myheadList!=null && uiData.listDictionary[kk].myheadList.Count>0)
            {
                uiData.listDictionary[kk].myheadList.RemoveAt(uiData.listDictionary[kk].myheadList.Count-1);
            }
        });

        //增加容器中的一项[一般的删除和增加都是在列表的尾部]
        Testbtn12.onClick.AddListener(()=>{
            var kk = _rand.Next(0,uiData.listDictionary.Count);

            var zz = 1;//_rand.Next(1,2);
            if(uiData.listDictionary[kk]!=null && uiData.listDictionary[kk].myheadList!=null && uiData.listDictionary[kk].myheadList.Count>0)
            {
                for(int i=0;i<zz;i++)
                {
                    var pp = _rand.Next(0,10);
                    myShowData temp_1 = new myShowData();
                    temp_1.myheadimg = RandomImagePath[pp];
                    temp_1.myheadname = names[pp];
                    temp_1.myheadprogress = progress[pp];
                    temp_1.myheadcallBack = callbacks[pp];
                    temp_1.myheadhaveselect = togglse[pp];
                    uiData.listDictionary[kk].myheadList.Add(temp_1);
                }
            }
        });


        //删除容器中的一行[一般的删除和增加都是在列表的尾部]
        Testbtn13.onClick.AddListener(()=>{
            
            if(uiData.listDictionary!=null && uiData.listDictionary.Count>0)
            {
                uiData.listDictionary.Remove(uiData.listDictionary.Count-1);
            }
        });


        //增加容器中的一行[一般的删除和增加都是在列表的尾部]
        Testbtn14.onClick.AddListener(()=>{
            var zz = 1;//_rand.Next(1,2);
            for(int i=0;i<zz;i++)
            {
                myData temp = new myData();
                temp.myheadList = new List<myShowData>();
                temp.myselfname = i.ToString();
                for(int j=0;j<zz+2;j++)
                {
                    myShowData temp_1 = new myShowData();
                    temp_1.myheadimg = RandomImagePath[j];
                    temp_1.myheadname = names[j];
                    temp_1.myheadprogress = progress[j];
                    temp_1.myheadcallBack = callbacks[j];
                    temp_1.myheadhaveselect = togglse[j];
                    temp.myheadList.Add(temp_1);
                }
                uiData.listDictionary.Add(uiData.listDictionary.Count,temp);
            }  
            
        });
    }
}
