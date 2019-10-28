var SetFocusFirstElement = true;
/** Объект - контроллер формы. Содержит реализации методов используемых на странице. */
var Nakladnaya = {
    /**Эти строковые ресурсы передаются в клиентское приложение от Вэб-сервера */
    /*
    StrResources: {
    "BtnSave": "Сохранить",
    "BtnCancel": "Отмена",
    "BtnDelete": "Удалить",
    "BtnYes": "Да",
    "BtnNo": "Нет",
    "Title": "ТТН",
    "GpToPayer": "Заменить все поля раздела Плательщик значениями из раздела Грузополучатель?",
    "GoToShipper": "Заменить все поля раздела Поставщик значениями из раздела Грузоотправитель?",
    "ReplaceValues": "Во все поля формы будут установлены значения из корректируемого документа. Продолжить?"
    },*/

    /** Инициализирует основной элемент-контейнер формы - аккордион */
    init: function () {
        $("#accordion").accordion({
            collapsible: true,
            activate: Nakladnaya.sectionActivate,
            beforeActivate: Nakladnaya.sectionBeforeActivate,
            create: Nakladnaya.accordionCreate,
            heightStyle: "content",
            active: false
        });
        $("#accordion a").click(function (e) { e.stopPropagation(); });
        
        $("#tabs").tabs({
            activate: function (event, ui) {
                var tabId = ui.newPanel[0].id;
                switch (tabId) {
                case "tabs-2":
                    window.v4_insert = function() { cmd('cmd', 'AddResource', 'PageId', idp, 'DocId', v4_ItemId, 'MrisId', 0); };
                    break;
                case "tabs-3":
                    window.v4_insert = function() { cmd('cmd', 'AddFactUsl', 'PageId', idp, 'DocId', v4_ItemId, 'FactUslId', 0); };
                    break;
                default:
                    window.v4_insert = function() {};
                    break;

                }
            }
        });
    }

    /** Обработчик события элемента пользовательского интерфейса jQuery Accordion */
    , accordionCreate: function (event, ui) {
        $("div.headerTitle", ui.header).hide();
    }

    /** Обработчик события элемента пользовательского интерфейса jQuery Accordion */
    , sectionBeforeActivate: function (event, ui) {
        if (ui.oldHeader.length) {
            //collapsed
            var header = $("div.headerTitle", ui.oldHeader);
            if (header.length) {
                header.show();
                cmd('cmd', 'SetAccordionHeader', 'id', header[0].id);
            }
        }

        if (ui.newHeader.length)
            $("div.headerTitle", ui.newHeader).hide();
    }

    /** Обработчик события элемента пользовательского интерфейса jQuery Accordion */
    , sectionActivate: function (event, ui) {
        if (ui.newPanel.length) {
            var required = $("input.v4s_required", ui.newPanel);

            if (SetFocusFirstElement) {
                if (required.length)
                    required[0].focus();
                else {
                    var to_focus = $("input:text", ui.newPanel).first();
                    if (to_focus.length) to_focus[0].focus();
                }
            }
            SetFocusFirstElement = true;
        }
    }

    /**
    * Вызывается при установке или снятии флажка связывания данных между панелями Грузоотправитель-Поставщик и Грузополучатель-Плательщик
    * @param {string} checkbox_selector - селектор элемента флажок
    * @param {string} srv_cmd - команда отправляемая на сервер
    */
    , panelToPanel: function (checkbox_selector, srv_cmd) {
        var state = $(checkbox_selector).prop('checked');

        var fYes = function () { cmd('cmd', srv_cmd, 'value', state); };

        var not_filled = $("input:text", $(checkbox_selector).parents('div:eq(2)')[0]).filter(function () {
            return this.value;
        }).first();

        if (!state || not_filled.length < 1)
            fYes();
        else
            v4_confirmMsgBox(Nakladnaya.StrResources.GoToShipper, Nakladnaya.StrResources.Title,
            Nakladnaya.StrResources.BtnYes, Nakladnaya.StrResources.BtnNo,
            fYes, function () { $(checkbox_selector).prop('checked', false) });
    }

    /**
    * Устанавливает или отключает состояние режима корректировки документа
    * @param {boolean} state - состояние корректировки документа
    */
    , setCorrectableMode: function (state) {
    /*
        if (state) {
            $("#labelCorrectableTtn").removeClass("disabled_label");
            var active = $("#accordion").accordion("option", "active");
            if (active != 6 && active != 7) {
                $("#accordion").accordion("option", "active", false);
                $("#CorrectableFlag > input")[0].focus();
            }
            $("#accordion > div.non_correctable_block").addClass("ui-state-disabled");
        }
        else {
            $("#labelCorrectableTtn").addClass("disabled_label");
            $("#accordion > div.non_correctable_block").removeClass("ui-state-disabled");
        }
        */
    }

}

