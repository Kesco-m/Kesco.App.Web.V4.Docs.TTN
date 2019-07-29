<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Nakladnaya.aspx.cs" Inherits="Kesco.App.Web.Docs.TTN.Nakladnaya" %>
<%@ Register TagPrefix="v4control" Namespace="Kesco.Lib.Web.Controls.V4" Assembly="Controls.V4" %>
<%@ Register TagPrefix="v4dbselect" Namespace="Kesco.Lib.Web.DBSelect.V4" Assembly="DBSelect.V4" %>
<%@ Register TagPrefix="csg" Namespace="Kesco.Lib.Web.Controls.V4.Grid" Assembly="Controls.V4" %>
<%@ Register TagPrefix="csld" Namespace="Kesco.Lib.Web.DBSelect.V4.LinkedDoc" Assembly="DBSelect.V4" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <link rel="stylesheet" type="text/css" href="Kesco.Nakladnaya.css"/>
    <script src="Kesco.Nakladnaya.js?v=1" type="text/javascript"></script>
</head>
<body>
<div><%= RenderDocumentHeader() %></div>
<div id="divKuratorSign" class="warning" style="-ms-word-wrap: normal; display: table; text-align: right; width: 99%;">
    <%= RenderKuratorSign(Contract.Value) %>
    <div class="spacer"></div>
</div>
<div class="v4formContainer">
<div class="marginL">
    <% RenderDocNumDateNameRows(Response.Output); %>
</div>
<div class="v4formContainer">
    <div class="predicate_block" id="divCorrectable">
        <!--Документ корректировочный-->
        <div class="marginL">
            <div class="inline_predicate_block">
                <!--Документ корректировочный-->
                <div class="label"><%: FieldLabels[CorrectableFlag] %></div>
                <v4control:CheckBox ID="CorrectableFlag" runat="server" NextControl="CorrectableTtn" CSSClass="aligned_control" OnChanged="CorrectableFlag_Changed"></v4control:CheckBox>
            </div>

            <div class="inline_predicate_block">
                <!--Документ корректируемый-->
                <div class="label" id="labelCorrectableTtn"><%: FieldLabels[CorrectableTtn] %></div>
                <v4dbselect:DBSDocument ID="CorrectableTtn" runat="server" Width="370px" NextControl="DateOfPosting"
                                        AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch"/>
            </div>
        </div>
    </div>
    <%= RenderCorrectingDoc() %> <!--Документ корректировочный-->
</div>
<div id="tabs">
<ul>
    <li id="tabs1">
        <a href="#tabs-1">
            <nobr>&nbsp;<%= Resx.GetString("TTN_lblInvoiceDetail") %></nobr>
        </a>
    </li>
    <li id="tabs2">
        <a href="#tabs-2">
            <nobr>&nbsp;<%: SectionTitles.Resource %></nobr>
        </a>
    </li>
    <li id="tabs3">
        <a href="#tabs-3">
            <nobr>&nbsp;<%: SectionTitles.Services %></nobr>
        </a>
    </li>
</ul>
<div id="tabs-1">
<div id="accordion">
<!--Данные накладной-->
<div class="non_correctable_block" class="ui-state-disabled-invisible" id="q1">
    <span>
        <b><%: SectionTitles.General %></b>
    </span>
    <div id="GeneralTitle" class="headerTitle"></div>
</div>
<div>
    <div class="predicate_block">
        <!--Дата проводки 1С-->
        <div class="label"><%: FieldLabels[DateOfPosting] %></div>
        <v4control:DatePicker ID="DateOfPosting" runat="server" NextControl="Currency" CSSClass="aligned_control"></v4control:DatePicker>
    </div>

    <div class="predicate_block">
        <!--Валюта оплаты-->
        <div class="label"><%: FieldLabels[Currency] %></div>
        <v4dbselect:DBSCurrency ID="Currency" runat="server" Width="100px" CLID="25" IsAlwaysAdvancedSearch="True" NextControl="Notes"
                                AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Currency_Changed">
        </v4dbselect:DBSCurrency>
    </div>

    <div class="area_block" id="divNotes">
        <!--Примечание для печатных форм-->
        <div class="label">
            <%: FieldLabels[Notes] %><br/><span style="font-size: 7pt">(<%= Resx.GetString("lblPrnDescription") %>)</span>
        </div>
        <v4control:TextArea ID="Notes" runat="server" NextControl="GoTitle" Width="400px" Height="50px" CSSClass="aligned_control"></v4control:TextArea>
    </div>
</div>
<!--Грузоотправитель-->
<div class="non_correctable_block" id="GoPanel">
    <span>
        <b><%: SectionTitles.GO %></b>
    </span>
    <div id="GoTitle" class="headerTitle"></div>
