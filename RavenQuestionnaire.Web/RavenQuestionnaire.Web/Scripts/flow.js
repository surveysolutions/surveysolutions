var conditions = [];

var addCondition = function(condition) {
    conditions.push(condition);
};

var parser = (function () {
    var parse = make_parse();

    function go(source) {
        var tree = null;
        try {
            tree = parse(source);
        } catch (e) {
        }
        return tree;
    }

    return {
        parse: go
    };
} ());

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
    this.PublicKey = "";
    this.IsQuestion = false;
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
                    r.PublicKey = block.attr('id');
                    r.Left = block.css('left').replace("px", "") * 1;
                    r.Top = block.css('top').replace("px", "") * 1;
                    r.Width = block.outerWidth();
                    r.Height = block.outerHeight();
                    r.LabelText = "";
                    r.Condition = "";
                    r.IsQuestion = block.hasClass("question");
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
        updateConnectionLabel: function (searchOption, text, num) {
            var connection = jsPlumb.getConnections(searchOption)[0];

            jsPlumbDemo.labelTexts[connection.id] = "=="+"'"+text+"'";
            jsPlumbDemo.labelConditions[connection.id] = "["+searchOption.source+"]=="+num;

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

// parse.js
// Parser for Simplified JavaScript written in Simplified JavaScript
// From Top Down Operator Precedence
// http://javascript.crockford.com/tdop/index.html
// Douglas Crockford
// 2010-06-26

var make_parse = function () {
    var scope;
    var symbol_table = {};
    var token;
    var tokens;
    var token_nr;

    var itself = function () {
        return this;
    };

    var original_scope = {
        define: function (n) {
            var t = this.def[n.value];
            if (typeof t === "object") {
                n.error(t.reserved ? "Already reserved." : "Already defined.");
            }
            this.def[n.value] = n;
            n.reserved = false;
            n.nud = itself;
            n.led = null;
            n.std = null;
            n.lbp = 0;
            n.scope = scope;
            return n;
        },
        find: function (n) {
            o = symbol_table[n];
            if (o)
                return symbol_table[n];

            o = symbol_table["(name)"]
            o.value = n;
            o.nud = itself;
            this.define(n);

            return o;
        },
        pop: function () {
            scope = this.parent;
        },
        reserve: function (n) {
            if (n.arity !== "name" || n.reserved) {
                return;
            }
            var t = this.def[n.value];
            if (t) {
                if (t.reserved) {
                    return;
                }
                if (t.arity === "name") {
                    n.error("Already defined.");
                }
            }
            this.def[n.value] = n;
            n.reserved = true;
        }
    };

    var new_scope = function () {
        var s = scope;
        scope = Object.create(original_scope);
        scope.def = {};
        scope.parent = s;
        return scope;
    };

    var advance = function (id) {
        var a, o, t, v;
        if (id && token.id !== id) {
            token.error("Expected '" + id + "'.");
        }
        if (token_nr >= tokens.length) {
            token = symbol_table["(end)"];
            return;
        }
        t = tokens[token_nr];
        token_nr += 1;
        v = t.value;
        a = t.type;
        if (a === "name") {
            o = scope.find(v);
        } else if (a === "operator") {
            o = symbol_table[v];
            if (!o) {
                t.error("Unknown operator.");
            }
        } else if (a === "string" || a === "number") {
            o = symbol_table["(literal)"];
            a = "literal";
        } else {
            t.error("Unexpected token.");
        }
        token = Object.create(o);
        token.from = t.from;
        token.to = t.to;
        token.value = v;
        token.arity = a;
        return token;
    };

    var expression = function (rbp) {
        var left;
        var t = token;
        advance();
        left = t.nud();
        while (rbp < token.lbp) {
            t = token;
            advance();
            left = t.led(left);
        }
        return left;
    };

    var statement = function () {
        var n = token, v;

        if (n.std) {
            advance();
            scope.reserve(n);
            return n.std();
        }
        v = expression(0);
        return v;
    };

    var statements = function () {
        var a = [], s;
        while (true) {
            if (token.id === "(end)") {
                break;
            }
            s = statement();
            if (s) {
                a.push(s);
            }
        }
        return a.length === 0 ? null : a.length === 1 ? a[0] : a;
    };

    var original_symbol = {
        nud: function () {
            this.error("Undefined.");
        },
        led: function (left) {
            this.error("Missing operator.");
        }
    };

    var symbol = function (id, bp) {
        var s = symbol_table[id];
        bp = bp || 0;
        if (s) {
            if (bp >= s.lbp) {
                s.lbp = bp;
            }
        } else {
            s = Object.create(original_symbol);
            s.id = s.value = id;
            s.lbp = bp;
            symbol_table[id] = s;
        }
        return s;
    };

    var constant = function (s, v) {
        var x = symbol(s);
        x.nud = function () {
            scope.reserve(this);
            this.value = symbol_table[this.id].value;
            this.arity = "literal";
            return this;
        };
        x.value = v;
        return x;
    };

    var infix = function (id, bp, led) {
        var s = symbol(id, bp);
        s.led = led || function (left) {
            this.first = left;
            this.second = expression(bp);
            this.arity = "binary";
            return this;
        };
        return s;
    };

    var infixr = function (id, bp, led) {
        var s = symbol(id, bp);
        s.led = led || function (left) {
            this.first = left;
            this.second = expression(bp - 1);
            this.arity = "binary";
            return this;
        };
        return s;
    };

    var prefix = function (id, nud) {
        var s = symbol(id);
        s.nud = nud || function () {
            scope.reserve(this);
            this.first = expression(70);
            this.arity = "unary";
            return this;
        };
        return s;
    };

    var stmt = function (s, f) {
        var x = symbol(s);
        x.std = f;
        return x;
    };

    symbol(")");
    symbol("(end)");
    symbol("(name)");
    symbol("(name)").nud = itself;
    symbol("(literal)").nud = itself;

    infixr("and", 30);
    infixr("or", 30);

    infixr("==", 40);
    infixr("!=", 40);
    infixr("<", 40);
    infixr("<=", 40);
    infixr(">", 40);
    infixr(">=", 40);

    infix("+", 50);
    infix("-", 50);

    infix("*", 60);
    infix("/", 60);

    prefix("(", function () {
        var e = expression(0);
        advance(")");
        return e;
    });

    return function (source) {
        tokens = source.tokens('=<>!+-*&|/%^', '=<>&|');
        token_nr = 0;
        new_scope();
        advance();
        var s = statements();
        advance("(end)");
        scope.pop();
        return s;
    };
};

// tokens.js
// 2010-02-23

// (c) 2006 Douglas Crockford

// Produce an array of simple token objects from a string.
// A simple token object contains these members:
//      type: 'name', 'string', 'number', 'operator'
//      value: string or number value of the token
//      from: index of first character of the token
//      to: index of the last character + 1

// Comments of the // type are ignored.

// Operators are by default single characters. Multicharacter
// operators can be made by supplying a string of prefix and
// suffix characters.
// characters. For example,
//      '<>+-&', '=>&:'
// will match any of these:
//      <=  >>  >>>  <>  >=  +: -: &: &&: &&



String.prototype.tokens = function (prefix, suffix) {
    var c;                      // The current character.
    var from;                   // The index of the start of the token.
    var i = 0;                  // The index of the current character.
    var length = this.length;
    var n;                      // The number value.
    var q;                      // The quote character.
    var str;                    // The string value.

    var result = [];            // An array to hold the results.

    var make = function (type, value) {

        // Make a token object.

        return {
            type: type,
            value: value,
            from: from,
            to: i
        };
    };

    // Begin tokenization. If the source string is empty, return nothing.

    if (!this) {
        return;
    }

    // If prefix and suffix strings are not provided, supply defaults.

    if (typeof prefix !== 'string') {
        prefix = '<>+-&';
    }
    if (typeof suffix !== 'string') {
        suffix = '=>&:';
    }


    // Loop through this text, one character at a time.

    c = this.charAt(i);
    while (c) {
        from = i;

        // Ignore whitespace.

        if (c <= ' ') {
            i += 1;
            c = this.charAt(i);

            // name.

        } else if (c == '[') {
            str = c;
            i += 1;
            for (; ; ) {
                c = this.charAt(i);
                if (c !== ']') {
                    str += c;
                    i += 1;
                } else {
                    str += c;
                    i += 1;
                    c = this.charAt(i);
                    break;
                }
            }
            result.push(make('name', str));

        } else if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) {
            str = c;
            i += 1;
            for (; ; ) {
                c = this.charAt(i);
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') ||
                        (c >= '0' && c <= '9') || c === '_') {
                    str += c;
                    i += 1;
                } else {
                    break;
                }
            }
            result.push(make('name', str));

            // number.

            // A number cannot start with a decimal point. It must start with a digit,
            // possibly '0'.

        } else if (c >= '0' && c <= '9') {
            str = c;
            i += 1;

            // Look for more digits.

            for (; ; ) {
                c = this.charAt(i);
                if (c < '0' || c > '9') {
                    break;
                }
                i += 1;
                str += c;
            }

            // Look for a decimal fraction part.

            if (c === '.') {
                i += 1;
                str += c;
                for (; ; ) {
                    c = this.charAt(i);
                    if (c < '0' || c > '9') {
                        break;
                    }
                    i += 1;
                    str += c;
                }
            }

            // Look for an exponent part.

            if (c === 'e' || c === 'E') {
                i += 1;
                str += c;
                c = this.charAt(i);
                if (c === '-' || c === '+') {
                    i += 1;
                    str += c;
                    c = this.charAt(i);
                }
                if (c < '0' || c > '9') {
                    make('number', str).error("Bad exponent");
                }
                do {
                    i += 1;
                    str += c;
                    c = this.charAt(i);
                } while (c >= '0' && c <= '9');
            }

            // Make sure the next character is not a letter.

            if (c >= 'a' && c <= 'z') {
                str += c;
                i += 1;
                make('number', str).error("Bad number");
            }

            // Convert the string value to a number. If it is finite, then it is a good
            // token.

            n = +str;
            if (isFinite(n)) {
                result.push(make('number', n));
            } else {
                make('number', str).error("Bad number");
            }

            // string

        } else if (c === '\'' || c === '"') {
            str = '';
            q = c;
            i += 1;
            for (; ; ) {
                c = this.charAt(i);
                if (c < ' ') {
                    make('string', str).error(c === '\n' || c === '\r' || c === '' ?
                        "Unterminated string." :
                        "Control character in string.", make('', str));
                }

                // Look for the closing quote.

                if (c === q) {
                    break;
                }

                // Look for escapement.

                if (c === '\\') {
                    i += 1;
                    if (i >= length) {
                        make('string', str).error("Unterminated string");
                    }
                    c = this.charAt(i);
                    switch (c) {
                        case 'b':
                            c = '\b';
                            break;
                        case 'f':
                            c = '\f';
                            break;
                        case 'n':
                            c = '\n';
                            break;
                        case 'r':
                            c = '\r';
                            break;
                        case 't':
                            c = '\t';
                            break;
                        case 'u':
                            if (i >= length) {
                                make('string', str).error("Unterminated string");
                            }
                            c = parseInt(this.substr(i + 1, 4), 16);
                            if (!isFinite(c) || c < 0) {
                                make('string', str).error("Unterminated string");
                            }
                            c = String.fromCharCode(c);
                            i += 4;
                            break;
                    }
                }
                str += c;
                i += 1;
            }
            i += 1;
            result.push(make('string', str));
            c = this.charAt(i);

            // comment.

        } else if (c === '/' && this.charAt(i + 1) === '/') {
            i += 1;
            for (; ; ) {
                c = this.charAt(i);
                if (c === '\n' || c === '\r' || c === '') {
                    break;
                }
                i += 1;
            }

            // combining

        } else if (prefix.indexOf(c) >= 0) {
            str = c;
            i += 1;
            while (true) {
                c = this.charAt(i);
                if (i >= length || suffix.indexOf(c) < 0) {
                    break;
                }
                str += c;
                i += 1;
            }
            result.push(make('operator', str));

            // single-character operator

        } else {
            i += 1;
            result.push(make('operator', c));
            c = this.charAt(i);
        }
    }
    return result;
};