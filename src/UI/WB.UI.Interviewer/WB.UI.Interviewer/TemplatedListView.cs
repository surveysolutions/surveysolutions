using System;
using Xamarin.Forms;
using XLabs.Forms.Controls;

namespace WB.UI.Interviewer
{
    public class TemplatedListView : ListView
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

        public static readonly BindableProperty TemplateSelectorProperty = BindableProperty.Create<TemplatedListView, IDataTemplateSelector>(p => p.TemplateSelector, null);

        public IDataTemplateSelector TemplateSelector
        {
            get { return (IDataTemplateSelector)GetValue(TemplateSelectorProperty); }
            set { SetValue(TemplateSelectorProperty, value); }
        }


        private void OnBindingContextChanged(object sender, EventArgs e)
        {
            var cell = (ViewCell)sender;
            if (TemplateSelector != null)
            {
                var template = TemplateSelector.SelectTemplate(cell, cell.BindingContext);


                var viewCell = new ExtendedViewCell { View = new Label(), ShowSeparator = false, ShowDisclousure = false, SeparatorColor = Color.Black};
                if (template != null)
                    viewCell.View = (Xamarin.Forms.View)template.CreateContent();
                cell.View = viewCell.View;
            }
        }

    }
}