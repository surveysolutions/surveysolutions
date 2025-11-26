using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public class QuestionnaireContextProvider(IDesignerQuestionnaireStorage questionnaireStorage) 
    : IQuestionnaireContextProvider
{
    public string GetQuestionnaireContext(Guid questionnaireId)
    {
        var questionnaireDocument = questionnaireStorage.Get(questionnaireId);
        if (questionnaireDocument == null)
            throw new ArgumentException($"Questionnaire with id {questionnaireId} not found");
        
        var simplifiedTree = BuildSimplifiedTree(questionnaireDocument);
        var json = JsonConvert.SerializeObject(simplifiedTree, Formatting.Indented);

        return $"Context for questionnaire: {json}";
    }

    private QuestionnaireTreeNode BuildSimplifiedTree(QuestionnaireDocument document)
    {
        return new QuestionnaireTreeNode
        {
            VariableName = document.VariableName,
            Title = document.Title,
            Type = "Questionnaire",
            Children = document.Children
                .Where(child => child is not StaticText)
                .Select(child => BuildSimplifiedNode(child))
                .Where(node => node != null)
                .ToList()!
        };
    }

    private QuestionnaireTreeNode? BuildSimplifiedNode(IComposite node)
    {
        // Skip StaticText nodes
        if (node is StaticText)
            return null;

        var treeNode = new QuestionnaireTreeNode
        {
            VariableName = node.VariableName,
            Type = GetNodeType(node)
        };

        // Add title based on node type
        if (node is Group group)
        {
            treeNode.Title = group.Title;
            
            if (group.Children.Any())
            {
                treeNode.Children = group.Children
                    .Where(child => child is not StaticText)
                    .Select(child => BuildSimplifiedNode(child))
                    .Where(childNode => childNode != null)
                    .ToList()!;
            }
        }
        else if (node is IQuestion question)
        {
            treeNode.Title = question.QuestionText ?? string.Empty;
        }
        else if (node is Variable variable)
        {
            treeNode.Title = variable.Label;
        }

        return treeNode;
    }

    private string GetNodeType(IComposite node)
    {
        return node switch
        {
            Group g when g.IsRoster => "Roster",
            Group => "Group",
            IQuestion q => MapQuestionTypeToCSharpType(q),
            Variable => "Variable",
            _ => node.GetType().Name
        };
    }

    private static string MapQuestionTypeToCSharpType(IQuestion question)
    {
        var questionType = question.QuestionType;
        return questionType switch
        {
            QuestionType.Text => "string",
            QuestionType.Numeric => (question as INumericQuestion)?.IsInteger == true ? "int?" : "double?",
            QuestionType.DateTime => "DateTime?",
            QuestionType.SingleOption => "int?",
            QuestionType.MultyOption => "int[]",
            QuestionType.YesNo => "bool[]",
            QuestionType.GpsCoordinates => "GeoPosition",
            QuestionType.TextList => "string[]",
            QuestionType.QRBarcode => "string",
            QuestionType.Multimedia => "string",
            QuestionType.Area => "Area",
            QuestionType.Audio => "string",
            QuestionType.AutoPropagate => "int?",
            _ => questionType.ToString()
        };
    }
}
