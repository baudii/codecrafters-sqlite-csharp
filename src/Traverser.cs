using codecrafters_sqlite.src.Models;
using codecrafters_sqlite.src.Records;
using codecrafters_sqlite.src.SqlParser;
using codecrafters_sqlite.src.Traversers;
using System.Diagnostics;

namespace codecrafters_sqlite.src;
internal static class Traverser
{
	private static Dictionary<string, TableRecord> indexes = [];
	public static void Start(DbReader reader, SqlCommand sqlCommand, Page schemaPageHeader, ushort pageSize)
	{
		SqlSchemaRecord[] schemas = new SqlSchemaRecord[schemaPageHeader.NumberOfCells];
		for (int i = 0; i < schemaPageHeader.Pointers.Length; i++)
		{
			schemas[i] = new SqlSchemaRecord(reader, schemaPageHeader.Pointers[i]);
			if (schemas[i].RecordData.GetValueOrDefault("type")?.Equals("index") == true)
			{
				var data = schemas[i].RecordData;
				var sql = data["sql"].ToString()!;
				var indexColumns = sql[(sql.IndexOf('(') + 1)..sql.LastIndexOf(')')].Split(',');
				foreach (var indexColumn in indexColumns)
					indexes.Add(indexColumn, schemas[i]);
			}
		}

		var schema = schemas.First(x => x.RecordData["tbl_name"]?.ToString() == sqlCommand.TableName);
		PlanQuery(reader, sqlCommand, schema, pageSize);
	}

	private static void PlanQuery(DbReader reader, SqlCommand sqlCommand, SqlSchemaRecord schema, ushort pageSize, bool showTime = false)
	{
		var schemaSqlCommand = new SqlCommand(schema.RecordData["sql"].ToString()!);
		var columns = schemaSqlCommand.ExtractColumns();
		var tableTraverser = new TableTraverser(reader, pageSize, columns);

		Stopwatch sw = Stopwatch.StartNew();
		if (sqlCommand.Filter != null && indexes.TryGetValue(sqlCommand.Filter.ColName, out var indexSchema))
		{
			var indexTraverser = new IndexTraverser(reader, pageSize);
			indexTraverser.Traverse(sqlCommand, Convert.ToUInt32(indexSchema.RecordData["rootpage"]));
		}

		tableTraverser.Traverse(sqlCommand, Convert.ToUInt32(schema.RecordData["rootpage"]));
		if (showTime)
			Console.WriteLine($"Query executed in {sw.Elapsed}");
	}
}
