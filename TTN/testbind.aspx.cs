using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Kesco.Lib.Entities.Documents.EF.Trade;
using Kesco.Lib.Entities.Resources;
using Kesco.Lib.Web.Controls.V4.Common;
using System.Reflection;
using Kesco.Lib.BaseExtention.BindModels;

namespace TTN
{
    public partial class testbind : EntityPage
    {
        protected Mris mris;
        //13447
        protected override string HelpUrl { get; set; }

        protected override void OnInit(EventArgs e)
        {
            mris = new Mris("13447");
            efResource.Changed += efResource_Changed;

            efResource.BindStringValue = mris.ResourceIdBind;
            mris.ResourceIdBind.ValueChangedEvent_Invoke(mris.ResourceIdBind.Value, "");

        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!V4IsPostBack)
            {
                mris.ResourceId = 100;
            }
        }

        void efResource_Changed(object sender, Kesco.Lib.Web.Controls.V4.ProperyChangedEventArgs e)
        {
            ShowMessage(mris.ResourceId.ToString());
        }

        protected override void ProcessCommand(string cmd, NameValueCollection param)
        {
            switch (cmd)
            {
                case "ChangeMris":
                    mris.ResourceId = 100;
                    break;
            }
        }

        public testbind()
        {
            HelpUrl = "hlp/mris/help.htm";
        }
    }
}