using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PerfItMvc
{
	public interface ICounterHandler : IDisposable
	{
		string CounterType { get; }
		void OnActionExecuting(ActionExecutingContext filterContext, PerfItContext context);
		void OnActionExecuted(ActionExecutedContext filterContext, PerfItContext context);
		string Name { get; }
		CounterCreationData[] BuildCreationData();
	}

}
