using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Ji2.Camera;
using Ji2.Screens;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace Ji2Core.Core.ScreenNavigation
{
    public class Screens : MonoBehaviour, IScreenSize
    {
        [Header("Optional 3D screens")]
        [SerializeField] private Canvas _threeDimensionalCanvas;
        [SerializeField] private RectTransform _threeDimensionalRoot;

        [Header("Overlay screens")]
        [FormerlySerializedAs("canvas")]
        [SerializeField] private Canvas _overlayCanvas;
        [FormerlySerializedAs("transform")]
        [SerializeField] private RectTransform _overlayRoot;
        [SerializeField] private List<BaseScreen> screens;

        private Dictionary<Type, BaseScreen> _screenOrigins;

        private IObjectResolver _resolver;
        private CameraSource _cameraSource;
        private readonly Stack<BaseScreen> _screenStack = new();
        public BaseScreen CurrentScreen => _screenStack.TryPeek(out var screen) ? screen : null;

        public Vector2 ScreenSize => _overlayRoot != null
            ? new Vector2(_overlayRoot.rect.width, _overlayRoot.rect.height)
            : new Vector2(Screen.width, Screen.height);

        [Inject]
        private void Construct(CameraSource cameraSource, IObjectResolver resolver)
        {
            _resolver = resolver;
            _cameraSource = cameraSource;
            _cameraSource.CameraChanged += OnCameraChanged;
            if (_threeDimensionalCanvas != null)
            {
                _threeDimensionalCanvas.worldCamera = _cameraSource.MainCamera;
            }

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

        private BaseScreen InstantiateScreen(Type type)
        {
            var origin = _screenOrigins[type];
            var root = origin.DisplayMode == ScreenDisplayMode.ThreeDimensional
                       && _threeDimensionalRoot != null
                ? _threeDimensionalRoot
                : _overlayRoot;
            if (root == null)
            {
                throw new InvalidOperationException(
                    "Screens requires an overlay root. Assign it in the Screens prefab.");
            }

            var screen = _resolver.Instantiate(origin, root);

            if (screen is IOverlayContentScreen overlayContentScreen && _overlayRoot != null)
            {
                overlayContentScreen.AttachOverlayContent(_overlayRoot);
            }

            _screenStack.Push(screen);
            return screen;
        }

        private void OnCameraChanged(UnityEngine.Camera camera)
        {
            if (_threeDimensionalCanvas != null)
            {
                _threeDimensionalCanvas.worldCamera = camera;
            }
        }

        private async UniTask CloseCurrent()
        {
            await CurrentScreen.Hide();
            Destroy(CurrentScreen.gameObject);
            _screenStack.Pop();
        }

        private void OnDestroy()
        {
            if (_cameraSource != null)
            {
                _cameraSource.CameraChanged -= OnCameraChanged;
            }
        }
    }
}
