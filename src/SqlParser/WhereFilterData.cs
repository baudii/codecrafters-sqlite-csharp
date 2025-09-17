namespace codecrafters_sqlite.src.SqlParser;

internal class WhereFilterData
{
	public required string ColName { get; set; }
	public required string ComparisonType { get; set; }
	public required string TestValue { get; set; }
	
	public bool AreEqual(object comp) => Compare(comp) == 0;

	public int Compare(object comp)
	{
		return ComparisonType switch
		{
			"=" => string.Compare(comp.ToString(), TestValue),
			_ => throw new NotSupportedException(ComparisonType)
		};
	}
}
