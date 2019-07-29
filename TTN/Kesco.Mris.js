
function ShowControl(ctrl, readonly) {
    if (readonly == "True") {
        $("#" + ctrl).hide();
    } else {
        $("#" + ctrl).show();
    }
}

function dialogRecalc(func, name, value, ndx) {
    cmdasync("cmd", func, "name", name, "value", value, "ndx", ndx);
}

if (parent != null) parent.frame_progressBarShow(0);

$(document).ready(function() {
    window.v4_save = function() {
        $("#btnSave").focus();
        cmdasync("cmd", "SaveMrisAndClose");
    };

    setTimeout(function () {
        $('#efResource_0').focus();
        $('#efResource_0').select();
    }, 10);
});

// Остатки  ------------------------------------------------------------------------------------------------------------
rest_DialogShow.form = null;

function rest_DialogShow(title) {
    var idContainer = "divRest";
    if (null == rest_DialogShow.form) {
        var width = 310;
        var height = 150;
        var onOpen = null;
        var onClose = null;
        var buttons = null;

        rest_DialogShow.form =
            v4_dialog(idContainer, $("#" + idContainer), title, width, height, onOpen, onClose, buttons);
    }
    $("#divRest").dialog("option", "title", title);
    rest_DialogShow.form.dialog("open");
}