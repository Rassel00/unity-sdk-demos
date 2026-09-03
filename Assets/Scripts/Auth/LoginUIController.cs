using UnityEngine;
using UnityEngine.UI;

namespace UnitySdkDemos.Auth
{
    /// <summary>
    /// 구글 로그인 데모용 uGUI 컨트롤러.
    /// 로그인/로그아웃 버튼과 상태 텍스트를 <see cref="GoogleSignInManager"/> 이벤트에 연결한다.
    /// </summary>
    public class LoginUIController : MonoBehaviour
    {
        [SerializeField]
        private Button _signInButton;

        [SerializeField]
        private Button _signOutButton;

        [SerializeField]
        private Text _statusText;

        private void OnEnable()
        {
            if (_signInButton != null)
            {
                _signInButton.onClick.AddListener(HandleSignInClicked);
            }

            if (_signOutButton != null)
            {
                _signOutButton.onClick.AddListener(HandleSignOutClicked);
            }

            GoogleSignInManager manager = GoogleSignInManager.Instance;
            if (manager != null)
            {
                manager.OnSignInSucceeded += HandleSignInSucceeded;
                manager.OnSignInFailed += HandleSignInFailed;
                manager.OnSignedOut += HandleSignedOut;
            }

            UpdateButtons(false);
            SetStatus("로그인이 필요합니다.");
        }

        private void OnDisable()
        {
            if (_signInButton != null)
            {
                _signInButton.onClick.RemoveListener(HandleSignInClicked);
            }

            if (_signOutButton != null)
            {
                _signOutButton.onClick.RemoveListener(HandleSignOutClicked);
            }

            GoogleSignInManager manager = GoogleSignInManager.Instance;
            if (manager != null)
            {
                manager.OnSignInSucceeded -= HandleSignInSucceeded;
                manager.OnSignInFailed -= HandleSignInFailed;
                manager.OnSignedOut -= HandleSignedOut;
            }
        }

        private void HandleSignInClicked()
        {
            SetStatus("로그인 중...");
            if (GoogleSignInManager.Instance != null)
            {
                GoogleSignInManager.Instance.SignIn();
            }
        }

        private void HandleSignOutClicked()
        {
            if (GoogleSignInManager.Instance != null)
            {
                GoogleSignInManager.Instance.SignOut();
            }
        }

        private void HandleSignInSucceeded(GoogleUserInfo user)
        {
            SetStatus($"환영합니다, {user.DisplayName}\n{user.Email}");
            UpdateButtons(true);
        }

        private void HandleSignInFailed(string message)
        {
            SetStatus($"로그인 실패: {message}");
            UpdateButtons(false);
        }

        private void HandleSignedOut()
        {
            SetStatus("로그아웃되었습니다.");
            UpdateButtons(false);
        }

        private void UpdateButtons(bool isSignedIn)
        {
            if (_signInButton != null)
            {
                _signInButton.gameObject.SetActive(!isSignedIn);
            }

            if (_signOutButton != null)
            {
                _signOutButton.gameObject.SetActive(isSignedIn);
            }
        }

        private void SetStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }
        }
    }
}
