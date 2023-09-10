// See https://aka.ms/new-console-template for more information
using System.Drawing;
using System.IO;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;

class Program
{
    static string inputFileName;// = args[0];
    static string outputFileName;// = args[1];
    static string text;// = args[2];
    static int angle;// = 0;
    static int opacity;// = 0;
    static int fontSize;// = 0;
    static int numberOfWatermarks;//;

    static void Main(string[] args)
    {
        Console.WriteLine("Hello, I am WaterMark King!");

        //if (args.Length != 7)
        if (args.Length < 3)
        {
            Console.WriteLine("올바른 파라미터 수가 아닙니다.");
            return;
        }

         inputFileName = args[0];
         outputFileName = args[1];
         text = args[2];
         angle = 0;
         opacity = 0;
         fontSize = 0;
         numberOfWatermarks = 0;

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

        if (Directory.Exists(inputFileName))
        {
            Console.WriteLine($"{inputFileName}는 디렉터리입니다.");

            //디렉터리안에있는 모든 파일을 전부 변환
            TraverseDirectory(inputFileName);
        }
        else
        {
            Console.WriteLine($"{inputFileName}는 디렉터리가 아닙니다.");
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
    static string AddSuffixBeforeExtension(string path, string suffix)
    {
        string directory = Path.GetDirectoryName(path);
        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);

        string newFilename = $"{filenameWithoutExtension}{suffix}{extension}";
        return Path.Combine(directory, newFilename);
    }
    static void TraverseDirectory(string path)
    {
        // 현재 디렉터리의 모든 파일을 출력
        foreach (string file in Directory.GetFiles(path))
        {
            Console.WriteLine(file);
            string newName = AddSuffixBeforeExtension(file, "_w");
            MakeWaterMarkImage(file, newName,
          text, angle, opacity, fontSize, numberOfWatermarks);
        }

        // 현재 디렉터리의 모든 하위 디렉터리를 탐색
        foreach (string directory in Directory.GetDirectories(path))
        {
            TraverseDirectory(directory);
        }
    }
    static bool MakeWaterMarkImage(string inputFileName, string outputFileName, string text, int angle, int opacity, int fontSize, int numberOfWatermarks)
    {
        Image<Bgra, byte> imgInput;

        // Load the input image
        try
        {
            imgInput = new Image<Bgra, byte>(inputFileName);
        }
        catch (Exception e)
        {
            // Failed to load the input image
            Console.WriteLine(e.Message);
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

