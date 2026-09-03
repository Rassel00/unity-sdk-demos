namespace UnitySdkDemos.Auth
{
    /// <summary>
    /// 구글 로그인 성공 시 반환되는 사용자 정보 모델.
    /// </summary>
    public class GoogleUserInfo
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public string IdToken { get; set; }
        public string ImageUrl { get; set; }
    }
}
