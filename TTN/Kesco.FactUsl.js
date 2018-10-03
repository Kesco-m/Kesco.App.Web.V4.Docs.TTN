
function dialogRecalc(func, name, value, ndx) {
    cmd("cmd", func, "name", name, "value", value, "ndx", ndx);
}

if (parent != null) parent.frame_progressBarShow(0);
