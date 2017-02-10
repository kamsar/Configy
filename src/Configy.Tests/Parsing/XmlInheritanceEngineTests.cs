using System.Xml;
using Configy.Parsing;
using FluentAssertions;
using Xunit;

namespace Configy.Tests.Parsing
{
	public class XmlInheritanceEngineTests
	{
		[Fact]
		public void ProcessInheritance_IsOkayWithComments()
		{
			var source = CreateTestNode(@"<config><!-- haha --><element type=""foo"" /><!-- lul --></config>");
			var target = CreateTestNode(@"<config><!-- kek --><element type=""foo"" /><!-- lawl --></config>");

			var sut = new XmlInheritanceEngine();

			var result = sut.ProcessInheritance(source, target);

			result.OuterXml.Should().Be(@"<config><!-- haha --><element type=""foo"" /><!-- lul --></config>");
		}

		[Fact]
		public void ProcessInheritance_ShouldPassBaseElement()
		{
			var source = CreateTestNode(@"<config><element type=""foo"" /></config>");
			var target = CreateTestNode("<config />");

			var sut = new XmlInheritanceEngine();

			var result = sut.ProcessInheritance(source, target);

			result.OuterXml.Should().Be(@"<config><element type=""foo"" /></config>");
		}

		[Fact]
		public void ProcessInheritance_ShouldPassChildElement()
		{
			var source = CreateTestNode("<config />");
			var target = CreateTestNode(@"<config><element type=""foo"" /></config>");

			var sut = new XmlInheritanceEngine();

			var result = sut.ProcessInheritance(source, target);

			result.OuterXml.Should().Be(@"<config><element type=""foo"" /></config>");
		}

		[Fact]
		public void ProcessInheritance_ShouldAddAttributeWhenPatched()
		{
			var source = CreateTestNode(@"<config><element bonkers=""foo"" /></config>");
			var target = CreateTestNode(@"<config><element monkeys=""bars"" /></config>");

			var sut = new XmlInheritanceEngine();

			var result = sut.ProcessInheritance(source, target);

			result.OuterXml.Should().Be(@"<config><element bonkers=""foo"" monkeys=""bars"" /></config>");
		}

		[Fact]
		public void ProcessInheritance_ShouldOverrideAttributeWhenPatched()
		{
			var source = CreateTestNode(@"<config><element type=""foo"" /></config>");
			var target = CreateTestNode(@"<config><element type=""bars"" /></config>");

			var sut = new XmlInheritanceEngine();

			var result = sut.ProcessInheritance(source, target);

			result.OuterXml.Should().Be(@"<config><element type=""bars"" /></config>");
		}

		[Fact]
		public void ProcessInheritance_ShouldRemoveSourceAttributes_WhenTypeIsPatched()
		{
			var source = CreateTestNode(@"<config><element type=""foo"" baz=""baz"" /></config>");
			var target = CreateTestNode(@"<config><element type=""bars"" /></config>");

			var sut = new XmlInheritanceEngine();

			var result = sut.ProcessInheritance(source, target);

			result.OuterXml.Should().Be(@"<config><element type=""bars"" /></config>");
		}

		[Fact]
		public void ProcessInheritance_ShouldRemoveSourceChildren_WhenTypeIsPatched()
		{
			var source = CreateTestNode(@"<config><element type=""foo"" baz=""baz""><cfg /></element></config>");
			var target = CreateTestNode(@"<config><element type=""bars"" /></config>");

			var sut = new XmlInheritanceEngine();

			var result = sut.ProcessInheritance(source, target);

			result.OuterXml.Should().Be(@"<config><element type=""bars""></element></config>");
		}

		[Fact]
		public void ProcessInheritance_ShouldAppendSourceChild_WhenInTypeParameters()
		{
			var source = CreateTestNode(@"<config><element type=""foo""><cfg /></element></config>");
			var target = CreateTestNode(@"<config><element><cfg /></element></config>");

			var sut = new XmlInheritanceEngine();

			var result = sut.ProcessInheritance(source, target);

			// note that no checking of type params is done - it's a pure append if overridden, unless the parent changes the type attribute.
			result.OuterXml.Should().Be(@"<config><element type=""foo""><cfg /><cfg /></element></config>");
		}

		private XmlElement CreateTestNode(string xml)
		{
			var doc = new XmlDocument();
			doc.LoadXml(xml);

			return doc.DocumentElement;
		}
	}
}
