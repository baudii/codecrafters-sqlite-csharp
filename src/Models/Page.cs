namespace codecrafters_sqlite.src.Models;
internal class Page
{
	public Page(DbReader reader)
	{
		PageType = reader.ReadByte();
		FirstFreeBlock = reader.ReadTwoBytesAsUInt16();
		NumberOfCells = reader.ReadTwoBytesAsUInt16();
		StartOfCellContent = reader.ReadTwoBytesAsUInt16();
		if (StartOfCellContent == 0)
			StartOfCellContent = ushort.MaxValue + 1;

		NumberOfFragmentedBytes = reader.ReadByte();
		if (PageType is 2 or 5)
			RightMostPointer = reader.ReadFourBytesAsUInt32();

		Pointers = new ushort[NumberOfCells];
		for (int i = 0; i < NumberOfCells; i++)
		{
			Pointers[i] = reader.ReadTwoBytesAsUInt16();
		}
	}

	public byte PageType { get; set; }
	public ushort FirstFreeBlock { get; set; }
	public ushort NumberOfCells { get; set; }
	public uint StartOfCellContent { get; set; }
	public byte NumberOfFragmentedBytes { get; set; }
	public uint RightMostPointer { get; set; }
	public ushort[] Pointers { get; set; }
}
