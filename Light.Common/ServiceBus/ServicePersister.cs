using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Light.Bus.Common.NewFolder;
using Light.Common;

namespace Light.Bus.Common
{
	public static class ServicePersister
	{
		private static readonly ConcurrentDictionary<Type, MethodInfo[]> ServiceMethods =
			new ConcurrentDictionary<Type, MethodInfo[]>();

		private static readonly ConcurrentDictionary<string, Lazy<Type>> OperationTypes =
			new ConcurrentDictionary<string, Lazy<Type>>();

		private static readonly ConcurrentDictionary<string, Lazy<MethodInfo>> OperationHandlers =
			new ConcurrentDictionary<string, Lazy<MethodInfo>>();

		private static readonly ConcurrentDictionary<string, Lazy<List<MethodInfo>>> EventHandlers =
			new ConcurrentDictionary<string, Lazy<List<MethodInfo>>>();

		public static Type[] GetServiceTypes(List<Assembly> assemblies)
		{
			var result = new List<Type>();
			foreach (var assembly in assemblies)
			{
				var componentTypes = GetServiceTypes(assembly);
				result.AddRange(componentTypes);
			}

			return result.ToArray();
		}

		public static Type[] GetServiceTypes(Assembly assembly)
		{
			var result = new List<Type>();

			var componentTypes = assembly.GetTypes()
				.Where(it => !it.IsAbstract && it.IsSubclassOf(typeof(ServiceComponent)))
				.ToArray();

			foreach (var item in componentTypes)
			{
				var methods = item.GetMethods().Where(m => m.GetCustomAttributes(typeof(MessageHandlerAttribute), true).Length > 0).ToArray();

				ServiceMethods.TryAdd(item, methods);
			}

			result.AddRange(componentTypes);

			return result.ToArray();
		}

		public static List<Tuple<Type, MethodInfo[]>> Resolve(Type messageType)
		{
			var result = new List<Tuple<Type, MethodInfo[]>>();
			foreach (var service in ServiceMethods)
			{
				var methods = service.Value.Where(it => it.GetParameters().Any(it => it.ParameterType == messageType)).ToArray();

				if (!methods.Any())
					continue;

				result.Add(new Tuple<Type, MethodInfo[]>(service.Key, methods));
			}
			return result;
		}

		public static List<Tuple<Type, MethodInfo[]>> ResolveEvent(Type messageType)
		{
			var result = new List<Tuple<Type, MethodInfo[]>>();
			foreach (var service in ServiceMethods)
			{
				var methods = service.Value.Where(it => it.GetParameters().Any(it => it.ParameterType.IsAssignableFrom(messageType))).ToArray();

				if (!methods.Any())
					continue;

				result.Add(new Tuple<Type, MethodInfo[]>(service.Key, methods));
			}
			return result;
		}

		public static object GetOperation(GenericOperation superContract)
		{
			if (superContract is null) throw new ArgumentNullException(nameof(superContract));

			var lazyOperationType = OperationTypes.GetOrAdd(superContract.PayloadTypeName, x => new Lazy<Type>(() =>
			{
				var assembly = Assembly.GetAssembly(typeof(Operation));

				if (assembly == null) return null;

				var operationType = assembly.GetType(superContract.PayloadTypeName);

				return operationType;
			}));

			if (lazyOperationType.Value == null) throw new ArgumentNullException($"Missing an operaton {superContract.PayloadTypeName}");

			var operation = JsonConvert.ToObject(superContract.Payload, lazyOperationType.Value);

			return operation;
		}

		public static async Task<object> InvokeServiceHandlerAsync(
			ServiceComponent component,
			object operation)
		{
			if (component is null) throw new ArgumentNullException(nameof(component));
			if (operation is null) throw new ArgumentNullException(nameof(operation));

			var lazyMethod = OperationHandlers.GetOrAdd($"{component.GetType().FullName}:{operation.GetType().FullName}", x => new Lazy<MethodInfo>(() =>
			{
				var methods = component.GetType().GetMethods()
					.Where(it => it.GetCustomAttributes(typeof(MessageHandlerAttribute), true).Length > 0)
					.Where(it => it.GetParameters().Any(it => it.ParameterType.IsAssignableFrom(operation.GetType())))
					.ToList();

				return methods.FirstOrDefault();
			}));

			if (lazyMethod.Value is null) throw new NotImplementedException("HandleAsync");

			var task = lazyMethod.Value.Invoke(component, new object[] { operation }) as Task;

			await task.ConfigureAwait(false);

			var resultProperty = task.GetType().GetProperty("Result");

			var result = resultProperty.GetValue(task);

			return result;
		}

		public static async Task InvokeEventHandlersAsync(
			ServiceComponent component,
			object @event)
		{
			if (component is null) throw new ArgumentNullException(nameof(component));
			if (@event is null) throw new ArgumentNullException(nameof(@event));

			var lazyMethod = EventHandlers.GetOrAdd($"{component.GetType().FullName}:{@event.GetType().FullName}", x => new Lazy<List<MethodInfo>>(() =>
			{
				var methods = component.GetType().GetMethods()
					.Where(it => it.GetCustomAttributes(typeof(MessageHandlerAttribute), true).Length > 0)
					.Where(it => it.GetParameters().Any(it => it.ParameterType.IsAssignableFrom(@event.GetType())))
					.ToList();

				return methods;
			}));

			if (!lazyMethod.Value.Any())
				return;

			var tasks = new List<Task>();
			foreach (var item in lazyMethod.Value)
			{
				var task = item.Invoke(component, new object[] { @event }) as Task;
				tasks.Add(task);
			}

			await Task.CompletedTask;
		}
	}
}