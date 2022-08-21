using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.General.Parts;
using Fusi.Tools;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Sidon.Services
{
    internal sealed class SidonImporter
    {
        private readonly SidonReader _reader;
        private readonly ICadmusRepository _repository;
        private readonly Regex _ws;

        public bool IsDryMode { get; set; }

        public SidonImporter(SidonReader reader, ICadmusRepository repository)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _repository = repository ??
                throw new ArgumentNullException(nameof(repository));
            _ws = new Regex(@"\s+");
        }

        private IItem BuildItem(SidonDocument doc, int nr, int start, int count)
        {
            bool poetic = doc.Blocks[start].IsPoetic;
            string title = $"{doc.Book}_{doc.Number:000}_{nr:000} {doc.Title}" +
                (poetic ? "*" : "");

            IItem item = new Item
            {
                Title = title,
                Description = title,
                FacetId = "text",
                GroupId = $"{doc.Book}-{doc.Number:000}",
                // SortKey = $"{doc.Book}-{doc.Number:000}-{nr:000}",
                CreatorId = "zeus",
                UserId = "zeus",
                Flags = poetic ? 8 : 0
            };

            TokenTextPart part = new()
            {
                Citation = $"{doc.Book}-{doc.Number} #{nr}" +
                    (poetic ? "*" : ""),
                ItemId = item.Id,
                CreatorId = item.CreatorId,
                UserId = item.UserId
            };

            for (int i = 0; i < count; i++)
            {
                part.Lines.Add(new TextLine
                {
                    Y = i + 1,
                    Text = _ws.Replace(doc.Blocks[start + i].Content!, " ").Trim()
                });
            }
            item.Parts.Add(part);

            return item;
        }

        public void Import(CancellationToken cancel,
            IProgress<ProgressReport>? progress = null)
        {
            ProgressReport? report = progress != null? new() : null;

            foreach (SidonDocument doc in _reader.Read())
            {
                if (progress != null)
                {
                    report!.Count++;
                    report.Message = $"--- EP {doc.Book}.{doc.Number}";
                    progress.Report(report);
                }

                int itemNr = 0;
                int i = 0;
                while (i < doc.Blocks.Count)
                {
                    bool isPoetic = doc.Blocks[i].IsPoetic;
                    int start = i++;

                    while (i < doc.Blocks.Count
                           && isPoetic == doc.Blocks[i].IsPoetic
                           && doc.Blocks[i].Number == 0)
                    {
                        i++;
                    }
                    IItem item = BuildItem(doc, ++itemNr, start, i - start);
                    Debug.Assert(item.Parts.Count > 0);
                    if (!IsDryMode)
                    {
                        _repository.AddItem(item, true);
                        _repository.AddPart(item.Parts[0], true);
                    }
                    if (progress != null)
                    {
                        report!.Message = item.ToString();
                        progress.Report(report);
                    }
                }

                if (cancel.IsCancellationRequested) break;
            }
        }
    }
}
