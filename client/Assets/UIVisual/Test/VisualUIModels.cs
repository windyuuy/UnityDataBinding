
using Unity.VisualScripting;

public class GetUIModels : Unit
{

	[DoNotSerialize]
	public ValueOutput sharedUIModels;

	protected override void Definition()
	{
		sharedUIModels = ValueOutput<TestData>("commonUIModels", (flow) => TestData.Inst);
	}
}