</div>
<div>
    <div class="predicate_block" id="divShipperToGo" runat="server">
        <!--Совпадает-->
        <v4control:Button ID="btnShipperToGo" runat="server" OnClick="cmd ('cmd', 'OnShipperToGo');"></v4control:Button>
    </div>

    <div class="predicate_block">
        <!--Название-->
        <div class="label"><%: FieldLabels[GO] %></div>
        <v4dbselect:DBSPerson ID="GO" runat="server" Width="380px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="DispatchPoint"
                              IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control">
        </v4dbselect:DBSPerson>
    </div>

    <div class="predicate_block">
        <!--Пункт отправления-->
        <div class="label"><%: FieldLabels[DispatchPoint] %></div>
        <v4dbselect:DBSTransportNode ID="DispatchPoint" runat="server" NextControl="GoNotes"
                                     Width="380px" CSSClass="aligned_control" AutoSetSingleValue="True">
        </v4dbselect:DBSTransportNode>
    </div>

    <div class="predicate_block">
        <!--Отметки-->
        <div class="label"><%: FieldLabels[GoNotes] %></div>
        <v4control:TextBox ID="GoNotes" runat="server" NextControl="GoStore" Width="380px" CSSClass="aligned_control"></v4control:TextBox>
    </div>

    <div id="GoInfoPanel" class="predicate_block" style="display: none">
        <!--Реквизиты грузоотправителя-->
        <div class="label">
            <%: FieldLabels[GoInfo] %>
            <a href="#" onclick="cmd('cmd', 'GetContact', 'Type', 'Go'); ">
                <img id="imgGoAddress" runat="server" border="0" src="/styles/contact.gif" alt="выбрать адрес отличный от юридического"/>
            </a>
        </div>
        <v4control:TextBox ID="GoInfo" runat="server" IsReadonly="true" Width="380px" NextControl="GoStore"></v4control:TextBox>
    </div>

    <div id="GoCodePanel" class="predicate_block" style="display: none">
        <!--ОКПО грузоотправителя-->
        <div class="label"><%: FieldLabels[GoCodeInfo] %></div>
        <v4control:TextBox ID="GoCodeInfo" runat="server" IsReadonly="true" Width="100%" Visible="false" NextControl="GoStore"></v4control:TextBox>
    </div>

    <div class="predicate_block">
        <div class="inline_predicate_block">
            <!--Рассчетный счет-->
            <div class="label"><%: FieldLabels[GoStore] %></div>
            <v4dbselect:DBSStore ID="GoStore" runat="server" Width="380px" CLID="18" IsAlwaysAdvancedSearch="True"
                                 AutoSetSingleValue="True" CSSClass="aligned_control">
            </v4dbselect:DBSStore>
        </div>

        <div id="GoStoreInfoPanel" class="inline_predicate_block" style="display: none">
            <!--Банковские реквизиты грузоотправителя-->
            <div class="label"><%: FieldLabels[GoStoreInfo] %></div>
            <v4control:TextBox ID="GoStoreInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
        </div>
    </div>

</div>
<!--Грузополучатель-->
<div class="non_correctable_block">
    <span>
        <b><%: SectionTitles.GP %></b>
    </span>
    <div id="GpTitle" class="headerTitle"></div>
