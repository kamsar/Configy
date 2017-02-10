using System.Linq;
using System.Xml;

namespace Configy.Parsing
{
	public class XmlInheritanceEngine : IXmlInheritanceEngine
	{
		public XmlElement ProcessInheritance(XmlElement source, XmlElement target)
		{
			var result = new XmlDocument();
			result.LoadXml(source.OuterXml);

			foreach(XmlElement currentTargetNode in target.ChildNodes.OfType<XmlElement>())
			{
				var resultNode = result.SelectSingleNode($"{result.DocumentElement.Name}/{currentTargetNode.Name}");

				// Type registration is only on target, so add to output
				if (resultNode == null)
				{
					result.DocumentElement.AppendChild(result.ImportNode(currentTargetNode, true));
					continue; // we can skip children as we added any that existed
				}

				// Type registration is in both source and target. 

				// if target type registration has a `type` attribute we clear all inherited attributes and children as we've changed the dep registration type
				// so the target takes over everything
				if (currentTargetNode.Attributes?["type"] != null)
				{
					resultNode.InnerXml = null;
					resultNode.Attributes?.RemoveAll();
				}

				// Patch in attributes from target.
				foreach (XmlAttribute targetAttribute in currentTargetNode.Attributes)
				{
					var resultAttribute = resultNode.Attributes[targetAttribute.Name];

					// attribute is only in target, so copy to output
					if (resultAttribute == null)
					{
						var newAttribute = result.CreateAttribute(targetAttribute.Name);
						newAttribute.InnerText = targetAttribute.InnerText;

						resultNode.Attributes.Append(newAttribute);
					}
					else // attribute is in both, so target overrides source
					{
						resultAttribute.InnerText = targetAttribute.InnerText;
					}
				}

				ProcessTypeChildren(resultNode, currentTargetNode);
			}

			return result.DocumentElement;
		}

		protected virtual void ProcessTypeChildren(XmlNode resultNode, XmlNode targetNode)
		{
			foreach (XmlNode child in targetNode.ChildNodes)
			{
				resultNode.AppendChild(resultNode.OwnerDocument.ImportNode(child, true));
			}
		}
	}
}
