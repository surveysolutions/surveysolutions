function FlowGraph(parentKey){ 
	this.Blocks=[];
	this.Connections=[];
	this.ParentPublicKey = parentKey;
} 
function FlowConnection(){ 
	this.Source="";
	this.Target="";
    this.LabelText = "";
    this.Condition = "";
} 
function FlowBlock(){
    this.Height = 0;
    this.Width = 0;
    this.Left = 0;
    this.Top = 0;
    this.QuestionId = "";
} 


(function () {
    
    function Edge(v1, v2) {
        this.v1 = v1;
        this.v2 = v2;
    }

    function Graph() {
        this.V = {};
        this.E = [];
    }

    Graph.prototype.getFirstVertex = function () {
        for (var v in this.V) {
            return v;
        }
    };

    Graph.prototype.addEdge = function (v1, v2, weight) {
        if (!this.V[v1])
            this.V[v1] = {};
        if (!this.V[v2])
            this.V[v2] = {};
        this.V[v1][v2] = weight;
        this.E.push(new Edge(v1, v2));
    };

    Graph.prototype.explore = function (v, proc, prefunc, postfunc) {
        var visited = {};
        var graph = this;
        function helper(v) {
            prefunc(v);
            visited[v] = true;
            proc(v);
            $.each(graph.V[v], function (k, v) {
                if (!visited[k])
                    helper(k);
            });
            postfunc(v);
        }
        helper(v);
    };

    Graph.prototype.hasCicle = function () {
        var pre = {};
        var post = {};
        var ccn = 1;
        function previsit(v) {
            pre[v] = ccn;
            ccn++;
        }
        function postvisit(v) {
            post[v] = ccn;
            ccn++;
        }
        function action(v) {
        }
        //We explore using dfs and mark Pre and Post number for each vertex
        this.explore(this.getFirstVertex(), action, previsit, postvisit);

        var hasCicle = false;
        //We check back edge to see if this graph is acyclic
        $.each(this.E, function (i, e) {
            var v1 = e.v1;
            var v2 = e.v2;
            if (pre[v1] > pre[v2] && post[v2] > post[v1]) {
                hasCicle = true;
            }
        });
        return hasCicle;
    };

    Graph.prototype.topologicalSort = function () {
        var pre = {};
        var post = {};
        var ccn = 1;

        function previsit(v) {
            pre[v] = ccn;
            ccn++;
        }
        function postvisit(v) {
            post[v] = ccn;
            ccn++;
        }
        function action(v) {

        }

        this.explore(this.getFirstVertex(), action, previsit, postvisit);

        var linearizedSequence = [];
        $.each(post, function (k, v) {
            linearizedSequence.push(k);
        });

        linearizedSequence.sort(function (a, b) {
            return post[a] < post[b];
        });

        var result = [];
        $.each(linearizedSequence, function (i, v) {
            result.push(v);
        });
        return result;
    };


    window.jsPlumbDemo = {
        labelTexts: [],
        labelConditions: [],
        init: function () {

            // notice the 'curviness' argument to this Bezier curve.  the curves on this page are far smoother
            // than the curves on the first demo, which use the default curviness value.
            jsPlumb.Defaults.Endpoint = ["Dot", { radius: 2}];
            jsPlumb.Defaults.HoverPaintStyle = { strokeStyle: "#42a62c", lineWidth: 3 };
            jsPlumb.Defaults.EndpointStyle = { radius: 3, fillStyle: "#0069D6" };
            jsPlumb.Defaults.PaintStyle = { lineWidth: 3, strokeStyle: "#456" };
            jsPlumb.Defaults.Anchor = "Continuous";
            jsPlumb.Defaults.Connector = ["StateMachine", { curviness: 20}];

            jsPlumb.Defaults.Overlays = [
							["Arrow", {
							    location: 1,
							    id: "arrow"
							}],
							["Label", {
							    location: 0.5,
							    id:"label",
							    cssClass: "aLabel"
							}]
						];
            init = function(connection) {
                var label = connection.getOverlay("label");
                if (! $('#'+connection.sourceId).hasClass("group")) {
                    label.setLabel("No condition");
                    label.canvas.classList.add("initialized");
                }
            };			

			jsPlumb.bind("jsPlumbConnection", function(connInfo, originalEvent) { 
				init(connInfo.connection);
			});
            
            jsPlumbDemo.initEndpoints();

            jsPlumbDemo.initConnections();

            $(".w").each(function () {
                var e = $(this);

                var scope = e.attr("scope");

                e.resizable({ containment: "#" + scope });

                var dragoptions = {
                    containment: "#" + scope,
                    scroll: false,
                    scope: scope,
                    start: function (event, ui) {
                        $(this).data("startPosition", $(this).position());
                    }
                };
                jsPlumb.draggable(e, dragoptions);
            });

            $("#canvas").resizable({
                handles: 's',
                stop: function(event, ui) {
                    $(this).css("width", '');
                }
            });


            jsPlumb.bind("dblclick", function (conn) {
                if (confirm("Delete connection from?"))
                    jsPlumb.detach(conn);
            });
            
            
        },
        initEndpoints: function () {
            $(".ep").each(function (i, e) {
                var p = $(e).parent();
                var s = $(p).attr('scope');
                jsPlumb.makeSource($(e), {
                    parent: p,
                    endpoint: {
                        maxConnections: -1,
                        scope: s
                    },
                    scope: s
                });
            });
            $(".w").each(function (i, e) {
                var s = $(e).attr('scope');
                jsPlumb.makeTarget($(e), {
                    dropOptions: { hoverClass: "dragHover" },
                    endpoint: {
                        anchor: "Continuous",
                        scope: s
                    },
                    scope: s
                });
            });
        },
        getAllFlowGraphs: function () {
            var result = [];
            var scopes = [];
            $(".w").each(function (i, e) {
                var s = $(e).attr('scope');
                if (jQuery.inArray(s, scopes) == -1) {
                    scopes.push(s);
                }
            });
            var connections = this.getDistinctConnections();
            $.each(scopes, function (i, scope) {
                var graph = new FlowGraph(scope == "canvas" ? null : scope);
                var conn = connections[scope];
                var ids = [];
                if ((conn != null) && (conn.length > 0))
                {
                    var g = new Graph();
                    var value = null;
                    for (var i = 0, len = conn.length; value = conn[i], i < len; i++) {
                        graph.Connections.push(value);
                        g.addEdge(value.Source, value.Target, 1);
                    }
                    var orderedIds = g.topologicalSort();
                    $.each(orderedIds, function(j, id) {
                        graph.Blocks.push(getBlock(id));
                        ids.push(id);
                    });
                }
                $.each($('.w[scope=' + scope + ']'), function(j, block) {
                    var id = $(block).attr('id');
                    if (jQuery.inArray(id, ids) == -1) {
                        graph.Blocks.push(getBlock(id));
                        ids.push(id);
                    }
                });
                function getBlock(id) {
                    var block = $("#" + id);
                    var r = new FlowBlock();
                    r.QuestionId = block.attr('id');
                    r.Left = block.css('left').replace("px", "") * 1;
                    r.Top = block.css('top').replace("px", "") * 1;
                    r.Width = block.outerWidth();
                    r.Height = block.outerHeight();
                    r.LabelText = "";
                    r.Condition = "";
                    return r;
                };
                result.push(graph);
            });
            return result;
        },
        getAllGraphs: function () {
            var connections = this.getDistinctConnections();
            var result = [];
            for (var key in connections) {
                var conn = connections[key];
                var g = new Graph();
                var block = null;
                for (var i = 0, len = conn.length; block = conn[i], i < len; i++) {
                    g.addEdge(block.Source, block.Target, 1);
                }
                result.push(g);
            }
            return result;
        },
        checkFlow: function () {
            var isOk = true;
            $('.w').removeClass('highlight-error');
            var graphs = this.getAllGraphs();
            var g = null;
            for (var i = 0, len = graphs.length; g = graphs[i], i < len; i++) {
                if (g.hasCicle() === true) {
                    $.jGrowl("Highlihted flow has cycle", { theme: 'alert-message error', sticky: true });
                    var scope = $("#" + g.getFirstVertex()).attr('scope');
                    $('.w[scope="' + scope + '"]').addClass('highlight-error');
                    isOk = false;
                }
            }
            return isOk;
        },
        getDistinctConnections: function () {
            var connections = jsPlumb.getAllConnections();
            var result = {};
            if (connections.length > 0) {
                var ids = [];
                key = $("#" + connections[0].sourceId).attr('scope');
                result[key] = [];
                for (var i = 0, len = connections.length; value = connections[i], i < len; i++) {
                    var r = getConnection(value);
                    if (jQuery.inArray(value.id, ids) == -1) {
                        result[key].push(r);
                        ids.push(value.id);
                    }
                }
            }
            else {
                for (var key in connections) {
                    var ids = [];
                    var conn = connections[key];
                    if (conn.length == 0)
                        continue;
                    result[key] = [];
                    for (var i = 0, len = conn.length; value = conn[i], i < len; i++) {
                        var r = getConnection(value);
                        if (jQuery.inArray(value.id, ids) == -1) {
                            result[key].push(r);
                            ids.push(value.id);
                        }
                    }
                }
            }
            function getConnection(jsConnection) {
                var c = new FlowConnection();
                var id = jsConnection.id;
                c.Source = jsConnection.sourceId;
                c.Target = jsConnection.targetId;
                c.LabelText = jsPlumbDemo.labelTexts[id];
                c.Condition = jsPlumbDemo.labelConditions[id];
                return c;
            }
            return result;
        },
        updateConnectionLabel: function (searchOption, text) {
            var connection = jsPlumb.getConnections(searchOption)[0];

            jsPlumbDemo.labelTexts[connection.id] = "=="+"'"+text+"'";
            jsPlumbDemo.labelConditions[connection.id] = "["+searchOption.source+"]=="+"'"+text+"'";

            if (connection != null) {
                var label = connection.getOverlay("label");
                label.setLabel(jsPlumbDemo.labelTexts[connection.id]);
                label.canvas.classList.add("initialized");
                
                jsPlumb.repaintEverything();
            }
        }
    };
})();

jsPlumb.bind("ready", function () {

    // chrome fix.
    document.onselectstart = function () { return false; };

    // render mode
    var resetRenderMode = function (desiredMode) {
        var newMode = jsPlumb.setRenderMode(desiredMode);
        $(".rmode").removeClass("selected");
        $(".rmode[mode='" + newMode + "']").addClass("selected");
        var disableList = (newMode === jsPlumb.VML) ? ".rmode[mode='canvas'],.rmode[mode='svg']" : ".rmode[mode='vml']";
        $(disableList).attr("disabled", true);
        jsPlumbDemo.init();
    };

    $(".rmode").bind("click", function () {
        var desiredMode = $(this).attr("mode");
        if (jsPlumbDemo.reset) jsPlumbDemo.reset();
        jsPlumb.reset();
        resetRenderMode(desiredMode);
    });


    resetRenderMode(jsPlumb.CANVAS);
});