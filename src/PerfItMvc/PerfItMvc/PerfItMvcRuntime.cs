using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web.Http;
using System.Web.Mvc;

using PerfItMvc.Handlers;

namespace PerfItMvc
{
	public static class PerfItMvcRuntime
	{
		/// <summary>
		/// Counter handler factories with counter type as the key.
		/// Factory's first param is applicationName and second is the filter
		/// Use it to register your own counters or replace built-in implementations
		/// </summary>
		public static Dictionary<string, Func<string, PerfItMvcFilterAttribute, ICounterHandler>> HandlerFactories { get; private set; }


		private static Dictionary<string, PerfItCounterContext> _counterContexts =
						new Dictionary<string, PerfItCounterContext>();

		internal static bool IsEnabled
		{
			get
			{
				var value = ConfigurationManager.AppSettings[Constants.PerfItPublishCounters];
				bool result = true;
				if (!string.IsNullOrEmpty(value))
				{
					Boolean.TryParse(value, out result);
				}
				return result;
			}
		}

		static PerfItMvcRuntime()
		{

			HandlerFactories = new Dictionary<string, Func<string,PerfItMvcFilterAttribute, ICounterHandler>>();

			HandlerFactories.Add(CounterTypes.TotalNoOfOperations,
				(appName, filter) => new TotalCountHandler(appName, filter));

			HandlerFactories.Add(CounterTypes.AverageTimeTaken,
				(appName, filter) => new AverageTimeHandler(appName, filter));

			HandlerFactories.Add(CounterTypes.LastOperationExecutionTime,
				(appName, filter) => new LastOperationExecutionTimeHandler(appName, filter));

			HandlerFactories.Add(CounterTypes.NumberOfOperationsPerSecond,
				(appName, filter) => new NumberOfOperationsPerSecondHandler(appName, filter));
		}
		
		public static void Start(string applicationName)
		{
			var filters = PerfItMvcRuntime.FindAllFilters();
			foreach (var filter in filters)
			{
				foreach (var counterType in filter.Counters)
				{
					if (!PerfItMvcRuntime.HandlerFactories.ContainsKey(counterType))
						throw new ArgumentException("Counter type not registered: " + counterType);

					var counterHandler = PerfItMvcRuntime.HandlerFactories[counterType](applicationName,filter);
					if (!_counterContexts.Keys.Contains(counterHandler.Name))
					{
						_counterContexts.Add(counterHandler.Name, new PerfItCounterContext()
						{
							Handler = counterHandler,
							Name = counterHandler.Name
						});
					}
				}
			}
		}

		public static void Stop()
		{
			foreach (var counterHandlers in _counterContexts.Values)
			{
				counterHandlers.Handler.Dispose();
			}

			_counterContexts.Clear();
		}

		internal static void OnActionExecuting(string actionName,ActionExecutingContext filterContext, PerfItMvcContext context)
		{
			if (!_counterContexts.ContainsKey(actionName))
				return;
			_counterContexts[actionName].Handler.OnActionExecuting(filterContext, context);
	
		}

		internal static void OnActionExecuted(string actionName,ActionExecutedContext filterContext, PerfItMvcContext context)
		{
			if (!_counterContexts.ContainsKey(actionName))
				return;
			_counterContexts[actionName].Handler.OnActionExecuted(filterContext, context);
		}

	
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
		public static void Install()
		{
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
					using (var counterHandler = HandlerFactories[counterType]("Dummy",filter))
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
		internal static IEnumerable<PerfItMvcFilterAttribute> FindAllFilters()
		{
			var attributes = new List<PerfItMvcFilterAttribute>();
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
					var attr = (PerfItMvcFilterAttribute)methodInfo.GetCustomAttributes(typeof(PerfItMvcFilterAttribute), true).FirstOrDefault();
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


	public class PerfItCounterContext
	{
		public string Name { get; set; }
		public ICounterHandler Handler { get; set; }

	}
}
