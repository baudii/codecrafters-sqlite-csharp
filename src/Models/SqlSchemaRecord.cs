namespace codecrafters_sqlite.src.Models;
internal class SqlSchemaRecord(DbReader reader, ushort pointer) : TableRecord(reader, pointer, "type", "name", "tbl_name", "rootpage", "sql")
{
	public string[] ExtractColumnNamesFromSql()
	{
		var sql = RecordData["sql"].ToString()!;
		var start = sql.IndexOf('(') + 1;
		var end = sql.IndexOf(')');
		return sql[start..end]
			.Split(',')
			.Select(x =>
			{
				var trimmed = x.Trim();
				if (trimmed.StartsWith('\"'))
				{
					return trimmed[1..trimmed.LastIndexOf('\"')];
				}
				else
				{
					return trimmed.Split(' ')[0];
				}
			})
			.ToArray();
	}
}