</div>
<div>
    <div class="predicate_block" id="divPayerToGp" runat="server">
        <!--Совпадает-->
        <v4control:Button ID="btnPayerToGp" runat="server" OnClick="cmd ('cmd', 'OnPayerToGp');"></v4control:Button>
    </div>

    <div class="predicate_block">
        <!--Название-->
        <div class="label"><%: FieldLabels[GP] %></div>
        <v4dbselect:DBSPerson ID="GP" runat="server" Width="380px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="DestinationPoint"
                              IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control">
        </v4dbselect:DBSPerson>
    </div>

    <div class="predicate_block">
        <!--Пункт назначения-->
        <div class="label"><%: FieldLabels[DestinationPoint] %></div>
        <v4dbselect:DBSTransportNode ID="DestinationPoint" runat="server" NextControl="GpNotes"
                                     Width="380px" CSSClass="aligned_control" AutoSetSingleValue="True">
        </v4dbselect:DBSTransportNode>
    </div>

    <div class="predicate_block">
        <!--Отметки-->
        <div class="label"><%: FieldLabels[GpNotes] %></div>
        <v4control:TextBox ID="GpNotes" runat="server" Width="380px" CSSClass="aligned_control" NextControl="GpStore"></v4control:TextBox>
    </div>

    <div id="GpInfoPanel" class="predicate_block" style="display: none">
        <!--Реквизиты грузополучателя-->
        <div class="label">
            <%: FieldLabels[GpInfo] %>
            <a href="#" onclick="cmd('cmd', 'GetContact', 'Type', 'Gp'); ">
                <img id="imgGpAddress" runat="server" border="0" src="/styles/contact.gif" alt="выбрать адрес отличный от юридического"/>
            </a>
        </div>
        <v4control:TextBox ID="GpInfo" runat="server" IsReadonly="true" Width="380px" NextControl="GpStore"></v4control:TextBox>
    </div>

    <div id="GpCodePanel" class="predicate_block" style="display: none">
        <!--ОКПО грузополучателя-->
        <div class="label"><%: FieldLabels[GpCodeInfo] %></div>
        <v4control:TextBox ID="GpCodeInfo" runat="server" IsReadonly="true" Width="100%" Visible="false" NextControl="GpStore"></v4control:TextBox>
    </div>

    <div class="predicate_block">
        <!--Рассчетный счет-->
        <div class="inline_predicate_block">
            <!--Рассчетный счет-->
            <div class="label"><%: FieldLabels[GpStore] %></div>
            <v4dbselect:DBSStore ID="GpStore" runat="server" Width="380px" CLID="18" IsAlwaysAdvancedSearch="True"
                                 AutoSetSingleValue="True" CSSClass="aligned_control">
            </v4dbselect:DBSStore>
        </div>

        <div id="GpStoreInfoPanel" class="inline_predicate_block" style="display: none">
            <!--Банковские реквизиты грузополучателя-->
            <div class="label"><%: FieldLabels[GpStoreInfo] %></div>
            <v4control:TextBox ID="GpStoreInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
        </div>
    </div>

</div>
<!--Поставщик-->
<div class="non_correctable_block">
    <span>
        <b><%: SectionTitles.Shipper %></b>
    </span>
    <div id="ShipperTitle" class="headerTitle"></div>
