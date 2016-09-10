using EvoVI.PluginContracts;
using EvoVI.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace EvoVI
{
    public static class PluginManager
    {
        #region Constants
        public static readonly string[] GLOBAL_PLUGIN_PARAMETERS = new string[] { "enabled" };
        #endregion


        #region Variables
        public static List<IPlugin> Plugins = new List<IPlugin>();
        private static IniFile _pluginFile;
        private static Dictionary<string, Dictionary<string, PluginParameterDefault>> _pluginDefaults = new Dictionary<string, Dictionary<string, PluginParameterDefault>>();
        private static Dictionary<string, List<IPlugin>> _dllDictionary = new Dictionary<string, List<IPlugin>>();
        private static Dictionary<IPlugin, Thread> _pluginThreads = new Dictionary<IPlugin, Thread>();
        #endregion


        #region Properties
        /// <summary> Returns the last loaded plugin configuration.
        /// </summary>
        public static IniFile PluginFile
        {
            get { return PluginManager._pluginFile; }
        }


        /// <summary> Returns the database containing default plugin parameters.
        /// </summary>
        public static Dictionary<string, Dictionary<string, PluginParameterDefault>> PluginDefaults
        {
            get { return PluginManager._pluginDefaults; }
        }


        /// <summary> Returns the plugin configuration path.
        /// </summary>
        public static string PluginConfigPath
        {
            get { return PluginManager.GetPluginPath() + "\\" + "plugins.ini"; }
        }

        public static Dictionary<string, List<IPlugin>> LoadedDLLs
        {
            get { return PluginManager._dllDictionary; }
        }
        #endregion


        #region Functions
        /// <summary> Gets a plugin by name.
        /// </summary>
        public static IPlugin GetPlugin(string pluginName)
        {
            for (int i = 0; i < Plugins.Count; i++)
            {
                if (String.Equals(Plugins[i].Name, pluginName, StringComparison.InvariantCultureIgnoreCase)) return Plugins[i];
            }

            return null;
        }


        /// <summary> Gets the path to the plugin directory.
        /// </summary>
        /// <returns>The absolute path to the plugin directory.</returns>
        public static string GetPluginPath()
        {
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
            return appPath + "\\" + "Plugins";
        }


        /// <summary> Loads all plugins inside the [ApplicationPath]/Plugins folder.
        /// </summary>
        public static void LoadPlugins(bool loadDisabledPlugins=false)
        {
            Plugins.Clear();
            _dllDictionary.Clear();

            /* Load plugin files */
            string[] dllFileNames = null;
            string pluginPath = GetPluginPath();

            _pluginFile = new IniFile(PluginConfigPath);
            _pluginFile.Read();

            if (Directory.Exists(pluginPath))
            {
                dllFileNames = Directory.GetFiles(pluginPath, "*.dll");

                ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
                foreach (string dllFile in dllFileNames)
                {
                    Assembly assembly = Assembly.Load(AssemblyName.GetAssemblyName(dllFile));
                    assemblies.Add(assembly);
                }

                Type pluginType = typeof(IPlugin);
                ICollection<Type> pluginTypes = new List<Type>();
                foreach (Assembly assembly in assemblies)
                {
                    if (assembly != null)
                    {
                        Type[] types = assembly.GetTypes();

                        foreach (Type type in types)
                        {
                            if (type.IsInterface || type.IsAbstract) { continue; }
                            if (type.GetInterface(pluginType.FullName) != null) { pluginTypes.Add(type); }
                        }
                    }
                }

                foreach (Type type in pluginTypes)
                {
                    IPlugin plugin = (IPlugin)Activator.CreateInstance(type);
                    
                    // Check for plugin integrity before adding it to the list
                    if (
                        // Plugin with invalid name
                        (String.IsNullOrWhiteSpace(plugin.Name)) ||

                        // Plugins that have *explicitly* been disabled
                        (
                            (!loadDisabledPlugins) &&
                            (_pluginFile.HasKey(plugin.Name, "Enabled")) &&
                            (!_pluginFile.ValueIsBoolAndTrue(plugin.Name, "Enabled"))
                        )
                    )
                    { continue; }
                    
                    // Add the DLL file name to the dll dictionary
                    string dllFileName = Path.GetFileName(plugin.GetType().Assembly.CodeBase);
                    if (!_dllDictionary.ContainsKey(dllFileName)) { _dllDictionary.Add(dllFileName, new List<IPlugin>()); }
                    _dllDictionary[dllFileName].Add(plugin);

                    // Create an execution thread entry for the plugin
                    if (!_pluginThreads.ContainsKey(plugin)) { _pluginThreads.Add(plugin, new Thread(plugin.OnGameDataUpdate)); }

                    Plugins.Add(plugin);
                }
            }
            else
            {
                Directory.CreateDirectory(pluginPath);
            }

            // Fill plugins.ini file with standard values, if not set
            _pluginDefaults.Clear();
            for (int i = 0; i < Plugins.Count; i++)
            {
                string pluginName = Plugins[i].Name;

                /* Create entry in the default plugin db */
                if (!_pluginDefaults.ContainsKey(pluginName)) { _pluginDefaults.Add(pluginName, new Dictionary<string, PluginParameterDefault>()); }

                /* Global plugin parameters */
                if (!_pluginFile.HasKey(pluginName, "Enabled")) { _pluginFile.SetValue(pluginName, "Enabled", "False"); }

                /* Fill in custom plugin parameters */
                List<PluginParameterDefault> pluginParams = Plugins[i].GetDefaultPluginParameters();
                for (int u = 0; u < pluginParams.Count; u++)
                {
                    PluginParameterDefault currParam = pluginParams[u];
                    if (!_pluginDefaults[pluginName].ContainsKey(currParam.Key)) { _pluginDefaults[pluginName].Add(currParam.Key, currParam); }

                    if (!_pluginFile.HasKey(pluginName, currParam.Key))
                    {
                        _pluginFile.SetValue(pluginName, currParam.Key, currParam.DefaultValue);
                    }
                }
            }

            _pluginFile.Write(PluginConfigPath);
        }


        /// <summary> Initializes all loaded plugins.
        /// </summary>
        public static void InitializePlugins()
        {
            for (int i = 0; i < Plugins.Count; i++) { Plugins[i].Initialize(); }
        }


        /// <summary> Calls OnProgramShutdown on each plugin (sequentially, non-threaded).
        /// </summary>
        public static void ShutdownPlugins()
        {
            // Check for running threads and wait for their completion before shutdown
            foreach (KeyValuePair<IPlugin, Thread> pluginThread in _pluginThreads)
            {
                if (pluginThread.Value.IsAlive) { pluginThread.Value.Abort(); }

                // Wait for... an abortion O.o
                while (pluginThread.Value.IsAlive) { Thread.Sleep(100); }

                // Tell the plugin to shut down
                pluginThread.Key.OnProgramShutdown();
            }
        }


        /// <summary> Calls OnGameDataUpdate on each plugin (parallel, threaded).
        /// </summary>
        public static void CallGameDataUpdateOnPlugins()
        {
            for (int i = 0; i < Plugins.Count; i++)
            {
                IPlugin currPlugin = Plugins[i];
                if (!_pluginThreads.ContainsKey(currPlugin)) { continue; }

                // Check the state of the thread
                if (
                    (!_pluginThreads[currPlugin].IsAlive)
                )
                {
                    _pluginThreads[currPlugin] = (new Thread(currPlugin.OnGameDataUpdate));
                    _pluginThreads[currPlugin].Start();
                }
            }
        }
        #endregion
    }
}