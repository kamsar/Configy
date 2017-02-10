using System.Xml;

namespace Configy.Parsing
{
	public interface IXmlInheritanceEngine
	{
		XmlElement ProcessInheritance(XmlElement source, XmlElement target);
	}
}
