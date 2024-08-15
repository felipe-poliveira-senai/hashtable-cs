using System.Text;

public static class CsvUtils
{
    public static string[] ReadCsvLine(string csvLine, string separator = ",")
    {
        var csvCells = new List<string>();
        var curContext = CsvLineContext.Unknown;
        var strBuilder = new StringBuilder();
        var onEscape = false;

        foreach (var curChar in csvLine)
        {
            switch (curContext)
            {
                // If the context of the csv is unknown...
                case CsvLineContext.Unknown:
                    // If cur char is '"' it means that it is a String context
                    if (curChar == '"')
                    {
                        curContext = CsvLineContext.String;
                        continue;
                    }
                    // If cur char is no blank append it and set the context to Simple
                    else if (curChar != ' ' && curChar != ',')
                    {
                        strBuilder.Append(curChar);
                        curContext = CsvLineContext.Simple;
                    }
                    // if the char is blank skip to the next character
                    else
                    {
                        continue;
                    }
                    break;
                // If the context of the csv is SimpleInput
                case CsvLineContext.Simple:
                    // If the current character is equal than the separator it means that it reaches the end value...
                    // so the value will be added in the list
                    if (curChar.ToString() == separator)
                    {
                        csvCells.Add(strBuilder.ToString());
                        strBuilder.Clear();
                        curContext = CsvLineContext.Unknown;
                        continue;
                    }
                    // Otherwise, keep adding the values in the string builder
                    else
                    {
                        strBuilder.Append(curChar);
                    }
                    break;
                // If the context of the csv is a String....
                case CsvLineContext.String:
                    // if is on escape "\\" always add the current value and immediately disable the escape mode
                    if (onEscape)
                    {
                        strBuilder.Append(curChar);
                        onEscape = false;
                    }
                    // If the cur char is a '\' it means that the escape mode should be activated and none character should be added to the buffer
                    if (curChar == '\\')
                    {
                        onEscape = true;
                    }
                    // If the cur char is another '"' it means that the string has come to an end, so change the context to EndOfString
                    if (curChar == '"')
                    {
                        curContext = CsvLineContext.EndOfString;
                        csvCells.Add(strBuilder.ToString());
                        strBuilder.Clear();
                    }
                    // If none of the above, keep adding the values
                    else
                    {
                        strBuilder.Append(curChar);
                    }
                    continue;
                // If the context of the csv is EndOfString...
                case CsvLineContext.EndOfString:
                    // if it reaches the separator change the context to unknown
                    if (curChar.ToString() == separator)
                    {
                        curContext = CsvLineContext.Unknown;
                    }
                    continue;
            }
        }

        // If there is content to add in the builder add it
        if (strBuilder.Length > 0)
        {
            csvCells.Add(strBuilder.ToString());
        }


        return csvCells.ToArray();
    }

    private enum CsvLineContext
    {
        String,
        EndOfString,
        Simple,
        Unknown,
    }
}