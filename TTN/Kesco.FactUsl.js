﻿
function ShowControl(ctrl, readonly) {
    if (readonly == "True") {
        $("#" + ctrl).hide();
    } else {
        $("#" + ctrl).show();
    }
}

function dialogRecalc(func, name, value, ndx) {
    cmd("cmd", func, "name", name, "value", value, "ndx", ndx);
}

if (parent != null) parent.frameService_progressBarShow(0);

$(document).ready(function () {
    window.v4_save = function () {
        $("#btnSave").focus();
         cmd('cmd', 'SaveData');
    };

});