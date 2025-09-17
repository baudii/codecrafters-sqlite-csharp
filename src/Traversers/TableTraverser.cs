using codecrafters_sqlite.src.Models;
using codecrafters_sqlite.src.Records;
using codecrafters_sqlite.src.SqlParser;

namespace codecrafters_sqlite.src.Traversers;
internal class TableTraverser(DbReader reader, ushort pageSize, string[] columns) : TraverserBase(reader, pageSize)
{
	public void ReadLeafTable(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			TableRecord record = new(Reader, offset + page.Pointers[i], columns);
			if (sqlCommand.Filter != null && sqlCommand.Filter.Compare(record.RecordData[sqlCommand.Filter.ColName]) != 0)
				continue;

			var records = sqlCommand.Columns.Select(x => record.RecordData.First(p => p.Key == x).Value);
			Console.WriteLine(string.Join("|", records));
			if (IndexedRows > 0)
			{
				RowIds[Convert.ToUInt32(record.RowId)] = false;
				IndexedRows--;
				if (IndexedRows == 0)
					Finished = true;
			}
		}
	}

	public void ReadInteriorTable(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			Reader.Seek(offset + page.Pointers[i]);
			var leftPointer = Reader.ReadFourBytesAsUInt32();
			var rowId = Reader.ReadVarint(out _);
			if (IndexedRows == -1 || RowIds.Any(x => x.Key < rowId && x.Value))
				Traverse(sqlCommand, leftPointer);
		}

		Traverse(sqlCommand, page.RightMostPointer);
	}

	protected override void HandleTraverse(SqlCommand sqlCommand, Page page, uint offset)
	{
		if (page.PageType == 5)
		{
			ReadInteriorTable(sqlCommand, page, offset);
		}
		else if (page.PageType == 13)
		{
			ReadLeafTable(sqlCommand, page, offset);
		}
	}
}
