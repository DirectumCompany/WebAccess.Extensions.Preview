using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Hosting;
using System.Security.Cryptography;
using System.Web.Script.Serialization;
using System.Text;

namespace WebAccess.Extensions.Preview.OWAS {
  /// <summary>Summary description for OWASPreview</summary>
  public class PreviewHandler : IHttpHandler {

    /// <summary>Хендрер выдачи JSON описания объекта и тела объекта</summary>
    /// <param name="context"></param>
    public void ProcessRequest(HttpContext context) {

      var key = context.Request["key"];
      if (string.IsNullOrEmpty(key)) return;

      Guid keyGuid = new Guid(key);
      string filePath = KeyManagement.GetFilePath(keyGuid);

      if (File.Exists(filePath)) {
        if (context.Request["get"] == null) {
          CheckFileInfoWOPI OWASJSON = new CheckFileInfoWOPI();
          if (!File.Exists(filePath)) return;
          var fileInfo = new FileInfo(filePath);
          var Url = context.Request.Url;
          OWASJSON.FileUrl = string.Format("{0}://{1}{2}?key={3}&get=true", Url.Scheme, Url.Authority, Url.AbsolutePath, HttpUtility.UrlEncode(key));
          OWASJSON.BaseFileName = KeyManagement.GetFileName(keyGuid);
          OWASJSON.Size = fileInfo.Length;
          OWASJSON.SHA256 = Utility.CreateSHAFileHash(filePath);
          OWASJSON.Version = OWASJSON.SHA256;

          var json = new JavaScriptSerializer().Serialize(OWASJSON);
          NpoComputer.WebAccess.Log.LogInfo("json: {0}", json);
          context.Response.ContentType = "application/json";
          context.Response.Write(json);
        } else {
          //try {
          context.Response.ClearContent();
          context.Response.ContentType = "text/octet-stream";
          context.Response.AddHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(filePath));
          context.Response.TransmitFile(filePath);
          context.Response.End();
          //} finally {
          //  //File.Delete(filePath);
          //}
        }
      }
    }

    public bool IsReusable {
      get {
        return false;
      }
    }
  }


}