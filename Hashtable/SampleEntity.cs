using System.Text;

public class SampleEntity : IHashTableRowSerializer
{

    public required Guid Guid { get; set; }

    public required DateTime BirthDate { get; set; }

    public required string Name { get; set; }

    public string? GetHashTableRowId()
    {
        return Guid.ToString();
    }

    public (string, byte[])[] SerializeToHashTableRow()
    {
        return [
            (nameof(Guid), Encoding.UTF8.GetBytes(Guid.ToString())),
            (nameof(Name), Encoding.UTF8.GetBytes(Name)),
            (nameof(BirthDate), Encoding.UTF8.GetBytes(BirthDate.ToShortDateString())),
        ];
    }
}