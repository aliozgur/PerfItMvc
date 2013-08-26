using System;
using System.Diagnostics;
using System.Web.Mvc;


namespace PerfItMvc.Handlers
{
	public class TotalCountHandler : CounterHandlerBase
	{

		private readonly Lazy<PerformanceCounter> _counter;

		public TotalCountHandler(string applicationName, PerfItMvcFilterAttribute filter)
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
			get { return CounterTypes.TotalNoOfOperations; }
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			// nothing 
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			_counter.Value.Increment();
		}

		protected override CounterCreationData[] DoGetCreationData()
		{
			return new[]
                       {
                           new CounterCreationData()
                               {
                                   CounterName = Name,
                                   CounterType = PerformanceCounterType.NumberOfItems32,
                                   CounterHelp = _filter.Description
                               }
                       };
		}

		public override void Dispose()
		{
			base.Dispose();
			if (_counter != null && _counter.IsValueCreated)
			{
				_counter.Value.RemoveInstance();
				_counter.Value.Dispose();
			}
		}


	}
}
