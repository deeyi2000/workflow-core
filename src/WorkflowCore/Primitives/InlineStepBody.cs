using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace WorkflowCore.Primitives
{
    public class InlineStepBody : StepBody
    {
        public dynamic Input { get; set; } = new System.Dynamic.ExpandoObject();
        public dynamic Output { get; set; } = new System.Dynamic.ExpandoObject();

        private readonly Func<InlineStepBody, IStepExecutionContext, ExecutionResult> _body;

        public InlineStepBody(Func<InlineStepBody, IStepExecutionContext, ExecutionResult> body)
        {
            _body = body;
        }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            return _body.Invoke(this, context);
        }
    }
}
