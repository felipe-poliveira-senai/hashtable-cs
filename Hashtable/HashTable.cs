using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

public class HashTable
{
    /// <summary>
    /// Store the hash of all the content of the hash table
    /// </summary>
    private byte[]? _tableHash;

    /// <summary>
    /// Store the hashes for each row
    /// </summary>
    public IDictionary<string, HashTableRow> RowsHashes { get; private set; }

    /// <summary>
    /// Store the state of the message digester for TableHash
    /// </summary>
    public HashAlgorithm _tableHashDigester;

    /// <summary>
    /// Store a function that will serve as seed to produce message digesters
    /// </summary>
    private Func<HashAlgorithm> _hashAlgorithmSeeder;

    public HashTable(Func<HashAlgorithm> hashAlgSeeder, int initialCapacity)
    {
        _hashAlgorithmSeeder = hashAlgSeeder;
        _tableHashDigester = hashAlgSeeder();
        RowsHashes = new Dictionary<string, HashTableRow>(initialCapacity);
    }

    public static HashTable FromHashTableRowSerializer(Func<HashAlgorithm> hashAlgSeeder, ICollection<IHashTableRowSerializer> data)
    {
        // The hash table that will be returned
        var hashTable = new HashTable(hashAlgSeeder, data.Count);

        // Mark the row index 
        var rowIndex = -1;
        foreach (var serializedData in data)
        {
            // Advance the row id
            rowIndex++;

            // The rowId will be the serializedData custom id or the rowId if not defined
            string? rowId = serializedData.GetHashTableRowId();
            if (rowId == null)
            {
                rowId = rowIndex.ToString();
            }

            // Create the row and add the values from the serializedData into it
            var row = hashTable.AddRow(rowId);
            foreach (var (colId, colValue) in serializedData.SerializeToHashTableRow())
            {
                row.Add(colId, colValue);
            }
        }

        return hashTable;
    }

    /// <summary>
    /// Create a HashTableRow into the HashTable
    /// </summary>
    /// <param name="rowIdentifier"></param>
    /// <returns></returns>
    public HashTableRow AddRow(string rowIdentifier)
    {
        var newRow = new HashTableRow(this, _hashAlgorithmSeeder);
        RowsHashes.Add(rowIdentifier, newRow);
        return newRow;
    }

    /// <summary>
    /// Create a HashTableDifference instance based on the comparison between this Hash Table and a given hash table
    /// </summary>
    /// <param name="otherHashTable"></param>
    /// <param name="hashTableDifferenceMode"></param>
    /// <returns></returns>
    public HashTableDifference Difference(HashTable otherHashTable, HashTableDifferenceMode hashTableDifferenceMode = HashTableDifferenceMode.Shallow)
    {
        return new HashTableDifference(this, otherHashTable, hashTableDifferenceMode);
    }

    /// <summary>
    /// Return the hash of all the content
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public byte[] GetTableHash()
    {
        if (_tableHash == null)
        {
            throw new Exception("Can not return TableHash because it is null. Use .AddRow() and TableHashRow.Add() before using this method");
        }

        return _tableHash;
    }

    /// <summary>
    /// Update the table hash with the given value
    /// </summary>
    /// <param name="value"></param>
    private void UpdateTableHash(byte[] value)
    {
        _tableHashDigester.ComputeHash((_tableHash != null) ? _tableHash.Concat(value) : value);
        _tableHash = _tableHashDigester.Hash;
    }

    public class HashTableRow : IDisposable
    {

        public HashTableRow(HashTable hashTable, Func<HashAlgorithm> hashAlgorithmSeeder)
        {
            Table = hashTable;
            Hashes = new Dictionary<string, byte[]>();
            _rowHashDigester = hashAlgorithmSeeder();
        }

        public HashTable Table { get; private set; }

        /// <summary>
        /// Store the hashes for each value in the row
        /// </summary>
        public IDictionary<string, byte[]> Hashes { get; private set; }

        /// <summary>
        /// Store the total hash of the row
        /// </summary>
        private byte[]? _rowHash;

        /// <summary>
        /// The algorithm used to hash each value in the row
        /// </summary>
        private HashAlgorithm _rowHashDigester;

        /// <summary>
        /// Flag that indicates if the row has being disposed. If a row is disposed it can not
        /// add more values into it as the .Dispose() method will also calculate the table hash for the row
        /// </summary>
        private bool _disposed = false;

