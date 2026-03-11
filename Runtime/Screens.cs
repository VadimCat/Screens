using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Ji2.Camera;
using Ji2.Screens;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Ji2Core.Core.ScreenNavigation
{
    public class Screens : MonoBehaviour, IScreenSize
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private List<BaseScreen> screens;
        [SerializeField] private RectTransform transform;
        [SerializeField] private CanvasScaler scaler;

        private Dictionary<Type, BaseScreen> _screenOrigins;

        private CameraSource _cameraSource;
        private IObjectResolver _resolver;
        private readonly Stack<BaseScreen> _screenStack = new();
        public BaseScreen CurrentScreen => _screenStack.TryPeek(out var screen) ? screen : null;

        public Vector2 ScreenSize => new(transform.rect.width, transform.rect.height);

        public float ScaleFactor => transform.rect.height / scaler.referenceResolution.y;

        [Inject]
        private void Construct(CameraSource cameraSource, IObjectResolver resolver)
        {
            _resolver = resolver;
            _cameraSource = cameraSource;
            _cameraSource.CameraChanged += SetCamera;
            SetCamera(cameraSource.MainCamera);

            _screenOrigins = new Dictionary<Type, BaseScreen>();
            foreach (var screen in screens)
            {
                _screenOrigins[screen.GetType()] = screen;
            }
        }

        public async UniTask<BaseScreen> PushScreen(Type type)
        {
            var screen = InstantiateScreen(type);
            await screen.Show();
            return screen;
        }

        public async UniTask<TScreen> PushScreen<TScreen>() where TScreen : BaseScreen
        {
            if (CurrentScreen is TScreen screen)
            {
                Debug.LogError($"Screen of type {typeof(TScreen)} is already the current screen.");
                return screen;
            }

            var newScreen = InstantiateScreen(typeof(TScreen));
            await newScreen.Show();
            return (TScreen)CurrentScreen;
        }

        public async UniTask CloseScreen<TScreen>() where TScreen : BaseScreen
        {
            if (CurrentScreen is TScreen)
            {
                await CloseCurrent();
            }
        }

        private void SetCamera(Camera camera)
        {
            canvas.worldCamera = camera;
        }

        private BaseScreen InstantiateScreen(Type type)
        {
            var screen = _resolver.Instantiate(_screenOrigins[type], transform);
            _screenStack.Push(screen);
            return screen;
        }

        private async UniTask CloseCurrent()
        {
            await CurrentScreen.Hide();
            Destroy(CurrentScreen.gameObject);
            _screenStack.Pop();
        }

        private void OnDestroy()
        {
            _cameraSource.CameraChanged -= SetCamera;
        }
    }

    public interface IScreenSize
    {
        Vector2 ScreenSize { get; }
    }
}