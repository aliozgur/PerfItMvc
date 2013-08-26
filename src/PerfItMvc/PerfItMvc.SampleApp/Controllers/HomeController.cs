using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PerfItMvc;

namespace PerfItMvc.SampleApp.Controllers
{
	public class HomeController : Controller
	{
		[PerfItMvcFilter(Name="Index_Get", Description = "Welcome Action",Counters = new[]{CounterTypes.AverageTimeTaken})]
		public ActionResult Index()
		{
			return View();
		}

		[PerfItMvcFilter(Name="Index_Post", Description = "Welcome Action", Counters = new[] { CounterTypes.AverageTimeTaken })]
		[HttpPost]
		public ActionResult Index(string id)
		{
			return View();
		}

		[PerfItMvcFilter(Description = "About Action", Counters = new[] { CounterTypes.TotalNoOfOperations, CounterTypes.AverageTimeTaken })]
		public ActionResult About()
		{
			return View();
		}
	}
}
