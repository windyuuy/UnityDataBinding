using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class test : MonoBehaviour
{
    public GameObject img;
    public GameObject btn;
    public Button     Testbtn;
    public Toggle     toggle;
    public Slider     slider;
    public ScrollRect  scroll;

    public Text text;


    public Text text1;

    private Text text2;

    private bool isselect = false;

    System.Random  _rand = new System.Random();

    List<string> RandomImagePath  = new List<string>();

    List<UnityAction> callbacks  = new List<UnityAction>();
    
    void Start()
    {
        RandomImagePath.Add("bg_growup_pic_sticker_01");
        RandomImagePath.Add("bg_growup_pic_sticker_02");
        RandomImagePath.Add("bg_growup_pic_sticker_03");
        RandomImagePath.Add("bg_growup_pic_sticker_04");
        RandomImagePath.Add("bg_growup_pic_sticker_05");
        // Testbtn.onClick.AddListener(()=>{
        //     var zz = _rand.Next(0,5);
        //     img.GetComponent<ImgeBind>().upadeImageData(RandomImagePath[zz]);     
        // });


        callbacks.Add(()=>{Debug.Log("我是函数1");});
        callbacks.Add(()=>{Debug.Log("我是函数2");});
        callbacks.Add(()=>{Debug.Log("我是函数3");});
        callbacks.Add(()=>{Debug.Log("我是函数4");});
        // Testbtn.onClick.AddListener(()=>{
        //     var zz = _rand.Next(0,4);
        //     btn.GetComponent<ButtonBind>().upadeButtonData(callbacks[zz]);     
        // });


        // Testbtn.onClick.AddListener(()=>{
        //     var zz = _rand.Next(0,5);
        //     text.GetComponent<TextBind>().upadeTextData(RandomImagePath[zz]);  

        // });


        // Testbtn.onClick.AddListener(()=>{
        //     isselect = !isselect;
        //     toggle.GetComponent<ToggleBind>().upadeToggleData(isselect);     
        // });


        // Testbtn.onClick.AddListener(()=>{
        //     var zz = _rand.Next(0,10);
        //     slider.GetComponent<ProgressBind>().upadeProgressData((float)zz/10);     
        // });


        Testbtn.onClick.AddListener(()=>{
            var zz = _rand.Next(0,10);
            var kk = _rand.Next(0,100);
            List<int> lisy = new List<int>();
            for(int i=0;i<zz;i++)
            {
                lisy.Add(i+kk);
            }
            print("uuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuuu");
            scroll.GetComponent<ContainerBind>().upadeScrollViewData(lisy);     
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


    // Update is called once per frame
    void Update()
    {
        
    }
}
