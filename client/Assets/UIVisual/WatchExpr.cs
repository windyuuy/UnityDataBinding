
using Unity.VisualScripting;
using DataBinding;
using System.Collections.Generic;

using TExpr = vm.CombineType<object, string, System.Func<object, object, object>>;

public class WatchExpr : Unit
{
    [DoNotSerialize]
    public ControlInput inputTrigger;

    [DoNotSerialize]
    public ControlOutput outputTrigger;

    [DoNotSerialize]
    public ValueInput expr;

    [DoNotSerialize]
    public ValueInput host;

    [DoNotSerialize]
    public ValueOutput result;

    protected vm.Watcher watcher;

    protected override void Definition()
    {
        inputTrigger = ControlInput("inputTrigger", (flow) => {
            if (watcher != null)
            {
                watcher.teardown();
                watcher = null;
            }
            var inputHost=flow.GetValue<IStdHost>(host);
            if(inputHost != null)
            {
                var inputExpr = flow.GetValue(expr);
                var flowRefer = flow.stack.ToReference();
                watcher = inputHost.Watch(inputExpr, (host, newValue, oldValue) =>
                {
                    Trigger(flowRefer, outputTrigger, newValue);
                });
            }
            return outputTrigger;
        });
        outputTrigger = ControlOutput("outputTrigger");

        expr = ValueInput<TExpr>("expr");
        host = ValueInput<IStdHost>("host");

        result = ValueOutput(nameof(result), (flow) =>
        {
            if(watcher != null)
            {
                var v = watcher.value;
                return v;
            }
            return null;
        });

        Succession(inputTrigger, outputTrigger);

    }


    public void Trigger(GraphReference reference, ControlOutput outputTrigger, object args)
    {
        var flow = Flow.New(reference);

        if (!ShouldTrigger(flow, args))
        {
            flow.Dispose();
            return;
        }

        AssignArguments(flow, args);

        Run(flow, outputTrigger);
    }

    protected virtual bool ShouldTrigger(Flow flow, object args)
    {
        return true;
    }

    protected virtual void AssignArguments(Flow flow, object args)
    {
        flow.SetValue(result, args);
    }

    private void Run(Flow flow,ControlOutput outputTrigger)
    {
        if (flow.enableDebug)
        {
            var editorData = flow.stack.GetElementDebugData<IUnitDebugData>(this);

            editorData.lastInvokeFrame = EditorTimeBinding.frame;
            editorData.lastInvokeTime = EditorTimeBinding.time;
        }

        {
            flow.Run(outputTrigger);
        }
    }

}
