using System.Linq;

namespace EvoVI.Plugins
{
    public struct PluginParameterDefault
    {
        #region Variables
        private string _key;
        private string _defaultValue;
        private string[] _allowedValues;
        #endregion


        #region Properties
        /// <summary> Returns the parameter key.
        /// </summary>
        public string Key
        {
            get { return _key; }
        }


        /// <summary> Returns the default parameter value.
        /// </summary>
        public string DefaultValue
        {
            get { return _defaultValue; }
        }


        /// <summary> Returns allowed values for the parameter.
        /// </summary>
        public string[] AllowedValues
        {
            get { return _allowedValues; }
        }
        #endregion


        #region Constructor
        /// <summary> Creates a struct containing information about a plugin's configurable parameters.
        /// </summary>
        /// <param name="pParamName">The name of the parameter (key).</param>
        /// <param name="pDefaultValue">The parameter's default value.
        /// Set to null, if there is no default value.
        /// </param>
        /// <param name="pAllowedValues">A list of valid values for the parameter.
        /// Set to null, to allow any value.
        /// </param>
        public PluginParameterDefault(string pParamName, string pDefaultValue = null, string[] pAllowedValues = null)
        {
            this._key = pParamName;
            this._defaultValue = pDefaultValue;
            this._allowedValues = pAllowedValues;
        }
        #endregion


        #region Functions
        /// <summary> Returns whether the current value is in range of the allowed values.
        /// </summary>
        /// <param name="currentValue">The value to check.</param>
        /// <returns>Whether the current value is in range of the allowed values.</returns>
        public bool IsValueAllowed(string currentValue)
        {
            return (
                (_allowedValues == null) ||
                (_allowedValues.Contains(currentValue))
            );
        }
        #endregion
    }
}
