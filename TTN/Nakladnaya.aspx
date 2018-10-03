<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Nakladnaya.aspx.cs" Inherits="Kesco.App.Web.Docs.TTN.Nakladnaya" %>
<%@ Register TagPrefix="v4control" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="csg" Namespace="Kesco.Lib.Web.Controls.V4.Grid" Assembly="Controls.V4" %>
<%@ Import Namespace="Kesco.Lib.Localization" %>
<%@ Import Namespace="Kesco.Lib.BaseExtention.Enums.Controls" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Nakladnaya.css" />
    <script src="/styles/Kesco.V4/JS/jquery.floatThead.min.js" type="text/javascript"></script>    
    <script src="Nakladnaya.js" type="text/javascript"></script>
    <script src="/styles/Kesco.V4/JS/Kesco.Grid.js" type="text/javascript"></script>
</head>
<body>
<div><%=RenderDocumentHeader()%></div>
<div class="spacer"></div>
<div class="v4formContainer">
<div class="marginL">
    <%RenderDocNumDateNameRows(Response.Output);%>
</div>
<div class="spacer"></div>
<div class="v4formContainer">
    <div class="predicate_block"><!--Документ корректировочный-->
        <div class="marginL">
            <div class="inline_predicate_block"><!--Документ корректировочный-->
                <div class="label" ><%:FieldLabels[CorrectableFlag]%></div>
                <v4control:CheckBox ID="CorrectableFlag" runat="server" NextControl="CorrectableTtn" CSSClass="aligned_control" OnChanged="CorrectableFlag_Changed"></v4control:CheckBox>
            </div>

            <div class="inline_predicate_block"><!--Документ корректируемый-->
                <div class="label" id="labelCorrectableTtn"><%:FieldLabels[CorrectableTtn]%></div>
                <v4dbselect:DBSDocument ID="CorrectableTtn" runat="server" Width="540px" NextControl="DateOfPosting" 
                AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch"/>
            </div>
        </div>
    </div>

    <div id="tabs">
      <ul>
        <li><a href="#tabs-1"><nobr>&nbsp;<IMG src="/styles/DocMain.gif" align=absMiddle/>&nbsp;<%=Resx.GetString("TTN_lblInvoiceDetail")%></nobr></a></li>
        <li><a href="#tabs-2"><nobr>&nbsp;<IMG src="/styles/Resource.gif" align=absMiddle/>&nbsp;<%:SectionTitles.Resource%></nobr></a></li>
        <li><a href="#tabs-3"><nobr>&nbsp;<IMG src="/styles/Service.gif" align=absMiddle/>&nbsp;<%:SectionTitles.Services%></nobr></a></li>
      </ul>
      <div id="tabs-1">
        <div id="accordion">
            <!--Данные накладной-->
            <div class="non_correctable_block" class="ui-state-disabled-invisible" id="q1"><span><b><%:SectionTitles.General%></b></span><div id="GeneralTitle" class="headerTitle"></div></div>
            <div>
                <div class="predicate_block"><!--Дата проводки 1С-->
                <div class="label" ><%:FieldLabels[DateOfPosting]%></div>
                <v4control:DatePicker ID="DateOfPosting" runat="server" NextControl="Currency" CSSClass="aligned_control"></v4control:DatePicker>
                </div>

                <div class="predicate_block"><!--Валюта оплаты-->
                <div class="label" ><%:FieldLabels[Currency]%></div>
                <v4dbselect:DBSCurrency ID="Currency" runat="server" Width="200px" CLID="25" IsAlwaysAdvancedSearch="True" NextControl="DateOfPosting" 
                AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Currency_Changed"></v4dbselect:DBSCurrency>
                </div>

                <div class="area_block"><!--Примечание для печатных форм-->
                <div class="label" ><%:FieldLabels[Notes]%></div>
                <v4control:TextArea ID="Notes" runat="server" NextControl="GoPanel" Width="400px" Height="50px" CSSClass="aligned_control"></v4control:TextArea>
                </div>
            </div>
            <!--Грузоотправитель-->
            <div class="non_correctable_block" id="GoPanel"><span><b><%:SectionTitles.GO%></b></span><div id="GoTitle" class="headerTitle"></div></div>
            <div>
                <div id="GoInfoPanel" class="predicate_block" style="display: none"><!--Реквизиты грузоотправителя-->
                <div class="label" ><%:FieldLabels[GoInfo]%></div>
                <v4control:TextBox ID="GoInfo" runat="server" IsReadonly="true" Width="100%" ></v4control:TextBox>
                </div>

                <div id="GoCodePanel" class="predicate_block" style="display: none"><!--ОКПО грузоотправителя-->
                <div class="label" ><%:FieldLabels[GoCodeInfo]%></div>
                <v4control:TextBox ID="GoCodeInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Грузоотправитель-->
                <div class="label" ><%:FieldLabels[GO]%></div>
                <v4dbselect:DBSPerson ID="GO" runat="server" Width="500px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="GoAddress"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSPerson>
                </div>

                <div class="predicate_block"><!--Адрес-->
                <div class="label" ><%:FieldLabels[GoAddress]%></div>
                <v4dbselect:DBSPersonContact ID="GoAddress" runat="server" IsAlwaysAdvancedSearch="True" NextControl="DispatchPoint"
                Width="500px" CSSClass="aligned_control"></v4dbselect:DBSPersonContact>
                </div>

                <div class="predicate_block"><!--Пункт отправления-->
                <div class="label" ><%:FieldLabels[DispatchPoint]%></div>
                <v4dbselect:DBSTransportNode ID="DispatchPoint" runat="server" IsAlwaysAdvancedSearch="True" NextControl="GoStore"
                Width="500px" CSSClass="aligned_control"></v4dbselect:DBSTransportNode>
                </div>

                <div class="predicate_block">
                <div class="inline_predicate_block"><!--Рассчетный счет-->
                <div class="label" ><%:FieldLabels[GoStore]%></div>
                <v4dbselect:DBSStore ID="GoStore" runat="server" Width="200px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="GoNotes"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSStore>
                </div>

                <div id="GoStoreInfoPanel" class="inline_predicate_block" style="display: none"><!--Банковские реквизиты грузоотправителя-->
                <div class="label" ><%:FieldLabels[GoStoreInfo]%></div>
                <v4control:TextBox ID="GoStoreInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>
                </div>

                <div class="predicate_block"><!--Отметки-->
                <div class="label" ><%:FieldLabels[GoNotes]%></div>
                <v4control:TextBox ID="GoNotes" runat="server" NextControl="GP" Width="520px" CSSClass="aligned_control"></v4control:TextBox>
                </div>
            </div>
            <!--Грузополучатель-->
            <div class="non_correctable_block"><span><b><%:SectionTitles.GP%></b></span><div id="GpTitle" class="headerTitle"></div></div>
            <div>
                <div id="GpInfoPanel" class="predicate_block" style="display: none"><!--Реквизиты грузополучателя-->
                <div class="label" ><%:FieldLabels[GpInfo]%></div>
                <v4control:TextBox ID="GpInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>

                <div id="GpCodePanel" class="predicate_block" style="display: none"><!--ОКПО грузополучателя-->
                <div class="label" ><%:FieldLabels[GpCodeInfo]%></div>
                <v4control:TextBox ID="GpCodeInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Название-->
                <div class="label" ><%:FieldLabels[GP]%></div>
                <v4dbselect:DBSPerson ID="GP" runat="server" Width="500px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="GpAddress"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSPerson>
                </div>

                <div class="predicate_block"><!--Адрес-->
                <div class="label" ><%:FieldLabels[GpAddress]%></div>
                <v4dbselect:DBSPersonContact ID="GpAddress" runat="server" IsAlwaysAdvancedSearch="True" NextControl="DestinationPoint"
                Width="500px" CSSClass="aligned_control"></v4dbselect:DBSPersonContact>
                </div>

                <div class="predicate_block"><!--Пункт отправления-->
                <div class="label" ><%:FieldLabels[DestinationPoint]%></div>
                <v4dbselect:DBSTransportNode ID="DestinationPoint" runat="server" IsAlwaysAdvancedSearch="True" NextControl="GpStore"
                Width="200px" CSSClass="aligned_control"></v4dbselect:DBSTransportNode>
                </div>

                <div class="predicate_block"><!--Рассчетный счет-->
                <div class="inline_predicate_block"><!--Рассчетный счет-->
                <div class="label" ><%:FieldLabels[GpStore]%></div>
                <v4dbselect:DBSStore ID="GpStore" runat="server" Width="500px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="GpNotes"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSStore>
                </div>
                <div id="GpStoreInfoPanel" class="inline_predicate_block" style="display: none"><!--Банковские реквизиты грузополучателя-->
                <div class="label" ><%:FieldLabels[GpStoreInfo]%></div>
                <v4control:TextBox ID="GpStoreInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>
                </div>

                <div class="predicate_block"><!--Отметки-->
                <div class="label" ><%:FieldLabels[GpNotes]%></div>
                <v4control:TextBox ID="GpNotes" runat="server" NextControl="Shipper" Width="520px" CSSClass="aligned_control"></v4control:TextBox>
                </div>
            </div>
            <!--Поставщик-->
            <div class="non_correctable_block"><span><b><%:SectionTitles.Shipper%></b></span><div id="ShipperTitle" class="headerTitle"></div></div>
            <div>
                <div class="predicate_block"><!--Совпадает-->
                <div class="label" ><%:SectionTitles.GoToShipper%></div>
                <div class="aligned_control label" ><input id="checkboxShipper" type="checkbox" runat="server" onclick="Nakladnaya.panelToPanel('#checkboxShipper','OnGoToShipper')"/></div>
                </div>

                <div id="ShipperInfoPanel" class="predicate_block" style="display: none"><!--Реквизиты поставщика-->
                <div class="label" ><%:FieldLabels[ShipperInfo]%></div>
                <v4control:TextBox ID="ShipperInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>

                <div id="ShipperCodePanel" class="predicate_block" style="display: none"><!--ОКПО поставщика-->
                <div class="label" ><%:FieldLabels[ShipperCodeInfo]%></div>
                <v4control:TextBox ID="ShipperCodeInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Поставщик-->
                <div class="label" ><%:FieldLabels[Shipper]%></div>
                <v4dbselect:DBSPerson ID="Shipper" runat="server" Width="500px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="ShipperAddress"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSPerson>
                </div>

                <div class="predicate_block"><!--Адрес-->
                <div class="label" ><%:FieldLabels[ShipperAddress]%></div>
                <v4dbselect:DBSPersonContact ID="ShipperAddress" runat="server" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="ShipperStore"
                Width="500px" CSSClass="aligned_control"></v4dbselect:DBSPersonContact>
                </div>

                <div class="predicate_block">
                <div class="inline_predicate_block"><!--Расчетный счет-->
                <div class="label" ><%:FieldLabels[ShipperStore]%></div>
                <v4dbselect:DBSStore ID="ShipperStore" runat="server" Width="200px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="Director"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSStore>
                </div>
                <div id="ShipperStoreInfoPanel" class="inline_predicate_block" style="display: none"><!--Банковские реквизиты поставщика-->
                <div class="label" ><%:FieldLabels[ShipperStoreInfo]%></div>
                <v4control:TextBox ID="ShipperStoreInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>
                </div>

                <center><p><%:SectionTitles.SignersOfPaper%></p></center>

                <div class="predicate_block"><!--Руководитель-->
                <div class="inline_predicate_block">
                <div class="label" ><%:FieldLabels[Director]%></div>
                <v4dbselect:DBSPerson ID="Director" runat="server" Width="300px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="DirectorPosition"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Director_BeforeSearch"></v4dbselect:DBSPerson>
                </div>
                <div class="inline_predicate_block"><!--Должность-->
                <div class="label" ><%:FieldLabels[DirectorPosition]%></div>
                <v4dbselect:DBSPosition ID="DirectorPosition" runat="server" Width="200px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="Accountant"
                AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="DirectorPosition_BeforeSearch"></v4dbselect:DBSPosition>
                </div>
                </div><!--Руководитель-->

                <div class="predicate_block"><!--Бухгалтер-->
                <div class="inline_predicate_block">
                <div class="label" ><%:FieldLabels[Accountant]%></div>
                <v4dbselect:DBSPerson ID="Accountant" runat="server" Width="300px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="AccountantPosition"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Accountant_BeforeSearch"></v4dbselect:DBSPerson>
                </div>
                <div class="inline_predicate_block"><!--Должность-->
                <div class="label" ><%:FieldLabels[AccountantPosition]%></div>
                <v4dbselect:DBSPosition ID="AccountantPosition" runat="server" Width="200px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="StoreKeeper"
                AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="AccountantPosition_BeforeSearch"></v4dbselect:DBSPosition>
                </div>
                </div><!--Бухгалтер-->

                <div class="predicate_block"><!--Отпуск груза произвел -->
                <div class="inline_predicate_block">
                <div class="label" ><%:FieldLabels[StoreKeeper]%></div>
                <v4dbselect:DBSPerson ID="StoreKeeper" runat="server" Width="300px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="StoreKeeperPosition"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="StoreKeeper_BeforeSearch"></v4dbselect:DBSPerson>
                </div>
                <div class="inline_predicate_block"><!--Должность-->
                <div class="label" ><%:FieldLabels[StoreKeeperPosition]%></div>
                <v4dbselect:DBSPosition ID="StoreKeeperPosition" runat="server" Width="200px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="Payer"
                AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="StoreKeeperPosition_BeforeSearch"></v4dbselect:DBSPosition>
                </div>
                </div><!--Отпуск груза произвел -->
            </div>
            <!--Плательщик-->
            <div class="non_correctable_block"><span><b><%:SectionTitles.Payer%></b></span><div id="PayerTitle" class="headerTitle"></div></div>
            <div>
                <div class="predicate_block"><!--Совпадает-->
                <div class="label" ><%:SectionTitles.GpToPayer%></div>
                <div class="aligned_control label" ><input id="checkboxPayer" runat="server" type="checkbox" onclick="Nakladnaya.panelToPanel('#checkboxPayer','OnGpToPayer')"/></div>
                </div>

                <div id="PayerInfoPanel" class="predicate_block" style="display: none"><!--Реквизиты грузополучателя-->
                <div class="label" ><%:FieldLabels[PayerInfo]%></div>
                <v4control:TextBox ID="PayerInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>

                <div id="PayerCodePanel" class="predicate_block" style="display: none"><!--ОКПО грузополучателя-->
                <div class="label" ><%:FieldLabels[PayerCodeInfo]%></div>
                <v4control:TextBox ID="PayerCodeInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Плательщик-->
                <div class="label" ><%:FieldLabels[Payer]%></div>
                <v4dbselect:DBSPerson ID="Payer" runat="server" Width="500px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="PayerAddress"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSPerson>
                </div>

                <div class="predicate_block"><!--Адрес-->
                <div class="label" ><%:FieldLabels[PayerAddress]%></div>
                <v4dbselect:DBSPersonContact ID="PayerAddress" runat="server" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="PayerStore"
                Width="500px" CSSClass="aligned_control"></v4dbselect:DBSPersonContact>
                </div>

                <div class="predicate_block"><!--Рассчетный счет-->
                <div class="inline_predicate_block">
                <div class="label" ><%:FieldLabels[PayerStore]%></div>
                <v4dbselect:DBSStore ID="PayerStore" runat="server" Width="200px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="Contract"
                IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSStore>
                </div>
                <div id="PayerStoreInfoPanel" class="inline_predicate_block" style="display: none"><!--Банковские реквизиты плательщика-->
                <div class="label" ><%:FieldLabels[PayerStoreInfo]%></div>
                <v4control:TextBox ID="PayerStoreInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>
                </div>
            </div>
            <!--Документы-->
            <div class="non_correctable_block"><span><b><%:SectionTitles.Documents%></b></span><div id="DocumentsTitle" class="headerTitle"></div></div>
            <div>
                <div id="ContractInfoPanel" class="predicate_block" style="display: none"><!--Договор - основание-->
                <div class="label" ><%:FieldLabels[ContractInfo]%></div>
                <v4control:TextBox ID="ContractInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Договор-->
                <div class="label" ><%:FieldLabels[Contract]%></div>
                <v4dbselect:DBSDocument ID="Contract" runat="server" Width="540px" NextControl="Enclosure"
                AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Contract_Changed"  OnBeforeSearch="Document_BeforeSearch"></v4dbselect:DBSDocument>
                </div>

                <div id="CuratorPanel" class="predicate_block" style="display: none"><!--Куратор договора-->
                <div class="label" ><%:SectionTitles.Curator%></div>
                <v4dbselect:DBSEmployee ID="Curator" runat="server" Width="540px" IsReadOnly="true" Visible="false"
                AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSEmployee>
                </div>

                <div class="predicate_block"><!--Приложение-->
                <div class="label" ><%:FieldLabels[Enclosure]%></div>
                <v4dbselect:DBSDocument ID="Enclosure" runat="server" Width="540px" IsMultiSelect="True" NextControl="ApplicationForPurchasing"
                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Enclosure_Changed" OnBeforeSearch="Document_BeforeSearch"></v4dbselect:DBSDocument>
                </div>

                <div class="predicate_block"><!--Заявка на покупку-->
                <div class="label" ><%:FieldLabels[ApplicationForPurchasing]%></div>
                <v4dbselect:DBSDocument ID="ApplicationForPurchasing" runat="server" Width="540px" IsMultiSelect="True" NextControl="LetterOfCredit"
                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch"></v4dbselect:DBSDocument>
                </div>

                <div class="predicate_block"><!--Аккредитив-->
                <div class="label" ><%:FieldLabels[LetterOfCredit]%></div>
                <v4dbselect:DBSDocument ID="LetterOfCredit" runat="server" Width="540px" NextControl="BillofLading"
                AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch"></v4dbselect:DBSDocument>
                </div>

                <div class="predicate_block"><!--Коносамент-->
                <div class="label" ><%:FieldLabels[BillOfLading]%></div>
                <v4dbselect:DBSDocument ID="BillOfLading" runat="server" Width="540px" IsMultiSelect="True" NextControl="Invoice"
                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch"></v4dbselect:DBSDocument>
                </div>

                <div class="predicate_block"><!--Счет, инвойс, проформа-->
                <div class="label" ><%:FieldLabels[Invoice]%></div>
                <v4dbselect:DBSDocument ID="Invoice" runat="server" Width="540px" IsMultiSelect="True" NextControl="PaymentDocuments"
                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch"></v4dbselect:DBSDocument>
                </div>

                <div class="predicate_block"><!--Счет, инвойс, проформа-->
                <div class="label" ><%:FieldLabels[PaymentDocuments]%></div>
                <v4dbselect:DBSDocument ID="PaymentDocuments" runat="server" Width="540px" IsMultiSelect="True"
                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch"></v4dbselect:DBSDocument>
                </div>
            </div>
            <!--Транспорт-->
            <div class="non_correctable_block"><span><b><%:SectionTitles.Transport%></b></span><div id="TransportTitle" class="headerTitle"></div></div>
            <div>
                <div class="predicate_block"><!--Доверенность-->
                <div class="label" ><%:FieldLabels[PowerOfAttorney]%></div>
                <v4control:TextBox ID="PowerOfAttorney" runat="server" NextControl="Driver" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Водитель-->
                <div class="label" ><%:FieldLabels[Driver]%></div>
                <v4control:TextBox ID="Driver" runat="server" NextControl="Car" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Автомобиль-->
                <div class="label" ><%:FieldLabels[Car]%></div>
                <v4control:TextBox ID="Car" runat="server" NextControl="CarNumber" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Номер автомобиля-->
                <div class="label" ><%:FieldLabels[CarNumber]%></div>
                <v4control:TextBox ID="CarNumber" runat="server" NextControl="TrailerNumber" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Номер прицепа-->
                <div class="label" ><%:FieldLabels[TrailerNumber]%></div>
                <v4control:TextBox ID="TrailerNumber" runat="server" NextControl="CarTtn" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
                </div>

                <div class="predicate_block"><!--Транспортная накладная-->
                <div class="label" ><%:FieldLabels[CarTtn]%></div>
                <v4dbselect:DBSDocument ID="CarTtn" runat="server" Width="200px"
                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control"></v4dbselect:DBSDocument>
                </div>
            </div>
        </div>
    </div>
    <!--Товары-->
    <div id="tabs-2">
        <div id="RenderProductSelectPanel" class="predicate_block">
            <v4control:Radio ID="rbProductSelect" runat="server" Name="Radio" IsRow="True" OnChanged="ProductSelect_Changed"/>
        </div>

        <div>
            <asp:HyperLink id="hrSelectVagon" runat="server"/>
        </div>

        <div id="MonthResourcePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("TTN_lblMonthResource")%>:</div>
            <v4control:DatePicker runat="server" ID="MonthResource"/>
        </div>

        <div id="ShipperStorePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblShipperStore")%>:</div>
            <v4dbselect:DBSStore ID="DBSShipperStore" runat="server" Width="500px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="PayerStore"
                                 IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="ShipperStore_Changed"/>
        </div>
        
        <div id="PayerStorePanel" class="predicate_block">
            <div class="label"><%=Resx.GetString("lblPayerStore")%>:</div>
            <v4dbselect:DBSStore ID="DBSPayerStore" runat="server" Width="500px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="Res"
                                 IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="PayerStore_Changed"/>
        </div>
        
        <div>
            <v4control:Button ID="addResource" runat="server"></v4control:Button>
            <div class="spacer"></div>
            <csg:Grid runat="server" ID="GridResource" MarginBottom="500" ExistServiceColumn="True"/>
        </div>
    </div>
    <!--Услуги-->
    <div id="tabs-3">
        <div>
            <v4control:Button ID="addFactUsl" runat="server"></v4control:Button>
            <div class="spacer"></div>
            <csg:Grid runat="server" ID="GridUsl" MarginBottom="500" ExistServiceColumn="True"></csg:Grid>
        </div>
    </div>
