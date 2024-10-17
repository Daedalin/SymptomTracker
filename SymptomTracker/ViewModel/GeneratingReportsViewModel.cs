using Daedalin.Core.MVVM.ViewModel;
using SymptomTracker.BLL;
using SymptomTracker.Utils.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymptomTracker.ViewModel
{
    internal class GeneratingReportsViewModel : ViewModelBase
    {
        private eEventType m_EventType;

        public GeneratingReportsViewModel() : base()
        {
            GeneratingReportsClick = new RelayCommand(OnGeneratingReportsClick);
            FromDate = DateTime.Now;
            TillDate = DateTime.Now;
        }

        public RelayCommand GeneratingReportsClick { get; set; }
        public DateTime FromDate
        {
            get => GetProperty<DateTime>();
            set => SetProperty(value);
        }

        public DateTime TillDate
        {
            get => GetProperty<DateTime>();
            set => SetProperty(value);
        }
        public List<KeyValuePair<eEventType, string>> Typs { get => Enums.EventType; }

        public KeyValuePair<eEventType, string> SelectedType
        {
            get
            {
                return Typs.FirstOrDefault(t => t.Key == m_EventType);
            }
            set
            {
                m_EventType = value.Key;
                OnPropertyChanged();
            }
        }

        public async void OnGeneratingReportsClick()
        {
            GeneratingReportsClick.IsEnabled = false;
            var Data = await RealtimeDatabaseBll.GetDateForReport(m_EventType, FromDate, TillDate);
            Validate(Data);

            List<string> ImagePaths = new List<string>();
            foreach (var day in Data.Result)
            {
                if (!day.Events.Any(e => e.HasImage))
                    continue;

                foreach (var EventId in day.Events.Where(e => e.HasImage).Select(s => s.ID))
                {
                    var ImagePathResult = await StorageBll.DownloadImage(day.Date, EventId);
                    if (ImagePathResult != null && ImagePathResult.Success)
                        ImagePaths.Add(ImagePathResult.Result);
                }
            }
            var FolerPath = Path.Combine(FileSystem.Current.AppDataDirectory, "Download");
            if (!File.Exists(FolerPath))
                Directory.CreateDirectory(FolerPath);

            var path = Path.Combine(FolerPath, $"Generated-PDF-{DateTime.Now.ToShortDateString()}.pdf");
            var result = PDF_Bll.GeneratingReports(Data.Result, ImagePaths, FromDate, TillDate, path);
            Validate(result);

            bool supportsUri = await Launcher.Default.CanOpenAsync(path);

            if (supportsUri)
                await Launcher.Default.OpenAsync(path);

            GeneratingReportsClick.IsEnabled = true;
        }
    }
}
