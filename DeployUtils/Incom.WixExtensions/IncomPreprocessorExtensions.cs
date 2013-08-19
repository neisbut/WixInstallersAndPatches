using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Tools.WindowsInstallerXml;

namespace Incom.WixExtensions
{
	/// <summary>
	/// Расширение для препроцессора
	/// </summary>
	public class IncomPreprocessorExtensions : PreprocessorExtension
	{
		private readonly string[] prefixes = new[] { "incom" };

		/// <summary>
		/// Поддерживаемые префиксы
		/// </summary>
		public override string[] Prefixes
		{
			get { return prefixes; }
		}

		/// <summary>
		/// Выполнить функцию
		/// </summary>
		/// <param name="prefix">Префикс</param>
		/// <param name="function">Имя функции</param>
		/// <param name="args">Аргументы</param>
		/// <returns>Вычисленное значение</returns>
		public override string EvaluateFunction(string prefix, string function, string[] args)
		{
			if (prefix == "incom")
			{
				switch (function.ToLower())
				{
					case "fileversion":
						var ver = FileVersionInfo.GetVersionInfo(Path.GetFullPath(args[0])).FileVersion;
						Console.WriteLine(string.Format("Version of {0}: {1}", args[0], ver));

						return ver;
					case "previousminorversion":

						var version = Version.Parse(args[0]);

						if (version.Build > 0)
							return new Version(version.Major, version.Minor, 0, 0).ToString();
						if (version.Minor > 0)
							return new Version(version.Major, version.Minor - 1, 0, 0).ToString();

						return new Version(0, 0, 1, 0).ToString();


					case "changeguid":
						var guid = Guid.Parse(args[0]).ToByteArray();
						version = Version.Parse(args[1]);

						var major = BitConverter.GetBytes((Int16)((version.Major & 0xFF) ^ ((version.Major >> 16) & 0xFF)));
						var minor = BitConverter.GetBytes((Int16)((version.Minor & 0xFF) ^ ((version.Minor >> 16) & 0xFF)));
						var build = BitConverter.GetBytes((Int16)((version.Build & 0xFF) ^ ((version.Build >> 16) & 0xFF)));
						var revision = BitConverter.GetBytes((Int16)((version.Revision & 0xFF) ^ ((version.Revision >> 16) & 0xFF)));

						var len = 4;
						if (args.Length > 2)
							len = int.Parse(args[2]);

						if (len > 0)
						{
							guid[0] = major[0];
							guid[1] = major[1];
						}
						if (len > 1)
						{
							guid[2] = minor[0];
							guid[3] = minor[1];
						}
						if (len > 2)
						{
							guid[4] = build[0];
							guid[5] = build[1];
						}
						if (len > 3)
						{
							guid[6] = revision[0];
							guid[7] = revision[1];
						}

						return new Guid(guid).ToString();
				}
			}

			return base.EvaluateFunction(prefix, function, args);
		}
	}
}