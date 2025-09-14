namespace codecrafters_sqlite.src.Models;
internal class SqlSchemaRecord(DbReader reader, ushort pointer) : Record(reader, pointer, "type", "name", "tbl_name", "rootpage", "sql")
{
	public string[] ExtractColumnNamesFromSql()
	{
		var sql = RecordData["sql"].ToString()!;
		var start = sql.IndexOf('(') + 1;
		var end = sql.IndexOf(')');
		return sql[start..end]
			.Split(',')
			.Select(x => x.Trim().Split(' ')[0])
			.ToArray();
	}
}
