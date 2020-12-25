using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

using System.Windows.Media;
using System.Windows.Media.Imaging;

using TagLib;

namespace Music_Player_WPF
{
    public static class UsefulFunctions
	{
		public static int Clamp(int i, int min, int max)
        {
			if (i > max)
				return max;
			if (i < min)
				return min;
			return i;
		}



		private static bool HasExtension(string name, string ext)
		{
			string tempExtension = name.Substring(name.Length - ext.Length);
			return ext == tempExtension;
		}
	}
}
