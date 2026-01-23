using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Ji2.Screens
{
    public abstract class BaseScreen : MonoBehaviour
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