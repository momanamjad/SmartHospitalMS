namespace SmartHospitalMS
{
    /// <summary>
    /// Session is a static class that persists user data globally.
    /// JS Analogy: This is exactly like 'localStorage'.
    /// Once the user logs in, we save their info here so all forms
    /// can check who is logged in and what their role is.
    /// </summary>
    public static class Session
    {
        // Private field (Encapsulation)
        private static User _currentUser;

        // Public property to access the user
        public static User CurrentUser
        {
            get => _currentUser;
            set => _currentUser = value;
        }

        // Helper to check if someone is logged in
        public static bool IsLoggedIn => _currentUser != null;

        // Helper to clear session (Logout)
        public static void Logout()
        {
            _currentUser = null;
        }
    }
}
