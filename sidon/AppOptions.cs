using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Sidon.Commands;

namespace Sidon
{
    internal sealed class AppOptions
    {
        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public ICommand? Command { get; set; }

        /// <summary>
        /// The configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// The logger.
        /// </summary>
        public ILogger Logger { get; }

        /// <summary>
        /// Create a new instance of the <see cref="AppOptions"/> class.
        /// </summary>
        public AppOptions()
        {
            Configuration = BuildConfiguration();
            Logger = new SerilogLoggerProvider(Serilog.Log.Logger)
                .CreateLogger(nameof(Program));
        }

        private static IConfiguration BuildConfiguration()
        {
            ConfigurationBuilder cb = new();
            return cb
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
        }

        public static AppOptions? Parse(string[] args)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            AppOptions options = new();
            CommandLineApplication app = new()
            {
                Name = "Cursus",
                FullName = "Cursus CLI"
            };
            app.HelpOption("-?|-h|--help");

            // app-level options
            RootCommand.Configure(app, options);

            int result = app.Execute(args);
            return result != 0 ? null : options;
        }
    }
}
