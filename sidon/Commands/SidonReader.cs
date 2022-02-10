using Fusi.Tools;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;

namespace sidon.Commands
{
    internal class SidonReader
    {
        private readonly TextReader _reader;
        private bool _eof;
        private readonly Regex _headerRegex;
        private readonly Regex _nrRegex;

        public ILogger? Logger { get; set; }

        public SidonReader(TextReader reader)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _headerRegex = new Regex(@"^EPISTULA\s+([IVXLC]+)");
            _nrRegex = new Regex(@"^(\d+)\.\s*");
        }

        private void ParseDocument(SidonDocument document)
        {
            for (int i = 0; i < document.Blocks.Count; i++)
            {
                SidonBlock block = document.Blocks[i];

                // .N prose
                Match m = _nrRegex.Match(block.Content!);
                if (m.Success)
                {
                    block.Number = int.Parse(m.Groups[1].Value,
                        CultureInfo.InvariantCulture);
                    block.Content = block.Content![m.Length..];
                    continue;
                }

                switch (block.Content![0])
                {
                    case '@':   // poetry
                        block.IsPoetic = true;
                        block.Content = block.Content![1..];
                        break;
                    case '#':   // prose
                        block.Content = block.Content![1..];
                        break;
                    case ' ':   // indent = poetry
                        block.IsPoetic = true;
                        block.Content = block.Content.Trim();
                        break;
                    default:
                        // not indent but followed by 3-spaces = poetry
                        if (i + 1 < document.Blocks.Count
                            && document.Blocks[i + 1].Content!.StartsWith("   "))
                        {
                            block.IsPoetic = true;
                            block.Content = block.Content.Trim();
                        }
                        // else prose
                        break;
                }
            }
        }

        public IEnumerable<SidonDocument> Read(int bookNumber)
        {
            if (_eof) yield break;

            int lineNr = 0;
            string? line;
            SidonDocument? document = null;

            // collect all the documents
            while ((line = _reader.ReadLine()) != null)
            {
                // skip title
                if (++lineNr == 1) continue;

                // a blank line starts a new document
                if (line.Length == 0)
                {
                    if (document != null)
                    {
                        ParseDocument(document);
                        Logger?.LogInformation($"Document: {document}");
                        yield return document;
                    }

                    // next is EPISTULA n
                    document = new SidonDocument
                    {
                        Book = bookNumber
                    };
                    line = _reader.ReadLine();
                    lineNr++;
                    if (line == null) break;
                    Match m = _headerRegex.Match(line);
                    if (!m.Success)
                    {
                        Logger?.LogError(
                            $"[{lineNr}] expected Epistula header not found");
                        continue;
                    }
                    document.Number = RomanNumber.FromRoman(m.Groups[1].Value);

                    // next is title
                    document.Title = _reader.ReadLine();
                    lineNr++;
                    if (document.Title == null)
                    {
                        Logger?.LogError(
                            $"[{lineNr}] expected Epistula title not found");
                        break;
                    }
                }
                else if (document != null)
                {
                    document.Blocks.Add(new SidonBlock { Content = line });
                }
            }
            _eof = true;
        }
    }
}
