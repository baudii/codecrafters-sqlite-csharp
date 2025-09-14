// Parse arguments
using codecrafters_sqlite.src;
using codecrafters_sqlite.src.Models;

var (path, command) = args.Length switch
{
    0 => throw new InvalidOperationException("Missing <database path> and <command>"),
    1 => throw new InvalidOperationException("Missing <command>"),
    _ => (args[0], args[1])
};

var reader = new DbReader(File.OpenRead(path));
var dbHeader = DbHeader.Read(reader);
reader.Seek(100);
var schemaPageHeader = new PageHeader(reader);

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
    var table = command.Split(' ')[^1];

    SqlSchemaRecord[] schemas = new SqlSchemaRecord[schemaPageHeader.NumberOfCells];
	for (int i = 0; i < schemaPageHeader.Pointers.Length; i++)
	{
        schemas[i] = new SqlSchemaRecord(reader, schemaPageHeader.Pointers[i]);
	}

    var desired = schemas.First(x => x.RecordData["tbl_name"].ToString() == table);
    var rootPage = (byte)desired.RecordData["rootpage"];

	var seekValue = (rootPage - 1) * dbHeader.PageSize;

	reader.Seek(seekValue);
	var tablePageHeader = new PageHeader(reader);
    var columns = desired.ExtractColumnNamesFromSql();
    Record[] headers = new Record[tablePageHeader.NumberOfCells];
    for (int i = 0; i < tablePageHeader.NumberOfCells; i++)
    {
        var record = new Record(reader, (ushort)(seekValue + tablePageHeader.Pointers[i]), columns);
        Console.WriteLine(record.RecordData[sqlCommand.Columns[0]]);
    }
}