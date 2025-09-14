namespace codecrafters_sqlite.src.Models;
internal class PageHeader
{
	public byte PageType { get; set; }
	public ushort FirstFreeBlock { get; set; }
	public ushort NumberOfCells { get; set; }
	public uint StartOfCellContent { get; set; }
	public byte NumberOfFragmentedBytes { get; set; }
	public uint RightMostPointer { get; set; }
	public ushort[] Pointers { get; set; } = null!;

	public static PageHeader Read(DbReader reader)
	{
		var header = new PageHeader();

		header.PageType = reader.ReadInt(1)[0];
		header.FirstFreeBlock = reader.ReadTwoBytesAsUInt16();
		header.NumberOfCells = reader.ReadTwoBytesAsUInt16();
		header.StartOfCellContent = reader.ReadTwoBytesAsUInt16();
		if (header.StartOfCellContent == 0)
			header.StartOfCellContent = ushort.MaxValue + 1;

		header.NumberOfFragmentedBytes = reader.ReadInt(1)[0];
		if (header.PageType is 2 or 5)
			header.RightMostPointer = reader.ReadFourBytesAsUInt32();

		header.Pointers = new ushort[header.NumberOfCells];
		for (int i = 0; i < header.NumberOfCells; i++)
		{
			header.Pointers[i] = reader.ReadTwoBytesAsUInt16();
		}

		return header;
	}
}
