using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Fusi.Tools;
using Microsoft.Extensions.CommandLineUtils;
using Sidon.Services;
using System.Text;

namespace Sidon.Commands
{
    internal sealed class ImportDocumentsCommand : ICommand
    {
        private readonly ImportDocumentsCommandOptions _options;

        public ImportDocumentsCommand(ImportDocumentsCommandOptions options)
        {
            _options = options;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Import documents matching the specified file mask.";
            command.HelpOption("-?|-h|--help");

            CommandArgument inputFileMaskArgument = command.Argument("[inputFileMask]",
                "The input file(s) mask.");

            CommandArgument dbNameArgument = command.Argument("[dbName]",
                "The target database name.");

            CommandOption dryOption = command.Option("--dry|-d",
                "Dry mode: do not write to database.",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                options.Command = new ImportDocumentsCommand(
                    new ImportDocumentsCommandOptions(options)
                    {
                        InputFileMask = inputFileMaskArgument.Value,
                        DatabaseName = dbNameArgument.Value,
                        IsDryMode = dryOption.HasValue()
                    });
                return 0;
            });
        }

        public Task Run()
        {
            Console.WriteLine("IMPORT DOCUMENTS");
            Console.WriteLine("Input: " + _options.InputFileMask);
            Console.WriteLine("Database: " + _options.DatabaseName);
            Console.WriteLine("Dry mode: " + (_options.IsDryMode ? "yes" : "no"));

            string inputDir = Path.GetDirectoryName(_options.InputFileMask) ?? "";
            int bookNr = 0;
            ICadmusRepository repository = new AppRepositoryProvider(
                _options.Configuration).CreateRepository(_options.DatabaseName!);

            foreach (string path in Directory.GetFiles(
                inputDir,
                Path.GetFileName(_options.InputFileMask) ?? "").OrderBy(s => s))
            {
                // we assume that file names are sorted by book number
                bookNr++;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(path);
                Console.ResetColor();

                using StreamReader textReader = new(
                    new FileStream(path, FileMode.Open, FileAccess.Read,
                    FileShare.Read), Encoding.UTF8);

                SidonReader reader = new(textReader, bookNr)
                {
                    Logger = _options.Logger
                };
                SidonImporter importer = new(reader, repository)
                {
                    IsDryMode = _options.IsDryMode
                };
                importer.Import(CancellationToken.None, new Progress<ProgressReport>(
                    r => Console.WriteLine(r.Message)));
            }

            return Task.CompletedTask;
        }
    }

    internal sealed class ImportDocumentsCommandOptions : AppCommandOptions
    {
        public ImportDocumentsCommandOptions(AppOptions options) : base(options)
        {
        }

        public string? InputFileMask { get; set; }
        public string? DatabaseName { get; set; }
        public bool IsDryMode { get; set; }
    }
}
