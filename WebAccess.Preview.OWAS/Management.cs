using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Web;
using System.IO;
using System.Security.Cryptography;

namespace WebAccess.Extensions.Preview.OWAS {
  internal static class KeyManagement {
    /// <summary>Коллекция ключей</summary>
    private static ConcurrentDictionary<Guid, AccessKey> keys = new ConcurrentDictionary<Guid, AccessKey>();
    /// <summary>Время жизни ключа, в минутах</summary>
    private static Int16 keyLifeTime = 5;

    /// <summary></summary>
    /// <param name="filePath">Путь к файлу</param>
    /// <returns></returns>
    static internal string AddFilePath(string filePath, string fileName) {
      var keyGuid = Guid.NewGuid();
      fileName = fileName + Path.GetExtension(filePath);
      keys.TryAdd(keyGuid, new AccessKey { filePath = filePath, created = DateTime.Now, fileName = fileName });
      return keyGuid.ToString();
    }

    /// <summary>Получение пути к файлу по ИДентификатору</summary>
    /// <param name="keyGuid">идентификатор ключа</param>
    /// <returns></returns>
    static internal string GetFilePath(Guid keyGuid) {
      AccessKey ak;
      var filePath = string.Empty;
      if (keys.TryGetValue(keyGuid, out ak) && (ak.created.AddMinutes(keyLifeTime) > DateTime.Now)) {
        filePath = ak.filePath;
      }
      return filePath;
    }

    /// <summary>"Красивое" имя файла</summary>
    /// <param name="keyGuid">Клюя доступа</param>
    /// <returns></returns>
    internal static string GetFileName(Guid keyGuid) {
      AccessKey ak;
      var fileName = string.Empty;
      if (keys.TryGetValue(keyGuid, out ak) && (ak.created.AddMinutes(keyLifeTime) > DateTime.Now)) {
        fileName = ak.fileName;
      }
      return fileName;
    }
  }

  internal static class Utility {
    /// <summary>Хеш по файлу</summary>
    /// <param name="fileInfo"></param>
    /// <returns></returns>
    internal static string CreateSHAFileHash(string filename) {
      using (var fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
        fileStream.Position = 0;
        using (SHA256Managed HashTool = new SHA256Managed()) {
          Byte[] EncryptedBytes = HashTool.ComputeHash(fileStream);
          return Convert.ToBase64String(EncryptedBytes);
        }
      }
    }
  }


  /// <summary>Информация</summary>
  internal class AccessKey {
    /// <summary>Путь к файлу</summary>
    public string filePath;
    /// <summary>Наименование документа</summary>
    public string fileName;
    /// <summary>Путь к файлу</summary> 
    public DateTime created;
  }

  public class CheckFileInfoWOPI {
    public string BaseFileName;
    public string BreadcrumbBrandName;
    public string FileUrl;
    public string OwnerId;
    public long Size;
    public string SHA256;
    public string Version;

    public CheckFileInfoWOPI() {
      this.OwnerId = Guid.NewGuid().ToString();
      this.BreadcrumbBrandName = "WebAccess Preview";
    }
  }
}