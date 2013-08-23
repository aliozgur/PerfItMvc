using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerfItMvc
{
    public class PerfItContext
    {

        public PerfItContext()
        {
            Data = new Dictionary<string, object>();    
            CountersToRun = new ConcurrentBag<string>();
        }

        public PerfItActionFilterAttribute Filter { get; set; }
        public Dictionary<string, object> Data { get; private set; }
        public ConcurrentBag<string> CountersToRun { get; private set; } 
    }


	public class PerfItCounterContext
	{
		public string Name { get; set; }
		public ICounterHandler Handler { get; set; }

	}
}
