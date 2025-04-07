namespace Project.Services.Features
{
    public class SessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void SetEmployeeCode(string employeeCode)
        {
            _httpContextAccessor.HttpContext.Session.SetString("EmployeeCode", employeeCode);
        }

        public string GetEmployeeCode()
        {
            return _httpContextAccessor.HttpContext.Session.GetString("EmployeeCode");
        }

        public void ClearSession()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
        }

        public bool IsSessionActive()
        {
            return !string.IsNullOrEmpty(GetEmployeeCode());
        }
    }
}
