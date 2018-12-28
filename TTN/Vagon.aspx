<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Vagon.aspx.cs" Inherits="Kesco.App.Web.Docs.TTN.Vagon" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="v4control" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Kesco.Nakladnaya.css" />
</head>
<script type="text/javascript">
    if (parent != null) parent.frameVagon_progressBarShow(0);
</script>
<body>
<div class="marginD"><%=RenderHeader()%></div>    
    <div class="v4formContainer">
        <div class="marginL">
                
            <div id="Div2" class="predicate_block">
                <div class="label"><%=Resx.GetString("lblShipperStore")%>:</div>
                <v4dbselect:DBSStore ID="efShipperStore" runat="server" Width="300px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="efStorePayer"
                                     AutoSetSingleValue="True" CSSClass="aligned_control"/>
            </div>
        
            <div id="Div3" class="predicate_block">
                <div class="label"><%=Resx.GetString("lblPayerStore")%>:</div>
                <v4dbselect:DBSStore ID="efPayerStore" runat="server" Width="300px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="efCost"
                                      AutoSetSingleValue="True" CSSClass="aligned_control"/>
            </div>    
        
            <hr/>
        
            <div id="SumPanel" class="predicate_block">
                <div class="inline_predicate_block">
                    <div><%=Resx.GetString("lblCost")%>:</div>
                    <v4control:Number ID="efCost" runat="server" Width="100px" NextControl="efNDS" IsRequired="True"/>
                </div>
                <div class="inline_predicate_block_left">
                    <div><%=Resx.GetString("lblNDS")%>:</div>
                    <v4dbselect:DBSStavkaNDS ID="efNDS" Width="100px" CSSClass="aligned_control" runat="server" IsRequired="True" NextControl="efStoreShipper"/>
                </div>
            </div>
        
        </div>
    </div>
</body>
</html>
