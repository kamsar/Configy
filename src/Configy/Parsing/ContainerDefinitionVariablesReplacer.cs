using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Configy.Parsing
{
	public class ContainerDefinitionVariablesReplacer : IContainerDefinitionVariablesReplacer
	{
		private readonly Dictionary<string, string> _variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public virtual void AddVariable(string name, string value)
		{
			if(_variables.ContainsKey(name)) throw new InvalidOperationException($"The variable {name} has already been defined!");

			_variables.Add(name, value);
		}

		public virtual void ReplaceVariables(ContainerDefinition definition)
		{
			ApplyVariables(definition.Definition, _variables);
		}

		protected virtual void ApplyVariables(XmlElement input, IDictionary<string, string> variables)
		{
			input.InnerXml = ApplyVariables(input.InnerXml, variables);
		}

		protected virtual string ApplyVariables(string input, IDictionary<string, string> variables)
		{
			var value = new StringBuilder(input);

			foreach (var variable in variables)
			{
				value.Replace($"$({variable.Key})", variable.Value);
			}

			return value.ToString();
		}
	}
}
