using codecrafters_sqlite.src.Models;

namespace codecrafters_sqlite.src;
internal class PageTraverser(DbReader reader, ushort pageSize, string[] columns)
{
	public void ReadTable(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			Record record = new(reader, offset + page.Pointers[i], columns);
			if (sqlCommand.Filter != null && !sqlCommand.Filter.CanFilterBy(record.RecordData[sqlCommand.Filter.ColName]))
			{
				continue;
			}

			var records = sqlCommand.Columns.Select(x => record.RecordData.First(p => p.Key == x).Value);
			Console.WriteLine(string.Join("|", records));
		}
	}

	public void ReadInterior(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			reader.Seek(offset + page.Pointers[i]);
			var leftPointer = reader.ReadFourBytesAsUInt32();
			Traverse(sqlCommand, leftPointer);
		}
	}

	public void Traverse(SqlCommand sqlCommand, uint rootpage, long rowId = -1)
	{
		var offset = (rootpage - 1) * pageSize;
		reader.Seek(offset);
		var page = new Page(reader);

		if (sqlCommand.Columns.Contains("count(*)"))
		{
			Console.WriteLine(page.NumberOfCells);
		}
		else if (page.PageType == 13)
		{
			ReadTable(sqlCommand, page, offset);
		}
		else if (page.PageType == 5)
		{
			ReadInterior(sqlCommand, page, offset);
		}
	}
}
