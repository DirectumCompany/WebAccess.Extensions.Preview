using NpoComputer.WebAccess;
using NpoComputer.WebAccess.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Hosting;
using System.Web.Services;

namespace WebAccess.Extensions.Preview.OWAS {
  
  /// <summary>Сервис для выдачи и загрузки файлов</summary>
  /// http://club.directum.ru/post/Code-Snippets-predprosmotr-ofisnykh-formatov-v-OfficeWebApp-i-Office-Online.aspx
  [WebService(Namespace = "http://npo-comp.ru/services/OWAS/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  [System.Web.Script.Services.ScriptService()]
  public class Service : System.Web.Services.WebService {

    /// <summary>Метод выгрузки файла для возможности загрузки сервисом Dropbox</summary>
    /// <param name="id">ИД документа</param>
    /// <param name="version">версия документа</param>
    /// <returns>ИД для загрузки файла</returns>
    [WebMethod]
    public WebServiceResponse<string> ExportForPreview(int id, int version) {
      // Версия документа на выгрузку
      NpoComputer.WebAccess.API.EDocumentVersion docVersion;

      // Если контекст пользователя не задан, не продолжать работу
      if (WAAPIEntry.Context == null) return WebServiceResponse<string>.Fail("Контекст пользователя не найден. Сначала нужно войти в веб-доступ.");
      
      // Получение пути ко временной папке текущего пользователя
      string userDirectory = WAAPIEntry.Context.TempDirectory;
      if (!Path.IsPathRooted(userDirectory)) userDirectory = HostingEnvironment.MapPath(userDirectory);

      try {
        var document = WAAPIEntry.Context.EDocuments.GetDocumentByID(id);
        if (document == null) throw new Exception(string.Format("Документ (ИД={0}) не найден или у Вас нет прав на него.", id));

        // Получение версии документа
        if (version > 0) docVersion = document.get_Versions(version);
        else docVersion = document.LastActualVersion;

        if (docVersion == null) throw new Exception(string.Format("Документ (ИД={0}) не содержит версии с номером {1}.", id, version));

        // Формирование имени для выгрузки
        var fileName = String.Format("{0}_v{2}.{1}", id, document.Editor.Extension, docVersion.Number);
        var filePath = Path.Combine(userDirectory, fileName);

        docVersion.ExportToFile(NpoComputer.WebAccess.API.EDocument.ExportMode.Read, filePath);
        return WebServiceResponse<string>.OK(KeyManagement.AddFilePath(filePath, document.Name));
      } catch (Exception ex) {
        Log.LogException(ex);
        return WebServiceResponse<string>.Fail(ex.Message);
      }
    }
  }

}
