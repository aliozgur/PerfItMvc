﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PerfItMvc;

namespace PerfItMvc.SampleApp.Controllers
{


	public class HomeController : Controller
	{
		[PerfItActionFilter(Description = "Welcome Action",Counters = new[]{CounterTypes.TotalNoOfOperations,CounterTypes.AverageTimeTaken})]
		public ActionResult Index()
		{
			return View();
		}

		[PerfItActionFilter(Description = "About Action", Counters = new[] { CounterTypes.TotalNoOfOperations, CounterTypes.AverageTimeTaken })]
		public ActionResult About()
		{
			return View();
		}
	}
}