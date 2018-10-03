var resources_pageId, resources_docId, resources_resourceId, resources_form;
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
            create: Nakladnaya.accordionCreate
        });
        $("#accordion a").click(function (e) {e.stopPropagation();});
        $("#tabs").tabs();
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

            if (required.length)
                required[0].focus();
            else {
                var to_focus = $("input:text", ui.newPanel).filter(function () {
                    return !this.value;
                }).first();

                if (to_focus.length) to_focus[0].focus();
            }
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
    * Запрашивает у пользователя подтверждение установки значений из корректируемого документа
    */
    , setCorrectableDocument: function () {
        v4_confirmMsgBox(Nakladnaya.StrResources.ReplaceValues, Nakladnaya.StrResources.Title
            , Nakladnaya.StrResources.BtnYes, Nakladnaya.StrResources.BtnNo
            , function () { cmd('cmd', 'SetCorrectableDocument', 'value', true); }
            , function () { cmd('cmd', 'SetCorrectableDocument', 'value', false); });
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

resources_RecordsAdd.form = null;
var resources_ifrIsLoaded = false;
function resources_RecordsAdd(form, titleForm, pageId, docId, resourceId) {
    if (titleForm && titleForm != "") title = titleForm;

    if (pageId && pageId != "" && docId && docId != "" && resourceId && resourceId != "") {
        resources_pageId = pageId;
        resources_resourceId = resourceId;
        resources_docId = docId;
        resources_form = form;
    } else {
        resources_pageId = "";
        resources_docId = "";
        resources_resourceId = 0;
        resources_form = "";
    }

    var idContainer = "divResourceAdd";
    if (null == resources_RecordsAdd.form) {
        var width = 770;
        var height = 580;
        var onOpen = function () {
            if (!resources_ifrIsLoaded) {
                if (!resources_form || resources_form == "") {
                    alert(nakladnaya_clientLocalization.errOpenEditForm);
                }
                $("#ifrMris").attr('src', resources_form + ".aspx?idpp=" + resources_pageId + "&idDoc=" + resources_docId + "&id=" + resources_resourceId);
                resources_ifrIsLoaded = true;
            }
        };
        var onClose = function () { resources_Records_Close(); };
        var buttons = null;

        resources_RecordsAdd.form = v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons);
    }
    
    $("#divResourceAdd").dialog("option", "title", title);
    resources_RecordsAdd.form.dialog("open");
    frame_progressBarShow(1);

}

function resources_Records_Close(ifrIdp) {

    if (null == resources_RecordsAdd.form) return;
    if (ifrIdp == null) {
        var resource_idp = $("#ifrMris")[0].contentWindow.idp;
        v4_closeIFrameSrc("ifrMris", resource_idp);
    }

    resources_RecordsAdd.form.dialog("close");
    resources_RecordsAdd.form = null;
    resources_ifrIsLoaded = false;
    //$("#ifrMris").remove();

}

function resources_Records_Save() {
    cmd("cmd", "RefreshResource");
}

function vagon_Records_Save() {
    cmd("cmd", "RefreshResourceByVagon");
}

function mris_edit(id) {
    resources_RecordsAdd("MrisForm", nakladnaya_clientLocalization.mris_title, idp, v4_ItemId, id);
}

function mris_copy(id) {
    cmd("cmd", "MrisCopy", "MrisId", id);
}

function mris_delete(id) {
    cmd("cmd", "MrisDelete", "MrisId", id);
}

///
function factusl_edit(id) {
    resources_RecordsAdd("FactUslForm", nakladnaya_clientLocalization.factusl_title, idp, v4_ItemId, id);
}

function factusl_copy(id) {
    cmd("cmd", "FactUslCopy", "FactUslId", id);
}

function factusl_delete(id) {
    cmd("cmd", "FactUslDelete", "FactUslId", id);
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

$(window).resize(function () {
    setIframeHeight();
    setIframeVagonHeight();
});

function setIframeHeight() {
    $('#ifrMris').height($('#divResourceAdd').height());
};

function Select_Vagon(control, callbackKey, result, isMultiReturn) {
    if (result.length == 0) return;
    cmd("cmd", "OnSelectedVagon", "ResultId", result[0].value);
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
        var width = 570;
        var height = 280;
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

function frameVagon_progressBarShow(show) {
    if (show == 1)
        $("#divVagonProgressBar").show();
    else
        $("#divVagonProgressBar").hide();
}


//после подготовки документа инициализируется объект-контроллер
$(Nakladnaya.init);


