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
        #endregion


        #region Functions
        /// <summary> Loads all plugins inside the [ApplicationPath]/Plugins folder.
        /// </summary>
        public static void LoadPlugins()
        {
            Plugins.Clear();

            string[] dllFileNames = null;
            string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase).Replace("file:\\", "");
            string pluginPath = appPath + "\\" + "Plugins";

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
                        (String.IsNullOrWhiteSpace(plugin.Name))
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


        /// <summary> Initializes all loaded plugins.
        /// </summary>
        public static void InitializeAll()
        {
            for (int i = 0; i < Plugins.Count; i++) { Plugins[i].Initialize(); }
        }
        #endregion
    }
}