        public void Add(string cellId, byte[] value)
        {
            // Can not add if the row has being disposed
            if (_disposed)
            {
                throw new Exception("Can not add more values into the row if it has being disposed");
            }

            // Create a new hash digester for the given value,
            // Add the digestedValue in the Hashes dictionary
            var digestedValue = _rowHashDigester.ComputeHash(value);
            Hashes[cellId] = digestedValue;

            // Include the value in the total row hash
            _rowHash = _rowHashDigester.ComputeHash((_rowHash != null) ? _rowHash.Concat(value) : value);

            // Call the Table to update its hash value
            Table.UpdateTableHash(value);
        }

        /// <summary>
        /// Return the hash of the row content. If the row has has never added a content with .Add method this 
        /// method will throw a exception
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public byte[] GetRowHash()
        {
            if (_rowHash == null)
            {
                throw new Exception("Can not return RowHash because it is null. Use .Add() before retrieving the row hash");
            }

            return _rowHash;
        }

        public void Dispose()
        {
            _disposed = true;
        }
    }
}

public class HashTableDifference
{
    public HashTableDifference(HashTable hashTable1, HashTable hashTable2, HashTableDifferenceMode differenceMode = HashTableDifferenceMode.Deep)
    {
        RowsWithDifferences = new Dictionary<string, HashTableRowDifference>();
        LookForDifferences(hashTable1, hashTable2, differenceMode);
    }

    /// <summary>
    /// Store all the differences 
    /// </summary>
    public IDictionary<string, HashTableRowDifference> RowsWithDifferences { get; private set ;}

    /// <summary>
    /// Return a flag indicating if the Hash Table has differences
    /// </summary>
    /// <returns></returns>
    public bool HasDifferences() => RowsWithDifferences.Count > 0;

    /// <summary>
    /// Main method to check for differences between two HashTable
    /// </summary>
    /// <param name="hashTable1">The hash table used as reference</param>
    /// <param name="hashTable2">The hash table that will be compared with the reference hash table</param>
    /// <param name="differenceMode">How deep the differences should be returned</param>
    /// <exception cref="Exception"></exception>
    private void LookForDifferences(HashTable hashTable1, HashTable hashTable2, HashTableDifferenceMode differenceMode = HashTableDifferenceMode.Deep)
    {
        // Ignore of table are the same
        if (hashTable1.GetTableHash().SequenceEqual(hashTable2.GetTableHash()))
        {
            return;
        }

        HashSet<string> verifiedRows = new HashSet<string>();
        
        // Iterate over each row in hashTable1
        foreach (var table1Entries in hashTable1.RowsHashes)
        {
            var rowKeyInTable1 = table1Entries.Key;
            var rowInTable1 = table1Entries.Value;

            // add the row key to the verified rows set
            verifiedRows.Add(rowKeyInTable1);

            // if the row does not exists in table 2 consider it removed
            if (!hashTable2.RowsHashes.ContainsKey(rowKeyInTable1))
            {
                RowsWithDifferences.Add(rowKeyInTable1, new(DifferenceType.Removed));
                continue;
            }

            // if the row exists and the hash does not match considered it changed
            var rowInTable2 = hashTable2.RowsHashes[rowKeyInTable1] ?? throw new Exception($"Row not found in hashTable2: {rowKeyInTable1}");
            if (!rowInTable1.GetRowHash().SequenceEqual(rowInTable2.GetRowHash()))
            {
                RowsWithDifferences.Add(rowKeyInTable1, new(DifferenceType.Changed));
            }
        }

        // Now for each row in hashTable2 does was not verified it means that the row is new
        foreach (var rowEntryInTable2 in hashTable2.RowsHashes.Where(rowEntry => !verifiedRows.Contains(rowEntry.Key)))
        {
            RowsWithDifferences.Add(rowEntryInTable2.Key, new(DifferenceType.New));
        }
    }


    /// <summary>
    /// Store the difference between two HashTableRow
    /// </summary>
    public class HashTableRowDifference
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="differenceType"></param>
        public HashTableRowDifference(DifferenceType differenceType)
        {
            ColumnsWithDifferences = new Dictionary<string, byte[]>();
            DifferenceType = differenceType;
        }

        /// <summary>
        /// The columns with the differences
        /// </summary>
        public IDictionary<string, byte[]> ColumnsWithDifferences { get; private set ;}

