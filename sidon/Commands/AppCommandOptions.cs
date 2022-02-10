namespace Sidon.Commands
{
    /// <summary>
    /// Base class for command options including <see cref="AppOptions"/>.
    /// </summary>
    internal abstract class AppCommandOptions
    {
        public AppOptions AppOptions { get; }

        protected AppCommandOptions(AppOptions options)
        {
            AppOptions = options
                ?? throw new ArgumentNullException(nameof(options));
        }
    }
}
