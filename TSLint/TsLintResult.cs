namespace TSLint
{
    internal class TsLintPosition
    {
        public int Character { get; set; }
        public int Line { get; set; }
    }

    internal class TsLintResult
    {
        public TsLintPosition StartPosition { get; set; }
        public TsLintPosition EndPosition { get; set; }
        public string Failure { get; set; }
        public string RuleName { get; set; }
    }
}
