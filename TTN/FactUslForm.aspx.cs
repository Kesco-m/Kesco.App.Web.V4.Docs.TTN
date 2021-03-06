﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Kesco.Lib.BaseExtention;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Entities.Documents.EF.Trade;
using Kesco.Lib.Entities.Persons.PersonOld;
using Kesco.Lib.Entities.Resources;
using Kesco.Lib.Log;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Web.Controls.V4.Common.DocumentPage;
using Kesco.Lib.Web.Settings;
using Kesco.Lib.Web.SignalR;
using Convert = Kesco.Lib.ConvertExtention.Convert;

namespace Kesco.App.Web.Docs.TTN
{
    /// <summary>
    ///     Класс формы FactUslForm
    /// </summary>
    public partial class FactUslForm : EntityPage
    {
        /// <summary>
        ///     Конструктор по умолчанию
        /// </summary>
        public FactUslForm()
        {
            HelpUrl = "hlp/help.htm?id=3";
        }

        /// <summary>
        ///     Задание ссылки на справку
        /// </summary>
        public override string HelpUrl { get; set; }

        protected override void EntityInitialization(Entity copy = null)
        {
            Entity = new FactUsl();
        }

        /// <summary>
        ///     Событие загрузки страницы
        /// </summary>
        /// <param name="sender">Объект страницы</param>
        /// <param name="e">Аргументы</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!V4IsPostBack)
                BindField();

            efResource.BeforeSearch += Res_BeforeSearch;
            efUnitAdv.BeforeSearch += UnitAdv_BeforeSearch;
            efStavkaNDS.BeforeSearch += StavkaNDS_BeforeSearch;

            efResourceRus.OnRenderNtf += ResourceRus_OnRenderNtf;
            efResourceLat.OnRenderNtf += ResourceLat_OnRenderNtf;

            efUnitAdv.PreRender += UnitAdv_PreRender;
            efStavkaNDS.PreRender += StavkaNDS_PreRender;
            efCostOutNDS.PreRender += CostOutNDS_PreRender;
            efSummaOutNDS.PreRender += SummaOutNDS_PreRender;
            efSummaNDS.PreRender += SummaNDS_PreRender;
            efVsego.PreRender += Vsego_PreRender;
            efCount.PreRender += Count_PreRender;
            efCurrency.PreRender += Currency_PreRender;

            //efCount.Precision = factUsl.ResourceId > 0 ? factUsl.Resource.GetScale4Unit(factUsl.UnitId.ToString(), 3, Document.GOPersonDataField.Value.ToString()) : 3;

            //efCostOutNDS.Precision =
            //efSummaOutNDS.Precision =
            //efSummaNDS.Precision =
            //efVsego.Precision = Scale;

            DocumentReadOnly = !((DocPage) ParentPage).DocEditable;
            JS.Write("ShowControl('trUnitAdv','{0}')", DocumentReadOnly);
        }

        #region InitControls

        /// <summary>
        ///     Инициализация контролов
        /// </summary>
        private void SetInitValue()
        {
            efCount.Precision = factUsl.ResourceId > 0
                ? factUsl.Resource.GetScale4Unit(factUsl.UnitId.ToString(), 3,
                    Document.PlatelschikField.Value.ToString())
                : 3;

            efCostOutNDS.Precision =
                efSummaOutNDS.Precision =
                    efSummaNDS.Precision =
                        efVsego.Precision = Scale;

            var orderList = new Dictionary<string, object>();
            var sqlParams = new Dictionary<string, object> {{"@КодДокумента", Document.Id}};
            var curOrder = 0;
            var dt = DBManager.GetData(SQLQueries.SELECT_ID_DOC_ОказанныеУслуги_GRID, Config.DS_document,
                CommandType.Text, sqlParams);
            var isNew = factUsl.PositionId == null || factUsl.PositionId.Value == 0;
            if (dt == null || !isNew && dt.Rows.Count < 2 || isNew && dt.Rows.Count < 1)
            {
                OrderPanel.Visible = false;
            }
            else
            {
                orderList.Add("0", Resx.GetString("TTN_lblPutOnFirstPosition"));
                foreach (DataRow row in dt.Rows)
                {
                    if (isNew || row["КодОказаннойУслуги"].ToString() != factUsl.PositionId.Value.ToString())
                        orderList.Add(row["КодОказаннойУслуги"].ToString(),
                            row["РесурсРус"] + " [" + row["ЦенаБезНДС"] + "]");

                    if (!isNew && row["КодОказаннойУслуги"].ToString() == factUsl.PositionId.Value.ToString())
                        curOrder = orderList.Count;
                }
            }

            efOrder.DataItems = orderList;
            if (orderList.Count > 1)
            {
                if (factUsl.PositionId == null || factUsl.PositionId.Value == 0)
                {
                    efOrder.Value = orderList.ElementAt(orderList.Count - 1).Key;
                }
                else
                {
                    if (curOrder > 0) curOrder = curOrder - 1;
                    efOrder.Value = orderList.ElementAt(curOrder).Key;
                }
            }
        }

