using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.CodeGenerationV2;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.Questionnaire.Documents;
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
            var entityName = !string.IsNullOrEmpty(entity.VariableName) ? entity.VariableName : entity.GetTitle();
            result += $"{Environment.NewLine} context (entity name):'{entityName}'";
        }

        // Build path to target entity
        var pathToEntity = FindPathToEntity(questionnaireDocument, entityId);
        
        // Precompute requested groups and their ancestor chain to ensure off-path branches are included
        var (requestedGroupIds, requestedAncestors) = FindRequestedGroupsWithAncestors(questionnaireDocument, loadGroups);
        
        var simplifiedTree = BuildOptimizedTree(questionnaireDocument, pathToEntity, entityId, requestedGroupIds, requestedAncestors);
        var json = JsonConvert.SerializeObject(simplifiedTree, Formatting.Indented);

        result += $"{Environment.NewLine} context (questionnaire):{json}";
        
        return result;
    }

    private QuestionnaireTreeNode BuildOptimizedTree(QuestionnaireDocument document, 
        List<Guid> pathToEntity, Guid targetEntityId, HashSet<Guid> requestedGroupIds, HashSet<Guid> requestedAncestors)
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
            node.Children = BuildOptimizedChildren(relevantChildren, pathToEntity, targetEntityId, 0,
                requestedGroupIds, requestedAncestors,
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
        HashSet<Guid> requestedGroupIds,
        HashSet<Guid> requestedAncestors,
        ReadOnlyQuestionnaireDocument questionnaireDocument,
        bool forceFullExpansion = false)
    {
        var result = new List<QuestionnaireTreeNode>();
        
        foreach (var child in children)
        {
            var childId = child.PublicKey;
            var isInPath = pathToEntity.Contains(childId);
            var isTarget = childId == targetEntityId;
            var isTargetSibling = pathToEntity.Any() && pathToEntity.Last() != childId && 
                                  children.Any(c => c.PublicKey == pathToEntity.Last());
            
            var isRequestedGroup = requestedGroupIds.Contains(childId);
            var isRequestedAncestor = requestedAncestors.Contains(childId);
            
            // If we are forcing full expansion of this subtree, include all children regardless of path/request
            var shouldInclude = forceFullExpansion || currentDepth == 0 || isInPath || isTarget || isRequestedGroup || isRequestedAncestor ||
                               (isTargetSibling && pathToEntity.Count > 0 && 
                                children.Any(c => c.PublicKey == pathToEntity[^1]));
            
            if (shouldInclude)
            {
                // Include children if forceFullExpansion OR this node is along any included branch
                var includeChildren = forceFullExpansion || isInPath || isTarget || isRequestedGroup || isRequestedAncestor;
                
                // Force include if this composite's variable name directly matches any requested group name (case-insensitive)
                if (!includeChildren && child is Group g && !string.IsNullOrEmpty(g.VariableName))
                {
                    includeChildren = _requestedGroupNames?.Contains(g.VariableName.ToLowerInvariant()) == true;
                }
                
                // If this specific node is explicitly requested by name or id, propagate full expansion to its subtree
                var propagateFullExpansion = forceFullExpansion || isRequestedGroup ||
                    (child is Group g2 && !string.IsNullOrEmpty(g2.VariableName) &&
                     _requestedGroupNames?.Contains(g2.VariableName.ToLowerInvariant()) == true);
                
                var treeNode = BuildOptimizedNode(child, pathToEntity, targetEntityId, currentDepth + 1, 
                    includeChildren, requestedGroupIds, requestedAncestors, questionnaireDocument, propagateFullExpansion);
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
        HashSet<Guid> requestedGroupIds,
        HashSet<Guid> requestedAncestors,
        ReadOnlyQuestionnaireDocument questionnaireDocument,
        bool forceFullExpansion = false)
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
            
            // If this group's variable name is explicitly requested, force expansion
            var isExplicitlyRequestedByName = !string.IsNullOrEmpty(group.VariableName) &&
                                              _requestedGroupNames?.Contains(group.VariableName.ToLowerInvariant()) == true;
            var expandChildren = forceFullExpansion || includeChildren || isExplicitlyRequestedByName;
            
            if (expandChildren && relevantChildren.Any())
            {
                treeNode.Children = BuildOptimizedChildren(relevantChildren, pathToEntity, targetEntityId, currentDepth, 
                    requestedGroupIds, requestedAncestors, questionnaireDocument, forceFullExpansion || isExplicitlyRequestedByName);
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

    // Find all groups by variable names in loadGroups and collect their ancestor chain up to the questionnaire root
    private (HashSet<Guid> requestedGroupIds, HashSet<Guid> requestedAncestors) FindRequestedGroupsWithAncestors(
        QuestionnaireDocument document, List<string> loadGroups)
    {
        var requestedGroupIds = new HashSet<Guid>();
        var requestedAncestors = new HashSet<Guid>();
        
        // If there are no requested groups, nothing to collect
        if (loadGroups.Count == 0)
            return (requestedGroupIds, requestedAncestors);
        
        // Normalize requested names to lowercase for case-insensitive matching
        _requestedGroupNames = new HashSet<string>(loadGroups.Select(n => n.Trim().ToLowerInvariant()));
        
        // DFS: track path stack to add ancestors when we hit a requested group
        var pathStack = new Stack<IComposite>();
        
        void Dfs(IEnumerable<IComposite> children)
        {
            foreach (var child in children)
            {
                pathStack.Push(child);
                
                if (child is Group g)
                {
                    // If this group's variable name is requested, mark it and all ancestors
                    if (!string.IsNullOrEmpty(g.VariableName) && _requestedGroupNames.Contains(g.VariableName.ToLowerInvariant()))
                    {
                        requestedGroupIds.Add(g.PublicKey);
                        foreach (var ancestor in pathStack)
                        {
                            requestedAncestors.Add(ancestor.PublicKey);
                        }
                    }
                    
                    // Continue traversal
                    Dfs(g.Children);
                }
                else if (child is IQuestion || child is Variable)
                {
                    // no-op, still need to maintain stack for ancestor tracking
                }
                
                pathStack.Pop();
            }
        }
        
        Dfs(document.Children);
        
        return (requestedGroupIds, requestedAncestors);
    }
    
    // Holds normalized requested group names for direct variable-name matching during traversal
    private HashSet<string>? _requestedGroupNames;
}
