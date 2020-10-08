using System;
using System.Collections.Generic;
using System.IO;


namespace Labb3ImageReader
{
    class Program
    {
        public static Dictionary<string, string> SupportedImages = new Dictionary<string, string>();


        static void Main(string[] args)
        {
            string filename = "";

            if (args.Length == 1)
            {
                filename = args[0];
            }
            else
            {

                Console.WriteLine("Error: This program requires a (path)filename as an input argument.");
                Environment.Exit(-1);
            }

            SetUpSupportedImages();

            try
            {
                if (File.Exists(filename))
                {
                    FileStream fs = File.OpenRead(filename);
                    byte[] fileBytes = new byte[50];
                    fs.Read(fileBytes, 0, fileBytes.Length);
                    string fileType = GetImageType(fileBytes);

                    switch (fileType)
                    {
                        case "BMP":
                        case "PNG":
                            Console.WriteLine("This is a ." + fileType.ToLower() + "image.Resolution: " + GetImageResolution(fileType, fileBytes) + "pixels.");
                            if (fileType.Equals("PNG"))
                            {
                                PrintChunks(fs);
                            }
                            break;
                        default:
                            Console.WriteLine("This is not a valid .bmp or .png file!");
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("File not found.");
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine(e.StackTrace);
            }
        }
        public static string GetImageResolution(string fileType, byte[] fileBytes)
        {
            string resolution = "0x0";

            switch (fileType)
            {
                case "BMP":
                    resolution = GetBMPResolution(fileBytes);
                    break;
                case "PNG":
                    resolution = GetPNGResolution(fileBytes);
                    break;
            }
            return resolution;
        }

        private static string GetBMPResolution(byte[] fileBytes)
        {
            int WIDTH_OFFSET = 0x12;
            int HEIGHT_OFFSET = 0x16;

            int width = (fileBytes[WIDTH_OFFSET] | fileBytes[WIDTH_OFFSET + 1] << 8 |
                fileBytes[WIDTH_OFFSET + 2] << 16 | fileBytes[WIDTH_OFFSET + 3] << 24);
            int height = (fileBytes[HEIGHT_OFFSET] | fileBytes[HEIGHT_OFFSET + 1] << 8 |
                fileBytes[HEIGHT_OFFSET + 2] << 16 | fileBytes[HEIGHT_OFFSET + 3] << 24);

            return width + "x" + height;
        }

        private static string GetPNGResolution(byte[] fileBytes)
        {
            int WIDTH_OFFSET = 0x10;
            int HEIGHT_OFFSET = 0x14;

            int width = (fileBytes[WIDTH_OFFSET] << 24 | fileBytes[WIDTH_OFFSET + 1] << 16 |
                fileBytes[HEIGHT_OFFSET + 2] << 8 | fileBytes[WIDTH_OFFSET + 3]);
            int height = (fileBytes[HEIGHT_OFFSET] << 24 | fileBytes[HEIGHT_OFFSET + 1] << 16 |
                fileBytes[HEIGHT_OFFSET + 2] << 8 | fileBytes[HEIGHT_OFFSET + 3]);

            return width + "x" + height;
        }

        public static string GetImageType(byte[] fileBytes)
        {
            string imageType = "Unsupported";
            string testForBMPSignature = (fileBytes[0].ToString("X2") + fileBytes[1].ToString("X2")).ToUpper();
            string testForPNGSignature = (fileBytes[0].ToString("X2") + fileBytes[1].ToString("X2") +
                                          fileBytes[2].ToString("X2") + fileBytes[3].ToString("X2") +
                                          fileBytes[4].ToString("X2") + fileBytes[5].ToString("X2") +
                                          fileBytes[6].ToString("X2") + fileBytes[7].ToString("X2")).ToUpper();
            foreach (string key in SupportedImages.Keys)
            {
                if (SupportedImages[key] == testForBMPSignature)
                {
                    imageType = "BMP";
                    break;
                }
                else if ((SupportedImages[key] == testForPNGSignature))
                {
                    imageType = "PNG";
                    break;
                }
            }

            return imageType;
        }

        private static void PrintChunks(FileStream fs)
        {
            int CHUNK_OVERHEAD_BYTES = 12;
            int offset = 8;
            int chunkLengthInBytes;
            string chunkTypeAsString;
            Byte[] chunkLengthAndType = new byte[8];

            fs.Seek(offset, SeekOrigin.Begin);
            while (fs.Read(chunkLengthAndType, 0, 8) != 0)
            {
                chunkLengthInBytes = (chunkLengthAndType[0] << 24 | chunkLengthAndType[1] << 16 |
                    chunkLengthAndType[2] << 8 | chunkLengthAndType[3]);
                chunkTypeAsString = ((char)chunkLengthAndType[4]).ToString() + ((char)chunkLengthAndType[5]).ToString() +
                                    ((char)chunkLengthAndType[6]).ToString() + ((char)chunkLengthAndType[7]).ToString();
                Console.WriteLine("Chunk Type: " + chunkTypeAsString + " /Chunk data length: " + chunkLengthInBytes +
                    "bytes (" + (chunkLengthInBytes + 12) + "bytes including overhead)");

                offset += chunkLengthInBytes + CHUNK_OVERHEAD_BYTES;
                fs.Seek(offset, SeekOrigin.Begin);
            }
        }

        public static void SetUpSupportedImages()
        {
            SupportedImages["BMP"] = "424D";

            SupportedImages["PNG"] = "89504E470D0A1A0A";
        }
    }
}
           
