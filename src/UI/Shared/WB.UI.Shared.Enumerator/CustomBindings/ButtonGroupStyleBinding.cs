﻿using Android.Widget;
using MvvmCross.Binding;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ButtonGroupStyleBinding : BaseBinding<Button, GroupStatus>
    {
        public ButtonGroupStyleBinding(Button androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        } 

        protected override void SetValueToView(Button control, GroupStatus value)
        {
            var groupBackgroundResourceId = GetGroupBackgroundResourceIdByStatus(value);

            control.SetBackgroundResource(groupBackgroundResourceId);
        }

        private static int GetGroupBackgroundResourceIdByStatus(GroupStatus status)
        {
            switch (status)
            {
                case GroupStatus.Completed:
                    return Resource.Drawable.group_completed;
                case GroupStatus.StartedInvalid:
                case GroupStatus.CompletedInvalid:
                    return Resource.Drawable.group_with_invalid_answers;
                default:
                    return Resource.Drawable.group_started;
            }
        }
        
    }
}
