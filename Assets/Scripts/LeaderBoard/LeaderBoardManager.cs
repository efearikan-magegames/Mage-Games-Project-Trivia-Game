using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Liderlik Tabloları verilerini yönetmek için oluşturulmuş singleton yapıdaki sınıf.
/// </summary>
/// <remarks>
/// Verilen URL üzerinden elde edilen <see cref="LeaderBoardPage"/> verisi
/// <see cref="LeaderBoard_Data.isLocal"/> = <see langword="false"/> etiketi ile,
/// lokal Liderlik Tablosunu tutan <see cref="LeaderBoard_LocalWrapper"/> verisi ise
/// <see cref="LeaderBoard_Data.isLocal"/> = <see langword="true"/> etiketi ile
/// <see cref="LeaderBoard_Data"/> formatındaki bir liste yapısında birleştirilerek
/// <see cref="SortEntries"/> metodu ile sıralanıyor ve sıralama sonucunda
/// <see cref="AssignRanks"/> metodu ile <see cref="LeaderBoard_Data.rank"/> alanları
/// yeniden belirleniyor.
/// </remarks>
public class LeaderBoardManager : MonoBehaviour
{
    public static LeaderBoardManager Instance { get; private set; }

    /// <summary>
    /// <see cref="LeaderBoardPage"/> verisinin internet üzerinden alınabilmesini sağlayan URL.
    /// Sayfa yapısı için URL formatlandı.
    /// </summary>
    [SerializeField] private string leaderBoard_URL = "https://magegamessite.web.app/case1/leaderboard_page_{0}.json"; //{ "https://magegamessite.web.app/case1/leaderboard_page_0.json", "https://magegamessite.web.app/case1/leaderboard_page_1.json" };

    /// <summary>
    /// Lokal <see cref="LeaderBoard_LocalWrapper"/> verisine erişebilmek için
    /// lokal dosya işlemleri uygulayan yardımcı sınıf verisi.
    /// </summary>
    private LocalLeaderBoardStorage storageData;

    /// <summary>
    /// URL üzerinden elde edilen <see cref="LeaderBoardPage"/> verilerin ve
    /// lokal dosyalarda saklanan <see cref="LeaderBoard_LocalWrapper"/> verilerinin
    /// birleştirilerek kullanıldığı liste yapısı.
    /// </summary>
    /// <remarks>
    /// Gelen veriler <see cref="LeaderBoard_Data"/> formatına dönüştürülerek saklanır.
    /// </remarks>
    private readonly List<LeaderBoard_Data> leaderBoardEntries = new();

    /// <summary>
    /// Liderlik Tabloları verilerinin URL aracılığı ile
    /// <see cref="LoadAllLeaderBoardEntries"/> coroutine'inde
    /// hala yüklenmekte olup olmadığını belirten alan.
    /// Eş zamanlı istekleri önlemek için kullanılıyor.
    /// <see cref="LoadAllLeaderBoardEntries"/> coroutine'i
    /// tarafından ayarlanır.
    /// </summary>
    /// <value>
    /// Sayfa yükleniyorken <see langword="true"/>,
    /// yüklenme işlemi yapılmıyorken <see langword="false"/>
    /// değerini alır.
    /// </value>
    public bool IsLoading { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        Instance = this;

        storageData = new();
    }

    /// <summary>
    /// <see cref="IsLoading"/> durumuna göre
    /// <see cref="LoadAllLeaderBoardEntries"/> coroutine'ini
    /// başlatan metot.
    /// </summary>
    /// <param name="onPageLoaded">
    /// Yükleme işlemi tamamlandığında başarı durumunu
    /// iletecek callback'i ileten Action.
    /// </param>
    public void Start_LoadingLeaderBoardPages(Action<bool> onPageLoaded)
    {
        if(!IsLoading)
        {
            // Veriler henüz yüklenmedi, yüklemeyi başlat.
            IsLoading = true;
            StartCoroutine(LoadAllLeaderBoardEntries(onPageLoaded));
        }
        else
        {
            // Veriler henüz yüklenmedi, şu an yüklenmekte...
            Debug.Log("Page is already loading...");
            onPageLoaded?.Invoke(false);
        }
    }

