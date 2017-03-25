using System;
using System.Collections.Generic;

namespace NDK.Framework {

	#region IFramework
	/// <summary>
	/// 
	/// </summary>
	public class Program : Framework, IFramework {

		#region Implement IFramework abstract methods.
		/// <summary>
		/// Gets the unique guid.
		/// When implementing a class, this method should return the same unique guid every time. 
		/// </summary>
		/// <returns></returns>
		public override Guid GetGuid() {
			return new Guid("{D436174B-60D5-46AB-85FB-66CA44D98159}");
		} // GetGuid

		/// <summary>
		/// Gets the name.
		/// When implementing a class, this method should return a proper display name.
		/// </summary>
		/// <returns></returns>
		public override String GetName() {
			return "NDK Run";
		} // GetName
		#endregion

		#region Run method.
		/// <summary>
		/// The main application method.
		/// </summary>
		/// <param name="args">The arguments.</param>
		public void Run(String[] args) {
			List<String> argsList = null;
			List<Guid> argGuids = null;
			List<IPlugin> plugins = null;

			try {
				// Get the first argument.
				// Add all remaining arguments to the argument list.
				argsList = new List<String>();
				String arg = String.Empty;
				if (args.Length > 0) {
					arg = args[0];
					for (Int32 argIndex = 1; argIndex < args.Length; argIndex++) {
						argsList.Add(args[argIndex]);
					}
				}

				// If the first argument is one or more guids, then run the plugin with that guid as a commandline program.
				argGuids = new List<Guid>();
				foreach (String argGuidStr in arg.Split(',', ';')) {
					Guid argGuid = Guid.Empty;
					if ((Guid.TryParse(argGuidStr, out argGuid) == true) && (Guid.Empty.Equals(argGuid) == false) && (argGuids.Contains(argGuid) == false)) {
						argGuids.Add(argGuid);
						arg = "program";
					}
				}

				//
				switch (arg.ToLower()) {
					// Install the service.
					case "i":
					case "install":
						// Install the service.
						this.Log("Application: Installing the service.");
						ServiceManager.Install("run");
						this.Log("Application: The service is installed");
						break;

					// Uninstall the service.
					case "u":
					case "uninstall":
						// Uninstall the service.
						this.Log("Application: Uninstalling the service.");
						ServiceManager.Uninstall();
						this.Log("Application: The service is uninstalled.");
						break;

					// Uninstall the service.
					case "start":
						// Start the service.
						this.Log("Application: Starting the service.");
						ServiceManager.Start();
						this.Log("Application: The service is started.");
						break;

					// Stop the service.
					case "stop":
						// Stop the service.
						this.Log("Application: Stopping the service.");
						ServiceManager.Stop();
						this.Log("Application: The service is stopped.");
						break;

					// Run the service.
					case "service":
						// Create a new instance of the service object, and start the service.
						this.Log("Application: The service execution is starting.");
						try {
							ServiceManager.Run(this, argsList.ToArray());
						} catch (Exception exception) {
							this.LogError(exception);
						}
						this.Log("Application: The service execution has ended.");
						break;

					// Run the service.
					// This is used to debug, without installing the service.
					// The service can only start, when it is installed on the system disk, and not a network disk, where this is developped.
					case "programservice":
						// Create a new instance of the service object, and start the service.
						this.Log("Application: The service execution is starting.");
						try {
							ServiceManager service = new ServiceManager(this, argsList.ToArray());
							service.RunThread();
						} catch (Exception exception) {
							this.LogError(exception);
						}
						this.Log("Application: The service execution has ended.");
						break;

					// Run the plugin.
					case "program":
						// Get plugins.
						plugins = new List<IPlugin>();
						plugins.AddRange(this.GetPlugins());

						// Keep plugins that are selected in the arguments, remove plugins, that are not selected in the arguments.
						for (Int32 pluginIndex = 0; pluginIndex < plugins.Count;) {
							IPlugin plugin = plugins[pluginIndex];
							if (argGuids.Contains(plugin.GetGuid()) == true) {
								pluginIndex++;
								this.LogDebug("Application:  {0}   {1}  (requested)", plugin.GetGuid(), plugin.GetName());
							} else {
								plugins.RemoveAt(pluginIndex);
								this.LogDebug("Application:  {0}   {1}", plugin.GetGuid(), plugin.GetName());
							}
						}

						// Process all selected plugins.
						foreach (BasePlugin plugin in plugins) {
							// Run the plugin.
							this.Log("Application: The plugin execution is starting.");
							try {
								plugin.Run();
							} catch (Exception exception) {
								this.LogError(exception);
							}
							this.Log("Application: The plugin execution has ended.");
						}
						break;

					// Help.
					default:
						// Write help to the console.
						Console.WriteLine("The following arguments are supported.");
						Console.WriteLine("");
						Console.WriteLine("  install      Install the service.");
						Console.WriteLine("  uninstall    Uninstall the service.");
						Console.WriteLine("  start        Start the service.");
						Console.WriteLine("  stop         Stop the service.");
						Console.WriteLine("  service      Run the service.");

						plugins = new PluginList<IPlugin>();
						Console.WriteLine("  <guid>       Run the plugin as a application.");
						Console.WriteLine("               Separate multiple guids with a comma or semicolon.");
						Console.WriteLine("               {0} plugin(s) found.", plugins.Count);
						foreach (BasePlugin plugin in plugins) {
							Console.WriteLine("               {0}   {1}", plugin.GetGuid(), plugin.GetName());
						}
						break;
				}
			} catch (Exception exception) {
				// Write error to the logger.
				this.LogError("Application: The following critical error occured.");
				this.LogError(exception.Message);
				this.LogError(exception.StackTrace);

				// Write error to console.
				Console.WriteLine("Application: The following critical error occured.");
				Console.WriteLine(exception.Message);
				Console.WriteLine(exception.StackTrace);
			}
		} // Run
		#endregion

		#region Main method
		public static void Main(String[] args) {
			Program program = new Program();
			program.Run(args);
		} // Main
		#endregion

	} // Program
	#endregion

} // NDK.Framework