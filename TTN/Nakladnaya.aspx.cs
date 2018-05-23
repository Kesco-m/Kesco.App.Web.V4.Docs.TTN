using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Stores;
using Kesco.Lib.Web.Settings;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.DBSelect.V4;
using System.Globalization;

//TODO DBSStore расширенный поиск доделать

namespace Kesco.App.Web.Docs.TTN
{
	/// <summary>
	/// Класс объекта сраницы
	/// </summary>
	public partial class Nakladnaya : DocPage
	{
		//Словарь содержит метки полей формы
		public static Dictionary<V4Control, string> FieldLabels = new Dictionary<V4Control, string>(new ReferenceEqualityComparer());

		//Строка используется для формирование идентификатора элемента-заголовка раздела
		public const string suffixTitle = "Title";

		//Тип делегат формирования заголовка раздела
		public delegate string GetTitleDelegate();

		//Словарь содержит делегаты для формирования заголовков раздела, ключи - идентификаторы элементов заголовков разделов
		public static Dictionary<string, GetTitleDelegate> AccordionTitles = new Dictionary<string, GetTitleDelegate>();

		//Контроллер раздела Грузоотправитель
		private PersonPanel _goPanel;
		//Контроллер раздела Грузополучатель
		private PersonPanel _gpPanel;
		//Контроллер раздела Поставщик
		private PersonPanel _shipperPanel;
		//Контроллер раздела Плательщик
		private PersonPanel _payerPanel;

		//Массив всех элементов управления-полей формы
		private V4Control[] _ctrls;

		//Строки для формирования идентификаторов элементов заголовков разделов
		public static class SectionPrefixes
		{
			public const string General = "General";
			public const string Go = "Go";
			public const string Gp = "Gp";
			public const string Shipper = "Shipper";
			public const string Payer = "Payer";
			public const string Documents = "Documents";
			public const string Transport = "Transport";
		}

		//Строки для формирования заголовков разделов
		public static class SectionTitles
		{
			public const string General = "Данные накладной";
			public const string SignersOfPaper = "Подписанты печатных форм";
			public const string Documents = "Документы";
			public const string Curator = "Куратор";
			public const string GO = "Грузоотправитель";
			public const string GP = "Грузополучатель";
			public const string Shipper = "Поставщик";
			public const string Payer = "Плательщик";
			public const string GoToShipper = "Совпадает с грузоотправителем:";
			public const string GpToPayer = "Совпадает с грузополучателем:";

			public const string Products = "Товары";
			public const string Services = "Услуги";
			public const string Transport = "Транспорт";

			public const string RequiredInfo = "<span class='required_info'>Отсутствует</span>";
		}

		//Строки для уведомлений полей формы
		public static class ControlNtfs
		{
			public static readonly NameValueCollection PersonAddress = new NameValueCollection()
			{
				{SectionPrefixes.Go, "адрес не соответствует грузоотправителю"},
				{SectionPrefixes.Gp, "адрес не соответствует грузополучателю"},
				{SectionPrefixes.Shipper, "адрес не соответствует поставщику"},
				{SectionPrefixes.Payer, "адрес не соответствует плательщику"}
			};

			public static readonly NameValueCollection PersonInfo = new NameValueCollection()
			{
				{SectionPrefixes.Go, "реквизиты не соответствуют грузоотправителю"},
				{SectionPrefixes.Gp, "реквизиты не соответствуют грузополучателю"},
				{SectionPrefixes.Shipper, "название не соответствует поставщику"},
				{SectionPrefixes.Payer, "название не соответствует плательщику"}
			};

			public static readonly NameValueCollection PersonCode = new NameValueCollection()
			{
				{SectionPrefixes.Go, "ОКПО не соответствует грузоотправителю"},
				{SectionPrefixes.Gp, "ОКПО не соответствует грузополучателю"},
				{SectionPrefixes.Shipper, "ОКПО не соответствует поставщику"},
				{SectionPrefixes.Payer, "ОКПО не соответствует плательщику"}
			};

			public static readonly NameValueCollection PersonNoCode = new NameValueCollection()
			{
				{SectionPrefixes.Go, "у грузоотправителя отсутствует окпо"},
				{SectionPrefixes.Gp, "у грузополучателя отсутствует окпо"},
				{SectionPrefixes.Shipper, "у поставщика отсутствует окпо"},
				{SectionPrefixes.Payer, "у плательщика отсутствует окпо"}
			};

			public static readonly NameValueCollection PersonStore = new NameValueCollection()
			{
				{SectionPrefixes.Go, "расчетный счет не соответствует грузоотправителю"},
				{SectionPrefixes.Gp, "расчетный счет не соответствует грузополучателю"},
				{SectionPrefixes.Shipper, "расчетный счет не соответствует поставщику"},
				{SectionPrefixes.Payer, "расчетный счет не соответствует плательщику"}
			};

			public const string PersonDate = "у лица отсутствует реквизиты на дату документа";
			public const string InfoStore = "реквизиты не соответствуют расчетному счёту";
			public const string AddressDate = "адрес не действует дату документа";
			public const string StoreAccountDate = "расчетный счет не действует дату документа";
			public const string PersonPosition = "должность не соответствует лицу";
			public const string PostingOnDate = "дата проводки должна быть больше или равна дате текущего документа";
			public const string BaseDocumentOnDate = "дата основания должна быть меньше или равна дате текущего документа";
			public const string PersonDocument = "лица документа не соответствуют поставщику и плательщику";
			public const string CurrencyDocument = "валюта банковского счета не соответствует валюте документа";
			public const string EnclosureContract = "приложения не связаны с документом";
			public const string PaymentInvoice = "платежные документы не имеют оснований";
			//public const string StoreDocument = "расчетный счет не соответствует счету в договоре";
			//public const string StoreEnclosure = "расчетный счет не соответствует счету в приложении";
			//public const string BankDate = "у банка отсутствует реквизиты на дату";
			//public const string BankName = "у банка отсутствует краткое название";
			public const string DocumentForm = "документ не имеет электронной формы";
			//public const string DogovorTtn = "срок действия договора не соответствует дате накладной";
			//public const string PersonOnDate = "валюта оплаты не соответствует валюте оплаты в приложении";
			//public const string PersonOnDate = "валюта оплаты не соответствует валюте оплаты в счете";
			//public const string PersonOnDate = "не указано приложение к договору";
		}

		/// <summary>
		/// Класс сравнения ссылок на объекты, равны если указывают на один объект
		/// </summary>
		public sealed class ReferenceEqualityComparer : IEqualityComparer<V4Control>
		{
			public bool Equals(V4Control x, V4Control y)
			{
				return Object.ReferenceEquals(x,y);
			}

			public int GetHashCode(V4Control obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			ClientScripts.InitializeGlobalVariables(this);

			FillLables();

			Kesco.Lib.Entities.Documents.EF.Trade.TTN d = Doc as Kesco.Lib.Entities.Documents.EF.Trade.TTN;

			//Заголовок документа
			//Корректируемый документ должен иметь тип ТТН
			CorrectableTtn.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = d.Type, QueryType = DocTypeQueryType.Equals });
			CorrectableTtn.Filter.NextType.Value = d.TypeID.ToString();
			CorrectableTtn.Filter.NextType.FieldId = d.CorrectingDocField.DocFieldId;
			CorrectableTtn.OnRenderNtf += Document_OnRenderNtf;
			CorrectableTtn.Changed += CorrectableTtn_Changed;

			//Секция Данные накладной
			DateOfPosting.OnRenderNtf += DateOfPosting_OnRenderNtf;

