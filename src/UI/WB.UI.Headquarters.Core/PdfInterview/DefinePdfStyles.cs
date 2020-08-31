using MigraDocCore.DocumentObjectModel;

namespace WB.UI.Headquarters.PdfInterview
{
    public class DefinePdfStyles
    {
        const string DefaultFonts = "Noto Sans, Arial, sans-serif";

        public void Define(Document document)
        {
            var defaultStyle = document.Styles.AddStyle(PdfStyles.Default, StyleNames.DefaultParagraphFont);
            defaultStyle.Font.Name = DefaultFonts;
            defaultStyle.Font.Bold = false;
            defaultStyle.Font.Italic = false;
            defaultStyle.Font.Color = Colors.Black;
            defaultStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.AtLeast;
            //defaultPaddingStyle.ParagraphFormat.AddTabStop("8cm", TabAlignment.Center); 

            var tableOfContent = document.Styles.AddStyle(PdfStyles.TableOfContent, PdfStyles.Default);
            tableOfContent.ParagraphFormat.Font.Size = Unit.FromPoint(8);
            tableOfContent.ParagraphFormat.Font.Color = Colors.Black;
            //tableOfContent.ParagraphFormat.SpaceBefore = "8pt";
            //tableOfContent.ParagraphFormat.LeftIndent = "8pt";
            // tableOfContent.ParagraphFormat.LineSpacing = Unit.FromPoint(14);
            // tableOfContent.ParagraphFormat.LineSpacingRule = LineSpacingRule.Exactly;
            tableOfContent.ParagraphFormat.LineSpacingRule = LineSpacingRule.OnePtFive;
            tableOfContent.ParagraphFormat.AddTabStop(Unit.FromPoint(37), TabAlignment.Left);

            document.Styles.AddStyle(PdfStyles.HeaderLineValue, PdfStyles.Default).Font =
                new Font() { Size = 18, Bold = true };
            
            var sectionHeader = document.Styles.AddStyle(PdfStyles.SectionHeader, PdfStyles.Default);
            sectionHeader.Font.Size = Unit.FromPoint(18); 
            //sectionHeader.ParagraphFormat.LeftIndent = "8pt";
            //sectionHeader.ParagraphFormat.Borders.Top = new Border() { Width = "1pt", Color = Colors.DarkGray };
            sectionHeader.ParagraphFormat.LineSpacing = 0;
            sectionHeader.ParagraphFormat.LineSpacingRule = LineSpacingRule.Single;
            sectionHeader.ParagraphFormat.OutlineLevel = OutlineLevel.Level1;
            //sectionHeader.ParagraphFormat.SpaceBefore = "40pt";
            sectionHeader.ParagraphFormat.SpaceAfter = Unit.FromPoint(15);

            var groupHeader = document.Styles.AddStyle(PdfStyles.GroupHeader, PdfStyles.Default);
            groupHeader.Font.Size = Unit.FromPoint(12);
            groupHeader.ParagraphFormat.LeftIndent = Unit.FromPoint(49);
            groupHeader.ParagraphFormat.SpaceBefore = Unit.FromPoint(15);
            groupHeader.ParagraphFormat.SpaceAfter = Unit.FromPoint(15);
            
            document.Styles.AddStyle(PdfStyles.RosterTitle, PdfStyles.Default).Font =
                new Font() { Size = Unit.FromPoint(12), Italic = true };

            var questionStyle = document.Styles.AddStyle(PdfStyles.QuestionTitle, PdfStyles.Default);
            questionStyle.Font.Size = Unit.FromPoint(8);
            questionStyle.ParagraphFormat.LineSpacingRule = LineSpacingRule.OnePtFive;

            document.Styles.AddStyle(PdfStyles.IdentifyerQuestionAnswer, PdfStyles.Default).Font =
                new Font() { Size = Unit.FromPoint(10), Bold = true };
            
            var identifyerNotAnswered = document.Styles.AddStyle(PdfStyles.IdentifyerQuestionNotAnswered, PdfStyles.Default);
            identifyerNotAnswered.Font = new Font() { Size = Unit.FromPoint(10), Color = new Color(45, 156, 219), Italic = true };
            //notAnswered.ParagraphFormat.LineSpacing = 1.5;
            identifyerNotAnswered.ParagraphFormat.LineSpacingRule = LineSpacingRule.OnePtFive;

            document.Styles.AddStyle(PdfStyles.QuestionAnswer, PdfStyles.Default).Font =
                new Font() { Size = Unit.FromPoint(8), Bold = true };
            
            var notAnswered = document.Styles.AddStyle(PdfStyles.QuestionNotAnswered, PdfStyles.Default);
            notAnswered.Font = new Font() { Size = Unit.FromPoint(8), Color = new Color(45, 156, 219), Italic = true };
            //notAnswered.ParagraphFormat.LineSpacing = 1.5;
            notAnswered.ParagraphFormat.LineSpacingRule = LineSpacingRule.OnePtFive;
            
            var questionDateStyle = document.Styles.AddStyle(PdfStyles.QuestionAnswerDate, PdfStyles.Default);
            questionDateStyle.Font.Size = Unit.FromPoint(8);
            questionDateStyle.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            
            var questionTimeStyle = document.Styles.AddStyle(PdfStyles.QuestionAnswerTime, PdfStyles.Default);
            questionTimeStyle.Font.Size = Unit.FromPoint(8);
            questionTimeStyle.ParagraphFormat.Alignment = ParagraphAlignment.Right;
            
            document.Styles.AddStyle(PdfStyles.StaticTextTitle, PdfStyles.Default).Font =
                new Font() { Size = Unit.FromPoint(8) };
            document.Styles.AddStyle(PdfStyles.ValidateErrorTitle, PdfStyles.Default).Font =
                new Font() { Size = Unit.FromPoint(8), Italic = true, Color = PdfColors.Error };
            document.Styles.AddStyle(PdfStyles.ValidateErrorMessage, PdfStyles.Default).Font =
                new Font() { Size = Unit.FromPoint(8), Color = PdfColors.Error };
            document.Styles.AddStyle(PdfStyles.ValidateWarningTitle, PdfStyles.Default).Font =
                new Font() { Size = Unit.FromPoint(8), Italic = true, Color = PdfColors.Warning };
            document.Styles.AddStyle(PdfStyles.ValidateWarningMessage, PdfStyles.Default).Font =
                new Font() { Size = Unit.FromPoint(8), Color = PdfColors.Warning };

            var commentTitle = document.Styles.AddStyle(PdfStyles.CommentTitle, PdfStyles.Default);
            commentTitle.Font.Size = Unit.FromPoint(7);
            commentTitle.ParagraphFormat.LeftIndent = Unit.FromPoint(20);
            commentTitle.ParagraphFormat.SpaceBefore = Unit.FromPoint(5);

            var commentAuthor = document.Styles.AddStyle(PdfStyles.CommentAuthor, PdfStyles.Default);
            commentAuthor.Font.Size = Unit.FromPoint(8);
            commentAuthor.Font.Color = PdfColors.Comment;
            commentAuthor.Font.Italic = true;
            //commentAuthor.ParagraphFormat.LeftIndent = "0.5cm";

            var commentDateTime = document.Styles.AddStyle(PdfStyles.CommentDateTime, PdfStyles.Default);
            commentDateTime.Font.Size = Unit.FromPoint(8);
            commentDateTime.Font.Color = PdfColors.Comment;
            commentDateTime.Font.Italic = true;
            commentDateTime.ParagraphFormat.LeftIndent = Unit.FromPoint(20);

            var commentMessage = document.Styles.AddStyle(PdfStyles.CommentMessage, PdfStyles.Default);
            commentMessage.Font.Size = Unit.FromPoint(8);

            var yesNoTitle = document.Styles.AddStyle(PdfStyles.YesNoTitle, PdfStyles.Default);
            yesNoTitle.Font.Size = Unit.FromPoint(8);
            
            var headerLineTitle = document.Styles.AddStyle(PdfStyles.HeaderLineTitle, PdfStyles.Default);
            headerLineTitle.Font.Bold = true;
            headerLineTitle.Font.Size = Unit.FromPoint(7);
            headerLineTitle.ParagraphFormat.SpaceAfter = Unit.FromPoint(1);

            var headerDate = document.Styles.AddStyle(PdfStyles.HeaderLineDate, PdfStyles.Default);
            headerDate.Font.Bold = true;
            headerDate.Font.Size = Unit.FromPoint(12);

            var headerTime = document.Styles.AddStyle(PdfStyles.HeaderLineTime, PdfStyles.Default);
            headerTime.Font.Size = Unit.FromPoint(12);
        }
    }
}