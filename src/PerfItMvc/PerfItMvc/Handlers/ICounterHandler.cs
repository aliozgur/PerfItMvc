using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PerfItMvc.Handlers
{
	public interface ICounterHandler : IDisposable
	{
		string CounterType { get; }
		void OnActionExecuting(ActionExecutingContext filterContext);
		void OnActionExecuted(ActionExecutedContext filterContext);
		string Name { get; }
		CounterCreationData[] BuildCreationData();
	}

}
