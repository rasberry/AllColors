using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllColors
{

	// Inspired by 
	// https://stackoverflow.com/questions/596216/formula-to-determine-brightness-of-rgb-color

	class Program
	{
		enum Pattern {
			None = 0, BitOrder = 1, Luminance = 2, AERT = 3, HSP = 4, WCAG2 = 5,
			VofHSV = 6, IofHSI = 7, LofHSL = 8
		}

		static void Main(string[] args)
		{
			if (!ParseArgs(args)) {
				Usage();
				return;
			}

			using (var bmp = new Bitmap(FourKWidth,FourKHeigt,PixelFormat.Format24bppRgb))
			using (var fb = new FastBitmap.LockBitmap(bmp))
			{
				fb.LockBits();
				IEnumerator<Color> colorList = null;
				switch(ColorPattern)
				{
				default:
				case Pattern.BitOrder:
					colorList = PatternBitOrder(fb); break;
				case Pattern.Luminance:
					colorList = PatternSorter(fb,SortByLuminance); break;
				case Pattern.AERT:
					colorList = PatternSorter(fb,SortByAERT); break;
				case Pattern.HSP:
					colorList = PatternSorter(fb,SortByHSP); break;
				case Pattern.WCAG2:
					colorList = PatternSorter(fb,SortByWCAG2); break;
				case Pattern.VofHSV:
					colorList = PatternSorter(fb,SortByVofHSV); break;
				case Pattern.IofHSI:
					colorList = PatternSorter(fb,SortByIofHSI); break;
				case Pattern.LofHSL:
					colorList = PatternSorter(fb,SortByLofHSL); break;
				}
				Loop(fb,colorList);
				fb.UnlockBits();
				bmp.Save(FileName,ImageFormat.Png);
			}
		}

		static bool ParseArgs(string[] args)
		{
			int len = args.Length;
			for(int a=0; a<len; a++)
			{
				string c = args[a];
				if (c == "-p" && ++a < len) {
					if (Enum.TryParse(args[a],true,out Pattern p)) {
						ColorPattern = p;
					} else {
						Console.Error.WriteLine("Bad pattern '"+args[a]+"'");
						return false;
					}
				} else if (c == "-v") {
					ShowProgress = true;
				} else {
					FileName = c;
				}
			}

			if (String.IsNullOrWhiteSpace(FileName)) {
				Console.Error.WriteLine("an output filename must be provided");
				return false;
			}
			return true;
		}

		static void Usage()
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendLine(nameof(AllColors)+" [options] (image name)")
			  .AppendLine(" Options:")
			  .AppendLine("  -p (pattern name / number)     Choose a specific pattern (see below)")
			  .AppendLine("  -v                             Show Progress")
			;

			sb.AppendLine(" Patterns: ");
			var pVals = Enum.GetValues(typeof(Pattern));
			foreach(Pattern p in pVals) {
				if (p == Pattern.None) { continue; }
				sb.AppendLine("  " + (int)p+" "+p);
			}

			Console.WriteLine(sb.ToString());
		}

		static void Loop(FastBitmap.LockBitmap fb, IEnumerator<Color> colorList)
		{
			for(int y=0; y<FourKHeigt; y++) {
				for(int x=0; x<FourKWidth; x++) {
					colorList.MoveNext();
					var rgb = colorList.Current;
					fb.SetPixel(x,y,rgb);
				}
			}
		}

		static IEnumerator<Color> PatternBitOrder(FastBitmap.LockBitmap fb)
		{
			if (ShowProgress) { Console.WriteLine("Drawing "+nameof(PatternBitOrder)); }
			for(int i=0; i<NumberOfColors; i++) {
				yield return Color.FromArgb(i);
			}
		}

		delegate double ColorSortValue(Color c);
		delegate int ListSorter(int a, int b);

		static IEnumerator<Color> PatternSorter(FastBitmap.LockBitmap fb, ListSorter sorter)
		{
			if (ShowProgress) { Console.WriteLine("Sorting "+ColorPattern); }
			var cList = new List<int>(NumberOfColors);
			for(int i=0; i<NumberOfColors; i++) {
				cList.Add(i);
			}
			cList.Sort(new Comparison<int>(sorter));
			if (ShowProgress) { Console.WriteLine("Drawing "+ColorPattern); }
			for(int c=0; c<NumberOfColors; c++) {
				yield return Color.FromArgb(cList[c]);
			}
		}

		// https://en.wikipedia.org/wiki/Luminance_%28relative%29
		static int SortByLuminance(int a, int b)
		{
			Color ca = Color.FromArgb(a);
			Color cb = Color.FromArgb(b);
			double la = 0.2126 * ca.R + 0.7152 * ca.G + 0.0722 * ca.B;
			double lb = 0.2126 * cb.R + 0.7152 * cb.G + 0.0722 * cb.B;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// http://www.w3.org/TR/AERT#color-contrast
		static int SortByAERT(int a, int b)
		{
			Color ca = Color.FromArgb(a);
			Color cb = Color.FromArgb(b);
			double la = 0.299 * ca.R + 0.587 * ca.G + 0.114 * ca.B;
			double lb = 0.299 * cb.R + 0.587 * cb.G + 0.114 * cb.B;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// http://alienryderflex.com/hsp.html
		static int SortByHSP(int a, int b)
		{
			Color ca = Color.FromArgb(a);
			Color cb = Color.FromArgb(b);
			double la = Math.Sqrt(0.299 * ca.R * ca.R + 0.587 * ca.G * ca.G + 0.114 * ca.B * ca.B);
			double lb = Math.Sqrt(0.299 * cb.R * cb.R + 0.587 * cb.G * cb.G + 0.114 * cb.B * cb.B);
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// http://www.w3.org/TR/WCAG20/#relativeluminancedef
		static int SortByWCAG2(int a, int b)
		{
			Color ca = Color.FromArgb(a);
			Color cb = Color.FromArgb(b);
			double la = 0.2126 * WCAG2Normalize(ca.R) + 0.7152 * WCAG2Normalize(ca.G) + 0.0722 * WCAG2Normalize(ca.B);
			double lb = 0.2126 * WCAG2Normalize(cb.R) + 0.7152 * WCAG2Normalize(cb.G) + 0.0722 * WCAG2Normalize(cb.B);
			return la > lb ? 1 : la < lb ? -1 : 0;
		}
		static double WCAG2Normalize(byte component)
		{
			double val = component / 255.0;
			double c = val <= 0.03928
				? val / 12.92
				: Math.Pow((val + 0.055)/1.055,2.4)
			;
			return c;
		}

		// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
		static int SortByVofHSV(int a, int b)
		{
			Color ca = Color.FromArgb(a);
			Color cb = Color.FromArgb(b);
			byte la = Math.Max(ca.R,Math.Max(ca.G,ca.B));
			byte lb = Math.Max(cb.R,Math.Max(cb.G,cb.B));
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
		static int SortByIofHSI(int a, int b)
		{
			Color ca = Color.FromArgb(a);
			Color cb = Color.FromArgb(b);
			double la = ca.R + ca.G + ca.B / 3.0;
			double lb = cb.R + cb.G + cb.B / 3.0;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		// https://en.wikipedia.org/wiki/HSL_and_HSV#Lightness
		static int SortByLofHSL(int a, int b)
		{
			Color ca = Color.FromArgb(a);
			Color cb = Color.FromArgb(b);
			byte xa = Math.Max(ca.R,Math.Max(ca.G,ca.B));
			byte xb = Math.Max(cb.R,Math.Max(cb.G,cb.B));
			byte ma = Math.Min(ca.R,Math.Min(ca.G,ca.B));
			byte mb = Math.Min(cb.R,Math.Min(cb.G,cb.B));
			double la = xa + ma / 2.0;
			double lb = xb + mb / 2.0;
			return la > lb ? 1 : la < lb ? -1 : 0;
		}

		static Pattern ColorPattern = Pattern.None;
		static string FileName = null;
		static bool ShowProgress = false;
		const int FourKWidth = 4096;
		const int FourKHeigt = 4096;
		const int NumberOfColors = 16777216;
	}
}
