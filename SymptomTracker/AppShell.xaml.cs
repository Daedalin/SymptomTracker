namespace SymptomTracker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
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