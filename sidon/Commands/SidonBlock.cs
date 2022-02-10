namespace sidon.Commands
{
    internal class SidonBlock
    {
        public int Number { get; set; }
        public bool IsPoetic { get; set; }
        public string? Content { get; set; }

        public override string ToString()
        {
            return $"{(IsPoetic ? "P" : "R")}{Number}: {Content}";
        }
    }
}
