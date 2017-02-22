using System;
using System.Collections.Generic;

namespace Configy.Containers
{
	/// <summary>
	/// Represents a basic IoC container.
	/// </summary>
	public interface IContainer
	{
		/// <summary>
		/// The name of this container, used for display purposes
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The name of a container, if any, which this container extends (inherits)
		/// </summary>
		string Extends { get; }

		/// <summary>
		/// Resolves an instance of a type from a generic parameter. This should be an explicitly registered type.
		/// </summary>
		T Resolve<T>() where T : class;

		/// <summary>
		/// Resolves an instance of a type from the container. The 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		object Resolve(Type type);

		/// <summary>
		/// Registers an instance of a dependency type.
		/// </summary>
		/// <param name="type">Type to register the instance for</param>
		/// <param name="factory">Factory method to create the instance when it is needed</param>
		/// <param name="singleInstance">If true, it's a singleton. If false, new instance is created each time. Singleton is preferable for performance.</param>
		void Register(Type type, Func<object> factory, bool singleInstance);

		/// <summary>
		/// Activates a type, using constructor injection for any registered dependencies. This differs from Resolve because the type being activated need not be registered with the container.
		/// </summary>
		/// <param name="type">The type to activate an instance of. The container does not need to have the type registered.</param>
		/// <param name="unmappedConstructorParameters">An array of constructor parameters which are not registered with the container (e.g. strings, bools, etc). Keys are argument names, values are argument values</param>
		object Activate(Type type, KeyValuePair<string, object>[] unmappedConstructorParameters);

		void Assert(Type type);

		void AssertSingleton(Type type);

		void AssertTransient(Type type);
	}
}
