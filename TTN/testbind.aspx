<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="testbind.aspx.cs" Inherits="TTN.testbind" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <div class="v4formContainer" style="margin-left: 50px;margin-top: 50px">
         <v4dbselect:DBSResource ID="efResource" runat="server" Width="500px" CLID="25" IsAlwaysAdvancedSearch="True" 
             AutoSetSingleValue="True" IsRequired="True" CSSClass="aligned_control"/>
             
         <v4dbselect:Button runat="server" ID="btn" OnClick="cmd('cmd', 'ChangeMris');" Text="OK"/>

    </div>
</body>
</html>
