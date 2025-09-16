using codecrafters_sqlite.src.Models;
using codecrafters_sqlite.src.Records;
using System;
using System.Text;

namespace codecrafters_sqlite.src;
internal class PageTraverser(DbReader reader, ushort pageSize)
{
	string[] columns;
	Dictionary<string, SqlSchemaRecord> indexes = [];
	List<uint> rowIds = [];
	public void Start(SqlCommand sqlCommand, Page schemaPageHeader)
	{
		SqlSchemaRecord[] schemas = new SqlSchemaRecord[schemaPageHeader.NumberOfCells];
		for (int i = 0; i < schemaPageHeader.Pointers.Length; i++)
		{
			schemas[i] = new SqlSchemaRecord(reader, schemaPageHeader.Pointers[i]);
			if (schemas[i].RecordData["type"].Equals("index"))
			{
				var data = schemas[i].RecordData;
				var sql = data["sql"].ToString()!;
				var indexColumnName = sql[(sql.IndexOf('(')+1)..sql.LastIndexOf(')')];
				indexes.Add(indexColumnName, schemas[i]);
			}
		}

		var schema = schemas.First(x => x.RecordData["tbl_name"].ToString() == sqlCommand.TableName);
		columns = schema.ExtractColumnNamesFromSql();
		if (sqlCommand.Filter != null && indexes.TryGetValue(sqlCommand.Filter.ColName, out var indexSchema))
		{
			Traverse(sqlCommand, Convert.ToUInt32(indexSchema.RecordData["rootpage"]));
		}

		Traverse(sqlCommand, Convert.ToUInt32(schema.RecordData["rootpage"]));
	}

	public void ReadLeafTable(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			TableRecord record = new(reader, offset + page.Pointers[i], columns);
			if (sqlCommand.Filter != null && !sqlCommand.Filter.CanFilterBy(record.RecordData[sqlCommand.Filter.ColName]))
			{
				continue;
			}

			var records = sqlCommand.Columns.Select(x => record.RecordData.First(p => p.Key == x).Value);
			Console.WriteLine(string.Join("|", records));
		}
	}

	public void ReadInteriorTable(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			reader.Seek(offset + page.Pointers[i]);
			var leftPointer = reader.ReadFourBytesAsUInt32();
			var rowId = reader.ReadVarint(out _);
			if (rowIds.Count == 0 || rowIds.Any(x => x < rowId))
			{
				Traverse(sqlCommand, leftPointer);
			}
		}

		Traverse(sqlCommand, page.RightMostPointer);
	}

	public void ReadLeafIndex(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			reader.Seek(offset + page.Pointers[i]);
			_ = reader.ReadVarint(out _);
			var record = new Record();
			record.ReadPayload(reader, "column", "rowid");
			var val = record.RecordData["column"].ToString();
			var testVal = sqlCommand.Filter!.TestValue;
			if (sqlCommand.Filter!.CanFilterBy(val))
			{
				rowIds.Add((uint)record.RecordData["rowid"]);
			}
		}
	}

	public void ReadInteriorIndex(SqlCommand sqlCommand, Page page, uint offset)
	{
		for (int i = 0; i < page.NumberOfCells; i++)
		{
			reader.Seek(offset + page.Pointers[i]);
			var leftPointer = reader.ReadFourBytesAsUInt32();
			_ = reader.ReadVarint(out _);
			var record = new Record();
			record.ReadPayload(reader, "column", "rowid");
			var col = record.RecordData["column"].ToString();
			var testVal = sqlCommand.Filter!.TestValue;
			if (string.Compare(col, testVal) >= 0)
			{
				if (sqlCommand.Filter.CanFilterBy(col))
					rowIds.Add((uint)record.RecordData["rowid"]);
				Traverse(sqlCommand, leftPointer);
			}
		}

		Traverse(sqlCommand, page.RightMostPointer);
	}

	public void Traverse(SqlCommand sqlCommand, uint rootpage)
	{
		var offset = (rootpage - 1) * pageSize;
		reader.Seek(offset);
		var page = new Page(reader);
		if (sqlCommand.Columns.Contains("count(*)"))
		{
			Console.WriteLine(page.NumberOfCells);
		}
		else if (page.PageType == 2)
		{
			ReadInteriorIndex(sqlCommand, page, offset);
		}
		else if (page.PageType == 5)
		{
			ReadInteriorTable(sqlCommand, page, offset);
		}
		else if (page.PageType == 10)
		{
			ReadLeafIndex(sqlCommand, page, offset);
		}
		else if (page.PageType == 13)
		{
			ReadLeafTable(sqlCommand, page, offset);
		}
	}
}
