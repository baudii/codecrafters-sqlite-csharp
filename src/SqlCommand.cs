namespace codecrafters_sqlite.src;
internal class SqlCommand
{
	public SqlCommand(string sql)
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
		Columns = res.ToArray();
		TableName = splitSql[++i];
		i++;
		TryParseClause(splitSql, ref i);
	}

	public FilterContainer? Filter { get; set; }
	public DMLCommand Command { get; set; }
	public string[] Columns { get; set; }
	public string TableName { get; set; }

	private DMLCommand ParseDML(string commandString)
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
		{
			return;
		}

		if (splitSql[i++].Equals("WHERE", StringComparison.OrdinalIgnoreCase))
		{
			var filter = new FilterContainer()
			{
				Clause = Clauses.WHERE,
				ColName = splitSql[i++],
				ComparisonType = splitSql[i++],
				Comparable = splitSql[i++].Trim('\'')
			};

			filter.FilterHandler = comp =>
			{
				return filter.ComparisonType switch
				{
					"=" => comp.ToString() == filter.Comparable,
					_ => throw new NotSupportedException(filter.ComparisonType)
				};
			};

			Filter = filter;
			return;
		}

		throw new NotImplementedException();
	}
}

internal class FilterContainer
{
	public Clauses Clause { get; set; }
	public string ColName { get; set; }
	public string ComparisonType { get; set; }
	public string Comparable { get; set; }
	public Func<object, bool>? FilterHandler { get; set; }
}

enum DMLCommand
{
	SELECT
}

enum Clauses
{
	WHERE
}