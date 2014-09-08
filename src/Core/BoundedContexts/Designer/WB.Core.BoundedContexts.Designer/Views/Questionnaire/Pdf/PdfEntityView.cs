﻿using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    [DebuggerDisplay("Id = {PublicId}, Title = {Title}")]
    public abstract class PdfEntityView
    {
        protected PdfEntityView()
        {
            this.Children = new List<PdfEntityView>();
        }

        public string Title
        {
            get { return this.title; }
            set { this.title = System.Web.HttpUtility.HtmlDecode(value);  }
        }

        public Guid PublicId { get; set; }

        public int Depth { get; set; }


        public List<PdfEntityView> Children { get; set; }

        public string ItemNumber
        {
            get
            {
                return string.Join(".", this.GetQuestionNumberSections());
            }
        }

        private List<int> questionNumberSections = null;

        protected List<int> GetQuestionNumberSections()
        {
            if (this.questionNumberSections == null)
            {
                this.questionNumberSections = new List<int>();
                var parent = this.GetParent();
                var currentItem = this;
                while (parent != null)
                {
                    var currentItemNumber = parent.Children.IndexOf(currentItem) + 1;
                    this.questionNumberSections.Insert(0, currentItemNumber);
                    currentItem = parent;
                    parent = parent.GetParent();
                }
            }

            return this.questionNumberSections;
        }

        private PdfEntityView parent;
        private string title;

        private void SetParent(PdfEntityView value)
        {
            this.parent = value;
        }

        public PdfEntityView GetParent()
        {
            return this.parent;
        }

        public void AddChild(PdfEntityView child)
        {
            this.Children.Add(child);
            child.SetParent(this);
        }

        public void InsertChild(PdfEntityView child, int index)
        {
            this.Children.Insert(index, child);
            child.SetParent(this);
        }

        public void ReconnectWithParent()
        {
            foreach (var child in Children)
            {
                child.SetParent(this);
                child.ReconnectWithParent();
            }

        }
    }
}