public interface IHashTableSerializer
{
    
}

public interface IHashTableRowSerializer
{
    /// <summary>
    /// This method should serialize all the fields into the entity that should
    /// be included in the HashTable struct. 
    /// 
    /// Remember: If this method changes the order of fields for some reason and there 
    /// is no custom id returned by GetHashTableRowId this method maybe always create difference between
    /// older versions of HashTable
    /// </summary>
    /// <returns></returns>
    public (string, byte[])[] SerializeToHashTableRow();

    /// <summary>
    /// Return a custom identifier for the row. If no custom identifier
    /// is defined this method should return null
    /// </summary>
    /// <returns></returns>
    public string? GetHashTableRowId();
}