        #endregion

        #region Validation

        /// <summary>
        ///     Валидация контролов
        /// </summary>
        /// <returns></returns>
        private bool Validation()
        {
            var title = Resx.GetString("TTN_msgDocumentCanNotBeSaved");
            if (factUsl.ResourceId == 0)
            {
                // Не заполнено поле 'Услуга'."
                ShowMessage(Resx.GetString("TTN_ntfNotService"), title);
                efResource.Focus();
                return false;
            }

            if (factUsl.ResourceRus.Length == 0)
            {
                // Не заполнено поле 'Русское название продукта'
                ShowMessage(Resx.GetString("TTN_ntfNotResourceRus"), title);
                efResourceRus.Focus();
                return false;
            }

            if (factUsl.Count == 0)
            {
                // Не заполнено поле 'Количество'
                ShowMessage(Resx.GetString("TTN_ntfNotCount"), title);
                efCount.Focus();
                return false;
            }

            if (factUsl.CostOutNDS.ToString(CultureInfo.InvariantCulture).Length == 0)
            {
                // Не заполнено поле 'Цена без НДС'
                ShowMessage(Resx.GetString("TTN_ntfNotCostOutNDS"), title);
                efCostOutNDS.Focus();
                return false;
            }

            if (factUsl.CostOutNDS == 0)
            {
                // Цена без НДС должна быть больше или меньше 0
                ShowMessage(Resx.GetString("TTN_ntfCostOutNDSIncorrect"), title);
                efCostOutNDS.Focus();
                return false;
            }

            if (factUsl.StavkaNDSId == null)
            {
                // Не заполнено поле 'Cтавка НДС'
                ShowMessage(Resx.GetString("TTN_ntfNotStavkaNDS"), title);
                efStavkaNDS.Focus();
                return false;
            }

            if (factUsl.SummaOutNDS.ToString(CultureInfo.InvariantCulture).Length == 0)
            {
                // Не заполнено поле 'Сумма без НДС'
                ShowMessage(Resx.GetString("TTN_ntfNotSummaOutNDS"), title);
                efSummaOutNDS.Focus();
                return false;
            }

            if (factUsl.SummaOutNDS == 0)
            {
                // Сумма без НДС должна быть больше или меньше 0
                ShowMessage(Resx.GetString("TTN_ntfSummaOutNDSIncorrect"), title);
                efSummaOutNDS.Focus();
                return false;
            }

            if (factUsl.Vsego.ToString(CultureInfo.InvariantCulture).Length == 0)
            {
                // Не заполнено поле 'Всего'
                ShowMessage(Resx.GetString("TTN_ntfNotVsego"), title);
                efVsego.Focus();
                return false;
            }

            if (factUsl.Vsego == 0)
            {
                // Значение поля 'Всего' должно быть больше или меньше 0
                ShowMessage(Resx.GetString("TTN_ntfVsegoIncorrect"), title);
                efVsego.Focus();
                return false;
            }

            return true;
        }

        #endregion

        private bool CheckPersonBProject(string _p)
        {
            if (_p.Length == 0) return false;

            //Person p = new Person(_p);
            var p = ParentPage.GetObjectById(typeof(PersonOld), _p) as PersonOld;
            if (p == null || p.Unavailable) return false;
            if (p.BusinessProjectID <= 0) return false;

            return true;
        }

        /// <summary>
        ///     Отображает диалоговое окно выбора вида расчета
        /// </summary>
        /// <param name="name">Название изменяемого поля</param>
        /// <param name="value">Новое значение изменяемого поля</param>
        /// <param name="ndx">Индекс</param>
        private void DialogCostRecalc(string name, string value, string ndx)
        {
            ShowConfirm(Resx.GetString("TTN_msgNDSSUM"), Resx.GetString("TTN_msgChoiceCalculationType"),
                Resx.GetString("QSBtnYes"), Resx.GetString("QSBtnNo")
                , "dialogRecalc('DialogCostRecalc_Yes','" + name + "','" + value + "','" + ndx + "');"
                , "dialogRecalc('DialogCostRecalc_No','" + name + "','" + value + "','" + ndx + "');"
                , "btnSave", null);
        }

