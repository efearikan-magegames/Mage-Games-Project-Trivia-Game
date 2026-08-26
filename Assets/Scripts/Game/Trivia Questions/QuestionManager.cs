using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Soru verilerini yönetmek için oluşturulmuş sınıf.
/// URL üzerinden elde edilen <see cref="QuestionList"/> verisi
/// formatlanarak <see cref="Question"/> listesine ekleniyor.
/// </summary>
public class QuestionManager : MonoBehaviour
{
    /// <summary>
    /// <see cref="QuestionList"/> verisinin internet üzerinden alınabilmesini sağlayan URL.
    /// </summary>
    [SerializeField] private string question_URL = "https://magegamessite.web.app/case1/questions.json";

    /// <summary>
    /// Soru verilerinin URL aracılığı ile
    /// <see cref="GetRequestForQuestions"/> coroutine'inde
    /// hala yüklenmekte olup olmadığını belirten alan.
    /// Eş zamanlı istekleri önlemek için kullanılıyor.
    /// <see cref="GetRequestForQuestions"/> coroutine'i
    /// tarafından ayarlanır.
    /// </summary>
    /// <value>
    /// Sorular yükleniyorken <see langword="true"/>,
    /// yüklenme işlemi yapılmıyorken <see langword="false"/>
    /// değerini alır.
    /// </value>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// <see cref="IsLoading"/> durumuna göre
    /// <see cref="GetRequestForQuestions"/> coroutine'ini
    /// başlatan metot.
    /// Sorular zaten yüklenmişse veya
    /// yüklenmekteyse callback sağlanır.
    /// </summary>
    /// <param name="onQuestionsLoaded">
    /// <see cref="QuestionList"/> verisi elde edildiğinde
    /// callback sağlayan Action.
    /// </param>
    public void Start_LoadingQuestions(Action<List<Question>> onQuestionsLoaded)
    {
        if (!IsLoading)
        {
            // Veriler daha önce indirilmemiş. İndirme başlatılıyor...
            IsLoading = true;
            StartCoroutine(GetRequestForQuestions(onQuestionsLoaded));
        }
        else
        {
            // Veriler indirilmekte...
            Debug.Log("Questions are already loading...");
            onQuestionsLoaded?.Invoke(null);
        }
    }

    /// <summary>
    /// Soru verilerini verilen
    /// <see cref="question_URL"/> üzerinden indiren
    /// ve indirilen JSON dosyasını parçalayan ve
    /// <see cref="Question"/> formatına dönüştüren coroutine.
    /// </summary>
    /// <remarks>
    /// Coroutine sonunda elde edilen veri
    /// <see cref="Question"/> formatına dönüştürülür ve
    /// bir liste içerisinde bütün sorular karıştırılır.
    /// </remarks>
    /// <param name="onQuestionsLoaded">
    /// Yükleme işlemi tamamlandığında çağırılacak
    /// callback'i tutan Action.
    /// </param>
    IEnumerator GetRequestForQuestions(Action<List<Question>> onQuestionsLoaded)
    {
        string url = question_URL;

        // URL adresine istek gönderilir.
        using UnityWebRequest webRequest = UnityWebRequest.Get(url);

        // 10 saniye içerisinde cevap gelmezse istek timeout'lanır.
        webRequest.timeout = 10;

        // Yüklenme durumu yükleme ekranında gösterilmek üzere bir operasyon olarak tanımlanır.
        var operation = webRequest.SendWebRequest();

        // Operasyon sürdüğü sürece yükleme barı güncellenir.
        while (!operation.isDone)
        {
            if (LoadingScreenManager.Instance != null)
            {
                float loadingProgress = operation.progress * LoadingScreenManager.Instance.QuestionLoadProgressWeight;

                // Sahne yüklenmesinden sonra yükleme ilerlemesi kaldığı yerden devam eder.
                LoadingScreenManager.Instance.SetLoadingProgress(1 - LoadingScreenManager.Instance.QuestionLoadProgressWeight + loadingProgress);   
            }

            yield return null;
        }

        // Yapılan istek başarı durumuna göre değerlendirilir. Eğer bir hata meydana geldiyse konsola hata mesajı olarak yazılır.
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error: " + webRequest.error + ", Response Code: " + webRequest.responseCode + ", Result: " + webRequest.result);

            onQuestionsLoaded?.Invoke(null);

            IsLoading = false;
            
            yield break;
        }

