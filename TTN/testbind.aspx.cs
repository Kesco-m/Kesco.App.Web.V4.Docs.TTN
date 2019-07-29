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
using Kesco.Lib.Entities;
using Kesco.Lib.Web.Controls.V4;

namespace TTN
{
    public partial class testbind : EntityPage
    {
        protected Mris mris;
        public override string HelpUrl { get; set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            Kesco.Lib.Entities.Documents.EF.Trade.TTN ttn = new Kesco.Lib.Entities.Documents.EF.Trade.TTN();
            
            
        }

        protected override void EntityInitialization(Entity copy = null)
        {
            throw new NotImplementedException();
        }
    }
}