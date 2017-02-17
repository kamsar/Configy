using System;
using System.Xml;
using Configy.Containers;
using Configy.Parsing;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Configy.Tests
{
	public class XmlContainerBuilderTests
	{
		[Fact]
		public void GetContainer_ShouldThrowIfNoTypeIsPassed()
		{
			var def = CreateTestDefinition($"<foo singleInstance=\"true\" />");

			var sut = CreateTestBuilder();

			Assert.Throws<InvalidOperationException>(() => sut.GetContainer(def));
		}

		[Fact]
		public void GetContainer_ShouldThrowIfInvalidTypeIsPassed()
		{
			var def = CreateTestDefinition($"<foo type=\"Lol, Lol\" singleInstance=\"true\" />");

			var sut = CreateTestBuilder();

			Assert.Throws<InvalidOperationException>(() => sut.GetContainer(def));
		}

		[Fact]
		public void GetContainer_ShouldRegisterExpectedTypeInterfaces()
		{
			var type = typeof(XmlContainerBuilderTestMultiInterfaceDependency).AssemblyQualifiedName;

			var def = CreateTestDefinition($"<foo type=\"{type}\" singleInstance=\"true\" />");

			var sut = CreateTestBuilder();

			var container = sut.GetContainer(def);

			container.Resolve<IBuilderDependency1>().Should().BeOfType<XmlContainerBuilderTestMultiInterfaceDependency>();
			container.Resolve<IBuilderDependency2>().Should().BeOfType<XmlContainerBuilderTestMultiInterfaceDependency>();
		}

		[Fact]
		public void GetContainer_ShouldActivateTypeWithStringCtorParam()
		{
			var type = typeof(XmlContainerBuilderTestStringCtorParamDepdendency).AssemblyQualifiedName;

			var def = CreateTestDefinition($"<foo type=\"{type}\" foo=\"bar\" singleInstance=\"true\" />");

			var sut = CreateTestBuilder();

			var container = sut.GetContainer(def);

			container.Resolve<IBuilderDependency1>().Should().BeOfType<XmlContainerBuilderTestStringCtorParamDepdendency>();
		}

		[Fact]
		public void GetContainer_ShouldActivateTypeWithMissingCtorParam()
		{
			var type = typeof(XmlContainerBuilderTestStringCtorParamDepdendency).AssemblyQualifiedName;

			var def = CreateTestDefinition($"<foo type=\"{type}\" singleInstance=\"true\" />");

			var sut = CreateTestBuilder();

			var container = sut.GetContainer(def);

			Assert.Throws<MicroResolutionException>(() => container.Resolve<IBuilderDependency1>());
		}

		[Fact]
		public void GetContainer_ShouldActivateTypeWithBoolCtorParam()
		{
			var type = typeof(XmlContainerBuilderTestBoolCtorParamDepdendency).AssemblyQualifiedName;

			var def = CreateTestDefinition($"<foo type=\"{type}\" foo=\"true\" singleInstance=\"true\" />");

			var sut = CreateTestBuilder();

			var container = sut.GetContainer(def);

			container.Resolve<IBuilderDependency1>().Should().BeOfType<XmlContainerBuilderTestBoolCtorParamDepdendency>();
		}

		[Fact]
		public void GetContainer_ShouldNotActivateTypeWithInvalidBoolCtorParam()
		{
			var type = typeof(XmlContainerBuilderTestBoolCtorParamDepdendency).AssemblyQualifiedName;

			var def = CreateTestDefinition($"<foo type=\"{type}\" foo=\"LOLOL\" singleInstance=\"true\" />");

			var sut = CreateTestBuilder();

			var container = sut.GetContainer(def);

			Assert.Throws<InvalidCastException>(() => container.Resolve<IBuilderDependency1>());
		}

		[Fact]
		public void GetContainer_ShouldActivateTypeWithIntCtorParam()
		{
			var type = typeof(XmlContainerBuilderTestIntCtorParamDepdendency).AssemblyQualifiedName;

			var def = CreateTestDefinition($"<foo type=\"{type}\" foo=\"32768\" singleInstance=\"true\" />");

			var sut = CreateTestBuilder();

			var container = sut.GetContainer(def);

			container.Resolve<IBuilderDependency1>().Should().BeOfType<XmlContainerBuilderTestIntCtorParamDepdendency>();
		}

		[Fact]
		public void GetContainer_ShouldNotActivateTypeWithInvalidIntCtorParam()
		{
			var type = typeof(XmlContainerBuilderTestIntCtorParamDepdendency).AssemblyQualifiedName;

			var def = CreateTestDefinition($"<foo type=\"{type}\" foo=\"LOLOL\" singleInstance=\"true\" />");

			var sut = CreateTestBuilder();

			var container = sut.GetContainer(def);

			Assert.Throws<InvalidCastException>(() => container.Resolve<IBuilderDependency1>());
		}

		[Fact]
		public void GetContainers_RegistersExpectedContainers()
		{
			var type = typeof(XmlContainerBuilderTestMultiInterfaceDependency).AssemblyQualifiedName;

			var defs = new[]
			{
				CreateTestDefinition($"<foo type=\"{type}\" singleInstance=\"true\" />"),
				CreateTestDefinition($"<foo type=\"{type}\" singleInstance=\"true\" />")
			};

			var sut = CreateTestBuilder();

			sut.GetContainers(defs).Should().HaveCount(2);
		}

		[Fact]
		public void GetContainers_IgnoresAbstractContainers()
		{
			var type = typeof(XmlContainerBuilderTestMultiInterfaceDependency).AssemblyQualifiedName;

			var defs = new[]
			{
				CreateTestDefinition($"<foo type=\"{type}\" singleInstance=\"true\" />", true),
				CreateTestDefinition($"<foo type=\"{type}\" singleInstance=\"true\" />")
			};

			var sut = CreateTestBuilder();

			sut.GetContainers(defs).Should().HaveCount(1);
		}

		[Fact]
		public void GetContainer_AppliesVariables()
		{
			var type = typeof(XmlContainerBuilderTestMultiInterfaceDependency).AssemblyQualifiedName;

			// this is an integration test. shut up :P
			var replacer = new ContainerDefinitionVariablesReplacer();
			replacer.AddVariable("lmao", type);

			var def = CreateTestDefinition($"<foo type=\"$(lmao)\" singleInstance=\"true\" />");

			var sut = CreateTestBuilder(replacer);

			sut.GetContainer(def).Resolve<IBuilderDependency1>().Should().BeOfType<XmlContainerBuilderTestMultiInterfaceDependency>();
		}

		private ContainerDefinition CreateTestDefinition(string dependencyDefinitionXml, bool @abstract = false)
		{
			var doc = new XmlDocument();
			doc.LoadXml($"<configuration name=\"test\" abstract=\"{@abstract}\">{dependencyDefinitionXml}</configuration>");

			return new ContainerDefinition(doc.DocumentElement);
		}

		private XmlContainerBuilder CreateTestBuilder(IContainerDefinitionVariablesReplacer testVariablesReplacer = null)
		{
			return new XmlContainerBuilder(testVariablesReplacer ?? Substitute.For<IContainerDefinitionVariablesReplacer>());
		}
	}
}
