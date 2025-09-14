using System.Buffers.Binary;
using System.Text;

namespace codecrafters_sqlite.src.Models;
internal class SqlSchemaRecord
{
	public long RecordSize { get; set; }
	public long RowId { get; set; }
	public long HeaderSize { get; set; }
	public List<long> SerialTypes { get; set; } = [];
	public Dictionary<string, object> RecordData { get; set; } = [];
	
	public static SqlSchemaRecord Read(DbReader reader, ushort pointer)
	{
		var record = new SqlSchemaRecord();

		reader.Seek(pointer);
		record.RecordSize = reader.ReadVarint(out _);
		record.RowId = reader.ReadVarint(out _);
		long recordHeaderSize = reader.ReadVarint(out var readBytes);
		record.HeaderSize = recordHeaderSize;

		recordHeaderSize -= readBytes;
		string[] clmns = ["type", "name", "tbl_name", "rootpage", "sql"];
		int idx = 0;
		while (recordHeaderSize > 0)
		{
			long val = reader.ReadVarint(out readBytes);
			recordHeaderSize -= readBytes;
			record.SerialTypes.Add(val);
			record.RecordData[clmns[idx]] = val;
		}

		if (record.SerialTypes.Count != clmns.Length)
			throw new InvalidOperationException("Record format mismatch");

		for (int i = 0; i < record.SerialTypes.Count; i++)
		{
			var data = HandleSerialTypes(reader, record.SerialTypes[i]);
			record.RecordData[clmns[i]] = data;
		}

		return record;
	}

	public static object HandleSerialTypes(DbReader reader, long serialType)
	{
		byte[] bytes;
		switch (serialType)
		{
			case 0:
				break;
			case 1: return reader.ReadInt(1)[0];
			case 2: return reader.ReadTwoBytesAsUInt16();
			case <= 4:
				bytes = reader.ReadInt((int)serialType);
				return BinaryPrimitives.ReadInt32BigEndian(bytes);
			case <= 6:
				bytes = reader.ReadInt(6 + ((int)serialType - 5) * 2);
				return BinaryPrimitives.ReadInt32BigEndian(bytes);
			case >= 12:
				if (serialType % 2 == 1)
					return reader.ReadString((int)((serialType - 13) / 2));

				return reader.ReadString((int)((serialType - 12) / 2));
		}

		throw new NotImplementedException();
	}
}
