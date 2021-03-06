using System;
using System.Collections.Generic;
using System.Linq;
using Awesome.Net.Workflows.Activities;
using Awesome.Net.Workflows.Models;

namespace Awesome.Net.Workflows.FluentBuilders
{
    public class WorkflowBuilder : IWorkflowBuilder
    {
        public IActivityLibrary ActivityLibrary { get; }
        public List<ActivityRecord> Activities { get; set; } = new List<ActivityRecord>();
        public List<Transition> Transitions { get; set; } = new List<Transition>();

        public WorkflowBuilder(IActivityLibrary activityLibrary)
        {
            ActivityLibrary = activityLibrary;
        }

        public IActivityBuilder StartWith<T>(Action<T> setup = null, string id = null) where T : IActivity
        {
            var activityRecord = BuildActivity(default, setup, id);
            activityRecord.IsStart = true;
            var activityBuilder = new ActivityBuilder(this, activityRecord);
            return activityBuilder;
        }

        public ActivityRecord BuildActivity<T>(T activity = default, Action<T> setup = null, string id = null,
            bool addToWorkflow = true) where T : IActivity
        {
            activity = activity == null ? (T)ActivityLibrary.GetActivityByName(typeof(T).Name) : activity;
            setup?.Invoke(activity);
            var activityRecord = ActivityRecord.FromActivity(activity);
            if (id.IsNullOrWhiteSpace())
            {
                activityRecord.ActivityId = $"{RandomHelper.Generate26UniqueId()}";
            }
            else
            {
                var e = Activities.FirstOrDefault(x => x.ActivityId == id);
                if (e != null)
                {
                    throw new ArgumentException($"ActivityId: {id} already exists.", nameof(id));
                }
                else
                {
                    activityRecord.ActivityId = id;
                }
            }

            if (addToWorkflow)
            {
                Activities.Add(activityRecord);
            }

            return activityRecord;
        }

        public WorkflowType Build<T>(Action<WorkflowType> setup = null) where T : IWorkflow, new()
        {
            var workflow = new T();
            workflow.Build(this);
            return Build(workflow, setup);
        }

        public WorkflowType Build(IWorkflow workflow, Action<WorkflowType> setup = null)
        {
            var workflowType = Build(workflow.Name, x =>
             {
                 x.Id = workflow.Id;
                 x.WorkflowTypeId = workflow.WorkflowTypeId;
                 x.Name = workflow.Name;
                 x.IsEnabled = workflow.IsEnabled;
                 x.IsSingleton = workflow.IsSingleton;
                 x.DeleteFinishedWorkflows = workflow.DeleteFinishedWorkflows;

                 setup?.Invoke(x);
             });

            return workflowType;
        }

        public WorkflowType Build(string name, Action<WorkflowType> setup = null)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            var workflowType = new WorkflowType()
            {
                Id = Guid.NewGuid(),
                WorkflowTypeId = RandomHelper.Generate26UniqueId(),
                Name = name,
                IsEnabled = true,
                IsSingleton = false,
                DeleteFinishedWorkflows = false,
                Activities = Activities,
                Transitions = CleanupTransitions()
            };

            setup?.Invoke(workflowType);

            return workflowType;
        }

        // Remove invalid transitions
        private List<Transition> CleanupTransitions()
        {
            var transitions = Transitions.Where(x => !x.DestinationActivityId.IsNullOrEmpty());
            var validTransitions = new List<Transition>();
            foreach (var transition in transitions)
            {
                var isValidTransition = Activities.Any(x => x.ActivityId == transition.DestinationActivityId);
                if (isValidTransition)
                {
                    validTransitions.Add(transition);
                }
            }

            return validTransitions;
        }
    }
}
