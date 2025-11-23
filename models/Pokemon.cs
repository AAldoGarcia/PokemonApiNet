namespace NoSeProgramarJajajLoL.models
{
    public class Pokemon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string SpriteUrl { get; set; }
        public List<string> Types { get; set; } = new();
        public Dictionary<string, int> Stats { get; set; } = new();
    }
}