using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PerfItMvc.Handlers
{
	public class LastOperationExecutionTimeHandler : CounterHandlerBase
	{
		private const string TimeTakenTicksKey = "LastOperationExecutionTimeHandler_#_StopWatch_#_";
		private readonly Lazy<PerformanceCounter> _counter;

		public LastOperationExecutionTimeHandler(string applicationName, PerfItMvcFilterAttribute filter)
			: base(applicationName, filter)
		{
			_counter = new Lazy<PerformanceCounter>(() =>
					{
						var counter = new PerformanceCounter()
						{
							CategoryName = filter.CategoryName,
							CounterName = Name,
							InstanceName = applicationName,
							ReadOnly = false,
							InstanceLifetime = PerformanceCounterInstanceLifetime.Process
						};
						counter.RawValue = 0;
						return counter;
					});
		}

		public override string CounterType
		{
			get { return CounterTypes.LastOperationExecutionTime; }
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			filterContext.Controller.TempData.Add(TimeTakenTicksKey + Name, Stopwatch.StartNew());
			//context.Data.Add(TimeTakenTicksKey + Name, Stopwatch.StartNew());
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			//var sw = (Stopwatch)context.Data[TimeTakenTicksKey + Name];
			var sw = (Stopwatch)filterContext.Controller.TempData[TimeTakenTicksKey + Name];

			sw.Stop();
			_counter.Value.RawValue = sw.ElapsedMilliseconds;
		}

		protected override CounterCreationData[] DoGetCreationData()
		{
			var counterCreationDatas = new CounterCreationData[1];
			counterCreationDatas[0] = new CounterCreationData()
			{
				CounterType = PerformanceCounterType.NumberOfItems32,
				CounterName = Name,
				CounterHelp = _filter.Description
			};

			return counterCreationDatas;
		}
	}
}
