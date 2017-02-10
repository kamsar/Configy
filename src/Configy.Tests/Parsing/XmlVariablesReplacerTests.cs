using System.Xml;
using Configy.Parsing;
using FluentAssertions;
using Xunit;

namespace Configy.Tests.Parsing
{
	public class XmlVariablesReplacerTests
	{
		[Fact]
		public void ReplaceVariables_ShouldNotReplaceRootAttributeVariable()
		{
			var data = CreateTestNode(@"<b var=""$(var)""></b>");

			var sut = new XmlVariablesReplacer();
			sut.AddVariable("var", "baz");

			sut.ReplaceVariables(data);

			data.Attributes["var"].InnerText.Should().Be("$(var)");
		}

		[Fact]
		public void ReplaceVariables_ShouldReplaceBodyVariable()
		{
			var data = CreateTestNode(@"<b>$(var)</b>");

			var sut = new XmlVariablesReplacer();
			sut.AddVariable("var", "baz");

			sut.ReplaceVariables(data);

			data.InnerText.Should().Be("baz");
		}

		[Fact]
		public void ReplaceVariables_ShouldReplaceAttributeVariable_OnChildElement()
		{
			var data = CreateTestNode(@"<a><b><c var=""$(var)""></c></b></a>");

			var sut = new XmlVariablesReplacer();
			sut.AddVariable("var", "baz");

			sut.ReplaceVariables(data);

			data.SelectSingleNode("/a/b/c").Attributes["var"].InnerText.Should().Be("baz");
		}

		[Fact]
		public void ReplaceVariables_ShouldReplaceBodyVariable_OnChildElement()
		{
			var data = CreateTestNode(@"<a><b>$(var)</b></a>");

			var sut = new XmlVariablesReplacer();
			sut.AddVariable("var", "baz");

			sut.ReplaceVariables(data);

			data.SelectSingleNode("/a/b").InnerText.Should().Be("baz");
		}

		private XmlNode CreateTestNode(string xml)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);

			return doc.DocumentElement;
		}
	}
}