        /// <summary>
        ///     Отображает диалоговое окно выбора типа перерасчета
        /// </summary>
        /// <param name="name">Название изменяемого поля</param>
        /// <param name="value">Новое значение изменяемого поля</param>
        /// <param name="ndx">Индекс</param>
        private void DialogRecalc(string name, string value, string ndx)
        {
            var helpText = string.Format(@"
            Нажмите:<br>
					<b>{0}</b> - {1};
					<b>{2}</b> - {3};
					<b>{4}</b> - {5};
					<b>{6}</b> - {7}",
                "OK",
                Resx.GetString("TTN_msgOKInfo"),
                Resx.GetString("QSBtnRecalc"),
                Resx.GetString("TTN_msgRecalcInfo"),
                Resx.GetString("QSBtnCancel"),
                Resx.GetString("TTN_msgCancel"),
                Resx.GetString("QSBtnChange"),
                Resx.GetString("TTN_msgChange")
            );

            ShowRecalc(helpText, Resx.GetString("TTN_msgOKInfo"), "ОК", Resx.GetString("QSBtnRecalc"),
                Resx.GetString("QSBtnCancel"), Resx.GetString("QSBtnChange")
                , "dialogRecalc('DialogRecalc_Yes','" + name + "','" + value + "','" + ndx + "');"
                , "dialogRecalc('DialogRecalc_Recalc','" + name + "','" + value + "','" + ndx + "');"
                , "dialogRecalc('DialogRecalc_No','" + name + "','" + value + "','" + ndx + "');"
                , "dialogRecalc('DialogRecalc_Change','" + name + "','" + value + "','" + ndx + "');"
                , null, 500);
        }

        /// <summary>
        ///     Обработка клиентских команд
        /// </summary>
        /// <param name="cmd">Команды</param>
        /// <param name="param">Параметры</param>
        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "SaveAndClose":
                    SaveData(true);
                    break;
                case "SaveData":
                    SaveData(false);
                    break;
                case "DeleteData":
                    DeleteData();
                    break;
                case "CloseWindow":
                    JS.Write("parent.resources_Records_Close();");
                    break;
                case "DialogCostRecalc_Yes":
                    var d_kol = factUsl.Count > 0 && !factUsl.Count.Equals("0") ? factUsl.Count : 1;

                    var _costOutNDS = factUsl.CostOutNDS;
                    decimal _summaOutNDS = 0;
                    decimal _summaNDS = 0;
                    decimal _vsego = 0;
                    var stavka = factUsl.StavkaNDS;
                    var prst = (decimal) stavka.Величина * 100;
                    var scale = Document != null && !Document.Unavailable ? Document.CurrencyScale : 2;

                    _vsego = Convert.Round((decimal) (d_kol * (double) _costOutNDS), scale);
                    _summaNDS = Convert.Round(_vsego / (100 + prst) * prst, scale);
                    _summaOutNDS = _vsego - _summaNDS;
                    _costOutNDS = Convert.Round((decimal) ((double) _summaOutNDS / d_kol), scale * 2);

                    var maxscale =
                        factUsl.Resource.GetScale4Unit(efUnit.Value, 3, Document.PlatelschikField.Value.ToString());
                    factUsl.Count = d_kol;
                    efCount.Value = Convert.Decimal2Str((decimal) factUsl.Count, maxscale, false);

                    factUsl.CostOutNDS = _costOutNDS;
                    efCostOutNDS.Value = Convert.Decimal2Str(factUsl.CostOutNDS, scale * 2, false);
                    factUsl.SummaOutNDS = Convert.Round(_summaOutNDS, scale);
                    efSummaOutNDS.Value = Convert.Decimal2Str(factUsl.SummaOutNDS, scale, false);
                    factUsl.SummaNDS = Convert.Round(_summaNDS, scale);
                    efSummaNDS.Value = Convert.Decimal2Str(factUsl.SummaNDS, scale, false);
                    factUsl.Vsego = Convert.Round(_vsego, scale);
                    efVsego.Value = Convert.Decimal2Str(factUsl.Vsego, scale, false);

