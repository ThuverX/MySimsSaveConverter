using Ionic.Zlib;

namespace SaveConverter;

internal static class Program
{
    
    public class Entry
    {
        public uint NameLength;
        public string Name;
        public uint Size;
        public uint UncompressedSize;
        public byte[] DataCompressed;
        public byte[] DataUncompressed;
        public byte Checksum;
        public Entry(BinaryReader reader)
        {
            NameLength = reader.ReadUInt32();
            Name = reader.ReadString((int)NameLength);
            Size = reader.ReadUInt32();
            reader.BaseStream.Position += 4;
            UncompressedSize = reader.ReadUInt32();
            reader.BaseStream.Position += 4;
            DataCompressed = reader.ReadBytes((int)Size);
            DataUncompressed = ZlibStream.UncompressBuffer(DataCompressed);
            Checksum = reader.ReadByte();
        }
        
        public Entry(){}

        public void Write(BinaryWriter writer)
        {
            using var memoryStream = new MemoryStream();
            var compressor = new ZlibStream(memoryStream, CompressionMode.Compress, CompressionLevel.Default);
            compressor.Write(DataUncompressed, 0, DataUncompressed.Length);
            compressor.Close();
            var compressed = memoryStream.ToArray();
            
            writer.Write(Name.Length);
            writer.WriteString(Name);
            writer.Write(compressed.Length);
            writer.WriteLength(4);
            writer.Write(UncompressedSize);
            writer.WriteLength(4);
            writer.Write(compressed);
            writer.Write((byte)1);
        }
    }

    static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            Console.WriteLine("Usage: SaveConverter.exe <input> <output>");
            Console.WriteLine("Extract: SaveConverter.exe savegame.sav extracted_folder");
            Console.WriteLine("Pack: SaveConverter.exe folder_to_pack packed.sav");
            return;
        }

        var inPath = args[0];
        var outPath = args[1];

        if (File.Exists(inPath) && inPath.ToLower().EndsWith(".sav"))
        {

            using var reader = new BinaryReader(File.Open(inPath, FileMode.Open));
            var fileCount = reader.ReadUInt32();
            var entries = new List<Entry>();

            for (var i = 0; i < fileCount; i++)
            {
                entries.Add(new Entry(reader));
            }

            Directory.CreateDirectory(outPath);

            foreach (var entry in entries)
            {
                var fullPath = Path.Join(outPath, $"{entry.Name}");
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                File.WriteAllBytes(fullPath, entry.DataUncompressed);
            }
            
            Console.WriteLine($"Extraction completed: {fileCount} files extracted to \"{outPath}\".");
        }
        else if (Directory.Exists(inPath))
        {
            using var writer = new BinaryWriter(File.Open(outPath, FileMode.OpenOrCreate));
            var files = Directory.GetFiles(inPath, "*", SearchOption.AllDirectories);
            var entries = new List<Entry>();
            
            foreach (var file in files)
            {
                var relPath = Path.GetRelativePath(inPath, file);
                var data = File.ReadAllBytes(file);
                var entry = new Entry
                {
                    Name = relPath,
                    UncompressedSize = (uint)data.Length,
                    DataUncompressed = data,
                };
                
                entries.Add(entry);
            }
            
            writer.Write(entries.Count);
            
            foreach (var entry in entries)
            {
                entry.Write(writer);
            }
            
            Console.WriteLine($"Packing completed: {entries.Count} files written to \"{outPath}\".");
        }
        else
        {
            Console.WriteLine("Error: Invalid input path.");
            Console.WriteLine("Ensure the input is either:");
            Console.WriteLine("- A valid .sav file for extraction");
            Console.WriteLine("- A valid directory for packing");
        }
    }
}