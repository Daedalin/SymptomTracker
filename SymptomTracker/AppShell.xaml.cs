namespace SymptomTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            var hasLogin = await HasLogin();
            if (hasLogin)
                return;

            if ((args.Source == ShellNavigationSource.Pop || args.Source == ShellNavigationSource.PopToRoot) &&
                 args.Current.Location.ToString().Contains("LoginPage"))
            {
                args.Cancel();
            }
        }

        private async Task<bool> HasLogin()
        {
            var Result = await ViewModelBase.FirebaseBll.Login();
            return Result.Success && Result.Result;
        }


        public void RemovePage<T>()
        {
            var Page = Current.Navigation.NavigationStack.First(t =>
            {
                return t != null && t.GetType() == typeof(T);

            });
            Current.Navigation.RemovePage(Page);
        }
    }
}