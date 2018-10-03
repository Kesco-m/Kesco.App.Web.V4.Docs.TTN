using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.BaseExtention.Enums.Docs;
using Kesco.Lib.Entities.Documents;
using Kesco.Lib.Entities.Documents.EF;
using Kesco.Lib.Entities.Documents.EF.Dogovora;
using Kesco.Lib.Entities.Persons;
using Kesco.Lib.Entities.Persons.PersonOld;
using Kesco.Lib.Entities.Stores;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Entities.Resources;
using Kesco.Lib.Entities.Documents.EF.Trade;
using Kesco.Lib.Log;

namespace Kesco.App.Web.Docs.TTN
{
    /// <summary>
    ///     Класс формы Движения на складах
    /// </summary>
    public partial class MrisForm : EntityPage
    {
        #region declaration, property, field

        protected Mris mris;
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
        /// Точность вывода валют
        /// </summary>
        private int Scale
        {
            get { return Document.CurrencyScale != null ? Document.CurrencyScale : 2; }
        }

        /// <summary>
        /// ТТН
        /// </summary>
        private Lib.Entities.Documents.EF.Trade.TTN Document
        {
            get { return ParentPage.Document; }
        }

        private bool DocumentReadOnly
        {
            set
            {
                efShipperStore.IsReadOnly = value;
                efPayerStore.IsReadOnly = value;
                efResource.IsReadOnly = value;
                efResourceRus.IsReadOnly = value;
                efResourceLat.IsReadOnly = value;
                efUnitAdv.IsReadOnly = value;
                efCountry.IsReadOnly = value;
                efGTD.IsReadOnly = value;
                efCount.IsReadOnly = value;
                efCostOutNDS.IsReadOnly = value;
                efStavkaNDS.IsReadOnly = value;
                efSummaOutNDS.IsReadOnly = value;
                efSummaNDS.IsReadOnly = value;
                efAktsiz.IsReadOnly = value;
                efVsego.IsReadOnly = value;
            }
        }

        #endregion

        /// <summary>
        ///  Конструктор по умолчанию
        /// </summary>
        public MrisForm()
        {
            HelpUrl = "hlp/mris/help.htm";
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

                GetOstatok();
                V4SetFocus("efShipperStore");
            }

            efGTD.Filter.Type.Add(new DocTypeParam() { DocTypeEnum = DocTypeEnum.ТаможеннаяДекларация, QueryType = DocTypeQueryType.Equals });
            efCountry.Filter.TerritoryCode.Add("2");

            efResource.BeforeSearch += Resource_BeforeSearch;
            efUnitAdv.BeforeSearch += UnitAdv_BeforeSearch;
            efStavkaNDS.BeforeSearch += StavkaNDS_BeforeSearch;

            efShipperStore.OnRenderNtf += ShipperStore_OnRenderNtf;
            efPayerStore.OnRenderNtf += PayerStore_OnRenderNtf;
            efResource.OnRenderNtf += Resource_OnRenderNtf;
            efResourceRus.OnRenderNtf += ResourceRus_OnRenderNtf;
            efResourceLat.OnRenderNtf += ResourceLat_OnRenderNtf;
            efGTD.OnRenderNtf += Contract_OnRenderNtf;

            efCount.PreRender += Count_PreRender;
            efCurrency.PreRender += Currency_PreRender;
            efBDOst.PreRender += new EventHandler(BDOst_PreRender);
            efEDOst.PreRender += new EventHandler(EDOst_PreRender);

            efCostOutNDS.PreRender += new EventHandler(CostOutNDS_PreRender);
            efSummaOutNDS.PreRender += new EventHandler(SummaOutNDS_PreRender);
            efSummaNDS.PreRender += new EventHandler(SummaNDS_PreRender);
            efVsego.PreRender += new EventHandler(Vsego_PreRender);
            efAktsiz.PreRender += new EventHandler(Aktsiz_PreRender);

            DocumentReadOnly = !ParentPage.DocEditable;

        }

