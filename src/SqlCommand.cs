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

	public WhereFilterData? Filter { get; set; }
	public DMLCommand Command { get; set; }
	public string[] Columns { get; set; }
	public string TableName { get; set; }

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
		{
			return;
		}

		if (splitSql[i++].Equals("WHERE", StringComparison.OrdinalIgnoreCase))
		{
			Filter = new WhereFilterData()
			{
				ColName = splitSql[i++],
				ComparisonType = splitSql[i++]
			};
			int start = i;
			string val;
			do
			{
				val = splitSql[i++];
			}
			while (!val.EndsWith('\''));
			Filter.TestValue = string.Join(" ", splitSql[start..i]).Trim('\'');

			return;
		}

		throw new NotImplementedException();
	}
}

internal class WhereFilterData
{
	public required string ColName { get; set; }
	public required string ComparisonType { get; set; }
	public string TestValue { get; set; }
	
	public bool CanFilterBy(object comp)
	{
		return ComparisonType switch
		{
			"=" => comp.ToString() == TestValue,
			_ => throw new NotSupportedException(ComparisonType)
		};
	}
}

enum DMLCommand
{
	SELECT
}

enum Clauses
{
	WHERE
}