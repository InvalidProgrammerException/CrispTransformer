Design:
The basic idea is that the the program takes a configuration file with instructions, written in C#, to handle the input from a row of the csv file to create output into the destination file.

The Transform class accepts 4 files: the csv input file, the log file, the output file, and the configuration file. The constructor will read the configuration file and compile it and output any errors to the log file.

The TransformCsv method will read the file. It will read the first row to get the column names, and then it will it use the code from the configuration file for handling each subsequent row of the input csv file.

The Transform method provides several methods, including convenience methods, for the configuration file to use. The methods are:
Output - writes a string to the output file
OutputInt - writes an integer to the output file
OutputDec - writes a decimal to the output file
LogLine - writes a line of text to the output file
StrToDec - converts a string to decimal - skipping any non-decimal characters
StrToInt - converts a string to integer - skipping any non-integer characters (except '.', which will be treated as an error)
Field - retrieves the value for a field for the current row of the input csv file
ValidateValue - validates a value against an expected regular expression
ValidateField - validates a field against an expected regular expression

Instructions on using the program:
Run the CrispTranformer program from the command line with 4 arguments that specify the csv input file, the log file, the output file, and the configuration file.

For this example, assume we are running from the "CrispTransformer\CrispTransformer\bin\Debug" directory. The csv input file is "orders.csv", the log file is "log.txt", the output file is "out.txt" and the configuration file is "config.txt"

So you would run the program with following line: 
CrispTransformer.exe logfile:=log.txt outputfile:=out.txt csvfile:=orders.csv configfile:=config.txt

If you wanted to use the config2.txt configuration file you would run:
CrispTransformer.exe logfile:=log.txt outputfile:=out.txt csvfile:=orders.csv configfile:=config2.txt

Assumptions and simplifications made:
I assumed that a programmer would be executing this process. A simpler format would be more appropriate for a non-programmer. To simplify this project, I placed most of the code into the trnasform project and we would probably want to pull some of the functionality into other classes. Also, testing should be made much more robust.

List of next steps if this were a real project:
Obviously clarification with the people involved would be necessary but going with current design I see several possible enhancements that could be made. Currently, this program supports an older version of C# in the configuration file, so it doesn't support things like string interpolation. So using a new, possibly Rosalyn, compiler would be beneficial.

Also, more convenience methods could be supported. Additionally methods that worked with preprocessing of all rows would allow things like work the sum of a column, or allow things like functions that work with groupings. Also we could add the ability to dynamically specify assemblies to reference.

