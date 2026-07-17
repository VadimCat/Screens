using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Ji2.Screens
{
    public enum ScreenDisplayMode
    {
        ThreeDimensional,
        Overlay
    }

    public interface IOverlayContentScreen
    {
        void AttachOverlayContent(RectTransform overlayRoot);
    }

    public abstract class BaseScreen : MonoBehaviour
    {
        [SerializeField] private ScreenDisplayMode _displayMode = ScreenDisplayMode.ThreeDimensional;

        public ScreenDisplayMode DisplayMode => _displayMode;

        public virtual UniTask Show()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask Hide()
        {
            gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }
    }
}