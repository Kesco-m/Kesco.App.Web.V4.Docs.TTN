using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Entities.Documents.EF.Trade;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Settings;

namespace Kesco.App.Web.Docs.TTN
{
    public partial class DistribDocPage : EntityPage
    {
        #region declaration, property, field

        public bool TypeNabor; //false-набор приходов на склад (Shipper) , true-набор расходов со склада (Payer)
        private DateTime date = DateTime.Now;        

        protected Mris mris;
        protected string id;
        protected string idDoc;
        protected string idParentPage;
        protected string typeNabor;

        /// <summary>
        /// Вызывающая страница (накладная)
        /// </summary>
        protected Nakladnaya ParentPage;

        /// <summary>
        ///  Статическое поле для получения строки подключения документа
        /// </summary>
        public static string CN
        {
            get { return string.IsNullOrEmpty(_connectionString) ? (_connectionString = Config.DS_document) : _connectionString; }
        }

        /// <summary>
        ///  Инкапсулирует и сохраняет в себе строку подключения
        /// </summary>
        public static string _connectionString;

        /// <summary>
        /// Точность вывода валют
        /// </summary>
        private int Scale
        {
            get { return Document.CurrencyScale; }
        }

        /// <summary>
        /// ТТН
        /// </summary>
        private Lib.Entities.Documents.EF.Trade.TTN Document
        {
            get { return ParentPage.Document; }
        }

        #region Остатки
        public DataTable ost;
        public int CurrentDv = 0;
        public Hashtable ostDocs = new Hashtable();

        public decimal OstatokFakt { get { return PrihodFaktBezNabora - RashodBezNaboraFakt; } }
        public decimal OstatokRaschet { get { return PrihodRaschetBezNabora - RashodBezNaboraRaschet; } }
        public decimal PrihodBezNabora = 0m;
        public decimal PrihodFaktBezNabora
        {
            get
            {
                if (ost.Rows.Count > 0)
                {
                    object s = ost.Compute("Sum(Факт)", "");
                    return s == DBNull.Value ? 0m : Convert.ToDecimal(s);
                }
                return 0m;
            }
        }
        public decimal PrihodRaschetBezNabora
        {
            get
            {
                if (ost.Rows.Count > 0)
                {
                    object s = ost.Compute("Sum(Расчет)", "");
                    return s == DBNull.Value ? 0m : Convert.ToDecimal(s);
                }
                return 0m;
            }
        }
        public decimal RashodBezNabora { get { return ВсегоНабрать - Набрано; } }
        public decimal RashodBezNaboraFakt = 0m; //out
        public decimal RashodBezNaboraRaschet = 0m; //out
        public DataTable DvDoc;
        public DataTable NaborDoc;
        public decimal ВсегоНабрать
        {
            get
            {
                object sum = DvDoc.Compute("Sum(Количество)", "");
                return sum == DBNull.Value ? 0m : Convert.ToDecimal(sum);
            }
        }
        public decimal Набрано
        {
            get
            {
                object sum = NaborDoc.Compute("Sum(Количество)", "");
                return sum == DBNull.Value ? 0m : Convert.ToDecimal(sum);
            }
        }

        #endregion

        #region Filter
        //0 отображать все
        //1 отображать все используемые
        //2 фильтр по вагонам
        //3 фильтр по накладным
        public string FilterValue = "";

        private ArrayList filterArray = new ArrayList(); //коды движения на складах
        public ArrayList FilterArray { get { return filterArray; } }
        private int filter = 0;
        public int Filter
        {
            get { return filter; }
            set
            {
                filter = value;
                filterArray.Clear();
                if (filter == 3 && this.FilterValue.Trim().Length > 0) //по номеру накладной
                {
                    string ids = "";
                    foreach (DataRow r in ost.Rows)
                        if (r["КодОтправкиВагона"] != DBNull.Value)
                            ids += (ids.Length > 0 ? "," : "") + r["КодДвиженияНаСкладе"].ToString();
                    if (ids.Length > 0)
                    {
                        string sql = string.Format(SQLQueries.SELECT_КодыДвиженияНаСкладе, ids);

                        sql = @"SELECT DISTINCT КодДвиженияНаСкладе FROM vwДвиженияНаСкладах d (nolock)
								WHERE d.КодДвиженияНаСкладе IN(" + ids + @") AND
								EXISTS(SELECT * FROM vwОтправкаВагоновУчастки t (nolock) WHERE d.КодОтправкиВагона=t.КодОтправкиВагона AND t.НомерДокумента like '%'+@НомерДокумента+'%')";
                        var sqlParams = new Dictionary<string, object> { { "@НомерДокумента", FilterValue.Trim() } };
                        var dt = DBManager.GetData(sql, Config.DS_document, CommandType.Text, sqlParams);
                        foreach (DataRow r in dt.Rows)
                            filterArray.Add(r[0]);
                    }
                }
            }
        }
        #endregion

