using System;
using Xamarin.Forms;

namespace WB.UI.Interviewer
{
    public class TemplatedListView<T> : ListView
    {
        public TemplatedListView()
        {
            ItemTemplate = new DataTemplate(GetHookedCell);
        }

        Cell GetHookedCell()
        {
            var content = new ViewCell();
            content.BindingContextChanged += OnBindingContextChanged;
            return content;
        }

        public static readonly BindableProperty TemplateSelectorProperty = BindableProperty.Create<TemplatedListView<T>, TemplateSelector>(x => x.TemplateSelector, default(TemplateSelector));

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


        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            if (TemplateSelector == null) return;

            var cell = (ViewCell)sender;

            var template =  this.CreateViewByDataTemplate(cell.BindingContext);
            if (template != null)
            {
                cell.View = template;
            }
        }

        private View CreateViewByDataTemplate(object bindingContext)
        {
            var templatedControl = (View)this.GetTemplateFor(bindingContext.GetType()).CreateContent();
            templatedControl.BindingContext = bindingContext;

            return templatedControl;
        }

    }
}