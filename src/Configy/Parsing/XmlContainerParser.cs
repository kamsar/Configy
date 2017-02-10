using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace Configy.Parsing
{
	// Parses all containers up into individual container nodes,
	// and applies inheritance, resulting in fully processed XML for the XmlContainerBuilder to turn into IContainers.
	public class XmlContainerParser
	{
		private const int MaxInheritanceIterations = 5000;
		private readonly XmlElement _configurationsNode;
		private readonly XmlElement _baseContainerNode;
		private readonly IXmlInheritanceEngine _xmlInheritanceEngine;

		public XmlContainerParser(XmlElement configurationsNode, XmlElement baseContainerNode, IXmlInheritanceEngine xmlInheritanceEngine)
		{
			_configurationsNode = configurationsNode;
			_baseContainerNode = baseContainerNode;
			_xmlInheritanceEngine = xmlInheritanceEngine;
		}

		public IList<ContainerDefinition> GetContainers()
		{
			var containers = _configurationsNode.ChildNodes
				.OfType<XmlElement>()
				.Where(element => element.Name.Equals("configuration"))
				.Select(element => new ContainerDefinition(element))
				.ToArray();

			var nameChecker = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (var container in containers)
			{
				if (string.IsNullOrWhiteSpace(container.Name)) throw new InvalidOperationException("At least one container had no name assigned. Each container must have a unique name.");

				if (nameChecker.Contains(container.Name)) throw new InvalidOperationException($"The container '{container.Name}' is defined twice. Containers should have unique names.");

				nameChecker.Add(container.Name);
			}
			
			// order the containers in inheritance order
			var orderedContainers = OrderBySpecificity(containers);
			var containerLookup = orderedContainers.ToDictionary(container => container.Name, StringComparer.Ordinal);

			foreach (var container in orderedContainers)
			{
				if (string.IsNullOrWhiteSpace(container.Extends))
				{
					// apply base container inheritance
					container.Definition = _xmlInheritanceEngine.ProcessInheritance(_baseContainerNode, container.Definition);
					continue;
				}

				// apply extends inheritance
				container.Definition = _xmlInheritanceEngine.ProcessInheritance(containerLookup[container.Extends].Definition, container.Definition);
			}

			return orderedContainers;
		}

		protected virtual IList<ContainerDefinition> OrderBySpecificity(IEnumerable<ContainerDefinition> containers)
		{
			var processQueue = new Queue<ContainerDefinition>(containers);

			var added = new HashSet<string>();
			var result = new List<ContainerDefinition>();
			int iterationCount = 0;

			while (processQueue.Count > 0 && iterationCount < MaxInheritanceIterations)
			{
				iterationCount++;

				var current = processQueue.Dequeue();

				// extends for the current item not added. Push it back onto the queue, and we'll pick it up later.
				if (!string.IsNullOrWhiteSpace(current.Extends) && !added.Contains(current.Extends))
				{
					processQueue.Enqueue(current);
					continue;
				}

				result.Add(current);
				added.Add(current.Name);
			}

			if (iterationCount == MaxInheritanceIterations) throw new InvalidOperationException("There is an extends inheritance loop, or a container extending a nonexistant container. Unresolved containers probably at fault: " + string.Join(", ", processQueue.Select(container => container.Name)));

			return result.ToArray();
		}
	}
}
