﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rebus2.Pipeline
{
    public class DefaultPipelineManager : IPipelineManager
    {
        readonly List<RegisteredStep> _sendSteps = new List<RegisteredStep>();
        readonly List<RegisteredStep> _receiveSteps = new List<RegisteredStep>();

        public IEnumerable<IStep> SendPipeline()
        {
            return _sendSteps.Select(s => s.Step);
        }

        public IEnumerable<StagedReceiveStep> ReceivePipeline()
        {
            return _receiveSteps.Select(s => new StagedReceiveStep(s.Step, (ReceiveStage)s.Stage));
        }

        public DefaultPipelineManager OnReceive(IStep step, ReceiveStage stage)
        {
            _receiveSteps.Add(new RegisteredStep(step, (int)stage));
            return this;
        }

        public DefaultPipelineManager OnReceive(Action<StepContext, Func<Task>> step, ReceiveStage stage, string stepDescription = null)
        {
            return OnReceive(new StepContainer(step, stepDescription), stage);
        }

        public class StepContainer : IStep
        {
            readonly Action<StepContext, Func<Task>> _step;
            readonly string _description;

            public StepContainer(Action<StepContext, Func<Task>> step, string description)
            {
                _step = step;
                _description = description;
            }

            public async Task Process(StepContext context, Func<Task> next)
            {
                _step(context, next);
            }

            public override string ToString()
            {
                return string.Format("Step: {0}", _description);
            }
        }

        class RegisteredStep
        {
            public RegisteredStep(IStep step, int stage)
            {
                Step = step;
                Stage = stage;
            }

            public IStep Step { get; private set; }
            public int Stage { get; private set; }
        }
    }
}