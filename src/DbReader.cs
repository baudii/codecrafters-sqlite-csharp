using System.Buffers.Binary;
using System.Text;

namespace codecrafters_sqlite.src;
internal class DbReader(FileStream dbFile)
{
	public FileStream Fs => dbFile;
	public void Seek(long offset)
	{
		dbFile.Seek(offset, SeekOrigin.Begin);
	}

	public void Skip(long offset)
	{
		dbFile.Seek(offset, SeekOrigin.Current);
	}

	public ushort ReadTwoBytesAsUInt16()
	{
		var bytes = ReadInt(2);
		return BinaryPrimitives.ReadUInt16BigEndian(bytes);
	}
	public uint ReadFourBytesAsUInt32()
	{
		var bytes = ReadInt(4);
		return BinaryPrimitives.ReadUInt32BigEndian(bytes);
	}


	public byte[] ReadInt(int byteCnt)
	{
		byte[] bytes = new byte[byteCnt];
		_ = dbFile.Read(bytes, 0, byteCnt);
		return bytes;
	}

	public long ReadVarint(out int readbytes)
	{
		long res = 0;
		byte[] buffer = new byte[1];
		byte leadingOne = 1 << 7;
		for (int i = 0; i < 9; ++i)
		{
			var read = dbFile.Read(buffer, 0, 1);
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

	public string ReadString(int length)
	{
		var buffer = new byte[length].AsSpan();
		int read = dbFile.Read(buffer);
		if (read != length)
		{
			throw new ArgumentException("Failed to read the whole string");
		}

		return Encoding.UTF8.GetString(buffer);
	}

	public string ReadBlob(int length)
	{
		var buffer = new byte[length].AsSpan();
		int read = dbFile.Read(buffer);
		if (read != length)
		{
			throw new ArgumentException("Failed to read the whole string");
		}

		return Encoding.UTF8.GetString(buffer);
	}
}
