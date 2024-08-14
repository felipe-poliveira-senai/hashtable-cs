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
var sampleHashTableFromCsv = new StreamReader("C:\\tmp\\customers-100.csv").FromCsvToHashTable(SHA256.Create);
var sampleHashTable2FromCsv = new StreamReader("C:\\tmp\\customers-100-2.csv").FromCsvToHashTable(SHA256.Create);
var diffBetweenHashTables = sampleHashTableFromCsv.Difference(sampleHashTable2FromCsv);
System.Console.WriteLine($"Has differences: {diffBetweenHashTables.HasDifferences()}");
#endregion

#region  BENCHMARK
var stopwatch = new Stopwatch();
stopwatch.Start();
var csvHashTable = new StreamReader("C:\\tmp\\customers-1000000.csv").FromCsvToHashTable(SHA256.Create);
stopwatch.Stop();
System.Console.WriteLine($"From CSV to Hash took:\t\t{stopwatch.ElapsedMilliseconds}");

System.Console.WriteLine($"HashTable.TableHash\t\t{csvHashTable.GetTableHash().ToHexString()}");

stopwatch.Restart();
var csvHashTable2 = new StreamReader("C:\\tmp\\customers-1000000-2.csv").FromCsvToHashTable(SHA256.Create);
stopwatch.Stop();
System.Console.WriteLine($"From CSV to Hash took:\t\t{stopwatch.ElapsedMilliseconds}");
System.Console.WriteLine($"HashTable2.TableHash\t\t{csvHashTable2.GetTableHash().ToHexString()}");

stopwatch.Restart();
var tableDiff = csvHashTable.Difference(csvHashTable2);
stopwatch.Stop();
System.Console.WriteLine($"Took:\t\t\t\t{stopwatch.ElapsedMilliseconds} to diff tables");
System.Console.WriteLine($"Has differences: {tableDiff.HasDifferences()}");

#endregion

