
if (parent != null) parent.frameDistrib_progressBarShow(0);

function filterGrid(type) {
    cmd('cmd', 'SetFilterDocs', 'Type', type);
}

function n_ok() {
    var el = event.srcElement;
    if (el.getAttribute("type") == 'number' && el.getAttribute("ok") != null && el.oldVal != el.innerText) eval(el.getAttribute("ok"));
}

function SetNaborKol() {
    var el = event.srcElement;
    cmd('cmd', 'SetNaborKol', 'dvNabor', el.getAttribute("dvNabor"), 'el', el.innerText);
}

function SetNaborKol2() {
    var el = event.srcElement;
    cmd('cmd', 'SetNaborKol2', 'dvDoc', el.getAttribute("dvDoc"), 'dvNabor', el.getAttribute("dvNabor"), 'el', el.innerText);
}

function n_keydown() {
    var el = event.srcElement;
    if (el.getAttribute("type") != 'number') return;
    var k = event.keyCode;
    if (k == 13 && el.getAttribute("ok") != null && el.oldVal != el.innerText) {
        eval(el.getAttribute("ok"));
    }
    if (!(k >= 48 && k <= 57 || k >= 96 && k <= 105 || k == 8 || k == 37 || k == 39 || k == 46 || k == 110 || k == 190 || k == 191)) {
        event.preventDefault();
        event.returnValue = false;
    }
    if ((k == 110 || k == 190 || k == 191)) {
        event.preventDefault();
        event.returnValue = false;
        var sc = el.getAttribute("scale") != undefined ? el.getAttribute("scale") : window.defaultScale;
        if (sc > 0 && el.innerText.indexOf(',') == -1)
            document.selection.createRange().text = ',';
    }
}

function n_focus() {
    var el = event.srcElement;
    if (el.getAttribute("type") == 'number')
        el.oldVal = el.innerText;
}