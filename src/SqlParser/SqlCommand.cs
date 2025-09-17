namespace codecrafters_sqlite.src.SqlParser;
internal class SqlCommand
{
	private string sql;
	public SqlCommand(string sql)
	{
		this.sql = sql;
	}

	public WhereFilterData? Filter { get; set; }
	public DMLCommand Command { get; set; }
	public string[] Columns { get; set; } = [];
	public string TableName { get; set; } = string.Empty;

	public string[] ExtractColumns()
	{
		var start = sql.IndexOf('(') + 1;
		var end = sql.IndexOf(')');
		return sql[start..end]
			.Split(',')
			.Select(x =>
			{
				var trimmed = x.Trim();
				if (trimmed.StartsWith('\"'))
					return trimmed[1..trimmed.LastIndexOf('\"')];
				else
				{
					return trimmed.Split(' ')[0];
				}
			})
			.ToArray();
	}

	public void ParseSql()
	{
		var splitSql = sql.Split(' ');
		Command = ParseDML(splitSql[0]);
		int i = 1;
		var res = new List<string>();
		do
		{
			res.Add(splitSql[i++].TrimEnd(','));
		}
		while (!splitSql[i].Equals("FROM", StringComparison.OrdinalIgnoreCase));
		i++;
		Columns = res.ToArray();
		TableName = splitSql[i++];
		TryParseClause(splitSql, ref i);
	}

	private static DMLCommand ParseDML(string commandString)
	{
		if (commandString.Equals("SELECT", StringComparison.OrdinalIgnoreCase));
		{
			return DMLCommand.SELECT;
		}

		throw new NotImplementedException();
	}

	private void TryParseClause(string[] splitSql, ref int i)
	{
		if (i >= splitSql.Length)
			return;

		if (splitSql[i++].Equals("WHERE", StringComparison.OrdinalIgnoreCase))
		{
			var colName = splitSql[i++];
			var comparisonType = splitSql[i++];
			int start = i;
			string val;
			do
			{
				val = splitSql[i++];
			}
			while (!val.EndsWith('\''));

			Filter = new WhereFilterData()
			{
				ColName = colName,
				ComparisonType = comparisonType,
				TestValue = string.Join(" ", splitSql[start..i]).Trim('\'')
			};

			return;
		}

		throw new NotImplementedException();
	}
}
