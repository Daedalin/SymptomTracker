namespace SymptomTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            var hasLogin = ViewModelBase.RealtimeDatabaseBll.HasLogin;
            if (!hasLogin && (args.Source == ShellNavigationSource.Pop || args.Source == ShellNavigationSource.PopToRoot))
            {
                if (args.Current.Location.OriginalString.Contains("LoginPage"))
                {
                    args.Cancel();
                    return;
                }
            }

            base.OnNavigating(args);
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