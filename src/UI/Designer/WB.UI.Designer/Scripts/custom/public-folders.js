function FoldersModel(element, rootNodesUrl, subNodesUrl, addNodeUrl, renameNodeUrl, removeNodeUrl) {
    var self = this;

    var glyph_opts = {
        preset: "bootstrap3",
        map: {
            expanderClosed: "glyphicon glyphicon-menu-right",  // glyphicon-plus-sign
            expanderLazy: "glyphicon glyphicon-menu-right",  // glyphicon-plus-sign
            expanderOpen: "glyphicon glyphicon-menu-down"  // glyphicon-minus-sign
        }
    };

    element.fancytree({
        extensions: ["contextMenu", "edit", "glyph"],
        icon: true,
        glyph: glyph_opts,
        selectMode: 1,
        /*wide: {
            iconWidth: "1em",       // Adjust this if fancy-icon-width != "16px"
            iconSpacing: "0.5em",   // Adjust this if fancy-icon-spacing != "3px"
            labelSpacing: "0.1em",  // Adjust this if padding between icon and label != "3px"
            levelOfs: "1.5em"       // Adjust this if ul padding != "16px"
        },*/
        /*icon: function (event, data) {
            if (data.node.isFolder()) {
                return "glyphicon glyphicon-book";
            }
        },*/
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
        contextMenu: {
            menu: function (node) {
                if (node.key === "00000000-0000-0000-0000-000000000000")
                    return { "createSubFolder": { "name": "Create Folder", "icon": "add" } };

                return {
                    "createSubFolder": { "name": "Create Folder", "icon": "add" },
                    "edit": { "name": "Edit", "icon": "edit" },
                    "delete": { "name": "Delete", "icon": "delete" }
                };
            },
            actions: function (node, action, options) {
                if (action === "createSubFolder") {
                    var defaultFolderName = "New folder";
                    self.postRequest(addNodeUrl, { 'parentId': node.key, 'title': defaultFolderName }, function (data) {
                        node.editCreateNode("child", {
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
                if (data.node.key === "00000000-0000-0000-0000-000000000000")
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
}

