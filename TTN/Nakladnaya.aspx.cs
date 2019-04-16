using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Runtime.CompilerServices;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Corporate;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Documents.EF.Dogovora;
using Kesco.Lib.Entities.Documents.EF.Trade;
using Kesco.Lib.Entities.Persons.PersonOld;
using Kesco.Lib.Entities.Resources;
using Kesco.Lib.Entities.Stores;
using Kesco.Lib.Web.Settings;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.DBSelect.V4;
using Kesco.Lib.Web.Controls.V4.Common;

//TODO DBSStore расширенный поиск доделать

namespace Kesco.App.Web.Docs.TTN
{
    public class V4PageCard
    {
        public string PersonId { get; set; }
        public DateTime Date { get; set; }
        public PersonOld.Card Object { get; set; }
    }

    /// <summary>
    /// Класс объекта страницы
    /// </summary>
    public partial class Nakladnaya : DocPage
    {
        #region declaration, property, field
        //Словарь содержит метки полей формы
        public static Dictionary<V4Control, string> FieldLabels = 
            new Dictionary<V4Control, string>(new Nakladnaya.ReferenceEqualityComparer());

        /// <summary>
        /// Словарь с параметрами строки запроса
        /// </summary>
        public Dictionary<string, string> _qsParams = new Dictionary<string, string>();

        public override bool DocEditable
        {
            get { return base.DocEditable && !Document.IsCorrected; }
        }

        public Kesco.Lib.Entities.Documents.EF.Trade.TTN Document
        {
            get { return Doc as Kesco.Lib.Entities.Documents.EF.Trade.TTN; }
        }

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

        private TextBox GoAddress = new TextBox();
        private TextBox GpAddress = new TextBox();

        // итоговые значения табличных полей
        public decimal[,] ItogArray = new decimal[3, 3];

        public DBSPerson ShipperField
        {
            get { return Shipper; }
        }

        public DBSPerson PayerField
        {
            get { return Payer; }
        }

        /// <summary>
        ///     Задание ссылки на справку
        /// </summary>
        public override string HelpUrl { get; set; }

        public override int LikeId { get; set; }

        public DBSStore ShipperStoreField
        {
            get { return DBSShipperStore; }
        }

        public DBSStore PayerStoreField
        {
            get { return DBSPayerStore; }
        }

        #region Resx

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
        public class SectionTitlesClass
        {
            public string General = "Данные накладной";
            public string SignersOfPaper = "Подписанты печатных форм";
            public string Documents = "Документы-основания";
            public string Curator = "Куратор";
            public string GO = "Грузоотправитель";
            public string GP = "Грузополучатель";
            public string Shipper = "Поставщик";
            public string Payer = "Плательщик";
            public string ShipperToGo = "Совпадает с поставщиком:";
            public string PayerToGp = "Совпадает с плательщиком:";
            public string GoToShipper = "Совпадает с грузоотправителем:";
            public string GpToPayer = "Совпадает с грузополучателем:";
            public string Resource = "Товары";
            public string Services = "Услуги";
            public string Transport = "Транспорт";
            public string RequiredInfo = "<span class='required_info'>Отсутствует</span>";
            public string RequiredRedInfo = "<span class='required_red_info'>Отсутствует</span>";
        }

        //Строки для уведомлений полей формы
        public class ControlNtfsClass
        {
            public NameValueCollection PersonAddress = new NameValueCollection()
			{
				{SectionPrefixes.Go, "адрес не соответствует грузоотправителю"},
				{SectionPrefixes.Gp, "адрес не соответствует грузополучателю"},
				{SectionPrefixes.Shipper, "адрес не соответствует поставщику"},
				{SectionPrefixes.Payer, "адрес не соответствует плательщику"}
			};

            public NameValueCollection PersonInfo = new NameValueCollection()
			{
				{SectionPrefixes.Go, "реквизиты не соответствуют грузоотправителю"},
				{SectionPrefixes.Gp, "реквизиты не соответствуют грузополучателю"},
				{SectionPrefixes.Shipper, "название не соответствует поставщику"},
				{SectionPrefixes.Payer, "название не соответствует плательщику"}
			};

            public NameValueCollection PersonCode = new NameValueCollection()
			{
				{SectionPrefixes.Go, "ОКПО не соответствует грузоотправителю"},
				{SectionPrefixes.Gp, "ОКПО не соответствует грузополучателю"},
				{SectionPrefixes.Shipper, "ОКПО не соответствует поставщику"},
				{SectionPrefixes.Payer, "ОКПО не соответствует плательщику"}
			};

            public NameValueCollection PersonNoCode = new NameValueCollection()
			{
				{SectionPrefixes.Go, "у грузоотправителя отсутствует ОКПО"},
				{SectionPrefixes.Gp, "у грузополучателя отсутствует ОКПО"},
				{SectionPrefixes.Shipper, "у поставщика отсутствует ОКПО"},
				{SectionPrefixes.Payer, "у плательщика отсутствует ОКПО"}
			};

            public NameValueCollection PersonStore = new NameValueCollection()
			{
				{SectionPrefixes.Go, "расчетный счет не соответствует грузоотправителю"},
				{SectionPrefixes.Gp, "расчетный счет не соответствует грузополучателю"},
				{SectionPrefixes.Shipper, "расчетный счет не соответствует поставщику"},
				{SectionPrefixes.Payer, "расчетный счет не соответствует плательщику"}
			};

            public string PersonDate = "у лица отсутствует реквизиты на дату документа";
            public string InfoStore = "реквизиты не соответствуют расчетному счёту";
            public string AddressDate = "адрес не действует дату документа";
            public string StoreAccountDate = "расчетный счет не действует на дату документа";
            public string PersonPosition = "должность не соответствует лицу";
            public string PostingOnDate = "дата проводки должна быть больше или равна дате текущего документа";
            public string BaseDocumentOnDate = "дата основания должна быть меньше или равна дате текущего документа";
            public string PersonDocument = "лица документа не соответствуют поставщику и плательщику";
            public string CurrencyDocument = "валюта банковского счета не соответствует валюте документа";
            public string EnclosureContract = "приложения не связаны с документом";
            public string PaymentInvoice = "платежные документы не имеют оснований";
            //public const string StoreDocument = "расчетный счет не соответствует счету в договоре";
            //public const string StoreEnclosure = "расчетный счет не соответствует счету в приложении";
            //public const string BankDate = "у банка отсутствует реквизиты на дату";
            //public const string BankName = "у банка отсутствует краткое название";
            public string DocumentForm = "документ не имеет электронной формы";
            //public const string DogovorTtn = "срок действия договора не соответствует дате накладной";
            //public const string PersonOnDate = "валюта оплаты не соответствует валюте оплаты в приложении";
            //public const string PersonOnDate = "валюта оплаты не соответствует валюте оплаты в счете";
            //public const string PersonOnDate = "не указано приложение к договору";
            public string CurrencyContract = "валюта оплаты не соответствует валюте договора";
        }

        private SectionTitlesClass sectionTitles { get; set; }

        public SectionTitlesClass SectionTitles
        {
            get
            {
                if (sectionTitles != null)
                {
                    return sectionTitles;
                }

                sectionTitles = InitSectionTitles();
                return sectionTitles;
            }
        }

        private SectionTitlesClass InitSectionTitles()
        {
            var sectionTitles = new SectionTitlesClass
            {
                General = Resx.GetString("TTN_lblSectionGeneral"),
                SignersOfPaper = Resx.GetString("TTN_lblSectionSignersOfPaper"),
                Documents = Resx.GetString("TTN_lblSectionDocuments"),
                GO = Resx.GetString("TTN_lblSectionGO"),
                ShipperToGo = Resx.GetString("TTN_lblSectionShipperToGo"),
                GP = Resx.GetString("TTN_lblSectionGP"),
                PayerToGp = Resx.GetString("TTN_lblSectionPayerToGp"),
                Shipper = Resx.GetString("TTN_lblSectionShipper"),
                GoToShipper = Resx.GetString("TTN_lblSectionGoToShipper"),
                Payer = Resx.GetString("TTN_lblSectionPayer"),
                GpToPayer = Resx.GetString("TTN_lblSectionGpToPayer"),
                Resource = Resx.GetString("TTN_lblSectionResource"),
                Services = Resx.GetString("TTN_lblSectionServices"),
                Transport = Resx.GetString("TTN_lblSectionTransport"),
                RequiredInfo = "<span class='required_info'>" + Resx.GetString("TTN_lblSectionRequiredInfo") + "</span>"
            };
            return sectionTitles;
        }

        private ControlNtfsClass controlNtfs { get; set; }

        public ControlNtfsClass ControlNtfs
        {
            get
            {
                if (controlNtfs != null)
                {
                    return controlNtfs;
                }

                controlNtfs = InitControlNtfs();
                return controlNtfs;
            }
        }

        private ControlNtfsClass InitControlNtfs()
        {
            var controlNtfs = new ControlNtfsClass
            {
                PersonAddress = new NameValueCollection()
                {
                    {SectionPrefixes.Go, Resx.GetString("TTN_ntfPersonAddressGo")},
                    {SectionPrefixes.Gp, Resx.GetString("TTN_ntfPersonAddressGp")},
                    {SectionPrefixes.Shipper, Resx.GetString("TTN_ntfPersonAddressShipper")},
                    {SectionPrefixes.Payer, Resx.GetString("TTN_ntfPersonAddressPayer")}
                },
                PersonInfo = new NameValueCollection()
                {
                    {SectionPrefixes.Go, Resx.GetString("TTN_ntfPersonInfoGo")},
                    {SectionPrefixes.Gp, Resx.GetString("TTN_ntfPersonInfoGp")},
                    {SectionPrefixes.Shipper, Resx.GetString("TTN_ntfPersonInfoShipper")},
                    {SectionPrefixes.Payer, Resx.GetString("TTN_ntfPersonInfoPayer")}
                },
                PersonCode = new NameValueCollection()
                {
                    {SectionPrefixes.Go, Resx.GetString("TTN_ntfPersonCodeGo")},
                    {SectionPrefixes.Gp, Resx.GetString("TTN_ntfPersonCodeGp")},
                    {SectionPrefixes.Shipper, Resx.GetString("TTN_ntfPersonCodeShipper")},
                    {SectionPrefixes.Payer, Resx.GetString("TTN_ntfPersonCodePayer")}
                },
                PersonNoCode = new NameValueCollection()
                {
                    {SectionPrefixes.Go, Resx.GetString("TTN_ntfPersonNoCodeGo")},
                    {SectionPrefixes.Gp, Resx.GetString("TTN_ntfPersonNoCodeGp")},
                    {SectionPrefixes.Shipper, Resx.GetString("TTN_ntfPersonNoCodeShipper")},
                    {SectionPrefixes.Payer, Resx.GetString("TTN_ntfPersonNoCodePayer")}
                },
                PersonStore = new NameValueCollection()
                {
                    {SectionPrefixes.Go, Resx.GetString("TTN_ntfPersonStoreGo")},
                    {SectionPrefixes.Gp, Resx.GetString("TTN_ntfPersonStoreGp")},
                    {SectionPrefixes.Shipper, Resx.GetString("TTN_ntfPersonStoreShipper")},
                    {SectionPrefixes.Payer, Resx.GetString("TTN_ntfPersonStorePayer")}
                },
                PersonDate = Resx.GetString("TTN_ntfPersonDate"),
                InfoStore = Resx.GetString("TTN_ntfInfoStore"),
                AddressDate = Resx.GetString("TTN_ntfAddressDate"),
                StoreAccountDate = Resx.GetString("TTN_ntfStoreAccountDate"),
                PersonPosition = Resx.GetString("TTN_ntfPersonPosition"),
                PostingOnDate = Resx.GetString("TTN_ntfPostingOnDate"),
                BaseDocumentOnDate = Resx.GetString("TTN_ntfBaseDocumentOnDate"),
                PersonDocument = Resx.GetString("TTN_ntfPersonDocument"),
                CurrencyDocument = Resx.GetString("TTN_ntfCurrencyDocument"),
                EnclosureContract = Resx.GetString("TTN_ntfEnclosureContract"),
                PaymentInvoice = Resx.GetString("TTN_ntfPaymentInvoice"),
                DocumentForm = Resx.GetString("TTN_ntfDocumentForm"),
                CurrencyContract = Resx.GetString("_Msg_ДоговорВалюта")
            };

            return controlNtfs;
        }

        #endregion

        private List<V4PageCard> cardList { get; set; }
        private List<V4PageCard> CardList
        {
            get
            {
                if (cardList != null)
                {
                    return cardList;
                }

                cardList = new List<V4PageCard>();
                return cardList;
            }
        }

        #endregion

        /// <summary>
        ///  Конструктор по умолчанию
        /// </summary>
        public Nakladnaya()
        {
            HelpUrl = "hlp/help.htm?id=1";
            LikeId = 2;
            InterfaceVersion = "v1";
        }
        
        #region Initialization, binding

        /// <summary>
        ///     Событие инициализации страницы
        /// </summary>
        /// <param name="sender">Объект страницы</param>
        /// <param name="e">Аргументы</param>
        protected void Page_Init(object sender, EventArgs e)
        {
            NumberRequired = true;

            NextControlAfterNumber = "DocDate_0";
            NextControlAfterDate = "CorrectableFlag_0";
            //NextControlAfterDocDesc = "docNumberInp";
        }

        /// <summary>
        /// Биндинг контролов
        /// </summary>
        private void BindControls()
        {
            SetStoreValueByFirstPosition();

            if (rbProductSelect.Value == string.Empty)
            {
                rbProductSelect.Value = "0";
            }

            if (!Document.IsCorrected)
            {
                if (Document.IsNew || Document.DataUnavailable)
                {
                    selectVagon.Text = Resx.GetString("TTN_lblFillAreasWagonShipments");
                    selectVagon.OnClick = "javascript:cmd('cmd','AddSumm');";
                }
                else
                {
                    selectVagon.Text = Resx.GetString("TTN_lblSelectAreasWagonShipments");
                    selectVagon.OnClick = "javascript:cmd('cmd','SelectVagon');";
                }
            }
            //if (Doc.DocId != 0)
            //    Shipper.WeakList = DocPersons.GetDocsPersonsByDocId(Doc.DocId).Select(o => o.PersonId.ToString()).ToList();

        }

        /// <summary>
        /// Биндинг контролов
        /// </summary>
        private void SetStoreValueByFirstPosition()
        {
            if (Document.PositionMris != null && Document.PositionMris.Count > 0)
            {
                if (Document.PositionMris[0].ShipperStoreId > 0)
                {
                    DBSShipperStore.Value = Document.PositionMris[0].ShipperStoreId.ToString();
                }
                if (Document.PositionMris[0].PayerStoreId > 0)
                {
                    DBSPayerStore.Value = Document.PositionMris[0].PayerStoreId.ToString();
                }
            }
        }

        /// <summary>
        /// Установка плательщика и поставщика для новой ТТН по договору
        /// </summary>
        private void SetInitValue()
        {
            if (!Doc.IsNew && !Doc.DataUnavailable) return;
            if (!CurrentPerson.IsNullEmptyOrZero())
            {
                switch (Docdir)
                {
                    case DocDirs.In:
                        Document.PlatelschikField.Value = CurrentPerson;
                        Payer_Changed(null, null);
                        _payerPanel.Person_Changed(null, null);
                        //PlatelschikBS.TryFindSingleValue();
                        break;
                    case DocDirs.Out:
                        Document.PostavschikField.Value = CurrentPerson;
                        Shipper_Changed(null, null);
                        _shipperPanel.Person_Changed(null, null);
                        //if (ttn._Postavschik.Length==0) PostavschikBS.TryFindSingleValue();
                        break;
                }
            }
            else
            {
                var dogovor = ""; var d = 0;
                foreach (var _bd in Document.BasisDocLinks)
                {
                    var bd = new Document(_bd.BaseDocId.ToString());

                    if (bd.DocType == null) continue;
                    if (bd.DocType.ChildOf(DocTypeEnum.Договор))
                    {
                        dogovor = _bd.BaseDocId.ToString();
                        d++;
                    }
                }

                if (d != 1) return;
                Contract.Value = dogovor;
                Contract_Changed(null, null);
                ShipperOrPayer_Changed(null, null);
            }
        }

        /// <summary>
        /// Устанавливает наиментования полей в зависимости от локализации клиента
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        private string GetDocFieldDescription(DocField f)
        {
            if (IsRusLocal) return f.DocumentField;
            if (IsEstLocal) return f.DocumentFieldET;

            return f.DocumentFieldEN;
        }

