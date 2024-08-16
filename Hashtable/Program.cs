// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

// var hashTable = new HashTable(SHA256.Create, 10);
// var row1 = hashTable.AddRow("r0"); //O(1)

// row1.Add("0", Encoding.UTF8.GetBytes("0"));
// row1.Add("1", Encoding.UTF8.GetBytes("1"));

// System.Console.WriteLine($"HashTable.TableHash:\t{hashTable.GetTableHash().ToHexString()}");

// var hashTable2 = new HashTable(SHA256.Create, 10);
// var row2 = hashTable2.AddRow("r0");

// row2.Add("0", Encoding.UTF8.GetBytes("0"));
// row2.Add("1", Encoding.UTF8.GetBytes("1"));

// System.Console.WriteLine($"HashTable.TableHash:\t{hashTable2.GetTableHash().ToHexString()}");

// #region SAMPLE CODE
// var sampleHashTableFromCsv = new StreamReader("C:\\tmp\\customers-100.csv")
//     .FromCsvToHashTable(
//         SHA256.Create, 
//         separator : ",", 
//         cellIndexesUsedAsRowIdentifier : [0], 
//         useFirstRowAsColumnIdentifier : true
//     );
// var sampleHashTable2FromCsv = new StreamReader("C:\\tmp\\customers-100-2.csv")
//     .FromCsvToHashTable(
//         SHA256.Create, 
//         separator : ",", 
//         cellIndexesUsedAsRowIdentifier : [0], 
//         useFirstRowAsColumnIdentifier : true
//     );
// var diffBetweenHashTables = sampleHashTableFromCsv.Difference(sampleHashTable2FromCsv);
// System.Console.WriteLine($"Has differences: {diffBetweenHashTables.HasDifferences()}");
// #endregion

// var refEntradaNf = new StreamReader("C:\\tmp\\2-entrada nf.csv").FromCsvToHashTable(SHA256.Create, cellIndexesUsedAsRowIdentifier : [0, 5], ignoreDuplicatedRowIds: true);
// var otherEntradaNf = new StreamReader("C:\\tmp\\2-entrada nf-2.csv").FromCsvToHashTable(SHA256.Create, cellIndexesUsedAsRowIdentifier : [0, 5], ignoreDuplicatedRowIds: true);
// var difBetweenEntradaNfs = refEntradaNf.Difference(otherEntradaNf);

// System.Console.WriteLine($"HashTable1:\t{refEntradaNf.GetTableHash().ToHexString()}");
// System.Console.WriteLine($"HashTable2:\t{otherEntradaNf.GetTableHash().ToHexString()}");
// System.Console.WriteLine($"Has Differences:\t{difBetweenEntradaNfs.HasDifferences()}");

// if (difBetweenEntradaNfs.HasDifferences())
// {
//     foreach (var rowWithDiffEntry in difBetweenEntradaNfs.RowsWithDifferences)
//     {
//         System.Console.WriteLine($"Row[{rowWithDiffEntry.Key}]:Diff={rowWithDiffEntry.Value.DifferenceType};");
//     }
// }

var guid1 = Guid.NewGuid();
var guid2 = Guid.NewGuid();

SampleEntity[] sampleEntities = [
    new SampleEntity{ BirthDate = DateTime.Now, Guid = guid1, Name = "User 1" },
    new SampleEntity{ BirthDate = DateTime.Now, Guid = guid2, Name = "User 2" }
];

SampleEntity[] sampleEntities2 = [
    new SampleEntity{ BirthDate = DateTime.Now, Guid = guid1, Name = "User 1" },
    new SampleEntity{ BirthDate = DateTime.Now, Guid = guid2, Name = "User 2" }
];

var sampleEntityHashTabe = HashTable.FromHashTableRowSerializer(SHA256.Create, sampleEntities);
var sampleEntityHashTabe2 = HashTable.FromHashTableRowSerializer(SHA256.Create, sampleEntities2);
var sampleEntityHashTabeDiff = sampleEntityHashTabe.Difference(sampleEntityHashTabe2);
System.Console.WriteLine(sampleEntityHashTabeDiff.HasDifferences());
System.Console.WriteLine(DateTime.Now.ToLongDateString());
foreach (var rowDiff in sampleEntityHashTabeDiff.RowsWithDifferences)
{
    Console.WriteLine($"RowDiff[{rowDiff.Key}]={rowDiff.Value.DifferenceType}");
}


