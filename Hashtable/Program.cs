// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

var hashTable = new HashTable(SHA256.Create, 10);
var row1 = hashTable.AddRow("r0"); //O(1)

row1.Add("0", Encoding.UTF8.GetBytes("0"));
row1.Add("1", Encoding.UTF8.GetBytes("1"));

System.Console.WriteLine($"HashTable.TableHash:\t{hashTable.GetTableHash().ToHexString()}");

var hashTable2 = new HashTable(SHA256.Create, 10);
var row2 = hashTable2.AddRow("r0");

row2.Add("0", Encoding.UTF8.GetBytes("0"));
row2.Add("1", Encoding.UTF8.GetBytes("1"));

System.Console.WriteLine($"HashTable.TableHash:\t{hashTable2.GetTableHash().ToHexString()}");

#region SAMPLE CODE
var sampleHashTableFromCsv = new StreamReader("C:\\tmp\\customers-100.csv")
    .FromCsvToHashTable(
        SHA256.Create, 
        separator : ",", 
        cellIndexesUsedAsIdentifier : [0], 
        useFirstRowAsIdentifier : true
    );
var sampleHashTable2FromCsv = new StreamReader("C:\\tmp\\customers-100-2.csv")
    .FromCsvToHashTable(
        SHA256.Create, 
        separator : ",", 
        cellIndexesUsedAsIdentifier : [0], 
        useFirstRowAsIdentifier : true
    );
var diffBetweenHashTables = sampleHashTableFromCsv.Difference(sampleHashTable2FromCsv);
System.Console.WriteLine($"Has differences: {diffBetweenHashTables.HasDifferences()}");
#endregion


