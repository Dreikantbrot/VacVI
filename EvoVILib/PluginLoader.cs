using EvoVI.PluginContracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EvoVI
{
    public static class PluginLoader
    {
        #region Variables
        public static List<IPlugin> Plugins = new List<IPlugin>();
        private static IniFile _pluginConfig;
        #endregion


        #region Properties
        /// <summary> Returns the last laded plugin configuration.
        /// </summary>
        public static IniFile PluginConfig
        {
            get { return PluginLoader._pluginConfig; }
        }
        #endregion


        #region Functions
        /// <summary> Loads all plugins inside the [ApplicationPath]/Plugins folder.
        /// </summary>
        public static void LoadPlugins(bool loadDisabledPlugins=false)
        {
            Plugins.Clear();

            string[] dllFileNames = null;
            string pluginPath = GetPluginPath();

            _pluginConfig = new IniFile(pluginPath + "\\" + "plugins.ini");
            _pluginConfig.Read();

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
                            (_pluginConfig.HasKey(plugin.Name, "Enabled")) &&
                            (!_pluginConfig.ValueIsBoolAndTrue(plugin.Name, "Enabled"))
                        )
                    )
                    { continue; }
                    
                    Plugins.Add(plugin);
                }
            }
        }


        /// <summary> Gets a plugin by name.
        /// </summary>
        public static IPlugin GetPlugin(string pluginName)
        {
            for (int i = 0; i < Plugins.Count; i++) { if (Plugins[i].Name == pluginName) return Plugins[i]; }

            return null;
        }


        public static string GetPluginPath()
        {
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
            return appPath + "\\" + "Plugins";
        }


        /// <summary> Initializes all loaded plugins.
        /// </summary>
        public static void InitializeAll()
        {
            for (int i = 0; i < Plugins.Count; i++) { Plugins[i].Initialize(); }
        }
        #endregion
    }
}