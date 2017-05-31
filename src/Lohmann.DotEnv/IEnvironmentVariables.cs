namespace Lohmann.DotEnv 
{
    public interface IEnvironmentVariableProvider
    {
        string Get(string variable);
        void Set(string variable, string value, bool overrideExistingValue = false);
    }
}