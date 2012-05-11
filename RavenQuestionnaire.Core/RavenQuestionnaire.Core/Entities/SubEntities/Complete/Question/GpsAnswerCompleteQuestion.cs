using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.Composite;

namespace RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question
{
    public class GpsAnswerCompleteQuestion : IAnswerStrategy
    {
        private ICompleteQuestion document;
        public GpsAnswerCompleteQuestion(ICompleteQuestion document)
        {
            this.document = document;
        }

        public void Add(IComposite c, Guid? parent)
        {
            ICompleteAnswer questionAnswer = this.document.Children[0] as ICompleteAnswer;
            if (questionAnswer == null)
                throw new CompositeException("document is corrapted");
            CompleteAnswer currentAnswer = c as CompleteAnswer;
            if (currentAnswer == null || !this.document.Children.Any(a => a.PublicKey == currentAnswer.PublicKey))
                throw new CompositeException("answer wasn't found");


            /*  string stringValue = currentAnswer.CustomAnswer.ToString();
              var array = currentAnswer.CustomAnswer as string[];
              if (array != null && array.Length > 0)
                  stringValue = array[0];*/
            this.document.Valid = true;
            string[] coordinates = currentAnswer.AnswerValue.ToString().Split(';') ?? new string[2];
            if (coordinates.Length != 2)
              //  throw new InvalidCastException("incorrect format");
                this.document.Valid = false;
            foreach (string coordinate in coordinates)
            {
                double value;
                if (!double.TryParse(coordinate, out value))
                {
                    var altSeparator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator == "." ? "," : ".";
                    if (!double.TryParse(coordinate.Replace(altSeparator,
                                                    CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator), out value))
                    {
                      //  throw new InvalidCastException("incorrect format");
                            this.document.Valid = false;
                    }
                }
            }
            questionAnswer.Selected = true;
          //  currentAnswer.AnswerType = AnswerType.Text;
            questionAnswer.AnswerValue = currentAnswer.AnswerValue.ToString();
       

        }
        public void Remove()
        {
            ICompleteAnswer questionAnswer = this.document.Children[0] as ICompleteAnswer;
            if (questionAnswer == null)
                throw new CompositeException("document is corrapted");
            questionAnswer.Selected = false;
            questionAnswer.AnswerValue = null;
        }
        public void Create(IEnumerable<IComposite> answers)
        {
            document.Children.Add(new CompleteAnswerFactory().ConvertToCompleteAnswer(new Answer()));
            //  document.OnAdded(new CompositeAddedEventArgs(new CompositeAddedEventArgs(result), newanswer));
        }
    }
}
