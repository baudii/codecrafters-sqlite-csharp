using codecrafters_sqlite.src.Models;
using codecrafters_sqlite.src.Records;
using codecrafters_sqlite.src.SqlParser;

namespace codecrafters_sqlite.src.Traversers;
internal class IndexTraverser : TraverserBase
{
	public IndexTraverser(DbReader reader, ushort pageSize) : base(reader, pageSize)
	{
		IndexedRows = 0;
	}

	public void ReadLeafIndex(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			Reader.Seek(offset + page.Pointers[i]);
			_ = Reader.ReadVarint(out _);
			var record = new Record();
			record.ReadPayload(Reader, "column", "rowid");
			var val = record.RecordData["column"].ToString();
			if (sqlCommand.Filter!.Compare(val) == 0)
			{
				RowIds.TryAdd(Convert.ToUInt32(record.RecordData["rowid"]), true);
				IndexedRows++;
			}
		}
	}

	public void ReadInteriorIndex(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			Reader.Seek(offset + page.Pointers[i]);
			var leftPointer = Reader.ReadFourBytesAsUInt32();
			_ = Reader.ReadVarint(out _);
			var record = new Record();
			record.ReadPayload(Reader, "column", "rowid");
			var col = record.RecordData["column"].ToString();
			var comp = sqlCommand.Filter!.Compare(col);
			if (comp >= 0)
			{
				Traverse(sqlCommand, leftPointer);
				if (comp > 0)
					break;

				RowIds.TryAdd((uint)record.RecordData["rowid"], true);
				IndexedRows++;
			}
		}

		Traverse(sqlCommand, page.RightMostPointer);
	}

	protected override void HandleTraverse(SqlCommand sqlCommand, Page page, uint offset)
	{
		if (page.PageType == 10)
			ReadLeafIndex(sqlCommand, page, offset);
		else if (page.PageType == 2)
		{
			ReadInteriorIndex(sqlCommand, page, offset);
		}
	}
}
