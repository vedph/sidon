namespace sidon.Commands
{
    internal class SidonDocument
    {
        public int Book { get; set; }
        public int Number { get; set; }
        public string? Title { get; set; }
        public IList<SidonBlock> Blocks { get; }

        public SidonDocument()
        {
            Blocks = new List<SidonBlock>();
        }

        public override string ToString()
        {
            return $"{Book}.{Number} {Title} ({Blocks.Count})";
        }
    }
}
