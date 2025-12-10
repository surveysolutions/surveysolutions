using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Assistant;

public class QuestionnaireContextProvider(IDesignerQuestionnaireStorage questionnaireStorage,
    IQuestionTypeToCSharpTypeMapper questionTypeToCSharpTypeMapper) 
    : IQuestionnaireContextProvider
{
    public string GetQuestionnaireContext(Guid questionnaireId, Guid entityId, List<string>? loadGroups = null)
    {
        string result = string.Empty;
        
        loadGroups ??= new List<string>();
        
        var questionnaireDocument = questionnaireStorage.Get(questionnaireId);
        if (questionnaireDocument == null)
            throw new ArgumentException($"Questionnaire with id {questionnaireId} not found");
        
        var entity = questionnaireDocument.Find<IComposite>(entityId);
        if (entity != null)
        {
            result = result + $"context (entity name):'{entity.VariableName}'";
        }

        // Build path to target entity
        var pathToEntity = FindPathToEntity(questionnaireDocument, entityId);
        
        var simplifiedTree = BuildOptimizedTree(questionnaireDocument, pathToEntity, entityId, loadGroups);
        var json = JsonConvert.SerializeObject(simplifiedTree, Formatting.Indented);

        result = result + $"context (questionnaire):{json}";
        
        return result;
    }

    private QuestionnaireTreeNode BuildOptimizedTree(QuestionnaireDocument document, 
        List<Guid> pathToEntity, Guid targetEntityId, List<string> loadGroups)
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
            node.Children = BuildOptimizedChildren(relevantChildren, pathToEntity, targetEntityId, 0, loadGroups,
                new ReadOnlyQuestionnaireDocument(document));
            node.HasOmittedChildren = relevantChildren.Count > (node.Children?.Count ?? 0);
        }

        return node;
    }
    
    private List<QuestionnaireTreeNode>? BuildOptimizedChildren(
        List<IComposite> children, 
        List<Guid> pathToEntity, 
        Guid targetEntityId,
        int currentDepth,
        List<string> loadGroups,
        ReadOnlyQuestionnaireDocument questionnaireDocument)
    {
        var result = new List<QuestionnaireTreeNode>();
        
        foreach (var child in children)
        {
            var childId = child.PublicKey;
            var isInPath = pathToEntity.Contains(childId);
            var isTarget = childId == targetEntityId;
            var isTargetSibling = pathToEntity.Any() && pathToEntity.Last() != childId && 
                                  children.Any(c => c.PublicKey == pathToEntity.Last());
            
            var isRequestedForLoad = !string.IsNullOrEmpty(child.VariableName) && 
                                     loadGroups.Contains(child.VariableName);
            
            var shouldInclude = currentDepth == 0 || isInPath || isTarget || isRequestedForLoad ||
                               (isTargetSibling && pathToEntity.Count > 0 && 
                                children.Any(c => c.PublicKey == pathToEntity[^1]));
            
            if (shouldInclude)
            {
                var includeChildren = isInPath || isTarget || isRequestedForLoad;
                
                var treeNode = BuildOptimizedNode(child, pathToEntity, targetEntityId, currentDepth + 1, 
                    includeChildren, loadGroups, questionnaireDocument);
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
        bool includeChildren,
        List<string> loadGroups,
        ReadOnlyQuestionnaireDocument questionnaireDocument)
    {
        // Skip StaticText nodes
        if (node is StaticText)
            return null;

        var treeNode = new QuestionnaireTreeNode
        {
            VariableName = node.VariableName,
            Type = GetNodeType(node,questionnaireDocument)
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
                treeNode.Children = BuildOptimizedChildren(relevantChildren, pathToEntity, targetEntityId, currentDepth, loadGroups, questionnaireDocument);
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

    private string GetNodeType(IComposite node, ReadOnlyQuestionnaireDocument questionnaireDocument)
    {
        return node switch
        {
            Group g when g.IsRoster => "Roster",
            Group => "Group",
            IQuestion q => MapQuestionTypeToCSharpType(q, questionnaireDocument),
            Variable => "Variable",
            _ => node.GetType().Name
        };
    }

    private string MapQuestionTypeToCSharpType(IQuestion question, ReadOnlyQuestionnaireDocument questionnaireDocument)
    {
        return questionTypeToCSharpTypeMapper.GetQuestionType(question, questionnaireDocument);
    }
}