        #region InitControls
        /// <summary>
        /// Инициализация контролов
        /// </summary>
        void SetInitValue()
        {
            efRestType.Items.Add("", Resx.GetString("TTN_optTransactionBalance"));
            efRestType.Items.Add("1", Resx.GetString("TTN_optBalanceCompletedDocuments"));
            efRestType.Items.Add("0", Resx.GetString("TTN_optBalanceSignedDocuments"));

            efDateDocB.Value = DateDocE.Value = Document.Date.ToShortDateString();

            efCount.Precision = mris.ResourceId > 0 ? mris.Resource.GetScale4Unit(mris.UnitId.ToString(), 3, Document.GOPersonDataField.Id) : 3;

            efCostOutNDS.Precision =
            efSummaOutNDS.Precision =
            efSummaNDS.Precision =
            efAktsiz.Precision =
            efVsego.Precision = Scale;
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
                    mris = new Mris(id);
                    //mris = GetObjectById(typeof(Mris), id) as Mris;
                    if (mris == null || mris.Id == "0")
                        throw new LogicalException(Resx.GetString("TTN_ ERRMoveStockInitialized"), "", System.Reflection.Assembly.GetExecutingAssembly().GetName(), Priority.Info);
                }
                else
                {
                    mris = new Mris { DocumentId = int.Parse(idDoc) };
                    efChanged.ChangedByID = null;
                }