        #endregion

        /// <summary>
        ///  Конструктор по умолчанию
        /// </summary>
        public DistribDocPage()
        {
            HelpUrl = string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!V4IsPostBack)
            {
                divTitle.Value = string.Format("{0}: {1}", typeNabor == "Shipper" ? Resx.GetString("TTN_lblSetArrival") : Resx.GetString("TTN_lblSetCost"), typeNabor == "Shipper" ? mris.ShipperStore.Name : mris.PayerStore.Name);
            }
        }

        #region Binder

        protected override void EntityFieldInit()
        {
            if (!V4IsPostBack)
            {
                id = Request.QueryString["id"];
                idDoc = Request.QueryString["idDoc"];
                idParentPage = Request.QueryString["idpp"];
                typeNabor = Request.QueryString["type"];
                TypeNabor = typeNabor == "Payer";

                ParentPage = Application[idParentPage] as Nakladnaya;
                if (ParentPage == null)
                {
                    ShowMessage(Resx.GetString("errRetrievingPageObject"), Resx.GetString("errPrinting"), MessageStatus.Error);
                    return;
                }

                if (!String.IsNullOrEmpty(id) && id != "0")
                {
                    mris = new Mris(id);
                    CurrentDv = Convert.ToInt32(id);
                    if (mris == null || mris.Id == "0")
                        throw new LogicalException(Resx.GetString("TTN_ ERRMoveStockInitialized"), "", System.Reflection.Assembly.GetExecutingAssembly().GetName(), Priority.Info);
                }
                else
                {
                    mris = new Mris { DocumentId = int.Parse(idDoc) };
                }

                SetInitValue();
            }

            Entity = mris;

            efShipper.Value = ParentPage.ShipperField.Value;
            efResource.Value = mris.Resource.Id;
            efUnit.Value = mris.Unit.Id;
            efResidence.Value = mris.ShipperStore.Residence.Id;
            efStore.Value = mris.ShipperStore.Id;

            base.EntityFieldInit();
        }
        
        public void SetInitValue()
        {
            var Sklad = TypeNabor ? mris.PayerStore : mris.ShipperStore;
            //получим движения нашего документа по заданному ресурсу
            var sqlParams = new Dictionary<string, object>
            {
				{"@КодДокумента", Document.Id},
				{"@КодРесурса", mris.Resource.Id},
                {"@КодЕдиницыИзмерения", mris.UnitId}
            };
            DvDoc = DBManager.GetData(SQLQueries.SELECT_DvDoc, Config.DS_document, CommandType.Text, sqlParams);

            DvDoc.Constraints.Add("pk", DvDoc.Columns["КодДвиженияНаСкладе"], true);

            if (DvDoc.Rows.Count == 1)
                CurrentDv = Convert.ToInt32(DvDoc.Rows[0]["КодДвиженияНаСкладе"]);

            sqlParams = new Dictionary<string, object>
            {
                {"@КодДокумента", Document.Id},
                {"@КодРесурса", mris.ResourceId},
                {"@КодЕдиницыИзмерения", mris.UnitId},
            };
            var sql = TypeNabor ? SQLQueries.SELECT_NaborDocPayer : SQLQueries.SELECT_NaborDocShipper;
            NaborDoc = DBManager.GetData(sql, CN, CommandType.Text, sqlParams);
            NaborDoc.Columns.Add("ТипТранзакции", typeof(int));

            DataColumn[] fcNaborDoc = { NaborDoc.Columns["КодДвиженияВДокументе"], NaborDoc.Columns["КодДвиженияВНаборе"] };
            NaborDoc.Constraints.Add("pk", fcNaborDoc, true);

            // Приход без набора
            ost = Mris.GetOstatkiDoc(int.Parse(Sklad.Id), TypeNabor, mris.ResourceId, int.Parse(mris.UnitId.ToString()), int.Parse(Document.Id), Document.Date);

            Document doc;
            foreach (DataRow r in ost.Rows)
            {
                if (!ostDocs.ContainsKey(r["КодДокумента"]))
                {
                    doc = new Document(r["КодДокумента"].ToString());
                    if (doc.Unavailable)
                        ostDocs.Add(r["КодДокумента"], "#" + r["КодДокумента"].ToString());
                    else
                        ostDocs.Add(r["КодДокумента"], doc.GetFullDocumentName(CurrentUser));
                }

                //Для получения итогов по типу транзакции дополним наборы этим полем
                foreach (DataRow nr in NaborDoc.Select("КодДвиженияВНаборе=" + r["КодДвиженияНаСкладе"]))
                    nr["ТипТранзакции"] = r["ТипТранзакции"];

            }

            // РасходБезНабора
            Mris.GetDebit(int.Parse(Sklad.Id), mris.ResourceId, int.Parse(mris.UnitId.ToString()), int.Parse(Document.Id), date, out RashodBezNaboraFakt, out RashodBezNaboraRaschet);
        }
        
