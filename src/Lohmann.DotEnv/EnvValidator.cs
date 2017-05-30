using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Lohmann.DotEnv
{
    /// <summary>
    /// Validates the presence of environment variables in your .NET application's process.
    /// </summary>
    public class EnvValidator
    {
        private IEnvironmentVariableProvider _environmentVariableProvider;

        private static Lazy<EnvValidator> _default =
            new Lazy<EnvValidator>(() => new EnvValidator(new DefaultEnvironmentVariableProvider()));

        public EnvValidator(IEnvironmentVariableProvider environmentVariableProvider)
        {
            _environmentVariableProvider = environmentVariableProvider;
        }

        /// <summary>
        /// Returns the default instance of the EnvValidator class.
        /// </summary>
        /// <returns>The default instance.</returns>
        public static EnvValidator Default
        {
            get
            {
                return _default.Value;
            }
        }

        /// <summary>
        /// Validates the setting of the provided variables.
        /// A variable is considered defined it its value is != empty, whitespace or null.
        /// </summary>
        /// <param name="requiredVariables"></param>
        /// <returns>The validation results.</returns>
        public EnvValidatorResult Validate(IEnumerable<string> requiredVariables) 
        {
            var results = new List<string>();

            foreach(var requiredVariable in requiredVariables)
            {
                var value = _environmentVariableProvider.Get(requiredVariable);
                if (string.IsNullOrWhiteSpace(value)) 
                {
                    results.Add($"Missing environment variable definition for '{requiredVariable}'.");
                }
            }

            return new EnvValidatorResult(results);
        }

        /// <summary>
        /// Validates the setting of environment variables by scanning the provided model type get'able properties.
        /// A variable is considered defined it its value is != empty, whitespace or null.
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns>The validation results.</returns>
        public EnvValidatorResult Validate(Type modelType)
        {
            var readablePropertyNames = modelType.GetRuntimeProperties()
                .Where(p => p.CanRead && p.PropertyType == typeof(string))
                .Select(p => p.Name);

            return Validate(readablePropertyNames);
        } 

        /// <summary>
        /// Validates the setting of environment variables by scanning the provided model type get'able properties.
        /// A variable is considered defined it its value is != empty, whitespace or null.
        /// </summary>
        /// <returns>The validation results.</returns>
        public EnvValidatorResult Validate<TModel>() 
        {
            var type = typeof(TModel);
            return Validate(type);
        }
    }
}