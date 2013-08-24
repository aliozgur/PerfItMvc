## PerfIt MVC!

Windows performance monitoring for ASP.NET MVC controller actions

[Adapted from PerfIt by Ali Kheyrollahi](https://github.com/aliostad/PerfIt)

## How to install and use?
1. Reference PerfItMvc in your ASP.NET MVC Application

2. Decorate your controller actions with __PerfItMvcFilterAttribute__

3. Add an [Installer class] (http://msdn.microsoft.com/en-us/library/system.configuration.install.installer.aspx) to your ASP.NET MVC Project

4. Override Install and Uninstall methods of the newly added Installer class
```c#
  public override void Install(IDictionary stateSaver)
  {
	  base.Install(stateSaver);
	  PerfItMvcRuntime.Install();
  }

  public override void Uninstall(IDictionary savedState)
  {
	  base.Uninstall(savedState);
	  PerfItMvcRuntime.Uninstall();
  }
```

5. Open Visual Studio Developer Command prop as _Administrator_

6. Enter InstallUtil.exe -i "<Path to your ASP.NET MVC app DLL>" 
To Uninstall you can use -u switch

7. In you Global.asax file 
 * PerfItMvcRuntime.Start("PerfItMvc.SampleApp") to Application_Start()
 * PerfItMvcRuntime.Stop() to Application_End()

## Samples
* For sample usage please refer to the [Sample ASP.NET MVC 4 App] (/src/PerfItMvc/PerfItMvc.SampleApp)
* You can refer to Ali Kheyrollahi's [blog post](http://byterot.blogspot.co.uk/2013/04/Monitor-your-ASP-NET-Web-API-application-using-your-own-custom-counters.html) for installation of the original PerfIt.
