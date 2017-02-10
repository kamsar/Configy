using System;
using System.Xml;
using Configy.Parsing;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Configy.Tests.Parsing
{
	public class XmlContainerParserTests
	{
		[Fact]
		public void GetContainers_ReturnsExpectedContainer()
		{
			var input = @"<configuration name=""Foo"" />";
			var sut = CreateContainerParser(input, string.Empty);

			sut.GetContainers().Should().HaveCount(1).And.ContainSingle(definition => definition.Name == "Foo");
		}

		[Fact]
		public void GetContainers_ReturnsExpectedContainerOrder_WhenExtending()
		{
			var input = @"<configuration name=""Foo"" extends=""Bar"" /><configuration name=""Bar"" />";
			var sut = CreateContainerParser(input, string.Empty);

			var result = sut.GetContainers();
			
			result.Should().HaveCount(2);
			result[0].Should().Match<ContainerDefinition>(container => container.Name == "Bar");
			result[1].Should().Match<ContainerDefinition>(container => container.Name == "Foo");
		}

		[Fact]
		public void GetContainers_Throws_WhenExtendLoop()
		{
			var input = @"<configuration name=""Foo"" extends=""Bar"" /><configuration name=""Bar"" extends=""Foo"" />";
			var sut = CreateContainerParser(input, string.Empty);

			Assert.Throws<InvalidOperationException>(() => sut.GetContainers());
		}

		[Fact]
		public void GetContainers_Throws_WhenDuplicateNames()
		{
			var input = @"<configuration name=""Foo"" /><configuration name=""Foo"" />";
			var sut = CreateContainerParser(input, string.Empty);

			Assert.Throws<InvalidOperationException>(() => sut.GetContainers());
		}

		[Fact]
		public void GetContainers_Throws_WhenBlankName()
		{
			var input = @"<configuration name="""" />";
			var sut = CreateContainerParser(input, string.Empty);

			Assert.Throws<InvalidOperationException>(() => sut.GetContainers());
		}

		[Fact]
		public void GetContainers_Throws_WhenNoName()
		{
			var input = @"<configuration />";
			var sut = CreateContainerParser(input, string.Empty);

			Assert.Throws<InvalidOperationException>(() => sut.GetContainers());
		}

		private XmlContainerParser CreateContainerParser(string inputXml, string baseXml)
		{
			var input = new XmlDocument();
			input.LoadXml("<configurations>" + inputXml + "</configurations>");

			var baseDoc = new XmlDocument();
			baseDoc.LoadXml("<defaults>" + baseXml + "</defaults>");

			var fauxEngine = Substitute.For<IXmlInheritanceEngine>();
			fauxEngine.ProcessInheritance(Arg.Any<XmlElement>(), Arg.Any<XmlElement>()).Returns(info => info.ArgAt<XmlElement>(1));

			return new XmlContainerParser(input.DocumentElement, baseDoc.DocumentElement, fauxEngine);
		}
	}
}
