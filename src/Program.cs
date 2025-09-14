// Parse arguments
using codecrafters_sqlite.src;
using codecrafters_sqlite.src.Models;

var (path, command) = args.Length switch
{
    0 => throw new InvalidOperationException("Missing <database path> and <command>"),
    1 => throw new InvalidOperationException("Missing <command>"),
    _ => (args[0], args[1])
};

Console.WriteLine($"Path: {path}, Command: {command}");
var reader = new DbReader(File.OpenRead(path));
if (command == ".dbinfo")
{
    var dbHeader = DbHeader.Read(reader);
    reader.Seek(100);
    var pageHeader = PageHeader.Read(reader);
	Console.WriteLine($"database page size: {dbHeader.PageSize}");
	Console.WriteLine($"number of tables: {pageHeader.NumberOfCells}");
}
else if (command == ".tables")
{
    reader.Seek(100);
    var pageHeader = PageHeader.Read(reader);
    for (int i = 0; i < pageHeader.Pointers.Length; i++)
    {
        var schema = SqlSchemaRecord.Read(reader, pageHeader.Pointers[i]);
        Console.WriteLine(schema.RecordData["tbl_name"]);
	}
}
else
{
    throw new InvalidOperationException($"Invalid command: {command}");
}