    /// <summary>
    /// Liderlik Tabloları verilerini verilen
    /// <see cref="leaderBoard_URL"/> üzerinden indiren
    /// ve indirilen JSON dosyasını parçalayan coroutine.
    /// </summary>
    /// <param name="onPageLoaded">
    /// Yükleme işlemi tamamlandığında başarı durumunu
    /// iletecek callback'i tutan Action.
    /// </param>
    IEnumerator LoadAllLeaderBoardEntries(Action<bool> onPageLoaded)
    {
        leaderBoardEntries.Clear();

        for (int pageIndex = 0; ; pageIndex++)
        {
            // URL sayfa indexine göre formatlanır.
            string url = string.Format(leaderBoard_URL, pageIndex);

            // URL adresine istek gönderilir.
            using UnityWebRequest webRequest = UnityWebRequest.Get(url);

            // 10 saniye içerisinde cevap gelmezse istek timeout'lanır.
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            // Yapılan istek başarı durumuna göre değerlendirilir. Eğer bir hata meydana geldiyse konsola hata mesajı olarak yazılır.
            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + webRequest.error + ", Response Code: " + webRequest.responseCode + ", Result: " + webRequest.result);

                // Son veri bilgisinin eksik olması durumunda
                // mevcut sayfa index'inde başka sayfa bulunmuyorsa
                // "Sayfa bulunamadı" hatası yakalanır ve URL istekleri sonlandırılır.
                if (webRequest.responseCode == 404) break;
                
                // Sayfa bulunamadı hatası dışındaki hatalar
                // başarısız indirme olarak aksiyona bildiriliyor.
                onPageLoaded?.Invoke(false);

                IsLoading = false;
    
                yield break;
            }

            // URL isteği başarılı, JSON verisi elde eldildi.
            Debug.Log("Received: " + webRequest.downloadHandler.text);

            string json = webRequest.downloadHandler.text;

            // Elde edilen veri o formata uygun olarak hazırlanmış sınıfa JSON parçalama işlemi ile aktarılır.
            LeaderBoardPage leaderBoard = JsonUtility.FromJson<LeaderBoardPage>(json);

            // Elde edilen veri kontrol edilir, parçalama sırasında bir sorun meydana geldiyse konsola hata mesajı yazılır.
            if (leaderBoard == null || leaderBoard.data == null)
            {
                Debug.LogError("Failed to parse JSON for page " + pageIndex);
                
                // Json parçalama işleminin başarısız olması aksiyona bildiriliyor.
                onPageLoaded?.Invoke(false);

                IsLoading = false;
                
                yield break;
            }

            // İndirilen veriler listeye eklenir.
            foreach (LeaderBoard_Data webData in leaderBoard.data)
            {
                webData.isLocal = false;
                webData.time    = -1;
                webData.rank    = 0;

                leaderBoardEntries.Add(webData);
            }