</div>

<div class="marginA">
    <div id="itog">
        <div class="non_correctable_block"><span style="color: #454545"><b><%=Resx.GetString("lblItogoInvoice")%>:</b></span><div id="Div1" class="headerTitle"></div></div>    
    </div>
    
    <table class="itogtable" style="width:100%;">
    <tr>
        <td class="itogtable" style="background-color:#ededed">&nbsp;</td>
        <td class="itogtable" style="background-color:#ededed" align="center"><%=Resx.GetString("lblSummaOutNDS")%></td>
        <td class="itogtable" style="background-color:#ededed" align="center"><%=Resx.GetString("lblNDS")%></td>
        <td class="itogtable" style="background-color:#ededed" align="center"><%=Resx.GetString("lTotal")%></td>
    </tr>
    <tr>
        <td class="itogtable" style="background-color:#ededed"><%=Resx.GetString("lblProducts")%>:</td>
        <td class="itogtable"><%:ItogArray[0,0]%></td>
        <td class="itogtable"><%:ItogArray[0,1]%></td>
        <td class="itogtable"><%:ItogArray[0,2]%></td>
    </tr>
    <tr>
        <td class="itogtable" style="background-color:#ededed"><%=Resx.GetString("lblServices")%>:</td>
        <td class="itogtable"><%:ItogArray[1,0]%></td>
        <td class="itogtable"><%:ItogArray[1,1]%></td>
        <td class="itogtable"><%:ItogArray[1,2]%></td>
    </tr>
    <tr>
        <td class="itogtable" style="background-color:#ededed"><%=Resx.GetString("lTotal")%>:</td>
        <td class="itogtable"><%:ItogArray[0, 0] + ItogArray[1, 0]%></td>
        <td class="itogtable"><%:ItogArray[0, 1] + ItogArray[1, 1]%></td>
        <td class="itogtable"><%:ItogArray[0, 2] + ItogArray[1, 2]%></td>
    </tr>
    </table>
