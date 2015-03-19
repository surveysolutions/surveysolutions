using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace WB.UI.Interviewer
{
    public class RepeaterView<T> : StackLayout
    {
        public RepeaterView()
        {
            this.Spacing = 0;
        }

        public static readonly BindableProperty TemplateSelectorProperty = BindableProperty.Create<RepeaterView<T>, TemplateSelector>(x => x.TemplateSelector, default(TemplateSelector));

        public TemplateSelector TemplateSelector
        {
            get { return (TemplateSelector)GetValue(TemplateSelectorProperty); }
            set { SetValue(TemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Select a datatemplate dynamically
        /// Prefer the TemplateSelector then the DataTemplate
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private DataTemplate GetTemplateFor(Type type)
        {
            DataTemplate retTemplate = null;
            if (TemplateSelector != null)
                retTemplate = TemplateSelector.TemplateFor(type);
            return retTemplate ?? ItemTemplate;
        }

        private View CreateViewByDataTemplate(T bindingContext)
        {
            var templatedControl = (View)this.GetTemplateFor(bindingContext.GetType()).CreateContent();
            templatedControl.BindingContext = bindingContext;

            return templatedControl;
        }

        public ObservableCollection<T> ItemsSource
        {
            get { return (ObservableCollection<T>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create<RepeaterView<T>, ObservableCollection<T>>(p => p.ItemsSource, new ObservableCollection<T>(), BindingMode.OneWay, null, ItemsChanged);

        private static void ItemsChanged(BindableObject bindable, ObservableCollection<T> oldValue, ObservableCollection<T> newValue)
        {
            var repeater = bindable as RepeaterView<T>;

            if (repeater == null) return;

            repeater.ItemsSource.CollectionChanged += repeater.ItemsSource_CollectionChanged;
            repeater.Children.Clear();

            foreach (var repeaterRow in newValue.Select(repeater.CreateViewByDataTemplate))
            {
                repeater.Children.Add(repeaterRow);
            }

            repeater.UpdateChildrenLayout();
            repeater.InvalidateLayout();
        }

        public delegate void RepeaterViewItemAddedEventHandler(object sender, RepeaterViewItemAddedEventArgs args);
        public event RepeaterViewItemAddedEventHandler ItemCreated;

        protected virtual void NotifyItemAdded(View view, object model)
        {
            if (ItemCreated != null)
            {
                ItemCreated(this, new RepeaterViewItemAddedEventArgs(view, model));
            }
        }

        void ItemsSource_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                this.Children.RemoveAt(e.OldStartingIndex);
                this.UpdateChildrenLayout();
                this.InvalidateLayout();
            }

            if (e.NewItems != null)
            {
                foreach (T item in e.NewItems)
                {
                    View repeaterRow = this.CreateViewByDataTemplate(item);

                    this.Children.Insert(this.ItemsSource.IndexOf(item), repeaterRow);
                    NotifyItemAdded(repeaterRow, item);
                }

                this.UpdateChildrenLayout();
                this.InvalidateLayout();
            }
        }

        public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create<RepeaterView<T>, DataTemplate>(p => p.ItemTemplate, default(DataTemplate));

        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
    }

    public class RepeaterViewItemAddedEventArgs : EventArgs
    {
        public RepeaterViewItemAddedEventArgs(View view, object model)
        {
            View = view;
            Model = model;
        }

        public View View { get; set; }
        public object Model { get; set; }
    }
}
