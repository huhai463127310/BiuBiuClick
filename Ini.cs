using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace BiuBiuClick
{
    public partial class Ini
    {
		//INI文件名
		private string path;

		//声明读写INI文件的API函数
		[DllImport("kernel32")]
		private static extern long WritePrivateProfileString(string section, string key,
															 string val, string filePath);
		[DllImport("kernel32")]
		private static extern int GetPrivateProfileString(string section, string key, string def,
														  StringBuilder retVal, int size, string filePath);

		//类的构造函数，传递INI文件名
		public Ini(string INIPath)
		{
			path = INIPath;
		}

		//写INI文件
		public void IniWriteValue(string Section, string Key, string Value)
		{
			WritePrivateProfileString(Section, Key, Value, this.path);
		}

		//读取INI文件指定
		public string ReadValue(string Section, string Key)
		{
			StringBuilder temp = new StringBuilder(255);
			int i = GetPrivateProfileString(Section, Key, "", temp, 255, this.path);
			return temp.ToString();
		}
	}
}