        /// <summary>
        /// Установка курса на указанную дату
        /// </summary>
        /// <returns></returns>
        private string SetKursAndScale()
        {
            decimal kurs;
            Document.KursField.Value = "";
            Document.FormulaDescrField.Value = "";

            var strDocId = Contract.Value;
            if (string.IsNullOrEmpty(strDocId)) return "";

            var d = GetObjectById(typeof(Dogovor), strDocId) as Dogovor;
            if (d == null || d.Unavailable) return "";
            if (!d.IsDogovor) return "";

            //var strEncId = Enclosure.Value;
            //var p = GetObjectById(typeof(Prilozhenie), strEncId) as Prilozhenie;
            //if (p != null && !p.Unavailable && !p.DataUnavailable)
            //{
            //    var vidVzaimoraschetov = "0";
            //    if (p.IsEnclosure)
            //    {
            //        //если вид взаиморасчетов есть значит у договора есть эл.форма
            //        if (p.Contract == null || p.Contract.Value == null || p.Contract.Value.ToString().Length == 0)
            //        {
            //            // Отсутствует связь приложения с договором!
            //            return Resx.GetString("TTN_ntfNoLinkContractAnfEnclosure") + p.Id;
            //        }

            //        var pd = GetObjectById(typeof (Dogovor), p.Contract.Value.ToString()) as Dogovor;
            //        if (pd == null || pd.DataUnavailable)
            //        {
            //            // Попытка получить вид взаиморасчета у договора без электронной формы!
            //            return Resx.GetString("TTN_ntfGetTypeByContractNoForm") + p.Contract.Value;
            //        }

            //        vidVzaimoraschetov = pd.VidVzaimoraschetovField.Value.ToString();
            //    }

            //    if (p.IsEnclosure && vidVzaimoraschetov == "0")
            //    {
            //        if (!d.UE) return "";
            //    }
            //    else
            //    {
            //        if (p.UE)
            //        {
            //            kurs = p.GetCoefUe2Valuta(Document.Date);
            //            Currency.BindDocField.Value = p.Valyuta;
            //            Document.KursField.Value = (kurs == 1)? "" : Lib.ConvertExtention.Convert.Decimal2Str(kurs, Document.CurrencyScale*2);
            //            Document.FormulaDescrField.Value = (kurs == 1) ? "" : p.FormulaDescrField.Value;
            //            return "";
            //        }

            //        if (!d.UE) return "";
            //    }
            //}
            //else
            //{
            //    if (!d.UE) return "";
            //}

            if (!d.UE) return "";

            kurs = 0;
            try
            {
                kurs = d.GetCoefUe2Valuta(Document.Date);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            Currency.BindDocField.Value = d.Valyuta;
            Document.KursField.Value = (kurs == 1) ? "" : Lib.ConvertExtention.Convert.Decimal2Str(kurs, Document.CurrencyScale * 2);
            Document.FormulaDescrField.Value = (kurs == 1) ? "" : d.FormulaDescrField.Value;

            return "";
        }

        /// <summary>
        /// Карточка клиента
        /// </summary>
        /// <param name="p">Клиент</param>
        /// <param name="d">Дата</param>
        /// <returns></returns>
        public PersonOld.Card GetCardById(PersonOld p, DateTime d)
        {
            var ret = CardList.Find(o => o.PersonId == p.Id && o.Date == d);

            if (ret != null && IsPostBack
                            && (string.IsNullOrEmpty(ret.Object.CurrentPostRequest) || ret.Object.CurrentPostRequest != IDPostRequest)
                            && ret.Object.GetLastChanged(ret.Object.Id) != ret.Object.Changed)
            {
                cardList.Remove(ret);
                ret = null;
            }

            if (ret == null)
            {
                var card = p.GetCard(d);
                if (card != null)
                {
                    card.CurrentPostRequest = IDPostRequest;
                    ret = new V4PageCard { PersonId = p.Id, Date = d, Object = card };
                    cardList.Add(ret);
                }
            }
            else if (V4IsPostBack)
            {
                if (string.IsNullOrEmpty(ret.Object.CurrentPostRequest) || ret.Object.CurrentPostRequest != IDPostRequest)
                {
                    ret.Object.CurrentPostRequest = IDPostRequest;
                }
            }

            if (ret != null)
            {
                return ret.Object;
            }

            return null;
        }

        #endregion

        /// <summary>
		    ///     Событие загрузки страницы
		    /// </summary>
		    /// <param name="sender">Объект страницы</param>
		    /// <param name="e">Аргументы</param>
		protected void Page_Load(object sender, EventArgs e)
        {
            if (Doc.Unavailable && Doc.Id != "0")
            {
                Response.Write(string.Format(Resx.GetString("ppFltNotAccess"), Doc.Id));
                Response.End();
            }

            ClientScripts.InitializeGlobalVariables(this);

            FillLables();

            //Заголовок документа
            //Корректируемый документ должен иметь тип ТТН
            CorrectableTtn.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = Document.Type, QueryType = DocTypeQueryType.Equals });
            //CorrectableTtn.Filter.NextType.Value = Document.TypeID.ToString();
            //CorrectableTtn.Filter.NextType.FieldId = Document.CorrectingDocField.DocFieldId;
            CorrectableTtn.OnRenderNtf += Document_OnRenderNtf;
            CorrectableTtn.Changed += CorrectableTtn_Changed;

            //Секция Данные накладной
            DateOfPosting.OnRenderNtf += DateOfPosting_OnRenderNtf;
            Currency.OnRenderNtf += Currency_OnRenderNtf;

            //секция Грузоотправитель
            _goPanel = PersonPanel.Init(this, SectionPrefixes.Go, GoInfo, GoCodeInfo, GO, GoAddress, DispatchPoint, GoStore, GoStoreInfo, GoNotes);
            btnShipperToGo.Text = btnGoToShipper.Text = Resx.GetString("TTN_lblGoAndShipper");
            btnPayerToGp.Text = btnGpToPayer.Text = Resx.GetString("TTN_lblGpAndPayer");
            //GO.OnRenderNtf += GO_OnRenderNtf;
            GO.Changed += GO_Changed;

            //секция Грузополучатель
            _gpPanel = PersonPanel.Init(this, SectionPrefixes.Gp, GpInfo, GpCodeInfo, GP, GpAddress, DestinationPoint, GpStore, GpStoreInfo, GpNotes);
            GP.Changed += GP_Changed;

            //секция Поставщик
            _shipperPanel = PersonPanel.Init(this, SectionPrefixes.Shipper, ShipperInfo, ShipperCodeInfo, Shipper, ShipperAddress, null, ShipperStore, ShipperStoreInfo, null);
            Shipper.Changed += Shipper_Changed;

            //секция Плательщик
            _payerPanel = PersonPanel.Init(this, SectionPrefixes.Payer, PayerInfo, PayerCodeInfo, Payer, PayerAddress, null, PayerStore, PayerStoreInfo, null);
            Payer.Changed += Payer_Changed;

            DirectorPosition.OnRenderNtf += Position_OnRenderNtf;
            AccountantPosition.OnRenderNtf += Position_OnRenderNtf;
            StoreKeeperPosition.OnRenderNtf += Position_OnRenderNtf;

            _shipperPanel.PostGetTitle = Shipper_PostGetTitle;

            //Секция Транспорт
            CarTtn.BeforeSearch += s => CarTtn_BeforeSearch(CarTtn);
            CarTtn.OnRenderNtf += Document_OnRenderNtf;

            //Секция Товары
            addResource.OnClick = string.Format("cmd('cmd','AddResource', 'PageId', '{0}', 'DocId', '{1}', 'MrisId', {2})", IDPage, Document.Id, 0);
            rbProductSelect.Items.Add(new Lib.Web.Controls.V4.Item("0", " " + Resx.GetString("TTN_optImplementationByResources")));
            rbProductSelect.Items.Add(new Lib.Web.Controls.V4.Item("1", " " + Resx.GetString("TTN_optRealizationResourcesByWagons")));
            rbProductSelect.Items.Add(new Lib.Web.Controls.V4.Item("2", " " + Resx.GetString("TTN_optGroupByResource")));

            DBSShipperStore.BeforeSearch += StoreShipper_BeforeSearch;
            DBSPayerStore.BeforeSearch += StorePayer_BeforeSearch;

            DBSShipperStore.OnRenderNtf += StoreShipper_OnRenderNtf;
            DBSPayerStore.OnRenderNtf += StorePayer_OnRenderNtf;

            //Секция Услуги
            addFactUsl.OnClick = string.Format("cmd('cmd','AddFactUsl', 'PageId', '{0}', 'DocId', '{1}', 'FactUslId', {2})", IDPage, Document.Id, 0);
            
            //Секция Документы
            //Договор
            Contract.OnRenderNtf += Document_OnRenderNtf;
            Contract.BeforeSearch += s => Contract_BeforeSearch(Contract, Enclosure);

            //Приложение
            Enclosure.OnRenderNtf += Document_OnRenderNtf;
            Enclosure.OnRenderNtf += Enclosure_OnRenderNtf;
            Enclosure.BeforeSearch += s => Enclosure_BeforeSearch(Enclosure, Contract);

            //Заявка на покупку
            ApplicationForPurchasing.BeforeSearch += ApplicationForPurchasing_BeforeSearch;
            ApplicationForPurchasing.OnRenderNtf += Document_OnRenderNtf;

            //Аккредитив
            LetterOfCredit.BeforeSearch += LetterOfCredit_BeforeSearch;
            LetterOfCredit.OnRenderNtf += Document_OnRenderNtf;

            //Коносамент
            BillOfLading.BeforeSearch += BillOfLading_BeforeSearch;
            BillOfLading.OnRenderNtf += Document_OnRenderNtf;

            Invoice.OnRenderNtf += Document_OnRenderNtf;
            Invoice.BeforeSearch += Invoice_BeforeSearch;
            //Invoice.BeforeSearch += new BeforeSearchEventHandler((object s) => LinkedDoc_BeforeSearch(Invoice, PaymentDocuments));
            //Invoice.Changed += (s, eargs) => LinkedDoc_Changed2(Invoice, PaymentDocuments);

            PaymentDocuments.OnRenderNtf += Document_OnRenderNtf;
            PaymentDocuments.OnRenderNtf += PaymentDocuments_OnRenderNtf;
            PaymentDocuments.BeforeSearch += PaymentDocuments_BeforeSearch;
            //PaymentDocuments.BeforeSearch += new BeforeSearchEventHandler((object s) => LinkedDoc_BeforeSearch(PaymentDocuments, Invoice));
            //PaymentDocuments.Changed += (s, eargs) => LinkedDoc_Changed1(PaymentDocuments, Invoice);

            if (CopyId.IsNullEmptyOrZero()) RefreshTableCurrentDoc();
            if (!Doc.IsNew)
            {
                RenderNonPanelControlsNtf();
                LinkedDocs.RefreshData();
            }

            SetInitValue();
            SetStoreRequired();
            BindControls();

            V4SetFocus("DocNumber");
            NextControlAfterNumber = "DocDate_0";
            NextControlAfterDate = "CorrectableFlag_0";
            //NextControlAfterDocDesc = "docNumberInp";

