# Lohmann.DotEnv

Loads environment variables from a .env file into your .NET application's process environment variables. 
Storing configuration in the environment separate from code is based on [The Twelve-Factor App](https://12factor.net/config) methodology.

The .env-file format is compatible to the NodeJS library [motdotla/dotenv](https://github.com/motdotla/dotenv).


## Install

```
dotnet add package Lohmann.DotEnv
```

## Usage

### Default
Create a .env file located right beside your executable:
```
HOST=api.contoso.com
TOKEN=b3LvpJ7ORbp4qu7P5socUg==
```

Setup your (console) app to use the .env file:
```
    class Program
    {
        static void Main(string[] args)
        {
            System.Threading.CancellationTokenSource cts = new System.Threading.CancellationTokenSource();
            System.Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            MainAsync(args, cts.Token).Wait();
        }

        static async Task MainAsync(string[] args, System.Threading.CancellationToken cancellationToken)
        {
            // You load the config off the .env file by issung the LoadAsync method.
            await Lohmann.DotEnv.EnvFile.Default.LoadAsync();

            // You can use the built-in validator to check if all required env variables are set:
            var validationResult = Lohmann.DotEnv.EnvValidator.Default.Validate(new [] { "HOST", "TOKEN" });
            if (validationResult.HasValidationErrors) 
            {
                // ... report errors
                System.Environment.Exit(-1);
            }

            // Now you can access Environment Variables as follows:
            var host = System.Environment.GetEnvironmentVariable("HOST");
            var token = System.Environment.GetEnvironmentVariable("TOKEN");
        }
    }
```

### Customization

* You can override the env file path in the `LoadAsync` method.
* You can also load the env config from a stream as an `LoadAsync` overload.
* You can change the error behavior for a missing env file via `LoadAsync(throwWhenFileNotExists: false)`

## FAQ

### Should I commit the .env file?
**No!** - The .env file should only contain environment specific values such as connection strings or tokens. Since these values are **extremley** sensible, you should never commit them into source control.

## License
See [LICENSE](LICENSE)
