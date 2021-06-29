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
            selector: 'fancytree-title, .tree-icon',
            menu: function (node) {
                if (node.isLazy())
                    node.load();

                if (node.key == "root")
                    return { "createSubFolder": { "name": localization.CreateSubFolder, "icon": "add" } };

                if (removeNodeUrl == null)
                {
                    return {
                        "createSubFolder": { "name": localization.CreateSubFolder, "icon": "add" },
                        "edit": { "name": localization.Edit, "icon": "edit" }
                    };
                }

                return {
                    "createSubFolder": { "name": localization.CreateSubFolder, "icon": "add" },
                    "edit": { "name": localization.Edit, "icon": "edit" },
                    "delete": { "name": localization.Delete, "icon": "delete" }
                };
            },
            actions: function (node, action, options) {
                if (action === "createSubFolder") {
                    var defaultFolderName = localization.NewFolderName;
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
                    var message = localization.DeleteConfirmation.replace("{0}", "<b>" + node.title + "</b>");
                    bootbox.confirm(message, function (result) {
                        if (result)
                        {
                            self.postRequest(removeNodeUrl, { 'id': node.key }, function () {
                                node.remove();
                            });
                        }
                    });
                } 
            }
        },
        edit: {
            triggerStart: ["f2", "shift+click", "mac+enter"],
            beforeEdit: function (event, data) {
                // Return false to prevent edit mode
                if (data.node.key === "root")
                    return false;
            },
            edit: function (event, data) {
                data.input.select();
            },
            beforeClose: function (event, data) {
                // Return false to prevent cancel/save (data.input is available)
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
        $.post({url: url, data: params, headers: { 'X-CSRF-TOKEN': getCsrfCookie()}}).done(callback);
    };
}