</div>
</div>

<div class="spacer"></div>
<% StartRenderVariablePart(Response.Output);%>
<% EndRenderVariablePart(Response.Output);%>

<div id="divResourceAdd" style="display: none;">
    <div class="v4DivTable" id="divProgressBar" style="display: none; width: 100%; height:100%">
        <div class="v4DivTableRow">
                <div class="v4DivTableCell">
                    <img src="/styles/ProgressBar.gif"/><br/><%=Resx.GetString("lblWait")%>...
                </div>
        </div>
    </div>
    <div id="divFrame">
    <iframe id="ifrMris" style="width:100%; height: 100%;" onload="setIframeHeight();"></iframe>
    </div>
</div>

<div id="divSelectVagon" style="display: none;">
    <div class="v4DivTable" id="divVagonProgressBar" style="display: none; width: 100%; height:100%">
        <div class="v4DivTableRow">
                <div class="v4DivTableCell">
                    <img src="/styles/ProgressBar.gif"/><br/><%=Resx.GetString("lblWait")%>...
                </div>
        </div>
    </div>
    <div id="divVagonFrame">
    <iframe id="ifrVagon" style="width:100%; height: 100%;" onload="setIframeVagonHeight();"></iframe>
    </div>
</div>

</div>

</body>
</html>
