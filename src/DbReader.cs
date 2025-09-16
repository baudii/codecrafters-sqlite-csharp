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

	public byte ReadByte()
	{
		return (byte)dbFile.ReadByte();
	}

	public byte[] ReadInt(int byteCnt)
	{
		byte[] bytes = new byte[byteCnt];
		_ = dbFile.Read(bytes, 0, byteCnt);
		return bytes;
	}

	public uint Read3Bytes()
	{
		var bytes = new byte[4];
		_ = dbFile.Read(bytes.AsSpan(1, 3));
		return BinaryPrimitives.ReadUInt32BigEndian(bytes);
	}

	public long ReadVarint(out int readbytes)
	{
		long res = 0;
		byte leadingOne = 1 << 7;
		for (int i = 0; i < 9; ++i)
		{
			res <<= 7;
			byte r = ReadByte();
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

	public int ReadVarint2(out int bytesRead)
	{
		int result = 0;
		bytesRead = 0;
		byte b;
		do
		{
			b = (byte)dbFile.ReadByte();
			result = (result << 7) | (b & 0x7F);
			bytesRead++;
		} while ((b & 0x80) != 0);

		return result;
	}

	public string ReadString(int length)
	{
		var buffer = new byte[length];
		int read = dbFile.Read(buffer, 0, length);
		if (read != length)
		{
			throw new ArgumentException("Failed to read the whole string");
		}

		return Encoding.UTF8.GetString(buffer);
	}

	public byte[] ReadBlob(int length)
	{
		var buffer = new byte[length];
		int read = dbFile.Read(buffer, 0, length);
		if (read != length)
		{
			throw new ArgumentException("Failed to read the whole BLOB");
		}

		return buffer;
	}
}
