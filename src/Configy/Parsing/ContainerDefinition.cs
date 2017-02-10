using System;
using System.Xml;

namespace Configy.Parsing
{
	public class ContainerDefinition
	{
		public ContainerDefinition(XmlElement definition)
		{
			Definition = definition;
		}

		public XmlElement Definition { get; set; }

		public string Name => Definition.Attributes?["name"]?.InnerText;

		public string Extends => Definition.Attributes?["extends"]?.InnerText;

		public bool Abstract => Definition.Attributes?["abstract"]?.InnerText.Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase) ?? false;
	}
}
