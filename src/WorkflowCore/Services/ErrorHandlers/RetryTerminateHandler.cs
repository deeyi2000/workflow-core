using System;
using System.Collections.Generic;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Models.LifeCycleEvents;

namespace WorkflowCore.Services.ErrorHandlers
{
    public class RetryTerminateHandler : IWorkflowErrorHandler
    {
        private readonly ILifeCycleEventPublisher _eventPublisher;
        private readonly IDateTimeProvider _datetimeProvider;
        private readonly WorkflowOptions _options;
        public WorkflowErrorHandling Type => WorkflowErrorHandling.RetryTerminate;

        public RetryTerminateHandler(ILifeCycleEventPublisher eventPublisher, IDateTimeProvider datetimeProvider, WorkflowOptions options)
        {
            _eventPublisher = eventPublisher;
            _datetimeProvider = datetimeProvider;
            _options = options;
        }

        public void Handle(WorkflowInstance workflow, WorkflowDefinition def, ExecutionPointer pointer, WorkflowStep step, Exception exception, Queue<ExecutionPointer> bubbleUpQueue)
        {
            pointer.RetryCount++;
            if(pointer.RetryCount >= 5)
            {
                workflow.Status = WorkflowStatus.Terminated;
                _eventPublisher.PublishNotification(new WorkflowTerminated
                {
                    EventTimeUtc = _datetimeProvider.UtcNow,
                    Reference = workflow.Reference,
                    WorkflowInstanceId = workflow.Id,
                    WorkflowDefinitionId = workflow.WorkflowDefinitionId,
                    Version = workflow.Version
                });
            }
            else
            {
                pointer.SleepUntil = _datetimeProvider.UtcNow.Add((TimeSpan)(step.RetryInterval ?? def.DefaultErrorRetryInterval));
                step.PrimeForRetry(pointer);
            }
        }
    }
}
