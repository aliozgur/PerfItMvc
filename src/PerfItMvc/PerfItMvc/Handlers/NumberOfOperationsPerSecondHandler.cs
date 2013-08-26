using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace PerfItMvc.Handlers
{
    public class NumberOfOperationsPerSecondHandler : CounterHandlerBase
    {
        private readonly Lazy<PerformanceCounter> _counter;
        private const string TimeTakenTicksKey = "NumberOfOperationsPerSecondHandler_#_StopWatch_#_";

        public NumberOfOperationsPerSecondHandler(string applicationName, PerfItMvcFilterAttribute filter) 
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
            get { return CounterTypes.NumberOfOperationsPerSecond; }
        }

		
		public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
			filterContext.Controller.TempData.Add(TimeTakenTicksKey + Name, Stopwatch.StartNew());

            //context.Data.Add(TimeTakenTicksKey + Name, Stopwatch.StartNew());
        }

		public override  void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //var sw = (Stopwatch)context.Data[TimeTakenTicksKey + Name];
			var sw = (Stopwatch)filterContext.Controller.TempData[TimeTakenTicksKey + Name];
			sw.Stop();
            _counter.Value.Increment();
        }

        protected override CounterCreationData[] DoGetCreationData()
        {
            var counterCreationDatas = new CounterCreationData[1];
            counterCreationDatas[0] = new CounterCreationData()
            {
                CounterType = PerformanceCounterType.RateOfCountsPerSecond32,
                CounterName = Name,
                CounterHelp = _filter.Description
            };

            return counterCreationDatas;
        }
    }
}
