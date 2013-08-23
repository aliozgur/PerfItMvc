using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PerfItMvc
{
	public class PerfItActionFilterAttribute:ActionFilterAttribute
	{
		public string CategoryName { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string[] Counters { get; set; }

		private PerfItContext _context = null;


		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			_context = new PerfItContext();
			this.Name = !String.IsNullOrWhiteSpace(Name) ? Name : filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "." + filterContext.ActionDescriptor.ActionName;
			this.CategoryName = !String.IsNullOrWhiteSpace(CategoryName) ? CategoryName :  Assembly.GetExecutingAssembly().GetName().Name;

			base.OnActionExecuting(filterContext);
			foreach (var key in PerfItRuntime.HandlerFactories.Keys)
			{
				var x = PerfItRuntime.HandlerFactories[key];
				x(this).OnActionExecuting(filterContext,_context);
			}
		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			this.Name = !String.IsNullOrWhiteSpace(Name) ? Name : filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "." + filterContext.ActionDescriptor.ActionName ;
			this.CategoryName = !String.IsNullOrWhiteSpace(CategoryName) ? CategoryName : Assembly.GetExecutingAssembly().GetName().Name;
			base.OnActionExecuted(filterContext);
			foreach (var key in PerfItRuntime.HandlerFactories.Keys)
			{
				var x = PerfItRuntime.HandlerFactories[key];
				x(this).OnActionExecuted(filterContext, _context);
			}
		}
	}
}