                    break;
                case "DialogCostRecalc_No":
                    ShowCalcMessage(factUsl.Recalc(param["value"], param["ndx"], param["name"], "0", Scale));
                    break;
                case "DialogRecalc_Yes":
                    ShowCalcMessage(factUsl.Recalc(param["value"], param["ndx"], param["name"], "0", Scale));
                    break;
                case "DialogRecalc_Recalc":
                    ShowCalcMessage(factUsl.Recalc(param["value"], param["ndx"], param["name"], "1", Scale));
                    break;
                case "DialogRecalc_No":
                    ShowCalcMessage(factUsl.Recalc(param["value"], param["ndx"], param["name"], "2", Scale));
                    break;
                case "DialogRecalc_Change":
                    ShowCalcMessage(factUsl.Recalc(param["value"], param["ndx"], param["name"], "3", Scale));
                    break;
            }
        }

        private void ShowCalcMessage(string message)
        {
            if (!message.IsNullEmptyOrZero())
                ShowMessage(Resx.GetString(message), Resx.GetString("errDoisserWarrning"));
        }

        #region declaration, property, field

        protected FactUsl factUsl;
        protected string id;
        protected string idDoc;
        protected string idParentPage;
        protected string ue;

        // регион - рф
        public const int RegionRussia = 188;

        /// <summary>
        ///     ТТН
        /// </summary>
        private Lib.Entities.Documents.EF.Trade.TTN Document => ((Nakladnaya) ParentPage).Document;

        /// <summary>
        ///     Точность вывода валют
        /// </summary>
        private int Scale => Document.CurrencyScale;

        /// <summary>
        ///     Определяет режим редактирования
        /// </summary>
        private bool DocumentReadOnly
        {
            set
            {
                efResource.IsReadOnly = value;
                efResourceRus.IsReadOnly = value;
                efResourceLat.IsReadOnly = value;
                efUnitAdv.IsReadOnly = value;
                chAgent1.IsReadOnly = value;
                chAgent2.IsReadOnly = value;
                efCount.IsReadOnly = value;
                efCostOutNDS.IsReadOnly = value;
                efStavkaNDS.IsReadOnly = value;
                efSummaOutNDS.IsReadOnly = value;
                efSummaNDS.IsReadOnly = value;
                efVsego.IsReadOnly = value;
                if (OrderPanel.Visible) OrderPanel.Visible = !value;
            }
            get { return !((DocPage) ParentPage).DocEditable; }
        }

        #endregion

        #region Binder

        /// <summary>
        ///     инициализация контролов
        /// </summary>
        protected override void EntityFieldInit()
        {
            if (!V4IsPostBack)
            {
                id = Request.QueryString["id"];
                idDoc = Request.QueryString["idDoc"];
                idParentPage = Request.QueryString["idpp"];
                ue = Request.QueryString["ue"];

                ParentPage = KescoHub.GetPage(idParentPage) as DocPage;
                if (ParentPage == null)
                {
                    ShowMessage(Resx.GetString("errRetrievingPageObject"), Resx.GetString("errPrinting"),
                        MessageStatus.Error);
                    return;
                }

                if (!string.IsNullOrEmpty(id) && id != "0")
                {
                    factUsl = new FactUsl(id);
                    //factUsl = GetObjectById(typeof(FactUsl), id) as FactUsl;
                    if (factUsl == null || factUsl.Id == "0")
                        throw new LogicalException(Resx.GetString("TTN_ ERRMoveStockInitialized"), "",
                            Assembly.GetExecutingAssembly().GetName(), Priority.Info);
                }
                else
                {
                    factUsl = new FactUsl {DocumentId = int.Parse(idDoc)};
                    RenderAgent1();
                    RenderAgent2();
                    efChanged.ChangedByID = null;
                }

                SetInitValue();
            }

            Entity = factUsl;

            efResource.BindStringValue = factUsl.ResourceIdBind;
            efResourceRus.BindStringValue = factUsl.ResourceRusBind;
            chAgent1.BindStringValue = factUsl.Agent1Bind;
            chAgent2.BindStringValue = factUsl.Agent2Bind;
            efResourceLat.BindStringValue = factUsl.ResourceLatBind;
            efUnit.BindStringValue = factUsl.UnitIdBind;
            efCount.BindStringValue = factUsl.CountBind;
            efMCoef.BindStringValue = factUsl.CoefBind;
            efCostOutNDS.BindStringValue = factUsl.CostOutNDSBind;
            efStavkaNDS.BindStringValue = factUsl.StavkaNDSIdBind;
            efSummaOutNDS.BindStringValue = factUsl.SummaOutNDSBind;
            efSummaNDS.BindStringValue = factUsl.SummaNDSBind;
            efVsego.BindStringValue = factUsl.VsegoBind;

            base.EntityFieldInit();
        }

        /// <summary>
        ///     Заполнение контролов данными объекта
        /// </summary>
        private void BindField()
        {
            RenderOsnUnit();
            RenderAdvUnit();

            RenderAgent1();
            RenderAgent2();

            efChanged.SetChangeDateTime = factUsl.ChangedTime;
            efChanged.ChangedByID = factUsl.ChangedId;
        }

        #endregion

        #region OnRenderNtf

        /// <summary>
        ///     Нотификация наименования продукта (русское)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ntf"></param>
        private void ResourceRus_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (factUsl.ResourceId == 0) return;
            if (!efResourceRus.Value.Equals(factUsl.Resource.Name))
                ntf.Add(new Notification
                {
                    Message = Resx.GetString("TTN_ntfRussianNameNotMatchProduct"),
                    Status = NtfStatus.Error,
                    DashSpace = true
                });
        }

        /// <summary>
        ///     Нотификация наименования продукта (латвийское)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ntf"></param>
        private void ResourceLat_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (factUsl.ResourceId == 0 || factUsl.Resource.Unavailable) return;
            if (!efResourceLat.Value.Equals(factUsl.Resource.ResourceLat))
                ntf.Add(new Notification
                {
                    Message = Resx.GetString("TTN_ntfLatinNameNotMatchProduct"),
                    Status = NtfStatus.Error,
                    DashSpace = true
                });
        }

        #endregion

        #region BeforeSearch

        /// <summary>
        ///     Событие, устанавливающее параметры фильтрации перед поиском услуг в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        private void Res_BeforeSearch(object sender)
        {
            efResource.Filter.AllChildrenWithParentIDs.Clear();
            efResource.Filter.AllChildrenWithParentIDs.Value = "3";
        }

        /// <summary>
        ///     Событие, устанавливающее параметры фильтрации перед поиском дополнительных единиц измерения в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        private void UnitAdv_BeforeSearch(object sender)
        {
            if (factUsl.ResourceId == 0) return;
            efUnitAdv.Filter.Resource = factUsl.ResourceId;
        }

        /// <summary>
        ///     Событие, устанавливающее параметры фильтрации перед поиском ставок в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        private void StavkaNDS_BeforeSearch(object sender)
        {
            efStavkaNDS.Filter.TerritoryCode = RegionRussia;
        }

        #endregion

        #region Changed

        /// <summary>
        ///     Событие, отслеживающее изменение контрола Услуга
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Resource_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (e.OldValue.Equals(e.NewValue)) return;
            ClearUnits();

            if (factUsl.ResourceId == 0)
            {
                efResourceRus.Value =
                    efResourceLat.Value = "";
                efResourceRus.RenderNtf();
                efResourceLat.RenderNtf();
            }
            else
            {
                efResourceRus.Value = factUsl.Resource.Name;
                efResourceLat.Value = factUsl.Resource.ResourceLat;

                efResourceRus.RenderNtf();
                var efUnitAdvOld = efUnitAdv.Value;
                efUnitAdv.TryFindSingleValue();
                UnitAdv_Changed(null, new ProperyChangedEventArgs(efUnitAdvOld, efUnitAdv.Value));
            }
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола количества
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Count_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (efCount.Value.Trim().Length == 0) return;
            ShowCalcMessage(factUsl.Recalc(e.OldValue, "3", "Count", "0", Scale));
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола цены без НДС
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void CostOutNDS_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (efCostOutNDS.Value.Trim().Length >= 0)
            {
                if (factUsl.StavkaNDSId > 0
                    && e.NewValue.Length > 0
                    && factUsl.StavkaNDSId > 2
                    && factUsl.CostOutNDS != 0)
                    DialogCostRecalc("CostOutNDS", e.OldValue, "2");
                else
                    ShowCalcMessage(factUsl.Recalc(e.OldValue, "2", "CostOutNDS", "0", Scale));
            }
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола ставки НДС
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void StavkaNDS_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (efStavkaNDS.Value.Trim().Length >= 0)
            {
                if (factUsl.CostOutNDS > 0
                    && !e.NewValue.Equals(e.OldValue)
                    && e.NewValue.Length > 0
                    && factUsl.StavkaNDSId > 2
                    && factUsl.CostOutNDS != 0)
                    DialogCostRecalc("StavkaNDS", e.OldValue, "2");
                else
                    ShowCalcMessage(factUsl.Recalc(e.OldValue, "2", "StavkaNDS", "0", Scale));
            }
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола суммы без НДС
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void SummaOutNDS_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (factUsl.StavkaNDSId != 0)
                DialogRecalc("SummaOutNDS", e.OldValue, "4");
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола суммы НДС
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void SummaNDS_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (factUsl.StavkaNDSId != 0)
                DialogRecalc("SummaNDS", e.OldValue, "5");
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола всего
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Vsego_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (factUsl.StavkaNDSId != 0)
                DialogRecalc("Vsego", e.OldValue, "8");
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола дополнительной единицы измерения
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void UnitAdv_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (efUnitAdv.Value.Length == 0)
            {
                factUsl.UnitId = null;
                efUnit.RefreshRequired = true;
                return;
            }

            SetUnitInfo(efUnitAdv.Value);

            efUnitAdv.Value = "";
            SetAdvUnitInfo(e.OldValue);
        }

        #endregion

        #region PreRender

        /// <summary>
        ///     PreRender для количества
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Count_PreRender(object sender, EventArgs e)
        {
            var maxscale = Scale;


            if (!factUsl.UnitId.ToString().IsNullEmptyOrZero())
                maxscale = factUsl.Resource.GetScale4Unit(factUsl.UnitId.ToString(), 3,
                    Document.PlatelschikField.Value.ToString());
            efCount.Value = Convert.Decimal2StrInit((decimal) factUsl.Count, maxscale);
            efCount.Precision = maxscale;
        }

        /// <summary>
        ///     PreRender для цены без НДС
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CostOutNDS_PreRender(object sender, EventArgs e)
        {
            efCostOutNDS.Value = Convert.Decimal2StrInit(factUsl.CostOutNDS, Scale);
        }

        /// <summary>
        ///     PreRender для суммы без НДС
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SummaOutNDS_PreRender(object sender, EventArgs e)
        {
            efSummaOutNDS.Value = Convert.Decimal2StrInit(factUsl.SummaOutNDS, Scale);
        }

        /// <summary>
        ///     PreRender для суммы НДС
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SummaNDS_PreRender(object sender, EventArgs e)
        {
            efSummaNDS.Value = Convert.Decimal2StrInit(factUsl.SummaNDS, Scale);
        }

        /// <summary>
        ///     PreRender для поля "всего"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vsego_PreRender(object sender, EventArgs e)
        {
            efVsego.Value = Convert.Decimal2StrInit(factUsl.Vsego, Scale);
        }

        /// <summary>
        ///     PreRender для ставки НДС
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StavkaNDS_PreRender(object sender, EventArgs e)
        {
            if (!DocumentReadOnly)
            {
                if (factUsl.Agent1.ToString() == "1")
                    efStavkaNDS.IsReadOnly = true;
                else
                    efStavkaNDS.IsReadOnly = false;
            }
        }

        /// <summary>
        ///     PreRender для выбора ед. изм.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UnitAdv_PreRender(object sender, EventArgs e)
        {
            if (!DocumentReadOnly)
            {
                if (factUsl.Agent1.ToString() == "1")
                    efUnitAdv.IsReadOnly = true;
                else
                    efUnitAdv.IsReadOnly = false;
            }
        }

        #endregion

        #region Render

        /// <summary>
        ///     Рендер Агент Поставщик
        /// </summary>
        protected void RenderAgent1()
        {
            if (factUsl.DocumentId == 0)
            {
                efAgent1.Value = Resx.GetString("lblPerson") + "1 - " + Resx.GetString("TTN_msgAgent");
                return;
            }

            if (!Document.IsNew && Document.Unavailable)
            {
                efAgent1.Value = Resx.GetString("lblPerson") + "1 - " + Resx.GetString("TTN_msgAgent");
                return;
            }

            if (!CheckPersonBProject(Document.DocumentData.PersonId1.ToString()))
            {
                Agent1Panel.Visible = false;
                return;
            }

            switch (Document.Type)
            {
                case DocTypeEnum.ТоварноТранспортнаяНакладная:
                    if (Document.PostavschikDataField.Value.ToString().Length == 0 ||
                        Document.PostavschikField.Unavailable)
                        efAgent1.Value = Document.PostavschikField.Name + " - " + Resx.GetString("TTN_msgAgent");
                    else
                        efAgent1.Value = Document.PostavschikDataField.Value + " - " + Resx.GetString("TTN_msgAgent");
                    break;
                case DocTypeEnum.АктВыполненныхРаботУслуг:
                    //Agent1.Value = Document.PostavschikDataField.Value.ToString();
                    break;
            }
        }

        /// <summary>
        ///     Рендер Агент Плательщик
        /// </summary>
        protected void RenderAgent2()
        {
            if (factUsl.DocumentId == 0)
            {
                efAgent2.Value = Resx.GetString("lblPerson") + "2 - " + Resx.GetString("TTN_msgAgent");
                return;
            }

            if (!Document.IsNew && Document.Unavailable)
            {
                efAgent2.Value = Resx.GetString("lblPerson") + "2 - " + Resx.GetString("TTN_msgAgent");
                return;
            }

            if (!CheckPersonBProject(Document.DocumentData.PersonId2.ToString()))
            {
                Agent2Panel.Visible = false;
                return;
            }

            switch (Document.Type)
            {
                case DocTypeEnum.ТоварноТранспортнаяНакладная:
                    if (Document.PlatelschikDataField.Value.ToString().Length == 0 ||
                        Document.PlatelschikField.Unavailable)
                        efAgent2.Value = Document.PlatelschikField.Name + " - " + Resx.GetString("TTN_msgAgent");
                    else
                        efAgent2.Value = Document.PlatelschikDataField.Value + " - " + Resx.GetString("TTN_msgAgent");
                    break;
                case DocTypeEnum.АктВыполненныхРаботУслуг:
                    //Agent2.Value = Document.PlatelschikDataField.Value.ToString();
                    break;
            }
        }

        /// <summary>
        ///     Рендер валюты
        /// </summary>
        protected void Currency_PreRender(object sender, EventArgs e)
        {
            switch (factUsl.Document.Type)
            {
                case DocTypeEnum.ТоварноТранспортнаяНакладная:
                    if (!Document.Currency.Id.IsNullEmptyOrZero() && ue == "0")
                    {
                        efCurrency.InnerText = Document.Currency.Name;
                    }
                    else
                    {
                        efCurrency.InnerText = "у.е.";
                    }
                    break;
                case DocTypeEnum.АктВыполненныхРаботУслуг:
                    //Currency.Value = (akt._Currency.Length > 0 && UE == 0) ? akt.Currency._Name : "у.е.";
                    break;
            }
        }

        /// <summary>
        ///     Рендер поля Эквивалент
        /// </summary>
        protected void RenderOsnUnit()
        {
            if (factUsl.Coef == 0 || factUsl.ResourceId == 0 || factUsl.Resource == null ||
                factUsl.Resource.Unavailable)
            {
                efOsnUnit.Value = "";
                JS.Write("ShowControl('trEquivalent','True');");
                return;
            }

            var resourceUnit = GetObjectById(typeof(Unit), factUsl.Resource.UnitCode.ToString()) as Unit;

            if (resourceUnit == null || resourceUnit.Id.IsNullEmptyOrZero() || resourceUnit.Unavailable)
            {
                efOsnUnit.Value = "";
                JS.Write("ShowControl('trEquivalent','True');");
                return;
            }


            if (resourceUnit.Id == factUsl.UnitId.ToString())
            {
                JS.Write("ShowControl('trEquivalent','True');");
                return;
            }

            JS.Write("ShowControl('trEquivalent','False');");
            efOsnUnit.Value = "&nbsp;" + factUsl.Resource.Unit.ЕдиницаРус;
        }

        /// <summary>
        ///     Рендер поля Эквивалент
        /// </summary>
        protected void RenderAdvUnit()
        {
            if (factUsl.UnitId == 0)
            {
                efAdvUnit.Value = "";
                return;
            }

            var factUslUnit = GetObjectById(typeof(Unit), factUsl.UnitId.ToString()) as Unit;

            if (factUslUnit == null || factUslUnit.Unavailable)
            {
                efAdvUnit.Value = "";
                return;
            }

            efAdvUnit.Value = "1&nbsp;" + factUslUnit.ЕдиницаРус + "&nbsp;=&nbsp;";
        }

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
                    var btnEdit = MenuButtons.Find(btn => btn.ID == "btnEdit");
                    RemoveMenuButton(btnEdit);

                    var btnSave = MenuButtons.Find(btn => btn.ID == "btnSave");
                    btnSave.Text = Resx.GetString("cmdOK") + "&nbsp;(F2)";
                    btnSave.OnClick = "cmdasync('cmd', 'SaveAndClose');";
                    if (!((DocPage) ParentPage).DocEditable) RemoveMenuButton(btnSave);

                    var btnApply = MenuButtons.Find(btn => btn.ID == "btnApply");
                    btnApply.OnClick = "cmdasync('cmd', 'SaveData');";
                    if (!((DocPage) ParentPage).DocEditable) RemoveMenuButton(btnApply);

                    var btnReCheck = MenuButtons.Find(btn => btn.ID == "btnReCheck");
                    RemoveMenuButton(btnReCheck);

                    if (((DocPage) ParentPage).DocEditable && factUsl.PositionId != null && factUsl.PositionId != 0)
                    {
                        var btnClear = new Button
                        {
                            ID = "btnDelete",
                            V4Page = this,
                            Text = Resx.GetString("cmdDelete"),
                            Title = Resx.GetString("cmdDeleteTitle"),
                            IconJQueryUI = ButtonIconsEnum.Delete,
                            Width = 105,
                            OnClick =
                                string.Format("if(confirm('{0} {1}?')) cmdasync('cmd', 'DeleteData');",
                                    Resx.GetString("msgDeleteConfirm"), factUsl.ResourceRus)
                        };

                        AddMenuButton(btnClear);
                    }

                    RenderButtons(w);
                }
                catch (Exception e)
                {
                    var dex = new DetailedException(Resx.GetString("TTN_errFailedGenerateButtons") + ": " + e.Message,
                        e);
                    Logger.WriteEx(dex);
                    throw dex;
                }

                return w.ToString();
            }
        }

        protected string DivHeaderClass()
        {
            if (DocumentReadOnly) return "label";
            return "";
        }

        #endregion

        #region MenuButtons

        /// <summary>
        ///     Очистка всех данных формы
        /// </summary>
        private void DeleteData()
        {
            factUsl.Delete(false);
            JS.Write("parent.factUsl_Records_Save(\"addFactUsl\", \"False\", \"True\", \"False\");");
        }

        /// <summary>
        ///     Кнопка: Сохранить
        /// </summary>
        private void SaveData(bool closeForm)
        {
            if (Validation())
            {
                var isNew = factUsl.Id.IsNullEmptyOrZero();

                if (Document.PositionFactUsl != null && Document.PositionFactUsl.Count > 0 &&
                    !efOrder.Value.IsNullEmptyOrZero() && efOrder.Value != "0")
                    factUsl.Order = Document.PositionFactUsl
                                        .Find(m => m.PositionId == System.Convert.ToInt32(efOrder.Value)).Order + 1;
                else
                    factUsl.Order = 1;

                var ctrlFocus = factUsl.Id.IsNullEmptyOrZero() ? "" : "addFactUsl";

                var reloadParentForm = false;
                if (factUsl.DocumentId == 0 || Document.IsModified)
                {
                    List<DBCommand> cmds = null;
                    Document.Save(false, cmds);
                    factUsl.DocumentId = Document.DocId;
                    reloadParentForm = true;
                    closeForm = true;
                }

                factUsl.Save(false);
                if (OrderPanel.Visible && efOrder.Value != "")
                {
                    var nextPosition = System.Convert.ToInt32(efOrder.Value);
                    if (!(factUsl.PositionId > nextPosition &&
                          factUsl.PositionId > Document.PositionMris.Max(e => e.PositionId)))
                        factUsl.ReOrder(nextPosition);
                }

                JS.Write("parent.factUsl_Records_Save('{0}','{1}','{2}','{3}');", ctrlFocus, reloadParentForm,
                    closeForm, isNew);

                if (!closeForm)
                {
                    SetCurrentUrlParams(new Dictionary<string, object> {{"id", factUsl.Id}});
                    RefreshPage();
                }
            }
        }

        #endregion

        #region Unit

        /// <summary>
        ///     Установка единицы измерения по ресурсу при выборе доп. единицы измерения
        /// </summary>
        /// <param name="_unit">ед.изм.</param>
        private void SetUnitInfo(string _unit)
        {
            if (factUsl.ResourceId == 0 || factUsl.Resource == null || factUsl.Resource.Unavailable) return;

            if (_unit.Equals("10000001")) SetUnitInfoByOsnUnit(factUsl.Resource);
            else SetUnitInfoByAdvUnit(factUsl.Resource, _unit);
            RenderOsnUnit();
            RenderAdvUnit();
        }

        /// <summary>
        ///     Установка единицы измерения по ресурсу
        /// </summary>
        /// <param name="res">Ресурс</param>
        private void SetUnitInfoByOsnUnit(Resource res)
        {
            ClearUnits();
            factUsl.UnitId = res.UnitCode;
            efUnit.RefreshRequired = true;
            factUsl.Coef = 1;
            efMCoef.Value = factUsl.Coef.ToString();
        }

        /// <summary>
        ///     Заполнения объекта данными ед.изм. и коэффициента
        /// </summary>
        /// <param name="res">Ресурс</param>
        /// <param name="_unit">Ед.изм.</param>
        private void SetUnitInfoByAdvUnit(Resource res, string _unit)
        {
            ClearUnits();

            //UnitAdv rxu = new UnitAdv(_unit);
            var rxu = GetObjectById(typeof(UnitAdv), _unit) as UnitAdv;
            if (rxu == null || rxu.Unavailable) return;

            factUsl.UnitId = rxu.Unit.КодЕдиницыИзмерения;
            efUnit.RefreshRequired = true;
            //efUnit.Value = factUsl.UnitId.ToString();
            factUsl.Coef = rxu.Коэффициент;
            efMCoef.Value = factUsl.Coef.ToString();
        }

        /// <summary>
        ///     Пересчет полей в соответствии с доп. ед.изм.
        /// </summary>
        private void SetAdvUnitInfo(string _oldVal)
        {
            var oldVal = 0;
            int.TryParse(_oldVal, out oldVal);
            if (oldVal == 0) return;

            //Unit old = new Unit(_oldVal.ToString(CultureInfo.InvariantCulture));
            var old = GetObjectById(typeof(Unit), _oldVal.ToString(CultureInfo.InvariantCulture)) as Unit;
            if (old != null && factUsl.Count > 0 && factUsl.ResourceId > 0)
            {
                var maxscale =
                    factUsl.Resource.GetScale4Unit(efUnit.Value, 3, Document.PlatelschikField.Value.ToString());
                factUsl.Count = Math.Round(factUsl.Count / factUsl.Resource.ConvertionCoefficient(old, factUsl.Unit),
                    maxscale);
                efCount.Value = Convert.Decimal2StrInit((decimal) factUsl.Count, maxscale);
                factUsl.CostOutNDS =
                    Math.Round(factUsl.CostOutNDS * (decimal) factUsl.Resource.ConvertionCoefficient(old, factUsl.Unit),
                        Scale);
                efCostOutNDS.Value = factUsl.CostOutNDS.ToString(CultureInfo.InvariantCulture);
                ShowCalcMessage(factUsl.Recalc(factUsl.CostOutNDS.ToString(), "2", "CostOutNDS", "0", Scale));
            }
        }

        /// <summary>
        ///     Очистка контролов ед.изм.
        /// </summary>
        private void ClearUnits()
        {
            efUnitAdv.Value = "";
            factUsl.UnitId = 0;
            efUnit.RefreshRequired = true;
            factUsl.Coef = null;

            RenderOsnUnit();
            RenderAdvUnit();
        }

        #endregion
    }
}