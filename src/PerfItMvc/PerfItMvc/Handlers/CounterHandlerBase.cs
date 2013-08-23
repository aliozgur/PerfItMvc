using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PerfItMvc
{
	public abstract class CounterHandlerBase : ICounterHandler
	{
		protected string _applicationName;
		protected PerfItActionFilterAttribute _filter;

		public CounterHandlerBase(string applicationName, PerfItActionFilterAttribute filter)
		{
			_filter = filter;
			Name = filter.Name + "." + CounterType;
			_applicationName = applicationName;
		}

		public virtual void Dispose()
		{

		}

		/// <summary>
		/// type of counter. just a string identifier
		/// </summary>
		public abstract string CounterType { get; }

		/// <summary>
		/// called when request arrives in delegating handler
		/// </summary>
		/// <param name="request"></param>
		/// <param name="context"></param> 
		public abstract void OnActionExecuting(ActionExecutingContext filterContext, PerfItContext context);

		/// <summary>
		/// called as the async continuation on the delegating handler (when response is sent back)
		/// </summary>
		/// <param name="response"></param>
		/// <param name="context"></param>
		public abstract void OnActionExecuted(ActionExecutedContext filterContext, PerfItContext context);

		/// <summary>
		/// Handler to return data for creating counters
		/// </summary>
		/// <param name="filter">Filter attribute defined</param>
		/// <returns></returns>
		protected abstract CounterCreationData[] DoGetCreationData();


		public string Name { get; private set; }

		public CounterCreationData[] BuildCreationData()
		{
			return DoGetCreationData();
		}

	}
}