                SetInitValue();
            }

            Entity = mris;

            efShipperStore.BindStringValue = mris.ShipperStoreIdBind;
            efPayerStore.BindStringValue = mris.PayerStoreIdBind;
            efResource.BindStringValue = mris.ResourceIdBind;
            efResourceRus.BindStringValue = mris.ResourceRusBind;
            efResourceLat.BindStringValue = mris.ResourceLatBind;
            efUnit.BindStringValue = mris.UnitIdBind;
            efCountry.BindStringValue = mris.CountryIdBind;
            efGTD.BindStringValue = mris.GTDIdBind;
            efCount.BindStringValue = mris.CountBind;
            efMCoef.BindStringValue = mris.CoefBind;
            efCostOutNDS.BindStringValue = mris.CostOutNDSBind;
            efStavkaNDS.BindStringValue = mris.StavkaNDSIdBind;
            efSummaOutNDS.BindStringValue = mris.SummaOutNDSBind;
            efSummaNDS.BindStringValue = mris.SummaNDSBind;
            efAktsiz.BindStringValue = mris.AktsizBind;
            efVsego.BindStringValue = mris.VsegoBind;

            base.EntityFieldInit();
        }

        /// <summary>
        /// 
        /// </summary>
        void BindField()
        {
            RenderOsnUnit();
            RenderAdvUnit();

            efChanged.SetChangeDateTime = mris.ChangedTime;
            efChanged.ChangedByID = mris.ChangedId;
        }

        #endregion

        #region OnRenderNtf
        private void ShipperStore_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            var documentDate = Document.Date == DateTime.MinValue ? DateTime.Today : Document.Date;

            if (!Document.Type.Equals(DocTypeEnum.ТоварноТранспортнаяНакладная)) return;

            if (efShipperStore.Value.Length == 0)
            {
                if (Document.PostavschikDataField.Value.ToString().Length > 0)
                {
                    var p = new PersonOld(Document.PostavschikDataField.Id);
                    //var p = GetObjectById(typeof(PersonOld), Document.PostavschikDataField.Id) as PersonOld;
                    if (p == null || (!p.Unavailable && p.BusinessProjectID > 0))
                        efShipperStore.NtfNotValidData();
                }

                return;
            }

            var s = new Store(efShipperStore.Value);
            //var s = GetObjectById(typeof(Store), ShipperStore.Value) as Store;
            if (s == null || s.Unavailable)
            {
                efShipperStore.ValueText = "#" + efShipperStore.Value;
                ntf.Add(Resx.GetString("TTN_ntfWarehouseNotAvailable"), NtfStatus.Error);
                return;
            }
            
            if (s.KeeperId > 0)
            {
                var p = new PersonOld(s.KeeperId.ToString());
                //var p = GetObjectById(typeof(PersonOld), s.KeeperId.ToString()) as PersonOld;

                if (p == null)
                {
                    ntf.Add(Resx.GetString("TTN_ntfKeeperNotFound"), NtfStatus.Error);
                    return;
                }

                if (!p.IsChecked)
                {
                    ntf.Add(Resx.GetString("TTN_ntfKeeperNotVerified"), NtfStatus.Error);
                }

                var crd = p.GetCard(documentDate);
                if (crd == null)
                {
                    ntf.Add(string.Format(Resx.GetString("TTN_ntfKeeperNoDataOn") + " " + documentDate.ToString("dd.MM.yyyy")), NtfStatus.Error);
                    efShipperStore.ValueText = (s.IBAN.Length > 0 ? s.IBAN : s.Name) + " в " + p.Name;
                }
                else
                    efShipperStore.ValueText = (s.IBAN.Length > 0 ? s.IBAN : s.Name) + " в " + (crd.NameRus.Length > 0 ? crd.NameRus : crd.NameLat);
            }

            if (s != null && s.Id.Length > 0 && !s.Unavailable && !s.Manager.Id.Equals(Document.PostavschikDataField.Value.ToString()))
            {
                ntf.Add(Resx.GetString("TTN_ntfWarehouseNotMatchSupplier"), NtfStatus.Error);
            }
        }

        private void PayerStore_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            var documentDate = Document.Date == DateTime.MinValue ? DateTime.Today : Document.Date;

            if (!Document.Type.Equals(DocTypeEnum.ТоварноТранспортнаяНакладная)) return;

            if (efPayerStore.Value.Length == 0)
            {
                if (Document.PlatelschikDataField.Value.ToString().Length > 0)
                {
                    //var p = new PersonOld(Document.PlatelschikDataField.Id);
                    var p = GetObjectById(typeof(PersonOld), Document.PlatelschikDataField.Id) as PersonOld;
                    if (p==null || (!p.Unavailable && p.BusinessProjectID > 0))
                        efPayerStore.NtfNotValidData();
                }

                return;
            }

            //var s = new Store(efPayerStore.Value);
            var s = GetObjectById(typeof(Store), efPayerStore.Value) as Store;
            if (s==null || s.Unavailable)
            {
                efPayerStore.ValueText = "#" + efPayerStore.Value;
                ntf.Add(Resx.GetString("TTN_ntfWarehouseNotAvailable"), NtfStatus.Error);
                return;
            }

            if (s.KeeperId > 0)
            {
                //var p = new PersonOld(s.KeeperId.ToString());
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

                var crd = p.GetCard(documentDate);
                if (crd == null)
                {
                    ntf.Add(string.Format(Resx.GetString("TTN_ntfKeeperNoDataOn") + " " + documentDate.ToString("dd.MM.yyyy")), NtfStatus.Error);
                    efPayerStore.ValueText = (s.IBAN.Length > 0 ? s.IBAN : s.Name) + " " + Resx.GetString("msgIN") + " " + p.Name;
                }
                else
                    efPayerStore.ValueText = (s.IBAN.Length > 0 ? s.IBAN : s.Name) + " " + Resx.GetString("msgIN") + " " + (crd.NameRus.Length > 0 ? crd.NameRus : crd.NameLat);
            }

            if (s != null && s.Id.Length > 0 && !s.Unavailable && !s.Manager.Id.Equals(Document.PlatelschikDataField.Value.ToString()))
            {
                ntf.Add(Resx.GetString("TTN_ntfWarehouseNotMatchPayer"), NtfStatus.Error);
            }

        }

        private void Resource_OnRenderNtf(object sender, Ntf ntf)
        {
            if (efResource.Value.Length == 0)
            {
                return;
            }

            if (Document.DogovorField.Value.ToString().Length == 0 || Document.DocumentData.Unavailable || Document.Type == DocTypeEnum.ТипНеУказан )
                return;

            int inx = 1;
            
            List<DogovorPosition> positions = null;
            if (Document.Prilozhenie != null && Document.Prilozhenie.ToString().Length > 0 && !Document.Prilozhenie.Unavailable)
            {
                positions = DocumentPosition<DogovorPosition>.LoadByDocId(int.Parse(Document.Prilozhenie.Id));
                inx = 2;
            }
            else if (Document.Dogovor != null && Document.Dogovor.Id.Length > 0 && !Document.Dogovor.Unavailable)
                positions = DocumentPosition<DogovorPosition>.LoadByDocId(int.Parse(Document.Dogovor.Id));
            else
                return;
            
            var fl = false;
            foreach (var p in positions.Where(p => p.Resource.Equals(efResource)))
            {
                if (efCostOutNDS.Equals(p.Cost)) fl = true;
                break;
            }

            if (!fl && positions.Count > 0)
                ntf.Add((inx == 2) ? Resx.GetString("TTN_ntfNotMatchApplication") : Resx.GetString("TTN_ntfNotComplyContract"), NtfStatus.Error);
             
        }

        private void ResourceRus_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (mris.ResourceId == 0) return;
            if (!efResourceRus.Value.Equals(mris.Resource.Name))
            {
                ntf.Add(Resx.GetString("TTN_ntfRussianNameNotMatchProduct"), NtfStatus.Error);
            }
        }

        private void ResourceLat_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (mris.ResourceId == 0 || mris.Resource.Unavailable) return;
            if (!efResourceLat.Value.Equals(mris.Resource.ResourceLat))
            {
                //ResourceLat.RenderNtf(new StringWriter(new StringBuilder("латинское название не соответствует продукту")));
                ntf.Add(Resx.GetString("TTN_ntfLatinNameNotMatchProduct"), NtfStatus.Error);
            }
        }

        private void Contract_OnRenderNtf(object sender, Ntf ntf)
        {
            ntf.Clear();
            if (mris.CountryId != RegionRussia && mris.CountryId > 0 && mris.GTDId == 0)
                efGTD.NtfNotValidData();

            if (efGTD.Value.Length == 0)
            {
                return;
            }

            var d = new Document(mris.GTDId.ToString());
            //var d = GetObjectById(typeof(Document), Contract.Value) as Document;
            if (d == null || d.Unavailable)
            {
                efGTD.ValueText = "#" + efGTD.Value;
                ntf.Add(Resx.GetString("TTN_ntfDocumentNotAvailable"), NtfStatus.Error);
                return;
            }
        }

        #endregion

        #region BeforeSearch

        /// <summary>
        /// Событие, устанавливающее параметры фильтрации перед поиском продуктов в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        void Resource_BeforeSearch(object sender)
        {
            efResource.Filter.AllChildrenWithParentIDs.Clear();
            efResource.Filter.AllChildrenWithParentIDs.Value = "2";
        }

        /// <summary>
        /// Событие, устанавливающее параметры фильтрации перед поиском дополнительных единиц измерения в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        void UnitAdv_BeforeSearch(object sender)
        {
            if (mris.ResourceId == 0) return;
            efUnitAdv.Filter.Resource = mris.ResourceId;
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
        ///     Событие, отслеживающее изменение контрола Склада поставщика
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void ShipperStore_Changed(object sender, ProperyChangedEventArgs e)
        {
            GetOstatok();
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола Склада плательщика
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void PayerStore_Changed(object sender, ProperyChangedEventArgs e)
        {
            GetOstatok();
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола Продукт
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Resource_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (mris.ResourceId == 0)
            {
                efResourceRus.Value =
                efResourceLat.Value = "";
                efResourceRus.RenderNtf();
                efResourceLat.RenderNtf();
                return;
            }

            if (e.OldValue.Equals(e.NewValue)) return;

            efResourceRus.Value = mris.Resource.Name;
            efResourceLat.Value = mris.Resource.ResourceLat;

            efResourceRus.RenderNtf();
            
            ClearUnits();
            efUnitAdv.TryFindSingleValue();

            FillSpecNDSByParentResource(mris.Resource);
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола типа остатка
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void RestType_Changed(object sender, ProperyChangedEventArgs e)
        {
            GetOstatok();
            RefreshErCol();
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола страна происхождения
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Country_Changed(object sender, ProperyChangedEventArgs e)
        {
            efGTD.IsRequired = false;
            if (efCountry.Value.Length > 0 && !efCountry.Value.Equals(RegionRussia.ToString()))
            {
                efGTD.IsRequired = true;
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
            mris.Recalc(e.OldValue, "3", "Count", "0", Scale);
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
                if (mris.StavkaNDSId > 0
                    && e.NewValue.Length > 0
                    && mris.StavkaNDSId > 2
                    && mris.CostOutNDS != 0)
                {
                    DialogCostRecalc("CostOutNDS", e.OldValue, "2");
                }
                else
                {
                    mris.Recalc(e.OldValue, "2", "CostOutNDS", "0", Scale);
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
            if (e.NewValue.Length >= 0)
            {
                if (mris.CostOutNDS > 0
                    && !e.NewValue.Equals(e.OldValue)
                    && e.NewValue.Length > 0
                    && mris.StavkaNDSId > 2
                    && mris.CostOutNDS != 0)
                    DialogCostRecalc("StavkaNDS", e.OldValue, "2");
                else
                {
                    mris.Recalc(e.OldValue, "2", "StavkaNDS", "0", Scale);
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
            if (mris.StavkaNDSId != 0)
                DialogRecalc("SummaOutNDS", e.OldValue, "4");
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола суммы НДС
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void SummaNDS_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (mris.StavkaNDSId != 0)
                DialogRecalc("SummaNDS", e.OldValue, "5");
        }

        /// <summary>
        ///     Событие, отслеживающее изменение контрола всего
        /// </summary>
        /// <param name="sender">Контрол</param>
        /// <param name="e">Аргументы</param>
        protected void Vsego_Changed(object sender, ProperyChangedEventArgs e)
        {
            if (mris.StavkaNDSId != 0)
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

            efUnit.Value = mris.UnitId.ToString();
            efUnit.ValueText = mris.Unit.Описание;
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
            var maxscale = mris.Resource.GetScale4Unit(efUnit.Value, 0, Document.GOPersonDataField.Id);
            efCount.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)mris.Count, maxscale);

            RefreshErCol();
        }

        /// <summary>
        /// PreRender для цены без НДС
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CostOutNDS_PreRender(object sender, EventArgs e)
        {
            efCostOutNDS.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)mris.CostOutNDS, Scale);
        }

        private void SummaOutNDS_PreRender(object sender, EventArgs e)
        {
            efSummaOutNDS.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)mris.SummaOutNDS, Scale);
        }

        private void SummaNDS_PreRender(object sender, EventArgs e)
        {
            efSummaNDS.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)mris.SummaNDS, Scale);
        }

        private void Vsego_PreRender(object sender, EventArgs e)
        {
            efVsego.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)mris.Vsego, Scale);
        }

        private void Aktsiz_PreRender(object sender, EventArgs e)
        {
            efAktsiz.Value = Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)mris.Aktsiz, Scale);
        }

        /// <summary>
        /// Рендер поля валюты
        /// </summary>
        protected void Currency_PreRender(object sender, EventArgs e)
        {
            if (Document.Type.Equals(DocTypeEnum.ТоварноТранспортнаяНакладная))
                efCurrency.InnerText = Document.Currency.Name;
        }

        private void BDOst_PreRender(object sender, EventArgs e)
        {
            efBDOst.Precision = GetScale();
        }

        private void EDOst_PreRender(object sender, EventArgs e)
        {
            efEDOst.Precision = GetScale();
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
                OnClick = "cmd('cmd', 'SaveMris');"
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
                OnClick = string.Format("v4_showConfirm('{0}','{1}','{2}','{3}','{4}', null);", 
                    string.Format("{0} {1}", Resx.GetString("msgDeleteConfirm"), mris.ResourceRus),
                    Resx.GetString("errDoisserWarrning"),
                    Resx.GetString("CONFIRM_StdCaptionYes"),
                    Resx.GetString("CONFIRM_StdCaptionNo"),
                    string.Format("cmd({0});", HttpUtility.JavaScriptStringEncode("'cmd', 'DeleteData'"))
                )
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

            var buttons = ParentPage.DocEditable ? new[] {btnAdd, btnRefresh, btnClear, btnClose} : new[] { btnClose };
            AddMenuButton(buttons);
        }

        /// <summary>
        ///     Кнопка: Сохранить
        /// </summary>
        private void SaveData()
        {
            if (Validation())
            {
                // 
                mris.DateMove = DateTime.Today;
                mris.TransactionType = 12; // реализация

                mris.Save(false);
                JS.Write("parent.resources_Records_Save();");
            }
        }

        /// <summary>
        /// Обновление данных формы из объекта
        /// </summary>
        private void RefreshData()
        {
            ClearObjects();
            BindField();
            RefreshNtf();
        }

        /// <summary>
        /// Очистка всех данных формы
        /// </summary>
        private void DeleteData()
        {
            mris.Delete(false);
            JS.Write("parent.resources_Records_Save();");
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

            if (mris.ShipperStoreId == 0)
            {
                // Не заполнено поле 'склад поставщика'
                ShowMessage(Resx.GetString("TTN_ntfNotShipperStore"), title);
                efShipperStore.Focus();
                return false;
            }

            if (mris.PayerStoreId == 0)
            {
                // Не заполнено поле 'склад плательщика' 
                ShowMessage(Resx.GetString("TTN_ntfNotPayerStore"), title);
                efPayerStore.Focus();
                return false;
            }

            if (mris.ResourceId == 0)
            {
                // Не заполнено поле 'продукт'
                ShowMessage(Resx.GetString("TTN_ntfNotProduct"), title);
                efResource.Focus();
                return false;
            }

            if (mris.ResourceRus.Length == 0)
            {
                // Не заполнено поле 'русское название продукта'
                ShowMessage(Resx.GetString("TTN_ntfNotResourceRus"), title);
                efResourceRus.Focus();
                return false;
            }

            if (mris.Count == 0)
            {
                // Не заполнено поле 'Количество'
                ShowMessage(Resx.GetString("TTN_ntfNotCount"), title);
                efCount.Focus();
                return false;
            }

            if (mris.UnitId == 0)
            {
                // Не заполнено поле 'Единица измерения'
                ShowMessage(Resx.GetString("TTN_ntfNotUnit"), title);
                efUnitAdv.Focus();
                return false;
            }

            if (mris.CostOutNDS.ToString(CultureInfo.InvariantCulture).Length == 0)
            {
                // Не заполнено поле 'Цена без НДС'
                ShowMessage(Resx.GetString("TTN_ntfNotCostOutNDS"), title);
                efCostOutNDS.Focus();
                return false;
            }
            
            if (mris.CostOutNDS == 0)
            {
                // Цена без НДС должна быть больше или меньше 0
                ShowMessage(Resx.GetString("TTN_ntfCostOutNDSIncorrect"), title);
                efCostOutNDS.Focus();
                return false;
            }

            if (mris.StavkaNDSId == 0)
            {
                // Не заполнено поле 'Cтавка НДС'
                ShowMessage(Resx.GetString("TTN_ntfNotStavkaNDS"), title);
                efStavkaNDS.Focus();
                return false;
            }

            if (mris.SummaOutNDS.ToString(CultureInfo.InvariantCulture).Length == 0)
            {
                // Не заполнено поле 'Сумма без НДС'
                ShowMessage(Resx.GetString("TTN_ntfNotSummaOutNDS"), title);
                efSummaOutNDS.Focus();
                return false;
            }
            
            if (mris.SummaOutNDS == 0)
            {
                // Сумма без НДС должна быть больше или меньше 0
                ShowMessage(Resx.GetString("TTN_ntfSummaOutNDSIncorrect"), title);
                efSummaOutNDS.Focus();
                return false;
            }

            if (mris.SummaNDS == 0)
            {
                // Не заполнено поле 'Сумма НДС'
                ShowMessage(Resx.GetString("TTN_ntfNotSummaNDS"), title);
                efSummaNDS.Focus();
                return false;
            }

            if (mris.Vsego.ToString(CultureInfo.InvariantCulture).Length == 0)
            {
                // Не заполнено поле 'Всего'
                ShowMessage(Resx.GetString("TTN_ntfNotVsego"), title);
                efVsego.Focus();
                return false;
            }
            
            if (mris.Vsego == 0)
            {
                // Значение поля 'Всего' должно быть больше или меньше 0
                ShowMessage(Resx.GetString("TTN_ntfVsegoIncorrect"), title);
                efVsego.Focus();
                return false;
            }

            if (mris.CountryId > 0 && !mris.CountryId.Equals(RegionRussia) && mris.GTDId == 0)
            {
                // Необходимо указать таможенную декларацию
                ShowMessage(Resx.GetString("TTN_ntfNotGTD"), title);
                efGTD.Focus();
                return false;
            }

            return true;
        }
        #endregion

        #region Unit

        /// <summary>
        /// Установка единицы измерения по ресурсу при выборе доп. единицы измерения
        /// </summary>
        /// <param name="_unit">ед.изм.</param>
        void SetUnitInfo(string _unit)
        {
            if (mris.ResourceId == 0 || mris.Resource == null || mris.Resource.Unavailable) return;

            if (_unit.Equals("10000001")) SetUnitInfoByOsnUnit(mris.Resource);
            else SetUnitInfoByAdvUnit(mris.Resource, _unit);
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
            mris.UnitId = res.UnitCode; efUnit.Value = mris.UnitId.ToString();
            mris.Coef = 1; efMCoef.Value = mris.Coef.ToString(CultureInfo.InvariantCulture);
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

            mris.UnitId = rxu.Unit.КодЕдиницыИзмерения; efUnit.Value = mris.UnitId.ToString();
            mris.Coef = rxu.Коэффициент; efMCoef.Value = mris.Coef.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Очистка контролов ед.изм.
        /// </summary>
        void ClearUnits()
        {
            efUnitAdv.Value = "";
            mris.UnitId = 0;
            mris.Coef = 0;

            RenderOsnUnit();
            RenderAdvUnit();
        }

        /// <summary>
        /// Рендер поля Эквивалент
        /// </summary>
        protected void RenderOsnUnit()
        {
            if (mris.Coef == 0) return;
            if (mris.ResourceId == 0 || mris.Resource == null || mris.Resource.Unavailable) return;
            if (mris.Resource.UnitCode == 0) return;

            efOsnUnit.Value = "&nbsp;" + mris.Resource.Unit.ЕдиницаРус;
        }

        /// <summary>
        /// Рендер поля Эквивалент
        /// </summary>
        protected void RenderAdvUnit()
        {
            if (mris.UnitId == 0) return;
            var un = mris.Unit;
            if (un == null || un.Unavailable) return;
            efAdvUnit.Value = "1&nbsp;" + un.ЕдиницаРус + "&nbsp;=&nbsp;";
            //efMCoef.Value = mris.Coef.ToString(CultureInfo.InvariantCulture);
            //efMCoef.Value = Lib.ConvertExtention.Convert.Double2Str(mris.Coef, Scale);
            
        }

        /// <summary>
        /// Пересчет полей в соответствии с доп. ед.изм.
        /// </summary>
        void SetAdvUnitInfo(string _oldVal)
        {
            int oldVal = 0;
            Int32.TryParse(_oldVal, out oldVal);
            if (oldVal == 0) return;

            //var old = new Unit(_oldVal.ToString(CultureInfo.InvariantCulture));
            var old = GetObjectById(typeof(Unit), _oldVal.ToString(CultureInfo.InvariantCulture)) as Unit;
            if (old != null && mris.Count > 0 && mris.ResourceId > 0)
            {
                var maxscale = mris.Resource.GetScale4Unit(efUnit.Value, 0, Document.GOPersonDataField.Id);
                mris.Count = Math.Round(mris.Count / mris.Resource.ConvertionCoefficient(old, mris.Unit), maxscale);
                efCount.Value = Kesco.Lib.ConvertExtention.Convert.Decimal2StrInit((decimal)mris.Count, maxscale);
                mris.CostOutNDS = Math.Round(mris.CostOutNDS * (decimal)mris.Resource.ConvertionCoefficient(old, mris.Unit), Scale);
                efCostOutNDS.Value = mris.CostOutNDS.ToString(CultureInfo.InvariantCulture);
                mris.Recalc(mris.CostOutNDS.ToString(), "2", "CostOutNDS", "0", Scale);
            }

            GetOstatok();
        }

        /// <summary>
        /// Расчет остатков на начало и конец дня
        /// </summary>
        void GetOstatok()
        {

            if (mris.ResourceId == 0 || mris.ShipperStoreId == 0 || mris.UnitId == 0)
            {
                ClearOstatok();
                return;
            }

            string Unit_SymbolRus = (mris.UnitId > 0 ? mris.Unit.ЕдиницаРус : "");
            string _unit = mris.UnitId.ToString();

            Unit_SymbolRus = " " + Unit_SymbolRus;

            string ost1 = Store.GetOstatokResource(mris.ShipperStoreId.ToString(),
                mris.ResourceId.ToString(CultureInfo.InvariantCulture), _unit, Document.Date, efResourceChild.Value.Equals("1"), efRestType.Value);
            string ost2 = Store.GetOstatokResource(mris.ShipperStoreId.ToString(),
                mris.ResourceId.ToString(CultureInfo.InvariantCulture), _unit, Document.Date.AddDays(1), efResourceChild.Value.Equals("1"), efRestType.Value);

            efBDOst.Value = ost1;
            efEDOst.Value = ost2;

            efBDUnit.Value = efEDUnit.Value = Unit_SymbolRus;

        }

        /// <summary>
        /// Очистка остатков на начало и конец дня
        /// </summary>
        void ClearOstatok()
        {
            efBDOst.Value = "";
            efEDOst.Value = "";
            efBDUnit.Value = "";
            efEDUnit.Value = "";
        }

        int GetScale()
        {
            int scale = 3;

            if (mris.ResourceId > 0 && mris.UnitId > 0)
                scale = mris.Resource.GetScale4Unit(mris.UnitId.ToString(), scale, Document.GOPersonDataField.Id);

            //if (sl._Upakovka.Length > 0)
            //    scale = sl.Resource.GetScale4Unit(sl.Upakovka._Unit, scale);

            return scale;
        }

        /// <summary>
        /// Контроль превышения остатка на складе
        /// </summary>
        void RefreshErCol()
        {
            if (Document.Date == DateTime.MinValue) return;
            if (mris.ShipperStoreId == 0) return;
            if (mris.ResourceId == 0) return;
            if (mris.UnitId == 0) return;
            if (mris.Count == 0) return;

            //string ostatok = Transactions.Transaction.GetOstatokResource(mris.ShipperStore, mris.Resource, mris.Unit, Kesco.Lib.ConvertExtention.Convert.DateTime2Str(Document.Date.AddDays(1)), ResourceChild.Value.Equals("1"), RestType.Value);

            //if ((decimal)mris.Count > Kesco.Lib.ConvertExtention.Convert.Str2Decimal(ostatok))
            //    divErKol.Value = "количество реализуемого продукта превышает остаток на складе";
        }

        #endregion

        /// <summary>
        /// Заполнение ставки НДС из накладной
        /// </summary>
        /// <param name="res">Ресурс</param>
        private void FillSpecNDSByParentResource(Resource res)
        {

            if (res != null && Document.PostavschikDataField.Value.ToString().Length > 0)
            {
                var shipper = new Person(ParentPage.ShipperField.Value);
                //var shipper = GetObjectById(typeof(Person), ParentPage.ShipperField.Value) as Person;
                if (shipper != null && shipper.RegionID.Equals(RegionRussia))
                {
                    mris.StavkaNDSId = res.NDS.Equals("1") ? 4 : 3;
                    efStavkaNDS.Value = mris.StavkaNDSId.ToString();
                }
            }
        }

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
                case "SaveMris":
                    SaveData();
                    break;
                case "DeleteData":
                    DeleteData();
                    break;
                case "DialogCostRecalc_Yes":
                    double d_kol = (mris.Count > 0 && !mris.Count.Equals("0")) ? mris.Count : 1;

				    decimal _costOutNDS = mris.CostOutNDS;
				    decimal _summaOutNDS = 0;
				    decimal _summaNDS = 0;
				    decimal _vsego = 0;
                    var stavka = mris.StavkaNDS;
                    decimal prst = (decimal)stavka.Величина * 100;
                    int scale = Document != null && !Document.Unavailable ? Document.CurrencyScale : 2;

				    _vsego = Lib.ConvertExtention.Convert.Round((decimal)(d_kol*(double)_costOutNDS), scale);
				    _summaNDS = Lib.ConvertExtention.Convert.Round(_vsego/(100+prst)*prst, scale);
				    _summaOutNDS = _vsego - _summaNDS;
				    _costOutNDS = Lib.ConvertExtention.Convert.Round((decimal)((double)_summaOutNDS/d_kol), scale * 2);

                    var maxscale = mris.Resource.GetScale4Unit(efUnit.Value, 0, Document.GOPersonDataField.Id);
				    mris.Count=d_kol; efCount.Value = Lib.ConvertExtention.Convert.Decimal2Str((decimal)mris.Count, maxscale);

                    mris.CostOutNDS = _costOutNDS; efCostOutNDS.Value = Lib.ConvertExtention.Convert.Decimal2Str(mris.CostOutNDS, scale * 2);
                    mris.SummaOutNDS = Lib.ConvertExtention.Convert.Round(_summaOutNDS, scale); efSummaOutNDS.Value = Lib.ConvertExtention.Convert.Decimal2Str(mris.SummaOutNDS, scale);
                    mris.SummaNDS = Lib.ConvertExtention.Convert.Round(_summaNDS, scale); efSummaNDS.Value = Lib.ConvertExtention.Convert.Decimal2Str(mris.SummaNDS, scale);
                    mris.Vsego = Lib.ConvertExtention.Convert.Round(_vsego, scale); efVsego.Value = Lib.ConvertExtention.Convert.Decimal2Str(mris.Vsego, scale);
                    break;
                case "DialogCostRecalc_No":
                    mris.Recalc(param["value"], param["ndx"], param["name"], "0", Scale);
                    break;
                case "DialogRecalc_Yes":
                    mris.Recalc(param["value"], param["ndx"], param["name"], "0", Scale);
                    break;
                case "DialogRecalc_Recalc":
                    mris.Recalc(param["value"], param["ndx"], param["name"], "1", Scale);
                    break;
                case "DialogRecalc_No":
                    mris.Recalc(param["value"], param["ndx"], param["name"], "2", Scale);
                    break;
                case "DialogRecalc_Change":
                    mris.Recalc(param["value"], param["ndx"], param["name"], "3", Scale);
                    break;
            }
        }
        
        /// <summary>
        ///     Задание ссылки на справку
        /// </summary>
        protected override string HelpUrl { get; set; }

    }
}