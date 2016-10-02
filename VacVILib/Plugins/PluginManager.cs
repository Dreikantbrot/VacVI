﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace VacVI.Plugins
{
    /// <summary> Loads, initializes, calls and manages plugins.</summary>
    public static class PluginManager
    {
        #region Constants
        const string LOG_SEPARATOR = "-------------------------------------";
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
        internal static string PluginConfigPath
        {
            get { return PluginManager.GetPluginPath() + "\\" + "plugins.ini"; }
        }


        /// <summary> Returns a dictionary mapping each plugin to it's origin DLL file.
        /// </summary>
        internal static Dictionary<string, List<IPlugin>> LoadedDLLs
        {
            get { return PluginManager._dllDictionary; }
        }


        /// <summary> Returns the path of the plugin error log file.
        /// </summary>
        private static string errorLogPath
        {
            get { return GetPluginPath() + "\\errorLog.log"; }
        }
        #endregion


        #region Static Contructor
        static PluginManager()
        {
            VacVI.Database.SaveDataReader.OnGameDataUpdate += CallGameDataUpdateOnPlugins;
        }
        #endregion


        #region Functions
        /// <summary> Gets a plugin by name.
        /// </summary>
        public static IPlugin GetPluginByName(string pluginName)
        {
            for (int i = 0; i < Plugins.Count; i++)
            {
                if (String.Equals(Plugins[i].Name, pluginName, StringComparison.InvariantCultureIgnoreCase)) { return Plugins[i]; }
            }

            return null;
        }


        /// <summary> Gets a plugin by ID.
        /// </summary>
        public static IPlugin GetPlugin(string pluginId)
        {
            for (int i = 0; i < Plugins.Count; i++)
            {
                if (String.Equals(Plugins[i].Id.ToString(), pluginId, StringComparison.InvariantCultureIgnoreCase)) { return Plugins[i]; }
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
        internal static void LoadPlugins(bool loadDisabledPlugins = false)
        {
            Plugins.Clear();
            _dllDictionary.Clear();

            ShutdownPlugins();
            _pluginThreads.Clear();

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
                            (_pluginFile.HasKey(plugin.Id.ToString(), "Enabled")) &&
                            (!_pluginFile.ValueIsBoolAndTrue(plugin.Id.ToString(), "Enabled"))
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
                string pluginId = Plugins[i].Id.ToString();

                /* Create entry in the default plugin db */
                if (!_pluginDefaults.ContainsKey(pluginId)) { _pluginDefaults.Add(pluginId, new Dictionary<string, PluginParameterDefault>()); }

                /* Global plugin parameters */
                if (!_pluginFile.HasKey(pluginId, "Enabled")) { _pluginFile.SetValue(pluginId, "Enabled", "False"); }

                /* Fill in custom plugin parameters */
                List<PluginParameterDefault> pluginParams = Plugins[i].GetDefaultPluginParameters();
                for (int u = 0; u < pluginParams.Count; u++)
                {
                    PluginParameterDefault currParam = pluginParams[u];
                    if (!_pluginDefaults[pluginId].ContainsKey(currParam.Key)) { _pluginDefaults[pluginId].Add(currParam.Key, currParam); }

                    if (!_pluginFile.HasKey(pluginId, currParam.Key))
                    {
                        _pluginFile.SetValue(pluginId, currParam.Key, currParam.DefaultValue);
                    }
                }
            }

            _pluginFile.Write(PluginConfigPath);
        }


        /// <summary> Loads all plugin diaog trees.
        /// </summary>
        internal static void InitializePluginDialogTrees()
        {
            for (int i = 0; i < Plugins.Count; i++) { Plugins[i].BuildDialogTree(); }
        }


        /// <summary> Initializes all loaded plugins.
        /// </summary>
        internal static void InitializePlugins()
        {
            ShutdownPlugins();

            for (int i = 0; i < Plugins.Count; i++)
            {
                IPlugin currPlugin = Plugins[i];
                if (
                    (!_pluginThreads.ContainsKey(currPlugin)) ||
                    (_pluginThreads[currPlugin] == null)
                )
                { continue; }

                // Check the state of the thread
                if (!_pluginThreads[currPlugin].IsAlive)
                {
                    Thread execThread = new Thread(new ThreadStart(currPlugin.Initialize));
                    _pluginThreads[currPlugin] = execThread;
                    try { _pluginThreads[currPlugin].Start(); }
                    catch (Exception e) { File.AppendAllText(errorLogPath, e.Message + Environment.NewLine + LOG_SEPARATOR); }

                    // Wait for the end of the initialization (max. 30 seconds)
                    if (_pluginThreads[currPlugin].IsAlive) { _pluginThreads[currPlugin].Join(30000); }
                }
            }
        }


        /// <summary> Calls OnProgramShutdown on each plugin (sequentially, non-threaded).
        /// </summary>
        internal static void ShutdownPlugins()
        {
            // Check for running threads and wait for their completion before shutdown
            foreach (KeyValuePair<IPlugin, Thread> pluginThread in _pluginThreads)
            {
                if (pluginThread.Value == null) { continue; }

                if (pluginThread.Value.IsAlive) { pluginThread.Value.Abort(); }

                // Wait for... an abortion O.o
                if (pluginThread.Value.IsAlive) { pluginThread.Value.Join(); }

                // Tell the plugin to shut down
                pluginThread.Key.OnProgramShutdown();
            }
        }


        /// <summary> Calls OnGameDataUpdate on each plugin (parallel, threaded).
        /// </summary>
        private static void CallGameDataUpdateOnPlugins()
        {
            for (int i = 0; i < Plugins.Count; i++)
            {
                IPlugin currPlugin = Plugins[i];
                if (
                    (!_pluginThreads.ContainsKey(currPlugin)) ||
                    (_pluginThreads[currPlugin] == null)
                )
                { continue; }

                // Check the state of the thread
                if (!_pluginThreads[currPlugin].IsAlive)
                {
                    Thread execThread = new Thread(new ThreadStart(currPlugin.OnGameDataUpdate));
                    _pluginThreads[currPlugin] = execThread;
                    try { execThread.Start(); }
                    catch (Exception e) { File.AppendAllText(errorLogPath, e.Message + Environment.NewLine + LOG_SEPARATOR); }
                }
            }
        }
        #endregion
    }
}