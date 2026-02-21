namespace KawaiiCandyBox.Content
{
    /// <summary>
    /// Implemented by all ScriptableObject definition types that
    /// have a unique string ID (LocationDefinition, QuestDefinition, etc.)
    /// This lets ContentRegistry.GetById() work generically across all types.
    /// </summary>
    public interface IIdentifiable
    {
        string Id { get; }
    }
}