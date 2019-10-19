namespace Spectre.Cli.Tests.Data.Settings
{
    public class OptionVectorSettings : CommandSettings
    {
        [CommandOption("--foo")]
        public string[] Foo { get; set; }

        [CommandOption("--bar")]
        public int[] Bar { get; set; }
    }
}