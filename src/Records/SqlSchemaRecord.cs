namespace codecrafters_sqlite.src.Records;
internal class SqlSchemaRecord(DbReader reader, uint pointer) : TableRecord(reader, pointer, "type", "name", "tbl_name", "rootpage", "sql")
{
}
