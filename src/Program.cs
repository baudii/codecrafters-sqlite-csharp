// Parse arguments
using System.Buffers.Binary;
using System.Text;

var (path, command) = args.Length switch
{
    0 => throw new InvalidOperationException("Missing <database path> and <command>"),
    1 => throw new InvalidOperationException("Missing <command>"),
    _ => (args[0], args[1])
};

var dbFile = File.OpenRead(path);

// Parse command and act accordingly
if (command == ".dbinfo")
{
    // You can use print statements as follows for debugging, they'll be visible when running tests.
    Console.Error.WriteLine("Logs from your program will appear here!");

    // Uncomment this line to pass the first stage
    dbFile.Seek(16, SeekOrigin.Begin); // Skip the first 16 bytes
	var pageSize = ReadTwoBytesAsInt16(dbFile);
	Console.WriteLine($"database page size: {pageSize}");

    dbFile.Seek(103, SeekOrigin.Begin);
    var cellNumber = ReadTwoBytesAsInt16(dbFile);
	Console.WriteLine($"number of tables: {cellNumber}");
}
else if (command == ".tables")
{
    dbFile.Seek(108, SeekOrigin.Begin);
    ushort[] pointers = new ushort[3];
    for (int i = 0; i < pointers.Length; i++)
    {
        pointers[i] = ReadTwoBytesAsInt16(dbFile);
    }

    for (int i = 0; i < pointers.Length; i++)
    {
        dbFile.Seek(pointers[i], SeekOrigin.Begin);
        var recordSize = ReadVarint(dbFile, out _);
        var rowId = ReadVarint(dbFile, out _);
        var serialTypes = ReadRecordHeader(dbFile);
        string prv = string.Empty;
        bool wr = false;
        foreach (var st in serialTypes)
        {
            if (prv == "table")
            {
                wr = true;
                prv = string.Empty;
            }
            HandleSerialTypes(dbFile, st, ref prv);
            if (wr)
            {
                Console.WriteLine(prv);
                wr = false;
            }
        }
    }
}
else
{
    throw new InvalidOperationException($"Invalid command: {command}");
}

static void HandleSerialTypes(FileStream dbFile, long serialType, ref string prv)
{
    switch (serialType)
    {
        case 0:
            break;
        case <= 8:
            var t = ReadVarint(dbFile, out int read);
            break;
        case >= 12:
            if (serialType % 2 == 1)
            {
				prv = ReadRecordString(dbFile, (int)((serialType - 13) / 2));
            }
            else
			{
				prv = ReadRecordString(dbFile, (int)((serialType - 12) / 2)).TrimEnd();
			}
            break;
    }
}

static string ReadRecordString(FileStream dbFile, int length)
{
    var buffer = new byte[length].AsSpan();
    int read = dbFile.Read(buffer);
    if (read != length)
    {
        throw new ArgumentException("Failed to read the whole string");
    }

    return Encoding.UTF8.GetString(buffer);
}

static List<long> ReadRecordHeader(FileStream databaseFile)
{
	long recordHeaderSize = ReadVarint(databaseFile, out var readBytes);
    recordHeaderSize -= readBytes;

    List<long> res = new();
    while (recordHeaderSize > 0)
    {
        long val = ReadVarint(databaseFile, out readBytes);
        recordHeaderSize -= readBytes;
		res.Add(val);
    }

    return res;
}

static long ReadIntAndConvert(FileStream dbFile, int byteCnt)
{
    var bytes = ReadInt(dbFile, byteCnt / 4);
    return bytes.Length switch
    {
        2 => BinaryPrimitives.ReadInt16BigEndian(bytes),
        4 => BinaryPrimitives.ReadInt32BigEndian(bytes),
        8 => BinaryPrimitives.ReadInt64BigEndian(bytes),
        _ => throw new ArgumentException("Failed to read")
    };
}

static byte[] ReadInt(FileStream dbFile, int byteCnt)
{
	byte[] bytes = new byte[byteCnt];
	_ = dbFile.Read(bytes, 0, byteCnt);
    return bytes;
}

static ushort ReadTwoBytesAsInt16(FileStream dbFile)
{
    var bytes = ReadInt(dbFile, 2);
	return BinaryPrimitives.ReadUInt16BigEndian(bytes);
}

static long ReadVarint(FileStream databaseFile, out int readbytes)
{
    long res = 0;
    byte[] buffer = new byte[1];
    byte leadingOne = 1 << 7;
	for (int i = 0; i < 9; ++i)
    {
        var read = databaseFile.Read(buffer, 0, 1);
        if (read != 1)
        {
            throw new InvalidOperationException($"Read {read} bytes. Should be 1.");
        }

        byte r = buffer[0];
        if (i == 8 || (r & leadingOne) == 0)
        {
            readbytes = i + 1;
			return res + r;
        }

		r -= leadingOne;
		res += r;
    }

    throw new InvalidOperationException("Outside of the loop.");
}