using System.Text;

namespace Kesco.App.Web.Docs.TTN
{
	//Класс объекта для выполнения скриптов на стороне клента по инициативе сервера
	public static class ClientScripts
	{
		//Метод для установки глобальных переменных для функций из Stores.js
		public static void InitializeGlobalVariables(Kesco.Lib.Web.Controls.V4.Common.Page p)
		{
			p.JS.Write("Nakladnaya.StrResources = {{{0}}}; ", p.Resx.GetString("TTN_JsResources"));
		}

		//Метод используется для установки содержимого HTML элемента
		//Класс V4 Div не используем, что бы избежать хранение большого объема данных в процессе сервера
		public static void SendSetInnerHtml(Kesco.Lib.Web.Controls.V4.Common.Page p, string strId, string htmlContent)
		{
			StringBuilder sb = new StringBuilder(htmlContent);
			sb.Replace(@"\", @"\\");
			sb.Replace(@"'", @"\'");
			sb.Replace("\"", "\\\"");
			sb.Replace("\r", "\\\r");

			p.JS.Write("(function(){{ var el = document.getElementById('{0}'); if(el) el.innerHTML='{1}'; }})();", strId, sb.ToString());
		}

		//Метод используется для установки состояния документ корректирующий
		public static void SetCorrectableState(Kesco.Lib.Web.Controls.V4.Common.Page p, bool fCorrectable)
		{
			p.JS.Write("Nakladnaya.setCorrectableMode({0});", fCorrectable ? "true" : "false");
		}

	}
}