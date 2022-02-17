using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.General.Parts;
using Fusi.Tools;
using Sidon.Services;
using System.Text.RegularExpressions;

namespace Sidon.Commands
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
            string title = $"{doc.Book}.{doc.Number}#{nr} {doc.Title}";

            IItem item = new Item
            {
                Title = title,
                Description = title,
                FacetId = "text",
                GroupId = $"{doc.Book}-{doc.Number:000}",
                SortKey = $"{doc.Book}-{doc.Number:000}-{nr:000}",
                CreatorId = "zeus",
                UserId = "zeus"
            };

            TokenTextPart part = new();
            part.Citation = $"{doc.Book}-{doc.Number} #{nr}" +
                (doc.Blocks[start].IsPoetic ? "*" : "");
            part.ItemId = item.Id;
            part.CreatorId = item.CreatorId;
            part.UserId = item.UserId;

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
            int nr = 0;
            ProgressReport? report = progress != null? new() : null;

            foreach (SidonDocument doc in _reader.Read())
            {
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
                    IItem item = BuildItem(doc, ++nr, start, i - start);
                    if (!IsDryMode) _repository.AddItem(item, true);
                }

                if (cancel.IsCancellationRequested) break;
                if (progress != null)
                {
                    report!.Count++;
                    report.Message = $"{doc.Book}.{doc.Number}";
                    progress.Report(report);
                }
            }
        }
    }
}