function tabActivate(n) {
    $("#tabs").tabs({ active: n });
}

function accordionCloseAll() {
    $("#accordion").accordion({ active: false });
}

function accordionActive(n, focus) {
    if (focus == 'false') SetFocusFirstElement = false;
    $("#accordion").accordion({ active: n });
}

$(window).resize(function () {
    setIframeHeight();
    setIframeVagonHeight();
    setIframeDistribHeight();
});

var resources_pageId, resources_docId, resources_resourceId, resources_ue;
// Товары   ------------------------------------------------------------------------------------------------------------
resources_RecordsAdd.form = null;
var resources_ifrIsLoaded = false;
function resources_RecordsAdd(titleForm, pageId, docId, resourceId, ue) {
    if (titleForm && titleForm != "") title = titleForm;

    if (pageId && pageId != "" && docId && docId != "" && resourceId && resourceId != "") {
        resources_pageId = pageId;
        resources_resourceId = resourceId;
        resources_docId = docId;
        resources_ue = ue;
    } else {
        resources_pageId = "";
        resources_docId = "";
        resources_resourceId = 0;
        resources_ue = ue;
    }

    var idContainer = "divResourceAdd";
    var width = 590; var height = 521;

    if (null == resources_RecordsAdd.form) {
        var onOpen = function () {
            if (!resources_ifrIsLoaded) {
                $("#ifrMris").attr('src', "MrisForm.aspx?idpp=" + resources_pageId + "&idDoc=" + resources_docId + "&id=" + resources_resourceId + "&ue=" + resources_ue);
                resources_ifrIsLoaded = true;
            }
        };
        var onClose = function () { resources_Records_Close(null, 0); };
        var buttons = null;

        resources_RecordsAdd.form = v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons);
    }
    
    $("#divResourceAdd").dialog("option", "title", title);
    resources_RecordsAdd.form.dialog("open");
    //  frame_progressBarShow(1);
}

function resources_Records_Close(ifrIdp, addFocus) {

    if (null == resources_RecordsAdd.form) return;
    if (ifrIdp == null) {
        var resource_idp = $("#ifrMris")[0].contentWindow.idp;
        v4_closeIFrameSrc("ifrMris", resource_idp);
    }

    resources_RecordsAdd.form.dialog("close");
    resources_RecordsAdd.form = null;
    resources_ifrIsLoaded = false;

    if (addFocus != "") {
        $('#' + addFocus).focus();
    }
}

var services_pageId, services_docId, resources_servicesId;
// Услуги  ------------------------------------------------------------------------------------------------------------
services_RecordsAdd.form = null;
var services_ifrIsLoaded = false;
function services_RecordsAdd(titleForm, pageId, docId, serviceId, ue) {
    if (titleForm && titleForm != "") title = titleForm;

    if (pageId && pageId != "" && docId && docId != "" && serviceId && serviceId != "") {
        services_pageId = pageId;
        resources_servicesId = serviceId;
        services_docId = docId;
        resources_ue = ue;
    } else {
        services_pageId = "";
        services_docId = "";
        resources_servicesId = 0;
        resources_ue = ue;
    }

    var idContainer = "divServiceAdd";
    var width = 565; var height = 380;

    if (null == services_RecordsAdd.form) {
        var onOpen = function () {
            if (!services_ifrIsLoaded) {
                $("#ifrFactUsl").attr('src', "FactUslForm.aspx?idpp=" + services_pageId + "&idDoc=" + services_docId + "&id=" + resources_servicesId + "&ue=" + resources_ue);
                services_ifrIsLoaded = true;
            }
        };
        var onClose = function () { services_Records_Close(null, 0); };
        var buttons = null;

        services_RecordsAdd.form = v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons);
    }

    $("#divServiceAdd").dialog("option", "title", title);
    services_RecordsAdd.form.dialog("open");
    frameService_progressBarShow(1);
}