</div>
<div>
    <div class="predicate_block" id="divGoToShipper" runat="server">
        <!--Совпадает-->
        <v4control:Button ID="btnGoToShipper" runat="server" OnClick="cmd ('cmd', 'OnGoToShipper');"></v4control:Button>
    </div>

    <div class="predicate_block">
        <!--Поставщик-->
        <div class="label"><%: FieldLabels[Shipper] %></div>
        <v4dbselect:DBSPerson ID="Shipper" runat="server" Width="380px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="ShipperStore"
                              IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control">
        </v4dbselect:DBSPerson>
    </div>

    <div class="predicate_block">
        <!--Адрес-->
        <div class="label">
            <%: FieldLabels[ShipperAddress] %>
            <a href="#" onclick="cmd('cmd', 'GetContact', 'Type', 'Shipper'); ">
                <img id="imgShipperAddress" runat="server" border="0" src="/styles/contact.gif" alt="выбрать адрес отличный от юридического"/>
            </a>
        </div>
        <v4control:TextBox ID="ShipperAddress" runat="server" IsReadonly="true" Width="370px" NextControl="ShipperStore"></v4control:TextBox>
    </div>

    <div id="ShipperInfoPanel" class="predicate_block" style="display: none">
        <!--Название поставщика-->
        <div class="label"><%: FieldLabels[ShipperInfo] %></div>
        <v4control:TextBox ID="ShipperInfo" runat="server" IsReadonly="true" Width="380px" Visible="false" NextControl="ShipperStore"></v4control:TextBox>
    </div>

    <div id="ShipperCodePanel" class="predicate_block" style="display: none">
        <!--ОКПО поставщика-->
        <div class="label"><%: FieldLabels[ShipperCodeInfo] %></div>
        <v4control:TextBox ID="ShipperCodeInfo" runat="server" IsReadonly="true" Width="100px" Visible="false" NextControl="ShipperStore"></v4control:TextBox>
    </div>

    <div class="predicate_block">
        <div class="inline_predicate_block">
            <!--Расчетный счет-->
            <div class="label"><%: FieldLabels[ShipperStore] %></div>
            <v4dbselect:DBSStore ID="ShipperStore" runat="server" Width="380px" CLID="18" IsAlwaysAdvancedSearch="True"
                                 AutoSetSingleValue="True" CSSClass="aligned_control">
            </v4dbselect:DBSStore>
        </div>
        <div id="ShipperStoreInfoPanel" class="inline_predicate_block" style="display: none">
            <!--Банковские реквизиты поставщика-->
            <div class="label"><%: FieldLabels[ShipperStoreInfo] %></div>
            <v4control:TextBox ID="ShipperStoreInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
        </div>
    </div>
    <br/>
    <fieldset style="padding: 10px;">
        <legend style="left: 5px; margin-left: 20px; padding: 5px;"><%: SectionTitles.SignersOfPaper %></legend>
        <table border="0" cellspacing="0" cellpadding="0">
            <tr>
                <!--Руководитель-->
                <td width="185px">
                    <%: FieldLabels[Director] %>
                    <div id="divDirector" class="inline_predicate_block">
                        <a href="#" onclick="cmd('cmd', 'GetPerson', 'Type', 'Director'); ">
                            <img id="imgDirector" border="0" src="/styles/User.gif" alt="выбрать сотрудника"/>
                        </a>
                    </div>
                    <div id="divDirectorDelete" class="inline_predicate_block">
                        <div id="divDD" class="inline_predicate_block">
                            <a href="#" onclick="cmd('cmd', 'DeletePerson', 'Type', 'Director'); ">
                                <img id="imgDirectorDelete" border="0" src="/styles/delete.gif" alt="удалить сотрудника"/>
                            </a>
                        </div>
                    </div>
                </td>
                <td width="200px">
                    <v4control:TextBox ID="Director" Width="200px" runat="server" IsReadOnly="True" CSSClass="aligned_control"/>
                </td>
                <td width="200px"><%: FieldLabels[DirectorPosition] %></td>
                <td width="300px">
                    <v4control:TextBox ID="DirectorPosition" Width="200px" runat="server" IsReadonly="true" CSSClass="aligned_control"/>
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <br/>
                </td>
            </tr>
            <tr>
                <!--Бухгалтер-->
                <td width="185px">
                    <%: FieldLabels[Accountant] %>
                    <div id="divAccountant" class="inline_predicate_block">
                        <a href="#" onclick="cmd('cmd', 'GetPerson', 'Type', 'Accountant'); ">
                            <img id="imgAccountant" border="0" src="/styles/User.gif" alt="выбрать сотрудника"/>
                        </a>
                    </div>
                    <div id="divAccountantDelete" class="inline_predicate_block">
                        <div id="divAD" class="inline_predicate_block">
                            <a href="#" onclick="cmd('cmd', 'DeletePerson', 'Type', 'Accountant'); ">
                                <img id="imgAccountantDelete" border="0" src="/styles/delete.gif" alt="удалить сотрудника"/>
                            </a>
                        </div>
                    </div>
                </td>
                <td width="200px">
                    <v4control:TextBox ID="Accountant" Width="200px" runat="server" IsReadOnly="True" CSSClass="aligned_control"/>
                </td>
                <td width="200px"><%: FieldLabels[AccountantPosition] %></td>
                <td width="300px">
                    <v4control:TextBox ID="AccountantPosition" Width="200px" runat="server" IsReadonly="true" CSSClass="aligned_control"/>
                </td>
            </tr>
            <tr>
                <td colspan="4">
                    <br/>
                </td>
            </tr>
            <tr>
                <!--Отпуск груза произвел-->
                <td width="185px">
                    <%: FieldLabels[StoreKeeper] %>
                    <div id="divStoreKeeper" class="inline_predicate_block">
                        <a href="#" onclick="cmd('cmd', 'GetPerson', 'Type', 'StoreKeeper'); ">
                            <img id="imgStoreKeeper" border="0" src="/styles/User.gif" alt="выбрать сотрудника"/>
                        </a>
                    </div>
                    <div id="divStoreKeeperDelete" class="inline_predicate_block">
                        <div id="divSD" class="inline_predicate_block">
                            <a href="#" onclick="cmd('cmd', 'DeletePerson', 'Type', 'StoreKeeper'); ">
                                <img id="imgStoreKeeperDelete" border="0" src="/styles/delete.gif" alt="удалить сотрудника"/>
                            </a>
                        </div>
                    </div>
                </td>
                <td width="200px">
                    <v4control:TextBox ID="StoreKeeper" Width="200px" runat="server" IsReadOnly="True" CSSClass="aligned_control"/>
                </td>
                <td width="200px"><%: FieldLabels[StoreKeeperPosition] %></td>
                <td width="300px">
                    <v4control:TextBox ID="StoreKeeperPosition" Width="200px" runat="server" IsReadonly="true" CSSClass="aligned_control"/>
                </td>
            </tr>
        </table>
    </fieldset>
</div>
<!--Плательщик-->
<div class="non_correctable_block">
    <span>
        <b><%: SectionTitles.Payer %></b>
    </span>
    <div id="PayerTitle" class="headerTitle"></div>
