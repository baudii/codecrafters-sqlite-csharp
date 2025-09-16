// Parse arguments
using codecrafters_sqlite.src;
using codecrafters_sqlite.src.Models;
using codecrafters_sqlite.src.Records;

var (path, command) = args.Length switch
{
    0 => throw new InvalidOperationException("Missing <database path> and <command>"),
    1 => throw new InvalidOperationException("Missing <command>"),
    _ => (args[0], args[1])
};

var reader = new DbReader(File.OpenRead(path));
var dbHeader = DbHeader.Read(reader);
reader.Seek(100);
var schemaPageHeader = new Page(reader);

if (command == ".dbinfo")
{
	Console.WriteLine($"database page size: {dbHeader.PageSize}");
	Console.WriteLine($"number of tables: {schemaPageHeader.NumberOfCells}");
}
else if (command == ".tables")
{
    for (int i = 0; i < schemaPageHeader.Pointers.Length; i++)
    {
        var schema = new SqlSchemaRecord(reader, schemaPageHeader.Pointers[i]);
        Console.WriteLine(schema.RecordData["tbl_name"]);
	}
}
else
{
	var sqlCommand = new SqlCommand(command);
	var traverser = new PageTraverser(reader, dbHeader.PageSize);
    traverser.Start(sqlCommand, schemaPageHeader);
}