function services_Records_Close(ifrIdp, addFocus) {

    if (null == services_RecordsAdd.form) return;
    if (ifrIdp == null) {
        var resource_idp = $("#ifrFactUsl")[0].contentWindow.idp;
        v4_closeIFrameSrc("ifrFactUsl", resource_idp);
    }

    services_RecordsAdd.form.dialog("close");
    services_RecordsAdd.form = null;
    services_ifrIsLoaded = false;


    if (addFocus != "") {
        $('#' + addFocus).focus();
    }
}



// ------------------------------------------------------------------------------------------------------------

function resources_Records_Save(ctrlFocus, reload, close, isnew) {
    cmdasync("cmd", "RefreshResource", "ctrlFocus", ctrlFocus, "ReloadForm", reload, "CloseForm", close, "IsNew", isnew);
}

function factUsl_Records_Save(ctrlFocus, reload, close, isnew) {
    cmdasync("cmd", "RefreshFactUsl", "ctrlFocus", ctrlFocus, "ReloadForm", reload, "CloseForm", close, "IsNew", isnew);
}

function vagon_Records_Save(reload) {
    cmdasync("cmd", "RefreshResourceByVagon", "ReloadForm", reload);
}

function mris_edit(id) {
    resources_RecordsAdd(nakladnaya_clientLocalization.mris_title, idp, v4_ItemId, id);
}

function mris_copy(id) {
    cmdasync("cmd", "MrisCopy", "MrisId", id);
}

function mris_delete(id) {
    cmdasync("cmd", "MrisDelete", "MrisId", id);
}

///
function factusl_edit(id) {
    services_RecordsAdd(nakladnaya_clientLocalization.factusl_title, idp, v4_ItemId, id);
}

function factusl_copy(id) {
    cmdasync("cmd", "FactUslCopy", "FactUslId", id);
}

function factusl_delete(id) {
    cmdasync("cmd", "FactUslDelete", "FactUslId", id);
}

function nakladnaya_setElementFocus(className, elId) {
    if (elId != null && elId.length > 0) {
        setTimeout(function () {
            var obj = gi(elId);
            if (obj) {
                obj.focus();
            }
        }, 100);
    } else
        $("." + className).first().focus();
}

function frame_progressBarShow(show) {
    if (show==1)
        $("#divProgressBar").show();
    else
        $("#divProgressBar").hide();
}

function frameService_progressBarShow(show) {
    if (show == 1)
        $("#divServiceProgressBar").show();
    else
        $("#divServiceProgressBar").hide();
}

function setIframeHeight() {
    $('#ifrMris').height($('#divResourceAdd').height());
};

function setIframeServiceHeight() {
    $('#ifrFactUsl').height($('#divServiceAdd').height());
};

function mris_detail(id) {
    cmdasync("cmd", "MrisDetail", "MrisId", id);
}

var vagon_pageId, vagon_docId, vagon_resultId;
// Выгоны  ------------------------------------------------------------------------------------------------------------
function Select_Vagon(control, callbackKey, result, isMultiReturn) {
    if (result.length == 0) return;
    cmdasync("cmd", "OnSelectedVagon", "ResultId", result[0].value);
}

vagon_RecordsAdd.form = null;
var vagon_ifrIsLoaded = false;
function vagon_RecordsAdd(titleForm, pageId, docId, resultId, sf, st, post, plat) {
    if (titleForm && titleForm != "") title = titleForm;

    if (pageId && pageId != "" && docId && docId != "") {
        vagon_pageId = pageId;
        vagon_docId = docId;
        vagon_resultId = resultId;
    } else {
        vagon_pageId = "";
        vagon_docId = "";
        vagon_resultId = "";
    }

    var idContainer = "divSelectVagon";
    if (null == vagon_RecordsAdd.form) {
        var width = 510;
        var height = 240;
        var onOpen = function () {
            if (!vagon_ifrIsLoaded) {
                $("#ifrVagon").attr('src', "Vagon.aspx?idpp=" + vagon_pageId + "&idDoc=" + vagon_docId + "&idResult=" + vagon_resultId
                 + "&sf=" + sf + "&st=" + st + "&post=" + post + "&plt=" + plat);
                vagon_ifrIsLoaded = true;
            }
        };
        var onClose = function () { vagon_Records_Close(); };
        var buttons = null;

        vagon_RecordsAdd.form = v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons);
    }

    $("#divSelectVagon").dialog("option", "title", title);
    vagon_RecordsAdd.form.dialog("open");
    frameVagon_progressBarShow(1);

}

