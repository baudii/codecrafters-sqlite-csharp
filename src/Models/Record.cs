using System.Buffers.Binary;

namespace codecrafters_sqlite.src.Models;
internal class Record
{
	public void ReadPayload(DbReader reader, params string[] columns)
	{
		long recordHeaderSize = reader.ReadVarint(out var readBytes);
		HeaderSize = recordHeaderSize;
		recordHeaderSize -= readBytes;

		while (recordHeaderSize > 0)
		{
			long val = reader.ReadVarint(out readBytes);
			recordHeaderSize -= readBytes;
			SerialTypes.Add(val);
		}

		if (columns.Length > 0 && SerialTypes.Count != columns.Length)
			throw new InvalidOperationException("Record format mismatch");

		for (int i = 0; i < SerialTypes.Count; i++)
		{
			var data = HandleSerialTypes(reader, SerialTypes[i]);
			if (columns.Length == 0)
			{
				RecordData[i.ToString()] = data;
			}
			else if (data.Equals(-1) && columns[i] == "id")
			{
				RecordData[columns[i]] = RowId;
			}
			else
			{
				RecordData[columns[i]] = data;
			}
		}
	}

	public long RecordSize { get; set; }
	public long RowId { get; set; }
	public long HeaderSize { get; set; }
	public List<long> SerialTypes { get; set; } = [];
	public Dictionary<string, object> RecordData { get; set; } = [];
	
	public static object HandleSerialTypes(DbReader reader, long serialType) =>
		serialType switch
		{
			0 => -1,
			1 => reader.ReadByte(),
			2 => reader.ReadTwoBytesAsUInt16(),
			3 => reader.Read3Bytes(),
			<= 6 => BinaryPrimitives.ReadInt32BigEndian(reader.ReadInt(6 + ((int)serialType - 5) * 2)),
			7 => throw new NotImplementedException(),
			<= 9 => 0,
			_ when serialType >= 12 && (serialType % 2 == 0) => reader.ReadBlob(((int)serialType - 12) / 2),
			_ when serialType >= 13 && (serialType % 2 == 1) => reader.ReadString((int)((serialType - 13) / 2)),
			_ => throw new NotImplementedException()
		};
}