</div>
<div>
    <div class="predicate_block" id="divGpToPayer" runat="server">
        <!--Совпадает-->
        <v4control:Button ID="btnGpToPayer" runat="server" OnClick="cmd ('cmd', 'OnGpToPayer');"></v4control:Button>
    </div>

    <div class="predicate_block">
        <!--Плательщик-->
        <div class="label"><%: FieldLabels[Payer] %></div>
        <v4dbselect:DBSPerson ID="Payer" runat="server" Width="380px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="PayerStore"
                              IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control">
        </v4dbselect:DBSPerson>
    </div>

    <div class="predicate_block">
        <!--Адрес-->
        <div class="label">
            <%: FieldLabels[PayerAddress] %>
            <a href="#" onclick="cmd('cmd', 'GetContact', 'Type', 'Payer'); ">
                <img id="imgPayerAddress" runat="server" border="0" src="/styles/contact.gif" alt="выбрать адрес отличный от юридического"/>
            </a>
        </div>
        <v4control:TextBox ID="PayerAddress" runat="server" IsReadonly="true" Width="370px" NextControl="PayerStore"></v4control:TextBox>
    </div>

    <div id="PayerInfoPanel" class="predicate_block" style="display: none">
        <!--Название плательщика-->
        <div class="label"><%: FieldLabels[PayerInfo] %></div>
        <v4control:TextBox ID="PayerInfo" runat="server" IsReadonly="true" Width="380px" Visible="false" NextControl="PayerStore"></v4control:TextBox>
    </div>

    <div id="PayerCodePanel" class="predicate_block" style="display: none">
        <!--ОКПО плательщика-->
        <div class="label"><%: FieldLabels[PayerCodeInfo] %></div>
        <v4control:TextBox ID="PayerCodeInfo" runat="server" IsReadonly="true" Width="100px" Visible="false" NextControl="PayerStore"></v4control:TextBox>
    </div>

    <div class="predicate_block">
        <!--Рассчетный счет-->
        <div class="inline_predicate_block">
            <div class="label"><%: FieldLabels[PayerStore] %></div>
            <v4dbselect:DBSStore ID="PayerStore" runat="server" Width="380px" CLID="18" IsAlwaysAdvancedSearch="True"
                                 AutoSetSingleValue="True" CSSClass="aligned_control">
            </v4dbselect:DBSStore>
        </div>
        <div id="PayerStoreInfoPanel" class="inline_predicate_block" style="display: none">
            <!--Банковские реквизиты плательщика-->
            <div class="label"><%: FieldLabels[PayerStoreInfo] %></div>
            <v4control:TextBox ID="PayerStoreInfo" runat="server" IsReadonly="true" Width="100%" Visible="false"></v4control:TextBox>
        </div>
    </div>
</div>
<!--Документы-->
<div class="non_correctable_block">
    <span>
        <b><%: SectionTitles.Documents %></b>
    </span>
    <div id="DocumentsTitle" class="headerTitle"></div>
