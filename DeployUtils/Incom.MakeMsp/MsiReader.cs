using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WindowsInstaller;

namespace MakeMsp
{
	/// <summary>
	/// Small internal msi reader
	/// </summary>
	class MsiReader
	{
		/// <summary>
		/// Поулчить значение свойства из таблицы Property
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="property">The property.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception">Ошибка получения класса WindowsInstaller.Installer</exception>
		public static string GetMSIParameters(string fileName, string property)
		{
			// Get the type of the Windows Installer object
			Type installerType = Type.GetTypeFromProgID("WindowsInstaller.Installer");
			if (installerType == null)
				throw new Exception("Ошибка получения класса WindowsInstaller.Installer");

			// Create the Windows Installer object
			var installer = (Installer)Activator.CreateInstance(installerType);
			if (installer == null)
				throw new Exception("Ошибка получения экземпляра объекта WindowsInstaller.Installer");

			// Open the MSI database in the input file
			Database database = installer.OpenDatabase(fileName, MsiOpenDatabaseMode.msiOpenDatabaseModeReadOnly);
			if (database == null)
				throw new Exception("Не удалось открыть базу данных msi для чтения");

			// Open a view on the Property table for the version property
			var view = database.OpenView("SELECT * FROM Property where Property='" + property + "'");
			if (view == null)
				throw new Exception("Не удалось открыть view msi для чтения");

			string product = null;
			try
			{
				// Execute the view query
				view.Execute(null);

				// Get the record from the view
				Record record = view.Fetch();
				product = record.get_StringData(2);
				Marshal.ReleaseComObject(record);
			}
			catch (Exception ex)
			{
				product = ex.Message;
			}

			view.Close();
			Marshal.ReleaseComObject(view);
			Marshal.ReleaseComObject(database);

			return product;
		}

	}
}
