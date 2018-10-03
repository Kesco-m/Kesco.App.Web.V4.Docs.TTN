using System;
using System.Globalization;
using System.IO;
using System.Collections.Specialized;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.Entities.Persons;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Entities.Resources;
using Kesco.Lib.Entities.Documents.EF.Trade;
using Kesco.Lib.Log;
using System.Text.RegularExpressions;

namespace Kesco.App.Web.Docs.TTN
{
    public partial class FactUslForm : EntityPage
    {
        #region declaration, property, field

        protected FactUsl factUsl;
        protected string id;
        protected string idDoc;
        protected string idParentPage;

        // регион - рф
        public const int RegionRussia = 188;

        /// <summary>
        /// Вызывающая страница (накладная)
        /// </summary>
        protected Nakladnaya ParentPage;

        /// <summary>
        /// ТТН
        /// </summary>
        private Lib.Entities.Documents.EF.Trade.TTN Document
        {
            get { return ParentPage.Document; }
        }

        /// <summary>
        /// Точность вывода валют
        /// </summary>
        private int Scale
        {
            get { return Document.CurrencyScale != null ? Document.CurrencyScale : 2; }
        }

        /// <summary>
        /// Определяет режим редактирования
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
            }
            get { return !ParentPage.DocEditable; }
        }

        #endregion

        /// <summary>
        ///  Конструктор по умолчанию
        /// </summary>
        public FactUslForm()
        {
            HelpUrl = "hlp/factusl/help.htm";
        }
        
        /// <summary>
        /// Событие загрузки страницы
        /// </summary>
        /// <param name="sender">Объект страницы</param>
        /// <param name="e">Аргументы</param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!V4IsPostBack)
            {
                BindField();

                V4SetFocus("efResource");
            }

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

            efCount.Precision = factUsl.ResourceId > 0 ? factUsl.Resource.GetScale4Unit(factUsl.UnitId.ToString(), 3, Document.GOPersonDataField.Id) : 3;

            efCostOutNDS.Precision =
            efSummaOutNDS.Precision =
            efSummaNDS.Precision =
            efVsego.Precision = Scale;

            DocumentReadOnly = !ParentPage.DocEditable;

        }

        #region InitControls
        /// <summary>
        /// Инициализация контролов
        /// </summary>
        void SetInitValue()
        {

        }
        #endregion

        #region Binder

        /// <summary>
        /// 
        /// </summary>
        protected override void EntityFieldInit()
        {
            if (!V4IsPostBack)
            {
                id = Request.QueryString["id"];
                idDoc = Request.QueryString["idDoc"];
                idParentPage = Request.QueryString["idpp"];

                ParentPage = Application[idParentPage] as Nakladnaya;
                if (ParentPage == null)
                {
                    ShowMessage(Resx.GetString("errRetrievingPageObject"), Resx.GetString("errPrinting"), MessageStatus.Error);
                    return;
                }

                if (!String.IsNullOrEmpty(id) && id != "0")
                {
                    factUsl = new FactUsl(id);
                    //factUsl = GetObjectById(typeof(FactUsl), id) as FactUsl;
                    if (factUsl == null || factUsl.Id == "0")
                        throw new LogicalException(Resx.GetString("TTN_ ERRMoveStockInitialized"), "", System.Reflection.Assembly.GetExecutingAssembly().GetName(), Priority.Info);
                }
                else
                {
                    factUsl = new FactUsl { DocumentId = int.Parse(idDoc) };
                    RenderAgent1();
                    RenderAgent2();
                    efChanged.ChangedByID = null;
                }

                SetInitValue();
            }

            Entity = factUsl;

            efResource.BindStringValue = factUsl.ResourceIdBind;
            efResourceRus.BindStringValue = factUsl.ResourceRusBind;
            efAgent1.BindStringValue = factUsl.Agent1Bind;
            efAgent2.BindStringValue = factUsl.Agent2Bind;
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
        /// Заполнение контролов данными объекта
        /// </summary>
        void BindField()
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

        private void ResourceRus_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (factUsl.ResourceId == 0) return;
            if (!efResourceRus.Value.Equals(factUsl.Resource.Name))
            {
                ntf.Add(Resx.GetString("TTN_ntfRussianNameNotMatchProduct"), NtfStatus.Error);
            }
        }

        private void ResourceLat_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (factUsl.ResourceId == 0 || factUsl.Resource.Unavailable) return;
            if (!efResourceLat.Value.Equals(factUsl.Resource.ResourceLat))
            {
                ntf.Add(Resx.GetString("TTN_ntfLatinNameNotMatchProduct"), NtfStatus.Error);
            }
        }

        #endregion

        #region BeforeSearch
        /// <summary>
        /// Событие, устанавливающее параметры фильтрации перед поиском услуг в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        void Res_BeforeSearch(object sender)
        {
            efResource.Filter.AllChildrenWithParentIDs.Clear();
            efResource.Filter.AllChildrenWithParentIDs.Value = "3";
        }

        /// <summary>
        /// Событие, устанавливающее параметры фильтрации перед поиском дополнительных единиц измерения в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        void UnitAdv_BeforeSearch(object sender)
        {
            if (factUsl.ResourceId == 0) return;
            efUnitAdv.Filter.Resource = factUsl.ResourceId;
        }

        /// <summary>
        /// Событие, устанавливающее параметры фильтрации перед поиском ставок в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        void StavkaNDS_BeforeSearch(object sender)
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
            if (factUsl.ResourceId == 0)
            {
                efResourceRus.Value =
                efResourceLat.Value = "";
                return;
            }

            if (e.OldValue.Equals(e.NewValue)) return;

            efResourceRus.Value = factUsl.Resource.Name;
            efResourceLat.Value = factUsl.Resource.ResourceLat;

            ClearUnits();

            efUnitAdv.TryFindSingleValue();
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола количества
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Count_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (efCount.Value.Trim().Length == 0) return;
            factUsl.Recalc(e.OldValue, "3", "Count", "0", Scale);
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
                {
                    DialogCostRecalc("CostOutNDS", e.OldValue, "2");
                }
                else
                {
                    factUsl.Recalc(e.OldValue, "2", "CostOutNDS", "0", Scale);
                }
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
                {
                    factUsl.Recalc(e.OldValue, "2", "StavkaNDS", "0", Scale);
                }
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
                efUnit.Value = "";
                return;
            }

            SetUnitInfo(efUnitAdv.Value);

            efUnitAdv.Value = "";
            SetAdvUnitInfo(e.OldValue);
            //SetUnitAdvValidator();

            efUnit.Value = factUsl.UnitId.ToString();
            efUnit.ValueText = factUsl.Unit.ЕдиницаРус;
            //Unit.RefreshFieldBind();
        }

        #endregion

        #region PreRender
        
        /// <summary>
        /// PreRender для количества
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Count_PreRender(object sender, EventArgs e)
        {
            /*
            if (!Editable)
            {
                Count.IsReadOnly = true;
            }
            else
            {
                if (factUsl.Agent1.ToString() == "1")
                    Count.IsReadOnly = true;
                else
                    Count.IsReadOnly = false;
            }
            */

            var maxscale = factUsl.Resource.GetScale4Unit(efUnit.Value, 0, Document.GOPersonDataField.Id);
            efCount.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)factUsl.Count, maxscale);
        }

        /// <summary>
        /// PreRender для цены без НДС
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CostOutNDS_PreRender(object sender, EventArgs e)
        {
            /*
            if (!Editable) efCostOutNDS.IsReadOnly = true;
            else
            {
                if (factUsl.Agent1.ToString() == "1")
                    efCostOutNDS.IsReadOnly = true;
                else
                    efCostOutNDS.IsReadOnly = false;
            }
            */
            efCostOutNDS.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)factUsl.CostOutNDS, Scale);
        }

        /// <summary>
        /// PreRender для суммы без НДС
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SummaOutNDS_PreRender(object sender, EventArgs e)
        {
            /*
            if (!Editable) efSummaOutNDS.IsReadOnly = true;
            else
            {
                if (factUsl.Agent1.ToString() == "1")
                    efSummaOutNDS.IsReadOnly = true;
                else
                    efSummaOutNDS.IsReadOnly = false;
            }
            */

            efSummaOutNDS.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)factUsl.SummaOutNDS, Scale);

        }

        /// <summary>
        /// PreRender для суммы НДС
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SummaNDS_PreRender(object sender, EventArgs e)
        {
            /*
            if (!Editable) efSummaNDS.IsReadOnly = true;
            else
            {
                if (factUsl.Agent1.ToString() == "1")
                    efSummaNDS.IsReadOnly = true;
                else
                    efSummaNDS.IsReadOnly = false;
            }
            */
            efSummaNDS.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)factUsl.SummaNDS, Scale);
        }

        /// <summary>
        /// PreRender для поля "всего"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Vsego_PreRender(object sender, EventArgs e)
        {
            /*
            if (!Editable) efVsego.IsReadOnly = true;
            else
            {
                if (factUsl.Agent1.ToString() == "1")
                    efVsego.IsReadOnly = true;
                else
                    efVsego.IsReadOnly = false;
            }
            */

            efVsego.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)factUsl.Vsego, Scale);
        }

        /// <summary>
        /// PreRender для ставки НДС
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
        /// PreRender для выбора ед. изм.
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
        /// Рендер Агент Поставщик
        /// </summary>
        protected void RenderAgent1()
        {
            if (factUsl.DocumentId == 0) { efAgent1.Value = Resx.GetString("lblPerson") + "1 - " + Resx.GetString("TTN_msgAgent"); return; }
            if (!factUsl.Document.IsNew && factUsl.Document.Unavailable) { efAgent1.Value = Resx.GetString("lblPerson") + "1 - " + Resx.GetString("TTN_msgAgent"); return; }

            if (!CheckPersonBProject(Document.DocumentData.PersonId1.ToString()))
            {
                chAgent1.Visible = false;
                return;
            }

            switch (factUsl.Document.Type)
            {
                case DocTypeEnum.ТоварноТранспортнаяНакладная:
                    if (Document.PlatelschikDataField.Value.ToString().Length == 0 || Document.PlatelschikField.Unavailable)
                        efAgent1.Value = Document.PlatelschikField.Name + " - " + Resx.GetString("TTN_msgAgent");
                    else
                        efAgent1.Value = Document.PlatelschikDataField.Value + " - " + Resx.GetString("TTN_msgAgent");
                    break;
                case DocTypeEnum.АктВыполненныхРаботУслуг:
                    //Agent1.Value = Document.PostavschikDataField.Value.ToString();
                    break;
            }
        }

        /// <summary>
        /// Рендер Агент Плательщик
        /// </summary>
        protected void RenderAgent2()
        {
            if (factUsl.DocumentId == 0) { efAgent2.Value = Resx.GetString("lblPerson") + "2 - " + Resx.GetString("TTN_msgAgent"); return; }
            if (!factUsl.Document.IsNew && factUsl.Document.Unavailable) { efAgent2.Value = Resx.GetString("lblPerson") + "2 - " + Resx.GetString("TTN_msgAgent"); return; }

            if (!CheckPersonBProject(Document.DocumentData.PersonId2.ToString()))
            {
                chAgent2.Visible = false;
                return;
            }

            switch (factUsl.Document.Type)
            {
                case DocTypeEnum.ТоварноТранспортнаяНакладная:
                    if (Document.PlatelschikDataField.Value.ToString().Length == 0 || Document.PlatelschikField.Unavailable)
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
        /// Рендер валюты
        /// </summary>
        protected void RenderCurrency()
        {
            string _currency = "";

            switch (factUsl.Document.Type)
            {
                case DocTypeEnum.ТоварноТранспортнаяНакладная:
                    efCurrency.Value = (Document.Currency.Id.Length > 0) ? Document.Currency.Name : "у.е.";
                    break;
                case DocTypeEnum.АктВыполненныхРаботУслуг:
                    //Currency.Value = (akt._Currency.Length > 0 && UE == 0) ? akt.Currency._Name : "у.е.";
                    break;
            }
        }

        /// <summary>
        /// Рендер поля Эквивалент
        /// </summary>
        protected void RenderOsnUnit()
        {
            if (factUsl.Coef == 0) return;
            if (factUsl.ResourceId == 0 || factUsl.Resource == null || factUsl.Resource.Unavailable) return;
            if (factUsl.Resource.UnitCode == 0) return;

            efOsnUnit.Value = factUsl.Resource.Unit.ЕдиницаРус;
        }

        /// <summary>
        /// Рендер поля Эквивалент
        /// </summary>
        protected void RenderAdvUnit()
        {
            if (factUsl.UnitId == 0) return;
            var un = factUsl.Unit;
            if (un == null || un.Unavailable) return;
            efAdvUnit.Value = "1 " + un.ЕдиницаРус + " = ";
            efMCoef.Value = factUsl.Coef.ToString(CultureInfo.InvariantCulture);
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

        #endregion

        #region MenuButtons

        /// <summary>
        ///     Инициализация/создание кнопок меню
        /// </summary>
        private void SetMenuButtons()
        {
            var btnAdd = new Button
            {
                ID = "btnSave",
                V4Page = this,
                Text = Resx.GetString("cmdSave"),
                Title = Resx.GetString("cmdSave"),
                Width = 105,
                IconJQueryUI = ButtonIconsEnum.Save,
                OnClick = "cmd('cmd', 'SaveData');"
            };

            var btnRefresh = new Button
            {
                ID = "btnRefresh",
                V4Page = this,
                Text = Resx.GetString("cmdRefresh"),
                Title = Resx.GetString("cmdRefreshTitle"),
                IconJQueryUI = ButtonIconsEnum.Refresh,
                Width = 105,
                OnClick = "cmd('cmd', 'RefreshData');"
            };

            var btnClear = new Button
            {
                ID = "btnDelete",
                V4Page = this,
                Text = Resx.GetString("cmdDelete"),
                Title = Resx.GetString("cmdDeleteTitle"),
                IconJQueryUI = ButtonIconsEnum.Delete,
                Width = 105,
                OnClick = string.Format("if(confirm('{0} {1}?')) cmd('cmd', 'DeleteData');", Resx.GetString("msgDeleteConfirm"), factUsl.ResourceRus)
            };

            var btnClose = new Button
            {
                ID = "btnClose",
                V4Page = this.ParentPage,
                Text = Resx.GetString("cmdClose"),
                Title = Resx.GetString("cmdCloseTitleApp"),
                IconJQueryUI = ButtonIconsEnum.Close,
                Width = 105,
                OnClick = "parent.resources_Records_Close(idp);"
            };

            var buttons = ParentPage.DocEditable ? new[] { btnAdd, btnRefresh, btnClear, btnClose } : new[] { btnClose };
            AddMenuButton(buttons);
        }

        /// <summary>
        /// Обновление данных формы из объекта
        /// </summary>
        private void RefreshData()
        {
            BindField();
        }

        /// <summary>
        /// Очистка всех данных формы
        /// </summary>
        private void DeleteData()
        {
            factUsl.Delete(false);
            JS.Write("parent.resources_Records_Save();");
        }

        /// <summary>
        ///     Кнопка: Сохранить
        /// </summary>
        private void SaveData()
        {
            if (Validation())
            {
                factUsl.Save(false);
                JS.Write("parent.resources_Records_Save();");
            }
        }

        #endregion

        #region Validation
        /// <summary>
        /// Валидация контролов
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

            if (factUsl.UnitId == 0)
            {
                // Не заполнено поле 'Единица измерения'
                ShowMessage(Resx.GetString("TTN_ntfNotUnit"), title);
                efUnitAdv.Focus();
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

            if (factUsl.StavkaNDSId == 0)
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

            if (factUsl.SummaNDS == 0)
            {
                // Не заполнено поле 'Сумма НДС'
                ShowMessage(Resx.GetString("TTN_ntfNotSummaNDS"), title);
                efSummaNDS.Focus();
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

        bool CheckPersonBProject(string _p)
        {
            if (_p.Length == 0) return false;

            //Person p = new Person(_p);
            var p = GetObjectById(typeof(Person), _p) as Person;
            if (p==null||p.Unavailable) return false;
            if (!p.HasBProject) return false;

            return true;
        }

        #region Unit

        /// <summary>
        /// Установка единицы измерения по ресурсу при выборе доп. единицы измерения
        /// </summary>
        /// <param name="_unit">ед.изм.</param>
        void SetUnitInfo(string _unit)
        {
            if (factUsl.ResourceId == 0 || factUsl.Resource == null || factUsl.Resource.Unavailable) return;

            if (_unit.Equals("10000001")) SetUnitInfoByOsnUnit(factUsl.Resource);
            else SetUnitInfoByAdvUnit(factUsl.Resource, _unit);
            RenderOsnUnit();
            RenderAdvUnit();
        }

        /// <summary>
        /// Установка единицы измерения по ресурсу
        /// </summary>
        /// <param name="res">Ресурс</param>
        void SetUnitInfoByOsnUnit(Resource res)
        {
            ClearUnits();
            factUsl.UnitId = res.UnitCode; efUnit.Value = factUsl.UnitId.ToString();
            factUsl.Coef = 1; efMCoef.Value = factUsl.Coef.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Заполнения объекта данными ед.изм. и коэффициента 
        /// </summary>
        /// <param name="res">Ресурс</param>
        /// <param name="_unit">Ед.изм.</param>
        void SetUnitInfoByAdvUnit(Resource res, string _unit)
        {
            ClearUnits();

            //UnitAdv rxu = new UnitAdv(_unit);
            var rxu = GetObjectById(typeof(UnitAdv), _unit) as UnitAdv;
            if (rxu == null || rxu.Unavailable) return;

            factUsl.UnitId = rxu.Unit.КодЕдиницыИзмерения; efUnit.Value = factUsl.UnitId.ToString();
            factUsl.Coef = rxu.Коэффициент; efMCoef.Value = factUsl.Coef.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Пересчет полей в соответствии с доп. ед.изм.
        /// </summary>
        void SetAdvUnitInfo(string _oldVal)
        {
            int oldVal = 0;
            Int32.TryParse(_oldVal, out oldVal);
            if (oldVal == 0) return;

            //Unit old = new Unit(_oldVal.ToString(CultureInfo.InvariantCulture));
            var old = GetObjectById(typeof(Unit), _oldVal.ToString(CultureInfo.InvariantCulture)) as Unit;
            if (old != null && factUsl.Count > 0 && factUsl.ResourceId > 0)
            {
                var maxscale = factUsl.Resource.GetScale4Unit(efUnit.Value, 0, Document.GOPersonDataField.Id);
                factUsl.Count = Math.Round(factUsl.Count / factUsl.Resource.ConvertionCoefficient(old, factUsl.Unit), maxscale);
                efCount.Value = Kesco.Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)factUsl.Count, maxscale);
                factUsl.CostOutNDS = Math.Round(factUsl.CostOutNDS * (decimal)factUsl.Resource.ConvertionCoefficient(old, factUsl.Unit), Scale);
                efCostOutNDS.Value = factUsl.CostOutNDS.ToString(CultureInfo.InvariantCulture);
                factUsl.Recalc(factUsl.CostOutNDS.ToString(), "2", "CostOutNDS", "0", Scale);
            }
        }

        /// <summary>
        /// Очистка контролов ед.изм.
        /// </summary>
        void ClearUnits()
        {
            efUnitAdv.Value = "";
            factUsl.UnitId = 0;
            factUsl.Coef = 0;

            RenderOsnUnit();
            RenderAdvUnit();
        }


        #endregion
        
        /// <summary>
        /// Отображает диалоговое окно выбора вида расчета
        /// </summary>
        /// <param name="name">Название изменяемого поля</param>
        /// <param name="value">Новое значение изменяемого поля</param>
        /// <param name="ndx">Индекс</param>
        void DialogCostRecalc(string name, string value, string ndx)
        {
            ShowConfirm(Resx.GetString("TTN_msgNDSSUM"), Resx.GetString("TTN_msgChoiceCalculationType"), Resx.GetString("QSBtnYes"), Resx.GetString("QSBtnNo")
                , "dialogRecalc('DialogCostRecalc_Yes','" + name + "','" + value + "','" + ndx + "');"
                , "dialogRecalc('DialogCostRecalc_No','" + name + "','" + value + "','" + ndx + "');"
                , null, null);
        }

        /// <summary>
        /// Отображает диалоговое окно выбора типа перерасчета
        /// </summary>
        /// <param name="name">Название изменяемого поля</param>
        /// <param name="value">Новое значение изменяемого поля</param>
        /// <param name="ndx">Индекс</param>
        void DialogRecalc(string name, string value, string ndx)
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

            ShowRecalc(helpText, Resx.GetString("TTN_msgOKInfo"), "ОК", Resx.GetString("QSBtnRecalc"), Resx.GetString("QSBtnCancel"), Resx.GetString("TTN_msgChange")
                , "dialogRecalc('DialogRecalc_Yes','" + name + "','" + value + "','" + ndx + "');"
                , "dialogRecalc('DialogRecalc_Recalc','" + name + "','" + value + "','" + ndx + "');"
                , "dialogRecalc('DialogRecalc_No','" + name + "','" + value + "','" + ndx + "');"
                , "dialogRecalc('DialogRecalc_Change','" + name + "','" + value + "','" + ndx + "');"
                , null, 500);
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
                case "RefreshData":
                    RefreshData();
                    break;
                case "SaveData":
                    SaveData();
                    break;
                case "DeleteData":
                    DeleteData();
                    break;
                case "CloseWindow":
                    JS.Write("parent.resources_Records_Close();");
                    break;
                case "DialogCostRecalc_Yes":
                    double d_kol = (factUsl.Count > 0 && !factUsl.Count.Equals("0")) ? factUsl.Count : 1;

                    decimal _costOutNDS = factUsl.CostOutNDS;
                    decimal _summaOutNDS = 0;
                    decimal _summaNDS = 0;
                    decimal _vsego = 0;
                    var stavka = factUsl.StavkaNDS;
                    decimal prst = (decimal)stavka.Величина * 100;
                    int scale = Document != null && !Document.Unavailable ? Document.CurrencyScale : 2;

                    _vsego = Lib.ConvertExtention.Convert.Round((decimal)(d_kol * (double)_costOutNDS), scale);
                    _summaNDS = Lib.ConvertExtention.Convert.Round(_vsego / (100 + prst) * prst, scale);
                    _summaOutNDS = _vsego - _summaNDS;
                    _costOutNDS = Lib.ConvertExtention.Convert.Round((decimal)((double)_summaOutNDS / d_kol), scale * 2);

                    var maxscale = factUsl.Resource.GetScale4Unit(efUnit.Value, 0, Document.GOPersonDataField.Id);
				    factUsl.Count=d_kol; efCount.Value = Kesco.Lib.ConvertExtention.Convert.Decimal2Str((decimal)factUsl.Count, maxscale);

                    factUsl.CostOutNDS = _costOutNDS; efCostOutNDS.Value = Lib.ConvertExtention.Convert.Decimal2Str(factUsl.CostOutNDS, scale * 2);
                    factUsl.SummaOutNDS = Lib.ConvertExtention.Convert.Round(_summaOutNDS, scale); efSummaOutNDS.Value = Kesco.Lib.ConvertExtention.Convert.Decimal2Str(factUsl.SummaOutNDS, scale);
                    factUsl.SummaNDS = Lib.ConvertExtention.Convert.Round(_summaNDS, scale); efSummaNDS.Value = Kesco.Lib.ConvertExtention.Convert.Decimal2Str(factUsl.SummaNDS, scale);
                    factUsl.Vsego = Lib.ConvertExtention.Convert.Round(_vsego, scale); efVsego.Value = Kesco.Lib.ConvertExtention.Convert.Decimal2Str(factUsl.Vsego, scale);

                    break;
                case "DialogCostRecalc_No":
                    factUsl.Recalc(param["value"], param["ndx"], param["name"], "0", Scale);
                    break;
                case "DialogRecalc_Yes":
                    factUsl.Recalc(param["value"], param["ndx"], param["name"], "0", Scale);
                    break;
                case "DialogRecalc_Recalc":
                    factUsl.Recalc(param["value"], param["ndx"], param["name"], "1", Scale);
                    break;
                case "DialogRecalc_No":
                    factUsl.Recalc(param["value"], param["ndx"], param["name"], "2", Scale);
                    break;
                case "DialogRecalc_Change":
                    factUsl.Recalc(param["value"], param["ndx"], param["name"], "3", Scale);
                    break;

            }
        }

        /// <summary>
        ///     Задание ссылки на справку
        /// </summary>
        protected override string HelpUrl { get; set; }
    }
}