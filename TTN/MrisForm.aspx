<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MrisForm.aspx.cs" Inherits="Kesco.App.Web.Docs.TTN.MrisForm" %>
<%@ Register TagPrefix="v4control" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Nakladnaya.css" />
</head>
<script src="Kesco.Mris.js" type="text/javascript"></script>
<body>
<div class="marginL"><%=RenderDocumentHeader()%></div>    
    <div class="v4formContainer">
        <div class="marginL">
            
        <div id="ShipperStorePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblShipperStore")%>:</div>
            <v4dbselect:DBSStore ID="efShipperStore" runat="server" Width="500px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="efPayerStore"
                                 AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="ShipperStore_Changed"/>
        </div>
        
        <div id="PayerStorePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblPayerStore")%>:</div>
            <v4dbselect:DBSStore ID="efPayerStore" runat="server" Width="500px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="efRes"
                                  AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="PayerStore_Changed"/>
        </div>
        
        <div id="ResourcePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblProduct")%>:</div>
            <v4dbselect:DBSResource ID="efResource" runat="server" Width="500px" CLID="25" IsAlwaysAdvancedSearch="True" NextControl="efUnitAdv"
             AutoSetSingleValue="True" IsRequired="True" CSSClass="aligned_control" OnChanged="Resource_Changed"/>
        </div>

        <div id="ResourceRusPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("TTN_lblProductNameRus")%>:</div>
            <v4control:TextBox ID="efResourceRus" runat="server" Width="500px" IsRequired="True" CSSClass="aligned_control" NextControl="efResourceLat"/>
        </div>
        
        <div id="ResourceLatPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("TTN_lblProductNameLat")%>:</div>
            <v4control:TextBox ID="efResourceLat" runat="server" Width="500px" CSSClass="aligned_control" NextControl="efUnitAdv"/>
        </div>
        
        <div id="Div2" class="predicate_block">
        <table cellpadding="0" cellspacing="0" border="0">
            <tr>
                <td nowrap styles='PADDING-LEFT:0px'>
                    <div class="label"><%=Resx.GetString("TTN_lblUnitsSelect")%>:</div>
                    <v4dbselect:DBSUnitAdv ID="efUnitAdv" runat="server" Width="150px" CSSClass="aligned_control" NextControl="efCountry" OnChanged="UnitAdv_Changed"/>
                </td>
                <td>
                    <div><%=Resx.GetString("lblUnitShort")%>:</div>
                </td>
                <td colspan="3">
                    <v4dbselect:DBSUnit ID="efUnit" runat="server" Width="150px" IsReadOnly="True" IsRequired="True"/>
                </td>
            </tr>
            <tr>
                <td></td>
                <td nowrap styles='PADDING-LEFT:0px'><div><%=Resx.GetString("lblEquivalent")%>:</div></td>
                <td nowrap><v4control:Div ID="efAdvUnit" runat="server" IsReadOnly="True" Width="100px"/></td>
                <td nowrap><v4control:Number ID="efMCoef" runat="server" IsReadOnly="True" Width="100px"/></td>
                <td nowrap><v4control:Div ID="efOsnUnit" runat="server" IsReadOnly="True" Width="100px"/></td>
            </tr>
        </table>
        </div>
        
        <div id="CountryPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblCountryOfOrigin")%>:</div>
            <v4dbselect:DBSTerritory ID="efCountry" runat="server" Width="500px" IsAlwaysAdvancedSearch="True" AutoSetSingleValue="True" CSSClass="aligned_control" NextControl="efContract" OnChanged="Country_Changed"/>
        </div>

        <div id="Div14" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblCustomsDeclaration")%>:</div>
            <v4dbselect:DBSDocument ID="efGTD" runat="server" Width="500px" NextControl="efCount" AutoSetSingleValue="True" CSSClass="aligned_control" />
        </div>

        <div id="RestPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <v4control:ComboBox runat="server" ID="efRestType" Width="250" CSSClass="aligned_control" OnChanged="RestType_Changed"/>
            </div>
            <div class="inline_predicate_block">
                <div id="Div7" class="predicate_block">
                    <v4control:CheckBox runat="server" ID="efResourceChild"/>
                    <div class="label"><%=Resx.GetString("TTN_lblIncludeSubResources")%></div>
                </div>
            </div>
            <div class="inline_predicate_block">
			    <table cellpadding="0" cellspacing="2" border="0">
				    <tr>
				        <td nowrap><%=Resx.GetString("TTN_lblAtBeginningDay")%> <v4control:DatePicker id="efDateDocB" runat="server" IsReadOnly="true"/>:</td>
				        <td nowrap><v4control:Number id="efBDOst" runat="server" IsReadOnly="true"/></td>
				        <td nowrap><v4control:Div id="efBDUnit" runat="server"/></td>
				    </tr>
				    <tr>
					    <td nowrap><%=Resx.GetString("TTN_lblAtEndDay")%> <v4control:DatePicker id="DateDocE" runat="server" IsReadOnly="true"/>:</td>
				        <td nowrap><v4control:Number id="efEDOst" runat="server" IsReadOnly="true"/></td>
				        <td nowrap><v4control:Div id="efEDUnit" runat="server"/></td>
				    </tr>
			    </table>            
            </div>
        </div>
        
        <div id="CountPanel" class="predicate_block">
        <div class="label"><%=Resx.GetString("lblQuantity")%>:</div>
            <v4control:Number ID="efCount" runat="server" Width="100px" NextControl="efCostOutNDS" OnChanged="Count_Changed" IsRequired="True"/>
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
                <v4control:Number ID="efSummaNDS" runat="server" Width="100px" NextControl="efAktsiz" IsRequired="True" OnChanged="SummaNDS_Changed"/>
            </div>
        </div>

        <div id="AktsizPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblAktsiz")%>:</div>
            <v4control:Number ID="efAktsiz" runat="server" Width="100px" NextControl="efVsego"/>
        </div>

        <div id="ItogPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label"><%=Resx.GetString("lTotal")%>:</div>
                <v4control:Number ID="efVsego" runat="server" Width="100px" NextControl="btnSave" IsRequired="True" OnChanged="Vsego_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label" class="inline_predicate_block"><%=Resx.GetString("ppFltВалюта")%>:</div>
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
