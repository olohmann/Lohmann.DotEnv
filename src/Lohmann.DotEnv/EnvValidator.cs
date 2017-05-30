using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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

        public EnvValidatorResult Validate(Type modelType)
        {
            return Validate(new string[] {});
        } 

        public EnvValidatorResult Validate<TModel>() 
        {
            var type = typeof(TModel);
            return Validate(type);
        }
    }
}