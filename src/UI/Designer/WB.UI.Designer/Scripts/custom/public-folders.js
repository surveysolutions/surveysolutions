function FoldersModel(element, localization, rootNodesUrl, subNodesUrl, addNodeUrl, renameNodeUrl, removeNodeUrl, supportRadioButton, selectFolderCallback) {
    var self = this;

    var glyph_opts = {
        preset: "bootstrap3",
        map: {
            expanderClosed: "glyphicon glyphicon-menu-right",  // glyphicon-plus-sign
            expanderLazy  : "glyphicon glyphicon-menu-right",  // glyphicon-plus-sign
            expanderOpen  : "glyphicon glyphicon-menu-down",  // glyphicon-minus-sign
            folder        : "tree-icon folder-closed",
            folderOpen    : "tree-icon folder-open",
            checkbox      : "tree-checkbox-unchecked",
            checkboxSelected: "tree-checkbox-checked"
        }
    };

    element.fancytree({
        extensions: ["contextMenu", "edit", "glyph"],
        checkbox: supportRadioButton ? "radio" : false,
        glyph: glyph_opts,
        selectMode: 1,
        icon: true,
        source: {
            url: rootNodesUrl,
            cache: false
        },
        lazyLoad: function (event, data) {
            var node = data.node;
            // Issue an ajax request to load child nodes
            data.result = {
                url: subNodesUrl,
                data: { parentId: node.key }
            }
        },
        select: function (event, data) {
            if (selectFolderCallback) {
                var nodeKey = data.node.isSelected() ? data.node.key : null;
                selectFolderCallback(nodeKey);
            }
        },
        contextMenu: {
            menu: function (node) {
                if (node.isLazy())
                    node.load();

                if (node.key == "root")
                    return { "createSubFolder": { "name": localization.CreateSubFolder, "icon": "add" } };

                return {
                    "createSubFolder": { "name": localization.CreateSubFolder, "icon": "add" },
                    "edit": { "name": localization.Edit, "icon": "edit" },
                    "delete": { "name": localization.Delete, "icon": "delete" }
                };
            },
            actions: function (node, action, options) {
                if (action === "createSubFolder") {
                    var defaultFolderName = "New folder";
                    self.postRequest(addNodeUrl, { 'parentId': node.key, 'title': defaultFolderName }, function (data) {
                        node.editCreateNode("child",
                            {
                                key: data.key,
                                title: data.title,
                                folder: true
                            });
                    });
                } else if (action === "edit") {
                    node.editStart();
                } else if (action === "delete") {
                    self.postRequest(removeNodeUrl, { 'id': node.key }, function() {
                        node.remove();
                    });
                } 
            }
        },
        edit: {
            triggerStart: ["f2", /*"dblclick",*/ "shift+click", "mac+enter"],
            beforeEdit: function (event, data) {
                // Return false to prevent edit mode
                if (data.node.key === "root")
                    return false;
            },
            edit: function (event, data) {
                // Editor was opened (available as data.input)
            },
            beforeClose: function (event, data) {
                // Return false to prevent cancel/save (data.input is available)
                console.log(event.type, event, data);
                if (data.originalEvent.type === "mousedown") {
                    // We could prevent the mouse click from generating a blur event
                    // (which would then again close the editor) and return `false` to keep
                    // the editor open:
                    //                  data.originalEvent.preventDefault();
                    //                  return false;
                    // Or go on with closing the editor, but discard any changes:
                    //                  data.save = false;
                }
            },
            save: function (event, data) {
                var newTitle = data.input.val();
                self.postRequest(renameNodeUrl, { "id": data.node.key, "newTitle": newTitle }, function() {
                    $(data.node.span).removeClass("pending");
                    data.node.parent.sortChildren(null, false);
                });
                return true;
            },
            close: function (event, data) {
                // Editor was removed
                if (data.save) {
                    // Since we started an async request, mark the node as preliminary
                    $(data.node.span).addClass("pending");
                }
            }
        }
    });

    self.postRequest = function(url, params, callback) {
        $.post({
            url: url,
            data: params,
            success: callback,
            dataType: 'json'
        });
    }

    self.getSelectedFolderId = function() {
        return element.fancytree('getTree').getSelectedNodes();
    }

    self.setSelectedFolderId = function(folderId) {
        var tree = element.fancytree('getTree');
        var node = tree.getNodeByKey(folderId);
        node.setSelected(true);
    }
}

