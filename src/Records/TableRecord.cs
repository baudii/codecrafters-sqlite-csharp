namespace codecrafters_sqlite.src.Records;
internal class TableRecord : Record
{
	public TableRecord(DbReader reader, uint pointer, params string[] columns)
	{
		reader.Seek(pointer);
		RecordSize = reader.ReadVarint(out _);
		RowId = reader.ReadVarint(out _);
		ReadPayload(reader, columns);
	}
}
