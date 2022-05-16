using tldr.Models;

namespace tldr.Services
{
    public class SessionService
    {
        private Blazored.LocalStorage.ILocalStorageService localStorage { get; set; }

        public SessionService(Blazored.LocalStorage.ILocalStorageService localStorage)
        {
            this.localStorage = localStorage;
        }
        
        private Session currentSession = null;
        public Session CurrentSession => currentSession;

        public async Task EnsureSession(Action onSetAction)
        {
            currentSession = await GetCurrentSession(onSetAction);
        }

        public async Task SaveSession()
        {
            await localStorage.SetItemAsync("session", CurrentSession);
        }
        public async Task<Session> GetCurrentSession(Action onSetAction)
        {
            if (currentSession == null)
            {
                currentSession = await GetSession();
                onSetAction?.Invoke();
            }

            return currentSession;
        }

        public async Task<Session> GetSession()
        {
            var session = await localStorage.GetItemAsync<Session>("session");
            if (session == null)
            {
                session = new Session();
                await localStorage.SetItemAsync("session", session);
            }

            return session;
        }
    }
}
