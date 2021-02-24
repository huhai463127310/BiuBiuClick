using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Diagnostics;

namespace BiuBiuClick
{
    class ImageMatcher
    {
        public static System.Drawing.Point FindImageCenter(string targetImagePath, string templateImagePath)
        {
            Point p = RectCenter(Match(targetImagePath, templateImagePath));
            return new System.Drawing.Point(p.X, p.Y);
        }

        public static Rect Match(string targetImagePath, string templateImagePath)
        {            
            Mat template = Cv2.ImRead(templateImagePath);
            Mat target = Cv2.ImRead(targetImagePath);

            return Match(target, template);
        }

        public static Rect Match(Mat target, Mat template)
        {            
            Mat result = new Mat();
            Cv2.MatchTemplate(target, template, result, TemplateMatchModes.SqDiffNormed);
            Point minLoc, maxLoc;
            Cv2.MinMaxLoc(result, out minLoc, out maxLoc);
            return new Rect(minLoc, template.Size());
        }

        public static Point RectCenter(Rect rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

        public static void Main(string[] args)
        {
            string targetImagePath = "C:/Users/zen/Pictures/screen.png", templateImagePath = System.Environment.CurrentDirectory + "/app/config/potplayer/potplayer-play.png";
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Rect rect = Match(targetImagePath, templateImagePath);
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
            Mat target = Cv2.ImRead(targetImagePath);
            Cv2.Rectangle(target, rect, Scalar.Blue, 1);
            Cv2.NamedWindow("target", WindowFlags.KeepRatio);
            Cv2.Circle(target, RectCenter(rect), 2, Scalar.Red);
            Cv2.ImShow("target", target);
            Cv2.WaitKey(0);
        }
    }
}
