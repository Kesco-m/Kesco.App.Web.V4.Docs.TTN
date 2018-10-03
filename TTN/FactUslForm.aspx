<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FactUslForm.aspx.cs" Inherits="Kesco.App.Web.Docs.TTN.FactUslForm" %>
<%@ Register TagPrefix="v4control" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Nakladnaya.css" />
</head>
<script src="Kesco.FactUsl.js" type="text/javascript"></script>
<body>
<div class="marginL"><%=RenderDocumentHeader()%></div>    
    <div class="v4formContainer">
        <div class="marginL" style="height: 440px">
            
        <div id="ResourcePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblService")%>:</div>
            <v4dbselect:DBSResource ID="efResource" runat="server" Width="500px" CLID="25" IsAlwaysAdvancedSearch="True" IsRequired="True" NextControl="chAgent1"
            IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Resource_Changed"/>
        </div>

        <div id="ResourceRusPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("TTN_lblServiceNameRus")%>:</div>
            <v4control:TextBox ID="efResourceRus" runat="server" Width="500px" IsRequired="True" CSSClass="aligned_control" NextControl="efResourceLat"/>
        </div>

        <div id="ResourceLatPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("TTN_lblServiceNameLat")%>:</div>
            <v4control:TextBox ID="efResourceLat" runat="server" Width="500px" CSSClass="aligned_control" NextControl="chAgent1"/>
        </div>

        <div id="Agent1Panel" class="predicate_block">
            <v4control:CheckBox ID="chAgent1" runat="server" Width="500px" NextControl="chAgent2"/>
            <div class="inline_predicate_block">
                <v4control:Div id="efAgent1" runat="server"/>
            </div>
        </div>

        <div id="Agent2Panel" class="predicate_block">
            <v4control:CheckBox ID="chAgent2" runat="server" Width="500px" NextControl="efUnitAdv"/>
            <div class="inline_predicate_block">
                <v4control:Div id="efAgent2" runat="server"/>
            </div>
        </div>

        <div id="Div2" class="predicate_block">
        <table cellpadding="0" cellspacing="0" border="0">
            <tr>
                <td nowrap styles='PADDING-LEFT:0px'>
                    <div class="label"><%=Resx.GetString("TTN_lblUnitsSelect")%>:</div>
                    <v4dbselect:DBSUnitAdv ID="efUnitAdv" runat="server" Width="150px" CSSClass="aligned_control" NextControl="efCount" OnChanged="UnitAdv_Changed"/>
                </td>
                <td>
                    <div><%=Resx.GetString("lblUnitShort")%>:</div>
                </td>
                <td colspan="3">
                    <v4dbselect:DBSUnit ID="efUnit" runat="server" Width="150px" IsReadOnly="True"/>
                </td>
            </tr>
            <tr>
                <td></td>
                <td nowrap styles='PADDING-LEFT:0px'><div><%=Resx.GetString("lblEquivalent")%>:</div></td>
                <td nowrap><v4control:Div ID="efAdvUnit" runat="server" IsReadOnly="True" Width="100px"/></td>
                <td nowrap><v4control:Div ID="efMCoef" runat="server" IsReadOnly="True" Width="100px"/></td>
                <td nowrap><v4control:Div ID="efOsnUnit" runat="server" IsReadOnly="True" Width="100px"/></td>
            </tr>
        </table>
        </div>

        <div id="CountPanel" class="predicate_block">
        <div class="label"><%=Resx.GetString("lblQuantity")%>:</div>
            <v4control:Number ID="efCount" runat="server" Width="100px" NextControl="efCostOutNDS" IsRequired="True" OnChanged="Count_Changed"/>
            <v4control:Div id="efErKol" runat="server"/>
            <v4control:Div id="efGross" runat="server"/>
        </div>
        
        <div id="CostPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label"><%=Resx.GetString("lblCostOutNDS")%>:</div>
                <v4control:Number ID="efCostOutNDS" runat="server" Width="100px" NextControl="efStavkaNDS" IsRequired="True" OnChanged="CostOutNDS_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label"><%=Resx.GetString("lblNDSRate")%>:</div>
                <v4dbselect:DBSStavkaNDS ID="efStavkaNDS" Width="100px" CSSClass="aligned_control" runat="server" IsRequired="True" NextControl="efSummaOutNDS" OnChanged="StavkaNDS_Changed"/>
            </div>
        </div>
      
        <div id="SumPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label"><%=Resx.GetString("lblSummaOutNDS")%>:</div>
                <v4control:Number ID="efSummaOutNDS" runat="server" Width="100px" NextControl="efSummaNDS" IsRequired="True" OnChanged="SummaOutNDS_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label"><%=Resx.GetString("lblNDS")%>:</div>
                <v4control:Number ID="efSummaNDS" runat="server" Width="100px" NextControl="efVsego" IsRequired="True" OnChanged="SummaNDS_Changed"/>
            </div>
        </div>

        <div id="ItogPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label"><%=Resx.GetString("lTotal")%>:</div>
                <v4control:Number ID="efVsego" runat="server" Width="100px" NextControl="btnSave" IsRequired="True" OnChanged="Vsego_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label"><%=Resx.GetString("ppFltВалюта")%>:</div>
                <v4control:Div ID="efCurrency" runat="server" Width="100px"/>
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