            // İndirilen verinin son veri olup olmadığı kontrol edilir.
            // Son veri ise URL istekleri sonlandırılır.
            if (leaderBoard.is_last) break;
        }

        // Yerel Liderlik Tablosu verileri yerel dosyadan yüklenir.
        LeaderBoard_LocalWrapper leaderBoard_Wrapper = storageData.Load_LeaderBoard();

        // Yerel Liderlik Tablosu verisi uygun formata dönüştürülerek listeye kaydedilir.
        foreach (LeaderBoard_Local localData in leaderBoard_Wrapper.data)
        {
            LeaderBoard_Data data = new()
            {
                isLocal = true,
                nickname = localData.nickname,
                score = localData.score,
                time = localData.time
            };

            leaderBoardEntries.Add(data);
        }

        // Elde edilen liste önce skora eşitlik durumunda süreye göre sıralanır.
        SortEntries();
        // Sıralama sonucunda veriler competition ranking kuralına göre rank değerleri belirleniyor.
        AssignRanks();

        // Yükleme başarılı bilgisi aksiyona iletiliyor.
        onPageLoaded?.Invoke(true);

        IsLoading = false;
    }

    /// <summary>
    /// Toplanan bütün <see cref="LeaderBoard_Data"/> verilerini barındıran
    /// <see cref="leaderBoardEntries"/> listesinin sıralamasını yapan metot.
    /// </summary>
    /// <remarks>
    /// Sıralama için liste yapısının
    /// <see cref="List{T}.Sort"/> özelliğini kullanır.
    /// </remarks>
    private void SortEntries()
    {
        leaderBoardEntries.Sort(CompareEntries);
    }

    /// <summary>
    /// <see cref="SortEntries"/> metodunun sıralama koşullarını belirleyen metot.
    /// Gelen verilerin önce <see cref="LeaderBoard_Data.score"/> değeri karşılaştırılır.
    /// Eşitlik durumunda <see cref="LeaderBoard_Data.time"/> değerine bakılır.
    /// Yine eşitlik olması durumunda berabere kabul edilir.
    /// </summary>
    /// <remarks>
    /// Karşılaştırılan verilerin <see cref="LeaderBoard_Data.time"/> değeri yoksa (<c>-1</c>)
    /// bu veri <see cref="LeaderBoard_Data.score"/> sıralamasında aynı olduğu verilerin sonuna yerleştirilir.
    /// </remarks>
    /// <param name="a">
    /// Karşılaştırma yapılacak ilk <see cref="LeaderBoard_Data"/> verisi.
    /// </param>
    /// <param name="b">
    /// Karşılaştırma yapılacak ikinci <see cref="LeaderBoard_Data"/> verisi.
    /// </param>
    /// <returns>
    /// Karşılaştırma sonucuna göre bir <see cref="int"/> değeri döndürülür.
    /// İlk değer ikinci değerden büyükse <c>negatif bir tamsayı</c>,
    /// İkinci değer ilk değerden büyükse <c>pozitif bir tamsayı</c>,
    /// İki değer de eşitse <c>0</c> döndürülür.
    /// </returns>
    private int CompareEntries(LeaderBoard_Data a, LeaderBoard_Data b)
    {
        int result = b.score.CompareTo(a.score);

        if (result != 0)
        {
            return result;
        }
        
        if (a.time < 0 && b.time < 0)
        {
            return 0;
        }
        else if (a.time < 0 && b.time >= 0)
        {
            return 1;
        }
        else if (a.time >= 0 && b.time < 0)
        {
            return -1;
        }
        else
        {
            result = a.time.CompareTo(b.time);

            return result;

        }
    }

    /// <summary>
    /// Sıralanmış <see cref="LeaderBoard_Data"/> verisinin
    /// <see cref="LeaderBoard_Data.rank"/> değerlerini atayan metot.
    /// </summary>
    /// <remarks>
    /// Competition ranking kurallarına göre sıralama ataması yapılır.
    /// </remarks>
    private void AssignRanks()
    {
        for (int i = 0; i < leaderBoardEntries.Count; i++)
        {
            if (i == 0 || CompareEntries(leaderBoardEntries[i], leaderBoardEntries[i - 1]) != 0)
            {
                leaderBoardEntries[i].rank = i + 1;
            }
            else
            {
                leaderBoardEntries[i].rank = leaderBoardEntries[i - 1].rank;
            }
        }
    }

    /// <summary>
    /// İstenen <see cref="LeaderBoard_Data"/> verilerini
    /// bir liste olarak ileten metot.
    /// </summary>
    /// <param name="pageIndex">
    /// İstenen sayfanın sayfa index'ini belirten veri.
    /// </param>
    /// <param name="pageSize">
    /// Sayfaların kaç <see cref="LeaderBoard_Data"/> verisi
    /// içereceğini belirten veri.
    /// </param>
    /// <returns>
    /// İstenen <see cref="LeaderBoard_Data"/> verilerini
    /// sayfa formatında oluşturarak bir liste olarak döndürür.
    /// </returns>
    public List<LeaderBoard_Data> GetPage(int pageIndex, int pageSize)
    {
        int startIndex = pageIndex * pageSize;

        if (startIndex >= leaderBoardEntries.Count)
        {
            List<LeaderBoard_Data> emptyList = new();
            return emptyList;
        }

        int count = Mathf.Min(pageSize, leaderBoardEntries.Count - startIndex);

        return leaderBoardEntries.GetRange(startIndex, count);
    }

    /// <summary>
    /// Parametre değerlerine göre sayfanın
    /// son sayfa olup olmadığını kontrol eden metot.
    /// </summary>
    /// <param name="pageIndex">
    /// Son sayfa olup olmadığı kontrol edilecek sayfa index'i verisi.
    /// </param>
    /// <param name="pageSize">
    /// Bir sayfanın içerdiği veri sayısı.
    /// </param>
    /// <returns>
    /// Sayfanın son sayfa olup olmadığını
    /// <see langword="bool"/> veri tipinde döndürülür.
    /// </returns>
    public bool IsLastPage(int pageIndex, int pageSize)
    {
        return (pageIndex + 1) * pageSize >= leaderBoardEntries.Count;
    }

    /// <summary>
    /// Yerel Liderlik Tablosu dosyasında
    /// veri olup olmadığını kontrol eden metot.
    /// Bilgi için <see cref="LocalLeaderBoardStorage.Load_LeaderBoard"/> metotdunun
    /// dönüş değerini kullanır.
    /// </summary>
    /// <returns>
    /// Dosyada en az 1 girdi bulunuyorsa <see langword="true"/>,
    /// hiçbir girdi bulunmuyorsa <see langword="false"/>
    /// değerini alır.
    /// </returns>
    public bool IsLocalLeaderBoardHasEntries()
    {
        return storageData.Load_LeaderBoard().data.Length > 0;
    }

    /// <summary>
    /// Yerel Liderlik Tablosu dosyasını
    /// silmek için kullanılan metot.
    /// Dosya silme işlemi için
    /// <see cref="LocalLeaderBoardStorage.Clear_LocalLeaderBoard"/> metodunu
    /// kullanır.
    /// </summary>
    /// <returns>
    /// Silme işlemi sonucu;
    /// başarılıysa <see langword="true"/>,
    /// başarısızsa <see langword="false"/>
    /// değerini alır.
    /// </returns>
    public bool DeleteLocalLeaderBoard()
    {
        return storageData.Clear_LocalLeaderBoard();
    }
}