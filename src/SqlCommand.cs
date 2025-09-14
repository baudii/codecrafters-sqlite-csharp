namespace codecrafters_sqlite.src;
internal class SqlCommand
{
	public SqlCommand(string sql)
	{
		var spl = sql.Split(' ');
		Command = GetCommandType(spl[0]);
		int i = 1;
		var res = new List<string>();
		do
		{
			res.Add(spl[i++]);
		}
		while (!spl[i].Equals("FROM", StringComparison.OrdinalIgnoreCase));
		Columns = res.ToArray();
		TableName = spl[i + 1];
	}

	public CommandType Command { get; set; }
	public string[] Columns { get; set; }
	public string TableName { get; set; }

	private CommandType GetCommandType(string commandString)
	{
		if (commandString.Equals("SELECT", StringComparison.OrdinalIgnoreCase));
		{
			return CommandType.SELECT;
		}

		throw new NotImplementedException();
	}
}

enum CommandType
{
	SELECT
}