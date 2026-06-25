using System.ComponentModel;
using System.Runtime.CompilerServices;
using SRLMB.QAQC.Core;

namespace SRLMB.QAQC.UI
{
    public sealed class CheckListItem : INotifyPropertyChanged
    {
        public IQaQcCheck Check { get; }
        public string DisplayName => $"{Check.Category} — {Check.Name}";

        private bool _isSelected = true;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value) return;
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public CheckListItem(IQaQcCheck check)
        {
            Check = check;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
