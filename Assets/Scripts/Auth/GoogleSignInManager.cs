using System;
using System.Threading.Tasks;
using Google;
using UnityEngine;

namespace UnitySdkDemos.Auth
{
    /// <summary>
    /// Google Sign-In(Thaina/google-signin-unity, Credential Manager 기반) 래퍼.
    /// 로그인/로그아웃/무음 로그인을 제공하고 결과를 이벤트로 알린다.
    /// 실제 계정 선택 UI는 Android/iOS 실기기에서만 동작한다(에디터 미지원).
    /// </summary>
    public class GoogleSignInManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Google Cloud Console에서 만든 '웹 애플리케이션' OAuth 클라이언트 ID")]
        private string _webClientId = "90963081022-h6eolsrodg9i24heo2r1faf3f3m9ue8f.apps.googleusercontent.com";

        [SerializeField]
        [Tooltip("시작 시 무음 로그인(이전 세션 자동 복원)을 시도할지 여부")]
        private bool _trySilentSignInOnStart = true;

        public static GoogleSignInManager Instance { get; private set; }

        public bool IsSignedIn { get; private set; }
        public GoogleUserInfo CurrentUser { get; private set; }

        public event Action<GoogleUserInfo> OnSignInSucceeded;
        public event Action<string> OnSignInFailed;
        public event Action OnSignedOut;

        private bool _isConfigured;
        private TaskScheduler _mainThreadScheduler;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Unity 메인 스레드 컨텍스트를 캡처해 Task 콜백을 메인 스레드로 되돌린다.
            _mainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            Configure();
        }

        private void Start()
        {
            if (_trySilentSignInOnStart)
            {
                SignInSilently();
            }
        }

        /// <summary>
        /// 구글 계정 선택 UI를 띄워 로그인한다. 결과는 이벤트로 전달된다.
        /// </summary>
        public void SignIn()
        {
            Configure();
            try
            {
                GoogleSignIn.DefaultInstance.SignIn()
                    .ContinueWith(OnSignInCompleted, _mainThreadScheduler);
            }
            catch (Exception exception)
            {
                OnSignInFailed?.Invoke(exception.Message);
            }
        }

        /// <summary>
        /// UI 없이 이전 세션을 조용히 복원한다(있을 경우).
        /// </summary>
        public void SignInSilently()
        {
            Configure();
            try
            {
                GoogleSignIn.DefaultInstance.SignInSilently()
                    .ContinueWith(OnSignInCompleted, _mainThreadScheduler);
            }
            catch (Exception exception)
            {
                OnSignInFailed?.Invoke(exception.Message);
            }
        }

        /// <summary>
        /// 로컬 세션을 로그아웃한다(다음 로그인 시 다시 계정 선택).
        /// </summary>
        public void SignOut()
        {
            if (!_isConfigured)
            {
                return;
            }

            try
            {
                GoogleSignIn.DefaultInstance.SignOut();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[GoogleSignIn] SignOut 실패: {exception.Message}");
            }

            IsSignedIn = false;
            CurrentUser = null;
            OnSignedOut?.Invoke();
        }

        private void Configure()
        {
            if (_isConfigured)
            {
                return;
            }

            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                WebClientId = _webClientId,
                RequestEmail = true,
                RequestProfile = true,
                RequestIdToken = true,
                UseGameSignIn = false
            };
            _isConfigured = true;
        }

        private void OnSignInCompleted(Task<GoogleSignInUser> task)
        {
            if (task.IsCanceled)
            {
                OnSignInFailed?.Invoke("로그인이 취소되었습니다.");
                return;
            }

            if (task.IsFaulted)
            {
                OnSignInFailed?.Invoke(ExtractErrorMessage(task.Exception));
                return;
            }

            GoogleSignInUser user = task.Result;
            CurrentUser = new GoogleUserInfo
            {
                UserId = user.UserId,
                Email = user.Email,
                DisplayName = user.DisplayName,
                IdToken = user.IdToken,
                ImageUrl = user.ImageUrl != null ? user.ImageUrl.ToString() : string.Empty
            };
            IsSignedIn = true;
            OnSignInSucceeded?.Invoke(CurrentUser);
        }

        private string ExtractErrorMessage(AggregateException exception)
        {
            if (exception == null)
            {
                return "알 수 없는 로그인 오류입니다.";
            }

            foreach (Exception inner in exception.Flatten().InnerExceptions)
            {
                if (inner is GoogleSignIn.SignInException signInException)
                {
                    return $"[{signInException.Status}] {signInException.Message}";
                }

                return inner.Message;
            }

            return exception.Message;
        }
    }
}
