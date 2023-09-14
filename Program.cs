using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Numerics;
using SixLabors.ImageSharp.PixelFormats;

using Path = System.IO.Path;

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
        if (args.Length != 7)
        {
            //디폴트 값 사용
            angle = 45;
            opacity = 20;//1~100
            fontSize = 12;
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

      
    



        MakeWaterMarkImage(inputFileName, outputFileName,
            text, angle, opacity, fontSize);

    }



    static string AddSuffixBeforeExtension(string path, string suffix)
    {
        string directory = Path.GetDirectoryName(path);
        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        string extension = Path.GetExtension(path);
        string newFilename = $"{filenameWithoutExtension}{suffix}{extension}";
        return Path.Combine(directory, newFilename);
    }

    static bool MakeWaterMarkImage(string inputFileName, string outputFileName,
        string watermarkText, int angle, int opacity, int fontSizeWeight)
    {
        using (Image image = Image.Load(inputFileName))
        {
            var div = 5;
            var fontSize = image.Width / div / fontSizeWeight;

            Font font = SystemFonts.CreateFont("Arial", fontSize);
            var brush = new SolidBrush(Color.WhiteSmoke);
            var position = new PointF(image.Width / 2, image.Height / 2);


            image.Mutate(x =>
            {
                var rendererOptions = new TextOptions(font);
                var size = TextMeasurer.MeasureSize(watermarkText, rendererOptions);
                var textLocation = new PointF((image.Width - size.Width) / 2, (image.Height - size.Height) / 2);
                var textCenter = new PointF((image.Width) / 2, (image.Height) / 2);

                List<PointF> startPoints = new List<PointF>();
                List<PointF> centerPoints = new List<PointF>();

                var xtick = image.Width / div;
                var ytick = image.Height / div;

                for (int i = 0; i < div - 1; i++)
                {
                    for (int j = 0; j < div - 1; j++)
                    {
                        var xpos = xtick * (i + 1);
                        var ypos = ytick * (j + 1);

                        startPoints.Add(new PointF(xpos - (size.Width * 0.7f), ypos));
                        centerPoints.Add(new PointF(xpos, ypos));
                    }
                }

                for (int i = 0; i < startPoints.Count(); i++)
                {
                    x.DrawText(
                 new DrawingOptions
                 {
                     GraphicsOptions = new GraphicsOptions
                     {
                         Antialias = true,
                         BlendPercentage = 0.2f,

                     },
                     Transform = Matrix3x2Extensions.CreateRotationDegrees(45, centerPoints[i])
                 },
                watermarkText,
                font,
                brush,
                startPoints[i]);
                }
            });

            image.Save(outputFileName);

            return true;
        }
    }
    static void TraverseDirectory(string path)
    {
        // 현재 디렉터리의 모든 파일을 출력
        foreach (string file in Directory.GetFiles(path))
        {
            Console.WriteLine(file);
            string newName = AddSuffixBeforeExtension(file, "_w");
            MakeWaterMarkImage(file, newName,
          text, angle, opacity, fontSize);
        }
        // 현재 디렉터리의 모든 하위 디렉터리를 탐색
        foreach (string directory in Directory.GetDirectories(path))
        {
            TraverseDirectory(directory);
        }
    }
}
