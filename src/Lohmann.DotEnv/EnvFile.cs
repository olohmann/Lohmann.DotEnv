using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lohmann.DotEnv
{
    /// <summary>
    /// Loads environment variables from a .env file into your .NET application's process environment variables.
    /// Storing configuration in the environment separate from code is based on [The Twelve-Factor App](https://12factor.net/config) methodology.
    /// </summary>
    public class EnvFile
    {
        private static readonly Regex KeyValueRegex =
            new Regex(@"^\s*([\w\.\-]+)\s*=\s*(.*)?\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.Singleline);

        private IEnvironmentVariableProvider _environmentVariableProvider;

        private static Lazy<EnvFile> _default =
            new Lazy<EnvFile>(() => new EnvFile(new DefaultEnvironmentVariableProvider()));

        /// <summary>
        /// Creates an EnvFile with the default provider for environment variables.
        /// </summary>
        public EnvFile()
        {
            _environmentVariableProvider = new DefaultEnvironmentVariableProvider();
        }

        /// <summary>
        /// Creates an EnvFile with a specific provider for environment variables.
        /// </summary>
        /// <param name="environmentVariableProvider">Specify a custom provider for setting/getting environment variables.</param>
        public EnvFile(IEnvironmentVariableProvider environmentVariableProvider)
        {
            _environmentVariableProvider = environmentVariableProvider;
        }

        /// <summary>
        /// Returns the default instance of the EnvFile class.
        /// </summary>
        /// <returns>The default instance.</returns>
        public static EnvFile Default
        {
            get
            {
                return _default.Value;
            }
        }

        /// <summary>
        /// /// Loads the default dotenv file './.env' and sets corresponding environment variables.
        /// </summary>
        /// <param name="throwWhenFileNotExists">Throw an exception if the provided file was not found.</param>
        /// <returns>A task signaling the completion of the load operation.</returns>
        public Task LoadAsync(bool throwWhenFileNotExists = true)
        {
            return LoadAsync("./.env", throwWhenFileNotExists);
        }

        /// <summary>
        /// Loads the specified dotenv file and sets corresponding environment variables.
        /// </summary>
        /// <param name="path">The path to the environment file.</param>
        /// <param name="throwWhenFileNotExists">Throw an exception if the provided file was not found.</param>
        /// <returns>A task signaling the completion of the load operation.</returns>
        public async Task LoadAsync(string path, bool throwWhenFileNotExists = true)
        {
            if (!File.Exists(path))
            {
                if (throwWhenFileNotExists)
                {
                    throw new FileNotFoundException("The environment file was not found.", path);
                }
                else 
                {
                    return;
                }
            }

            using (Stream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                await LoadAsync(fileStream).ConfigureAwait(continueOnCapturedContext: false);
            }
        }

        /// <summary>
        /// Loads the environment settings from the given stream and sets corresponding environment variables.
        /// </summary>
        /// <param name="stream">Stream with environment settings.</param>
        /// <returns>A task signaling the completion of the load operation.</returns>
        public async Task LoadAsync(Stream stream)
        {
            var result = new Dictionary<string, string>();
            using (var streamReader = new StreamReader(stream))
            {
                string line;
                do
                {
                    line = await streamReader.ReadLineAsync().ConfigureAwait(continueOnCapturedContext: false);
                    if (line != null)
                    {
                        var lineResult = ParseLine(line);
                        if (lineResult.HasValue)
                        {
                            result.Add(lineResult.Value.key, lineResult.Value.value);
                        }
                    }
                } while (line != null);

            }

            foreach (var keyValuePair in result)
            {
                _environmentVariableProvider.Set(keyValuePair.Key, keyValuePair.Value);
            }
        }

        private (string key, string value)? ParseLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return null;
            }

            var match = KeyValueRegex.Match(line);
            if (!match.Success)
            {
                return null;
            }

            string key = match.Groups[1].Value.Trim();
            string value = match.Groups[2].Value.Trim();

            // Replace escaped new lines on quoted strings.
            if (value.Length > 0
                && ((value[0] == '"' && value[value.Length - 1] == '"') ||
                    (value[0] == '\'' && value[value.Length - 1] == '\'')))
            {
                value = Regex.Replace(value, "^['\"]", "");
                value = Regex.Replace(value, "['\"]$", "");
                value = value.Trim();
                value = value.Replace("\n", Environment.NewLine);
            }

            return (key, value);
        }
    }
}
