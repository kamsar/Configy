using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Configy.Containers
{
	public class MicroContainer : IContainer
	{
		private readonly ConcurrentDictionary<Type, Lazy<object>> _singletons = new ConcurrentDictionary<Type, Lazy<object>>();
		private readonly ConcurrentDictionary<Type, Func<object>> _transients = new ConcurrentDictionary<Type, Func<object>>();

		public MicroContainer(string name, string extends)
		{
			Name = name;
			Extends = extends;
		}

		public string Name { get; }
		public string Extends { get; }

		/// <summary>
		/// Resolves a type with its dependencies. If the type is not registered, it is activated using Activate() automatically.
		/// </summary>
		/// <typeparam name="T">Type to resolve</typeparam>
		/// <returns></returns>
		public virtual T Resolve<T>() where T : class
		{
			var typeOfT = typeof(T);

			var result = Resolve(typeOfT);

			if (result == null)
			{
				if(typeOfT.IsClass)
					return (T)Activate(typeOfT, new KeyValuePair<string, object>[] { });

				return null;
			}

			return (T)result;
		}

		public virtual object Resolve(Type type)
		{
			Lazy<object> value;
			if (_singletons.TryGetValue(type, out value))
			{
				return value.Value;
			}

			Func<object> factory;
			if (_transients.TryGetValue(type, out factory))
			{
				return factory();
			}

			return null;
		}

		/// <summary>
		/// Adds a type to the container
		/// </summary>
		/// <param name="type">The type to register</param>
		/// <param name="factory">A factory function that creates the type instance to resolve to</param>
		/// <param name="singleInstance">If true, the object is a singleton. Otherwise the factory is called every time it is resolved.</param>
		public virtual void Register(Type type, Func<object> factory, bool singleInstance)
		{
			if (singleInstance)
			{
				_singletons.TryAdd(type, new Lazy<object>(factory));
			}
			else
			{
				_transients.TryAdd(type, factory);
			}
		}

		public virtual void Assert(Type type)
		{
			if (_singletons.ContainsKey(type) || _transients.ContainsKey(type)) return;

			throw new InvalidOperationException($"The expected type {type.FullName} was not registered with the container.");
		}

		public virtual void AssertSingleton(Type type)
		{
			if (_singletons.ContainsKey(type)) return;

			if(_transients.ContainsKey(type)) throw new InvalidOperationException($"{type.FullName} was registered with the {Name} container but it was expected to be a singleton and was not (singleInstance=false or undefined).");

			throw new InvalidOperationException($"The expected type {type.FullName} was not registered with the {Name} container.");
		}

		public virtual void AssertTransient(Type type)
		{
			if (_transients.ContainsKey(type)) return;

			if (_singletons.ContainsKey(type)) throw new InvalidOperationException($"{type.FullName} was registered with the {Name} container but it was expected to be a transient and was not (singleInstance=true).");

			throw new InvalidOperationException($"The expected type {type.FullName} was not registered with the {Name} container.");
		}

		/// <summary>
		/// Creates an instance of an object, injecting any registered dependencies in the configuration into its constructor.
		/// </summary>
		/// <param name="type">Type to resolve</param>
		/// <param name="unmappedConstructorParameters">Constructor parameters that are not expected to be in the configuration</param>
		public virtual object Activate(Type type, KeyValuePair<string, object>[] unmappedConstructorParameters)
		{
			var constructors = type.GetConstructors();
			if (constructors.Length > 1) throw new MicroResolutionException($"Cannot construct {type.FullName} because it has > 1 public constructor.");
			if (constructors.Length == 0) throw new MicroResolutionException($"Cannot construct {type.FullName} because it has no constructor!");

			var constructor = constructors.First();

			var ctorParams = constructor.GetParameters();

			object[] args = new object[ctorParams.Length];

			for (int parameterIndex = 0; parameterIndex < ctorParams.Length; parameterIndex++)
			{
				var currentParam = ctorParams[parameterIndex];

				if (unmappedConstructorParameters.Any(kv => kv.Key.Equals(currentParam.Name, StringComparison.Ordinal)))
				{
					args[parameterIndex] = unmappedConstructorParameters.First(kv => kv.Key.Equals(currentParam.Name, StringComparison.Ordinal)).Value;
				}
				else
				{
					args[parameterIndex] = Resolve(currentParam.ParameterType);
					if (args[parameterIndex] == null)
					{
						try
						{
							args[parameterIndex] = Activate(currentParam.ParameterType, new KeyValuePair<string, object>[] { });
						}
						catch (Exception ex)
						{
							throw new MicroResolutionException($"Cannot activate {type.FullName}, constructor param '{currentParam.Name}' ({currentParam.ParameterType.Name}). The type '{currentParam.ParameterType.Name}' is probably not registered, or may need to be an explicit unmapped parameter (as an XML attribute on the type registration). Inner message: {ex.Message}");
						}
					}
				}
			}

			var lambdaParams = Expression.Parameter(typeof(object[]), "parameters");
			var newParams = new Expression[ctorParams.Length];

			for (int i = 0; i < ctorParams.Length; i++)
			{
				var paramsParameter = Expression.ArrayIndex(lambdaParams, Expression.Constant(i));

				newParams[i] = Expression.Convert(paramsParameter, ctorParams[i].ParameterType);
			}

			var newExpression = Expression.New(constructor, newParams);

			var constructionLambda = Expression.Lambda(typeof(Func<object[], object>), newExpression, lambdaParams);

			return ((Func<object[], object>)constructionLambda.Compile()).Invoke(args);
		}
	}
}
