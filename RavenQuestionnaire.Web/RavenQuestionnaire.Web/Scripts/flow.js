(function () {



    window.jsPlumbDemo = {
        labelTexts: [],
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
							    label: function (label) {
							        return label.connection.labelText || "";
							    },
							    cssClass: "aLabel"
							}]
						];


            init = function (connection) {
                var p = { Target: connection.targetId, Condition: "No condition" };
                connection.labelText = $("#action" + connection.sourceId).tmpl(p).html();
            };
            jsPlumb.bind("jsPlumbConnection", function (connInfo) {
                init(connInfo.connection);
            });

            jsPlumbDemo.initEndpoints();

            jsPlumbDemo.initConnections();


            var dragoptions = {
                containment: "#" + "canvas",
                scroll: false,
                scope: "foo".concat(new String("canvas")),
                start: function (event, ui) {
                    $(this).data("startPosition", $(this).position());
                }
            };

            jsPlumb.draggable(jsPlumb.getSelector(".w"), dragoptions);


            jsPlumb.bind("dblclick", function (conn) {
                if (confirm("Delete connection from?"))
                    jsPlumb.detach(conn);
            });
        },
        initEndpoints: function () {
            $(".ep").each(function (i, e) {
                var p = $(e).parent();
                jsPlumb.makeSource($(e), {
                    parent: p,
                    endpoint: {
                        maxConnections: -1
                    }
                });
            });

            jsPlumb.makeTarget($(".w"), {
                dropOptions: { hoverClass: "dragHover" },
                endpoint: {
                    anchor: "Continuous"
                }
            });
        },
        getAllConnections: function () {
            var result = [];
            var ids = [];
            var connections = jsPlumb.getConnections();
            for (var i = 0, len = connections.length; value = connections[i], i < len; i++) {
                var r = {};
                var id = value.id;
                r.Source = value.sourceId;
                r.Target = value.targetId;
                r.LabelText = jsPlumbDemo.labelTexts[id];
                if (jQuery.inArray(id, ids) == -1) {
                    result.push(r);
                    ids.push(id);
                }
            }
            return result;
        },
        getAllBlocks: function () {
            var result = [];
            $.each($('.w'), function (index, block) {
                var r = {};
                r.QuestionId = $(block).attr('id');
                r.Left = $(block).css('left').replace("px", "");
                r.Top = $(block).css('top').replace("px", "");
                r.LabelText = "";
                result.push(r);
            });
            return result;
        },
        updateConnectionLabel: function (searchOption, expression) {
            var connection = jsPlumb.getConnections(searchOption)[0];

            jsPlumbDemo.labelTexts[connection.id] = expression;

            if (connection != null) {
                var p = { Target: searchOption.target, Condition: expression };
                connection.labelText = $("#action" + searchOption.source).tmpl(p).html();
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

