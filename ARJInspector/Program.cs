using System;
using System.Collections;
using System.IO;

namespace ARJInspector
{
    class Program
    {
        static byte[] ReadBytes(byte[] data, int offset, int bytesCount)
        {
            var bytes = new byte[bytesCount];
            Array.Copy(data, offset, bytes, 0, bytesCount);
            return bytes;
        }

        static ushort ReadWord(byte[] data, int offset)
        {
            byte[] bytes = ReadBytes(data, offset, 2);
            ushort word = BitConverter.ToUInt16(bytes, 0);
            return word;
        }

        static uint ReadDWord(byte[] data, int offset)
        {
            byte[] bytes = ReadBytes(data, offset, 4);
            uint dWord = BitConverter.ToUInt32(bytes, 0);
            return dWord;
        }

        static void InspectArchive(byte[] archiveData)
        {
            // ID
            var idOffset = 0;
            ushort id = ReadWord(archiveData, idOffset);

            Console.WriteLine($"Id: {id}");
            
            // Testing for ARJ file type
            var isARJFile = id == 0xEA60;

            if (!isARJFile)
            {
                Console.WriteLine("Not an ARJ file");
                return;
            }

            // Basic header size
            var basicHeaderSizeOffset = 2;
            ushort basicHeaderSize = ReadWord(archiveData, basicHeaderSizeOffset);

            Console.WriteLine($"Basic header size: {basicHeaderSize} bytes");

            // Testing for EoA
            var isEndOfArchive = basicHeaderSize == 0;
            if (isEndOfArchive)
            {
                Console.WriteLine("End of archive");
                return;
            }

            // Header with extra data size
            var extraHeaderSizeOffset = 4;
            byte extraHeaderSize = archiveData[extraHeaderSizeOffset];

            Console.WriteLine($"Header size with extra data: {extraHeaderSize} bytes");

            // Archiver version number
            var archiverVersionNumberOffset = 5;
            byte archiverVersionNumber = archiveData[archiverVersionNumberOffset];

            Console.WriteLine($"Archiver version number: {archiverVersionNumber}");

            // Minimum version needed to extract
            var minimumVersionOffset = 6;
            byte minimumVersion = archiveData[minimumVersionOffset];

            Console.WriteLine($"Minimum version needed to extract: {minimumVersion}");

            // Host OS
            var hostOSOffset = 7;
            HostOSType hostOS = (HostOSType)archiveData[hostOSOffset];

            Console.WriteLine($"Host OS: {hostOS}");

            // Internal flags
            var internalFlagsOffset = 8;
            var internalFlags = archiveData[internalFlagsOffset];
            var bits = new BitArray(new byte[] { internalFlags });

            Console.WriteLine($"Internal flags - Password: {bits[0]}");
            Console.WriteLine($"Internal flags - Reserved: {bits[1]}");
            Console.WriteLine($"Internal flags - File continues on next disk: {bits[2]}");
            Console.WriteLine($"Internal flags - File start position field is available: {bits[3]}");
            Console.WriteLine($"Internal flags - Path translation: {bits[4]}");

            // Compression method
            var compressionMethodOffset = 9;
            CompressionMethod compressionMethod = (CompressionMethod)archiveData[compressionMethodOffset];

            Console.WriteLine($"Compression method: {compressionMethod}");

            // File type
            var fileTypeOffset = 0xA;
            FileType fileType = (FileType)archiveData[fileTypeOffset];

            Console.WriteLine($"File type: {fileType}");


            // Date of original file in MS-DOS format
            // Two consecutive words, or a longword, YYYYYYYMMMMDDDDD hhhhhmmmmmmsssss
            //YYYYYYY is years from 1980 = 0
            //sssss is (seconds / 2).
            //3658 = 0011 0110 0101 1000 = 0011011 0010 11000 = 27 2 24 = 2007 - 02 - 24
            //7423 = 0111 0100 0010 0011 - 01110 100001 00011 = 14 33 2 = 14:33:06
            //Note that the MSB changes 2043 / 4.
            var dateOffset = 0xC;
            uint date = ReadDWord(archiveData, dateOffset);

            Console.WriteLine($"Date of original file: {date}");

            // Compressed size of file
            var compressedSizeOffset = 0x10;
            uint compressedSize = ReadDWord(archiveData, compressedSizeOffset);

            Console.WriteLine($"Compressed file size: {compressedSize} bytes");

            // Original size of file
            var originalSizeOffset = 0x14;
            var originalSize = ReadDWord(archiveData, originalSizeOffset);

            Console.WriteLine($"Original file size: {originalSize} bytes");

        }

        static void Main(string[] args)
        {
            var filePath = @"D:\MyProjects\CTFPhrack\PHRAKIDX.ARJ";
            var archiveData = File.ReadAllBytes(filePath);

            InspectArchive(archiveData);

            Console.Read();
        }
    }
}
