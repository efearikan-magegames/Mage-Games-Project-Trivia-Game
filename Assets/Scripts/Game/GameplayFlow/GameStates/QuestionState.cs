using UnityEngine;

/// <summary>
/// Önceki yükleme durumunda <see cref="GameplayManager.questions"/> verisine kaydedilen soruların
/// oyuncuya yansıtılarak oyuncunun bir cevap seçeneği seçmesinin beklendiği oyun durumu sınıfıdır.
/// </summary>
public class QuestionState : GameState
{
    /// <summary>
    /// Soru oyun durumu sınıfının constructor metodu.
    /// </summary>
    /// <param name="context">
    /// <see cref="GameplayManager"/> referansı içeren oyun bağlamını
    /// parent sınıfa iletir.
    /// </param>
    public QuestionState(GameplayManager context) : base(context) { }

    /// <summary>
    /// Zamanlayıcı geri sayımında
    /// geriye kalan zaman bilgisini tutan veri.
    /// </summary>
    private float remainingTime = 0f;

    /// <summary>
    /// Karşılaştırma yapılabilmesi için zamanlayıcıya yazılmış en son değeri tutan veri.
    /// </summary>
    /// <remarks>
    /// Amacı sürekli aynı değeri yazdırma işleminden kaçınmak.
    /// </remarks>
    private int timerCounter = 0;



    public override void EnterState()
    {
        // Zamanlayıcı hazırlanır.
        remainingTime = context.Config.questionDuration;
        timerCounter = Mathf.CeilToInt(remainingTime);

        // Sorunun geçerliliği test edilir.
        if (context.CurrentQuestion.error)
        {
            Debug.LogError("Invalid Question");
            
            if (context.HasMoreQuestions)
            {
                // Soru hatalıysa bir sonraki soruya geçilir.
                context.MoveToNextQuestion();
            }
            else
            {
                // Son soru da hatalıysa oyun bitirilir.
                context.ChangeGameState(new FinishGameState(context));
            }
            
            return;
        }
        
        // Arayüz ayarlanır.
        context.UI.ShowQuestion(context.CurrentQuestion);
        context.UI.SetChoiceButtonsInteractable(true);
        context.UI.OnAnswerSelected += OnAnswerSelected;
        context.UI.UpdateTimer(timerCounter);
    }

    public override void ExitState()
    {
        // Kalan süre bağlama bildiriliyor.
        context.IncreaseElapsedTime(context.Config.questionDuration - remainingTime);

        // Durum çıkışında aksiyon aboneliği iptal edilir.
        // Bırakılmazsa sonraki soruda aksiyon yeniden abone olur ve
        // bir tıklama iki kez sayılır.
        context.UI.OnAnswerSelected -= OnAnswerSelected;
    }

    public override void Tick()
    {
        // Her framede süre saniye cinsinden azaltılır.
        remainingTime -= Time.deltaTime;

        // Zamanlayıcıda gösterilmek için tam sayıya yuvarlanır.
        int currentTime = Mathf.CeilToInt(remainingTime);

        // Kalan süre karşılaştırılır. Önce sürenin kritik bölgede olup olmadığına bakılır.
        // Sonra bir önceki framelerde yazılan tam sayı değeri şu anki tam sayı değeri ile karşılaştırılır.
        // Eğer karşılaştırma sırası ters olsaydı kritik bölgede sayaçta gösterilen sayı
        // sürenin tam sayı olduğu framede sayaçta da tam sayı olarak görünürdü ve bu titremeye sebep olurdu.
        if (remainingTime < context.Config.recklessTime)
        {
            // Kalan süre kritik bölgedeyse sayaç doğrudan ondalıklı değer göstermeye başlar (float aşırı yüklemesi ve "0.0" formatı ile).
            context.UI.UpdateTimer(Mathf.Max(remainingTime, 0));
        }
        else if (timerCounter != currentTime)
        {
            // Kritik süreden yüksek kalan süreler tam sayıya yuvarlanarak gösterilir (int aşırı yüklemesi ve "0" formatı ile).
            timerCounter = currentTime;
            context.UI.UpdateTimer(Mathf.Max(timerCounter, 0));
        }

        if (remainingTime <= 0)
        {
            // Kalan süre sıfır veya sıfırdan düşük bir değeri görürse soru zaman aşımına uğrar.
            context.AddScore(context.Config.timeoutScore);

            // Zaman aşımı yaşandığı için seçilen seçenek butonu seçeneği -1 olarak iletildi.
            context.ChangeGameState(new AnswerRevealState(context, -1, context.UI.CorrectButtonIndex));
            return;
        }
    }

    /// <summary>
    /// Oyuncu herhangi bir seçenek butonu seçimi yaptığında uyarılan metottur.
    /// </summary>
    /// <param name="selectedIndex">
    /// Oyuncunun yaptığı seçenek butonu seçiminin index değeri.
    /// </param>
    private void OnAnswerSelected(int selectedIndex)
    {
        // Verilen cevap sorunun doğru cevabı ile karşılaştırılır ve ona göre bir skor uygulaması yapılır.
        // Cevap doğru ise doğru cevap puanı,
        // yanlış ise yanlış cevap puanı skora eklenir.
        context.AddScore(context.UI.CorrectButtonIndex == selectedIndex ? context.Config.correctScore : context.Config.wrongScore);

        // Başka bir seçim yapılamaması için butonlar kapatılır.
        context.UI.SetChoiceButtonsInteractable(false);

        // Ardından oyun cevap açığa çıkma durumuna geçirilir.
        context.ChangeGameState(new AnswerRevealState(context, selectedIndex, context.UI.CorrectButtonIndex));
    }
}
