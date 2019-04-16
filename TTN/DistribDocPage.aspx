<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DistribDocPage.aspx.cs" Inherits="Kesco.App.Web.Docs.TTN.DistribDocPage" %>
<%@ Register TagPrefix="v4control" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Kesco.Nakladnaya.css" />
    <style type="text/css">
        .nmb {background-color:white;text-align:right;padding-right:2px;}
	    .gn {font-weight:bold;COLOR: green;text-align:right}
	    .rn {font-weight:bold;COLOR: red;text-align:right}
	    .header {font-weight:bold;text-align: center;}
    </style>
    <script src="Kesco.Distrib.js" type="text/javascript"></script>
</head>
<body>
<div class="marginL"><%=RenderDocumentHeader()%></div>    
    <div class="v4formContainer">
        <div class="marginL">
            <div class="spacer"></div>
            <v4control:Div id="divTitle" runat="server" CSSClass="header"/>
            <div id="ShipperPayerPanel" class="predicate_block">
                <div class="label" ><%=Resx.GetString("TTN_lblOwherResource")%>:</div>
                <v4dbselect:DBSPerson ID="efShipperPayer" runat="server" Width="500px" CSSClass="aligned_control" IsReadOnly="True"/>
            </div>
            
        <div id="ResourcePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblResource")%>:</div>
            <v4dbselect:DBSResource ID="efResource" runat="server" Width="500px" CSSClass="aligned_control" IsReadOnly="True"/>
        </div>

        <div id="UnitPanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblUnit")%>:</div>
            <v4dbselect:DBSUnit ID="efUnit" runat="server" Width="500px" CSSClass="aligned_control" IsReadOnly="True"/>
        </div>

        <div id="Div1" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblPlace")%>:</div>
            <v4dbselect:DBSResidence ID="efResidence" runat="server" Width="500px" CSSClass="aligned_control" IsReadOnly="True"/>
        </div>

        <div id="Div2" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblStore")%>:</div>
            <v4dbselect:DBSStore ID="efStore" runat="server" Width="500px" CSSClass="aligned_control" IsReadOnly="True"/>
        </div>
        
        <table cellspacing="2" cellpadding="2" width="100%">
            <tr>
                <td><input type="radio" name="g1" onclick="filterGrid(0);" id="all"/>&nbsp;<label for="all"><%=Resx.GetString("TTN_lblShowAllRest")%></label></td>
                <td><input type="radio" name="g1" onclick="filterGrid(2);" id="rvagon"/>&nbsp;<label for="rvagon"><%=Resx.GetString("TTN_lblVagonFilter")%></label></td>
                <td><v4control:TextBox runat="server" ID="efVagon" OnChanged="Vagon_Changed"/></td>
            </tr>
            <tr>
                <td><input type="radio" name="g1" onclick="filterGrid(1);" id="only"/>&nbsp;<label for="only"><%=Resx.GetString("TTN_lblDisplaySetsDocument")%></label></td>
                <td><input type="radio" name="g1" onclick="filterGrid(3);" id="rzdkv"/>&nbsp;<label for="rzdkv"><%=Resx.GetString("TTN_lblFilterInvoiceNumber")%></label></td>
				<td><v4control:TextBox runat="server" ID="efKvitanciya" OnChanged="Kvitanciya_Changed"/></td>
            </tr>
        </table>
        
        <div id="divMainTable">
            <% RenderOstatkiTable(Response.Output); %>
        </div>

        </div>
    </div>
</body>
</html>
