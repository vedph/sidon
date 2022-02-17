using Microsoft.Extensions.CommandLineUtils;
using Sidon.Services;
using System.Text;

namespace Sidon.Commands
{
    internal sealed class DumpDocumentsCommand : ICommand
    {
        private readonly DumpDocumentsCommandOptions _options;

        public DumpDocumentsCommand(DumpDocumentsCommandOptions options)
        {
            _options = options;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Dump documents matching the specified file mask.";
            command.HelpOption("-?|-h|--help");

            CommandArgument inputFileMaskArgument = command.Argument("[inputFileMask]",
                "The input file(s) mask.");

            CommandArgument outputDirArgument = command.Argument("[outputDirectory]",
                "The output directory.");

            command.OnExecute(() =>
            {
                options.Command = new DumpDocumentsCommand(
                    new DumpDocumentsCommandOptions(options)
                    {
                        InputFileMask = inputFileMaskArgument.Value,
                        OutputDirectory = outputDirArgument.Value,
                    });
                return 0;
            });
        }

        public Task Run()
        {
            Console.WriteLine("DUMP DOCUMENTS");
            Console.WriteLine("Input: " + _options.InputFileMask);
            Console.WriteLine("Output: " + _options.OutputDirectory);

            // create output dir if not exists
            if (!Directory.Exists(_options.OutputDirectory))
                Directory.CreateDirectory(_options.OutputDirectory!);

            string inputDir = Path.GetDirectoryName(_options.InputFileMask) ?? "";
            int bookNr = 0;

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

                string outputPath = Path.Combine(_options.OutputDirectory ?? "",
                    Path.GetFileName(path));
                using StreamWriter textWriter = new(
                    new FileStream(outputPath, FileMode.Create, FileAccess.Write,
                    FileShare.Read), Encoding.UTF8);

                SidonReader reader = new(textReader, bookNr)
                {
                    Logger = _options.Logger
                };
                foreach (SidonDocument document in reader.Read())
                {
                    textWriter.WriteLine($"=== {document.Book}.{document.Number}: "
                        + document.Title);
                    foreach (SidonBlock? block in document.Blocks)
                    {
                        textWriter.Write(block.IsPoetic? '@':'#');
                        if (block.Number > 0) textWriter.Write($"{block.Number}:");
                        textWriter.WriteLine(block.Content);
                    }
                    textWriter.WriteLine();
                }
                textWriter.Flush();
            }

            return Task.CompletedTask;
        }
    }

    internal class DumpDocumentsCommandOptions : AppCommandOptions
    {
        public DumpDocumentsCommandOptions(AppOptions options) : base(options)
        {
        }

        public string? InputFileMask { get; set; }
        public string? OutputDirectory { get; set; }
    }
}
