using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Configy.Parsing
{
	public class XmlVariablesReplacer
	{
		private readonly Dictionary<string, string> _variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public void AddVariable(string name, string value)
		{
			if(_variables.ContainsKey(name)) throw new InvalidOperationException($"The variable {name} has already been defined!");

			_variables.Add(name, value);
		}

		public void ReplaceVariables(XmlNode node)
		{
			node.InnerXml = ApplyVariables(node.InnerXml);
			//var nodeQueue = new Queue<XmlNode>();

			//nodeQueue.Enqueue(node);

			//do
			//{
			//	var currentNode = nodeQueue.Dequeue();

			//	ReplaceVariablesInNode(currentNode);

			//	foreach (XmlNode child in currentNode.ChildNodes)
			//	{
			//		nodeQueue.Enqueue(child);
			//	}
			//}
			//while (nodeQueue.Count > 0);
		}

		protected virtual void ReplaceVariablesInNode(XmlNode node)
		{
			node.InnerText = ApplyVariables(node.InnerText);

			if (node.Attributes == null) return;

			foreach (XmlAttribute attribute in node.Attributes)
			{
				attribute.InnerText = ApplyVariables(attribute.InnerText);
			}
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
