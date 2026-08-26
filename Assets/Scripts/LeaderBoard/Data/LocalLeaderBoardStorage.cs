
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Lokal Liderlik tablosu okuma/yazma işlemleri için oluşturulmuş sınıf.
/// </summary>
[Serializable]
public class LocalLeaderBoardStorage
{
    /// <summary>
    /// Platformlar arası geçerli, yazılabilir dosya yolu verisi.
    /// </summary>
    private string leaderBoard_FilePath;
    /// <summary>
    /// Dosya yolunu tutan <see cref="leaderBoard_FilePath"/> verisini getiren, boşsa dolduran alan.
    /// Lazy initialization sorunundan dolayı <see cref="leaderBoard_FilePath"/> verisinin ilk değeri
    /// tanımlanırken değil ilk kez çağırıldığında hesaplanıyor.
    /// </summary>
    private string FilePath => leaderBoard_FilePath ??= Path.Combine(Application.persistentDataPath, "leaderboard.json");

    /// <summary>
    /// Yerel Liderlik Tablosunu dosyadan yüklemek için oluşturulmuş metot.
    /// Dosya operasyonları <see langword="try"/> <see langword="catch"/> koruması ile kontrol ediliyor,
    /// bir hata meydana gelmesi durumunda konsola hata mesajı yazdırılıyor.
    /// </summary>
    /// <remarks>
    /// Eğer yerel dosyalarda mevcut bir Liderlik Tablosu verisi bulunmuyorsa
    /// yeni ve boş bir tablo oluşturulur.
    /// </remarks>
    /// <returns>
    /// Dosyalardan elde edilen <see cref="LeaderBoard_LocalWrapper"/> verisi,
    /// yoksa boş olarak yeniden oluşturularak, döndürülür.
    /// </returns>
    public LeaderBoard_LocalWrapper Load_LeaderBoard()
    {
        // Boş yerel Liderlik Tablosu verisi oluşturuluyor.
        LeaderBoard_LocalWrapper localLeaderBoard = new();
        LeaderBoard_Local[] localLeaderBoardData = new LeaderBoard_Local[0];
        localLeaderBoard.data = localLeaderBoardData;

        // Dosya konumunun doğruluğu kontrol ediliyor.
        if (!File.Exists(FilePath))
        {
            // Dosya yolu geçersizse boş tablo döndürülüyor.
            Debug.LogWarning("File does not exists or there is no record yet in path:" + FilePath);

            return localLeaderBoard;
        }

        string json;

        try
        {
            // Dosya yolundaki dosya okunuyor ve içeriği string (json) verisine yazılıyor.
            json = File.ReadAllText(FilePath);
        }
        catch (Exception e)
        {
            // Okuma sırasında bir hata meydana gelmesi sonucunda konsoldan bildiriliyor ve
            // boş tablo döndürülüyor.
            Debug.LogError("Error occurs when try to reading Local Leader Board file: " + e.Message);
            return localLeaderBoard;
        }

        // Dosyalardan okunan metnin boş veya boşluk olup olmadığı kontrol ediliyor.
        if (string.IsNullOrWhiteSpace(json))
        {
            // Boş veya boşluk olduğu tespit edilen metin konsoldan bildiriliyor ve
            // boş tablo döndürülüyor.
            Debug.LogError("Local Leader Board string is null or white space");
            return localLeaderBoard;
        }

        // Json dosyası parçalanıyor.
        localLeaderBoard = JsonUtility.FromJson<LeaderBoard_LocalWrapper>(json);

        // Parçalanma işleminden sonra elde edilen verinin geçerliliği kontrol ediliyor.
        if (localLeaderBoard == null || localLeaderBoard.data == null)
        {
            // Veri boş veya eksik doldurulmuşsa konsoldan bildiriliyor.
            Debug.LogError("Failed to parse JSON");

            // Json parçalama işlemi uygulanmış hatalı veri sıfırlanıyor ve
            // boş tablo döndürülüyor.
            localLeaderBoard = new();
            localLeaderBoardData = new LeaderBoard_Local[0];
            localLeaderBoard.data = localLeaderBoardData;

            return localLeaderBoard;
        }

        // Elde edilen veri döndürülüyor.
        return localLeaderBoard;
    }

