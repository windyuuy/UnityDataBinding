
using Unity.VisualScripting;
using DataBinding;

public class WatchGraph: Unit
{
    [DoNotSerialize]
    public ControlInput inputTrigger;

    [DoNotSerialize]
    public ControlOutput outputEvalTrigger;

    [DoNotSerialize]
    public ControlOutput outputTrigger;

    [DoNotSerialize]
    public ValueOutput onEvalDone;

    [DoNotSerialize]
    public ValueInput host;

    [DoNotSerialize]
    public ValueOutput result;

    [DoNotSerialize]
    public object resultValue;

    protected GraphReference inputFlowRef=null;
    protected vm.Watcher watcher;

    protected override void Definition()
    {
        System.Action<object> callbackValue = (object newValue) =>
        {
            resultValue = newValue;
        };
        onEvalDone = ValueOutput("evalSignal", (flow) =>
        {
            return callbackValue;
        });
        outputEvalTrigger = ControlOutput("eval");
        inputTrigger = ControlInput("input", (flow) => {
            if (watcher != null)
            {
                watcher.teardown();
                watcher = null;
            }
            var inputHost = flow.GetValue<IStdHost>(host);
            if(inputHost != null)
            {
                var flowRefer = flow.stack.ToReference();
                inputFlowRef = flowRefer;
                watcher = inputHost.Watch((host, env) =>
                {
                    // 此处必定运行一次
                    EvalValue(host, host);
                    return resultValue;
                }, (host, newValue, oldValue) =>
                {
                    Trigger(flowRefer, newValue, outputTrigger);
                });
            }
            return outputTrigger;
        });
        outputTrigger = ControlOutput("output");

        host = ValueInput<IStdHost>("host");

        result = ValueOutput(nameof(result), (flow) =>
        {
            return resultValue;
        });

        Succession(inputTrigger, outputEvalTrigger);
        Succession(inputTrigger, outputTrigger);

    }

    public void EvalValue(object host, object env)
    {
        if (inputFlowRef != null)
        {
            var flow = Flow.New(inputFlowRef);
            Run(flow, outputEvalTrigger);
        }
    }


    public void Trigger(GraphReference reference, object args, ControlOutput outputTrigger)
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

    private void Run(Flow flow, ControlOutput outputTrigger)
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