function vagon_Records_Close(ifrIdp) {

    if (null == vagon_RecordsAdd.form) return;
    if (ifrIdp == null) {
        var _idp = $("#ifrVagon")[0].contentWindow.idp;
        v4_closeIFrameSrc("ifrVagon", _idp);
    }

    vagon_RecordsAdd.form.dialog("close");
    vagon_RecordsAdd.form = null;
    vagon_ifrIsLoaded = false;
}

function setIframeVagonHeight() {
    $('#ifrVagon').height($('#divSelectVagon').height());
};

function frameVagon_progressBarShow(show) {
    if (show == 1)
        $("#divVagonProgressBar").show();
    else
        $("#divVagonProgressBar").hide();
}

var distrib_pageId, distrib_docId, distrib_resourceId, distrib_typeNabor;
// Наборы  ------------------------------------------------------------------------------------------------------------
nabor_DialogShow.form = null;
function nabor_DialogShow(title) {
    var idContainer = "divSelectorNaborov";
    if (null == nabor_DialogShow.form) {
        var width = 510;
        var height = 180;
        var onOpen = null;
        var onClose = null;
        var buttons = null;

        nabor_DialogShow.form = v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons);
    }
    $("#divSelectorNaborov").dialog("option", "title", title);
    nabor_DialogShow.form.dialog("open");
}

distrib_RecordsAdd.form = null;
var distrib_ifrIsLoaded = false;
function distrib_RecordsAdd(typeNabor, titleForm, pageId, docId, resourceId) {
    nabor_DialogShow.form.dialog("close");
    if (titleForm && titleForm != "") title = titleForm;

    if (pageId && pageId != "" && docId && docId != "" && resourceId && resourceId != "") {
        distrib_pageId = pageId;
        distrib_docId = docId;
        distrib_resourceId = resourceId;
        distrib_typeNabor = typeNabor;
    } else {
        distrib_pageId = "";
        distrib_docId = "";
        distrib_resourceId = 0;
        distrib_typeNabor = "";
    }

    var idContainer = "divDistribDoc";
    if (null == distrib_RecordsAdd.form) {
        var width = 770;
        var height = 580;
        var onOpen = function () {
            if (!distrib_ifrIsLoaded) {
                $("#IfrDistrib").attr('src', "DistribDocPage.aspx?idpp=" + distrib_pageId + "&idDoc=" + distrib_docId + "&id=" + distrib_resourceId + "&type=" + distrib_typeNabor);
                distrib_ifrIsLoaded = true;
            }
        };
        var onClose = function () { distrib_Records_Close(); };
        var buttons = null;

        distrib_RecordsAdd.form = v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons);
    }

    $("#divDistribDoc").dialog("option", "title", title);
    distrib_RecordsAdd.form.dialog("open");
    frameDistrib_progressBarShow(1);

}

function distrib_Records_Close(ifrIdp) {

    if (null == distrib_RecordsAdd.form) return;
    if (ifrIdp == null) {
        var resource_idp = $("#IfrDistrib")[0].contentWindow.idp;
        v4_closeIFrameSrc("IfrDistrib", resource_idp);
    }

    distrib_RecordsAdd.form.dialog("close");
    distrib_RecordsAdd.form = null;
    distrib_ifrIsLoaded = false;
}

function distrib_Records_Save() {
    cmdasync("cmd", "DistribSave");
}

function frameDistrib_progressBarShow(show) {
    if (show == 1)
        $("#divDistribProgressBar").show();
    else
        $("#divDistribProgressBar").hide();
}

function setIframeDistribHeight() {
    $('#IfrDistrib').height($('#divDistribDoc').height());
};

// -------------------------------------------------------------------------------------------------------
personcontact_dialogShow.form = null;
function personcontact_dialogShow(type, positionElementId, title, oktext, canceltext) {
    var idContainer = "divAddress";
    if (null == personcontact_dialogShow.form) {
        var width = 560;
        var height = 150;
        var onOpen = function () { v4_openAddressForm(); };
        var onClose = function () { v4_closeAddressForm(); };
        var buttons = [
            {
                id: "btn_Apply" + type,
                text: oktext,
                icons: {
                    primary: v4_buttonIcons.Ok
                },
                click: function () { cmd('cmd', 'SetAddress', 'Type', type); }
            },
            {
                id: "btn_Cancel" + type,
                text: canceltext,
                icons: {
                    primary: v4_buttonIcons.Cancel
                },
                width: 75,
                click: v4_closeAddressForm
            }
        ];
        var dialogPostion = { my: "left top+12", at: "left", of: $("#" + positionElementId) };
        personcontact_dialogShow.form = v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons, dialogPostion);
    }

    $("#divAddress").dialog("option", "title", title);
    personcontact_dialogShow.form.dialog("open");
}

