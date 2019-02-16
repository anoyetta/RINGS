using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace aframe
{
    public class SuspendableObservableCollection<T> : ObservableCollection<T>
    {
        public SuspendableObservableCollection()
        {
        }

        public SuspendableObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }

        public SuspendableObservableCollection(List<T> list) : base(list)
        {
        }

        private bool _suppressNotification = false;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!this._suppressNotification)
            {
                base.OnCollectionChanged(e);
            }
        }

        public void AddRange(
            IEnumerable<T> elements,
            bool clear = false)
        {
            if (elements == null ||
                !elements.Any())
            {
                return;
            }

            try
            {
                this._suppressNotification = true;

                if (clear)
                {
                    this.Clear();
                }

                foreach (var item in elements)
                {
                    this.Add(item);
                }
            }
            finally
            {
                this._suppressNotification = false;
            }

            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
