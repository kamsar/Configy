using System;
using System.Collections.Generic;
using Configy.Containers;
using Xunit;

namespace Configy.Tests.Containers
{
	public class MicroContainerTests
	{
		[Fact]
		public void ResolvesType_WithConstructorDependency()
		{
			var micro = CreateTestContainer();

			micro.Register(typeof(ITest), () => new Test(), true);

			var instance = (IDependency)micro.Activate(typeof(TestDependencyParameter), new KeyValuePair<string, object>[] { });

			Assert.NotNull(instance);

			Assert.NotNull(instance.TestInstance);
		}

		[Fact]
		public void ResolvesType_WithConstructorDependency_AndExtraConstructorParameter()
		{
			var micro = CreateTestContainer();

			micro.Register(typeof(ITest), () => new Test(), true);

			var instance = (IDependency)micro.Activate(typeof(TestDependencyParameterStatic), new[] { new KeyValuePair<string, object>("value", "hello") });

			Assert.NotNull(instance);

			Assert.Equal("hello", ((TestDependencyParameterStatic)instance).Value);
		}

		[Fact]
		public void ResolvesType()
		{
			var micro = CreateTestContainer();

			micro.Register(typeof(ITest), () => new Test(), true);

			var instance = micro.Resolve<ITest>();

			Assert.NotNull(instance);
		}

		[Fact]
		public void ResolvesType_AsInstance()
		{
			var micro = CreateTestContainer();

			micro.Register(typeof(IUniqueTestInstance), () => new UniqueTestInstance(), false);

			var instance = micro.Resolve<IUniqueTestInstance>();

			Assert.NotNull(instance);

			var instance2 = micro.Resolve<IUniqueTestInstance>();

			Assert.NotEqual(instance.InstanceGuid, instance2.InstanceGuid);
		}

		[Fact]
		public void ResolvesType_AsSingleton()
		{
			var micro = CreateTestContainer();

			micro.Register(typeof(IUniqueTestInstance), () => new UniqueTestInstance(), true);

			var instance = micro.Resolve<IUniqueTestInstance>();

			Assert.NotNull(instance);

			var instance2 = micro.Resolve<IUniqueTestInstance>();

			Assert.Equal(instance.InstanceGuid, instance2.InstanceGuid);
		}

		private MicroContainer CreateTestContainer()
		{
			return new MicroContainer("Test", null);
		}
	}

	public interface ITest
	{
		string TestString { get; }
	}

	public class Test : ITest
	{
		public string TestString { get; set; }
	}

	public interface IDependency
	{
		ITest TestInstance { get; }
	}

	public class TestDependencyParameter : IDependency
	{
		public TestDependencyParameter(ITest dependency)
		{
			TestInstance = dependency;
		}

		public ITest TestInstance { get; private set; }
	}

	public class TestDependencyParameterStatic : IDependency
	{
		public TestDependencyParameterStatic(ITest depdendency, string value)
		{
			TestInstance = depdendency;
			Value = value;
		}

		public ITest TestInstance { get; private set; }
		public string Value { get; set; }
	}

	public interface IUniqueTestInstance
	{
		Guid InstanceGuid { get; }
	}

	public class UniqueTestInstance : IUniqueTestInstance
	{
		public UniqueTestInstance()
		{
			InstanceGuid = Guid.NewGuid();
		}

		public Guid InstanceGuid { get; private set; }
	}
}
