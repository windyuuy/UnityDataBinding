
using System;
using Unity.VisualScripting;

public class InvokeAction1 : Unit
{
    [DoNotSerialize]
    public ControlInput inputTrigger;

    [DoNotSerialize]
    public ControlOutput outputTrigger;

    [DoNotSerialize] // No need to serialize ports
    public ValueInput value;

    [DoNotSerialize]
    public ValueInput ation;

    [DoNotSerialize] // No need to serialize ports
    public ValueOutput result;

    protected override void Definition()
    {
        ation = ValueInput<Action<object>>(nameof(ation));
        value = ValueInput<object>(nameof(value));
        inputTrigger = ControlInput("call", (flow) => {
            var callbackValue = flow.GetValue<Action<object>>(ation);
            if(callbackValue != null)
            {
                var valueValue = flow.GetValue(value);
                callbackValue(valueValue);
            }
            return outputTrigger;
        });

        outputTrigger = ControlOutput("goon");

        result = ValueOutput(nameof(result), (flow) =>
        {
            var value1=flow.GetValue<Action<object>>(value);
            return value1;
        });

        Succession(inputTrigger, outputTrigger);

    }

}
