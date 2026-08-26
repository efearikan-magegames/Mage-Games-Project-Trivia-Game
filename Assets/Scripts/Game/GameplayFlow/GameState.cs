/// <summary>
/// Oyun durumlarının yönetilebilmesi için
/// oluşturulmuş soyut düz C# sınıfı.
/// "State" tasarım desenini kullanır.
/// </summary>
/// <remarks>
/// Durum sınıfları yapılarını bu sınıftan miras alır.
/// </remarks>
public abstract class GameState
{
    /// <summary>
    /// Oyun bağlamının durum sınıfları arasında kullanılabilmesi için
    /// kullanılan <see cref="GameplayManager"/> objesi.
    /// </summary>
    /// <remarks>
    /// Sadece diğer durum sınıfları tarafından kullanılabilir ve
    /// durum sınıfları arasında aktarılır.
    /// </remarks>
    protected GameplayManager context;

    /// <summary>
    /// Durum sınıflarının bağlamı iletebilmesi ve
    /// tanımlayabilmesi için kullanılan constructor.
    /// </summary>
    /// <param name="context">
    /// Bağlamı kullanabilmek için Parent sınıfa aktarılan bağlam verisi.
    /// </param>
    protected GameState(GameplayManager context)
    {
        this.context = context;
    }

    /// <summary>
    /// Durum sınıflarının çağırıldıklarında
    /// uygulayacakları işleri içeren metot.
    /// </summary>
    /// <remarks>
    /// Tüm durum sınıflarının bir giriş işi olacağı için
    /// soyut bir metot olarak tanımlı.
    /// </remarks>
    public abstract void EnterState();

    /// <summary>
    /// Durum sınıflarının durum bittikten sonra
    /// çıkış yaparken uygulayacakları işleri içeren metot.
    /// </summary>
    /// <remarks>
    /// Tüm durum sınıflarının bir çıkış işi olmak zorunda olmadığından
    /// sanal bir metot olarak tanımlı.
    /// </remarks>
    public virtual void ExitState() { }

    /// <summary>
    /// Durum sınıflarının durum süresince
    /// uygulayacakları işleri içeren metot.
    /// </summary>
    /// <remarks>
    /// Tüm durum sınıflarının bir tik işi olmak zorunda olmadığından
    /// sanal bir metot olarak tanımlı.
    /// Tik <see cref="GameplayManager"/> sınıfının <see cref="GameplayManager.Update"/> metodunu kullanır.
    /// </remarks>
    public virtual void Tick() { }
}
