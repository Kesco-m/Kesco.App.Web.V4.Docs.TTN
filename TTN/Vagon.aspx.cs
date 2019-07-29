using System;
using System.Collections.Generic;
using System.IO;
using System.Collections.Specialized;
using Kesco.Lib.BaseExtention.Enums.Controls;
using Kesco.Lib.DALC;
using Kesco.Lib.Entities;
using Kesco.Lib.Web.Controls.V4;
using Kesco.Lib.Web.Controls.V4.Common;
using Kesco.Lib.Entities.Resources;
using Kesco.Lib.Log;

namespace Kesco.App.Web.Docs.TTN
{
    public partial class Vagon : EntityPage
    {
        #region declaration, property, field
        protected string id;
        protected string idDoc;
        protected string idParentPage;
        protected string resultGuid;

        /// <summary>
        /// Вызывающая страница (накладная)
        /// </summary>
        protected Nakladnaya ParentPage;

        /// <summary>
        ///     ТТН
        /// </summary>
        private Lib.Entities.Documents.EF.Trade.TTN Document
        {
            get { return (ParentPage).Document; }
        }

        #endregion

        protected override void EntityFieldInit()
        {
            if (!V4IsPostBack)
            {
                id = Request.QueryString["id"];
                idDoc = Request.QueryString["idDoc"];
                idParentPage = Request.QueryString["idpp"];
                resultGuid = Request.QueryString["idResult"];

                ParentPage = Application[idParentPage] as Nakladnaya;
                if (ParentPage == null)
                {
                    ShowMessage(Resx.GetString("errRetrievingPageObject"), Resx.GetString("errPrinting"), MessageStatus.Error);
                    return;
                }

                if (Request.QueryString["sf"] != null) efShipperStore.Value = Request.QueryString["sf"].Length == 0 ? "0" : Request.QueryString["sf"];
                if (Request.QueryString["st"] != null) efPayerStore.Value = Request.QueryString["st"].Length == 0 ? "0" : Request.QueryString["st"];
            }

            base.EntityFieldInit();
        }

        protected override void EntityInitialization(Entity copy = null)
        {
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            efPayerStore.Filter.IsWarehouseType.Enabled = true;

            efNDS.BeforeSearch += StavkaNDS_BeforeSearch;
        }

        public override string HelpUrl { get; set; }

        #region Render
        /// <summary>
        ///     Подготовка данных для отрисовки заголовка страницы(панели с кнопками)
        /// </summary>
        /// <returns></returns>
        protected string RenderHeader()
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

        /// <summary>
        /// Событие, устанавливающее параметры фильтрации перед поиском ставок в фильтре
        /// </summary>
        /// <param name="sender">Контрол</param>
        void StavkaNDS_BeforeSearch(object sender)
        {
            efNDS.Filter.TerritoryCode = 188;
        }

        #region MenuButtons

        /// <summary>
        ///     Инициализация/создание кнопок меню
        /// </summary>
        private void SetMenuButtons()
        {
            var btnAccept = new Button
            {
                ID = "btnAccept",
                V4Page = this,
                Text = Resx.GetString("cmdApply"),
                Title = Resx.GetString("cmdApply"),
                Width = 105,
                IconJQueryUI = ButtonIconsEnum.Ok,
                OnClick = "cmd('cmd', 'Save');"
            };

            var btnClose = new Button
            {
                ID = "btnClose",
                V4Page = this.ParentPage,
                Text = Resx.GetString("cmdClose"),
                Title = Resx.GetString("cmdCloseTitleApp"),
                IconJQueryUI = ButtonIconsEnum.Close,
                Width = 105,
                OnClick = "parent.vagon_Records_Close(idp);"
            };

            Button[] buttons = { btnAccept, btnClose };

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
                case "Save":
                    SaveData();
                    break;
            }
        }

        /// <summary>
        ///     Кнопка: Сохранить
        /// </summary>
        private void SaveData()
        {
            #region Проверка идентичности ГО/ГП, транспортных узлов в выбранных отправках

            var info = string.Empty;
            if (ParentPage.CheckIdentity(resultGuid, out info))
            {
                var stavkaNDS = new StavkaNDS(efNDS.Value);

                var reloadParentForm = false;
                if (idDoc == "0")
                {
                    List<DBCommand> cmds = null;
                    Document.Save(false, cmds);
                    idDoc = Document.DocId.ToString();
                    reloadParentForm = true;
                }


                ParentPage.Document.FillPositionsByGuid(new Guid(resultGuid), int.Parse(efNDS.Value), Lib.ConvertExtention.Convert.Str2Decimal(efCost.Value), (decimal)stavkaNDS.Величина, efShipperStore.Value, efPayerStore.Value);

                JS.Write("parent.vagon_Records_Save('{0}');", reloadParentForm);
            }
            else
            {
                if (info != string.Empty) ShowMessage(info, Resx.GetString("errPrinting"), MessageStatus.Error);
            }
            #endregion
        }

        #endregion
    }
}