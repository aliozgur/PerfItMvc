using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PerfItMvc
{
	public class PerfItMvcContext
	{

		public PerfItMvcContext()
		{
			Data = new Dictionary<string, object>();
			CountersToRun = new ConcurrentBag<string>();
		}

		public PerfItMvcFilterAttribute Filter { get; set; }
		public Dictionary<string, object> Data { get; private set; }
		public ConcurrentBag<string> CountersToRun { get; private set; }
	}


}
