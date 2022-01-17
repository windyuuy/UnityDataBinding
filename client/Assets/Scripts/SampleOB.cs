
// ��Ҫ����DataBinding�����ռ�
using DataBinding;
// Ŀǰֻ֧�ּ���DataBinding.CollectionExt�е���������󣬲�֧��ϵͳ�������������磺System.Collection.Generic.List��System.Collection.Generic.Dictionary����
using DataBinding.CollectionExt;

// ��Ҫ���Observable���ԣ�ʹĿ���Ϊ�ɹ۲����
[Observable]
public class SampleOB
{
    // ע�⣺����ʹ�������ֶ�
    public double KKK { get; set; } = 234;

    // DataBinding.CollectionExt.List ��������
    // ע�⣺����ʹ�������ֶ�
    public List<int> IntList { get; set; } = new List<int> { 1, 2, 3, 4 };

    // DataBinding.CollectionExt.Dictionary ��������
    // ע�⣺����ʹ�������ֶ�
    public Dictionary<double, string> NumDictionary { get; set; } = new Dictionary<double, string>();
}