</div>
<div>
    <div class="predicate_block">
        <!--Договор-->
        <div class="label"><%: FieldLabels[Contract] %></div>
        <span id="advIcons" runat="server"></span>
        <v4dbselect:DBSDocument ID="Contract" runat="server" Width="370px" NextControl="Enclosure"
                                AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Contract_Changed" OnBeforeSearch="Document_BeforeSearch" IsAdvIcons="True">
        </v4dbselect:DBSDocument>
    </div>

    <div id="ContractInfoPanel" class="predicate_block" style="display: none">
        <!--Договор - основание-->
        <div class="label"><%: FieldLabels[ContractInfo] %></div>
        <v4control:TextBox ID="ContractInfo" runat="server" IsReadonly="true" Width="100%" Visible="false" NextControl="Contract"></v4control:TextBox>
    </div>

    <div class="predicate_block" id="divEnclosure">
        <!--Приложение-->
        <div class="label"><%: FieldLabels[Enclosure] %></div>
        <v4dbselect:DBSDocument ID="Enclosure" runat="server" Width="370px" IsMultiSelect="False" NextControl="ApplicationForPurchasing"
                                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Enclosure_Changed" OnBeforeSearch="Document_BeforeSearch">
        </v4dbselect:DBSDocument>
    </div>

    <div class="predicate_block" id="divApplicationForPurchasing">
        <!--Заявка на покупку-->
        <div class="label"><%: FieldLabels[ApplicationForPurchasing] %></div>
        <v4dbselect:DBSDocument ID="ApplicationForPurchasing" runat="server" Width="370px" IsMultiSelect="False" NextControl="LetterOfCredit"
                                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch">
        </v4dbselect:DBSDocument>
    </div>

    <div class="predicate_block" id="divLetterOfCredit">
        <!--Аккредитив-->
        <div class="label"><%: FieldLabels[LetterOfCredit] %></div>
        <v4dbselect:DBSDocument ID="LetterOfCredit" runat="server" Width="370px" NextControl="BillOfLading"
                                AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch">
        </v4dbselect:DBSDocument>
    </div>

    <div class="predicate_block" id="divBillOfLading">
        <!--Коносамент-->
        <div class="label"><%: FieldLabels[BillOfLading] %></div>
        <v4dbselect:DBSDocument ID="BillOfLading" runat="server" Width="370px" IsMultiSelect="False" NextControl="Invoice"
                                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch">
        </v4dbselect:DBSDocument>
    </div>

    <div class="predicate_block" id="divInvoice">
        <!--Счет, инвойс, проформа-->
        <div class="label"><%: FieldLabels[Invoice] %></div>
        <v4dbselect:DBSDocument ID="Invoice" runat="server" Width="370px" IsMultiSelect="True" IsRemove="True" ConfirmRemove="True" NextControl="PaymentDocuments"
                                OnChanged="Invoice_ValueChanged" OnDeleted="Invoice_ValueDeleted" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch">
        </v4dbselect:DBSDocument>
    </div>

    <div class="predicate_block" id="divPaymentDocuments">
        <!--Платежные документы-->
        <div class="label"><%: FieldLabels[PaymentDocuments] %></div>
        <v4dbselect:DBSDocument ID="PaymentDocuments" runat="server" Width="370px" IsMultiSelect="True" IsRemove="True" ConfirmRemove="True"
                                OnChanged="PaymentDocuments_ValueChanged" OnDeleted="PaymentDocuments_ValueDeleted" CSSClass="aligned_control" OnBeforeSearch="Document_BeforeSearch">
        </v4dbselect:DBSDocument>
    </div>
</div>
<!--Транспорт-->
<div class="non_correctable_block" id="divTransport">
    <span>
        <b><%: SectionTitles.Transport %></b>
    </span>
    <div id="TransportTitle" class="headerTitle"></div>
</div>
<div>
    <div class="predicate_block" id="divPowerOfAttorney" runat="server">
        <!--Доверенность-->
        <div class="label"><%: FieldLabels[PowerOfAttorney] %></div>
        <v4control:TextBox ID="PowerOfAttorney" runat="server" NextControl="Driver" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
    </div>

    <div class="predicate_block" id="divDriver" runat="server">
        <!--Водитель-->
        <div class="label"><%: FieldLabels[Driver] %></div>
        <v4control:TextBox ID="Driver" runat="server" NextControl="Car" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
    </div>

    <div class="predicate_block" id="divCar" runat="server">
        <!--Автомобиль-->
        <div class="label"><%: FieldLabels[Car] %></div>
        <v4control:TextBox ID="Car" runat="server" NextControl="CarNumber" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
    </div>

    <div class="predicate_block" id="divCarNumber" runat="server">
        <!--Номер автомобиля-->
        <div class="label"><%: FieldLabels[CarNumber] %></div>
        <v4control:TextBox ID="CarNumber" runat="server" NextControl="TrailerNumber" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
    </div>

    <div class="predicate_block" id="divTrailerNumber" runat="server">
        <!--Номер прицепа-->
        <div class="label"><%: FieldLabels[TrailerNumber] %></div>
        <v4control:TextBox ID="TrailerNumber" runat="server" NextControl="CarTtn" Width="220px" CSSClass="aligned_control"></v4control:TextBox>
    </div>

    <div class="predicate_block" id="divCarTtn" runat="server">
        <!--Транспортная накладная-->
        <div class="label"><%: FieldLabels[CarTtn] %></div>
        <v4dbselect:DBSDocument ID="CarTtn" runat="server" Width="350px"
                                IsRemove="True" AutoSetSingleValue="True" CSSClass="aligned_control">
        </v4dbselect:DBSDocument>
    </div>