        // URL isteği başarılı, JSON verisi elde edildi.
        Debug.Log("Received: " + webRequest.downloadHandler.text);

        string json = webRequest.downloadHandler.text;

        // JSON parçalayıcısı ham satır sonu terimleri ('\n' veya '\r' gibi) ile karşılaştığında çalışmaz.
        // Bu yüzden sorun yaratan ham satır sonu terimleri JSON verisinden ayıklanır.
        json = json.Replace("\n", "");
        json = json.Replace("\r", "");

        // Elde edilen veri o formata uygun olarak hazırlanmış sınıfa JSON parçalama işlemi ile aktarılır.
        QuestionList questionList = JsonUtility.FromJson<QuestionList>(json);

        // Elde edilen veri kontrol edilir, parçalama sırasında bir sorun meydana geldiyse konsola hata mesajı yazılır.
        if (questionList == null || questionList.questions == null)
        {
            Debug.LogError("Failed to parse JSON for Questions");
            
            onQuestionsLoaded?.Invoke(null);

            IsLoading = false;
            
            yield break;
        }

        List<Question> questions = new();

        // Geçerliliği doğrulanmış soru veriler soru listesine aktarılır.
        foreach (Question_ListWeb webQuestion in questionList.questions)
        {
            // Boş soru içeren sorular atlanır.
            if (webQuestion == null || webQuestion.choices == null)
            {
                continue;
            }

            int correctAnswerIndex = webQuestion.CorrectAnswerIndex;

            Question question = new()
            {
                error       = correctAnswerIndex == -1, // Hatalı sorular işaretlenir.
                category    = webQuestion.category,
                question    = webQuestion.question,
                choices     = new string[webQuestion.choices.Length]
            };

            // İndirlen soru verilerinin seçenek metinleri ayıklanır.
            for (int i = 0; i < webQuestion.choices.Length; i++)
            {
                question.choices[i] = StripPrefix(webQuestion.choices[i]);
            }

            // Hatalı olmayan sorularda doğru cevap 0. index'e alınır.
            if (!question.error) (question.choices[correctAnswerIndex], question.choices[0]) = (question.choices[0], question.choices[correctAnswerIndex]);

            // Hazırlanan soru verileri soru listesine eklenir.
            questions.Add(question);
        }

        IsLoading = false;

        // Elde edilen liste karıştırılır.
        ShuffleQuestionList(questions);

        // Soruların gösterimi için ilgili metot uyarılır.
        onQuestionsLoaded?.Invoke(questions);
    }

    /// <summary>
    /// Webden alınan soru verilerinin <see cref="Question.choices"/> metinlerini
    /// seçenek belirteçlerinden arındırılması için kullanılan metot.
    /// Boş bir metin alınması halinde <see cref="string.IndexOf"/> metodunu korumak için
    /// boş metin atlanır.
    /// </summary>
    /// <param name="choice">
    /// Ayıklanacak seçenek metni.
    /// </param>
    /// <returns>
    /// Ayıklanan metin döndürülür.
    /// Gönderilen metin boşsa anında döndürülür.
    /// </returns>
    private string StripPrefix(string choice)
    {
        if (choice == null) return choice;

        int index = choice.IndexOf(") ");

        return index == -1 ? choice : choice[(index + ") ".Length)..];
    }

    /// <summary>
    /// <see cref="Question"/> listesini karıştırmak için kullanılan metot.
    /// Karıştırma işlemi için Fisher-Yates'in algoritması
    /// <see cref="Random"/> sınıfı ile uygulanır.
    /// </summary>
    /// <remarks>
    /// Alınan parametre referans olduğundan karıştırma işlemi gerçek listeye uygulanır.
    /// </remarks>
    /// <param name="questions">
    /// Karıştırılacak liste verisi.
    /// </param>
    private void ShuffleQuestionList(List<Question> questions)
    {
        for (int i = questions.Count - 1; i > 0; i--)
        {
            int rng = UnityEngine.Random.Range(0, i + 1);

            (questions[i], questions[rng]) = (questions[rng], questions[i]);
        }
    }
}
