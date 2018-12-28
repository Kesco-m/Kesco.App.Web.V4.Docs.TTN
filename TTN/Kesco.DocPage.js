// -------------------------------------------------------------------------------------------------------
save_dialogShow.form = null;
function save_dialogShow(title) {
    var idContainer = "v4_divSaveConfirm";
    if (null == save_dialogShow.form) {
        var width = 260;
        var height = 150;
        var onOpen = function () { v4_openSaveConfirmForm(); };
        var onClose = function () { v4_closeSaveConfirmForm(); };
        var buttons = [
            {
                id: "btn_Apply_SDS",
                text: "Ok",
                icons: {
                    primary: v4_buttonIcons.Ok
                },
                click: function () { cmd('cmd', 'SaveDocument', 'AfterSaveProcess', $("input[name='SaveConfirm']:checked").val()); }
            }
        ];
        save_dialogShow.form = v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons);
    }

    $("#" + idContainer).dialog("option", "title", title);
    save_dialogShow.form.dialog("open");
}

function v4_closeSaveConfirmForm() {
    if (null != save_dialogShow.form) {
        save_dialogShow.form.dialog("close");
        save_dialogShow.form = null;
    }
}

function v4_openSaveConfirmForm() {
    if (null != save_dialogShow.form) {

    }
}