</div>
</div>
</div>
<!--Товары-->
<div id="tabs-2">
    <div id="MonthResourcePanel" class="predicate_block">
        <div class="label"><%= Resx.GetString("TTN_lblMonthResource") %>:</div>
        <v4control:DatePicker runat="server" ID="efMonthOfResources" MonthYearFormat="True" Width="200px"></v4control:DatePicker>
    </div>

    <div id="ShipperStorePanel" class="predicate_block">
        <div class="label"><%= Resx.GetString("lblShipperStore") %>:</div>
        <v4dbselect:DBSStore ID="DBSShipperStore" runat="server" Width="370px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="DBSPayerStore"
                             AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="ShipperStore_Changed"/>
    </div>

    <div id="PayerStorePanel" class="predicate_block">
        <div class="label"><%= Resx.GetString("lblPayerStore") %>:</div>
        <v4dbselect:DBSStore ID="DBSPayerStore" runat="server" Width="370px" CLID="18" IsAlwaysAdvancedSearch="True" NextControl="DBSShipperStore"
                             AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="PayerStore_Changed"/>
    </div>

    <div id="MrisButtonPanel" class="predicate_block" runat="server">
        <v4control:Button ID="addResource" IsComposite="True" runat="server"></v4control:Button>
        <v4control:Button ID="selectVagon" IsComposite="True" runat="server"></v4control:Button>
    </div>

    <div id="RenderProductSelectPanel" class="predicate_block" runat="server">
        <v4control:Radio ID="rbProductSelect" runat="server" Name="Radio" IsRow="True" OnChanged="ProductSelect_Changed"/>
    </div>

    <div id="NeedSaveDocumentPanelMris" class="predicate_block" runat="server">
        <v4control:Div runat="server" ID="divNeedSaveDocumentPanelMris" CSSClass="warning"></v4control:Div>
    </div>

    <div id="divResourceGrid" runat="server">
        <div class="spacer"></div>
        <csg:Grid runat="server" ID="GridResource" MarginBottom="500" ExistServiceColumn="True"/>
    </div>
</div>
<!--Услуги-->
<div id="tabs-3">
    <v4control:Button ID="addFactUsl" runat="server"></v4control:Button>
    <div id="NeedSaveDocumentPanelUsl" class="predicate_block" runat="server">
        <v4control:Div runat="server" ID="divNeedSaveDocumentPanelUsl" CSSClass="warning"></v4control:Div>
    </div>
    <div id="divFactUslGrid" runat="server">
        <div class="spacer"></div>
        <csg:Grid runat="server" ID="GridUsl" MarginBottom="500" ExistServiceColumn="True"></csg:Grid>
    </div>
</div>
</div>

<div class="spacer"></div>

<div class="marginA" id="itogTable" runat="server">
    <div id="itog">
        <div class="non_correctable_block">
            <span style="color: #454545">
                <b><%= Resx.GetString("lblItogoInvoice") %>:</b>
            </span>
            <div id="Div1" class="headerTitle"></div>
        </div>
    </div>

    <table class="itogtable" style="width: 100%;">
        <tr>
            <td class="itogtable" style="background-color: #ededed">&nbsp;</td>
            <td class="itogtable" style="background-color: #ededed" align="center"><%= Resx.GetString("lblSummaOutNDS") %></td>
            <td class="itogtable" style="background-color: #ededed" align="center"><%= Resx.GetString("lblNDS") %></td>
            <td class="itogtable" style="background-color: #ededed" align="center"><%= Resx.GetString("lTotal") %></td>
        </tr>
        <tr id="trProduct" runat="server">
            <td class="itogtable" style="background-color: #ededed"><%= Resx.GetString("lblProducts") %>:</td>
            <td class="itogtable">
                <span id="mrisSum"></span>
            </td>
            <td class="itogtable">
                <span id="mrisSumNDS"></span>
            </td>
            <td class="itogtable">
                <span id="mrisSumAll"></span>
            </td>
        </tr>
        <tr id="trServices" runat="server">
            <td class="itogtable" style="background-color: #ededed"><%= Resx.GetString("lblServices") %>:</td>
            <td class="itogtable">
                <span id="factUslSum"></span>
            </td>
            <td class="itogtable">
                <span id="factUslSumNDS"></span>
            </td>
            <td class="itogtable">
                <span id="factUslSumAll"></span>
            </td>
        </tr>
        <tr id="trTotal" runat="server">
            <td class="itogtable" style="background-color: #ededed"><%= Resx.GetString("lTotal") %>:</td>
            <td class="itogtable">
                <span id="AllSum"></span>
            </td>
            <td class="itogtable">
                <span id="AllNDSSum"></span>
            </td>
            <td class="itogtable">
                <span id="AllAllSum"></span>
            </td>
        </tr>
    </table>
</div>
</div>

<div class="spacer"></div>
<div class="marginA">
    <% StartRenderVariablePart(Response.Output, 200, 600, 0, 4, false); %>
    <% EndRenderVariablePart(Response.Output); %>
</div>

<% if (!Doc.IsNew && !Doc.Unavailable && !Doc.DataUnavailable)
   { %>
    <div class="spacer"></div>
    <div class="marginA">
        <csld:LinkedDoc runat="server" ID="LinkedDocs" DefaultLinkedDocType="2070" CurrentDocType="2145"/>
    </div>
<% } %>

