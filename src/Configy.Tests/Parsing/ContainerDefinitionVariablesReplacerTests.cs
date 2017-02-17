using System.Xml;
using Configy.Parsing;
using FluentAssertions;
using Xunit;

namespace Configy.Tests.Parsing
{
	public class ContainerDefinitionVariablesReplacerTests
	{
		[Fact]
		public void ReplaceVariables_ShouldNotReplaceRootAttributeVariable()
		{
			var data = CreateTestDefinition(@"<b var=""$(var)""></b>");

			var sut = new ContainerDefinitionVariablesReplacer();
			sut.AddVariable("var", "baz");

			sut.ReplaceVariables(data);

			data.Definition.Attributes["var"].InnerText.Should().Be("$(var)");
		}

		[Fact]
		public void ReplaceVariables_ShouldReplaceBodyVariable()
		{
			var data = CreateTestDefinition(@"<b>$(var)</b>");

			var sut = new ContainerDefinitionVariablesReplacer();
			sut.AddVariable("var", "baz");

			sut.ReplaceVariables(data);

			data.Definition.InnerText.Should().Be("baz");
		}

		[Fact]
		public void ReplaceVariables_ShouldReplaceAttributeVariable_OnChildElement()
		{
			var data = CreateTestDefinition(@"<a><b><c var=""$(var)""></c></b></a>");

			var sut = new ContainerDefinitionVariablesReplacer();
			sut.AddVariable("var", "baz");

			sut.ReplaceVariables(data);

			data.Definition.SelectSingleNode("/a/b/c").Attributes["var"].InnerText.Should().Be("baz");
		}

		[Fact]
		public void ReplaceVariables_ShouldReplaceBodyVariable_OnChildElement()
		{
			var data = CreateTestDefinition(@"<a><b>$(var)</b></a>");

			var sut = new ContainerDefinitionVariablesReplacer();
			sut.AddVariable("var", "baz");

			sut.ReplaceVariables(data);

			data.Definition.SelectSingleNode("/a/b").InnerText.Should().Be("baz");
		}

		private ContainerDefinition CreateTestDefinition(string xml)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);

			return new ContainerDefinition(doc.DocumentElement);
		}
	}
}
