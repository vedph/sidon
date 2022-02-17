using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sidon.Commands
{
    /// <summary>
    /// Base class for command options including <see cref="AppOptions"/>.
    /// </summary>
    internal abstract class AppCommandOptions
    {
        private readonly AppOptions _options;

        /// <summary>
        /// Gets or sets the command.
        /// </summary>
        public ICommand? Command => _options.Command;

        /// <summary>
        /// The configuration.
        /// </summary>
        public IConfiguration Configuration => _options.Configuration;

        /// <summary>
        /// The logger.
        /// </summary>
        public ILogger Logger => _options.Logger;

        protected AppCommandOptions(AppOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
