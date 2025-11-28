using System;
using System.Collections.Generic;
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
    public string GetQuestionnaireContext(Guid questionnaireId, Guid entityId)
    {
        var questionnaireDocument = questionnaireStorage.Get(questionnaireId);
        if (questionnaireDocument == null)
            throw new ArgumentException($"Questionnaire with id {questionnaireId} not found");
        
        // Build path to target entity
        var pathToEntity = FindPathToEntity(questionnaireDocument, entityId);
        
        var simplifiedTree = BuildOptimizedTree(questionnaireDocument, pathToEntity, entityId);
        var json = JsonConvert.SerializeObject(simplifiedTree, Formatting.Indented);

        return $"Context for questionnaire: {json}";
    }

    private QuestionnaireTreeNode BuildOptimizedTree(QuestionnaireDocument document, 
        List<Guid> pathToEntity, Guid targetEntityId)
    {
        var node = new QuestionnaireTreeNode
        {
            VariableName = document.VariableName,
            Title = document.Title,
            Type = "Questionnaire"
        };

        var relevantChildren = document.Children
            .Where(child => child is not StaticText)
            .ToList();

        if (relevantChildren.Any())
        {
            node.Children = BuildOptimizedChildren(relevantChildren, pathToEntity, targetEntityId, 0);
            node.HasOmittedChildren = relevantChildren.Count > (node.Children?.Count ?? 0);
        }

        return node;
    }
    
    private List<QuestionnaireTreeNode>? BuildOptimizedChildren(
        List<IComposite> children, 
        List<Guid> pathToEntity, 
        Guid targetEntityId,
        int currentDepth)
    {
        var result = new List<QuestionnaireTreeNode>();
        
        foreach (var child in children)
        {
            var childId = child.PublicKey;
            var isInPath = pathToEntity.Contains(childId);
            var isTarget = childId == targetEntityId;
            var isTargetSibling = pathToEntity.Any() && pathToEntity.Last() != childId && 
                                  children.Any(c => c.PublicKey == pathToEntity.Last());
            
            var shouldInclude = currentDepth == 0 || isInPath || isTarget || 
                               (isTargetSibling && pathToEntity.Count > 0 && 
                                children.Any(c => c.PublicKey == pathToEntity[^1]));
            
            if (shouldInclude)
            {
                var includeChildren = isInPath || isTarget;
                
                var treeNode = BuildOptimizedNode(child, pathToEntity, targetEntityId, currentDepth + 1, 
                    includeChildren);
                if (treeNode != null)
                {
                    result.Add(treeNode);
                }
            }
        }
        
        return result.Any() ? result : null;
    }

    private QuestionnaireTreeNode? BuildOptimizedNode(IComposite node, 
        List<Guid> pathToEntity, 
        Guid targetEntityId,
        int currentDepth,
        bool includeChildren)
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
            
            var relevantChildren = group.Children
                .Where(child => child is not StaticText)
                .ToList();
            
            if (includeChildren && relevantChildren.Any())
            {
                treeNode.Children = BuildOptimizedChildren(relevantChildren, pathToEntity, targetEntityId, currentDepth);
                treeNode.HasOmittedChildren = relevantChildren.Count > (treeNode.Children?.Count ?? 0);
            }
            else if (relevantChildren.Any())
            {
                treeNode.HasOmittedChildren = true;
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

    private List<Guid> FindPathToEntity(QuestionnaireDocument document, Guid targetEntityId)
    {
        var path = new List<Guid>();
        FindPathRecursive(document.Children, targetEntityId, path);
        return path;
    }
    
    private bool FindPathRecursive(IEnumerable<IComposite> children, Guid targetId, List<Guid> currentPath)
    {
        foreach (var child in children)
        {
            currentPath.Add(child.PublicKey);
            
            if (child.PublicKey == targetId)
                return true;
            
            if (child is Group group)
            {
                if (FindPathRecursive(group.Children, targetId, currentPath))
                    return true;
            }
            
            currentPath.RemoveAt(currentPath.Count - 1);
        }
        
        return false;
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
            QuestionType.GpsCoordinates => "GeoPosition",
            QuestionType.TextList => "string[]",
            QuestionType.QRBarcode => "string",
            QuestionType.Multimedia => "string",
            QuestionType.Area => "Area",
            QuestionType.Audio => "string",
            _ => throw new ArgumentOutOfRangeException(nameof(questionType), questionType, null)
        };
    }
}
