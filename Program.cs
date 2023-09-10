// See https://aka.ms/new-console-template for more information
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        //if (args.Length != 7)
        if (args.Length < 3)
        {
            Console.WriteLine("올바른 파라미터 수가 아닙니다.");
            return;
        }

        string inputFileName = args[0];
        string outputFileName = args[1];
        string text = args[2];
        int angle = 0;
        int opacity = 0;
        int fontSize = 0;
        int numberOfWatermarks = 0;

        if (args.Length != 7)
        {
            //디폴트 값 사용
            angle = 45;
            opacity = 10;
            fontSize = 16;
            numberOfWatermarks = 10;
        }
        else
        {
            // 나머지 int 값을 파싱하기
            if (!int.TryParse(args[3], out angle) ||
                !int.TryParse(args[4], out opacity) ||
                !int.TryParse(args[5], out fontSize) ||
                !int.TryParse(args[6], out numberOfWatermarks))
            {
                Console.WriteLine("정수 파라미터 중에 잘못된 값이 있습니다.");
                return;
            }
        }
     

        Console.WriteLine($"inputFileName: {inputFileName}");
        Console.WriteLine($"outputFileName: {outputFileName}");
        Console.WriteLine($"text: {text}");
        Console.WriteLine($"angle: {angle}");
        Console.WriteLine($"opacity: {opacity}");
        Console.WriteLine($"fontSize: {fontSize}");
        Console.WriteLine($"numberOfWatermarks: {numberOfWatermarks}");

        MakeWaterMarkImage(inputFileName, outputFileName,
            text, angle, opacity, fontSize, numberOfWatermarks);

    }
    static bool MakeWaterMarkImage(string inputFileName, string outputFileName, string text, int angle, int opacity, int fontSize, int numberOfWatermarks)
    {
        Image<Bgra, byte> imgInput;

        // Load the input image
        try
        {
            imgInput = new Image<Bgra, byte>(inputFileName);
        }
        catch (Exception)
        {
            // Failed to load the input image
            return false;
        }

        // Watermark text and style
        string watermark = text;
        MCvScalar color = new MCvScalar(255, 255, 255);
        int thickness = 2;
        FontFace font = FontFace.HersheyDuplex;
        double fontScale = fontSize / 10.0;

        // Define the number of watermarks to add
        int numberOfWatermarksToAdd = Math.Min(15, numberOfWatermarks);

        // Calculate the number of rows and columns to distribute watermarks
        int maxColumns = Math.Min(3, numberOfWatermarksToAdd);
        int numRows = (int)Math.Ceiling((double)numberOfWatermarksToAdd / maxColumns);
        int rowSpacing = imgInput.Height / (numRows + 1);
        int colSpacing = imgInput.Width / (maxColumns + 1);

        int watermarkCount = 0;
        for (int row = 1; row <= numRows && watermarkCount < numberOfWatermarksToAdd; row++)
        {
            for (int col = 1; col <= maxColumns && watermarkCount < numberOfWatermarksToAdd; col++)
            {
                int x = col * colSpacing;
                int y = row * rowSpacing;

                Point position = new Point(x, y);

                // Apply rotation to the watermark
                using (Image<Bgra, byte> rotatedWatermark = new Image<Bgra, byte>(imgInput.Width, imgInput.Height, new Bgra(0, 0, 0, 0)))
                {
                    CvInvoke.PutText(rotatedWatermark, watermark, position, font, fontScale, color, thickness, LineType.EightConnected);

                    // Create a rotation matrix
                    Matrix<double> rotationMatrix = new Matrix<double>(2, 3);
                    PointF center = new PointF(rotatedWatermark.Width / 2f, rotatedWatermark.Height / 2f);
                    CvInvoke.GetRotationMatrix2D(center, angle, 1.0, rotationMatrix);

                    // Apply rotation to the watermark
                    CvInvoke.WarpAffine(rotatedWatermark, rotatedWatermark, rotationMatrix, rotatedWatermark.Size);

                    // Overlay the rotated watermark on the original image
                    CvInvoke.AddWeighted(imgInput, 1.0, rotatedWatermark, opacity / 100.0, 0, imgInput);
                }

                watermarkCount++;
            }
        }

        // Save the watermarked image to the output file
        try
        {
            imgInput.Save(outputFileName);
        }
        catch (Exception)
        {
            // Failed to save the watermarked image
            return false;
        }

        return true;
    }
}

