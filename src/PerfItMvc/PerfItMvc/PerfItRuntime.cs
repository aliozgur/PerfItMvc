using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;

namespace PerfItMvc
{
	public static class PerfItRuntime
	{
		public static string ApplicationName { get; set; }

		static PerfItRuntime()
		{

			HandlerFactories = new Dictionary<string, Func<PerfItActionFilterAttribute, ICounterHandler>>();

			HandlerFactories.Add(CounterTypes.TotalNoOfOperations,
				(filter) => new TotalCountHandler(ApplicationName, filter));

			HandlerFactories.Add(CounterTypes.AverageTimeTaken,
				(filter) => new AverageTimeHandler(ApplicationName, filter));

			HandlerFactories.Add(CounterTypes.LastOperationExecutionTime,
				(filter) => new LastOperationExecutionTimeHandler(ApplicationName, filter));

			HandlerFactories.Add(CounterTypes.NumberOfOperationsPerSecond,
				(filter) => new NumberOfOperationsPerSecondHandler(ApplicationName,filter));

		}

		/// <summary>
		/// Counter handler factories with counter type as the key.
		/// Factory's first param is applicationName and second is the filter
		/// Use it to register your own counters or replace built-in implementations
		/// </summary>
		public static Dictionary<string, Func< PerfItActionFilterAttribute, ICounterHandler>> HandlerFactories { get; private set; }

		/// <summary>
		/// Uninstalls performance counters in the current assembly using PerfItFilterAttribute.
		/// </summary>
		public static void Uninstall()
		{
			var perfItFilterAttributes = FindAllFilters();

			var cayegories = perfItFilterAttributes.ToList().Select(x => x.CategoryName).Distinct();
			cayegories.ToList().ForEach(
				(x) =>
				{
					try
					{
						if (PerformanceCounterCategory.Exists(x))
							PerformanceCounterCategory.Delete(x);
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}
				);

		}

		/// <summary>
		/// Installs performance counters in the current assembly using PerfItFilterAttribute.
		/// </summary>
		public static void Install(string applicationName)
		{
			ApplicationName = applicationName;
			Uninstall();

			var perfItFilterAttributes = FindAllFilters();
			var dictionary = new Dictionary<string, CounterCreationDataCollection>();

			foreach (var filter in perfItFilterAttributes)
			{
				if (!dictionary.ContainsKey(filter.CategoryName))
				{
					dictionary.Add(filter.CategoryName, new CounterCreationDataCollection());
				}
				foreach (var counterType in filter.Counters)
				{
					if (!HandlerFactories.ContainsKey(counterType))
						throw new ArgumentException("Counter type not defined: " + counterType);
					using (var counterHandler = HandlerFactories[counterType](filter))
					{
						dictionary[filter.CategoryName].AddRange(counterHandler.BuildCreationData());
					}
				}
			}

			// now create them
			foreach (var categoryName in dictionary.Keys)
			{
				PerformanceCounterCategory.Create(categoryName, "PerfItMvc category for " + categoryName,
					PerformanceCounterCategoryType.MultiInstance, dictionary[categoryName]);
			}

		}

		/// <summary>
		/// Extracts all filters in the current assembly defined on ApiControllers
		/// </summary>
		/// <returns></returns>
		internal static IEnumerable<PerfItActionFilterAttribute> FindAllFilters()
		{
			var attributes = new List<PerfItActionFilterAttribute>();
			var assemblies = AppDomain.CurrentDomain.GetAssemblies()
				.Where(a => !(a is System.Reflection.Emit.AssemblyBuilder)
					&& a.GetType().FullName != "System.Reflection.Emit.InternalAssemblyBuilder"
					&& !a.GlobalAssemblyCache);
			var actionFilters = assemblies.SelectMany(x => x.GetExportedTypes())
										   .Where(t => typeof(Controller).IsAssignableFrom(t) &&
														!t.IsAbstract);

			foreach (var actionFilter in actionFilters)
			{
				var controllerName = actionFilter.Name;
				var methodInfos = actionFilter.GetMethods(BindingFlags.Instance | BindingFlags.Public);
				foreach (var methodInfo in methodInfos)
				{
					var attr = (PerfItActionFilterAttribute)methodInfo.GetCustomAttributes(typeof(PerfItActionFilterAttribute), true).FirstOrDefault();
					if (attr != null)
					{
						if (string.IsNullOrEmpty(attr.Name)) // default name
						{
							var actionName = (ActionNameAttribute)
											 methodInfo.GetCustomAttributes(typeof(ActionNameAttribute), true)
													   .FirstOrDefault();
							if (actionName == null)
							{
								attr.Name = controllerName + "." + methodInfo.Name;
							}
							else
							{
								attr.Name = controllerName + "." + actionName.Name;
							}

							if (string.IsNullOrEmpty(attr.CategoryName))
							{
								attr.CategoryName = actionFilter.Assembly.GetName().Name;
							}
							attributes.Add(attr);
						}

					}
				}
			}

			return attributes;
		}


	}
}
