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
var dbHeader = DbHeader.Read(reader);
reader.Seek(100);
var schemaPageHeader = PageHeader.Read(reader);

if (command == ".dbinfo")
{
	Console.WriteLine($"database page size: {dbHeader.PageSize}");
	Console.WriteLine($"number of tables: {schemaPageHeader.NumberOfCells}");
}
else if (command == ".tables")
{
    for (int i = 0; i < schemaPageHeader.Pointers.Length; i++)
    {
        var schema = SqlSchemaRecord.Read(reader, schemaPageHeader.Pointers[i]);
        Console.WriteLine(schema.RecordData["tbl_name"]);
	}
}
else
{
    var spl = command.Split(' ');
    var table = spl[^1];

    SqlSchemaRecord[] schemas = new SqlSchemaRecord[schemaPageHeader.NumberOfCells];
	for (int i = 0; i < schemaPageHeader.Pointers.Length; i++)
	{
        schemas[i] = SqlSchemaRecord.Read(reader, schemaPageHeader.Pointers[i]);
	}

    var desired = schemas.First(x => x.RecordData["tbl_name"].ToString() == table);
    var rootPage = (byte)desired.RecordData["rootpage"];

	var pageSize = dbHeader.PageSize;
	var seekValue = (rootPage - 1) * pageSize;

	reader.Seek(seekValue);
	var p = PageHeader.Read(reader);

	Console.WriteLine(p.NumberOfCells);
}
