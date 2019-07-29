<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FactUslForm.aspx.cs" Inherits="Kesco.App.Web.Docs.TTN.FactUslForm" %>
<%@ Register TagPrefix="v4control" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Kesco.Nakladnaya.css" />
</head>
<script src="Kesco.FactUsl.js?v=1" type="text/javascript"></script>
<body>
<div class="marginD"><%=RenderDocumentHeader()%></div>    
    <div class="v4formContainer">
        <div class="marginL">
            
        <div id="OrderPanel" class="predicate_block" runat="server">
            <div class="label"><%=Resx.GetString("TTN_lblOrder")%>:</div>
            <v4control:DropDownList ID="efOrder" runat="server" Width="350px" CSSClass="aligned_control" NextControl="btnSave" IsReadOnly="True"/>
        </div>

        <div id="ResourcePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblService")%>:</div>
            <v4dbselect:DBSResource ID="efResource" runat="server" Width="350px" CLID="25" IsAlwaysAdvancedSearch="True" IsRequired="True" NextControl="efUnitAdv"
            IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Resource_Changed"/>
        </div>

        <div id="ResourceRusPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("TTN_lblServiceNameRus")%>:</div>
            <v4control:TextBox ID="efResourceRus" runat="server" Width="370px" IsRequired="True" CSSClass="aligned_control" NextControl="efResourceLat"/>
        </div>

        <div id="ResourceLatPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("TTN_lblServiceNameLat")%>:</div>
            <v4control:TextBox ID="efResourceLat" runat="server" Width="370px" CSSClass="aligned_control" NextControl="chAgent1"/>
        </div>

        <div id="Agent1Panel" class="predicate_block" runat="server">
            <v4control:CheckBox ID="chAgent1" runat="server" Width="350px" NextControl="chAgent2"/>
            <div class="inline_predicate_block">
                <v4control:Div id="efAgent1" runat="server"/>
            </div>
        </div>

        <div id="Agent2Panel" class="predicate_block" runat="server">
            <v4control:CheckBox ID="chAgent2" runat="server" Width="350px" NextControl="efUnitAdv"/>
            <div class="inline_predicate_block">
                <v4control:Div id="efAgent2" runat="server"/>
            </div>
        </div>

        <div id="Div2" class="predicate_block">
            <div class="inline_predicate_block" id="trUnitAdv">
                <div class="label"><%=Resx.GetString("TTN_lblUnitsSelect")%>:</div>
                <v4dbselect:DBSUnitAdv ID="efUnitAdv" runat="server" Width="80px" CSSClass="aligned_control" NextControl="efCount" OnChanged="UnitAdv_Changed"/>
            </div>
            <div class="inline_predicate_block">
                <table cellpadding="0" cellspacing="0" border="0">
                    <tr>
                        <td>
                            <div class="<%=DivHeaderClass()%>"><%=Resx.GetString("lblUnitShort")%>:&nbsp;</div>
                        </td>
                        <td colspan="4" style="PADDING-LEFT: 5px; white-space: nowrap">
                            <v4dbselect:DBSUnit ID="efUnit" runat="server" IsReadOnly="True"/>
                        </td>
                    </tr>
                    <tr id="trEquivalent">
                        <td></td>
                        <td style="PADDING-LEFT: 5px; white-space: nowrap"><div><%=Resx.GetString("lblEquivalent")%>:&nbsp;</div></td>
                        <td style="white-space: nowrap"><v4control:Div ID="efAdvUnit" runat="server" IsReadOnly="True"/></td>
                        <td style="white-space: nowrap"><v4control:Div ID="efMCoef" runat="server" IsReadOnly="True"/></td>
                        <td style="white-space: nowrap"><v4control:Div ID="efOsnUnit" runat="server" IsReadOnly="True"/></td>
                    </tr>
                </table>
            </div>
        </div>

        <div id="CountPanel" class="predicate_block">
        <div class="label_100"><%=Resx.GetString("lblQuantity")%>:</div>
            <v4control:Number ID="efCount" runat="server" Width="100px" NextControl="efCostOutNDS" IsRequired="True" OnChanged="Count_Changed"/>
            <v4control:Div id="efErKol" runat="server"/>
            <v4control:Div id="efGross" runat="server"/>
        </div>
        
        <div id="CostPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label_100"><%=Resx.GetString("lblCostOutNDS")%>:</div>
                <v4control:Number ID="efCostOutNDS" runat="server" Width="100px" NextControl="efStavkaNDS" IsRequired="True" OnChanged="CostOutNDS_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label_100"><%=Resx.GetString("lblNDSRate")%>:</div>
                <v4dbselect:DBSStavkaNDS ID="efStavkaNDS" Width="80px" CSSClass="aligned_control" runat="server" IsRequired="True" NextControl="efSummaOutNDS" OnChanged="StavkaNDS_Changed"/>
            </div>
        </div>
      
        <div id="SumPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label_100"><%=Resx.GetString("lblSummaOutNDS")%>:</div>
                <v4control:Number ID="efSummaOutNDS" runat="server" Width="100px" NextControl="efSummaNDS" IsRequired="True" OnChanged="SummaOutNDS_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label_100"><%=Resx.GetString("lblNDS")%>:</div>
                <v4control:Number ID="efSummaNDS" runat="server" Width="100px" NextControl="efVsego" IsRequired="True" OnChanged="SummaNDS_Changed"/>
            </div>
        </div>

        <div id="ItogPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label_100"><%=Resx.GetString("lTotal")%>:</div>
                <v4control:Number ID="efVsego" runat="server" Width="100px" NextControl="btnSave" IsRequired="True" OnChanged="Vsego_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label_100"><%=Resx.GetString("ppFltВалюта")%>:</div>
                <div ID="efCurrency" runat="server" class="inline_predicate_block_text"></div>
            </div>
        </div>
        
        <!--Изменил Изменено-->
        <div class="footer">
            <v4control:Changed ID="efChanged" runat="server"/>
        </div>

        
    </div>    
    </div>
</body>
</html>
