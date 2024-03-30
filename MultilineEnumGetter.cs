using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDxGraphiK
{
	public class MultilineEnumGetter
	{
		Dictionary<string, object> keyValuePairs;

		public MultilineEnumGetter(string textFile, char separator)
		{
			keyValuePairs = new Dictionary<string, object>(0);
			string[] lines = File.ReadAllLines(textFile);
			foreach (string line in lines)
			{
				string[] split = line.Split(separator);

				if (split.Length == 2 && split[0].Length>0)
				{
					if (keyValuePairs.ContainsKey(split[0]))
					{
						var objet = keyValuePairs[split[0]] as List<string>;
						if (objet == null)
						{
							var list = new List<string>
							{
								keyValuePairs[split[0]] as string,
								split[1]
							};
							keyValuePairs[split[0]] = list;
						}
						else
						{
							objet.Add(split[1]);
						}
					}
					else
					{
						keyValuePairs.Add(split[0], split[1]);
					}
				}
			}
		}

		public bool GetSingleValue(string key, out string valeur)
		{
			if (this.keyValuePairs.ContainsKey(key))
			{
				valeur = this.keyValuePairs[key] as string;
				return true;
			}
			else
			{
				valeur = null;
				return false;
			}
		}

		public bool GetValues(string key, out List<string> valeur)
		{
			if (this.keyValuePairs.ContainsKey(key))
			{
				valeur = this.keyValuePairs[key] as List<string>;
				return true;
			}
			else
			{
				valeur = null;
				return false;
			}
		}
	}
}
