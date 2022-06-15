﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace WorkerHarness.Core
{
    public class Expression
    {
        // the value of the expression; could contain an object variable name ${...} and several string variable names @{...}
        private string _expression;

        // true if all variables within _expression has been resolve
        private bool _resolved = false;

        public bool Resolved => _resolved;

        // a dependency here is the name of the variable that the expression uses
        private IList<string> _dependencies;

        // an object variable that the expression may depend on.
        private object? _objectVariable;

        // TODO: to be deleted, for debugging
        public IList<string> Dependencies => _dependencies;

        // TODO: to be deleted, for debugging
        public string Value => _expression;

        /// <summary>
        /// constructor for an Expression object. Assume that an expression has been validated before being passed to the constructor
        /// </summary>
        /// <param name="expression" cref="string">a valid expression</param>
        public Expression(string expression)
        {
            _expression = expression;

            // use VariableHelper to extract the variable name
            _dependencies = VariableHelper.ExtractVariableNames(_expression);

            _resolved = !_dependencies.Any();
        }

        /// <summary>
        /// Update the expression with the given variable value
        /// </summary>
        /// <param name="variableName" cref="string">variable name</param>
        /// <param name="variableValue" cref="string">variable value</param>
        public bool TryResolve(string variableName, object variableValue)
        {
            // update the expression with the variable 
            if (_dependencies.Contains(variableName))
            {
                if (variableValue is string variableValueInString) // if the variable is a string variable, update the expression
                {
                    _expression = VariableHelper.ResolveStringVariable(variableName, variableValueInString, _expression);
                }
                else // if the variable is an object variable, buffer it
                {
                    _objectVariable = variableValue;
                }

                // attemp to resolve the object variable in _expression
                _expression = VariableHelper.ResolveObjectVariable(variableName, _objectVariable, _expression);
                // _resolved is true if the expression contains no variables
                _resolved = !VariableHelper.ContainVariables(_expression);
                // discard the _objectVariable if _resolved
                _objectVariable = _resolved ? null : _objectVariable;
            }

            return _resolved;
        }

        /// <summary>
        /// return true and set value if the expression is resolved, false otherwise.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryEvaluate(out string? value)
        {
            if (_resolved)
            {
                value = _expression;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
    }
}
