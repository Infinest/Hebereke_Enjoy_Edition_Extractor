using System;
using System.IO;

namespace Hebereke_Enjoy_Edition_Extractor
{
    internal class Program
    {
        private const UInt32 REVERSE_CRC32_GEN_POLYNOMIAL = 0xEDB88320;
        private const UInt32 VALID_ROM_CRC32 = 0x2A137974;

        private const string TARGET_FILE_NAME = "NesHebereke_x64_Release.dll";
        private const string OUTPUT_FILE_NAME = "Hebereke (Japan).nes";

        private const uint ROM_SIZE = 0x40010;

        private const int GENERIC_PROCESSING_ERROR = 13804;
        private const int SUCCESS = 0;

        private static readonly byte[] FIXED_HEADER = { 0x4E, 0x45, 0x53, 0x1A, 0x08, 0x10, 0x50, 0x48, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 };

        static int Main(string[] args)
        {
            string executionPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string targetFile = Path.Combine(Directory.GetCurrentDirectory(), TARGET_FILE_NAME);
            byte[] fileBuffer;
            try
            {
                using (FileStream fs = new FileStream(targetFile, FileMode.Open, FileAccess.Read))
                {
                    fs.Seek(0x12A40, SeekOrigin.Begin);
                    fileBuffer = new byte[fs.Length - fs.Position];
                    fs.Read(fileBuffer, 0, fileBuffer.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occured when trying to read file \"{TARGET_FILE_NAME}\":");
                Console.WriteLine(e.Message);
                return GENERIC_PROCESSING_ERROR;
            }


            byte[] romData;
            try
            {
                romData = DecompressROM(fileBuffer);
                UInt32 crc = crc32(romData);

                if (crc != VALID_ROM_CRC32)
                {
                    Console.WriteLine("Extracted byte data is invalid.");
                    throw new Exception();
                }

                bool gotResult = false;
                do
                {
                    Console.WriteLine("Would you like to apply a fixed (NES 2.0) ROM header?");
                    ConsoleKeyInfo key = Console.ReadKey();
                    switch (key.Key)
                    {
                        case ConsoleKey.Y:
                            FIXED_HEADER.CopyTo(romData, 0);
                            gotResult = true;
                            break;
                        case ConsoleKey.N:
                            gotResult = true;
                            break;
                        default:
                            Console.WriteLine();
                            break;

                    }
                } while (!gotResult);
                Console.WriteLine();
            }
            catch
            {
                Console.WriteLine("Extraction failed.");
                return GENERIC_PROCESSING_ERROR;
            }

            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), OUTPUT_FILE_NAME);
            try
            {
                using (FileStream fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    fs.Write(romData, 0, romData.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An error occurred when trying to write file \"{OUTPUT_FILE_NAME}\":");
                Console.WriteLine(e.Message);
                return GENERIC_PROCESSING_ERROR;
            }

            Console.WriteLine($"Extracted ROM to file \"{OUTPUT_FILE_NAME}\"");

            return SUCCESS;
        }


        /// <summary>
        /// Logic for decompressing the ROM from the inputBuffer
        /// </summary>
        /// <param name="inputBuffer">Byte array where index 0 is the beginning of ROM data</param>
        /// <returns>Byte array containing the final decoded ROM data</returns>
        private static byte[] DecompressROM(byte[] inputBuffer)
        {
            byte[] outputBuffer = new byte[ROM_SIZE];
            uint writtenBytes = 0;
            uint counter = 0;
            do
            {
                if (0x2e3dd < counter) break;
                sbyte signedCurrentByteValue = (sbyte)inputBuffer[counter++];
                if (signedCurrentByteValue < 0)
                {
                    if (writtenBytes < -signedCurrentByteValue) return outputBuffer;
                    int writeCount = inputBuffer[counter++];
                    do
                    {
                        if (ROM_SIZE <= writtenBytes) return outputBuffer;
                        outputBuffer[writtenBytes] = outputBuffer[signedCurrentByteValue + writtenBytes];
                        writtenBytes++;
                    } while (0 <= --writeCount);
                }
                else
                {
                    do
                    {
                        if (ROM_SIZE <= writtenBytes) return outputBuffer;
                        outputBuffer[writtenBytes] = inputBuffer[counter++];
                        writtenBytes++;
                    } while (0 <= --signedCurrentByteValue);
                }
            } while (writtenBytes < ROM_SIZE);

            return outputBuffer;
        }

        /// <summary>
        /// Calculates a CRC32 checksum
        /// </summary>
        /// <param name="inputBuffer"></param>
        /// <returns>CRC32 checksum</returns>
        private static UInt32 crc32(byte[] inputBuffer)
        {
            UInt32 crc = UInt32.MaxValue;
            for (int i = 0; i < inputBuffer.Length; i++)
            {
                byte current = inputBuffer[i];
                for (int bit = 0; bit < 8; bit++)
                {
                    UInt32 b = (current ^ crc) & 1;
                    crc >>= 1;
                    if (0 < b) crc = crc ^ REVERSE_CRC32_GEN_POLYNOMIAL;
                    current >>= 1;
                }
            }

            return ~crc;
        }
    }
}
