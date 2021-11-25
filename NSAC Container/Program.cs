using System;
using System.IO;
using System.Text;

namespace NSAC_Container
{
    class Program
    {
        struct NSAC
        {
            public char[] MagicNumber;
            public ushort Unknown0x04;
            public ushort NumFiles;
            public uint ArchiveSize;
            public uint DataStartAddress;
            public uint BasicFileInfoSize;
            public uint NamedFileInfoSize;
            public uint Unknown0x18;
        }
        struct NSACFile
        {
            public ushort FileNameLength;
            public uint StartAddress;
            public uint FileSize;
            public string FileName;
            public byte NullByte;
        }
        static void Main(string[] args)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Boolean import = false;
            NSAC Header;
            NSACFile[] FileNSAC;
            String namefile;
            if (args.Length > 0)
            {
                if (args[0] == "-i")
                {
                    import = true;
                    namefile = args[1];
                }
                else
                {
                    import = false;
                    namefile = args[0];

                }


                using (BinaryReader reader = new BinaryReader(File.Open(namefile, FileMode.Open)))
                {
                    Header.MagicNumber = reader.ReadChars(4);
                    Header.Unknown0x04 = reader.ReadUInt16();
                    Header.NumFiles = reader.ReadUInt16();
                    Header.ArchiveSize = reader.ReadUInt32();
                    Header.DataStartAddress = reader.ReadUInt32();
                    Header.BasicFileInfoSize = reader.ReadUInt32();
                    Header.NamedFileInfoSize = reader.ReadUInt32();
                    Header.Unknown0x18 = reader.ReadUInt32();

                    Console.WriteLine("DataStartAdress: " + Header.DataStartAddress);
                    Console.WriteLine("BasicFileInfoSize: " + Header.BasicFileInfoSize);
                    Console.WriteLine("NamedFileInfoSize: " + Header.NamedFileInfoSize);
                    Console.WriteLine("Unknown18: " + Header.Unknown0x18);

                    for (int x = 0; x < Header.NumFiles; x++)
                    {
                        ushort nfile = reader.ReadUInt16();
                        uint filedirection = reader.ReadUInt32();
                        uint filesize = reader.ReadUInt32();
                    }
                    FileNSAC = new NSACFile[Header.NumFiles];

                    for (int x = 0; x < Header.NumFiles; x++)
                    {
                        FileNSAC[x].FileNameLength = reader.ReadUInt16();
                        FileNSAC[x].StartAddress = reader.ReadUInt32();
                        FileNSAC[x].FileSize = reader.ReadUInt32();
                        FileNSAC[x].FileName = Encoding.GetEncoding(932).GetString(reader.ReadBytes((int)FileNSAC[x].FileNameLength));
                        FileNSAC[x].NullByte = reader.ReadByte();

                        Console.WriteLine(FileNSAC[x].FileName);
                    }
                    reader.Close();
                }

                if (import == false)
                {
                    for (int x = 0; x < Header.NumFiles; x++)
                    {
                        using (BinaryReader reader = new BinaryReader(File.Open(namefile, FileMode.Open)))
                        {
                            using (BinaryWriter writer = new BinaryWriter(File.Open(FileNSAC[x].FileName, FileMode.Create)))
                            {
                                reader.BaseStream.Position = FileNSAC[x].StartAddress;
                                writer.Write(reader.ReadBytes((int)FileNSAC[x].FileSize));
                                writer.Close();
                            }

                        }
                    }
                }
                else
                {
                    using (Stream streamOriginal = File.OpenRead(args[1]))
                    {

                        BinaryReader reader = new BinaryReader(streamOriginal);

                        using (BinaryWriter writer = new BinaryWriter(File.Open("_" + args[1], FileMode.Create)))
                        {
                            writer.Write(File.ReadAllBytes(args[1]));
                            for (int x = 0; x < Header.NumFiles; x++)
                            {


                                writer.BaseStream.Position = FileNSAC[x].StartAddress;
                                if (FileNSAC[x].FileName == Path.GetFileName(args[2]))
                                    writer.Write(File.ReadAllBytes(args[2]));

                            }
                            writer.Close();
                        }

                    }

                }



            }else
            { Console.WriteLine("Drag the NSAC file into the executable \nto import add - i NSACFile FiletoImport"); }
        }
    }
}
