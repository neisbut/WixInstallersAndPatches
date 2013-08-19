using Microsoft.Tools.WindowsInstallerXml;

namespace Incom.WixExtensions
{
	/// <summary>
	/// Расширение для Wix
	/// </summary>
	public class IncomWixExtension : WixExtension
	{

		private IncomPreprocessorExtensions preprocessorExtension;

		/// <summary>
		/// Расширение для препроцессора
		/// </summary>
		public override PreprocessorExtension PreprocessorExtension
		{
			get { return preprocessorExtension ?? (preprocessorExtension = new IncomPreprocessorExtensions()); }
		}

	}
}
