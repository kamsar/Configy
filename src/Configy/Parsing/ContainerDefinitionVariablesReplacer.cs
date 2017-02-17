using System;
using System.Collections.Generic;
using System.Text;

namespace Configy.Parsing
{
	public class ContainerDefinitionVariablesReplacer : IContainerDefinitionVariablesReplacer
	{
		private readonly Dictionary<string, string> _variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public void AddVariable(string name, string value)
		{
			if(_variables.ContainsKey(name)) throw new InvalidOperationException($"The variable {name} has already been defined!");

			_variables.Add(name, value);
		}

		public void ReplaceVariables(ContainerDefinition definition)
		{
			definition.Definition.InnerXml = ApplyVariables(definition.Definition.InnerXml);
		}

		protected virtual string ApplyVariables(string input)
		{
			var value = new StringBuilder(input);

			foreach (var variable in _variables)
			{
				value.Replace($"$({variable.Key})", variable.Value);
			}

			return value.ToString();
		}
	}
}
