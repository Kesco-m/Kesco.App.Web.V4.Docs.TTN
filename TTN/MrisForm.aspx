<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MrisForm.aspx.cs" Inherits="Kesco.App.Web.Docs.TTN.MrisForm" %>
<%@ Register TagPrefix="v4control" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Kesco.Nakladnaya.css" />
</head>
<script src="Kesco.Mris.js" type="text/javascript"></script>
<body>
<div class="marginD"><%=RenderDocumentHeader()%></div>    
    <div class="v4formContainer">
        <div class="marginL">
        <div id="ShipperStorePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblShipperStore")%>:</div>
            <v4dbselect:DBSStore ID="efStoreShipper" runat="server" Width="370px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="efStorePayer"
                                 AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="ShipperStore_Changed" />
        </div>
        
        <div id="PayerStorePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblPayerStore")%>:</div>
            <v4dbselect:DBSStore ID="efStorePayer" runat="server" Width="370px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="efResource"
                                  AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="PayerStore_Changed"/>
        </div>
        
        <div id="ResourcePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblProduct")%>:</div>
            <v4dbselect:DBSResource ID="efResource" runat="server" Width="370px" CLID="25" IsAlwaysAdvancedSearch="True" NextControl="efUnitAdv"
             AutoSetSingleValue="True" IsRequired="True" CSSClass="aligned_control" OnChanged="Resource_Changed"/>
        </div>

        <div id="ResourceRusPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("TTN_lblProductNameRus")%>:</div>
            <v4control:TextBox ID="efResourceRus" runat="server" Width="370px" IsRequired="True" CSSClass="aligned_control" NextControl="efResourceLat"/>
        </div>
        
        <div id="ResourceLatPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("TTN_lblProductNameLat")%>:</div>
            <v4control:TextBox ID="efResourceLat" runat="server" Width="370px" CSSClass="aligned_control" NextControl="efUnitAdv"/>
        </div>
        
        <div id="Div3" class="predicate_block">
            <div class="inline_predicate_block" id="trUnitAdv">
                <div class="label"><%=Resx.GetString("TTN_lblUnitsSelect")%>:</div>
                <v4dbselect:DBSUnitAdv ID="efUnitAdv" runat="server" Width="80px" CSSClass="aligned_control" NextControl="efCountry" OnChanged="UnitAdv_Changed"/>
            </div>
            <div class="inline_predicate_block">
                <table cellpadding="0" cellspacing="0" border="0">
                    <tr>
                        <td>
                            <div class="<%=DivHeaderClass()%>"><%=Resx.GetString("lblUnitShort")%>:&nbsp;</div>
                        </td>
                        <td colspan="4" style="PADDING-LEFT: 5px; white-space: nowrap">
                            <v4dbselect:DBSUnit ID="efUnit" runat="server" Width="100px" IsReadOnly="True" IsRequired="True"/>
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

        <div id="CountryPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblCountryOfOrigin")%>:</div>
            <v4dbselect:DBSTerritory ID="efCountry" runat="server" Width="370px" IsAlwaysAdvancedSearch="True" AutoSetSingleValue="True" CSSClass="aligned_control" NextControl="efGTD" OnChanged="Country_Changed"/>
        </div>

        <div id="Div14" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblCustomsDeclaration")%>:</div>
            <v4dbselect:DBSDocument ID="efGTD" runat="server" Width="370px" NextControl="efCount" AutoSetSingleValue="True" CSSClass="aligned_control" />
        </div>
        
        <div id="CountPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label_100"><%=Resx.GetString("lblQuantity")%>:</div>
                <v4control:Number ID="efCount" runat="server" Width="100px" NextControl="efCostOutNDS" OnChanged="Count_Changed" IsRequired="True"/>
                <v4control:Div id="efErKol" runat="server"/>
                <v4control:Div id="efGross" runat="server"/>
            </div>
            <div class="inline_predicate_block_left">
                <v4control:Button runat="server" Text="Проверить остаток" ID="btnRest" OnClick="cmd('cmd','CheckRest')"/>
            </div>
        </div>
        
        <div id="CostPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label_100"><%=Resx.GetString("lblCostOutNDS")%>:</div>
                <v4control:Number ID="efCostOutNDS" runat="server" Width="100px" NextControl="efStavkaNDS" IsRequired="True" OnChanged="CostOutNDS_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label_100"><%=Resx.GetString("lblNDSRate")%>:</div>
                <v4dbselect:DBSStavkaNDS ID="efStavkaNDS" Width="100px" runat="server" IsRequired="True" CSSClass="aligned_control" NextControl="efSummaOutNDS" OnChanged="StavkaNDS_Changed"/>
            </div>
        </div>
      
        <div id="SumPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label_100"><%=Resx.GetString("lblSummaOutNDS")%>:</div>
                <v4control:Number ID="efSummaOutNDS" runat="server" Width="100px" NextControl="efSummaNDS" IsRequired="True" OnChanged="SummaOutNDS_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label_100"><%=Resx.GetString("lblNDS")%>:</div>
                <v4control:Number ID="efSummaNDS" runat="server" Width="100px" NextControl="efAktsiz" IsRequired="True" OnChanged="SummaNDS_Changed"/>
            </div>
        </div>

        <div id="AktsizPanel" class="predicate_block">
            <div class="label_100"><%=Resx.GetString("lblAktsiz")%>:</div>
            <v4control:Number ID="efAktsiz" runat="server" Width="100px" NextControl="efVsego"/>
        </div>

        <div id="ItogPanel" class="predicate_block">
            <div class="inline_predicate_block">
                <div class="label_100"><%=Resx.GetString("lTotal")%>:</div>
                <v4control:Number ID="efVsego" runat="server" Width="100px" NextControl="btnSave" IsRequired="True" OnChanged="Vsego_Changed"/>
            </div>
            <div class="inline_predicate_block_left">
                <div class="label_100" class="inline_predicate_block"><%=Resx.GetString("ppFltВалюта")%>:</div>
                <div ID="efCurrency" runat="server" class="inline_predicate_block_text"></div>
            </div>
        </div>
        <!--Изменил Изменено-->
        <div class="footer">
            <v4control:Changed ID="efChanged" runat="server"/>
        </div>
        </div>
        
        <div id="divRest" style="display: none; text-align: center;">
			<table cellpadding="0" cellspacing="2" border="0" width="100%">
				<tr>
				    <td colspan="3">
				        <v4control:ComboBox runat="server" ID="efRestType" Width="230" OnChanged="RestType_Changed"/>            
				    </td>
                </tr>
				<tr>
				    <td colspan="3">
				        <div id="Div1" class="predicate_block">
                            <div class="inline_predicate_block_text"><v4control:CheckBox runat="server" ID="efResourceChild" OnChanged="RestType_Changed"/></div>
                            <div class="inline_predicate_block_text"><%=Resx.GetString("TTN_lblIncludeSubResources")%></div>
                        </div>
				    </td>
                </tr>
                <tr>
				    <td nowrap><%=Resx.GetString("TTN_lblAtBeginningDay")%> <v4control:DatePicker id="efDateDocB" runat="server" IsReadOnly="true"/>:</td>
				    <td nowrap><v4control:Div id="efBDOst" runat="server" IsReadOnly="true"/></td>
				    <td nowrap><v4control:Div id="efBDUnit" runat="server"/></td>
				</tr>
				<tr>
					<td nowrap><%=Resx.GetString("TTN_lblAtEndDay")%> <v4control:DatePicker id="DateDocE" runat="server" IsReadOnly="true"/>:</td>
				    <td nowrap><v4control:Div id="efEDOst" runat="server" IsReadOnly="true"/></td>
				    <td nowrap><v4control:Div id="efEDUnit" runat="server"/></td>
				</tr>
			</table>            
        </div>
    </div>
</body>
</html>