            JS.Write(@"nakladnaya_clientLocalization = {{
                mris_title:""{0}"",
                factusl_title:""{1}"", 
                errOpenEditForm:""{2}""
            }};",
                Resx.GetString("TTN_MrisTitle"),
                Resx.GetString("TTN_FactUslTitle"),
                Resx.GetString("TTN_errOpenEditForm")
                );

            addFactUsl.Text = Resx.GetString("TTN_btnAddService") + "&nbsp;(Ins)";
            addResource.Text = Resx.GetString("TTN_bntAddResource") + "&nbsp;(Ins)";

            CorrectableTtn.IsDisabled = !DocEditable || !CorrectableFlag.Checked;
            CorrectableTtn.IsRequired = (CorrectableFlag.Value == "1");
            itogTable.Visible = !Doc.IsNew && (trServices.Visible || trProduct.Visible);
            RenderProductSelectPanel.Visible = !Doc.IsNew && (trServices.Visible || trProduct.Visible);

            // проверить, вызывается 2 раза! уже есть вызов из DocPage на OnInit
            //SetControlProperties();

            if (!DocEditable)
            {
                if (!CorrectableFlag.Checked)
                    Hide("divCorrectable");
                JS.Write("accordionCloseAll();");
            }

            var form = Request.QueryString["form"];
            switch (form)
            {
                case "mris":
                    JS.Write(@"tabActivate(1);$('#addResource').focus();");
                    break;
                case "factusl":
                    JS.Write(@"tabActivate(2);$('#addFactUsl').focus();");
                    break;
            }
            
            SetAccortdionTitles();
            JS.Write("SetImgDeleteVisible('{0}','{1}','{2}');", Director.Value, Accountant.Value, StoreKeeper.Value);
        }

        #region Override

        protected override void PrepareDocToCopy(Document doc)
        {
            base.PrepareDocToCopy(doc);
            if (doc is Kesco.Lib.Entities.Documents.EF.Trade.TTN)
            {
                ((Kesco.Lib.Entities.Documents.EF.Trade.TTN)doc)._CorrectingDoc = string.Empty;
                ((Kesco.Lib.Entities.Documents.EF.Trade.TTN)doc).CorrectingFlagField.Value = 0;
                ((Kesco.Lib.Entities.Documents.EF.Trade.TTN)doc).CorrectingDocField.Value = "";
            }
        }

        protected override void SetDocMenuButtons()
        {
            base.SetDocMenuButtons();
            var btnOldVersion = new Button
            {
                ID = "btnOldVersion",
                V4Page = this,
                Text = Resx.GetString("btnOldVersion"),
                Title = Resx.GetString("btnOldVersion"),
                IconJQueryUI = ButtonIconsEnum.Alert,
                Width = 125,
                OnClick = string.Format("v4_windowOpen('{0}','_self');", HttpUtility.JavaScriptStringEncode(WebExtention.UriBuilder(ConfigurationManager.AppSettings["URI_Direction_OldVersion"], CurrentQS)))
            };

            AddMenuButton(btnOldVersion);
        }

        /// <summary>
        /// Инициализация контролов
        /// </summary>
        /// <param name="copy"></param>
        protected override void DocumentInitialization(Document copy = null)
        {
            if (copy == null)
            {
                Doc = new Kesco.Lib.Entities.Documents.EF.Trade.TTN();
                Doc.Date = DateTime.Now.Date;
                Document.DateProvodkiField.Value = DateTime.Now.Date.ToString();
            }
            else
            {
                Doc = (Kesco.Lib.Entities.Documents.EF.Trade.TTN)copy;
                
                //if (!copy.Id.IsNullEmptyOrZero()) CorrId = copy.Id;

                RefreshMrisCurrentDoc(false, CopyId ?? CorrId);
                RefreshFactUslCurrentDoc(false, CopyId ?? CorrId);
                RefreshBasisDocLinksCurrentDoc(false, CopyId ?? CorrId);
            }

            //Связываем контролы с полями
            FieldsToControlsMapping = new Dictionary<V4Control, DocField>
            {
                {CorrectableFlag, Document.CorrectingFlagField},
                {CorrectableTtn, Document.CorrectingDocField},

                //Данные накладной		
                {DateOfPosting, Document.DateProvodkiField},
                {Currency, Document.CurrencyField},
                {Notes, Document.PrimechanieField},
                {Director, Document.SignSupervisorField},
                {DirectorPosition, Document.SignSupervisorPostField},
                {Accountant, Document.SignBuhgalterField},
                {AccountantPosition, Document.SignBuhgalterPostField},
                {StoreKeeper, Document.SignOtpustilField},
                {StoreKeeperPosition, Document.SignOtpustilPostField},

                //Поставщик		
                {ShipperInfo, Document.PostavschikDataField},
                {ShipperCodeInfo, Document.PostavschikOKPOField},
                {Shipper, Document.PostavschikField},
                {ShipperAddress, Document.PostavschikAddressField},
                {ShipperStore, Document.PostavschikBSField},
                {ShipperStoreInfo, Document.PostavschikBSDataField},

                //Плательщик		
                {PayerInfo, Document.PlatelschikDataField},
                {PayerCodeInfo, Document.PlatelschikOKPOField},
                {Payer, Document.PlatelschikField},
                {PayerAddress, Document.PlatelschikAddressField},
                {PayerStore, Document.PlatelschikBSField},
                {PayerStoreInfo, Document.PlatelschikBSDataField},

                //Грузоотправитель		
                {GoInfo, Document.GOPersonDataField},
                {GoCodeInfo, Document.GOPersonOKPOField},
                {GO, Document.GOPersonField},
                //{GoAddress, null},
                {DispatchPoint, Document.GOPersonWeselField},
                {GoStore, Document.GOPersonBSField},
                {GoStoreInfo, Document.GOPersonBSDataField},
                {GoNotes, Document.GOPersonNoteField},

                //Грузополучатель		
                {GpInfo, Document.GPPersonDataField},
                {GpCodeInfo, Document.GPPersonOKPOField},
                {GP, Document.GPPersonField},
                //{GpAddress, null},
                {DestinationPoint, Document.GPPersonWeselField},
                {GpStore, Document.GPPersonBSField},
                {GpStoreInfo, Document.GPPersonBSDataField},
                {GpNotes, Document.GPPersonNoteField},

                //Документы		
                {ContractInfo, Document.DogovorTextField},
                {Contract, Document.DogovorField},
                {Enclosure, Document.PrilozhenieField},
                {ApplicationForPurchasing, Document.ZvkBField},
                {LetterOfCredit, Document.AkkredField},
                {BillOfLading, Document.BillOfLadingField},
                {Invoice, Document.SchetPredField},
                {PaymentDocuments, Document.PlatezhkiField},

                //Транспорт		
                {PowerOfAttorney, Document.DoverennostField},
                {Driver, Document.VoditelField},
                {Car, Document.AvtomobilField},
                {CarNumber, Document.AvtomobilNomerField},
                {TrailerNumber, Document.PritsepNomerField},
                {CarTtn, Document.TTNTrField},

                //Товары
                {efMonthOfResources, Document.MonthResourceField}
            };
        }

        protected override void DocumentToControls()
        {
            base.DocumentToControls();
            if (CorrectableFlag.Checked && this.DocEditable)
            {
                if (FieldsToControlsMapping == null) return;
                foreach (var item in FieldsToControlsMapping.Where(item => null != item.Value))
                {
                    if (item.Key.ID != "DateOfPosting" && item.Key.ID != "Notes")
                        item.Key.IsReadOnly = true;
                }
            }
        }
        /// <summary>
        /// Установка свойств контролов ReadOnly, Disabled, Visible
        /// </summary>
        protected override void SetControlProperties()
        {
            CorrectableFlag.IsReadOnly = !DocEditable;
            CorrectableTtn.IsReadOnly = false;
            CorrectableTtn.IsDisabled = !(DocEditable && CorrectableFlag.Checked);

            divShipperToGo.Disabled = !DocEditable;
            divPayerToGp.Disabled = !DocEditable;
            divGoToShipper.Disabled = !DocEditable;
            divGpToPayer.Disabled = !DocEditable;

            MrisButtonPanel.Visible = addFactUsl.Visible = selectVagon.Visible = DocEditable;
            DBSShipperStore.IsReadOnly = DBSPayerStore.IsReadOnly = !DocEditable;
            efMonthOfResources.IsReadOnly = !DocEditable || CorrectableFlag.Checked;

            imgGoAddress.Visible = imgGpAddress.Visible = imgShipperAddress.Visible = imgPayerAddress.Visible = DocEditable;

            //SetHiddenForEmptyReadOnly
            if (!DocEditable || CorrectableFlag.Checked)
            {
                Hide("divAccountant"); Hide("divAccountantDelete");
                Hide("divDirector"); Hide("divDirectorDelete");
                Hide("divStoreKeeper"); Hide("divStoreKeeperDelete");

                Hide("divGpToPayer"); Hide("divGoToShipper"); Hide("divShipperToGo"); Hide("divPayerToGp");
                if (efMonthOfResources.Value.IsNullEmptyOrZero())
                    Hide("MonthResourcePanel");
                else
                    efMonthOfResources.IsReadOnly = true;
            }
            else
            {
                Display("divAccountant"); Display("divAccountantDelete");
                Display("divDirector"); Display("divDirectorDelete");
                Display("divStoreKeeper"); Display("divStoreKeeperDelete");

                Display("divGpToPayer");Display("divGoToShipper");Display("divShipperToGo");Display("divPayerToGp"); 
                if (efMonthOfResources.Value.IsNullEmptyOrZero())
                    Display("MonthResourcePanel");
                else
                    efMonthOfResources.IsReadOnly = false;
            }

            if (!DocEditable)
            {
                if (Notes.Value == "") Hide("divNotes");

                if (Enclosure.Value == "") Hide("divEnclosure");
                if (ApplicationForPurchasing.Value == "") Hide("divApplicationForPurchasing");
                if (LetterOfCredit.Value == "") Hide("divLetterOfCredit");
                if (BillOfLading.Value == "") Hide("divBillOfLading");
                if (Invoice.Value == "") Hide("divInvoice");
                if (PaymentDocuments.Value == "") Hide("divPaymentDocuments");

                if (PowerOfAttorney.Value == "") Hide("divPowerOfAttorney");
                if (Driver.Value == "") Hide("divDriver");
                if (Car.Value == "") Hide("divCar");
                if (CarNumber.Value == "") Hide("divCarNumber");
                if (TrailerNumber.Value == "") Hide("divTrailerNumber");
                if (CarTtn.Value == "") Hide("divCarTtn");

                if (PowerOfAttorney.Value == "" &&
                    Driver.Value == "" &&
                    Car.Value == "" &&
                    CarNumber.Value == "" &&
                    TrailerNumber.Value == "" &&
                    CarTtn.Value == "")
                {
                    Hide("divTransport");
                }
            }

            // SetAlwaysReadOnly
            Director.IsReadOnly = DirectorPosition.IsReadOnly = true;
            Accountant.IsReadOnly = AccountantPosition.IsReadOnly = true;
            StoreKeeper.IsReadOnly = StoreKeeperPosition.IsReadOnly = true;

            ShipperInfo.IsReadOnly = PayerInfo.IsReadOnly = true;
            ShipperCodeInfo.IsReadOnly = PayerCodeInfo.IsReadOnly = true;
            ShipperAddress.IsReadOnly = PayerAddress.IsReadOnly = true;
            ShipperStoreInfo.IsReadOnly = PayerStoreInfo.IsReadOnly = true;

            GoInfo.IsReadOnly = GoCodeInfo.IsReadOnly = GoStoreInfo.IsReadOnly = true;
            GpInfo.IsReadOnly = GpCodeInfo.IsReadOnly = GpStoreInfo.IsReadOnly = true;

            if (!CopyId.IsNullEmptyOrZero()) SetStoreValueByFirstPosition();
        }

        /// <summary>
        ///     Сохранение даты документа в модели данных
        /// </summary>
        protected override void OnDocDateChanged(object sender, ProperyChangedEventArgs e)
        {
            base.OnDocDateChanged(sender, e);

            CorrectableTtn.RenderNtf();
            DateOfPosting.RenderNtf();
            CarTtn.RenderNtf();

            SetAccordionHeader(SectionPrefixes.General + suffixTitle);
            SetAccordionHeader(SectionPrefixes.Documents + suffixTitle);
            SetAccordionHeader(SectionPrefixes.Transport + suffixTitle);

            _goPanel.OnDocDateChanged();
            _gpPanel.OnDocDateChanged();
            _shipperPanel.OnDocDateChanged();
            _payerPanel.OnDocDateChanged();
        }

        /// <summary>
        ///     Обновляет табличные поля, специфичные для данного документа(без полной перезагрузки страницы)
        /// </summary>
        public override void RefreshTableCurrentDoc()
        {
            RefreshMrisCurrentDoc(false);
            RefreshFactUslCurrentDoc(false);
        }

        /// <summary>
        ///     Обновляет табличное поле товаров
        /// </summary>
        public void RefreshMrisCurrentDoc(bool isAddNew, string copyId = "")
        {
            FillMrisDataGrid(isAddNew);
            Document.LoadPositionMris(copyId);
            RenderItogTable();
        }

        /// <summary>
        ///     Обновляет табличное поле услуг
        /// </summary>
        public void RefreshFactUslCurrentDoc(bool isAddNew, string copyId = "")
        {
            FillFactUslDataGrid(isAddNew);
            Document.LoadPositionFactUsl(copyId);
            RenderItogTable();
        }

        /// <summary>
        ///     Обновляет табличное поле ссылок
        /// </summary>
        public void RefreshBasisDocLinksCurrentDoc(bool isNew, string copyId = "")
        {
            int docId = Convert.ToInt32(copyId);
            Document.BasisDocLinks = DocLink.LoadBasisDocsByChildId(docId);
            foreach (var item in Document.BasisDocLinks)
            {
                item.Id = null;
                item.SequelDocId = 0;
            }
            
            Invoice.SelectedItems.Clear();
            Invoice.SelectedItems.AddRange(Document.GetDocLinksItems(Document.SchetPredField.DocFieldId, docId));

            PaymentDocuments.SelectedItems.Clear();
            PaymentDocuments.SelectedItems.AddRange(Document.GetDocLinksItems(Document.PlatezhkiField.DocFieldId, docId));
            
        }

        /// <summary>
        ///     Обработка клиентских команд
        /// </summary>
        /// <param name="cmd">Команды</param>
        /// <param name="param">Параметры</param>
        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            List<string> validList;
            switch (cmd)
            {
                //Команда для установки данных панели поставщик как у грузоотправителя или наоборот
                case "OnGoToShipper":
                case "OnShipperToGo":
                    if (Shipper.Value != GO.Value && !(Shipper.Value.IsNullEmptyOrZero() && GO.Value.IsNullEmptyOrZero()))
                    {
                        if (Shipper.Value.IsNullEmptyOrZero()) {
                            _goPanel.BindTo(_shipperPanel); 
                        }
                        else if (GO.Value.IsNullEmptyOrZero()) {
                            _shipperPanel.BindTo(_goPanel);
                        }
                        else
                        {
                            if (cmd == "OnGoToShipper") 
                                _goPanel.BindTo(_shipperPanel);
                            else
                                _shipperPanel.BindTo(_goPanel);
                        }
                        SetStoreRequired();
                    }
                    break;
                //Команда для установки данных панели плательщик как у грузополучателем
                case "OnGpToPayer":
                case "OnPayerToGp":
                    if (Payer.Value != GP.Value && !(Payer.Value.IsNullEmptyOrZero() && GP.Value.IsNullEmptyOrZero()))
                    {
                        if (Payer.Value.IsNullEmptyOrZero()) {
                            _gpPanel.BindTo(_payerPanel);

                        }
                        else if (GP.Value.IsNullEmptyOrZero()) {
                            _payerPanel.BindTo(_gpPanel);
                        }
                        else
                        {
                            if (cmd == "OnGpToPayer")
                                _gpPanel.BindTo(_payerPanel);
                            else
                                _payerPanel.BindTo(_gpPanel);
                        }
                        SetStoreRequired();
                    }
                    break;

                //Команда для установки режима работы с корректируемым документом
                //Если value==0, то режим отключен
                case "SetCorrectableDocument":
                    {
                        if (param["value"] == "true")
                        {
                            Kesco.Lib.Entities.Documents.EF.Trade.TTN ttn = new Kesco.Lib.Entities.Documents.EF.Trade.TTN(CorrectableTtn.Value);
                            FillByCorrectableTtn(ttn);

                            RenderNonPanelControlsNtf();
                            
                            _goPanel.UpdateFieldVisibility();
                            _gpPanel.UpdateFieldVisibility();
                            _shipperPanel.UpdateFieldVisibility();
                            _payerPanel.UpdateFieldVisibility();
                            
                            SetAccortdionTitles();

                            //JS.Write("$('#DocDate_0').val('{0}');", ttn.Date.ToShortDateString());
                            //JS.Write("$('#docNumberInp_0').val('{0}');", ttn.Number);
                        }
                        else
                        {
                            CorrectableTtn.Value = param["oldValue"];
                            Document._CorrectingDoc = param["oldValue"];
                        }
                    }
                    CorrectableTtn.RenderNtf();
                    CorrectableTtn.IsRequired = (CorrectableFlag.Value == "1");

                    break;

                //Команда для установки заголовка раздела
                case "SetAccordionHeader":
                    SetAccordionHeader(param["id"]);
                    break;

                //Добавление товара
                case "AddResource":
                    if (ValidateDocument(out validList))
                    {
                        if (DBSShipperStore.IsRequired && DBSShipperStore.Value == "")
                        {
                            validList.Add(Resx.GetString("TTN_ntfNotShipperStore"));
                        }

                        if (DBSPayerStore.IsRequired && DBSPayerStore.Value == "")
                        {
                            validList.Add(Resx.GetString("TTN_ntfNotPayerStore"));
                        }

                        if (validList.Count > 0)
                        {
                            RenderErrors(validList, "<br/>" + Resx.GetString("TTN_msgEditingNoPossible"));
                        }
                        else
                        {
                            AddResource(Resx.GetString("TTN_MrisTitle"), param["PageId"], param["DocId"], param["MrisId"]);
                        }
                    }
                    else
                    {
                        RenderErrors(validList, "<br/>"+Resx.GetString("TTN_msgEditingNoPossible"));
                    }
                    break;

                //Обновление списка товаров
                case "RefreshResource":
                    SetStoreValueByFirstPosition();
                    if (param["reloadForm"] == "True")
                    {
                        var isNew = "";
                        if (param["isNew"] == "True") isNew = "&isnew=1";
                        RefreshDocument("form=mris" + isNew);
                    }
                    else
                    {
                        RefreshMrisCurrentDoc(param["isNew"] == "True");
                        JS.Write("resources_Records_Close({0}); $('#addResource').focus();", param["ctrlFocus"]);
                    }
                    break;

                //Обновление списка товаров
                case "RefreshResourceByVagon":
                    if (param["reloadForm"] == "True")
                    {
                        RefreshDocument("form=mris");
                    }
                    else
                    {
                        RefreshMrisCurrentDoc(true);
                        JS.Write("vagon_Records_Close();");
                    }
                    break;

                //Копирование товара
                case "MrisCopy":
                    CopyMrisPosition(param["MrisId"]);
                    break;

                //Удаление товара
                case "MrisDelete":
                    if (!Document.IsNew && Document.PositionMris.Count == 1)
                    {
                        ShowMessage(Resx.GetString("TTN_msgLastMrisDelete"), Resx.GetString("errDoisserWarrning") );
                        break;
                    }
                    
                    DeleteMrisPosition(param["MrisId"]);
                    break;

                //Редактирование товара
                case "MrisEdit":
                    AddResource(Resx.GetString("TTN_MrisTitle"), IDPage, Document.Id, param["MrisId"]);
                    break;

                //Добавление услуги
                case "AddFactUsl":
                    if (ValidateDocument(out validList, "AddFactUsl"))
                    {
                        AddService(Resx.GetString("TTN_FactUslTitle"), param["PageId"], param["DocId"], param["FactUslId"]);
                    }
                    else
                    {
                        RenderErrors(validList, "<br/>" + Resx.GetString("TTN_msgEditingNoPossible"));
                    }
                    break;

                //Обновление списка услуг
                case "RefreshFactUsl":
                    if (param["reloadForm"] == "True")
                    {
                        var isNew = "";
                        if (param["isNew"] == "True") isNew = "&isnew=1";
                        RefreshDocument("form=factusl"+isNew);
                    }
                    else
                    {
                        RefreshFactUslCurrentDoc(param["isNew"] == "True");
                        JS.Write("services_Records_Close({0}); $('#addFactUsl').focus();", param["ctrlFocus"]);
                    }
                    break;

                //Копирование услуги
                case "FactUslCopy":
                    CopyFactUslPosition(param["FactUslId"]);
                    break;

                //Удаление услуги
                case "FactUslDelete":
                    DeleteFactUslPosition(param["FactUslId"]);
                    break;

                //Редактирование услуги
                case "FactUslEdit":
                    AddService(Resx.GetString("TTN_FactUslTitle"), IDPage, Document.Id, param["FactUslId"]);
                    break;

                case "FlagCorrecting_Uncheck":
                    FlagCorrecting_Uncheck(false);
                    break;

                case "ShowCorrectingDoc":
                    FlagCorrecting_Uncheck(true, param["oldValue"]);
                    break;

                case "SelectVagon":
                    if (ValidateDocument(out validList))
                    {
                        if (DBSShipperStore.IsRequired && DBSShipperStore.Value == "")
                        {
                            validList.Add(Resx.GetString("TTN_ntfNotShipperStore"));
                        }

                        if (DBSPayerStore.IsRequired && DBSPayerStore.Value == "")
                        {
                            validList.Add(Resx.GetString("TTN_ntfNotPayerStore"));
                        }

                        if (validList.Count > 0)
                        {
                            RenderErrors(validList, "<br/>" + Resx.GetString("TTN_msgEditingNoPossible"));
                        }
                        else
                        {
                            // формирование параметров с ограничениями по ГО/ГП для проекта выбора отправок
                            string sParams = "";
                            if ((Document.GOPersonField.Value ?? "").ToString().Length > 0) sParams += "&goperson=" + Document.GOPersonField.Value;
                            if ((Document.GPPersonField.Value ?? "").ToString().Length > 0) sParams += "&gpperson=" + Document.GPPersonField.Value;
                            if ((Document.GOPersonWeselField.Value ?? "").ToString().Length > 0) sParams += "&gowesel=" + Document.GOPersonWeselField.Id;
                            if ((Document.GPPersonWeselField.Value ?? "").ToString().Length > 0) sParams += "&gpwesel=" + Document.GPPersonWeselField.Id;

                            var parameters = string.Format("idpp={0}&idDoc={1}{2}", IDPage, Document.Id, sParams);
                            ReturnDialogResult.ShowAdvancedDialogSearch(this, "Select_Vagon", "GridResource", Config.delivery_search, parameters, true, 0, 800, 600);
                        }
                    }
                    else
                    {
                        RenderErrors(validList, "<br/>" + Resx.GetString("TTN_msgEditingNoPossible"));
                    }
                    break;

                case "OnSelectedVagon":
                    AddVagon(Resx.GetString("TTN_lblShipmentAmounts"), IDPage, Document.Id, param["ResultId"]);
                    break;

                case "AddSumm":
                    if (ValidateDocument(out validList))
                    {
                        JS.Write("cmd('cmd', 'SelectVagon');");
                    }
                    else
                    {
                        RenderErrors(validList, "<br/>" + Resx.GetString("TTN_msgEditingNoPossible"));
                    }
                    break;

                // Наборы продукта
                case "MrisDetail":
                    DetailMrisPosition(param["MrisId"]);
                    break;

                case "NaborShipper":
                    DetailMrisPosition(param["MrisId"]);
                    break;

                case "NaborPayer":
                    DetailMrisPosition(param["MrisId"]);
                    break;

                case "DistribSave":
                    JS.Write("distrib_Records_Close();");
                    break;

                case "GetContact":
                    var position = "divAddress";
                    DBSContact.Value = null;
                    switch (param["Type"])
                    {
                        case SectionPrefixes.Go:
                            DBSGOPerson.Value = GO.Value;
                            tbFaceName.Value = SectionTitles.GO;
                            position = "imgGoAddress";
                            DBSContact.BeforeSearch += s => Address_BeforeSearch(DBSContact, GO.Value);
                            break;
                        case SectionPrefixes.Gp:
                            DBSGOPerson.Value = GP.Value;
                            tbFaceName.Value = SectionTitles.GP;
                            position = "imgGpAddress";
                            DBSContact.BeforeSearch += s => Address_BeforeSearch(DBSContact, GP.Value);
                            break;
                        case SectionPrefixes.Shipper:
                            DBSGOPerson.Value = Shipper.Value;
                            tbFaceName.Value = SectionTitles.Shipper;
                            position = "imgShipperAddress";
                            DBSContact.BeforeSearch += s => Address_BeforeSearch(DBSContact, Shipper.Value);
                            break;
                        case SectionPrefixes.Payer:
                            DBSGOPerson.Value = Payer.Value;
                            tbFaceName.Value = SectionTitles.Payer;
                            position = "imgPayerAddress";
                            DBSContact.BeforeSearch += s => Address_BeforeSearch(DBSContact, Payer.Value);
                            break;
                    }
                    DBSGOPerson.IsReadOnly = true;
                    DBSGOPerson.RefreshRequired = true;
                    DBSContact.Focus();
                    JS.Write("personcontact_dialogShow('{0}','{1}','{2}','{3}','{4}');", param["Type"], position, Resx.GetString("TTN_lblChoiceContact"), Resx.GetString("cmdApply"), Resx.GetString("cmdClose"));
                    break;
                case "SetAddress":
                    switch (param["Type"])
                    {
                        case SectionPrefixes.Go:
                            GoAddress.Value = DBSContact.ValueText;
                            GoAddress_Changed();
                            break;
                        case SectionPrefixes.Gp:
                            GpAddress.Value = DBSContact.ValueText;
                            GpAddress_Changed();
                            break;
                        case SectionPrefixes.Shipper:
                            ShipperAddress.Value = DBSContact.ValueText;
                            break;
                        case SectionPrefixes.Payer:
                            PayerAddress.Value = DBSContact.ValueText;
                            break;
                    }
                    JS.Write("v4_closeAddressForm();");
                    break;
                case "GetPerson":
                    position = "divSigner";
                    DBSPerson.Value = null;
                    DBSPosition.Value = null;
                    DBSPerson.Filter.PersonType = 2;
                    DBSPerson.BeforeSearch += Person_BeforeSearch;
                    DBSPosition.IsDisabled = true;
                    DBSPosition.BeforeSearch += s => PersonPosition_BeforeSearch(DBSPosition, param["Type"]);
                    switch (param["Type"])
                    {
                        case "Director":
                            tbPerson.Value = FieldLabels[Director];
                            DBSPerson.ValueText = Director.Value;
                            tbPosition.Value = FieldLabels[DirectorPosition];
                            DBSPosition.ValueText = DirectorPosition.Value;
                            position = "imgDirector";
                            break;
                        case "Accountant":
                            tbPerson.Value = FieldLabels[Accountant];
                            DBSPerson.ValueText = Accountant.Value;
                            tbPosition.Value = FieldLabels[AccountantPosition];
                            DBSPosition.ValueText = AccountantPosition.Value;
                            position = "imgAccountant";
                            break;
                        case "StoreKeeper":
                            tbPerson.Value = FieldLabels[StoreKeeper];
                            DBSPerson.ValueText = StoreKeeper.Value;
                            tbPosition.Value = FieldLabels[StoreKeeperPosition];
                            DBSPosition.ValueText = StoreKeeperPosition.Value;
                            position = "imgStoreKeeper";
                            break;
                    }
                    JS.Write("person_dialogShow('{0}','{1}','{2}','{3}','{4}');", param["Type"], position, Resx.GetString("TTN_lblChoiceEmployee"), Resx.GetString("cmdApply"), Resx.GetString("cmdClose"));
                    break;
                case "SetPerson":
                    if ((DBSPerson.ValueText.IsNullEmptyOrZero() || !DBSPerson.Value.IsNullEmptyOrZero()) &&
                        (DBSPosition.ValueText.IsNullEmptyOrZero() || !DBSPosition.Value.IsNullEmptyOrZero()))
                    {
                        switch (param["Type"])
                        {
                            case "Director":
                                Director.Value = DBSPerson.ValueText;
                                DirectorPosition.Value = DBSPosition.ValueText;
                                break;
                            case "Accountant":
                                Accountant.Value = DBSPerson.ValueText;
                                AccountantPosition.Value = DBSPosition.ValueText;
                                break;
                            case "StoreKeeper":
                                StoreKeeper.Value = DBSPerson.ValueText;
                                StoreKeeperPosition.Value = DBSPosition.ValueText;
                                break;
                        }
                    }
                    JS.Write("v4_closePersonForm();");
                    JS.Write("SetImgDeleteVisible('{0}','{1}','{2}');", Director.Value, Accountant.Value, StoreKeeper.Value);
                    break;
                case "DeletePerson":
                    switch (param["Type"])
                    {
                        case "Director":
                            Director.Value = null;
                            DirectorPosition.Value = null;
                            break;
                        case "Accountant":
                            Accountant.Value = null;
                            AccountantPosition.Value = null;
                            break;
                        case "StoreKeeper":
                            StoreKeeper.Value = null;
                            StoreKeeperPosition.Value = null;
                            break;
                    }
                    JS.Write("SetImgDeleteVisible('{0}','{1}','{2}');", Director.Value, Accountant.Value, StoreKeeper.Value);
                    break;
                default:
                    base.ProcessCommand(cmd, param);
                    break;

            }
        }

		protected override void LoadData(string id)
		{
			base.LoadData(id);

			if (!id.IsNullEmptyOrZero() || Document.DataUnavailable)
			{
				//Document.InvoiceDocLinks = DocLink.LoadBaseDocs(Doc.DocId, Document.SchetPredField.DocFieldId);
				//Document.PaymentDocLinks = DocLink.LoadBaseDocs(Doc.DocId, Document.PlatelschikField.DocFieldId);

				Document.BasisDocLinks = DocLink.LoadBasisDocsByChildId(Doc.DocId);
			}
		}

        #endregion

        #region Fill

        /// <summary>
        /// Метод заполнения таблицы движения на складе данными
        /// </summary>
        private void FillMrisDataGrid(bool isAddNew)
        {
            GridResource.EmptyDataString = Resx.GetString("TTN_msgGridNoMris");
            GridResource.EmptyDataNtfStatus = NtfStatus.Error;

            if (Doc.IsNew)
            {
                GridResource.SetDataSource(null);
                GridResource.RefreshGridData();
            }

            var currentPage = GridResource.GеtCurrentPage();

            divResourceGrid.Visible = true;
            Dictionary<string, object> sqlParams = GetSQLParams();
            
            #region Настройка параметров колонок, общих для все видов грида
           
            var listColumnVisible = new List<string>
            {
                "КодДвиженияНаСкладе",
                "КодРесурса",
                "КодОтправкиВагона",
                "КодЕдиницыИзмерения",
                "Точность",
                "КодСтавкиНДС",
                "КодСтраныПроисхождения",
                "КодТаможеннойДекларации",
                "Порядок",
                "Изменил",
                "Изменено"
            };

            var dictHeaderAlias = new Dictionary<string, string>
            {
                {"РесурсРус", Resx.GetString("lblProduct")},
                {"Количество", Resx.GetString("lblPosCol2")},
                {"ЕдиницаРус", Resx.GetString("lblPosCol3")},
                {"ЦенаБезНДС", Resx.GetString("lblPosCol4")},
                {"Величина100", "%"},
                {"СуммаБезНДС", Resx.GetString("lblPosCol5")},
                {"СуммаНДС", Resx.GetString("lblPosCol6")},
                {"Всего", Resx.GetString("lblPosCol7")}
            };

            var dictDefaultScale = new Dictionary<string, int>
            {
                {"Величина100", 0},
                {"СуммаБезНДС", 2},
                {"СуммаНДС", 2},
                {"Всего", 2}
            };

            var listFormatN = new List<string>
            {
                "Количество",
                "ЦенаБезНДС",
                "Величина100",
                "СуммаБезНДС",
                "СуммаНДС",
                "Всего"
            };

            var listTextAlignCenter = new List<string>
            {
                "ЕдиницаРус",
                "Величина100" 
            };

            #endregion

            switch (rbProductSelect.Value)
            {
                case "1":
                    GridResource.ExistServiceColumn = false;
                    GridResource.ExistServiceColumnDetail = false;
                    
                    GridResource.SetDataSource(SQLQueries.SELECT_ID_DOC_ДвиженияНаСкладах_GRID, Config.DS_document, CommandType.Text, sqlParams);

                    listColumnVisible.Add("СтранаПроисхождения");
                    listColumnVisible.Add("ТаможеннаяДекларация");
                    GridResource.Settings.SetColumnDisplayVisible(listColumnVisible, false);
                    
                    dictHeaderAlias.Add("ОтправкаВагона", Resx.GetString("lblPosCol12"));
                    GridResource.Settings.SetColumnHref("ОтправкаВагона", "КодОтправкиВагона", Config.delivery_form);
                    
                    break;

                case "2":
                    GridResource.ExistServiceColumn = false;
                    GridResource.ExistServiceColumnDetail = false;
                    
                    GridResource.SetDataSource(SQLQueries.SELECT_ID_DOC_ДвиженияНаСкладах_GRID_ПоРесурсу, Config.DS_document, CommandType.Text, sqlParams);

                    GridResource.Settings.SetColumnDisplayVisible(new List<string> { "КодРесурса", "КодЕдиницыИзмерения", "Точность", "КодСтавкиНДС" }, false);
                    
                    break;
                default:
                    GridResource.ExistServiceColumn = CopyId.IsNullEmptyOrZero();
                    GridResource.ExistServiceColumnDetail = CopyId.IsNullEmptyOrZero() && DocEditable;
                    
                    GridResource.SetDataSource(SQLQueries.SELECT_ID_DOC_ДвиженияНаСкладах_GRID, Config.DS_document, CommandType.Text, sqlParams);

                    listColumnVisible.Add("ОтправкаВагона");
                    GridResource.Settings.SetColumnDisplayVisible(listColumnVisible, false);

                    // Установка алиасов
                    dictHeaderAlias.Add("СтранаПроисхождения", Resx.GetString("lblPosCol13"));
                    dictHeaderAlias.Add("ТаможеннаяДекларация", Resx.GetString("lblPosCol14"));
                    
                    GridResource.Settings.SetColumnHref("СтранаПроисхождения", "КодСтраныПроисхождения", Config.territory_form);
                    GridResource.Settings.SetColumnHrefDocument("ТаможеннаяДекларация", "КодТаможеннойДекларации");

                    if (CopyId.IsNullEmptyOrZero())
                    {
                        if (DocEditable)
                        {
                            GridResource.SetServiceColumnDelete("mris_delete", new List<string> {"КодДвиженияНаСкладе"},
                                new List<string> {"РесурсРус"}, Resx.GetString("TTN_btnDeletePosition"));
                            GridResource.SetServiceColumnCopy("mris_copy", new List<string> {"КодДвиженияНаСкладе"},
                                Resx.GetString("TTN_btnCopyPosition"));
                        }

                        GridResource.SetServiceColumnDetail("mris_detail", new List<string> {"КодДвиженияНаСкладе"},
                            Resx.GetString("TTN_lblProductKits"));
                        GridResource.SetServiceColumnEdit("mris_edit", new List<string> {"КодДвиженияНаСкладе"},
                            Resx.GetString("TTN_btnEditPosition"));
                    }

                    break;
            }

            // Установка алиасов заголовков
            GridResource.Settings.SetColumnHeaderAlias(dictHeaderAlias);

            //Колонки выравниваем по центру
            GridResource.Settings.SetColumnTextAlign(listTextAlignCenter, "center");

            // Установка формата данных
            GridResource.Settings.SetColumnFormat(listFormatN, "N");
            GridResource.Settings.SetColumnFormatByValueScale("ЦенаБезНДС", 4, 2);
            GridResource.Settings.SetColumnFormatByColumnScale("Количество", "Точность", 3);

            //Установка точности по-умолчанию
            GridResource.Settings.SetColumnFormatDefaultScale(dictDefaultScale);

            //Установка ссылки на поле
            GridResource.Settings.SetColumnHref("РесурсРус", "КодРесурса", Config.resource_form);
            
            //Заполняем контрол
            GridResource.RefreshGridData();
            if (isAddNew) GridResource.GoToLastPage(); else GridResource.GoToPage(currentPage);

            ItogArray[0, 0] = Math.Round(GridResource.GetSumDecimalByColumnValue("СуммаБезНДС"), 2);
            ItogArray[0, 1] = Math.Round(GridResource.GetSumDecimalByColumnValue("СуммаНДС"), 2);
            ItogArray[0, 2] = Math.Round(GridResource.GetSumDecimalByColumnValue("Всего"), 2);

            trProduct.Visible = ItogArray[0, 2] != 0;

        }

        /// <summary>
        /// Метод заполнения таблицы оказанных услоуг данными
        /// </summary>
        private void FillFactUslDataGrid(bool isAddNew)
        {
            GridUsl.EmptyDataString = Resx.GetString("TTN_msgGridNoUsl");
            GridUsl.EmptyDataNtfStatus = NtfStatus.Information;

            if (CopyId.IsNullEmptyOrZero() && CorrId.IsNullEmptyOrZero())
            {
                Hide("NeedSaveDocumentPanelMris");
                Hide("NeedSaveDocumentPanelUsl");
            }
            else
            {
                Display("NeedSaveDocumentPanelMris");
                Display("NeedSaveDocumentPanelUsl");
            }
            divNeedSaveDocumentPanelMris.Value = divNeedSaveDocumentPanelUsl.Value = Resx.GetString("TTN_msgNeedSaveDocument") + "!";
            
            if (Doc.IsNew)
            {
                GridUsl.SetDataSource(null);
                GridUsl.RefreshGridData();
            }

            var currentPage = GridUsl.GеtCurrentPage();
            Dictionary<string, object> sqlParams = GetSQLParams();
            #region Настройка парметров колонок, общих для все видов грида

            var listColumnVisible = new List<string>
            {
                "КодОказаннойУслуги",
                "GuidОказаннойУслуги",
                "КодДвиженияНаСкладе",
                "КодРесурса",
                "РесурсЛат",
                "КодУчасткаОтправкиВагона",
                "КодЕдиницыИзмерения",
                "Коэффициент",
                "Точность",
                "КодСтавкиНДС",
                "Порядок",
                "Изменил",
                "Изменено"
            };

            var dictHeaderAlias = new Dictionary<string, string>
            {
                {"РесурсРус", Resx.GetString("lblService")},
                {"Количество", Resx.GetString("lblPosCol2")},
                {"ЕдиницаРус", Resx.GetString("lblPosCol3")},
                {"ЦенаБезНДС", Resx.GetString("lblPosCol4")},
                {"Величина100", "%"},
                {"СуммаБезНДС", Resx.GetString("lblPosCol5")},
                {"СуммаНДС", Resx.GetString("lblPosCol6")},
                {"Всего", Resx.GetString("lblPosCol7")}
            };

            var listFormatN = new List<string>
            {
                "Количество",
                "ЦенаБезНДС",
                "Величина100",
                "СуммаБезНДС",
                "СуммаНДС",
                "Всего"
            };

            var listBitColumn = new List<string>
            {
                "Агент1",
                "Агент2"
            };

            var dictDefaultScale = new Dictionary<string, int>
            {
                {"Величина100", 0},
                {"СуммаБезНДС", 2},
                {"СуммаНДС", 2},
                {"Всего", 2}
            };

            var listTextAlignCenter = new List<string>
            {
                "Агент1",
                "Агент2",
                "ЕдиницаРус",
                "Величина100" 
            };

            #endregion

            GridUsl.SetDataSource(SQLQueries.SELECT_ID_DOC_ОказанныеУслуги_GRID, Config.DS_document, CommandType.Text, sqlParams);

            GridUsl.ExistServiceColumn = CopyId.IsNullEmptyOrZero();
            GridUsl.ExistServiceColumnDetail = false;

            GridUsl.Settings.SetColumnDisplayVisible(listColumnVisible, false);
            GridUsl.Settings.SetColumnHeaderAlias(dictHeaderAlias);
            GridUsl.Settings.SetColumnHeaderAlias("Агент1", Resx.GetString("lblPosCol15"), Resx.GetString("lblShipperAgent"));
            GridUsl.Settings.SetColumnHeaderAlias("Агент2", Resx.GetString("lblPosCol16"), Resx.GetString("lblPayerAgent"));
        
            GridUsl.Settings.SetColumnBitFormat(listBitColumn);
            GridUsl.Settings.SetColumnTextAlign(listTextAlignCenter, "center");
            
            // Установка формата данных
            GridUsl.Settings.SetColumnFormat(listFormatN, "N");
            GridUsl.Settings.SetColumnFormatByValueScale("ЦенаБезНДС", 4, 2);
            GridUsl.Settings.SetColumnFormatByColumnScale("КолExistServiceColumnичество", "Точность", 0);
            GridUsl.Settings.SetColumnFormatDefaultScale(dictDefaultScale);
            //Установка ссылки на поле
            GridUsl.Settings.SetColumnHref("РесурсРус", "КодРесурса", Config.resource_form);

            if (CopyId.IsNullEmptyOrZero())
            {
                if (DocEditable)
                {
                    GridUsl.SetServiceColumnDelete("factusl_delete", new List<string> {"КодОказаннойУслуги"},
                        new List<string> {"РесурсРус"}, Resx.GetString("TTN_btnDeletePosition"));
                    GridUsl.SetServiceColumnCopy("factusl_copy", new List<string> {"КодОказаннойУслуги"},
                        Resx.GetString("TTN_btnCopyPosition"));
                }
                GridUsl.SetServiceColumnEdit("factusl_edit", new List<string> {"КодОказаннойУслуги"},
                    Resx.GetString("TTN_btnEditPosition"));
            }

            GridUsl.RefreshGridData();
            if (isAddNew) GridUsl.GoToLastPage(); else GridUsl.GoToPage(currentPage);

            ItogArray[1, 0] = Math.Round(GridUsl.GetSumDecimalByColumnValue("СуммаБезНДС"), 2);
            ItogArray[1, 1] = Math.Round(GridUsl.GetSumDecimalByColumnValue("СуммаНДС"), 2);
            ItogArray[1, 2] = Math.Round(GridUsl.GetSumDecimalByColumnValue("Всего"), 2);
            trServices.Visible = ItogArray[1, 2] != 0;

            trTotal.Visible = !Doc.IsNew && trServices.Visible && trProduct.Visible;
            itogTable.Visible = !Doc.IsNew && (trServices.Visible || trProduct.Visible);
            RenderProductSelectPanel.Visible = !Doc.IsNew && (trServices.Visible || trProduct.Visible);

        }

        /// <summary>
        /// Метод устанавливает значения полей Дата, Валюта, Поставщик и Плательщик в соответствии с полями объекта типа Документ
        /// Метод вызывается первым, затем следуют методы, которые конкретизируют тип документа и пытаются извлечь оттуда больше данных
        /// </summary>
        /// <param name="d">Договор</param>
        private bool FillByDocument(Document d)
        {
            //if (Doc.Date == default(DateTime))
            //    Doc.Date = d.Date;

            if (string.IsNullOrEmpty(Currency.Value) && null != d.Currency)
            {
                Currency.Value = d.Currency.Id;
                Currency.BindDocField.Value = d.Currency.Id;
            }

            if (d.DataUnavailable) return false;

            var fShipperChanged = false;
            if (string.IsNullOrEmpty(Shipper.Value))
            {
                if (string.IsNullOrEmpty(Payer.Value) || Payer.ValueInt == d.DocumentData.PersonId2)
                {
                    fShipperChanged = Shipper.ValueInt != d.DocumentData.PersonId1;
                    Shipper.ValueInt = d.DocumentData.PersonId1;
                }
                else
                {
                    fShipperChanged = Shipper.ValueInt != d.DocumentData.PersonId2;
                    Shipper.ValueInt = d.DocumentData.PersonId2;
                }

                var p = new PersonOld(Shipper.Value);
                var card = GetCardById(p, Doc.Date == DateTime.MinValue ? DateTime.Today : Doc.Date);
                if (card != null)
                {
                    ShipperInfo.Value = card.NameRus.Length > 0 ? card.NameRus : card.NameLat;
                    ShipperAddress.Value = card.АдресЮридический.Length == 0 ? card.АдресЮридическийЛат : card.АдресЮридический;
                    ShipperCodeInfo.Value = p.OKPO;
                    _shipperPanel.SetPerson((int)Shipper.ValueInt, ShipperAddress.Value, ShipperInfo.Value);
                }
                
            }

            var fPayerChanged = false;
            if (string.IsNullOrEmpty(Payer.Value))
            {
                if (Shipper.ValueInt == d.DocumentData.PersonId1)
                {
                    fPayerChanged = Payer.ValueInt != d.DocumentData.PersonId2;
                    Payer.ValueInt = d.DocumentData.PersonId2;
                }
                else
                {
                    fPayerChanged = Payer.ValueInt != d.DocumentData.PersonId1;
                    Payer.ValueInt = d.DocumentData.PersonId1;
                }
                
                var p = new PersonOld(Payer.Value);
                var card = GetCardById(p, Doc.Date == DateTime.MinValue ? DateTime.Today : Doc.Date);

                if (card != null)
                {
                    PayerInfo.Value = card.NameRus.Length > 0 ? card.NameRus : card.NameLat;
                    PayerAddress.Value = card.АдресЮридический.Length == 0 ? card.АдресЮридическийЛат : card.АдресЮридический;
                    PayerCodeInfo.Value = p.OKPO;
                    _payerPanel.SetPerson((int)Payer.ValueInt, PayerAddress.Value, PayerInfo.Value);
                }
            }

            return fPayerChanged || fShipperChanged;
        }

        /// <summary>
        /// Метод устанавливает значения полей Поставщик и Плательщик в соответствии с полями документа типа Договор
        /// </summary>
        /// <param name="d">Договор</param>
        private bool FillByDogovor(Dogovor d)
        {
            //if (Doc.Date == default(DateTime))
            //    Doc.Date = d.Date;

            var fShipperChanged = false;

            if (string.IsNullOrEmpty(ShipperStore.Value))
            {
                // Убираем связь с грузополучателем (иначе р.с. не обновится)
                JS.Write("$('#checkboxShipper').attr('checked', false);");
                _gpPanel.BindTo(null);

                var oldShipperStoreValue = ShipperStore.Value;
                if (Shipper.Value == d.Person1Field.Value.SafeToString() || Payer.Value == d.Person2Field.Value.SafeToString())
                {
                    ShipperStore.BindDocField.Value = d.Sklad1Field.Value;
                }
                else if (Shipper.Value == d.Person2Field.Value.SafeToString() || Payer.Value == d.Person1Field.Value.SafeToString())
                {
                    ShipperStore.BindDocField.Value = d.Sklad2Field.Value;
                }
                else if (string.IsNullOrEmpty(PayerStore.Value) || PayerStore.Value == d.Sklad2Field.Value.SafeToString())
                {
                    ShipperStore.BindDocField.Value = d.Sklad1Field.Value;
                }
                else
                {
                    ShipperStore.BindDocField.Value = d.Sklad2Field.Value;
                }
                fShipperChanged = ShipperStore.Value != oldShipperStoreValue;

            }

            var fPayerChanged = false;
            if (string.IsNullOrEmpty(PayerStore.Value))
            {
                // Убираем связь с грузоотправителем
                JS.Write("$('#checkboxPayer').attr('checked', false);");
                _goPanel.BindTo(null);

                if (ShipperStore.Value == d.Sklad1Field.Value.SafeToString() || Shipper.Value == d.Person1Field.Value.SafeToString())
                {
                    fPayerChanged = ShipperStore.Value != d.Sklad1Field.Value.SafeToString();
                    PayerStore.BindDocField.Value = d.Sklad2Field.Value;
                }
                else
                {
                    PayerStore.BindDocField.Value = d.Sklad1Field.Value;
                }
            }

            return fPayerChanged || fShipperChanged;
        }

        /// <summary>
        /// Метод устанавливает значения полей в соответствии с полями корректируемой ТТН
        /// </summary>
        /// <param name="ttn">Корректируемая ТТН</param>
        private void FillByCorrectableTtn(Kesco.Lib.Entities.Documents.EF.Trade.TTN ttn)
        {
            CorrId = ttn.Id;
            var oldId = Doc.Id;
            var oldNumber = Doc.Number;
            var oldDate = Doc.Date;
            Kesco.Lib.Entities.Documents.EF.Trade.TTN cloned = (Kesco.Lib.Entities.Documents.EF.Trade.TTN)ttn.Clone();

            cloned.CorrectingDocField.Value = ttn.DocId;
            cloned.CorrectingFlagField.Value = "1";

            Doc = cloned;
            Doc.Id = oldId;
            Doc.Number = oldNumber;
            Doc.Date = oldDate;
            
            DocumentInitialization(Doc);
            DocumentToControls();
            SetControlProperties();
        }

        /// <summary>
        /// Метод устанавливает наиментования полей 
        /// </summary>
        private void FillLables()
        {
            foreach (var item in FieldsToControlsMapping.Where(item => null != item.Value))
            {
                FieldLabels[item.Key] = GetDocFieldDescription(item.Value) + ":";
            }
            
            //Устанавливаются отдельно
            //FieldLabels[GoAddress] = Resx.GetString("TTN_lblGOAddress") + ":";
            //FieldLabels[GpAddress] = Resx.GetString("TTN_lblGPAddress") + ":";
        }

        /// <summary>
        /// Метод устанавливает поля ГО и ГП из последней накладной с такими же поставщиком, плательщиком и договором, если она существует
        /// </summary>
        private void TryFindGOGP()
        {
            if (string.IsNullOrEmpty(Shipper.Value)) return;
            if (string.IsNullOrEmpty(Payer.Value)) return;
            if (string.IsNullOrEmpty(Contract.Value)) return;
            if (!string.IsNullOrEmpty(GP.Value) || !string.IsNullOrEmpty(GO.Value)) return;

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

        private void RenderItogTable()
        {
            JS.Write("$('#mrisSum').text('{0}');", NFormat(ItogArray[0, 0]));
            JS.Write("$('#mrisSumNDS').text('{0}');", NFormat(ItogArray[0, 1]));
            JS.Write("$('#mrisSumAll').text('{0}');", NFormat(ItogArray[0, 2]));

            JS.Write("$('#factUslSum').text('{0}');", NFormat(ItogArray[1, 0]));
            JS.Write("$('#factUslSumNDS').text('{0}');", NFormat(ItogArray[1, 1]));
            JS.Write("$('#factUslSumAll').text('{0}');", NFormat(ItogArray[1, 2]));

            JS.Write("$('#AllSum').text('{0}');", NFormat(ItogArray[0, 0] + ItogArray[1, 0]));
            JS.Write("$('#AllNDSSum').text('{0}');", NFormat(ItogArray[0, 1] + ItogArray[1, 1]));
            JS.Write("$('#AllAllSum').text('{0}');", NFormat(ItogArray[0, 2] + ItogArray[1, 2]));
        }

        /// <summary>
        /// Формат вывода числовых данных (сумм) в итоговой таблице
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public string NFormat(decimal d)
        {
            var c = GetObjectById(typeof(Resource), Currency.Value) as Resource;
            return d.ToString("N2") + ((c != null) ? " " + c.Unit.ЕдиницаРус : "");
        }

        #endregion

        #region Titles

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
		public static string GetCtrlText(V4Control ctrl, bool fEncoded, Employee currentUser)
		{
			var sbValues = new StringBuilder();
			if (ctrl is Select)
			{
				var sc = ctrl as Select;

			    if (sc.IsMultiSelect)
			    {
			        foreach (Kesco.Lib.Entities.Item i in sc.SelectedItems)
			        {
			            if (sbValues.Length > 0) sbValues.Append(", ");

			            object o = i.Value;

                        if (o is Document)
                        {
                            AddTitle((o as Document).GetFullDocumentName(currentUser), fEncoded, ref sbValues);
                        }
                        else if (o is Entity)
                        {
                            AddTitle((o as Entity).Name, fEncoded, ref sbValues);
                        }
                        else if (o is Kesco.Lib.Entities.Item)
			            {
                            AddTitle(((Kesco.Lib.Entities.Item)o).Value.ToString(), fEncoded, ref sbValues);
			            }
			            else
			            {
			                var to = o.GetType();
			                var oText = to.GetProperty(sc.ValueField).GetValue(o, null);
			                if (null != oText)
			                {
                                AddTitle(oText.ToString(), fEncoded, ref sbValues);
			                }
			            }
			        }
			    }
			    else
			    {
			        var document = sc as DBSDocument;
                    if (document != null && document.AdvIcons != null)
			        {
                        foreach (var icon in document.AdvIcons)
			            {
                            sbValues.Append(icon);
			            }
			        }

                    AddTitle(sc.ValueText, fEncoded, ref sbValues);
			    }
			}
			else
                AddTitle(ctrl.Value, fEncoded, ref sbValues);

			return sbValues.ToString();
		}

        private static void AddTitle(string valueText, bool eEncoded, ref StringBuilder sbValues)
        {
            if (eEncoded)
                HttpUtility.HtmlEncode(sbValues.Append(valueText));
            else
                sbValues.Append(valueText);
        }


		/// <summary>
		/// Метод возвращает-суммарное описание данных для заголовка секции для группы элементов
		/// </summary>
		/// <param name="ctrls">Группа элементов для которых составляется описание</param>
		/// <returns>Строковое описание</returns>
        public string GetSectionHtmlDescription(IEnumerable<V4Control> ctrls, bool newLine = false)
		{
			const string sep = "; ";

			StringBuilder sbDescription = new StringBuilder();

			foreach (V4Control ctrl in ctrls)
			{
				if (null == ctrl) continue;

				string textOfControl = GetCtrlText(ctrl, true, CurrentUser);

				if (textOfControl.Length < 1)
				{
					if (!ctrl.IsRequired) continue;

                    textOfControl = (ctrl.IsReadOnly) ? SectionTitles.RequiredInfo : SectionTitles.RequiredRedInfo;
				}
				else
				{
                    if (ctrl.IsDisabled)
				        textOfControl = "";
				}

                if (textOfControl.Length > 0 && FieldLabels.ContainsKey(ctrl))
			    {
                    if (sbDescription.Length > 0)
                    {
                        sbDescription.Append(newLine ? "<br/>" : sep);
                    }

			        if (FieldLabels[ctrl].Contains(SectionTitles.GO) ||
			            FieldLabels[ctrl].Contains(SectionTitles.GP) ||
			            FieldLabels[ctrl].Contains(SectionTitles.Shipper) ||
			            FieldLabels[ctrl].Contains(SectionTitles.Payer)
			            )
			        {
			            sbDescription.AppendFormat("{0}", textOfControl);
			        }
			        else
			        {
    	                sbDescription.AppendFormat("{0} {1}", FieldLabels[ctrl], textOfControl);
			        }
			    }
			}

			return sbDescription.ToString();
		}

		/// <summary>
		/// Метод возвращает-суммарное описание данных для заголовка секции для групп элементов
		/// Для каждой группы формируется отдельная строка в описании
		/// </summary>
		/// <param name="ctrls">Массив групп элеметов</param>
		/// <returns>Описание данных для заголовка</returns>
		string CreateTitleMultiLineDescription(IEnumerable<V4Control>[] ctrls, bool newLine = false)
		{
			StringBuilder sb = new StringBuilder();

			//Уведомление добавляется как <div> элемент, поэтому после него не требуется добавлять <br/> перед следующим элементом
			bool fNtfAdded = false;
			foreach (IEnumerable<V4Control> ctrl_array in ctrls)
			{
                string dataInfo = GetSectionHtmlDescription(ctrl_array, newLine);

				using (StringWriter ntfText = new StringWriter())
				{
					string strNtfInfo = string.Empty;

				    foreach (V4Control ctrl in ctrl_array)
				    {
				        ctrl.RenderNtf(ntfText);
				    }

				    if (dataInfo.Length > 0 && ntfText.GetStringBuilder().Length > 0)
				    {
				        var ntfTextStr = ntfText.ToString();
				        if (ntfTextStr.Contains("<a"))
				        {
				            var beginStartTeg = ntfTextStr.IndexOf("<a");
                            var endStartTeg = ntfTextStr.IndexOf(">", beginStartTeg + 1);
				            var beginEndTeg = ntfTextStr.IndexOf("</a>",beginStartTeg + 4);
                            strNtfInfo = ntfTextStr.Substring(0, beginStartTeg) + 
                                ntfTextStr.Substring(endStartTeg + 1, beginEndTeg - endStartTeg - 1) +
                                ntfTextStr.Substring(beginEndTeg + 4);
                        }
				        else
				        {
                            strNtfInfo = ntfText.ToString();    
				        }
				    }

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
                var textOfControl1 = GetCtrlText(ctrls2[i], true, CurrentUser);
                var textOfControl2 = GetCtrlText(ctrls2[i + 1], true, CurrentUser);

				if (textOfControl1.Length < 1 && textOfControl2.Length < 1) continue;
                sbDescription2.Append("<br/>");
                
				if (textOfControl1.Length > 0) sbDescription2.Append(FieldLabels[ctrls2[i]] + " " + textOfControl1);
			    if (textOfControl2.Length > 0)
			    {
			        if (textOfControl1.Length > 0) sbDescription2.Append(", ");
    	            sbDescription2.Append(FieldLabels[ctrls2[i + 1]] + " " + textOfControl2);
			    }
			}

			if (sbDescription2.Length > 0)
			{
				//if (innerHtml.Length > 0) innerHtml.Append("<br/>");
				innerHtml.Append(" "+SectionTitles.SignersOfPaper + ":");
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

			return CreateTitleMultiLineDescription(ctrls, true);
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

        #endregion

        #region BeforeSearch

        private void StoreShipper_BeforeSearch(object sender)
        {
            if (Document.PostavschikField.Value != null)
                DBSShipperStore.Filter.ManagerId.Value = Document.PostavschikField.Value.ToString();
            DBSShipperStore.Filter.StoreTypeId.CompanyHowSearch = "0";
            DBSShipperStore.Filter.StoreTypeId.Value = "-1,21,22,23";
            DBSShipperStore.Filter.ValidAt.Value = Doc.Date == DateTime.MinValue ? "" : Doc.Date.ToString("yyyyMMdd");
        }

        private void StorePayer_BeforeSearch(object sender)
        {
            if (Document.PlatelschikField.Value != null)
                DBSPayerStore.Filter.ManagerId.Value = Document.PlatelschikField.Value.ToString();
            DBSPayerStore.Filter.StoreTypeId.CompanyHowSearch = "0";
            DBSPayerStore.Filter.StoreTypeId.Value = "-1,21,22,23";
            DBSPayerStore.Filter.ValidAt.Value = Doc.Date == DateTime.MinValue ? "" : Doc.Date.ToString("yyyyMMdd");
        }

        private void Contract_BeforeSearch(DBSDocument ctrl, DBSDocument link)
        {
            Contract.Filter.Type.DocTypeParams.AddRange(GetControlTypeFilter(Document.DogovorField.DocFieldId));
            LinkedDoc_BeforeSearch(ctrl, link);
        }

        private void Enclosure_BeforeSearch(DBSDocument ctrl, DBSDocument link)
        {
            Enclosure.Filter.Type.DocTypeParams.AddRange(GetControlTypeFilter(Document.PrilozhenieField.DocFieldId));
            LinkedDoc_BeforeSearch(ctrl, link);
        }

        private void LetterOfCredit_BeforeSearch(object sender)
        {
            LetterOfCredit.Filter.Type.DocTypeParams.AddRange(GetControlTypeFilter(Document.AkkredField.DocFieldId));
        }

        private void ApplicationForPurchasing_BeforeSearch(object sender)
        {
            ApplicationForPurchasing.Filter.Type.DocTypeParams.AddRange(GetControlTypeFilter(Document.ZvkBField.DocFieldId));
            //ApplicationForPurchasing.Filter.NextType.Value = Document.TypeID.ToString();
            //ApplicationForPurchasing.Filter.NextType.FieldId = Document.ZvkBField.DocFieldId;
        }

        private void BillOfLading_BeforeSearch(object sender)
        {
            BillOfLading.Filter.Type.DocTypeParams.AddRange(GetControlTypeFilter(Document.BillOfLadingField.DocFieldId));
            //BillOfLading.Filter.NextType.Value = Document.TypeID.ToString();
            //BillOfLading.Filter.NextType.FieldId = Document.BillOfLadingField.DocFieldId;
        }

        private void Invoice_BeforeSearch(object sender)
        {
            Invoice.Filter.Type.DocTypeParams.AddRange(GetControlTypeFilter(Document.SchetPredField.DocFieldId));
        }

        private void PaymentDocuments_BeforeSearch(object sender)
        {
            PaymentDocuments.Filter.Type.DocTypeParams.AddRange(GetControlTypeFilter(Document.PlatezhkiField.DocFieldId));
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
				    LinkedDocParam lp1 = new LinkedDocParam {QueryType = LinkedDocsType.DirectReasons, DocID = i.Id};
				    LinkedDocParam lp2 = new LinkedDocParam {QueryType = LinkedDocsType.DirectСonsequences, DocID = i.Id};

				    ctrl.Filter.LinkedDoc.LinkedDocParams.Add(lp1);
					ctrl.Filter.LinkedDoc.LinkedDocParams.Add(lp2);
				}

				return;
			}

			if (string.IsNullOrEmpty(link.Value)) return;

            LinkedDocParam lp3 = new LinkedDocParam {QueryType = LinkedDocsType.DirectReasons, DocID = link.Value};
            LinkedDocParam lp4 = new LinkedDocParam {QueryType = LinkedDocsType.DirectСonsequences, DocID = link.Value};

            ctrl.Filter.LinkedDoc.LinkedDocParams.Add(lp3);
			ctrl.Filter.LinkedDoc.LinkedDocParams.Add(lp4);
		}

        void CarTtn_BeforeSearch(DBSDocument ctrl)
		{
            ctrl.Filter.Type.DocTypeParams.AddRange(GetControlTypeFilter(Document.TTNTrField.DocFieldId));
            ctrl.Filter.NextType.Value = Document.TypeID.ToString();
            ctrl.Filter.NextType.FieldId = Document.TTNTrField.DocFieldId;
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
        /// Универсальный обработчик события установки фильтра поиска сотрудника
        /// </summary>
        /// <param name="sender"></param>
        protected void Person_BeforeSearch(object sender)
        {
            DBSPerson.Filter.PersonType = 2;
            DBSPerson.Filter.PersonValidAt = Document.Date;
            DBSPerson.Filter.PersonLink = Document.PostavschikField.ValueInt;
            DBSPerson.Filter.PersonLinkType = 1;
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
			sender.GetFilter().ValidAt.Value = Doc.Date==DateTime.MinValue?"": Doc.Date.ToString("yyyyMMdd");
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
            sender.GetFilter().ValidAt.Value = Doc.Date == DateTime.MinValue ? "" : Doc.Date.ToString("yyyyMMdd");
		}

        /// <summary>
        /// Универсальный обработчик события установки фильтра поиска сотрудника
        /// </summary>
        /// <param name="sender"></param>

        protected void Person_Changed(object sender, ProperyChangedEventArgs e)
        {
            DBSPosition.IsDisabled = false;
            if (e.OldValue.SafeToString().Equals(e.NewValue.SafeToString()) || e.NewValue.SafeToString().Length == 0)
            {
                DBSPosition.Value = null;
                DBSPosition.IsDisabled = true;
            }
            else
            {
                DBSPosition.OnBeforeSearch();
                var pl = DBSPosition.GetPersonLinks();
                if (pl.Count == 1)
                {
                    DBSPosition.IsDisabled = false;
                    DBSPosition.Value = pl[0].Id;
                }
            }
        }

        /// <summary>
        /// Универсальный обработчик события установки фильтра поиска должности
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="type">Тип должности</param>
        protected void PersonPosition_BeforeSearch(object sender, string type)
        {
            DBSPosition.Filter.LinkTypeID.Value = "1";
            DBSPosition.Filter.ChildID.Value = DBSPerson.Value;
            DBSPosition.Filter.ParentID.Value = Shipper.Value;
            switch (type)
            {
                case "Director":
                    DBSPosition.Filter.Parametr.Value = "1";
                    break;
                case "Accountant":
                    DBSPosition.Filter.Parametr.Value = "2";
                    break;
                case "StoreKeeper":
                    DBSPosition.Filter.Parametr.Value = "";
                    break;
            }
        }

        /// <summary>
        /// Универсальный обработчик события установки фильтра поиска докуменета
        /// </summary>
        /// <param name="sender"></param>
        protected void Document_BeforeSearch(object sender)
        {
            DBSDocument dbsdoc = sender as DBSDocument;
            if (null == dbsdoc) return;

            //фильтр по дате документа
            dbsdoc.Filter.Date.Value = Doc.Date.ToString("yyyyMMdd");
            dbsdoc.Filter.Date.DateSearchType = DateSearchType.LessThan;

            //фильтр по лицам документа
            dbsdoc.Filter.PersonIDs.Clear();
            if (!string.IsNullOrEmpty(Shipper.Value))
                dbsdoc.Filter.PersonIDs.Add(Shipper.Value);

            if (!string.IsNullOrEmpty(Payer.Value))
                dbsdoc.Filter.PersonIDs.Add(Payer.Value);

            dbsdoc.Filter.PersonIDs.UseAndOperator = true;

            //фильтр по договору
            dbsdoc.Filter.LinkedDoc.Clear();
            if (!string.IsNullOrEmpty(Contract.Value))
            {
                dbsdoc.Filter.LinkedDoc.Add(new LinkedDocParam { DocID = Contract.Value, QueryType = LinkedDocsType.AllReasons });
                dbsdoc.Filter.LinkedDoc.Add(new LinkedDocParam { DocID = Contract.Value, QueryType = LinkedDocsType.AllСonsequences });
            }

        }

        #endregion

        #region OnRenderNtf
        /// <summary>
		/// Универсальный обработчик события запроса уведомительного сообщения для лица
		/// </summary>
		/// <param name="ntf">Элемент отображающий уведомительное сообщение</param>
        /// <param name="personCtrl">Объект для которого формируется уведомительное сообщение</param>
        /// <param name="prefix">Тип объекта</param>
        public void Person_OnRenderNtf(Ntf ntf, DBSPerson personCtrl, string prefix)
		{
			ntf.Clear();

			if (string.IsNullOrEmpty(personCtrl.Value)) return;

            var p = GetObjectById(typeof(PersonOld), personCtrl.Value) as PersonOld;
            if (p == null || p.Unavailable)
            {
                // лицо не доступно
                ntf.Add(Resx.GetString("_Msg_ЛицоНеДоступно"), NtfStatus.Error);
                return;
            }

            if (!p.IsChecked)
            {
                // лицо не проверено
                ntf.Add(Resx.GetString("TTN_ntfPersonNotTested"), NtfStatus.Error);
            }

            var documentDate = Doc.Date == DateTime.MinValue ? DateTime.Today : Doc.Date;
            var crd = GetCardById(p, documentDate);

            if (crd != null)
            {
                if (crd.NameLat.Length == 0 && crd.NameRus.Length == 0)
                {
                    // у лица отсутствует краткое название
                    ntf.Add(Resx.GetString("_Msg_ЛицоНазвание"), NtfStatus.Error);
                }
                
                if (prefix == "Shipper" || prefix == "Payer")
                {
                    if (crd.АдресЮридический.Length == 0 && crd.АдресЮридическийЛат.Length == 0)
                    {
                        // у лица отсутствует юридический адрес
                        ntf.Add(Resx.GetString("_Msg_ЛицоАдресЮридический"), NtfStatus.Error);
                    }
                }
            }
            else
            {
                ntf.Add(ControlNtfs.PersonDate, NtfStatus.Error);
            }

            if (prefix == "Shipper" || prefix == "Payer")
            {
                var contract = string.IsNullOrWhiteSpace(Contract.Value)
                    ? null
                    : GetObjectById(typeof (Dogovor), Contract.Value) as Dogovor;
                if (contract != null)
                {
                    if (contract.GetPersonIndex(personCtrl.Value) <= (contract.DataUnavailable ? -1 : 0))
                    {
                        // лицо не соответствует ни одному лицу в договоре
                        ntf.Add(Resx.GetString("TTN_nntfPersonNotMatchContract"), NtfStatus.Error);
                    }
                }

                var enclosure = string.IsNullOrWhiteSpace(Enclosure.Value)
                    ? null
                    : GetObjectById(typeof (Dogovor), Enclosure.Value) as Dogovor;
                if (enclosure != null)
                {
                    if (enclosure.GetPersonIndex(personCtrl.Value) <= (enclosure.DataUnavailable ? -1 : 0))
                    {
                        // лицо не соответствует ни одному лицу в приложении
                        ntf.Add(Resx.GetString("TTN_nntfPersonNotMatchEnclosure"), NtfStatus.Error);
                    }
                }
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

            if (store == null) return;

            if (store.Unavailable)
            {
                // расчетный счет не доступен
                ntf.Add(Resx.GetString("TTN_msgAccountNotAvailable"), NtfStatus.Error);
                return;
            }

            if (!string.IsNullOrEmpty(Currency.Value) && Currency.ValueInt != store.ResourceId)
            {
                // валюта банковского счета не соответствует валюте документа
                ntf.Add(Resx.GetString("TTN_ntfCurrencyDocument"), NtfStatus.Error);
            }

            var documentDate = Document.Date == DateTime.MinValue ? DateTime.Today : Document.Date;
            if (!store.IsAlive(documentDate))
            {
                // расчетный счет не действует на дату документа
                ntf.Add(Resx.GetString("TTN_ntfStoreAccountDate"), NtfStatus.Error);
            }

            if (prefix == "Shipper")
            {
                var contract = Document.Dogovor;
                if (contract != null && !contract.DataUnavailable)
                {
                    if ((personCtrl.Value == contract.Person1Field.Value.ToString() &&
                          contract.Sklad1Field.ValueString.Length>0 && contract.Sklad1Field.ValueString != store.Id) ||
                        (personCtrl.Value == contract.Person2Field.Value.ToString() &&
                         contract.Sklad2Field.ValueString.Length > 0 && contract.Sklad2Field.ValueString != store.Id))
                    {
                        // расчетный счет не соответствует счету в договоре
                        ntf.Add(Resx.GetString("TTN_ntfPersonStoreDocument"), NtfStatus.Error);
                    }
                }

                var enclosure = string.IsNullOrWhiteSpace(Enclosure.Value)
                    ? null
                    : GetObjectById(typeof (Dogovor), Enclosure.Value) as Dogovor;
                if (enclosure != null && !enclosure.DataUnavailable)
                {
                    if ((personCtrl.Value == enclosure.Person1Field.Value.ToString() &&
                         enclosure.Sklad1Field.ValueString.Length > 0 && enclosure.Sklad1Field.ValueString != store.Id) ||
                        (personCtrl.Value == enclosure.Person2Field.Value.ToString() &&
                         enclosure.Sklad2Field.ValueString.Length > 0 && enclosure.Sklad2Field.ValueString != store.Id))
                    {
                        // расчетный счет не соответствует счету в приложении
                        ntf.Add(Resx.GetString("TTN_ntfPersonStoreEnclosure"), NtfStatus.Error);
                    }
                }
            }

            if (store.KeeperId > 0)
            {
                var p = GetObjectById(typeof(PersonOld), store.KeeperId.ToString()) as PersonOld;

                if (p == null)
                {
                    return;
                }

                if (!p.IsChecked)
                {
                    // банк не проверен
                    ntf.Add(Resx.GetString("_Msg_СчетБанкНеПроверен"), NtfStatus.Error);
                }

                //var card = p.GetCard(documentDate);
                var card = GetCardById(p, Doc.Date == DateTime.MinValue ? DateTime.Today : Doc.Date);
                if (card == null)
                {
                    // у банка отсутствует реквизиты на
                    ntf.Add(string.Format(Resx.GetString("_Msg_СчетРеквизитыБанка"), documentDate.ToString("dd.MM.yyyy")), NtfStatus.Error);
                }
            }

            if (personCtrl.ValueInt.HasValue && store.ManagerId != personCtrl.ValueInt)
                ntf.Add(ControlNtfs.PersonStore[prefix], NtfStatus.Error);

		}

        /// <summary>
        ///     Событие, валидирующее значение конрола Дата проводки
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="ntf">Класс нотификации</param>
        protected void DateOfPosting_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();

            if (string.IsNullOrEmpty(DateOfPosting.Value)) return;
            if (!DateOfPosting.ValueDate.HasValue) return;
            if (Doc.Date == default(DateTime)) return;

            if (DateOfPosting.ValueDate < Doc.Date.Date)
                ntf.Add(ControlNtfs.PostingOnDate, NtfStatus.Error);
        }

        /// <summary>
        ///     Событие, валидирующее значение конрола Руководитель\Бухгалтер\Отпуск груза произвел
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="ntf">Класс нотификации</param>
        protected void Position_OnRenderNtf(object sender, Ntf ntf)
        {
            //ntf.Clear();
            //if (string.IsNullOrEmpty(DBSPerson.Value)) return;
            //if (string.IsNullOrEmpty(DBSPosition.Value)) return;
        }

        public void Currency_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();

            var contract = string.IsNullOrWhiteSpace(Contract.Value) ? null : GetObjectById(typeof(Dogovor), Contract.Value) as Dogovor;

            if (!Currency.Value.IsNullEmptyOrZero() && contract != null && !contract.Valyuta.IsNullEmptyOrZero())
            if (Currency.Value != contract.Valyuta)
            {
                ntf.Add(ControlNtfs.CurrencyContract, NtfStatus.Error);
            }
        }

        /// <summary>
        ///     Событие, валидирующее значение конрола Платежные документы
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="ntf">Класс нотификации</param>
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

        /// <summary>
        ///     Событие, валидирующее значение конрола Приложение
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="ntf">Класс нотификации</param>
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

        /// <summary>
        ///     Событие, валидирующее значение конрола Документ
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="ntf">Класс нотификации</param>
        protected void Document_OnRenderNtf(object sender, Ntf ntf)
        {
            var dbsdoc = sender as DBSDocument;
            if (null == dbsdoc) return;

            ntf.Clear();

            if (string.IsNullOrEmpty(dbsdoc.Value)) return;
            if (Doc.Date == default(DateTime)) return;
            
            var d = GetObjectById(typeof(Document), dbsdoc.Value) as Document;

            if (null == d) return;

            if (d.DataUnavailable)
                ntf.Add(ControlNtfs.DocumentForm, NtfStatus.Error);
            else
            {
                if (dbsdoc.ID=="Contract" && Document.DogovorField.ValueString.Length != 0)
                {
                    var dog = Document.Dogovor;
                    if (dog != null && dog.KuratorField.ValueString.Length > 0)
                    {
                        var k = GetObjectById(typeof(Employee), dog.KuratorField.ValueString) as Employee;
                        var w  = new StringWriter();
                        w.Write(SectionTitles.Curator + ": ");
                        RenderLinkEmployee(w, "linkKurator", k, NtfStatus.Information);
                        ntf.Add(w.ToString(), NtfStatus.Information);
                    }
                }
            }
            if (d.Date == default(DateTime)) return;

            if (d.Date > Doc.Date)
                ntf.Add(ControlNtfs.BaseDocumentOnDate, NtfStatus.Error);
            
            if (d.GetPersonIndex(Shipper.Value) <= (d.DataUnavailable ? -1 : 0) ||
                d.GetPersonIndex(Payer.Value) <= (d.DataUnavailable ? -1 : 0))
                    ntf.Add(ControlNtfs.PersonDocument, NtfStatus.Error);
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
        /// Склад поставщика
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ntf"></param>
        private void StoreShipper_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            var documentDate = Document.Date == DateTime.MinValue ? DateTime.Today : Document.Date;

            if (DBSShipperStore.Value.Length == 0)
            {
                if (Document.PostavschikField.Value != null)
                {
                    var p = GetObjectById(typeof(PersonOld), Document.PostavschikField.Value.ToString()) as PersonOld;

                    if (p == null || (!p.Unavailable && p.BusinessProjectID > 0))
                        DBSShipperStore.NtfNotValidData();
                }

                return;
            }

            var s = GetObjectById(typeof(Store), DBSShipperStore.Value) as Store;
            if (s == null || s.Unavailable)
            {
                ntf.Add(Resx.GetString("TTN_ntfWarehouseNotAvailable"), NtfStatus.Error);
                return;
            }

            if (s.KeeperId > 0)
            {
                var p = GetObjectById(typeof(PersonOld), s.KeeperId.ToString()) as PersonOld;

                if (p == null)
                {
                    ntf.Add(Resx.GetString("TTN_ntfKeeperNotFound"), NtfStatus.Error);
                    return;
                }

                if (!p.IsChecked)
                {
                    ntf.Add(Resx.GetString("TTN_ntfKeeperNotVerified"), NtfStatus.Error);
                }

                //var card = p.GetCard(documentDate);
                var card = GetCardById(p, documentDate);
                if (card == null)
                {
                    ntf.Add(
                        string.Format(Resx.GetString("TTN_ntfKeeperNoDataOn") + " " +
                                      documentDate.ToString("dd.MM.yyyy")), NtfStatus.Error);
                    DBSShipperStore.ValueText = (s.IBAN.Length > 0 ? s.IBAN : s.Name) + " в " + p.Name;
                }
                else
                    DBSShipperStore.ValueText = (s.IBAN.Length > 0 ? s.IBAN : s.Name) + " в " +
                                               (card.NameRus.Length > 0 ? card.NameRus : card.NameLat);
            }

            if (s.Id.Length > 0 && !s.Unavailable && !s.ManagerId.Equals(Document.PostavschikField.Value))
            {
                ntf.Add(Resx.GetString("TTN_ntfWarehouseNotMatchSupplier"), NtfStatus.Error);
            }

            if (!s.IsAlive(Document.Date))
            {
                ntf.Add(Resx.GetString("STORE_IsNotActual").ToLower(), NtfStatus.Error);
            }
        }

        /// <summary>
        /// Склад плательщика
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ntf"></param>
        private void StorePayer_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            var documentDate = Document.Date == DateTime.MinValue ? DateTime.Today : Document.Date;

            if (DBSPayerStore.Value.Length == 0)
            {
                if (Document.PlatelschikField.Value != null)
                {
                    var p = GetObjectById(typeof(PersonOld), Document.PlatelschikField.Value.ToString()) as PersonOld;
                    if (p == null || (!p.Unavailable && p.BusinessProjectID > 0))
                        DBSPayerStore.NtfNotValidData();
                }

                return;
            }

            var s = GetObjectById(typeof(Store), DBSPayerStore.Value) as Store;
            if (s == null || s.Unavailable)
            {
                DBSPayerStore.ValueText = "#" + DBSPayerStore.Value;
                ntf.Add(Resx.GetString("TTN_ntfWarehouseNotAvailable"), NtfStatus.Error);
                return;
            }

            if (s.KeeperId > 0)
            {
                var p = GetObjectById(typeof(PersonOld), s.KeeperId.ToString()) as PersonOld;
                if (p == null)
                {
                    ntf.Add(Resx.GetString("TTN_ntfKeeperNotFound"), NtfStatus.Error);
                    return;
                }

                if (!p.IsChecked)
                {
                    ntf.Add(Resx.GetString("TTN_ntfKeeperNotVerified"), NtfStatus.Error);
                }

                //var card = p.GetCard(documentDate);
                var card = GetCardById(p, documentDate);
                if (card == null)
                {
                    ntf.Add(
                        string.Format(Resx.GetString("TTN_ntfKeeperNoDataOn") + " " +
                                      documentDate.ToString("dd.MM.yyyy")), NtfStatus.Error);
                    DBSPayerStore.ValueText = (s.IBAN.Length > 0 ? s.IBAN : s.Name) + " " + Resx.GetString("msgIN") + " " +
                                             p.Name;
                }
                else
                    DBSPayerStore.ValueText = (s.IBAN.Length > 0 ? s.IBAN : s.Name) + " " + Resx.GetString("msgIN") + " " +
                                             (card.NameRus.Length > 0 ? card.NameRus : card.NameLat);
            }

            if (s.Id.Length > 0 && !s.Unavailable && !s.ManagerId.Equals(Document.PlatelschikField.Value))
            {
                ntf.Add(Resx.GetString("TTN_ntfWarehouseNotMatchPayer"), NtfStatus.Error);
            }

            if (!s.IsAlive(Document.Date))
            {
                ntf.Add(Resx.GetString("STORE_IsNotActual").ToLower(), NtfStatus.Error);
            }
        }

        #endregion

        #region Changed

        /// <summary>
        ///     Событие, отслеживающее изменение контрола признака корректирующего документа
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void CorrectableFlag_Changed(object sender, ProperyChangedEventArgs e)
		{
            if (SequelFakturaExists())
            {
                CorrectableFlag.Value = CorrectableFlag.Value.Equals("1") ? "0" : "1";
                //Невозможно изменять связь с корректируемым документом реализации при наличии вытекающего счета-фактуры.
                //Сперва удалите вытекающий счет-фактуру.
                ShowMessage(Resx.GetString("TTN_msgFakturaExist"), Resx.GetString("errDoisserWarrning"));
                return;
            }

            if (CorrectableFlag.Checked)
            {
                CorrectableTtn.IsDisabled = false;
                FlagCorrecting_Uncheck(true, null);
                /*
                //Во все поля формы будут установлены значения из корректируемого документа. Продолжить?
                ShowConfirm(Resx.GetString("TTN_mgsReplaceValues"), 
                    Resx.GetString("errDoisserWarrning"),
                    Resx.GetString("CONFIRM_StdCaptionYes"),
                    Resx.GetString("CONFIRM_StdCaptionNo"),
                    "cmd('cmd', 'ShowCorrectingDoc');",
                    "cmd('cmd', 'FlagCorrecting_Uncheck');",
                    null, null
                    );
                */
            }
            else
            {
                if (Document._CorrectingDoc.Length > 0)
                {
                    //Вы уверены, что хотите очистить ссылку на корректируемый документ
                    ShowConfirm(Resx.GetString("MSG_ConfirmUnlinkCorrected"),
                        Resx.GetString("errDoisserWarrning"),
                        Resx.GetString("CONFIRM_StdCaptionYes"),
                        Resx.GetString("CONFIRM_StdCaptionNo"),
                        "cmd('cmd', 'FlagCorrecting_Uncheck');",
                        "cmd('cmd', 'ShowCorrectingDoc', 'oldValue', '" + CorrectableTtn.Value + "');",
                        null, null
                        );
                }
                else
                {
                    FlagCorrecting_Uncheck(false);
                }
            }

		}

        /// <summary>
        ///     Событие, отслеживающее изменение контрола корректирующего документа
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void CorrectableTtn_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (e != null)
            {
                if (SequelFakturaExists())
                {
                    CorrectingDoc_InfoClear(e.OldValue);
                    //Невозможно изменять связь с корректируемым документом реализации при наличии вытекающего счета-фактуры.
                    //Сперва удалите вытекающий счет-фактуру.
                    ShowMessage(Resx.GetString("TTN_msgFakturaExist"), Resx.GetString("errDoisserWarrning"));
                    CorrectableTtn.IsRequired = (CorrectableFlag.Value == "1");
                    return;
                }

                if (CorrectableTtn.ValueText != "")
                {
                    if (CorrectableTtn.ValueInt.ToString() == Document.Id)
                    {
                        CorrectingDoc_InfoClear(e.OldValue);
                        //В качестве корректируемого документа выбран текущий документ. Выберите документ, отличный от текущего.
                        ShowMessage(Resx.GetString("TTN_msgChosenCorrectedSelf"), Resx.GetString("errDoisserWarrning"));
                        CorrectableTtn.IsRequired = (CorrectableFlag.Value == "1");
                        return;
                    }

                    // Проверка того, что выбранный документ уже откорректирован. В этом случае предлагается заменить документ на вытекающую из него корректирующую ТТН
                    var addMessage = "";
                    var correctingNakl = new Lib.Entities.Documents.EF.Trade.TTN(e.NewValue);
                    var col = correctingNakl.GetSequelDocs(Convert.ToInt32(Document.CorrectingDocField.Id));
                    if (col.Count > 1)
                    {
                        ShowMessage(Resx.GetString("TTN_msg_Больше1Корректирующего"), Resx.GetString("errDoisserWarrning"));
                        CorrectingDoc_InfoClear(e.OldValue);
                        return;
                    }

                    if (col.Count == 1 && col[0].Id != Document.Id)
                    {
                        var original = CorrectableTtn.ValueText;
                        do
                        {
                            //Document._CorrectingDoc = col[0].Id;
                            CorrectingDoc_InfoClear(col[0].Id);
                            correctingNakl = new Lib.Entities.Documents.EF.Trade.TTN(col[0].Id);
                            col = correctingNakl.GetSequelDocs(Convert.ToInt32(Document.CorrectingDocField.Id));
                        } while (col.Count >= 1 && col[0].Id != Document.Id);

                        addMessage = string.Format(Resx.GetString("MSG_DocAlreadyCorrected") + "! " + Resx.GetString("MSG_LastDocAsCorrected"), original, CorrectableTtn.ValueText);
                    }
                    else
                    {
                        addMessage = Resx.GetString("MSG_ChosenCorrected");
                    }

                    //В качестве корректируемого документа Вы указали документ ...
                    //В результате этой операции ВСЕ данные текущего документа будут приведены в соответствие с корректируемым документом.
                    //Вы уверены, что хотите выполнить данную операцию?
                    ShowConfirm(
                        String.Format(addMessage + " " + Resx.GetString("MSG_DataMergedWithCorrected"), CorrectableTtn.ValueText),
                        Resx.GetString("errDoisserWarrning"),
                        Resx.GetString("CONFIRM_StdCaptionYes"),
                        Resx.GetString("CONFIRM_StdCaptionNo"),
                        "cmd('cmd', 'SetCorrectableDocument', 'value', 'true', 'oldValue', '" + e.OldValue + "');",
                        "cmd('cmd', 'SetCorrectableDocument', 'value', 'false', 'oldValue', '" + e.OldValue + "');",
                        null, 405);
                }
                else
                {
                    //Вы уверены, что хотите очистить ссылку на корректируемый документ
                    ShowConfirm(Resx.GetString("MSG_ConfirmUnlinkCorrected"),
                        Resx.GetString("errDoisserWarrning"),
                        Resx.GetString("CONFIRM_StdCaptionYes"),
                        Resx.GetString("CONFIRM_StdCaptionNo"),
                        "cmd('cmd', 'FlagCorrecting_Uncheck');",
                        "cmd('cmd', 'ShowCorrectingDoc', 'oldValue', '" + e.OldValue + "');",
                        null, null
                        );
                }
            }
        }

        /// <summary>
        /// Установка значения корректируемого документа
        /// </summary>
        /// <param name="value">Id документа (или "")</param>
        private void CorrectingDoc_InfoClear(string value)
        {
            CorrectableTtn.Value = value;
            Document._CorrectingDoc = value;
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола вылюты
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
		protected void Currency_Changed(object sender, ProperyChangedEventArgs e)
		{
			_goPanel.OnCurrencyChanged();
			_gpPanel.OnCurrencyChanged();
			_shipperPanel.OnCurrencyChanged();
			_payerPanel.OnCurrencyChanged();
		}

        /// <summary>
        ///     Событие, отслеживающее изменение контрола выбора продукта
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void ProductSelect_Changed(object sender, ProperyChangedEventArgs e)
        {
            FillMrisDataGrid(false);
        }

        private void SetStoreRequired()
        {
            // если наш, то поле Склад поставщика на закладке товары обязятельно
            DBSShipperStore.IsRequired = false;
            if (Document.PostavschikField.Value != null)
            {
                var shipper = GetObjectById(typeof(PersonOld), Document.PostavschikField.Value.ToString()) as PersonOld;
                if (shipper != null && shipper.BusinessProjectID > 0) DBSShipperStore.IsRequired = true;
            }

            DBSPayerStore.IsRequired = false;
            if (Document.PlatelschikField.Value != null)
            {
                var payer = GetObjectById(typeof(PersonOld), Document.PlatelschikField.Value.ToString()) as PersonOld;
                if (payer != null && payer.BusinessProjectID > 0) DBSPayerStore.IsRequired = true;
            }
            DBSShipperStore.RefreshRequired = true;
            DBSPayerStore.RefreshRequired = true;
        }

        protected void GO_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (GO.Value == GP.Value && !GO.Value.IsNullEmptyOrZero() && !GP.Value.IsNullEmptyOrZero())
            {
                _gpPanel.SetPerson(0, "", "");
            }
        }

        protected void GP_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (GO.Value == GP.Value && !GO.Value.IsNullEmptyOrZero() && !GP.Value.IsNullEmptyOrZero())
            {
                _goPanel.SetPerson(0, "", "");
            }
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола Поставщик
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Shipper_Changed(object sender, ProperyChangedEventArgs e)
        {
            ShipperOrPayer_Changed(sender, e);
            SetStoreRequired();

            var sqlParams = new Dictionary<string, object> { { "@КодТипаДокумента", DocTypeEnum.ТоварноТранспортнаяНакладная }
																, { "@КодЛица1", Shipper.Value }
                                                                , { "@КодДокумента", Contract.Value }
            };

            DataTable dt = DBManager.GetData(SQLQueries.SELECT_ПоследнийИзмДокументПоТипу, Config.DS_document, CommandType.Text, sqlParams);
            if (null == dt) return;

            if (dt.Rows.Count > 0)
            {
                var director = dt.Rows[0]["Text50_2"].ToString();
                if (director != "") Director.Value = director;

                var directorPosition = dt.Rows[0]["Text50_11"].ToString();
                if (directorPosition != "") DirectorPosition.Value = directorPosition;

                var accountant = dt.Rows[0]["Text50_3"].ToString();
                if (accountant != "") Accountant.Value = accountant;

                var accountantPosition = dt.Rows[0]["Text50_12"].ToString();
                if (accountantPosition != "") AccountantPosition.Value = accountantPosition;

                var storeKeeper = dt.Rows[0]["Text50_13"].ToString();
                if (storeKeeper != "") StoreKeeper.Value = storeKeeper;

                var storeKeeperPosition = dt.Rows[0]["Text100_3"].ToString();
                if (storeKeeperPosition != "") StoreKeeperPosition.Value = storeKeeperPosition;
            }

            // Если плательщик такой-же, то очищаем его
            if (Shipper.Value == Payer.Value && !Shipper.Value.IsNullEmptyOrZero() && !Payer.Value.IsNullEmptyOrZero())
            {
                _payerPanel.SetPerson(0, "", "");
            }

            DBSShipperStore.RenderNtf();
            JS.Write("SetImgDeleteVisible('{0}','{1}','{2}');", Director.Value, Accountant.Value, StoreKeeper.Value);
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола Плательщик
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Payer_Changed(object sender, ProperyChangedEventArgs e)
        {
            ShipperOrPayer_Changed(sender, e);
            SetStoreRequired();

            // Если поставщик такой-же, то очищаем его
            if (Shipper.Value == Payer.Value && !Shipper.Value.IsNullEmptyOrZero() && !Payer.Value.IsNullEmptyOrZero())
            {
                _shipperPanel.SetPerson(0, "", "");
            }

            DBSPayerStore.RenderNtf();
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола Поставщик или Плательщик
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void ShipperOrPayer_Changed(object sender, ProperyChangedEventArgs e)
        {
            DBSDocument[] docCtrls = { CarTtn, Contract, Enclosure, ApplicationForPurchasing, LetterOfCredit, BillOfLading, Invoice, PaymentDocuments };
            
            foreach (DBSDocument docCtrl in docCtrls)
            {
                docCtrl.RenderNtf();
            }
            
            SetAccordionHeader(SectionPrefixes.Documents + Nakladnaya.suffixTitle);
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола руководитель
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Position_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(DBSPosition.Value))
            {
                if (DBSPosition.TryFindSingleValue())
                {
                    DBSPosition.BindDocField.Value = DBSPosition.ValueInt;
                }
            }

            DBSPosition.RenderNtf();
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола приложение
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Enclosure_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(Contract.Value))
            {
                LinkedDoc_BeforeSearch(Contract, Enclosure);
                if (Contract.TryFindSingleValue())
                {
                    Contract.BindDocField.Value = Contract.ValueInt;
                    Contract_Changed(null, null);
                }
            }
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола договор
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Contract_Changed(object sender, ProperyChangedEventArgs e)
        {
            var strDocId = Contract.Value;
            if (string.IsNullOrEmpty(strDocId))
            {
                ContractInfo.Visible = false;
                ContractInfo.BindDocField.Value = null;
                return;
            }
            
            var d = GetObjectById(typeof(Dogovor), strDocId) as Dogovor;
            if (d == null || d.Unavailable)
            {
                ContractInfo.Visible = false;
                ContractInfo.BindDocField.Value = null;
                return;
            }

            var fUpdated = false;
            ContractInfo.Visible = true;
            ContractInfo.BindDocField.Value = d.GetFullDocumentName(CurrentUser);
            
            if (!d.IsDogovor)
                fUpdated = FillByDocument(d);
            else
            {
                if (FillByDogovor(d))
                    fUpdated = true;

                if (Currency.Value.IsNullEmptyOrZero())
                {
                    Currency.BindDocField.Value = d.Valyuta;
                }
            }
            
            if (fUpdated)
            {
                if (string.IsNullOrEmpty(PayerStore.Value))
                {
                    if (PayerStore.TryFindSingleValue())
                    {
                        PayerStore.BindDocField.Value = PayerStore.ValueInt;
                    }
                }

                if (string.IsNullOrEmpty(ShipperStore.Value))
                {
                    if (ShipperStore.TryFindSingleValue())
                    {
                        ShipperStore.BindDocField.Value = ShipperStore.ValueInt;
                    }
                }

                _shipperPanel.OnDocDateChanged();
                _payerPanel.OnDocDateChanged();
                
                TryFindGOGP();
                Contract.OnBeforeSearch();
            }

            if (Contract.Value.Length > 0 && Enclosure.Value.Length == 0)
            {
                LinkedDoc_BeforeSearch(Enclosure, Contract);
                if (Enclosure.TryFindSingleValue())
                {
                    Enclosure.BindDocField.Value = Enclosure.ValueInt;
                }
                if (Enclosure.Value.Length > 0) Enclosure.RenderNtf();
            }

            Currency_Changed(null, null);
            Currency.RenderNtf();
            ClientScripts.SendSetInnerHtml(this, SectionPrefixes.General + suffixTitle, GeneralDataGetTitle());
            
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола Склада поставщика
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void ShipperStore_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (Document.PositionMris != null && Document.PositionMris.Count > 0 && DBSShipperStore.Value != string.Empty)
            {
                Document.PositionMris.ForEach(delegate(Mris p)
                {
                    if (p.ShipperStoreId != int.Parse(DBSShipperStore.Value))
                    {
                        p.ShipperStoreId = int.Parse(DBSShipperStore.Value);
                        if (!p.Id.IsNullEmptyOrZero()) p.Save(false);
                    }
                });
            }
            DBSShipperStore.RenderNtf();
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола Склада плательщика
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void PayerStore_Changed(object sender, EventArgs e)
        {
            if (Document.PositionMris != null && Document.PositionMris.Count > 0 && DBSPayerStore.Value != string.Empty)
            {
                Document.PositionMris.ForEach(delegate(Mris p)
                {
                    if (p.PayerStoreId != int.Parse(DBSPayerStore.Value))
                    {
                        p.PayerStoreId = int.Parse(DBSPayerStore.Value);
                        if (!p.Id.IsNullEmptyOrZero()) p.Save(false);
                    }
                });
            }

            DBSPayerStore.RenderNtf();
        }

        protected void GoAddress_Changed()
        {
            _goPanel.SetAddress(GoAddress.Value);
            _goPanel.BindFieldsByPerson();
        }

        protected void GpAddress_Changed()
        {
            _gpPanel.SetAddress(GpAddress.Value);
            _gpPanel.BindFieldsByPerson();
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
                //link.Filter.LinkedDocument.LinkedDocParams.Clear();

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
                    link.SelectedItems.AddRange(dt.AsEnumerable().Select((row) => { string id = ((int)row["КодДокумента"]).ToString(); return new Kesco.Lib.Entities.Item() { Id = id, Value = link.GetObjectById(id) }; }));

                    //Инициализация перерисовки элемента управления
                    link.SetPropertyChanged("IsReadOnly");
                    link.Flush();
                }
            }
        }

        /// <summary>
        /// Событие изменения документа Счет, инвойс, проформа
        /// </summary>
        protected void Invoice_ValueChanged(object sender, ProperyChangedEventArgs e)
        {
            if (!e.NewValue.IsNullEmptyOrZero())
            {
                var value = e.NewValue.ToInt();

                if (Document.BasisDocLinks.Exists(i => i.BaseDocId == value && i.DocFieldId == Document.SchetPredField.DocFieldId))
                    return;

                var link = new DocLink { BaseDocId = value, SequelDocId = Doc.DocId, DocFieldId = Document.SchetPredField.DocFieldId };
                Document.BasisDocLinks.Add(link);

                var dtpList = GetControlTypeFilter(Document.PlatezhkiField.DocFieldId).Aggregate(string.Empty, (current, dtp) => current + (current == string.Empty ? dtp.DocTypeID : "," + dtp.DocTypeID));
                var sql = SQLQueries.SELECT_СвязиДокументовВытекающиеИзОснованийПоТипу(e.NewValue, Document.PlatezhkiField.DocFieldId.ToString(), dtpList);
                DataTable dt = DBManager.GetData(sql, Config.DS_document);
                if (null != dt)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var baseDocId = (int)row["КодДокумента"];
                        if (Document.BasisDocLinks.Exists(i => i.BaseDocId == baseDocId && i.DocFieldId == Document.PlatezhkiField.DocFieldId)) continue;

                        link = new DocLink { BaseDocId = baseDocId, SequelDocId = Doc.DocId, DocFieldId = Document.PlatezhkiField.DocFieldId };
                        Document.BasisDocLinks.Add(link);

                        PaymentDocuments.SelectedItems.AddRange(dt.AsEnumerable().Select((rw) => { string id = ((int)rw["КодДокумента"]).ToString(); return new Kesco.Lib.Entities.Item() { Id = id, Value = PaymentDocuments.GetObjectById(id) }; }));

                    }

                    //Инициализация перерисовки элемента управления
                    PaymentDocuments.SetPropertyChanged("IsReadOnly");
                    PaymentDocuments.Flush();
                }

            }
        }

        /// <summary>
        /// Событие удаление значения из списка документа Счет, инвойс, проформа
        /// </summary>
        protected void Invoice_ValueDeleted(object sender, ProperyDeletedEventArgs e)
        {
            if (!e.DelValue.IsNullEmptyOrZero())
            {
                var index = Document.BasisDocLinks.FindIndex(i => i.BaseDocId == e.DelValue.ToInt() && i.DocFieldId == Document.SchetPredField.DocFieldId);
                if (index != -1)
                {
                    var link = Document.BasisDocLinks[index];
                    if (link.DocLinkId > 0)
                        link.Delete();

                    Document.BasisDocLinks.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Событие изменения документа Платежные документ
        /// </summary>
        protected void PaymentDocuments_ValueChanged(object sender, ProperyChangedEventArgs e)
        {
            if (!e.NewValue.IsNullEmptyOrZero())
            {
                var value = e.NewValue.ToInt();

                if (Document.BasisDocLinks.Exists(i => i.BaseDocId == value && i.DocFieldId == Document.PlatezhkiField.DocFieldId))
                    return;

                var link = new DocLink { BaseDocId = value, SequelDocId = Doc.DocId, DocFieldId = Document.PlatezhkiField.DocFieldId };

                Document.BasisDocLinks.Add(link);

                var dtpList = GetControlTypeFilter(Document.SchetPredField.DocFieldId).Aggregate(string.Empty, (current, dtp) => current + (current == string.Empty ? dtp.DocTypeID : "," + dtp.DocTypeID));
                var sql = SQLQueries.SELECT_СвязиДокументовВытекающиеИзОснованийПоТипу(e.NewValue, Document.SchetPredField.DocFieldId.ToString(), dtpList);
                DataTable dt = DBManager.GetData(sql, Config.DS_document);
                if (null != dt)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        var baseDocId = (int)row["КодДокумента"];
                        if (Document.BasisDocLinks.Exists(i => i.BaseDocId == baseDocId && i.DocFieldId == Document.SchetPredField.DocFieldId)) continue;

                        link = new DocLink { BaseDocId = baseDocId, SequelDocId = Doc.DocId, DocFieldId = Document.SchetPredField.DocFieldId };
                        Document.BasisDocLinks.Add(link);

                        Invoice.SelectedItems.AddRange(dt.AsEnumerable().Select((rw) => { string id = ((int)rw["КодДокумента"]).ToString(); return new Kesco.Lib.Entities.Item() { Id = id, Value = Invoice.GetObjectById(id) }; }));
                    }

                    //Инициализация перерисовки элемента управления
                    Invoice.SetPropertyChanged("IsReadOnly");
                    Invoice.Flush();
                }


            }
        }

        /// <summary>
        /// Событие удаление значения из списка документа Платежные документ
        /// </summary>
        protected void PaymentDocuments_ValueDeleted(object sender, ProperyDeletedEventArgs e)
        {
            if (!e.DelValue.IsNullEmptyOrZero())
            {
                var index = Document.BasisDocLinks.FindIndex(i => i.BaseDocId == e.DelValue.ToInt() && i.DocFieldId == Document.PlatezhkiField.DocFieldId);
                if (index != -1)
                {
                    var link = Document.BasisDocLinks[index];
                    if (link.DocLinkId > 0)
                        link.Delete();

                    Document.BasisDocLinks.RemoveAt(index);
                }
            }
        }

        #endregion

        #region ProcessCommand
        /// <summary>
        /// Добавить товар
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="pageId">Идентификатор вызывающей страницы</param>/// 
        /// <param name="docId">Идентификатор договора</param>/// 
        /// <param name="mrisId">Идентификатор продукта</param>/// 
        private void AddResource(string title = "", string pageId = "", string docId = "", string mrisId = "")
        {
            JS.Write("resources_RecordsAdd('{0}','{1}','{2}','{3}');", HttpUtility.JavaScriptStringEncode(title), pageId, docId, mrisId);
        }

        /// <summary>
        ///     Добавить услугу
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="pageId">Идентификатор вызывающей страницы</param>/// 
        /// <param name="docId">Идентификатор договора</param>/// 
        /// <param name="mrisId">Идентификатор продукта</param>/// 
        private void AddService(string title = "", string pageId = "", string docId = "", string mrisId = "")
        {
            JS.Write("services_RecordsAdd('{0}','{1}','{2}','{3}');", HttpUtility.JavaScriptStringEncode(title), pageId, docId, mrisId);
        }

        /// <summary>
        ///     Добавить вагон
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="pageId">Идентификатор вызывающей страницы</param>/// 
        /// <param name="docId">Идентификатор договора</param>/// 
        /// <param name="resultId">Выбранное значение результата</param>/// /// 
        private void AddVagon(string title = "", string pageId = "", string docId = "", string resultId = "")
        {
            JS.Write("vagon_RecordsAdd('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}');", HttpUtility.JavaScriptStringEncode(title), pageId, docId, resultId, DBSShipperStore.Value, DBSPayerStore.Value, Shipper.Value, Payer.Value);
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
        /// Копирование товара
        /// </summary>
        /// <param name="copyId">Идентификатор копируемого товара</param>
        void CopyMrisPosition(string copyId)
        {
            var mrisCopy = new Mris(copyId);

            var mris = new Mris
            {
                TransactionType = mrisCopy.TransactionType,
                DocumentId = mrisCopy.DocumentId,
                ShipperStoreId = mrisCopy.ShipperStoreId,
                PayerStoreId = mrisCopy.PayerStoreId,
                ResourceId = mrisCopy.ResourceId,
                ResourceRus = mrisCopy.ResourceRus,
                ResourceLat = mrisCopy.ResourceLat,
                Count = mrisCopy.Count,
                UnitId = mrisCopy.UnitId,
                Coef = mrisCopy.Coef,
                CostOutNDS = mrisCopy.CostOutNDS,
                StavkaNDSId = mrisCopy.StavkaNDSId,
                SummaNDS = mrisCopy.SummaNDS,
                SummaOutNDS = mrisCopy.SummaOutNDS,
                Vsego = mrisCopy.Vsego,
                Aktsiz = mrisCopy.Aktsiz,
                CountryId = mrisCopy.CountryId,
                GTDId = mrisCopy.GTDId,
                DateMove = mrisCopy.DateMove,
                Order = Document.PositionMris.Max(l => l.Order) + 1
            };

            //mris.Delivery = mrisCopy.Delivery;

            mris.Save(false);
            RefreshMrisCurrentDoc(true);
        }

        /// <summary>
        /// Удаление товара
        /// </summary>
        /// <param name="mrisId">Идентификатор удаляемого товара</param>
        void DeleteMrisPosition(string mrisId)
        {
            var mris = new Mris(mrisId);
            mris.Delete(false);
            RefreshMrisCurrentDoc(false);
        }

        /// <summary>
        /// Копирование услуги
        /// </summary>
        /// <param name="copyId">Идентификатор копируемой услуги</param>
        void CopyFactUslPosition(string copyId)
        {
            var factUslCopy = new FactUsl(copyId);

            var factUsl = new FactUsl
            {
                DocumentId = factUslCopy.DocumentId,
                Agent1 = factUslCopy.Agent1,
                Agent2 = factUslCopy.Agent2,
                ResourceId = factUslCopy.ResourceId,
                ResourceRus = factUslCopy.ResourceRus,
                ResourceLat = factUslCopy.ResourceLat,
                UchastokId = factUslCopy.UchastokId,
                Count = factUslCopy.Count,
                UnitId = factUslCopy.UnitId,
                Coef = factUslCopy.Coef,
                CostOutNDS = factUslCopy.CostOutNDS,
                StavkaNDSId = factUslCopy.StavkaNDSId,
                SummaNDS = factUslCopy.SummaNDS,
                SummaOutNDS = factUslCopy.SummaOutNDS,
                Vsego = factUslCopy.Vsego,
                Order = Document.PositionFactUsl.Max(l => l.Order) + 1
            };
            
            factUsl.Save(false);
            RefreshFactUslCurrentDoc(true);
        }

        /// <summary>
        /// Удаление услуги
        /// </summary>
        /// <param name="factUslId">Идентификатор удаляемой услуги</param>
        void DeleteFactUslPosition(string factUslId)
        {
            var factUsl = new FactUsl(factUslId);
            factUsl.Delete(false);
            RefreshFactUslCurrentDoc(false);
        }

        /// <summary>
        /// Признак корректируемого документа
        /// </summary>
        void FlagCorrecting_Uncheck(bool corr, string oldValue = "")
        {
            if (corr)
            {
                CorrectableTtn.IsRequired = true;
                CorrectableFlag.Value = "1";
                Document._CorrectingDoc = oldValue;
                CorrectableTtn.Value = oldValue;
                SetControlProperties();
            }
            else
            {
                CorrectableTtn.IsRequired = false;
                CorrectableFlag.Value = "0";
                Document._CorrectingDoc = oldValue;
                CorrectableTtn.Value = oldValue;
                DocumentToControls();
                SetControlProperties();
            }
        }

        /// <summary>
        /// Выбор типа наборов
        /// </summary>
        /// <param name="mrisId"></param>
        void DetailMrisPosition(string mrisId)
        {
            var mris = new Mris(mrisId);

            if (DBSShipperStore.IsRequired && mris.ShipperStore.Id.IsNullEmptyOrZero() ||
                DBSPayerStore.IsRequired && mris.PayerStore.Id.IsNullEmptyOrZero())
            {
                ShowMessage(Resx.GetString("TTN_msgNoStorage"), Resx.GetString("errDoisserWarrning"));
            }
            else
            {
                lnkSkladFrom.Value = lnkSkladTo.Value = string.Empty;
                if (!mris.ShipperStore.Id.IsNullEmptyOrZero()) 
                    lnkSkladFrom.Value = GetLntSkladLink("Shipper", Resx.GetString("TTN_lblSetCost"), mris.ShipperStore.Name, Resx.GetString("TTN_lblResourceConsumption"), mris.Resource.Name, mris.Count.ToString(), mris.Unit.ЕдиницаРус, mrisId);

                if (!mris.PayerStore.Id.IsNullEmptyOrZero()) 
                    lnkSkladTo.Value = GetLntSkladLink("Payer", Resx.GetString("TTN_lblSetCost"), mris.PayerStore.Name, Resx.GetString("TTN_lblResourceConsumption"), mris.Resource.Name, mris.Count.ToString(), mris.Unit.ЕдиницаРус, mrisId);

                JS.Write("nabor_DialogShow('{0}');", Resx.GetString("TTN_lblChoiceTypeSets"));
            }
        }

        /// <summary>
        /// Формирование ссылки выбора типа набора
        /// </summary>
        /// <param name="type">Тип (Shipper или Payer)</param>
        /// <param name="cost">Текст: Набор расходов со склада</param>
        /// <param name="payerStore">Наиментование склада плательщика/поставщика</param>
        /// <param name="resourceConsumption">Текст:для расхода ресурса</param>
        /// <param name="resourceName">Наименование ресурса</param>
        /// <param name="count">Количество</param>
        /// <param name="unit">Ед. изм.</param>
        /// <param name="mrisId">Идентификатор ресурса</param>
        /// <returns>ссылка</returns>
        string GetLntSkladLink(string type, string cost, string payerStore, string resourceConsumption, string resourceName, string count, string unit, string mrisId)
        {
            cost = HttpUtility.HtmlEncode(cost);
            payerStore = HttpUtility.HtmlEncode(payerStore);
            resourceConsumption = HttpUtility.HtmlEncode(resourceConsumption);
            resourceName = HttpUtility.HtmlEncode(resourceName);
            return
                string.Format("<a href='#' onclick=\"{0}\" style='FONT-SIZE: 8pt; FONT-FAMILY: Verdana; COLOR: blue;'>{1}</a>",
                        string.Format("distrib_RecordsAdd('{0}','{1}','{2}','{3}','{4}');",
                        type,
                        cost + " " + payerStore + " " + resourceConsumption + " " + resourceName + " " + count + " " + unit,
                        IDPage, 
                        Document.Id, 
                        mrisId
                        ),
                        string.Format("{0}: {1}<br/>{2}: {3} {4} {5}",
                        cost,
                        payerStore,
                        resourceConsumption,
                        resourceName,
                        count,
                        unit
                        )
                    );
        }

        #endregion

        #region Render
        /// <summary>
        /// Отрисовка сообщения, если документ скорректирован 
        /// </summary>
        /// <returns></returns>
        public string RenderCorrectingDoc()
        {
            using (var w = new StringWriter())
            {
                if (Document.IsCorrected)
                {
                    w.Write(Resx.GetString("MSG_CorrectedWith") + " ");
                    if (Document.CorrectingSequelDoc.Unavailable)
                        w.Write("#" + Document.CorrectingSequelDoc);
                    else
                    {
                        RenderLinkDoc(w, Document._CorrectingSequelDoc);
                        w.Write(IsRusLocal ? Document.CorrectingSequelDoc.TypeDocRu : Document.CorrectingSequelDoc.TypeDocEn);

                        //if (Document.Name.Length > 0)
                        //    w.Write(" " + Document.Name);
                        //else
                        //    w.Write(" #" + Document.Id);

                        if (Document.CorrectingSequelDoc.Number.Length > 0) w.Write(" № " + Document.CorrectingSequelDoc.Number);
                        if (Document.CorrectingSequelDoc.Date != DateTime.MinValue) w.Write(" от " + Document.CorrectingSequelDoc.Date.ToString("dd.MM.yyyy"));

                        RenderLinkEnd(w);
                    }
                }
                return w.ToString();
            }
        }

        #endregion

        #region Checking
        /// <summary>
        /// Проверка наличия вытекающего из ТТН счета-фактуры
        /// </summary>
        /// <returns>true - из реализации вытекает счет-фактура; false - нету</returns>
        private bool SequelFakturaExists()
        {
            var docs = Document.GetSequelDocs(791);
            return docs.Any();
        }

        /// <summary>
        /// Проверка идентичности ГО/ГП, транспортных узлов в выбранных отправках
        /// </summary>
        /// <param name="resultGuid"></param>
        /// <returns></returns>
        public bool CheckIdentity(string resultGuid, out string info)
        {
            info = string.Empty;
            string distinctValue = "", sameValueTTN = "";
            StringDictionary values = new StringDictionary();

            var sqlParams = new Dictionary<string, object> { { "@gd", resultGuid } };
            var dt = DBManager.GetData(SQLQueries.DeliveryFieldsRequest, Config.DS_document, CommandType.Text, sqlParams);
            Document.FillGPersonsDictionary(dt, ref values);

            if (values.ContainsKey("кодго") && Document.GOPersonField.Value != null)
            {
                StringCollection colGO = Lib.ConvertExtention.Convert.Str2Collection(values["кодго"]);
                switch (colGO.Count)
                {
                    case 0:
                        break;
                    case 1:
                        if (Document.GOPersonField.Value.ToString().Length > 0 && !colGO.Contains(Document.GOPersonField.Value.ToString()))
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GOPersonField.Name;
                        break;
                    case 2:
                        if (Document.GOPersonField.Value.ToString().Length > 0)
                        {
                            if (!colGO.Contains(""))
                                distinctValue += (distinctValue.Length > 0 ? ", " : "") + Document.GOPersonField.Name;
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GOPersonField.Name;
                        }
                        break;
                    default:
                        if (Document.GOPersonField.Value.ToString().Length > 0)
                        {
                            distinctValue += (distinctValue.Length > 0 ? ", " : "") + Document.GOPersonField.Name;
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GOPersonField.Name;
                        }
                        break;
                }
            }

            if (values.ContainsKey("кодгп") && Document.GPPersonField.Value != null)
            {
                StringCollection colGP = Lib.ConvertExtention.Convert.Str2Collection(values["кодгп"]);
                switch (colGP.Count)
                {
                    case 0:
                        break;
                    case 1:
                        if (Document.GPPersonField.Value.ToString().Length > 0 && !colGP.Contains(Document.GPPersonField.Value.ToString()))
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GPPersonField.Name;
                        break;
                    case 2:
                        if (Document.GPPersonField.Value.ToString().Length > 0)
                        {
                            if (!colGP.Contains(""))
                                distinctValue += (distinctValue.Length > 0 ? ", " : "") + Document.GPPersonField.Name;
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GPPersonField.Name;
                        }
                        break;
                    default:
                        if (Document.GPPersonField.Value.ToString().Length > 0)
                        {
                            distinctValue += (distinctValue.Length > 0 ? ", " : "") + Document.GPPersonField.Name;
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GPPersonField.Name;
                        }
                        break;
                }
            }

            if (values.ContainsKey("узелотправления") && Document.GOPersonWeselField.Value != null)
            {
                StringCollection colGOWesel = Lib.ConvertExtention.Convert.Str2Collection(values["узелотправления"]);
                switch (colGOWesel.Count)
                {
                    case 0:
                        break;
                    case 1:
                        if (Document.GOPersonWeselField.Value.ToString().Length > 0 && !colGOWesel.Contains(Document.GOPersonWeselField.Value.ToString()))
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GOPersonWeselField.Name;
                        break;
                    case 2:
                        if (Document.GOPersonWeselField.Value.ToString().Length > 0)
                        {
                            if (!colGOWesel.Contains(""))
                                distinctValue += (distinctValue.Length > 0 ? ", " : "") + Document.GOPersonWeselField.Name;
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GOPersonWeselField.Name;
                        }
                        break;
                    default:
                        if (Document.GOPersonWeselField.Value.ToString().Length > 0)
                        {
                            distinctValue += (distinctValue.Length > 0 ? ", " : "") + Document.GOPersonWeselField.Name;
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GOPersonWeselField.Name;
                        }
                        break;
                }
            }

            if (values.ContainsKey("узелназначения") && Document.GPPersonWeselField.Value != null)
            {
                StringCollection colGPWesel = Lib.ConvertExtention.Convert.Str2Collection(values["узелназначения"]);
                switch (colGPWesel.Count)
                {
                    case 0:
                        break;
                    case 1:
                        if (Document.GPPersonWeselField.Value.ToString().Length > 0 && !colGPWesel.Contains(Document.GPPersonWeselField.Value.ToString()))
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GPPersonWeselField.Name;
                        break;
                    case 2:
                        if (Document.GPPersonWeselField.Value.ToString().Length > 0)
                        {
                            if (!colGPWesel.Contains(""))
                                distinctValue += (distinctValue.Length > 0 ? ", " : "") + Document.GPPersonWeselField.Name;
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GPPersonWeselField.Name;
                        }
                        break;
                    default:
                        if (Document.GPPersonWeselField.Value.ToString().Length > 0)
                        {
                            distinctValue += (distinctValue.Length > 0 ? ", " : "") + Document.GPPersonWeselField.Name;
                            sameValueTTN += (sameValueTTN.Length > 0 ? ", " : "") + Document.GPPersonWeselField.Name;
                        }
                        break;
                }
            }

            if (distinctValue.Length > 0)
            {
                info = String.Format(Resx.GetString("_Msg_РазныеСвойства"), distinctValue);
                Document.ClearDeliveryTemporary(resultGuid);
                return false;
            }
            if (sameValueTTN.Length > 0)
            {
                info = String.Format(Resx.GetString("_Msg_НеСовпадаютСвойства"), sameValueTTN);
                Document.ClearDeliveryTemporary(resultGuid);
                return false;
            }

            return true;

        }

        protected override bool ValidateDocument(out List<string> errors, params string[] exeptions)
        {
            base.ValidateDocument(out errors, exeptions);

            if ((exeptions.Contains("SaveButton") || exeptions.Contains("AddFactUsl") || exeptions.Contains("AddSign")) && Document.PositionMris.Count == 0)
            {
                errors.Add(Resx.GetString("TTN_msgNoMris"));
            }

            if (CorrectableFlag.Checked && Document.CorrectingDocField.Value.ToString().IsNullEmptyOrZero())
            {
                errors.Add(Resx.GetString("TTN_ntfNoCorrectedDocument"));
            }

            var err = SetKursAndScale();

            if (!err.IsNullEmptyOrZero()) errors.Add(err);

            return errors.Count <= 0;
        }

        #endregion

        /// <summary>
        /// Функция, формирующая словь с параметрами в зависимости от установленнного фильтра
        /// </summary>
        /// <returns>Словарь с параметрами</returns>
        private Dictionary<string, object> GetSQLParams()
        {
            var sqlParams = new Dictionary<string, object>();
            var docId = 0;

            docId = int.Parse(!CopyId.IsNullEmptyOrZero() ? CopyId : Doc.Id);
            sqlParams.Add("@КодДокумента", docId);

            return sqlParams;
        }

        /// <summary>
        /// Класс сравнения ссылок на объекты, равны если указывают на один объект
        /// </summary>
        public sealed class ReferenceEqualityComparer : IEqualityComparer<V4Control>
        {
            public bool Equals(V4Control x, V4Control y)
            {
                return ReferenceEquals(x, y);
            }

            public int GetHashCode(V4Control obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

    }
}