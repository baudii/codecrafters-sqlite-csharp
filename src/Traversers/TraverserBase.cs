using codecrafters_sqlite.src.Models;
using codecrafters_sqlite.src.SqlParser;

namespace codecrafters_sqlite.src.Traversers;
internal abstract class TraverserBase(DbReader reader, ushort pageSize)
{
	public DbReader Reader => reader;
	public ushort PageSize => pageSize;
	
	protected static bool Finished = false;
	protected static int IndexedRows { get; set; } = -1;
	protected static Dictionary<uint, bool> RowIds { get; set; } = [];

	public uint GetOffset(uint rootpage) => (rootpage - 1) * pageSize;
	public Page GetPage(uint offset)
	{
		reader.Seek(offset);
		return new Page(reader);
	}

	public void Traverse(SqlCommand sqlCommand, uint rootpage)
	{
		if (Finished)
			return;

		var offset = GetOffset(rootpage);
		var page = GetPage(offset);
		HandleTraverse(sqlCommand, page, offset);
	}

	protected abstract void HandleTraverse(SqlCommand sqlCommand, Page page, uint offset);
}
