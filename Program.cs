using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;
using Assimp;
using System.Runtime.InteropServices;
using OpenTK;
using System.Security.Cryptography;

namespace BDxGraphiK
{
	internal static class Program
	{
		/// <summary>
		/// Point d'entrée principal de l'application.
		/// </summary>
		public static GLForm glForm;
		public static string TempPath = Path.GetTempPath()+@"\"+Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
		public static System.Security.Cryptography.MD5 md5;


		[STAThread]
		static unsafe void Main()
		{
			/*XmlDocument xmlDoc = new XmlDocument();
			byte[] b = File.ReadAllBytes("D:\\Jeux\\KingdomHearts\\app_KH2Tools\\exportoriginal\\@KH2\\obj\\P_EX100.mdlx-[0]_A000_[IDLE].dae");
			byte[] s = Encoding.ASCII.GetBytes("xmlns");
			for (int i=0;i<b.Length&&i<100;i++)if(b[i]==s[0]&&b[++i]==s[1]&&b[++i]==s[2]&&b[++i]==s[3]&&b[++i]==s[4])b[(i+=101)%100-s.Length]=s[1];
			xmlDoc.Load(new MemoryStream(b));

			XmlNodeList nodes = xmlDoc.SelectNodes("//instance_controller");
			for (int i=0;i<nodes.Count ;i++)
			{
				XmlNode node = nodes[i];	
				var t = xmlDoc.SelectSingleNode("//controller[@id=\""+ node.Attributes["url"].Value.Substring(1) + "\"]//skin").Attributes["source"].Value.Substring(1);
				var nod = xmlDoc.SelectSingleNode("//geometry[@id=\""+ t + "\"]//*[@material]").Attributes["material"].Value += i;
				var test = xmlDoc.SelectSingleNode("//instance_controller[@url=\""+ node.Attributes["url"].Value + "\"]//instance_material").Attributes["symbol"].Value+=i;
			}

			xmlDoc.Save("D:\\Jeux\\KingdomHearts\\app_KH2Tools\\exportoriginal\\@KH2\\obj\\P_EX100.mdlx-[0]_A000_[IDLE]-out.dae");
			*/
			/*string[] lines = Directory.GetFiles(@"D:\Users\Daniel\Desktop\caca\sora");
			foreach (string line in lines)
			{
				File.Move(line, line.Replace(".vag", ".wav"));
			}

			if (Directory.Exists(TempPath) == false)
				Directory.CreateDirectory(TempPath);
			*/
			md5 = System.Security.Cryptography.MD5.Create();
			Directory.SetCurrentDirectory("content");
			System.Threading.Thread.CurrentThread.CurrentUICulture = Compatibility.us_cultureinfo_for_decimal_separator;
			System.Threading.Thread.CurrentThread.CurrentCulture = Compatibility.us_cultureinfo_for_decimal_separator;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(glForm = new GLForm());
		}
	}
}
