using System;

namespace Lohmann.DotEnv 
{
    public class DefaultEnvironmentVariableProvider : IEnvironmentVariableProvider
    {
        public string Get(string variable)
        {
            return System.Environment.GetEnvironmentVariable(variable);
        }

        public void Set(string variable, string value)
        {
            System.Environment.SetEnvironmentVariable(variable, value);
        }
    }
}