function v4_closeAddressForm() {
    if (null != personcontact_dialogShow.form) {
        personcontact_dialogShow.form.dialog("close");
        personcontact_dialogShow.form = null;
    }
}

function v4_openAddressForm() {
    if (null != personcontact_dialogShow.form) {

    }
}
// -------------------------------------------------------------------------------------------------------
person_dialogShow.form = null;
function person_dialogShow(type, positionElementId, title, oktext, canceltext) {
    var idContainer = "divSigner";
    if (null == person_dialogShow.form) {
        var width = 350;
        var height = 150;
        var onOpen = function () { v4_openPersonForm(); };
        var onClose = function () { v4_closePersonForm(); };
        var buttons = [
            {
                id: "btn_Apply" + type,
                text: oktext,
                icons: {
                    primary: v4_buttonIcons.Ok
                },
                click: function () { cmd('cmd', 'SetPerson', 'Type', type); }
            },
            {
                id: "btn_Cancel" + type,
                text: canceltext,
                icons: {
                    primary: v4_buttonIcons.Cancel
                },
                width: 75,
                click: v4_closePersonForm
            }
        ];
        var dialogPostion = { my: "left top+12", at: "left", of: $("#" + positionElementId) };
        person_dialogShow.form = v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons, dialogPostion);
    }

    $("#divSigner").dialog("option", "title", title);
    person_dialogShow.form.dialog("open");
}

function v4_closePersonForm() {
    if (null != person_dialogShow.form) {
        person_dialogShow.form.dialog("close");
        person_dialogShow.form = null;
    }
}

function v4_openPersonForm() {
    if (null != person_dialogShow.form) {

    }
}

function SetImgDeleteVisible(director, accountant, storeKeeper) {
    if (director == '') {
        $("#divDD").hide();
    } else { $("#divDD").show(); }

    if (accountant == '') {
        $("#divAD").hide();
    } else { $("#divAD").show(); }

    if (storeKeeper == '') {
        $("#divSD").hide();
    } else { $("#divSD").show(); }

}

//после подготовки документа инициализируется объект-контроллер
$(Nakladnaya.init);

$(document).ready(function () {
    SetExpandAccordionByLastControl();
});

function SetExpandAccordionByLastControl() {
//    $(".v4s_btn, .v4s_btnDetail").each(function () {
//        if (this.id == "GpStore_1" || this.id == "PayerStore_1") {
//            $(this).off('keydown');
//            $(this).keydown(function() {
//                if (event.keyCode == 9) AccordionNext();
//            });
//        }
//    });

    $("#Notes_0").off('keydown');
    $("#Notes_0").keydown(function () { if (event.keyCode == 9) AccordionNext(); });

    $("#GoStore").off('keydown');
    $("#GoStore").keydown(function () {
        if (event.keyCode == 9 && document.activeElement.id == "GoStore_1") AccordionNext();
    });

    $("#GpStore").off('keydown');
    $("#GpStore").keydown(function () {
        if (event.keyCode == 9 && document.activeElement.id == "GpStore_1") AccordionNext();
    });

    $("#PayerStore").off('keydown');
    $("#PayerStore").keydown(function () {
        if (event.keyCode == 9 && document.activeElement.id == "PayerStore_1") AccordionNext();
    });

    $("#divStoreKeeperDelete").off('keydown');
    $("#divStoreKeeper").off('keydown');
    if ($("#StoreKeeper").text() == '')
        $("#divStoreKeeper").keydown(function () { if (event.keyCode == 9) AccordionNext(); });
    else
        $("#divStoreKeeperDelete").keydown(function () { if (event.keyCode == 9) AccordionNext(); });

    $("#PaymentDocuments").off('keydown');
    $("#PaymentDocuments").keydown(function () {
        if (event.keyCode == 9 && document.activeElement.id == "PaymentDocuments_1") AccordionNext();
    });

    //$("#tabs1").off('keydown');
    $("#tabs1").keydown(function () {
        if (event.keyCode == 9 && $("#accordion").accordion("option", "active") == 0) $("#accordion").accordion({ active: 0 });
            $("#accordion").accordion({ active: 0 });
    });
}

function AccordionNext() {
    var active = $("#accordion").accordion("option", "active");
    var count = $("#accordion").find(".headerTitle").length;
    if (active < count) {
        $("#accordion").accordion({ active: active + 1 });
    }
}