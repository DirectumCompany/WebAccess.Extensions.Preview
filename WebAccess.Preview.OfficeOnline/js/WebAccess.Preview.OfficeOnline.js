(function () {

  // Нэймспейс клиентской логики расширения
  var OWAS = window.OWAS = {};

  //Список поддерживаемых расширений 
  var extensions = ["DOC", "DOCX", "DOCM", "DOT", "DOTM", "DOTX", "ODT", "XLS", "XLSX", "XLSM", "XLSB", "ODS", "PPT", "ODP", "POT", "POTM", "POTX", "PPS", "PPSM", "PPSX", "PPTM", "PPTX"];

  // Проверка на поддерживаемый формат для просмотра.
  OWAS.IsPreviewSupported = function () {
    return (WA._location == "doc" && ($.inArray(WA.CR.Editor.Extension, extensions) != -1));
  }

  //Создание кнопки предпросмотра документа
  OWAS.CreateButton = function () {
    var button = {};
    button.name = 'BUTTON_OWAS_PREVIEW';
    button.text = 'Предпросмотр OfficeOnline';
    button.icon = 'Preview.png';
    button = WA.CR.toolBar.buttons.create('TOOLBAR_SEND_GROUP', button);
    button.bind('click', OWAS.Preview);
    button.setIcon('ToolbarAndTab/Preview.png');
  }

  //Действие предпросмотра документа
  OWAS.Preview = function () {
    var OpenInOWAS = function (key) {
      var officeURL = "http://view.officeapps.live.com/op/view.aspx?src=";
      var fileSRC = encodeURIComponent(window.location.protocol + "//" + window.location.host + "/_vti_bin/ExportFile.ashx?key=" + key);
      console.log(fileSRC);
      window.open(officeURL + fileSRC);
    }

    if (WA.CR.Versions.length > 1) {
      // Создание новой формы
      var template = new WA.components.forms.FormBuilder("VERSIONS_DLG");
      var versionsSelect = {};
      $.each(WA.CR.Versions, function (i, item) {
        versionsSelect[item.Number] = item.Note;
      });
      // Добавление контрола типа: Select
      template.addSelect("VERSION", versionsSelect);
      // Создание конфигурации диалога
      var dialog = {};
      dialog.height = 170;
      dialog.width = 400;
      dialog.title = "Выберите версию для предпросмотра";
      dialog.okText = L("OK");
      dialog.cancelText = L("CANCEL");
      dialog.text = template.render();
      dialog.ok = function () {
        var version = template.getValue("VERSION");
        WA.services.call("OfficePreview.asmx/ExportForPreview", { id: WA.CR.ID, version: version }).done(OpenInOWAS);
      }
      // Отображение диалога с выбором версий документа
      ConfirmDialog(dialog);
    } else {
      // Вызов сервиса с передачей ИД и версии документа
      WA.services.call("OfficePreview.asmx/ExportForPreview", { id: WA.CR.ID, version: WA.CR.Versions[0].Number }).done(OpenInOWAS);
    }
  }
})();

WebAccess.ready(function () {
  // Регистрация добавления кнопки предпросмотра для поддерживаемых документов.
  if (OWAS.IsPreviewSupported()) OWAS.CreateButton();
});
