using System;

/// <summary>
/// Liderlik Tabloları gösterimi için hazırlanmış veri sınıfı.
/// Liderlik Tabloları verilerini sayfa formatında kaydeder ve ilgilir bayrakları tutar.
/// Verilen URL'deki JSON dosyasının formatına uygun olarak tasarlandı.
/// </summary>
[Serializable]
public class LeaderBoardPage
{
    /// <summary>
    /// Sayfa verisinin son sayfa olup olmadığı bilgisini tutan bayrak verisi.
    /// </summary>
    public bool                 is_last;

    /// <summary>
    /// Sayfa içeriğindeki verilerin kayıtlarını saklayan dizi verisi.
    /// </summary>
    public LeaderBoard_Data[]   data;
}

/// <summary>
/// Liderlik Tablolarının barındırdığı verileri tutmak için hazırlanmış veri sınıfı.
/// <see cref="LeaderBoardPage"/> sınıfı tarafından kullanılıyor.
/// </summary>
[Serializable]
public class LeaderBoard_Data
{
    /// <summary>
    /// Verinin nereden geldiğini belirten bayrak verisi.
    /// <see langword="false"/> verinin online kayıtlardan alındığını,
    /// <see langword="true"/> verinin lokal kayıtlardan alındığını
    /// temsil eder.
    /// </summary>
    public bool     isLocal;

    /// <summary>
    /// Oyuncunun Liderlik tablosundaki sıralama bilgisini tutan veri.
    /// </summary>
    public int      rank;
    /// <summary>
    /// Oyuncunun Liderlik tablosuna kayıt olurken kullandığı isim verisi.
    /// </summary>
    public string   nickname;
    /// <summary>
    /// Oyuncunun oyun sonunda kazandığı toplam skor puanını tutan veri.
    /// </summary>
    public int      score;
    /// <summary>
    /// Oyuncunun tüm soruları çözme süresini belirten veri.
    /// Bu alan ile ilgili bir veri bulunmuyorsa <c>-1</c> değeri alır.
    /// </summary>
    public float    time;
}

/// <summary>
/// Yerel Liderlik Tablosu verileri ve dosya operasyonları için
/// oluşturulmuş sarmalayıcı veri sınıfı.
/// </summary>
[Serializable]
public class LeaderBoard_LocalWrapper
{
    /// <summary>
    /// Dosya içerisindeki yerel Liderlik Tablosu verilerini saklayan dizi verisi.
    /// </summary>
    public LeaderBoard_Local[] data;
}

/// <summary>
/// Yerel Liderlik Tablosu verileri için hazırlanmış veri sınıfı.
/// </summary>
[Serializable]
public class LeaderBoard_Local
{
    /// <summary>
    /// Oyuncunun Liderlik tablosuna kayıt olurken kullandığı isim verisi.
    /// Yerel.
    /// </summary>
    public string   nickname;
    /// <summary>
    /// Oyuncunun oyun sonunda kazandığı toplam skor puanını tutan veri.
    /// Yerel.
    /// </summary>
    public int      score;
    /// <summary>
    /// Oyuncunun tüm soruları çözme süresini belirten veri.
    /// Yerel.
    /// Bu alan ile ilgili bir veri bulunmuyorsa <c>-1</c> değeri alır.
    /// </summary>
    public float    time;
}