
using Unity.VisualScripting;
using DataBind;

public class WatchImpulse : Unit
{
    [DoNotSerialize]
    public ControlInput inputTrigger;

    [DoNotSerialize]
    public ControlOutput outputEvalTrigger;

    [DoNotSerialize]
    public ControlOutput outputTrigger;

    [DoNotSerialize]
    public ValueInput host;

    protected GraphReference inputFlowRef = null;
    protected DataBind.VM.Watcher watcher;

    protected override void Definition()
    {
        outputEvalTrigger = ControlOutput("eval");

        inputTrigger = ControlInput("input", (flow) => {
            if (watcher != null)
            {
                watcher.teardown();
                watcher = null;
            }
            var inputHost = flow.GetValue<IStdHost>(host);
            var flowRefer = flow.stack.ToReference();
            inputFlowRef = flowRefer;
            watcher = inputHost.Watch((host, env) =>
            {
                // �˴��ض�����һ��
                EvalValue(host, host);
                return null;
            }, (host, newValue, oldValue) =>
            {
                Trigger(flowRefer, newValue, outputTrigger);
            });
            return outputTrigger;
        });
        outputTrigger = ControlOutput("output");

        host = ValueInput<IStdHost>("host");

        Succession(inputTrigger, outputTrigger);

    }

    public void EvalValue(object host, object env)
    {
        if (inputFlowRef != null)
        {
            //Trigger(inputFlowRef, host);
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

        Run(flow, outputTrigger);
    }

    protected virtual bool ShouldTrigger(Flow flow, object args)
    {
        return true;
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
