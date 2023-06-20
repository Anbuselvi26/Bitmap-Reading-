using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

class ImageInfo
{
    public int ImageNumber { get; set; }
    public string FilePath { get; set; }
    public double Brightness { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        List<string> filePaths = new List<string>();

        Console.WriteLine("Enter the number of BMP files:");
        int fileCount = int.Parse(Console.ReadLine());

        for (int i = 0; i < fileCount; i++)
        {
            Console.Write($"Enter the file path for BMP file {i + 1}: ");
            string filePath = Console.ReadLine();
            filePaths.Add(filePath);
        }

        foreach (string filePath in filePaths)
        {
            ProcessBmpFile(filePath);
            Console.WriteLine();
        }

        CompareBrightnessValues(filePaths);
        Console.ReadKey();
    }

    static void ProcessBmpFile(string filePath)
    {
        Console.WriteLine($"Processing BMP file: {filePath}");

        if (File.Exists(filePath))
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                string stringByte = BitConverter.ToString(bytes);

                if (stringByte[0] == '4' && stringByte[1] == '2' && stringByte[3] == '4' && stringByte[4] == 'D')
                {
                    using (var bmp = new Bitmap(filePath))
                    {
                        Console.WriteLine("Valid file");
                        Console.WriteLine("Resolution: " + bmp.Width + "x" + bmp.Height);
                        Console.WriteLine("Image width: " + bmp.Width);
                        Console.WriteLine("Image height: " + bmp.Height);
                    }

                    int offset = 2;
                    int size = 4;

                    // Extract the bytes representing the file size
                    byte[] fileSizeBytes = new byte[size];
                    Array.Copy(bytes, offset, fileSizeBytes, 0, size);

                    // Convert the binary value to decimal
                    int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

                    // Print the decimal file size
                    Console.WriteLine("File size: " + fileSize + " bytes");

                    double kb = fileSize / 1024.0;
                    double mb = kb / 1024.0;
                    Console.WriteLine("Size (mb): " + mb + "MB");

                    // BPP
                    int colorDepth = GetBmpColorDepth(filePath);

                    if (colorDepth > 0)
                    {
                        Console.WriteLine("Color Depth: " + colorDepth + " bits per pixel");
                    }
                    else
                    {
                        Console.WriteLine("Unable to determine the color depth of the BMP file.");
                    }

                    // Color count
                    int colorCount = GetColorCount(filePath);

                    Console.WriteLine("The color count of the BMP file is: " + colorCount);

                    // Pixel values
                    PrintPixelValues(filePath);
                    Console.WriteLine("Pixel values Output saved to 'PixelValue.txt'");

                    //brightness values
                    Bitmap image = new Bitmap(filePath);
                    double bvalues = CalculateAverageBrightness(image);
                    Console.WriteLine("Brightness value: " + bvalues);
                }
                else
                {
                    Console.WriteLine("Not a valid file.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
        else
        {
            Console.WriteLine("BMP file not found.");
        }
    }

    static int GetBmpColorDepth(string filePath)
    {
        using (FileStream fs = File.OpenRead(filePath))
        {
            fs.Seek(28, SeekOrigin.Begin); // Color depth information is at offset 28

            byte[] colorDepthBytes = new byte[2];
            fs.Read(colorDepthBytes, 0, 2);

            int colorDepth = BitConverter.ToUInt16(colorDepthBytes, 0);
            return colorDepth;
        }
    }

    static int GetColorCount(string filePath)
    {
        Bitmap bitmap = new Bitmap(filePath);
        HashSet<Color> colorSet = new HashSet<Color>();

        for (int x = 0; x < bitmap.Width; x++)
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                Color pixelColor = bitmap.GetPixel(x, y);
                colorSet.Add(pixelColor);
            }
        }

        return colorSet.Count;
    }

    static void PrintPixelValues(string filePath)
    {
        Bitmap bitmap = new Bitmap(filePath);
        using (StreamWriter writer = new StreamWriter("Pixelvalues.txt"))
        {
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    Color pixelColor = bitmap.GetPixel(x, y);

                    int red = pixelColor.R;
                    int green = pixelColor.G;
                    int blue = pixelColor.B;

                    writer.WriteLine("Pixel at ({0}, {1}): R={2}, G={3}, B={4}", x, y, red, green, blue);
                }
            }
        }
    }

    static void CompareBrightnessValues(List<string> filePaths)
    {
        List<ImageInfo> images = new List<ImageInfo>();

        for (int i = 0; i < filePaths.Count; i++)
        {
            string filePath = filePaths[i];
            Bitmap image = new Bitmap(filePath);
            double brightness = CalculateAverageBrightness(image);
            images.Add(new ImageInfo { ImageNumber = i + 1, FilePath = filePath, Brightness = brightness });
        }

        images.Sort((image1, image2) => image1.Brightness.CompareTo(image2.Brightness));

        Console.WriteLine("\nSorted Brightness Values:");

        foreach (var image in images)
        {
            Console.WriteLine($"Image #{image.ImageNumber} - File: {image.FilePath}, Brightness: {image.Brightness}");
        }
    }

    static double CalculateAverageBrightness(Bitmap image)
    {
        double totalBrightness = 0;
        int pixelCount = 0;

        for (int x = 0; x < image.Width; x++)
        {
            for (int y = 0; y < image.Height; y++)
            {
                Color pixelColor = image.GetPixel(x, y);
                double brightness = (0.299 * pixelColor.R + 0.587 * pixelColor.G + 0.114 * pixelColor.B);
                totalBrightness += brightness;
                pixelCount++;
            }
        }

        double averageBrightness = totalBrightness / pixelCount;
        return averageBrightness;
    }
}