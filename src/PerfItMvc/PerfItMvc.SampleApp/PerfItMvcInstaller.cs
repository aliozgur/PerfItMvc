using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace PerfItMvc.SampleApp
{
	[RunInstaller(true)]
	public partial class PerfItMvcInstaller : System.Configuration.Install.Installer
	{
		public PerfItMvcInstaller()
		{
			InitializeComponent();
		}

		public override void Install(IDictionary stateSaver)
		{
			base.Install(stateSaver);
			PerfItRuntime.Install("PerfItMvc.SampleApp");
		}

		public override void Uninstall(IDictionary savedState)
		{
			base.Uninstall(savedState);
			PerfItRuntime.Uninstall();
		}
	}
}
