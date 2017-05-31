using System;

namespace Lohmann.DotEnv 
{
    public class DefaultEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        public string Get(string variable)
        {
            return System.Environment.GetEnvironmentVariable(variable);
        }

        public void Set(string variable, string value, bool overrideExistingValue = false)
        {
            var existingValue = System.Environment.GetEnvironmentVariable(variable);

            if (existingValue == null) 
            {
                System.Environment.SetEnvironmentVariable(variable, value);
            } 
            else if (overrideExistingValue)
            {
                System.Environment.SetEnvironmentVariable(variable, value);
            }
        }
    }
}