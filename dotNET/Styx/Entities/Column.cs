namespace Rigsarkiv.Styx.Entities
{
    /// <summary>
    /// Column Entity
    /// </summary>
    public class Column
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string TypeOriginal { get; set; }
        public Table CodeList { get; set; }
    }
}