    /// <summary>
    /// Yerel Liderlik Tablosunu dosyaya kaydetmek için oluşturulmuş metot.
    /// Dosya operasyonları <see langword="try"/> <see langword="catch"/> koruması ile kontrol ediliyor,
    /// bir hata meydana gelmesi durumunda konsola hata mesajı yazdırılıyor. 
    /// </summary>
    /// <param name="board">
    /// Dosyaya kaydedilmek istenen <see cref="LeaderBoard_LocalWrapper"/> verisi
    /// </param>
    /// <returns>
    /// Dosya kaydının başarılı olup olmaması <see langword="bool"/> verisi olarak döndürülür.
    /// </returns>
    public bool Save_LocalLeaderBoard(LeaderBoard_LocalWrapper board)
    {
        // Tablo verisi json formatına dönüştürülüyor.
        string json = JsonUtility.ToJson(board, true);

        try
        {
            // Json formatındaki veri dosyaya yazılıyor.
            File.WriteAllText(FilePath, json);
        }
        catch (Exception e)
        {
            // Yazma sırasında bir hata meydana gelmesi sonucunda konsoldan bildiriliyor ve
            // yazma işleminin başarısız olduğu bilgisi döndürülüyor.
            Debug.LogError("Error occurs when try to writing Local Leader Board file: " + e.Message);
            return false;
        }    

        // Yazma işleminin başarılı bir şekilde tamamlandığı bilgisi döndürülüyor.
        return true;    
    }

    /// <summary>
    /// Yerel Liderlik Tablosu dosyasına veri eklemek için oluşturulmuş metot.
    /// <see cref="LeaderBoard_LocalWrapper"/> içerisindeki <see cref="LeaderBoard_Local"/> dizisi
    /// metot tarafından güncellenir.
    /// </summary>
    /// <param name="nickname">
    /// Dosyaya eklenmek istenen <see cref="LeaderBoard_Local.nickname"/> verisi.
    /// </param>
    /// <param name="score">
    /// Dosyaya eklenmek istenen <see cref="LeaderBoard_Local.score"/> verisi.
    /// </param>
    /// <param name="time">
    /// Dosyaya eklenmek istenen <see cref="LeaderBoard_Local.time"/> verisi.
    /// </param>
    /// <returns>
    /// Eklenmek istenilen verilerin başarılı bir şekilde dosyaya eklendiğini
    /// <see cref="Save_LocalLeaderBoard"/> metodu sonucunu doğrudan
    /// <see langword="bool"/> verisi olarak döndürülür.
    /// </returns>
    public bool AddEntryTo_LocalLeaderBoard(string nickname, int score, float time)
    {
        // Tablo verisi dosyadan okunarak yerel veriye yüklenir.
        LeaderBoard_LocalWrapper board = Load_LeaderBoard();

        // Eklenecek yeni verinin diğer verilerle birlikte saklanacağı liste.
        List<LeaderBoard_Local> dataList = new();

        // Başarısız yükleme sonucu oluşabilecek boş tablo kontrol ediliyor.
        if (board.data != null)
        {
            // Boş olmadığı doğrulanan tablo verileri listeye eklenir.
            foreach (LeaderBoard_Local boardData in board.data)
            {
                dataList.Add(boardData);
            }
        }

        // Dosyaya eklenmek istenen veriler de listeye eklenir.
        LeaderBoard_Local data = new()
        {
            nickname = nickname,
            score = score,
            time = time
        };

        dataList.Add(data);

        // Liste tablo verilerine aktarılır.
        board.data = dataList.ToArray();

        // Oluşan yeni tablo verisi dosyaya yazılır ve
        // yazma sonucuna göre başarı bayrağı döndürülür.
        return Save_LocalLeaderBoard(board);
    }

    /// <summary>
    /// Dosyada bulunan tüm yerel Liderlik Tablosu verilerini
    /// temizlemek için oluşturulmuş metot.
    /// </summary>
    /// <returns>
    /// Dosya silme işleminin başarı değeri döndürülür.
    /// </returns>
    public bool Clear_LocalLeaderBoard()
    {
        try
        {
            // Dosya içerisindeki veriler ile birlikte siliniyor.
            File.Delete(FilePath);            
        }
        catch (Exception e)
        {
            // Meydana gelebilecek olan hatalar konsol ile bildiriliyor.
            Debug.LogError("Error occurs when try to deleting Local Leader Board file: " + e.Message);

            return false;
        }

        return true;
    }
}