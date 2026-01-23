using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace Ji2.Screens
{
    public abstract class BaseScreen : SerializedMonoBehaviour
    {
        public virtual UniTask Show()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask Close()
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
    }
}