        #endregion

        #region MenuButtons

        /// <summary>
        ///     Инициализация/создание кнопок меню
        /// </summary>
        private void SetMenuButtons()
        {
            var btnAdd = new Button
            {
                ID = "btnAccept",
                V4Page = this,
                Text = Resx.GetString("cmdApply"),
                Title = Resx.GetString("cmdApply"),
                Width = 105,
                IconJQueryUI = ButtonIconsEnum.Ok,
                OnClick = "cmd('cmd', 'NaborOk');"
            };

            var btnClose = new Button
            {
                ID = "btnClose",
                V4Page = this.ParentPage,
                Text = Resx.GetString("cmdClose"),
                Title = Resx.GetString("cmdCloseTitleApp"),
                IconJQueryUI = ButtonIconsEnum.Close,
                Width = 105,
                OnClick = "parent.distrib_Records_Close(idp);"
            };

            var buttons = ParentPage.DocEditable ? new[] { btnAdd, btnClose } : new[] { btnClose };
            AddMenuButton(buttons);
        }

        /// <summary>
        /// Обработка клиентских команд
        /// </summary>
        /// <param name="cmd">Команды</param>
        /// <param name="param">Параметры</param>
        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "NaborOk":
                    SaveData();
                    JS.Write("parent.distrib_Records_Save();");
                    break;
                case "ArrangeAll": 
                    ArrangeAll(); 
                    RefreshOstatkiTable(); 
                    //SaveData(); 
                    break;
                case "ArrangeDv": 
                    CurrentDv = int.Parse(param["MrisId"]);
                    ArrangeDv(CurrentDv); 
                    RefreshOstatkiTable(); 
                    //SaveData(); 
                    break;
                case "SetFilterDocs":
                    Filter = int.Parse(param["Type"]); 
                    RefreshOstatkiTable(); 
                    //SaveData(); 
                    break;
                case "SetCurrentDv":
                    CurrentDv = int.Parse(param["CurrentDv"]); 
                    RefreshOstatkiTable(); 
                    //SaveData(); 
                    break;
                case "SetNaborKol":
                    string rez = SetNaborKol(CurrentDv, int.Parse(param["dvNabor"]), param["el"]);
                    if (rez.Length > 0) ShowMessage(rez);
                    RefreshOstatkiTable(); 
                    //SaveData(); 
                    break;
                case "SetNaborKol2":
                    string rez2 = SetNaborKol(int.Parse(param["dvDoc"]), int.Parse(param["dvNabor"]), param["el"]);
                    if (rez2.Length > 0)
                        ShowMessage(rez2);
                    RefreshOstatkiTable(); 
                    //SaveData(); 
                    break;
            }
        }

        /// <summary>
        ///     Кнопка: Сохранить
        /// </summary>
        private void SaveData()
        {
            Mris.SaveDistrib(Document.Id, mris.Resource.Id, mris.Unit.Id, TypeNabor, NaborDoc);
        }

        #endregion

        #region Render
        /// <summary>
        ///     Подготовка данных для отрисовки заголовка страницы(панели с кнопками)
        /// </summary>
        /// <returns></returns>
        protected string RenderDocumentHeader()
        {
            using (var w = new StringWriter())
            {
                try
                {
                    ClearMenuButtons();
                    SetMenuButtons();
                    RenderButtons(w);
                }
                catch (Exception e)
                {
                    var dex = new DetailedException(Resx.GetString("TTN_errFailedGenerateButtons") + ": " + e.Message, e);
                    Logger.WriteEx(dex);
                    throw dex;
                }

                return w.ToString();
            }
        }

        public void RenderOstatki(TextWriter w)
        {
            for (int i = 1; i <= 4; i++)
                RenderOstatki(w, i.ToString());
        }

        private void RenderOstatki(TextWriter w, string _TypeMovement)
        {
            decimal _kol;
            bool kolExceedRemainder;
            string formatNumber = "N" + Scale;
            //------------------------------------------
            //фильтр по номеру вагона
            string filter = "";
            if (Filter == 2 && FilterValue.Trim().Length > 0)
                filter = " and ОтправкаВагона like '%" + FilterValue.Trim().Replace("'", "").Replace("%", "") + "%'";
            //------------------------------------------

            if (ost.Columns.IndexOf("ТипТранзакции") == -1) return;

            DataRow[] rows = ost.Select("ТипТранзакции=1" + _TypeMovement + filter);

            if (rows.Length == 0) return;

            object summ = ost.Compute("Sum(Факт)", "ТипТранзакции=1" + _TypeMovement);
            decimal ostatokFakt = summ == DBNull.Value ? 0m : Convert.ToDecimal(summ);

            summ = ost.Compute("Sum(Расчет)", "ТипТранзакции=1" + _TypeMovement);
            decimal ostatokRaschet = summ == DBNull.Value ? 0m : Convert.ToDecimal(summ);

            summ = NaborDoc.Compute("Sum(Количество)", "ТипТранзакции=1" + _TypeMovement);
            decimal nabor = summ == DBNull.Value ? 0m : Convert.ToDecimal(summ);

            w.Write("<tr>");
            w.Write("<td style='PADDING-LEFT: 10px'><b><i>");
            switch (_TypeMovement)
            {
                case "1":
                    w.Write(Resx.GetString("TTN_lblProduction"));
                    break;
                case "2":
                    w.Write(Resx.GetString("TTN_lblPurchase"));
                    break;
                case "3":
                    w.Write(Resx.GetString("lblMove"));
                    break;
                case "4":
                    w.Write(Resx.GetString("TTN_lblRests"));
                    break;
            }
            w.Write("</i></b></td><td>&nbsp;</td>");

            w.Write("<td nowrap align='right' {0}>{1}</td>",
                ostatokFakt < 0 ? "style='COLOR: Red'" : "",
                ostatokFakt.ToString(formatNumber));

            w.Write("<td nowrap align='right' style='COLOR: DimGray'>{0}</td>", ostatokRaschet.ToString(formatNumber));

            w.Write("<td align='right'>{0}</td>", nabor.ToString(formatNumber));//<div class='nmb' type='number' contenteditable=true ></div>
            w.Write("</tr>");

            decimal Raschet = 0m, Fakt = 0m;
            foreach (DataRow r in rows)
            {
                //Фильтр по номеру накладной
                //---------------------------------
                if (Filter == 3 && FilterValue.Trim().Length > 0)
                    if (!FilterArray.Contains(r["КодДвиженияНаСкладе"]))
                        continue;
                //---------------------------------
                bool Finished = r["ТипПодписи"].ToString().Equals("1");

                if (r["Факт"] == DBNull.Value)
                {
                    continue;
                }

                //---------------------------------
                Fakt = Kesco.Lib.ConvertExtention.Convert.Round(Convert.ToDecimal(r["Факт"]), Scale);
                Raschet = Kesco.Lib.ConvertExtention.Convert.Round(Convert.ToDecimal(r["Расчет"]), Scale);
                //---------------------------------

                string docName = ostDocs[r["КодДокумента"]].ToString();
                string title = "";
                if (r["ОтправкаВагона"].ToString().Trim().Length > 0)
                    title = docName;
                if (!Finished)
                    title += " " + Resx.GetString("TTN_msgWorkDocumentNotCompleted");

                w.Write("<tr><td  style='PADDING-LEFT: 20px' {0}>", Finished ? "" : "title='" + title + "'");

                //Выводим документ
                if (docName.StartsWith("#"))
                    w.Write(r["ОтправкаВагона"].ToString().Trim().Length > 0 ? r["ОтправкаВагона"].ToString() : docName);
                else
                {
                    RenderLinkDocument(w, int.Parse(r["КодДокумента"].ToString()));
                    w.Write(r["ОтправкаВагона"].ToString().Trim().Length > 0 ? r["ОтправкаВагона"].ToString() : docName.Replace("№", "№ <b>") + "</b>");
                    RenderLinkEnd(w);
                }

                w.Write("</td>");
                w.Write("<td nowrap align='right' {0}>", Finished ? "" : "style='COLOR: DimGray'");
                w.Write((Convert.ToDecimal(r["Приход"])).ToString(formatNumber));
                w.Write("</td>");

                w.Write("<td nowrap align='right' {0}>", Fakt < 0 ? "style='COLOR: Red'" : "");
                w.Write(Fakt.ToString(formatNumber));
                w.Write("</td>");

                w.Write("<td nowrap align='right' style='COLOR: DimGray'>");
                w.Write(Raschet.ToString(formatNumber));
                w.Write("</td>");

                //количество набранное
                if (CurrentDv == 0)
                    w.Write("<td></td>");
                else if (CurrentDv == -1)
                {
                    _kol = GetNaborKol(Convert.ToInt32(r["КодДвиженияНаСкладе"]), "КодДвиженияВНаборе");
                    kolExceedRemainder = (Finished ? Fakt : Raschet) < _kol;
                    w.Write("<td align='right'{1}>{0}</td>", _kol != 0m ? _kol.ToString(formatNumber) : "", kolExceedRemainder ? " class='rn'" : "");
                }
                else
                {
                    _kol = GetNaborKol(CurrentDv, Convert.ToInt32(r["КодДвиженияНаСкладе"]));
                    if (_kol <= 0)
                        kolExceedRemainder = false;
                    else
                        kolExceedRemainder = (Finished ? Fakt : Raschet) < GetNaborKol(Convert.ToInt32(r["КодДвиженияНаСкладе"]), "КодДвиженияВНаборе");

                    w.Write("<td><div class='nmb{1}' type='number' contenteditable=true dvNabor={0} ok='SetNaborKol()'>", r["КодДвиженияНаСкладе"], kolExceedRemainder ? " rn" : "");
                    if (_kol != 0m)
                        w.Write(_kol.ToString(formatNumber));
                    w.Write("</div></td>");
                }

                w.Write("<tr>");
            }
        }

        protected void RenderOstatkiTable(TextWriter w)
        {
            string formatNumber = "N" + Scale;
            w.Write(
                "<table cellSpacing='0' cellPadding='0' width='100%' border='1' style='border-collapse:collapse' onkeydown='n_keydown()' onbeforedeactivate='n_ok()' onfocusin='n_focus()'>");
            //------------------------------------------------
            w.Write(@"<tr align='center'>
					<td width='50%' height='20'>&nbsp;</td>
					<td width='11%' height='20'>{0}</td>
					<td width='11%' height='20'>{1}</td>
					<td width='11%' height='20'>{2}</td>
					<td width='17%' height='20'>{3}</td></tr>", TypeNabor ? Resx.GetString("TTN_lblConsumption") : Resx.GetString("TTN_lblReceipt"),
                                                                Resx.GetString("TTN_lblFact"), Resx.GetString("TTN_lblCalculeted"), Resx.GetString("TTN_lblSet"));

            //------------------------------------------------
            //Остаток на складе на складе
            w.Write("<tr><td><b>{0} {1}</b></td>", Resx.GetString("TTN_lblRestSkladOn"), date.ToShortDateString());
            w.Write("<td>&nbsp;</td>");
            w.Write("<td nowrap align='right' {0}>", OstatokFakt < 0 ? "style='COLOR: Red'" : "");
            w.Write(OstatokFakt.ToString(formatNumber));
            w.Write("</td>");

            w.Write("<td nowrap align='right' style='COLOR: DimGray'>");
            w.Write(OstatokRaschet.ToString(formatNumber));
            w.Write("</td>");
            w.Write("<td>&nbsp;</td>");
            w.Write("</tr>");
            //------------------------------------------------
            //Расход(Приход) без набора
            w.Write("<tr><td><b>{0} {1}</b></td>", TypeNabor ? Resx.GetString("TTN_lblReceipt") : Resx.GetString("TTN_lblConsumption"), Resx.GetString("TTN_lblNoSet"));
            w.Write("<td>&nbsp;</td>");
            w.Write("<td nowrap class='gn'>{0}</td>", RashodBezNaboraFakt.ToString(formatNumber));
            w.Write("<td nowrap class='gn'>{0}</td>", RashodBezNaboraRaschet.ToString(formatNumber));
            w.Write("<td nowrap class='gn'>{0}</td></tr>", RashodBezNabora.ToString(formatNumber));
            //------------------------------------------------
            //Всего набрать
            w.Write("<tr{0}>", CurrentDv == -1 ? " style='background-color:#ffff99'" : "");
            w.Write(@"<td onclick='cmd(""cmd"",""SetCurrentDv"",""CurrentDv"",-1)'><b>{0}</b></td>
					<td>&nbsp;</td>
					<td>&nbsp;</td>
					<td>&nbsp;</td>
					<td align='right' nowrap>{1}", Resx.GetString("TTN_lblConsumption"), ВсегоНабрать.ToString(formatNumber));

            w.Write(@"<a href='#' onkeydown=""if(event.keyCode==13){");
            w.Write("event.returnValue=false;cmd('cmd','ArrangeAll');");
            w.Write("}");
            w.Write(
                @""" onclick=""cmd('cmd','ArrangeAll');""><IMG src='/styles/calc.gif' title='{0}' tabindex='0' border=0/></a>", Resx.GetString("TTN_lblDistributeByDocuments"));
            w.Write("</td></tr>");

            decimal ОсталосьНабрать = 0m, Набрано = 0m;
            //строки документа
            if (DvDoc.Rows.Count > 1 || Filter > 1)
            {
                string filter = "", docName = "";
                int idDoc;
                //				if(data.Filter==2 && Vagon.Value.Length>0)
                //					filter="ОтправкаВагона like '%"+Vagon.Value.Trim().Replace("'","").Replace("%","")+"%'";


                foreach (DataRow r in DvDoc.Select(filter))
                {
                    Набрано = GetNaborKol(Convert.ToInt32(r["КодДвиженияНаСкладе"]), "КодДвиженияВДокументе");
                    ОсталосьНабрать = Convert.ToDecimal(r["Количество"]) - Набрано;
                    w.Write("<tr{0}>", Convert.ToInt32(r["КодДвиженияНаСкладе"]) == CurrentDv ? " style='background-color:#ffff99'" : "");
                    w.Write(
                        "<td onclick='cmd(\"cmd\",\"SetCurrentDv\",\"CurrentDv\",{0})' title='{2}: {0}' style='font-weight:bold'><a href='#'>{1}</a></td>",
                        r["КодДвиженияНаСкладе"], r["ОтправкаВагона"], Resx.GetString("TTN_lblStockCode"));
                    w.Write("<td>&nbsp;</td>");
                    w.Write("<td align='right' title='{1}'>{0}</td>",
                        (Convert.ToDecimal(r["Количество"])).ToString(formatNumber), Resx.GetString("TTN_lblAllSet"));
                    w.Write("<td title='{1} {2}' class='gn'>{0}</td>",
                        ОсталосьНабрать <= 0 ? "" : ОсталосьНабрать.ToString(formatNumber),
                        TypeNabor ? Resx.GetString("TTN_lblReceipt") : Resx.GetString("TTN_lblConsumption"), Resx.GetString("TTN_lblNoSet"));
                    w.Write("<td align='right' nowrap>{0}", Набрано.ToString(formatNumber));

                    w.Write(@"<a href='#' onkeydown=""if(event.keyCode==13){");
                    w.Write("event.returnValue=false; cmd('cmd','ArrangeAll');");
                    w.Write("}");
                    w.Write(
                        @""" onclick=""cmd('cmd','ArrangeDv', 'MrisId' , {0});""><IMG src='/styles/calc.gif' title='{1}' tabindex='0' border=0/></a>",
                        r["КодДвиженияНаСкладе"], Resx.GetString("TTN_lblDistributeByDocuments"));
                    w.Write("</td></tr>");

                    //наборы
                    DataRow[] nabors = NaborDoc.Select("КодДвиженияВДокументе=" + r["КодДвиженияНаСкладе"]);
                    if (nabors != null)
                        foreach (DataRow nr in nabors)
                        {
                            w.Write("<tr>");
                            //отправка
                            w.Write("<td style='padding-left:20px' title='{2}: {0}'>{1}</td>",
                                nr["КодДвиженияВНаборе"], nr["ОтправкаВагона"], Resx.GetString("TTN_lblStockCode"));
                            //документ
                            w.Write("<td colspan=3>");
                            idDoc = Convert.ToInt32(ost.Rows.Find(nr["КодДвиженияВНаборе"])["КодДокумента"]);
                            docName = ostDocs[idDoc].ToString();
                            if (docName.StartsWith("#"))
                                w.Write(docName);
                            else
                            {
                                RenderLinkDocument(w, idDoc);
                                w.Write(docName);
                                RenderLinkEnd(w);
                            }
                            w.Write("</td>");

                            //количество
                            w.Write(
                                "<td nowrap><div class='nmb' type='number' contenteditable=true dvDoc={0} dvNabor={1} ok='SetNaborKol2()'>",
                                nr["КодДвиженияВДокументе"], nr["КодДвиженияВНаборе"]);
                            w.Write((Convert.ToDecimal(nr["Количество"])).ToString(formatNumber));
                            w.Write("</div></td>");
                            w.Write("</tr>");

                        }
                }
            }
            //------------------------------------------------
            //Остатки
            if (Filter != 1)
                RenderOstatki(w);
            //------------------------------------------------
            //Приход(Расход) без набора
            w.Write("<tr><td><b>{0} {1}</b></td><td>&nbsp;</td>", TypeNabor ? Resx.GetString("TTN_lblConsumption") : Resx.GetString("TTN_lblReceipt"), Resx.GetString("TTN_lblNoSet"));
            w.Write("<td nowrap align='right'>{0}</td>", PrihodFaktBezNabora.ToString(formatNumber));
            w.Write("<td nowrap align='right' style='COLOR: DimGray'>{0}</td>",
                PrihodRaschetBezNabora.ToString(formatNumber));
            w.Write("<td align='right'>&nbsp;</td></tr>");

            w.Write("</table>");
        }

        private void RefreshOstatkiTable()
        {
            StringWriter w = new StringWriter();
            RenderOstatkiTable(w);
            RefreshControlText(w, "divMainTable");
        }

        private void RefreshControlText(TextWriter w, string controlId)
        {
            JS.Write("var obj_{0} = document.getElementById('{0}'); if(obj_{0}){{obj_{0}.innerHTML='{1}';}}", controlId,
                HttpUtility.JavaScriptStringEncode(w.ToString()));
        }


        #endregion

        #region Changed
        protected void Kvitanciya_Changed(object sender, ProperyChangedEventArgs e)
        {
            FilterValue = efKvitanciya.Value.Trim();
            if (efKvitanciya.Value.Length == 0)
            {
                Filter = 0;
                JS.Write("document.all('all').checked=true;");
            }
            else
            {
                Filter = 3;
                JS.Write("document.all('rzdkv').checked=true;");
            }
            RefreshOstatkiTable(); 
            //SaveData();
        }

        protected void Vagon_Changed(object sender, ProperyChangedEventArgs e)
        {
            FilterValue = efVagon.Value.Trim();
            if (efVagon.Value.Length == 0)
            {
                Filter = 0;
                JS.Write("document.all('all').checked=true;");
            }
            else
            {
                Filter = 2;
                JS.Write("document.all('rvagon').checked=true;");
            }
            RefreshOstatkiTable(); 
            //SaveData();
        }

        #endregion

        public decimal GetNaborKol(int КодДвиженияНаСкладе, string ТипНабора)
        {
            object _nabor = NaborDoc.Compute("Sum(Количество)", ТипНабора + "=" + КодДвиженияНаСкладе);
            return _nabor == DBNull.Value ? 0m : Convert.ToDecimal(_nabor);
        }

        public string SetNaborKol(int КодДвиженияВДокументе, int КодДвиженияВНаборе, string val = "0")
        {
            if (val.Length == 0) val = "0";
            decimal kol = decimal.Parse(val);
            if (kol < 0) return Resx.GetString("_Msg_Количество0");
            decimal maxVal = GetMaxVal(КодДвиженияВДокументе, КодДвиженияВНаборе);
            if (kol > maxVal)
                return Resx.GetString("lblMaxValue") + ": " + maxVal.ToString("N" + this.Scale);

            DataRow r = NaborDoc.Rows.Find(new object[] { КодДвиженияВДокументе, КодДвиженияВНаборе });
            if (r == null)
            {
                if (kol > 0m)
                {
                    DataRow nr = NaborDoc.NewRow();
                    nr["КодНабора"] = -1;
                    nr["КодДвиженияВДокументе"] = КодДвиженияВДокументе;
                    nr["КодДвиженияВНаборе"] = КодДвиженияВНаборе;
                    DataRow ostRow = ost.Rows.Find(КодДвиженияВНаборе);
                    nr["ОтправкаВагона"] = ostRow["ОтправкаВагона"];
                    nr["Количество"] = kol;
                    nr["ТипТранзакции"] = ostRow["ТипТранзакции"];
                    NaborDoc.Rows.Add(nr);
                }
            }
            else
            {
                if (kol == 0m)
                    r.Delete();
                else
                    r["Количество"] = kol;
            }
            NaborDoc.AcceptChanges();
            return "";
        }

        public decimal GetNaborKol(int КодДвиженияВДокументе, int КодДвиженияВНаборе)
        {
            DataRow r = NaborDoc.Rows.Find(new object[] { КодДвиженияВДокументе, КодДвиженияВНаборе });
            if (r == null) return 0m;
            return Convert.ToDecimal(r["Количество"]);
        }

        public decimal GetMaxVal(int КодДвиженияВДокументе, int КодДвиженияВНаборе)
        {
            decimal КоличествоВКонтроле = GetNaborKol(КодДвиженияВДокументе, КодДвиженияВНаборе);

            decimal КоличествоВДокументе = Convert.ToDecimal(DvDoc.Rows.Find(КодДвиженияВДокументе)["Количество"]);

            decimal Набрано = GetNaborKol(КодДвиженияВДокументе, "КодДвиженияВДокументе") - КоличествоВКонтроле;

            decimal ОсталосьНабрать = КоличествоВДокументе - Набрано;
            return ОсталосьНабрать;
        }

        public void ArrangeDv(int КодДвиженияВДокументе)
        {
            decimal need, fakt;
            string ostField;

            // с удалением существующих наборов
            foreach (DataRow nr in this.NaborDoc.Select("КодДвиженияВДокументе=" + КодДвиженияВДокументе, ""))
                nr.Delete();
            NaborDoc.AcceptChanges();

            DataRow r = DvDoc.Rows.Find(КодДвиженияВДокументе);
            need = Convert.ToDecimal(r["Количество"]) - GetNaborKol(КодДвиженияВДокументе, "КодДвиженияВДокументе");
            foreach (DataRow ostRow in ost.Select("КодОтправкиВагона=" + (r["КодОтправкиВагона"].Equals(DBNull.Value) ? "-1" : r["КодОтправкиВагона"].ToString()), "КодДвиженияНаСкладе"))
            {
                ostField = Convert.ToInt32(ostRow["ТипПодписи"]) == 1 ? "Факт" : "Расчет";
                fakt = Math.Min(need, (ostRow[ostField] == DBNull.Value ? 0m : Convert.ToDecimal(ostRow[ostField])) - GetNaborKol(Convert.ToInt32(ostRow["КодДвиженияНаСкладе"]), "КодДвиженияВНаборе") - GetNaborKol(КодДвиженияВДокументе, Convert.ToInt32(ostRow["КодДвиженияНаСкладе"])));
                if (fakt <= 0) continue;
                SetNaborKol(Convert.ToInt32(r["КодДвиженияНаСкладе"]), Convert.ToInt32(ostRow["КодДвиженияНаСкладе"]), fakt.ToString());
                need -= fakt;
                if (need <= 0) break;
            }

            if (need <= 0m) return;
            foreach (DataRow ostRow in ost.Rows)
            {
                ostField = Convert.ToInt32(ostRow["ТипПодписи"]) == 1 ? "Факт" : "Расчет";
                fakt = Math.Min(need, (ostRow[ostField] == DBNull.Value ? 0m : Convert.ToDecimal(ostRow[ostField])) - GetNaborKol(Convert.ToInt32(ostRow["КодДвиженияНаСкладе"]), "КодДвиженияВНаборе"));
                if (fakt <= 0) continue;
                SetNaborKol(Convert.ToInt32(r["КодДвиженияНаСкладе"]), Convert.ToInt32(ostRow["КодДвиженияНаСкладе"]), fakt.ToString());
                need -= fakt;
                if (need <= 0) break;
            }
            NaborDoc.AcceptChanges();
        }

        public void ArrangeAll()
        {
            NaborDoc.Clear();
            decimal need, fakt;
            string ostField;
            //сначала распределяем по вагонам потом все остальное
            foreach (DataRow r in DvDoc.Select("КодОтправкиВагона is not null"))
            {
                need = Convert.ToDecimal(r["Количество"]);
                foreach (DataRow ostRow in ost.Select("КодОтправкиВагона=" + r["КодОтправкиВагона"]))
                {
                    ostField = Convert.ToInt32(ostRow["ТипПодписи"]) == 1 ? "Факт" : "Расчет";
                    fakt = Math.Min(need, (ostRow[ostField] == DBNull.Value ? 0m : Convert.ToDecimal(ostRow[ostField])) - GetNaborKol(Convert.ToInt32(ostRow["КодДвиженияНаСкладе"]), "КодДвиженияВНаборе"));
                    if (fakt <= 0) continue;
                    SetNaborKol(Convert.ToInt32(r["КодДвиженияНаСкладе"]), Convert.ToInt32(ostRow["КодДвиженияНаСкладе"]), fakt.ToString());
                    need -= fakt;
                    if (need <= 0) break;
                }
            }
            //затем распределим остальное
            foreach (DataRow r in DvDoc.Select("КодОтправкиВагона is null"))
            {
                need = Convert.ToDecimal(r["Количество"]) - GetNaborKol(Convert.ToInt32(r["КодДвиженияНаСкладе"]), "КодДвиженияВДокументе");
                if (need <= 0m) continue;
                foreach (DataRow ostRow in ost.Rows)
                {
                    ostField = Convert.ToInt32(ostRow["ТипПодписи"]) == 1 ? Resx.GetString("TTN_lblFact") : Resx.GetString("TTN_lblCalculeted");
                    fakt = Math.Min(need, (ostRow[ostField] == DBNull.Value ? 0m : Convert.ToDecimal(ostRow[ostField])) - GetNaborKol(Convert.ToInt32(ostRow["КодДвиженияНаСкладе"]), "КодДвиженияВНаборе"));
                    if (fakt <= 0) continue;
                    SetNaborKol(Convert.ToInt32(r["КодДвиженияНаСкладе"]), Convert.ToInt32(ostRow["КодДвиженияНаСкладе"]), fakt.ToString());
                    need -= fakt;
                    if (need <= 0) break;
                }
            }
            NaborDoc.AcceptChanges();
        }
        
        /// <summary>
        ///     Задание ссылки на справку
        /// </summary>
        protected override string HelpUrl { get; set; }
    }
}