        /// <summary>
        /// The difference type in the row
        /// </summary>
        public DifferenceType DifferenceType { get; private set ;}
    }
}

/// <summary>
/// </summary>
public enum HashTableDifferenceMode
{
    Shallow,
    Deep,
}

/// <summary>
/// The types of differences of digital content hash comparison
/// </summary>
public enum DifferenceType
{
    New,
    Removed,
    Changed,
}

/// <summary>
/// Add functionalities to StreamReader object
/// </summary>
public static class StreamReaderExtensions
{

    /// <summary>
    /// Expect that the content of the StreamReader return an .csv file, then parse the content into a HashTable
    /// using \n as line separators (for HashTableRow) and separate each column with the value of separator
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="hashAlgSeeder"></param>
    /// <param name="separator">Symbol that identifies cells in a CSV file content. Default value = ','</param>
    /// <returns></returns>
    public static HashTable FromCsvToHashTable(
        this StreamReader reader, 
        Func<HashAlgorithm> hashAlgSeeder, 
        string separator = ",", 
        int[]? cellIndexesUsedAsRowIdentifier = null, 
        bool useFirstRowAsColumnIdentifier = false,
        bool ignoreDuplicatedRowIds = false
        )
    {
        // Create the Hash Table based on 
        HashTable? hashTable = null;
        using (reader)
        {
            // Split each line from the CSV file
            var lines = reader.ReadToEnd().ReplaceLineEndings().Split(Environment.NewLine);
            hashTable = new HashTable(hashAlgSeeder, lines.Length);
            int rowIndex = -1;

            // Store the maximum value of the cellIndexesUsedAsIdentifier
            int greaterCellIndexUsedAsIdentifier = (cellIndexesUsedAsRowIdentifier != null && cellIndexesUsedAsRowIdentifier.Length > 0) ? cellIndexesUsedAsRowIdentifier.Max() : 0;

            // Store the cell identifiers
            IList<string>? cellIdentifiers = null;
            foreach (var line in lines)
            {
                // go to the next row
                rowIndex++;

                // get all the cells from the current line
                var cells = CsvUtils.ReadCsvLine(line, separator);

                // if the cells does not have any content go to the next
                if (cells.Length == 0)
                {
                    continue;
                }

                // If is the first row and the flag is marked as true use the first row only to set the identifiers
                if (rowIndex == 0 && useFirstRowAsColumnIdentifier)
                {
                    cellIdentifiers = cells.ToList();
                    continue;
                }

                // This variable store the value used to identify the row as an id
                string? rowId = null;

                // If the cell indexes are defined and the greater value in the array is lesser than the number of cells
                // in the row:
                // Concat all the values from the current cells identified by the given id indexes to compose the 'rowId'
                if (cellIndexesUsedAsRowIdentifier != null && greaterCellIndexUsedAsIdentifier < cells.Length)
                {
                    foreach (var cellIndexUsedAsIdentifier in cellIndexesUsedAsRowIdentifier)
                    {
                        rowId += cells[cellIndexUsedAsIdentifier];
                    }
                }

                // if the rowId was not set use the rowIndex as a id
                if (rowId == null)
                {
                    rowId = rowIndex.ToString();
                }

                // ignore duplicated row keys
                if (ignoreDuplicatedRowIds && hashTable.RowsHashes.ContainsKey(rowId))
                {
                    continue;
                }

                // for each line create a new HasTableRow
                var row = hashTable.AddRow(rowId);
                int cellIndex = -1;

                // And for each split value add it on the row
                foreach (var cell in cells)
                {
                    // Go to the next cell
                    cellIndex++;

                    // If the cell identifiers are not null and
                    // the current cell index is greater the amount of elements in the collection throw an exception
                    if (cellIdentifiers != null && cellIndex >= cellIdentifiers.Count)
                    {
                        throw new Exception($"The number of cellIdentifiers are lesser than the current cellIndex: {cellIndex}");
                    }

                    // Set the cellId as the cellIdentifiers (if set) or the cellIndex and add the value in the row
                    var cellId = (cellIdentifiers != null) ? cellIdentifiers[cellIndex] : cellIndex.ToString();
                    row.Add(cellId, Encoding.UTF8.GetBytes(cell));
                }
            }
        }

        return hashTable;
    }
}