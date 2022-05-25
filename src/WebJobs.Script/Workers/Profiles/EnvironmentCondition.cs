// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.Azure.WebJobs.Script.Workers.Profiles;
using Microsoft.Extensions.Logging;

namespace Microsoft.Azure.WebJobs.Script.Workers
{
    // Environment condition checks if environment variables match the expected output
    public class EnvironmentCondition : IWorkerProfileCondition
    {
        private readonly ILogger _logger;
        private readonly IEnvironment _environment;
        private readonly string _name;
        private readonly string _expression;
        private Regex _regex;

        internal EnvironmentCondition(ILogger logger, IEnvironment environment, WorkerProfileConditionDescriptor descriptor)
        {
            if (descriptor is null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));

            descriptor.Properties.TryGetValue(WorkerConstants.WorkerDescriptionProfileConditionName, out _name);
            descriptor.Properties.TryGetValue(WorkerConstants.WorkerDescriptionProfileConditionExpression, out _expression);

            Validate();
        }

        public string Name => _name;

        public string Expression => _expression;

        /// <inheritdoc />
        public bool Evaluate()
        {
            string value = _environment.GetEnvironmentVariable(Name);

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            _logger.LogDebug($"Evaluating EnvironmentCondition with value '{value}' and expression '{Expression}'");

            return _regex.IsMatch(value);
        }

        // Validates if condition parametrs meet expected values, fail if they don't
        private void Validate()
        {
            if (string.IsNullOrEmpty(Name))
            {
                throw new ValidationException($"EnvironmentCondition {nameof(Name)} cannot be empty.");
            }

            if (string.IsNullOrEmpty(Expression))
            {
                throw new ValidationException($"EnvironmentCondition {nameof(Expression)} cannot be empty.");
            }

            try
            {
                _regex = new Regex(Expression);
            }
            catch
            {
                throw new ValidationException($"EnvironmentCondition {nameof(Expression)} must be a valid regular expression.");
            }
        }
    }
}