<div id="divResourceAdd" style="display: none; padding: 2px 0 0 0;">
    <div class="v4DivTable" id="divProgressBar" style="display: none; height: 100%; position: absolute; width: 100%;">
        <div class="v4DivTableRow">
            <div class="v4DivTableCell">
                <img src="/styles/ProgressBar.gif" alt="wait"/><br/><%= Resx.GetString("lblWait") %>...
            </div>
        </div>
    </div>
    <div id="divFrame">
        <iframe id="ifrMris" style="width: 100%;" onload="setIframeHeight();"></iframe>
    </div>
</div>

<div id="divServiceAdd" style="display: none; padding: 2px 0 0 0;">
    <div class="v4DivTable" id="divServiceProgressBar" style="display: none; height: 100%; padding-top: 150px; position: absolute; width: 100%;">
        <div class="v4DivTableRow">
            <div class="v4DivTableCell" style="height: 100%;">
                <img src="/styles/ProgressBar.gif" alt="wait"/><br/><%= Resx.GetString("lblWait") %>...
            </div>
        </div>
    </div>
    <div id="divFactUslFrame">
        <iframe id="ifrFactUsl" style="width: 100%;" onload="setIframeServiceHeight();"></iframe>
    </div>
</div>

<div id="divSelectVagon" style="display: none; padding: 2px 0 0 0;">
    <div class="v4DivTable" id="divVagonProgressBar" style="display: none; height: 100%; position: absolute; width: 100%;">
        <div class="v4DivTableRow">
            <div class="v4DivTableCell">
                <img src="/styles/ProgressBar.gif" alt="wait"/><br/><%= Resx.GetString("lblWait") %>...
            </div>
        </div>
    </div>
    <div id="divVagonFrame">
        <iframe id="ifrVagon" style="width: 100%;" onload="setIframeVagonHeight();"></iframe>
    </div>
</div>

<div id="divSelectorNaborov" style="display: none; text-align: center;">
    <br/><br/>
    <v4control:Div runat="server" ID="lnkSkladFrom"/>
    <br/><br/>
    <v4control:Div runat="server" ID="lnkSkladTo"/>
</div>

<div id="divDistribDoc" style="display: none; padding: 2px 0 0 0;">
    <div class="v4DivTable" id="divDistribProgressBar" style="display: none; height: 100%; position: absolute; width: 100%;">
        <div class="v4DivTableRow">
            <div class="v4DivTableCell">
                <img src="/styles/ProgressBar.gif"/><br/><%= Resx.GetString("lblWait") %>...
            </div>
        </div>
    </div>
    <div id="divDistribFrame">
        <iframe id="IfrDistrib" style="width: 100%;" onload="setIframeDistribHeight();"></iframe>
    </div>
</div>

<div id="divAddress" style="display: none;">

    <table cellspacing="0" cellpadding="5" border="0">
        <tr>
            <td><v4control:TextBox ID="tbFaceName" runat="server" IsReadOnly="True"></v4control:TextBox>:</td>
            <td>
                <v4dbselect:DBSPerson ID="DBSGOPerson" runat="server" Width="200px"></v4dbselect:DBSPerson>
            </td>
        </tr>
        <tr>
            <td><%= Resx.GetString("TTN_lblChooseContact") %>:</td>
            <td>
                <v4dbselect:DBSPersonContact ID="DBSContact" runat="server" IsAlwaysAdvancedSearch="True" NextControl="tbFace"
                                             Width="370px" CSSClass="aligned_control">
                </v4dbselect:DBSPersonContact>
            </td>
        </tr>
    </table>
</div>

<div id="divSigner" style="display: none;">
    <table>
        <tr>
            <td>
                <v4control:TextBox ID="tbPerson" runat="server" IsReadonly="true" NextControl="tbPerson"/>
            </td>
            <td>
                <v4dbselect:DBSPerson ID="DBSPerson" runat="server" Width="180px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="DBSPosition"
                                      IsCaller="True" CallerType="Person" AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Person_Changed">
                </v4dbselect:DBSPerson>
            </td>
        </tr>
        <tr>
            <td>
                <v4control:TextBox ID="tbPosition" runat="server" IsReadonly="true" NextControl="tbPerson"/>
            </td>
            <td>
                <v4dbselect:DBSPersonSigner ID="DBSPosition" runat="server" Width="180px" CLID="4" IsAlwaysAdvancedSearch="True" NextControl="DBSPerson"
                                            AutoSetSingleValue="True" CSSClass="aligned_control" OnChanged="Position_Changed">
                </v4dbselect:DBSPersonSigner>
            </td>
        </tr>
    </table>
</div>

</body>
</html>