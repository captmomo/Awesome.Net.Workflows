using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Awesome.Net.Workflows.Contexts;
using Awesome.Net.Workflows.Expressions;
using Awesome.Net.Workflows.Models;
using Microsoft.Extensions.Localization;

namespace Awesome.Net.Workflows.Activities
{
    public class SetOutputTask : TaskActivity
    {
        public override LocalizedString Category => T["Primitives"];

        public string OutputName
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public WorkflowExpression<object> Value
        {
            get => GetExpressionProperty<object>();
            set => SetProperty(value);
        }

        public override IEnumerable<Outcome> GetPossibleOutcomes(WorkflowExecutionContext workflowContext,
            ActivityExecutionContext activityContext)
        {
            return Outcomes(T["Done"]);
        }

        public override async Task<ActivityExecutionResult> ExecuteAsync(WorkflowExecutionContext workflowContext,
            ActivityExecutionContext activityContext)
        {
            var value = await ExpressionEvaluator.EvaluateAsync(Value, workflowContext);
            workflowContext.Output[OutputName] = value;

            return Outcomes("Done");
        }

        public SetOutputTask(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