			//секция Грузоотправитель
			_goPanel = PersonPanel.Init(this, SectionPrefixes.Go, GoInfo, GoCodeInfo, GO, GoAddress, DispatchPoint, GoStore, GoStoreInfo, GoNotes);

			//секция Грузополучатель
			_gpPanel = PersonPanel.Init(this, SectionPrefixes.Gp, GpInfo, GpCodeInfo, GP, GpAddress, DestinationPoint, GpStore, GpStoreInfo, GpNotes);

			//секция Поставщик
			_shipperPanel = PersonPanel.Init(this, SectionPrefixes.Shipper, ShipperInfo, ShipperCodeInfo, Shipper, ShipperAddress, null, ShipperStore, ShipperStoreInfo, null);

			_shipperPanel.PostGetTitle = Shipper_PostGetTitle;

			Shipper.Changed += ShipperOrPayer_Changed;

			//секция Плательщик
			_payerPanel = PersonPanel.Init(this, SectionPrefixes.Payer, PayerInfo, PayerCodeInfo, Payer, PayerAddress, null, PayerStore, PayerStoreInfo, null);
			Payer.Changed += ShipperOrPayer_Changed;

			Director.Filter.PersonType = 2;//Физические лица
			Accountant.Filter.PersonType = 2;//Физические лица
			StoreKeeper.Filter.PersonType = 2;//Физические лица

			Director.Changed += Director_Changed;
			Accountant.Changed += Accountant_Changed;
			StoreKeeper.Changed += StoreKeeper_Changed;

			DirectorPosition.OnRenderNtf += DirectorPosition_OnRenderNtf;
			AccountantPosition.OnRenderNtf+= AccountantPosition_OnRenderNtf;
			StoreKeeperPosition.OnRenderNtf += StoreKeeperPosition_OnRenderNtf;

