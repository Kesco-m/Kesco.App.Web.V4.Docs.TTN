'use strict'

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
    }
}

//после подготовки документа инициализируется объект-контроллер
$(Nakladnaya.init);