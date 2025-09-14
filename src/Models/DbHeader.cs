namespace codecrafters_sqlite.src.Models;

internal class DbHeader
{
	public ushort PageSize { get; set; }
	public static DbHeader Read(DbReader reader)
	{
		var ret = new DbHeader();

		reader.Seek(0);
		var str = reader.ReadString(16);
		if (str != "SQLite format 3\0")
			throw new InvalidOperationException("Not a valid SQLite db file");

		ret.PageSize = reader.ReadTwoBytesAsUInt16();
		return ret;
	}
}
