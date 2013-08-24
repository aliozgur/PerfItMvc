using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PerfItMvc
{
	public class PerfItMvcFilterAttribute:ActionFilterAttribute
	{
		public string CategoryName { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string[] Counters { get; set; }

		private PerfItMvcContext _context = null;


		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!PerfItMvcRuntime.IsEnabled)
			{
				base.OnActionExecuting(filterContext);
				return;		
			}
			
			_context = new PerfItMvcContext();
			this.Name = !String.IsNullOrWhiteSpace(Name) ? Name : filterContext.ActionDescriptor.ControllerDescriptor.ControllerType.Name + "." + filterContext.ActionDescriptor.ActionName;
			this.CategoryName = !String.IsNullOrWhiteSpace(CategoryName) ? CategoryName : filterContext.Controller.GetType().Assembly.GetName().Name;

			base.OnActionExecuting(filterContext);
			_context = new PerfItMvcContext();
			
			foreach (string counterType in Counters)
			{
				PerfItMvcRuntime.OnActionExecuting(this.Name + "." + counterType, filterContext, _context);			
			}

		}

		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (!PerfItMvcRuntime.IsEnabled)
			{
				base.OnActionExecuted(filterContext);
				return;
			}

			this.Name = !String.IsNullOrWhiteSpace(Name) ? Name : filterContext.ActionDescriptor.ControllerDescriptor.ControllerName + "." + filterContext.ActionDescriptor.ActionName;
			this.CategoryName = !String.IsNullOrWhiteSpace(CategoryName) ? CategoryName : filterContext.Controller.GetType().Assembly.GetName().Name;
			base.OnActionExecuted(filterContext);
			foreach (string counterType in Counters)
			{
				PerfItMvcRuntime.OnActionExecuted(this.Name + "." + counterType, filterContext, _context);
			}
		}
	}
}
