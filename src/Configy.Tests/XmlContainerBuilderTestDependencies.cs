using System;

namespace Configy.Tests
{
	public class XmlContainerBuilderTestMultiInterfaceDependency : IBuilderDependency1, IBuilderDependency2
	{
	}

	public class XmlContainerBuilderTestStringCtorParamDepdendency : IBuilderDependency1
	{
		public XmlContainerBuilderTestStringCtorParamDepdendency(string foo)
		{
			if(foo == null) throw new ArgumentException();
		}
	}

	public class XmlContainerBuilderTestBoolCtorParamDepdendency : IBuilderDependency1
	{
		public XmlContainerBuilderTestBoolCtorParamDepdendency(bool foo)
		{
		}
	}

	public class XmlContainerBuilderTestIntCtorParamDepdendency : IBuilderDependency1
	{
		public XmlContainerBuilderTestIntCtorParamDepdendency(int foo)
		{
		}
	}

	public interface IBuilderDependency1
	{
		
	}

	public interface IBuilderDependency2
	{
		
	}
}