			//Секция Транспорт
			CarTtn.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.ТранспортнаяНакладная, QueryType = DocTypeQueryType.Equals });
			//Транспортная накладная это документ который является основанием по полю 1584 для ТТН 2145
			CarTtn.Filter.NextType.Value = d.TypeID.ToString();
			CarTtn.Filter.NextType.FieldId = d.TTNTrField.DocFieldId;
			CarTtn.OnRenderNtf += Document_OnRenderNtf;

			//Секция Документы
			Contract.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.Договор, QueryType = DocTypeQueryType.WithChildrenSynonyms });
			Contract.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.Счет, QueryType = DocTypeQueryType.Equals });
			Contract.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.ИнвойсПроформа, QueryType = DocTypeQueryType.Equals });

			Contract.Filter.NextType.Value = d.TypeID.ToString();
			Contract.Filter.NextType.FieldId = d.DogovorField.DocFieldId;
			Contract.OnRenderNtf += Document_OnRenderNtf;
			Contract.BeforeSearch += new BeforeSearchEventHandler((object s) => LinkedDoc_BeforeSearch(Contract, Enclosure));

			Enclosure.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.Приложение, QueryType = DocTypeQueryType.WithChildren });
			//Enclosure.Filter.NextType.Value = d.TypeID.ToString();
			//Enclosure.Filter.NextType.FieldId = d.PrilozhenieField.DocFieldId;
			Enclosure.OnRenderNtf += Document_OnRenderNtf;
			Enclosure.OnRenderNtf += Enclosure_OnRenderNtf;
			Enclosure.BeforeSearch += new BeforeSearchEventHandler((object s) => LinkedDoc_BeforeSearch(Enclosure, Contract));

			ApplicationForPurchasing.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.ЗаявкаНаПокупку, QueryType = DocTypeQueryType.Equals });
			ApplicationForPurchasing.Filter.NextType.Value = d.TypeID.ToString();
			ApplicationForPurchasing.Filter.NextType.FieldId = d.ZvkBField.DocFieldId;
			ApplicationForPurchasing.OnRenderNtf += Document_OnRenderNtf;

			LetterOfCredit.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.АккредитивLci, QueryType = DocTypeQueryType.Equals });
			LetterOfCredit.Filter.NextType.Value = d.TypeID.ToString();
			LetterOfCredit.Filter.NextType.FieldId = d.AkkredField.DocFieldId;
			LetterOfCredit.OnRenderNtf += Document_OnRenderNtf;

			BillOfLading.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.Коносамент, QueryType = DocTypeQueryType.Equals });
			BillOfLading.Filter.NextType.Value = d.TypeID.ToString();
			BillOfLading.Filter.NextType.FieldId = d.BillOfLadingField.DocFieldId;
			BillOfLading.OnRenderNtf += Document_OnRenderNtf;

			Invoice.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.Счет, QueryType = DocTypeQueryType.Equals });
			Invoice.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.ИнвойсПроформа, QueryType = DocTypeQueryType.Equals });
			Invoice.Filter.NextType.Value = d.TypeID.ToString();
			Invoice.Filter.NextType.FieldId = d.SchetPredField.DocFieldId;
			Invoice.OnRenderNtf += Document_OnRenderNtf;
			//Invoice.BeforeSearch += new BeforeSearchEventHandler((object s) => LinkedDoc_BeforeSearch(Invoice, PaymentDocuments));
			Invoice.Changed += new ChangedEventHandler((object s, ProperyChangedEventArgs eargs) => LinkedDoc_Changed2(Invoice, PaymentDocuments));

			PaymentDocuments.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.ПлатежноеПоручение, QueryType = DocTypeQueryType.Equals });
			PaymentDocuments.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.Swift, QueryType = DocTypeQueryType.Equals });
			PaymentDocuments.Filter.NextType.Value = d.TypeID.ToString();
			PaymentDocuments.Filter.NextType.FieldId = d.PlatezhkiField.DocFieldId;
			PaymentDocuments.OnRenderNtf += Document_OnRenderNtf;
			PaymentDocuments.OnRenderNtf += PaymentDocuments_OnRenderNtf;
			//PaymentDocuments.BeforeSearch += new BeforeSearchEventHandler((object s) => LinkedDoc_BeforeSearch(PaymentDocuments, Invoice));
			PaymentDocuments.Changed += new ChangedEventHandler((object s, ProperyChangedEventArgs eargs) => LinkedDoc_Changed1(PaymentDocuments, Invoice));

			if(!Doc.IsNew)
				RenderNonPanelControlsNtf();

			SetAccortdionTitles();
		}

		protected override void ProcessCommand(string cmd, NameValueCollection param)
		{
			switch (cmd)
			{
				//Команда для установки данных панели поставщик как у грузоотправителя
				case "OnGoToShipper":
					if (param["value"] == "true")
						_goPanel.BindTo(_shipperPanel);
					else
						_goPanel.BindTo(null);
					break;

				//Команда для установки данных панели плательщик как у грузополучателем
				case "OnGpToPayer":
					if (param["value"] == "true")
						_gpPanel.BindTo(_payerPanel);
					else
						_gpPanel.BindTo(null);
					break;

				//Команда для установки режима работы с корректируемым документом
				//Если value==0, то режим отключен
				case "SetCorrectableDocument":
					{
						CorrectableFlag.Value = param["value"] == "true" ? "1" : "0";
						CorrectableTtn.IsDisabled = CorrectableFlag.Value != "1";
						if (!CorrectableTtn.IsDisabled && !string.IsNullOrEmpty(CorrectableTtn.Value))
							CorrectableTtn_Changed(null, null);

						ClientScripts.SetCorrectableState(this, !CorrectableTtn.IsDisabled);
					}
					break;

				//Команда для установки заголовка раздела
				case "SetAccordionHeader":
					SetAccordionHeader(param["id"]);
					break;

				default:
					base.ProcessCommand(cmd, param);
					break;
			}
		}

		/// <summary>
		/// Метод формирует и возвращает массив свойств объекта документа TTN.
		/// Порядок элементов в массиве соответствует порядку следования полей в массиве _ctrls
		/// и может быть сопоставлен с ним
		/// </summary>
		/// <param name="d">Объект-документ</param>
		/// <returns>Массив полей документа</returns>
		DocField[] GetDocFields(Kesco.Lib.Entities.Documents.EF.Trade.TTN d)
		{
			return new DocField[]{
				d.CorrectingFlagField,
				d.CorrectingDocField,

				//Данные накладной
				d.DateProvodkiField,
				d.CurrencyField,
				d.PrimechanieField,
				d.SignSupervisorField,
				d.SignSupervisorPostField,
				d.SignBuhgalterField,
				d.SignBuhgalterPostField,
				d.SignOtpustilField,
				d.SignOtpustilPostField,

				//Поставщик
				d.PostavschikDataField,
				d.PostavschikOKPOField,
				d.PostavschikField,
				d.PostavschikAddressField,
				d.PostavschikBSField,
				d.PostavschikBSDataField,

				//Плательщик
				d.PlatelschikDataField,
				d.PlatelschikOKPOField,
				d.PlatelschikField,
				d.PlatelschikAddressField,
				d.PlatelschikBSField,
				d.PlatelschikBSDataField,

				//Грузоотправитель
				d.GOPersonDataField,
				d.GOPersonOKPOField,
				d.GOPersonField,
				null,
				d.GOPersonWeselField,
				d.GOPersonBSField,
				d.GOPersonBSDataField,
				d.GOPersonNoteField,

				//Грузополучатель
				d.GPPersonDataField,
				d.GPPersonOKPOField,
				d.GPPersonField,
				null,
				d.GPPersonWeselField,
				d.GPPersonBSField,
				d.GPPersonBSDataField,
				d.GPPersonNoteField,

				//Документы
				d.DogovorTextField,
				d.DogovorField,
				d.PrilozhenieField,
				d.ZvkBField,
				d.AkkredField,
				d.BillOfLadingField,
				d.SchetPredField,
				d.PlatezhkiField,

				//Транспорт
				d.DoverennostField,
				d.VoditelField,
				d.AvtomobilField,
				d.AvtomobilNomerField,
				d.PritsepNomerField,
				d.TTNTrField
			};
		}

		private void FillLables()
		{
			Kesco.Lib.Entities.Documents.EF.Trade.TTN d = Doc as Kesco.Lib.Entities.Documents.EF.Trade.TTN;

			DocField[] fields = GetDocFields(d);

			Debug.Assert(fields.Length == _ctrls.Length);

			for (int i = 0; i < _ctrls.Length; ++i)
			{
				if (null == fields[i]) continue;
				FieldLabels[_ctrls[i]] = GetDocFieldDescription(fields[i]) + ":";
			}

			//Устанавливаются отдельно
			FieldLabels[GoAddress] = "Адрес грузоотправителя:";
			FieldLabels[GpAddress] = "Адрес грузополучателя:";
		}

		private string GetDocFieldDescription(DocField f)
		{
			if (IsRusLocal) return f.DocumentField;
			if (IsEstLocal) return f.DocumentFieldET;

			return f.DocumentFieldEN;
		}

		/// <summary>
		/// Метод устанавливает заголовки всех разделов страницы
		/// </summary>
		void SetAccortdionTitles()
		{
			AccordionTitles[SectionPrefixes.General + suffixTitle] = GeneralDataGetTitle;
			AccordionTitles[SectionPrefixes.Go + suffixTitle] = _goPanel.GetTitle;
			AccordionTitles[SectionPrefixes.Gp + suffixTitle] = _gpPanel.GetTitle;
			AccordionTitles[SectionPrefixes.Shipper + suffixTitle] = _shipperPanel.GetTitle;
			AccordionTitles[SectionPrefixes.Payer + suffixTitle] = _payerPanel.GetTitle;
			AccordionTitles[SectionPrefixes.Documents + suffixTitle] = DocumentsGetTitle;
			AccordionTitles[SectionPrefixes.Transport + suffixTitle] = TransportGetTitle;

			foreach (KeyValuePair<string, GetTitleDelegate> p in AccordionTitles)
				ClientScripts.SendSetInnerHtml(this, p.Key, p.Value());
		}

		/// <summary>
		/// Метод формирует описание данных установленных в элементе управления
		/// </summary>
		/// <param name="ctrl">Элемент управления</param>
		/// <param name="fEncoded">Если true, то строка будет закодирована в кодироку HTML</param>
		/// <returns>Строковое описание данных элемента управления</returns>
		public static string GetCtrlText(V4Control ctrl, bool fEncoded)
		{
			StringBuilder sbValues = new StringBuilder();
			if (ctrl is Select)
			{
				Select sc = ctrl as Select;

				if (sc.IsMultiSelect)
				{
					foreach (Kesco.Lib.Entities.Item i in sc.SelectedItems)
					{
						if (sbValues.Length > 0) sbValues.Append(", ");

						object o = i.Value;

						if (o is Kesco.Lib.Entities.Entity)
						{
							sbValues.Append((o as Kesco.Lib.Entities.Entity).Name);
						}
						else
							if (o is Kesco.Lib.Entities.Item)
							{
								sbValues.Append(((Kesco.Lib.Entities.Item)o).Value);
							}
							else
							{
								Type to = o.GetType();
								object oText = to.GetProperty(sc.ValueField).GetValue(o, null);
								if (null != oText)
								{
									string strName = oText.ToString();
									sbValues.Append(strName);
								}
							}
					}
				}
				else
					sbValues.Append(sc.ValueText);
			}
			else
				sbValues.Append(ctrl.Value);

			if (fEncoded)
				return HttpUtility.HtmlEncode(sbValues.ToString());

			return sbValues.ToString();
		}

		/// <summary>
		/// Метод возвращает-суммарное описание данных для заголовка секции для группы элементов
		/// </summary>
		/// <param name="ctrls">Группа элементов для которых составляется описание</param>
		/// <returns>Строковое описание</returns>
		public static string GetSectionHtmlDescription(IEnumerable<V4Control> ctrls)
		{
			const string sep = "; ";

			StringBuilder sbDescription = new StringBuilder();

			foreach (V4Control ctrl in ctrls)
			{
				if (null == ctrl) continue;

				string textOfControl = GetCtrlText(ctrl, true);

				if (textOfControl.Length < 1)
				{
					if (!ctrl.IsRequired) continue;

					textOfControl = SectionTitles.RequiredInfo;
				}

				if (sbDescription.Length > 0) sbDescription.Append(sep);

				sbDescription.AppendFormat("{0} {1}", FieldLabels[ctrl], textOfControl);
			}

			return sbDescription.ToString();
		}

		/// <summary>
		/// Метод возвращает-суммарное описание данных для заголовка секции для групп элементов
		/// Для каждой группы формируется отдельная строка в описании
		/// </summary>
		/// <param name="ctrls">Массив групп элеметов</param>
		/// <returns>Описание данных для заголовка</returns>
		string CreateTitleMultiLineDescription(IEnumerable<V4Control>[] ctrls)
		{
			StringBuilder sb = new StringBuilder();

			//Уведомление добавляется как <div> элемент, поэтому после него не требуется добавлять <br/> перед следующим элементом
			bool fNtfAdded = false;
			foreach (IEnumerable<V4Control> ctrl_array in ctrls)
			{
				string dataInfo = Nakladnaya.GetSectionHtmlDescription(ctrl_array);

				using (StringWriter ntfText = new StringWriter())
				{
					string strNtfInfo = string.Empty;

					foreach (V4Control ctrl in ctrl_array)
						ctrl.RenderNtf(ntfText);

					if (dataInfo.Length > 0 && ntfText.GetStringBuilder().Length > 0)
						strNtfInfo = ntfText.ToString();

					bool fData = dataInfo.Length > 0 || strNtfInfo.Length > 0;
					if (!fNtfAdded && sb.Length > 0 && fData)
						sb.Append("<br/>");

					if (fData) fNtfAdded = strNtfInfo.Length > 0;

					sb.Append(dataInfo);
					sb.Append(strNtfInfo);
				}
			}

			return sb.ToString();
		}

		/// <summary>
		/// Метод формирует заголовок раздела Транспорт
		/// </summary>
		/// <returns>Возвращает строку описание для раздела Транспорт</returns>
		protected string TransportGetTitle()
		{
			V4Control[][] ctrls = { new V4Control[] { Driver, Car, CarNumber, TrailerNumber }
									, new V4Control[] { PowerOfAttorney, CarTtn }};

			return CreateTitleMultiLineDescription(ctrls);
		}

		/// <summary>
		/// Метод формирует дополнительную часть заголовка раздела Поставщик
		/// </summary>
		/// <param name="innerHtml">Уже сформированная часть заголовка раздела</param>
		/// <returns>Возвращает строку полное описание для раздела Поставщик</returns>
		public string Shipper_PostGetTitle(StringBuilder innerHtml)
		{
			V4Control[] ctrls2 = { Director, DirectorPosition, Accountant, AccountantPosition, StoreKeeper, StoreKeeperPosition };

			StringBuilder sbDescription2 = new StringBuilder();

			for (int i = 0; i < ctrls2.Length - 1; i += 2)
			{
				string textOfControl1 = GetCtrlText(ctrls2[i], true);
				string textOfControl2 = GetCtrlText(ctrls2[i + 1], true);

				if (textOfControl1.Length < 1 && textOfControl2.Length < 1) continue;

				if (sbDescription2.Length > 0) sbDescription2.Append(", ");

				sbDescription2.Append(textOfControl2);

				if (textOfControl1.Length > 0 && textOfControl2.Length > 0)
					sbDescription2.Append(" ");

				sbDescription2.Append(textOfControl1);
			}

			if (sbDescription2.Length > 0)
			{
				if (innerHtml.Length > 0) innerHtml.Append("<br/>");

				innerHtml.Append(SectionTitles.SignersOfPaper + ": ");
			}

			innerHtml.Append(sbDescription2);

			return innerHtml.ToString();
		}

		/// <summary>
		/// Метод формирует заголовок раздела Данные накладной
		/// </summary>
		/// <returns>Возвращает строку описание для раздела Данные накладной</returns>
		string GeneralDataGetTitle()
		{
			V4Control[][] ctrls = { new V4Control[] { DateOfPosting, Currency, Notes } };

			return CreateTitleMultiLineDescription(ctrls);
		}

		/// <summary>
		/// Метод формирует заголовок раздела Документы
		/// </summary>
		/// <returns>Возвращает строку описание для раздела Документы</returns>
		protected string DocumentsGetTitle()
		{
			V4Control[][] ctrls = { new V4Control[] { Contract }
									, new V4Control[] { Enclosure }
									, new V4Control[] { ApplicationForPurchasing }
									, new V4Control[] { LetterOfCredit }
									, new V4Control[] { BillOfLading }
									, new V4Control[] { Invoice }
									, new V4Control[] { PaymentDocuments } };

			return CreateTitleMultiLineDescription(ctrls);
		}

		/// <summary>
		/// Метод устанавливает поля ГО и ГП из последней накладной с такими же поставщиком, плательщиком и договором, если она существует
		/// </summary>
		private void TryFindGOGP()
		{
			if (string.IsNullOrEmpty(Shipper.Value)) return;
			if (string.IsNullOrEmpty(Payer.Value)) return;
			if (string.IsNullOrEmpty(Contract.Value)) return;

			var sqlParams = new Dictionary<string, object> { { "@КодТипаДокумента", DocTypeEnum.ТоварноТранспортнаяНакладная }
																, { "@КодЛица1", Shipper.Value }
																, { "@КодЛица2", Payer.Value }
																, { "@КодДокументаОснования", Contract.Value }
			};

			DataTable dt = DBManager.GetData(SQLQueries.SELECT_ПоследнийДокументПоТипу, Config.DS_document, CommandType.Text, sqlParams);
			if (null == dt) return;

			if (dt.Rows.Count > 0)
			{
				object odb = dt.Rows[0]["КодЛица3"];
				if (odb != DBNull.Value)
					_goPanel.SetPerson((int)odb);

				odb = dt.Rows[0]["КодЛица4"];
				if (odb != DBNull.Value)
					_gpPanel.SetPerson((int)odb);
			}
		}

		/// <summary>
		/// Метод устанавливает значения полей Дата, Валюта, Поставщик и Плательщик в соответствии с полями объекта типа Документ
		/// Метод вызывается первым, затем следуют медоты, которые конкретизируют тип документа и пытаются извлечь оттуда больше данных
		/// </summary>
		/// <param name="d">Договор</param>
		bool FillByDocument(Document d)
		{
			if (Doc.Date == default(DateTime))
				Doc.Date = d.Date;

			if (string.IsNullOrEmpty(DateOfPosting.Value))
			{
				//DateOfPosting.ValueDate = d.Date;
				DateOfPosting.BindDocField.Value = d.Date;
			}

			if (string.IsNullOrEmpty(Currency.Value) && null != d.Currency)
			{
				Currency.Value = d.Currency.Id;
				Currency.BindDocField.Value = d.Currency.Id;
			}

			if (d.DataUnavailable) return false;

			bool fPayer = string.IsNullOrEmpty(Payer.Value);

			bool fChanged = false;
			if (string.IsNullOrEmpty(Shipper.Value))
			{
				if (fPayer || Payer.ValueInt == d.DocumentData.PersonId2)
				{
					fChanged = Shipper.ValueInt != d.DocumentData.PersonId1;
					Shipper.ValueInt = d.DocumentData.PersonId1;
				}
			}

			if (fPayer)
			{
				if (Shipper.ValueInt == d.DocumentData.PersonId1)
				{
					fChanged = Payer.ValueInt != d.DocumentData.PersonId2;
					Payer.ValueInt = d.DocumentData.PersonId2;
				}
			}

			return fChanged;
		}

		/// <summary>
		/// Метод устанавливает значения полей Поставщик и Плательщик в соответствии с полями документа типа Договор
		/// </summary>
		/// <param name="d">Договор</param>
		bool FillByDogovor(Kesco.Lib.Entities.Documents.EF.Dogovora.Dogovor d)
		{
			bool fChanged = false;
			bool fPayer = string.IsNullOrEmpty(PayerStore.Value);

			if (string.IsNullOrEmpty(ShipperStore.Value))
			{
				if (fPayer || PayerStore.Value == d.Sklad2Field.Id)
				{
					fChanged = ShipperStore.Value != d.Sklad1Field.Id;
					ShipperStore.Value = d.Sklad1Field.Id;
				}
			}

			if (fPayer)
			{
				if (ShipperStore.Value == d.Sklad1Field.Id)
				{
					fChanged = PayerStore.Value != d.Sklad2Field.Id;
					PayerStore.Value = d.Sklad2Field.Id;
				}
			}

			return fChanged;
		}

		/// <summary>
		/// Метод устанавливает значения полей в соответствии с полями корректируемой ТТН
		/// </summary>
		/// <param name="ttn">Корректируемая ТТН</param>
		bool FillByCorrectableTtn(Kesco.Lib.Entities.Documents.EF.Trade.TTN ttn)
		{
			Kesco.Lib.Entities.Documents.EF.Trade.TTN cloned = (Kesco.Lib.Entities.Documents.EF.Trade.TTN)ttn.Clone();

			cloned.CorrectingDocField.Value = ttn.DocId;
			cloned.CorrectingFlagField.Value = "1";
			cloned.Date = Doc.Date;

			Doc = cloned;

			SetControlProperties();
			DocumentToControls();
			return true;
		}

		/// <summary>
		/// Метод устанавливает заголовок раздела с указанным идентификатором HTML элемента
		/// </summary>
		/// <param name="panelId">Идентификатор HTML элемента заголовка</param>
		void SetAccordionHeader(string panelId)
		{
			try
			{
				ClientScripts.SendSetInnerHtml(this, panelId, AccordionTitles[panelId]());
			}
			catch (ArgumentNullException)
			{
			}
			catch (KeyNotFoundException)
			{
			}
		}

		/// <summary>
		/// Метод обновляет уведомления у элементов управления не входящих в панели Грузоотправитель, Грузополучатель, Поставщик, Плательщик
		/// </summary>
		void RenderNonPanelControlsNtf()
		{
			V4Control[] ctrls = new V4Control[] {
				//Данные накладной
				DateOfPosting,
				Currency,
				Notes,
				Director,
				DirectorPosition,
				Accountant,
				AccountantPosition,
				StoreKeeper,
				StoreKeeperPosition,

				//Документы
				ContractInfo,
				Contract,
				Enclosure,
				ApplicationForPurchasing,
				LetterOfCredit,
				BillOfLading,
				Invoice,
				PaymentDocuments,

				//Транспорт
				PowerOfAttorney,
				Driver,
				Car,
				CarNumber,
				TrailerNumber,
				CarTtn
			};

			foreach (V4Control c in ctrls)
				c.RenderNtf();
		}

		/// <summary>
		/// Универсальный обработчик события выбора документа,
		/// который устанавливает значения в полях формы связанных документов - оснований
		/// </summary>
		/// <param name="ctrl">Элемент управления Документ</param>
		/// <param name="link">Элемент управления Документ - основание для ctrl</param>
		void LinkedDoc_Changed1(DBSDocument ctrl, DBSDocument link)
		{
			if (link.SelectedItems.Count < 1)
			{
				StringBuilder sb = new StringBuilder();

				foreach (Kesco.Lib.Entities.Item i in ctrl.SelectedItems)
				{
					if (sb.Length > 0) sb.Append(",");
					sb.Append(i.Id);
				}

				var sqlParams = new Dictionary<string, object> { { "@Коды", sb.ToString() } };

				DataTable dt = DBManager.GetData(SQLQueries.SELECT_СвязиДокументовОснованияДляВытекающих, Config.DS_document, CommandType.Text, sqlParams);
				if (null != dt)
				{
					link.SelectedItems.AddRange(dt.AsEnumerable().Select((row) => { string id = ((int)row["КодДокумента"]).ToString(); return new Kesco.Lib.Entities.Item() { Id = id, Value = link.GetObjectById(id) }; }));

					//Инициализация перерисовки элемента управления
					link.SetPropertyChanged("IsReadOnly");
					link.Flush();
				}
			}
		}

		/// <summary>
		/// Универсальный обработчик события выбора документа,
		/// который устанавливает значения в полях формы связанных вытекающих документов
		/// </summary>
		/// <param name="ctrl">Элемент управления Документ</param>
		/// <param name="link">Элемент управления Документ - вытекающий из ctrl</param>
		void LinkedDoc_Changed2(DBSDocument ctrl, DBSDocument link)
		{
			if (link.SelectedItems.Count < 1)
			{
				//LinkedDoc_BeforeSearch(link, ctrl);
				//link.TryFindSingleValue();
				//Фильтр отключаем, что бы он применялся при обычном выборе записей
				//link.Filter.LinkedDoc.LinkedDocParams.Clear();

				StringBuilder sb = new StringBuilder();

				foreach (Kesco.Lib.Entities.Item i in ctrl.SelectedItems)
				{
					if (sb.Length > 0) sb.Append(",");
					sb.Append(i.Id);
				}

				var sqlParams = new Dictionary<string, object> { { "@Коды", sb.ToString() } };

				DataTable dt = DBManager.GetData(SQLQueries.SELECT_СвязиДокументовВытекающиеИзОснований, Config.DS_document, CommandType.Text, sqlParams);
				if (null != dt)
				{
					/*
					foreach (DataRow row in dt.Rows)
					{
						string id = ((int)row["КодДокумента"]).ToString();
						object obj = ctrl.GetObjectById(id);
						if (obj != null)
							link.SelectedItems.Add(new Kesco.Lib.Entities.Item { Id = id, Value = obj });
					}
					*/
					link.SelectedItems.AddRange(dt.AsEnumerable().Select((row) => { string id = ((int)row["КодДокумента"]).ToString(); return new Kesco.Lib.Entities.Item() { Id = id, Value = link.GetObjectById(id) }; }));

					//Инициализация перерисовки элемента управления
					link.SetPropertyChanged("IsReadOnly");
					link.Flush();
				}
			}
		}

		/// <summary>
		/// Универсальный обработчик события установки фильтра поиска документа,
		/// который устанавливает фильтр поиска с учетом связанных документов
		/// </summary>
		/// <param name="ctrl">Элемент управления Документ</param>
		/// <param name="link">Элемент управления Документ - связанный с ctrl</param>
		void LinkedDoc_BeforeSearch(DBSDocument ctrl, DBSDocument link)
		{
			ctrl.Filter.LinkedDoc.LinkedDocParams.Clear();

			if (link.IsMultiSelect)
			{
				foreach (Kesco.Lib.Entities.Item i in link.SelectedItems)
				{
					LinkedDocParam lp1 = new LinkedDocParam();
					lp1.QueryType = LinkedDocsType.DirectReasons;
					lp1.DocID = i.Id;

					LinkedDocParam lp2 = new LinkedDocParam();
					lp2.QueryType = LinkedDocsType.DirectСonsequences;
					lp2.DocID = i.Id;

					ctrl.Filter.LinkedDoc.LinkedDocParams.Add(lp1);
					ctrl.Filter.LinkedDoc.LinkedDocParams.Add(lp2);
				}

				return;
			}

			if (string.IsNullOrEmpty(link.Value)) return;

			LinkedDocParam lp3 = new LinkedDocParam();
			lp3.QueryType = LinkedDocsType.DirectReasons;
			lp3.DocID = link.Value;

			LinkedDocParam lp4 = new LinkedDocParam();
			lp4.QueryType = LinkedDocsType.DirectСonsequences;
			lp4.DocID = link.Value;

			ctrl.Filter.LinkedDoc.LinkedDocParams.Add(lp3);
			ctrl.Filter.LinkedDoc.LinkedDocParams.Add(lp4);
		}

		/// <summary>
		/// Универсальный обработчик события установки фильтра поиска лица,
		/// который устанавливает фильтр поиска с учетом значений в других полях формы
		/// </summary>
		/// <param name="sender">Элемент управления Лицо для которого осуществляется поиск</param>
		public void Person_BeforeSearch(DBSPerson sender)
		{
			sender.Filter.PersonValidAt = Doc.Date;
		}

		/// <summary>
		/// Универсальный обработчик события установки фильтра поиска адреса лица,
		/// который устанавливает фильтр поиска с учетом значений в других полях формы
		/// </summary>
		/// <param name="sender">Элемент управления Адрес лица для которого осуществляется поиск</param>
		/// /// <param name="personId">Код лица для которого выбирается адрес</param>
		public void Address_BeforeSearch(DBSPersonContact sender, string personId)
		{
			sender.GetFilter().ContactTypeId.Value = "1";
			sender.GetFilter().PersonId.Value = personId;
			sender.GetFilter().ValidAt.Value = Doc.Date.ToString("yyyyMMdd");
		}

		/// <summary>
		/// Универсальный обработчик события установки фильтра поиска расчетного счета лица,
		/// который устанавливает фильтр поиска с учетом значений в других полях формы
		/// </summary>
		/// <param name="sender">Элемент управления Склад лица для которого осуществляется поиск</param>
		/// /// <param name="personId">Код лица для которого выбирается счет</param>
		public void Store_BeforeSearch(DBSStore sender, string personId)
		{
			// Расчетный счёт (руб.)
			// Валютный счёт (в России неиспользуется)
			// Валютный транзитный счёт
			sender.GetFilter().StoreTypeId.Value = "1,2,3";
			sender.GetFilter().ManagerId.Value = personId;
			sender.GetFilter().StoreResourceId.Value = Currency.Value;
			sender.GetFilter().ValidAt.Value = Doc.Date.ToString("yyyyMMdd");
		}

		/// <summary>
		/// Универсальный обработчик события запроса уведомительного сообщения для лица
		/// </summary>
		/// <param name="ntf">Элемент отображающий уведомительное сообщение</param>
		/// <param name="personCtrl">Объект для которого формируется уведомительное сообщение</param>
		public void Person_OnRenderNtf(Ntf ntf, DBSPerson personCtrl)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(personCtrl.Value)) return;

			var sqlParams = new Dictionary<string, object> { { "@Дата", Doc.Date }
																, { "@КодЛица", personCtrl.Value } };

			object retObj = DBManager.ExecuteScalar(SQLQueries.SELECT_TEST_ЛицоДействует, CommandType.Text, Config.DS_person, sqlParams);
			if (retObj != DBNull.Value && (int)retObj == 0)
			{
				ntf.Add(ControlNtfs.PersonDate, NtfStatus.Error);
			}
		}

		/// <summary>
		/// Универсальный обработчик события запроса уведомительного сообщения для адреса лица
		/// </summary>
		/// <param name="ntf">Элемент отображающий уведомительное сообщение</param>
		/// <param name="personCtrl">Объект Лицо для которого формируется уведомительное сообщение</param>
		/// <param name="addrCtrl">Объект Адрес лица для которого формируется уведомительное сообщение</param>
		/// <param name="prefix">Строка конкретизирующая лицо Грузоотправитель, Грузополучатель, Поставщик или Плательщик</param>
		public void Address_OnRenderNtf(Ntf ntf, DBSPerson personCtrl, DBSPersonContact addrCtrl, string prefix)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(addrCtrl.Value)) return;

			var sqlParams = new Dictionary<string, object> { { "@Дата", Doc.Date }
																, { "@КодКонтакта", addrCtrl.Value } };

			object retObj = DBManager.ExecuteScalar(SQLQueries.SELECT_TEST_КонтактДействует, CommandType.Text, Config.DS_person, sqlParams);
			if (retObj != DBNull.Value && (int)retObj == 0)
			{
				ntf.Add(ControlNtfs.AddressDate, NtfStatus.Error);
			}

			if (!string.IsNullOrEmpty(personCtrl.Value)) return;

			if (personCtrl.ValueInt.HasValue)
			{
				sqlParams = new Dictionary<string, object> { { "@КодЛица", personCtrl.ValueInt }
																, { "@КодКонтакта", addrCtrl.Value } };

				retObj = DBManager.ExecuteScalar(SQLQueries.SELECT_TEST_КонтактЛица, CommandType.Text, Config.DS_person, sqlParams);
				if (retObj != DBNull.Value && (int)retObj == 0)
				{
					ntf.Add(ControlNtfs.PersonAddress[prefix], NtfStatus.Error);
				}
			}
		}

		/// <summary>
		/// Универсальный обработчик события запроса уведомительного сообщения для счета лица
		/// </summary>
		/// <param name="ntf">Элемент отображающий уведомительное сообщение</param>
		/// <param name="personCtrl">Объект Лицо для которого формируется уведомительное сообщение</param>
		/// <param name="addrCtrl">Объект Склад лица для которого формируется уведомительное сообщение</param>
		/// <param name="prefix">Строка конкретизирующая лицо Грузоотправитель, Грузополучатель, Поставщик или Плательщик</param>
		public void Store_OnRenderNtf(Ntf ntf, DBSPerson personCtrl, Store store, string prefix)
		{
			ntf.Clear();

			if (null == store || store.Unavailable) return;

			if (personCtrl.ValueInt.HasValue && store.ManagerCode != personCtrl.ValueInt)
				ntf.Add(ControlNtfs.PersonStore[prefix], NtfStatus.Error);

			if (Currency.ValueInt.HasValue && store.ResourceId != Currency.ValueInt)
				ntf.Add(ControlNtfs.CurrencyDocument, NtfStatus.Error);

			if (!store.IsValidAt(Doc.Date))
				ntf.Add(ControlNtfs.StoreAccountDate, NtfStatus.Error);
		}

		//--------------------------------------------------
		// Далее следуют стандартные обработчики событий V4,
		// реализации абстрактных методов
		// и переопределения методов базовых классов
		//--------------------------------------------------

		protected void CorrectableFlag_Changed(object sender, ProperyChangedEventArgs e)
		{
			if (CorrectableFlag.Value == "1")
			{
				ClientScripts.setCorrectableDocument(this);
			}
			else
			{
				CorrectableTtn.IsDisabled = true;
				ClientScripts.SetCorrectableState(this, false);
			}
		}

		protected void CorrectableTtn_Changed(object sender, ProperyChangedEventArgs e)
		{
			Kesco.Lib.Entities.Documents.EF.Trade.TTN ttn = new Kesco.Lib.Entities.Documents.EF.Trade.TTN(CorrectableTtn.Value);
			FillByCorrectableTtn(ttn);

			RenderNonPanelControlsNtf();

			_goPanel.Person_Changed(null,null);
			_gpPanel.Person_Changed(null, null);
			_shipperPanel.Person_Changed(null, null);
			_payerPanel.Person_Changed(null, null);

			SetAccortdionTitles();
		}

		protected void Currency_Changed(object sender, ProperyChangedEventArgs e)
		{
			_goPanel.OnCurrencyChanged();
			_gpPanel.OnCurrencyChanged();
			_shipperPanel.OnCurrencyChanged();
			_payerPanel.OnCurrencyChanged();
		}

		protected void DateOfPosting_OnRenderNtf(object sender, Ntf ntf)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(DateOfPosting.Value)) return;
			if (!DateOfPosting.ValueDate.HasValue) return;
			if (Doc.Date == default(DateTime)) return;

			if (DateOfPosting.ValueDate < Doc.Date.Date)
				ntf.Add(ControlNtfs.PostingOnDate, NtfStatus.Error);
		}

		protected void ShipperOrPayer_Changed(object sender, ProperyChangedEventArgs e)
		{
			DBSDocument[] docCtrls = { CarTtn, Contract, Enclosure, ApplicationForPurchasing, LetterOfCredit, BillOfLading, Invoice, PaymentDocuments };
			foreach (DBSDocument docCtrl in docCtrls)
			{
				docCtrl.RenderNtf();
			}

			SetAccordionHeader(SectionPrefixes.Documents + Nakladnaya.suffixTitle);
		}

		protected void StoreKeeper_Changed(object sender, ProperyChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(StoreKeeperPosition.Value))
				StoreKeeperPosition.TryFindSingleValue();
			StoreKeeperPosition.RenderNtf();
		}

		protected void Accountant_Changed(object sender, ProperyChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(AccountantPosition.Value))
				AccountantPosition.TryFindSingleValue();

			AccountantPosition.RenderNtf();
		}

		protected void Director_Changed(object sender, ProperyChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(DirectorPosition.Value))
				DirectorPosition.TryFindSingleValue();

			DirectorPosition.RenderNtf();
		}

		protected void StoreKeeperPosition_OnRenderNtf(object sender, Ntf ntf)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(StoreKeeper.Value)) return;
			if (string.IsNullOrEmpty(StoreKeeperPosition.Value)) return;

			if (StoreKeeper.Value != StoreKeeperPosition.Filter.PcId.Value)
				ntf.Add(ControlNtfs.PersonPosition, NtfStatus.Error);
		}

		protected void AccountantPosition_OnRenderNtf(object sender, Ntf ntf)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(Accountant.Value)) return;
			if (string.IsNullOrEmpty(AccountantPosition.Value)) return;

			if (Accountant.Value != AccountantPosition.Filter.PcId.Value)
				ntf.Add(ControlNtfs.PersonPosition, NtfStatus.Error);
		}

		protected void DirectorPosition_OnRenderNtf(object sender, Ntf ntf)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(Director.Value)) return;
			if (string.IsNullOrEmpty(DirectorPosition.Value)) return;

			if (Director.Value != DirectorPosition.Filter.PcId.Value)
				ntf.Add(ControlNtfs.PersonPosition, NtfStatus.Error);
		}

		protected void Director_BeforeSearch(object sender)
		{
			//Фильтр по должности
			//Director.Filter.PcId.Value = Director.Value;

			Director.Filter.PersonType = 2;//Физические лица
		}

		protected void Accountant_BeforeSearch(object sender)
		{
			//Фильтр по должности
			//Accountant.Filter.PcId.Value = Accountant.Value;
			Accountant.Filter.PersonType = 2;//Физические лица
		}

		protected void StoreKeeper_BeforeSearch(object sender)
		{
			//Фильтр по должности
			//StoreKeeper.Filter.PcId.Value = StoreKeeper.Value;
			StoreKeeper.Filter.PersonType = 2;//Физические лица
		}

		protected void DirectorPosition_BeforeSearch(object sender)
		{
			DirectorPosition.Filter.PcId.CompanyHowSearch = "0";
			DirectorPosition.Filter.PcId.Value = Director.Value;
		}

		protected void AccountantPosition_BeforeSearch(object sender)
		{
			AccountantPosition.Filter.PcId.CompanyHowSearch = "0";
			AccountantPosition.Filter.PcId.Value = Accountant.Value;
		}

		protected void StoreKeeperPosition_BeforeSearch(object sender)
		{
			StoreKeeperPosition.Filter.PcId.CompanyHowSearch = "0";
			StoreKeeperPosition.Filter.PcId.Value = StoreKeeper.Value;
		}

		protected void PaymentDocuments_OnRenderNtf(object sender, Ntf ntf)
		{
			if (Invoice.SelectedItems.Count < 1) return;
			if (PaymentDocuments.SelectedItems.Count < 1) return;

			StringBuilder sb_invoces = new StringBuilder();

			foreach (Kesco.Lib.Entities.Item i in Invoice.SelectedItems)
			{
				if (sb_invoces.Length > 0) sb_invoces.Append(",");
				sb_invoces.Append(i.Id);
			}

			StringBuilder sb_payments = new StringBuilder();

			foreach (Kesco.Lib.Entities.Item i in PaymentDocuments.SelectedItems)
			{
				if (sb_payments.Length > 0) sb_payments.Append(",");
				sb_payments.Append(i.Id);
			}

			var sqlParams = new Dictionary<string, object> { { "@КодыОснований", sb_invoces.ToString() }
																, { "@Коды", sb_payments.ToString()} };

			object retObj = DBManager.ExecuteScalar(SQLQueries.SELECT_TEST_СвязиДокументовВытекающие, CommandType.Text, Config.DS_document, sqlParams);
			if (retObj != DBNull.Value && (int)retObj == 0)
			{
				ntf.Add(ControlNtfs.PaymentInvoice, NtfStatus.Error);
			}
		}

		protected void Enclosure_Changed(object sender, ProperyChangedEventArgs e)
		{
			if (string.IsNullOrEmpty(Contract.Value))
			{
				LinkedDoc_BeforeSearch(Contract, Enclosure);
				if (Contract.TryFindSingleValue())
					Contract_Changed(null, null);
			}
		}

		protected void Enclosure_OnRenderNtf(object sender, Ntf ntf)
		{
			if (string.IsNullOrEmpty(Contract.Value)) return;
			if (Enclosure.SelectedItems.Count < 1) return;

			StringBuilder sb = new StringBuilder();

			foreach (Kesco.Lib.Entities.Item i in Enclosure.SelectedItems)
			{
				if (sb.Length > 0) sb.Append(",");
				sb.Append(i.Id);
			}

			var sqlParams = new Dictionary<string, object> { { "@КодыОснований", Contract.Value }
																, { "@Коды", sb.ToString()} };

			object retObj = DBManager.ExecuteScalar(SQLQueries.SELECT_TEST_СвязиДокументовВытекающие, CommandType.Text, Config.DS_document, sqlParams);
			if (retObj != DBNull.Value && (int)retObj == 0)
			{
				ntf.Add(ControlNtfs.EnclosureContract, NtfStatus.Error);
			}
		}

		protected void Document_OnRenderNtf(object sender, Ntf ntf)
		{
			DBSDocument dbsdoc = sender as DBSDocument;
			if (null == dbsdoc) return;

			ntf.Clear();

			if (string.IsNullOrEmpty(dbsdoc.Value)) return;
			if (Doc.Date == default(DateTime)) return;

			Kesco.Lib.Entities.Documents.Document d = new Kesco.Lib.Entities.Documents.Document(dbsdoc.Value);

			if (d.DataUnavailable)
				ntf.Add(ControlNtfs.DocumentForm, NtfStatus.Error);

			if (d.Date == default(DateTime)) return;

			if (d.Date > Doc.Date)
				ntf.Add(ControlNtfs.BaseDocumentOnDate, NtfStatus.Error);

			if (dbsdoc.Filter.PersonIDs.Count>0)
			{
				bool fPersonDocument = true;
				int count = 0;

				if (!string.IsNullOrEmpty(Shipper.Value))
				{
					++count;
					fPersonDocument = dbsdoc.Filter.PersonIDs.Value.Contains(Shipper.Value);
				}

				if (fPersonDocument && !string.IsNullOrEmpty(Payer.Value))
				{
					++count;
					fPersonDocument = dbsdoc.Filter.PersonIDs.Value.Contains(Payer.Value);
				}

				fPersonDocument = fPersonDocument && dbsdoc.Filter.PersonIDs.Count == count;

				if (!fPersonDocument)
					ntf.Add(ControlNtfs.PersonDocument, NtfStatus.Error);
			}
		}

		protected void Document_BeforeSearch(object sender)
		{
			DBSDocument dbsdoc = sender as DBSDocument;
			if (null == dbsdoc) return;

			//фильтр по дате документа
			dbsdoc.Filter.Date.Value = Doc.Date.ToString("yyyyMMdd");
			dbsdoc.Filter.Date.DateSearchType = DateSearchType.LessThan;

			//фильтр по лицам документа
			dbsdoc.Filter.PersonIDs.Clear();
			if(!string.IsNullOrEmpty(Shipper.Value))
				dbsdoc.Filter.PersonIDs.Add(Shipper.Value);

			if (!string.IsNullOrEmpty(Payer.Value))
				dbsdoc.Filter.PersonIDs.Add(Payer.Value);
		}

		protected void Contract_Changed(object sender, ProperyChangedEventArgs e)
		{
			string strDocId = Contract.Value;
			Kesco.Lib.Entities.Documents.Document d = string.IsNullOrWhiteSpace(strDocId) ? null : new Kesco.Lib.Entities.Documents.Document(strDocId);

			bool fUpdated = false;

			if (d == null || string.IsNullOrWhiteSpace(d.FullDocName))
			{
				Hide("ContractInfoPanel");
				ContractInfo.Visible = false;
				//ContractInfo.Value = null;
				ContractInfo.BindDocField.Value = null;
			}
			else
			{
				ContractInfo.Visible = true;
				//ContractInfo.Value = d.FullDocName;
				ContractInfo.BindDocField.Value = d.FullDocName;
				Display("ContractInfoPanel");

				fUpdated = FillByDocument(d);
			}

			Kesco.Lib.Entities.Documents.EF.Dogovora.Dogovor contract = string.IsNullOrWhiteSpace(strDocId) ? null : new Kesco.Lib.Entities.Documents.EF.Dogovora.Dogovor(strDocId);

			if (contract == null || string.IsNullOrWhiteSpace(contract._Kurator))
			{
				Hide("CuratorPanel");
				Curator.Visible = false;
				Curator.Value = null;
			}
			else
			{
				Curator.Visible = true;
				Curator.Value = contract._Kurator;
				Display("CuratorPanel");

				if (FillByDogovor(contract))
					fUpdated = true;
			}

			if (fUpdated)
			{
				if (string.IsNullOrEmpty(PayerAddress.Value))
					PayerAddress.TryFindSingleValue();

				if (string.IsNullOrEmpty(ShipperAddress.Value))
					ShipperAddress.TryFindSingleValue();

				if (string.IsNullOrEmpty(PayerStore.Value))
					PayerStore.TryFindSingleValue();

				if (string.IsNullOrEmpty(ShipperStore.Value))
					ShipperStore.TryFindSingleValue();

				_shipperPanel.OnDocDateChanged();
				_payerPanel.OnDocDateChanged();

				TryFindGOGP();
			}

			if (Enclosure.SelectedItems.Count<1)
			{
				LinkedDoc_BeforeSearch(Enclosure, Contract);
				Enclosure.TryFindSingleValue();
			}

			Enclosure.RenderNtf();
		}

		protected override void DocumentInitialization(Document copy = null)
		{
			_ctrls = new V4Control[] {
				CorrectableFlag,
				CorrectableTtn,

				//Данные накладной
				DateOfPosting,
				Currency,
				Notes,
				Director,
				DirectorPosition,
				Accountant,
				AccountantPosition,
				StoreKeeper,
				StoreKeeperPosition,

				//Поставщик
				ShipperInfo,
				ShipperCodeInfo,
				Shipper,
				ShipperAddress,
				ShipperStore,
				ShipperStoreInfo,

				//Плательщик
				PayerInfo,
				PayerCodeInfo,
				Payer,
				PayerAddress,
				PayerStore,
				PayerStoreInfo,

				//Грузоотправитель
				GoInfo,
				GoCodeInfo,
				GO,
				GoAddress,
				DispatchPoint,
				GoStore,
				GoStoreInfo,
				GoNotes,

				//Грузополучатель
				GpInfo,
				GpCodeInfo,
				GP,
				GpAddress,
				DestinationPoint,
				GpStore,
				GpStoreInfo,
				GpNotes,

				//Документы
				ContractInfo,
				Contract,
				Enclosure,
				ApplicationForPurchasing,
				LetterOfCredit,
				BillOfLading,
				Invoice,
				PaymentDocuments,

				//Транспорт
				PowerOfAttorney,
				Driver,
				Car,
				CarNumber,
				TrailerNumber,
				CarTtn
			};

			if (copy == null)
			{
				Doc = new Kesco.Lib.Entities.Documents.EF.Trade.TTN();
				Doc.Date = DateTime.Now.Date;
			}
			else
			{
				Doc = (Kesco.Lib.Entities.Documents.EF.Trade.TTN)copy;
			}
		}

		protected override void DocumentToControls()
		{
			Kesco.Lib.Entities.Documents.EF.Trade.TTN d = Doc as Kesco.Lib.Entities.Documents.EF.Trade.TTN;

			DocField[] fields = GetDocFields(d);

			Debug.Assert(fields.Length==_ctrls.Length);

			for (int i = 0; i < _ctrls.Length; ++i)
			{
				if (null == fields[i]) continue;
				if (null == fields[i].Value)
					_ctrls[i].Value = string.Empty;
				else
				{
					if ("DateTime" == fields[i].Value.GetType().Name)
					{
						//DatePicker работает только с русскими строковыми представлениями дат, обходим это ограничение
						//_ctrls[i].Value = ((DateTime)fields[i].Value).ToString(CultureInfo.GetCultureInfo("ru-RU"));
						DatePicker dp = _ctrls[i] as DatePicker;
						dp.ValueDate = fields[i].Value as DateTime?;
					}
					else
					{
						_ctrls[i].Value = fields[i].Value.ToString();//(int)(DateTime)(string)
					}
				}
			}

			//Устанавливаются отдельно
			//GoAddress.IsReadOnly = !DocEditable;
			//GoAddress.IsReadOnly = !DocEditable;
		}

		protected override void SetControlProperties()
		{
			Kesco.Lib.Entities.Documents.EF.Trade.TTN d = Doc as Kesco.Lib.Entities.Documents.EF.Trade.TTN;

			DocField[] fields = GetDocFields(d);

			Debug.Assert(fields.Length == _ctrls.Length);

			for (int i = 0; i < _ctrls.Length; ++i)
			{
				if (null == fields[i]) continue;

				_ctrls[i].BindDocField = fields[i];
				_ctrls[i].IsRequired = fields[i].IsMandatory;

				if (!_ctrls[i].IsReadOnly)
					_ctrls[i].IsReadOnly = !DocEditable;
			}

			GoAddress.IsReadOnly = !DocEditable;
			GoAddress.IsReadOnly = !DocEditable;
		}

		protected override void OnDocDateChanged(object sender, ProperyChangedEventArgs e)
		{
			base.OnDocDateChanged(sender, e);

			CorrectableTtn.RenderNtf();
			DateOfPosting.RenderNtf();
			CarTtn.RenderNtf();

			SetAccordionHeader(SectionPrefixes.General + Nakladnaya.suffixTitle);
			SetAccordionHeader(SectionPrefixes.Documents + Nakladnaya.suffixTitle);
			SetAccordionHeader(SectionPrefixes.Transport + Nakladnaya.suffixTitle);

			_goPanel.OnDocDateChanged();
			_gpPanel.OnDocDateChanged();
			_shipperPanel.OnDocDateChanged();
			_payerPanel.OnDocDateChanged();
		}
	}
}