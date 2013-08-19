using System;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MakeMsp
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 4)
			{
				Console.WriteLine("MakeMsp usage:");
				Console.WriteLine("	MakeMsp.exe (0) (1) (2) (3)");
				Console.WriteLine("	0 - Base msi");
				Console.WriteLine("	1 - Target msi");
				Console.WriteLine("	2 - path to wix patch creation file");
				Console.WriteLine("	3 - output file name");
				return;
			}

			// make temp paths
			var tempDir = Path.Combine(Path.GetTempPath(), "MspMaker");
			var rtmPath = Path.Combine(tempDir, "rtm");
			var latesPath = Path.Combine(tempDir, "last");
			var rtmFilePath = Path.Combine(tempDir, "rtm.msi");
			var latestFilePath = Path.Combine(tempDir, "last.msi");

			// создаем папки
			if (!Directory.Exists(rtmPath))
				Directory.CreateDirectory(rtmPath);
			if (!string.IsNullOrEmpty(Path.GetDirectoryName(args[3])))
				if (!Directory.Exists(Path.GetDirectoryName(args[3])))
					Directory.CreateDirectory(Path.GetDirectoryName(args[3]));

			// копируем оба пакета во временную папку
			Task.WaitAll(
				Task.Run(
					() =>
					{
						Console.WriteLine("Start copying RTM...");
						File.Copy(args[0], rtmFilePath, true);
						Console.WriteLine("Finished copying RTM...");
					})
					,
				Task.Run(
					() =>
					{
						Console.WriteLine("Start copying latest...");
						File.Copy(args[1], latestFilePath, true);
						Console.WriteLine("Finished copying latest...");
					}));

			// формируем ссылку на файл c PatchCreation
			var productName = MsiReader.GetMSIParameters(latestFilePath, "ProductName");
			var wixPachCreationReference = string.Format(
				@"<?xml version=""1.0"" encoding=""utf-8""?>
				<Wix xmlns='http://schemas.microsoft.com/wix/2006/wi'>

					<?define Family='{0}'?>
					<?define PatchFamily='{0}'?>

					<?define PatchId='{1}'?>
					<?define ProductCode='{2}'?>
					<?define PatchVersion='{3}'?>
					<?define BaseMsi='{4}'?>
					<?define NewMsi='{5}'?>
					
					<?include {6}?>

				</Wix>",
					new string(Transliterate(productName).Where(char.IsLetterOrDigit).Take(8).ToArray()),
					Guid.NewGuid().ToString(),
					MsiReader.GetMSIParameters(latestFilePath, "ProductCode"),
					MsiReader.GetMSIParameters(latestFilePath, "ProductVersion"),
					Path.Combine(rtmPath, "rtm.msi"),
					Path.Combine(latesPath, "last.msi"),
					Path.GetFullPath(args[2])
				);

			// записываем файл-ссылку с полученными значениями
			File.WriteAllText(Path.Combine(tempDir, "desc.xml"), wixPachCreationReference, Encoding.UTF8);

			// административная установка
			exec("msiexec", string.Format("/a \"{0}\" /qn TARGETDIR=\"{1}\\\"", rtmFilePath, rtmPath));
			exec("msiexec", string.Format("/a \"{0}\" /qn TARGETDIR=\"{1}\\\"", latestFilePath, latesPath));

			// компиляция и создание патча
			exec("candle", string.Format("\"{0}\" -out \"{1}\\patch.wixobj\"", Path.Combine(tempDir, "desc.xml"), tempDir));
			exec("light", string.Format("\"{0}\\patch.wixobj\" -out \"{0}\\patch.pcp\"", tempDir));
			exec("msimsp", string.Format("-s \"{0}\\patch.pcp\" -p \"{1}\" -l \"{0}\\msimsp.log\"", tempDir, args[3]));

			// clean up
			Directory.Delete(latesPath, true);
			Directory.Delete(rtmPath, true);

			Console.WriteLine("Finished!");
		}

		/// <summary>
		/// Выполнение команд
		/// </summary>
		static void exec(string cmd, string parameters, int tryNumber = 0)
		{
			// выведем что-нибудь в лог
			Console.WriteLine("C:>{0} {1}", cmd, parameters);

			switch (cmd)
			{
				case "msiinfo":
				case "msimsp":
					cmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft SDKs\\Windows\\v7.1A\\Bin", string.Format("{0}.exe", cmd));
					break;
				case "candle":
				case "light":
					cmd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "WiX Toolset v3.7\\bin", string.Format("{0}.exe", cmd));
					break;
			}

			var p = Process.Start(new ProcessStartInfo(cmd, parameters) { UseShellExecute = false, RedirectStandardOutput = true });

			p.WaitForExit();

			// выведем результат
			Console.WriteLine(p.StandardOutput.ReadToEnd());

			if (p.ExitCode != 0 && tryNumber > 1)
				throw new Exception("Something is wrong");

			if (p.ExitCode == 0) return;

			Thread.Sleep(100);
			exec(cmd, parameters, tryNumber + 1);
		}

		/// <summary>
		/// Little transliterate function
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		static string Transliterate(string s)
		{
			char[] rus = { 'а', 'б', 'в', 'г', 'д', 'е', 'ж', 'з', 'и', 'к', 'л', 'м', 'н', 'о', 'п', 'р', 'с', 'т', 'у', 'ф', 'х', 'ц', 'ч', 'ш', 'щ', 'ы', 'э', 'ю', 'я' };
			string[] eng = { "a", "b", "v", "g", "d", "e", "zh", "z", "i", "k", "l", "m", "n", "o", "p", "r", "s", "t", "u", "f", "h", "ts", "ch", "sh", "shch", "y", "eh", "yu", "ya" };

			var sb = new StringBuilder(s.Length);
			foreach (var c in s.ToLower())
			{
				if (char.IsLetter(c))
				{
					var i = Array.IndexOf(rus, c);
					if (i >= 0)
						sb.Append(eng[i]);
				}
				else
					sb.Append(c);
			}
			return sb.ToString();
		}

		
	}
}
