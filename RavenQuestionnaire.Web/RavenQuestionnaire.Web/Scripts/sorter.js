(function ($) {
    $.extend({
        tablesorter: new function () {

            this.defaults = {
                cssHeader: "header",
                cssAsc: "headerSortUp",
                cssDesc: "headerSortDown",
                sortInitialOrder: "asc",
                sortMultiSortKey: "shiftKey",
                sortForce: null,
                sortAppend: null,
                textExtraction: "simple",
                headers: {},
                pages: {},
                widthFixed: false,
                cancelSelection: true,
                sortList: [],
                headerList: [],
                pageList: [],
                debug: false,
                url: ""
            };
            //private methods
            function sendRequest(config) {
                config.send(getSortRequestData(config));
            }
            function getSortRequestData(config) {
                var data = [];
                for (var j = 0; j < config.sortList.length; j++) {
                    var s = config.sortList[j],
                        o = config.headerList[s[0]];
                    data[j] = { Field: o.metadata.key, Direction: s[1]==1? "Desc":"Asc" };
                }
                return data;
            }

            function getSortQueryData(config) {
                var data = getSortRequestData(config);
                var s = $.map(data, function (s) {
                    return s.Field + (s.Direction == "Asc" ? "" : s.Direction);
                });
                return s.join(",");
            }
            function buildHeaders(table) {

                var tableHeadersRows = [];

                for (var i = 0; i < table.tHead.rows.length; i++) { tableHeadersRows[i] = 0; };

                var $tableHeaders = $("thead th", table);

                $tableHeaders.each(function (index) {

                    this.count = 0;
                    this.column = index;
                    this.order = formatSortingOrder(table.config.sortInitialOrder);
                    this.metadata = readMetadata(this);
                    if (this.metadata.disable || checkHeaderOptions(table, index)) this.sortDisabled = true;

                    if (!this.sortDisabled) {
                        $(this).addClass(table.config.cssHeader);
                    }

                    // add cell to headerList
                    table.config.headerList[index] = this;
                });

                return $tableHeaders;
            };

            function readMetadata(cell) {
                var metaString = $(cell).attr("sorter");
                var meta = jQuery.parseJSON(metaString);
                return meta == null ? {} : meta;
            }

            function formatSortingOrder(v) {

                if (typeof (v) != "Number") {
                    i = (v.toLowerCase() == "desc") ? 1 : 0;
                } else {
                    i = (v == (0 || 1)) ? v : 0;
                }
                return i;
            }

            function checkHeaderOptions(table, i) {
                if ((table.config.headers[i]) && (table.config.headers[i].sorter === false)) { return true; };
                return false;
            }

            function setHeadersCss(table, $headers, list, css) {
                // remove all header information
                $headers.removeClass(css[0]).removeClass(css[1]);

                var h = [];
                $headers.each(function (offset) {
                    if (!this.sortDisabled) {
                        h[this.column] = $(this);
                    }
                });

                var l = list.length;
                for (var i = 0; i < l; i++) {
                    h[list[i][0]].addClass(css[list[i][1]]);
                }
            }
            function isValueInArray(v, a) {
                var l = a.length;
                for (var i = 0; i < l; i++) {
                    if (a[i][0] == v) {
                        return true;
                    }
                }
                return false;
            }

            function buildPages(table) {

                var $tablePages = $(".pagination a[href]");

                $tablePages.each(function (index) {
                    table.config.pageList[index] = this;
                });

                return $tablePages;
            }

            /* public methods */
            this.construct = function (settings) {
                return this.each(function () {

                    if (!this.tHead || !this.tBodies) return;

                    var $this, $headers, config;

                    this.config = {};

                    config = $.extend(this.config, $.tablesorter.defaults, settings);



                    // store common expression for speed					
                    $this = $(this);

                    // build headers
                    $headers = buildHeaders(this);

                    if (config.order!=null && config.order != "") {

                        var list = config.order.split(',');

                        var s = jQuery.map(list, function (o) {
                            var p = {};
                            var key = o;
                            p.k = 0;
                            p.d = 0;

                            if (o.substr(-4).toLowerCase() === "desc") {
                                p.d = 1;
                                key = o.substr(0, o.length - 4);
                            }
                            for (var i = 0; i < config.headerList.length; i++) {
                                if (key == config.headerList[i].metadata.key) {
                                    p.k = config.headerList[i].column;
                                    return p;
                                }
                            }
                            return null;
                        });
                        config.sortList = [];
                        for (var i = 0; i < s.length; i++) {
                            config.sortList[i] = [s[i].k, s[i].d];
                        }
                    }

                    // get the css class names, could be done else where.
                    var sortCSS = [config.cssDesc, config.cssAsc];

                    $pages = buildPages(this);

                    // apply event handling to headers
                    // this is to big, perhaps break it out?
                    $headers.click(function (e) {

                        $this.trigger("sortStart");

                        if (!this.sortDisabled) {

                            // store exp, for speed
                            var $cell = $(this);

                            // get current column index
                            var i = this.column;

                            // get current column sort order
                            this.order = this.count++ % 2;

                            // user only whants to sort on one column
                            if (!e[config.sortMultiSortKey]) {

                                // flush the sort list
                                config.sortList = [];

                                if (config.sortForce != null) {
                                    var a = config.sortForce;
                                    for (var j = 0; j < a.length; j++) {
                                        if (a[j][0] != i) {
                                            config.sortList.push(a[j]);
                                        }
                                    }
                                }

                                // add column to sort list
                                config.sortList.push([i, this.order]);

                                // multi column sorting
                            } else {
                                // the user has clicked on an all ready sortet column.
                                if (isValueInArray(i, config.sortList)) {

                                    // revers the sorting direction for all tables.
                                    for (var j = 0; j < config.sortList.length; j++) {
                                        var s = config.sortList[j], o = config.headerList[s[0]];
                                        if (s[0] == i) {
                                            o.count = s[1];
                                            o.count++;
                                            s[1] = o.count % 2;
                                        }
                                    }
                                } else {
                                    // add column to sort list array
                                    config.sortList.push([i, this.order]);
                                }
                            };

                            for (var i = 0; i < config.pageList.length; i++) {
                                var href = $(config.pageList[i]).attr('href');
                                var order = getSortQueryData(config);
                                if (order.length > 0) {
                                    var urlParts = parseUri(href);
                                    var query = getQueryPairs(urlParts.query);
                                    var keys = jQuery.map(query, function (q) {
                                        return q.key;
                                    });
                                    var pos = $.inArray("order", keys);
                                    if (pos == -1) {
                                        query.push({ key: "order", value: order });
                                    }
                                    else {
                                        query[pos].value = order;
                                    }
                                    var pairs = jQuery.map(query, function (q) {
                                        return q.key + '=' + q.value;
                                    });
                                    urlParts.query = pairs.join("&");
                                    href = getUrl(urlParts);
                                }
                                $(config.pageList[i]).attr('href', href);
                            }

                            setTimeout(function () {
                                //set css for headers
                                sendRequest(config);
                            }, 10);

                            setTimeout(function () {
                                //set css for headers
                                setHeadersCss($this[0], $headers, config.sortList, sortCSS);
                            }, 1);
                            // stop normal event by returning false
                            return false;
                        }
                        // cancel selection	
                    }).mousedown(function () {
                        if (config.cancelSelection) {
                            this.onselectstart = function () { return false; };
                            return false;
                        }
                    });

                    if ($.metadata && ($(this).metadata() && $(this).metadata().sortlist)) {
                        config.sortList = $(this).metadata().sortlist;
                    }

                    if (config.sortList.length > 0) {
                        setHeadersCss($this[0], $headers, config.sortList, sortCSS);
                    }
                });
            };
        }
    });

    // extend plugin scope
    $.fn.extend({
        tablesorter: $.tablesorter.construct
    });

    var ts = $.tablesorter;

})(jQuery);

function getQueryPairs(query) {
    var vars = query.split("&");
    var pairs = [];
    for (var i = 0; i < vars.length; i++) 
    {
        var pair = vars[i].split("=");
        pairs[i] = { key: pair[0], value: pair[1] };
    }
    return pairs;
}

parseUri = function (str) {
    var o = parseUri.options,
		m = o.parser[o.strictMode ? "strict" : "loose"].exec(str),
		uri = {},
		i = 14;

    while (i--) uri[o.key[i]] = m[i] || "";

    uri[o.q.name] = {};
    uri[o.key[12]].replace(o.q.parser, function ($0, $1, $2) {
        if ($1) uri[o.q.name][$1] = $2;
    });

    return uri;
};

getUrl = function (uri) {
    var url = "";
    if (uri.protocol != "") {
        uri.protocol + "://" + uri.authority;
    }
    url += uri.path;
    if (uri.query != "")
        url += "?" + uri.query;
    if (uri.anchor != "")
        url += "#" + uri.anchor;
    return url;
};

parseUri.options = {
    strictMode: false,
    key: ["source", "protocol", "authority", "userInfo", "user", "password", "host", "port", "relative", "path", "directory", "file", "query", "anchor"],
    q: {
        name: "queryKey",
        parser: /(?:^|&)([^&=]*)=?([^&]*)/g
    },
    parser: {
        strict: /^(?:([^:\/?#]+):)?(?:\/\/((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\/?#]*)(?::(\d*))?))?((((?:[^?#\/]*\/)*)([^?#]*))(?:\?([^#]*))?(?:#(.*))?)/,
        loose: /^(?:(?![^:@]+:[^:@\/]*@)([^:\/?#.]+):)?(?:\/\/)?((?:(([^:@]*)(?::([^:@]*))?)?@)?([^:\/?#]*)(?::(\d*))?)(((\/(?:[^?#](?![^?#\/]*\.[^?#\/.]+(?:[?#]|$)))*\/?)?([^?#\/]*))(?:\?([^#]*))?(?:#(.*))?)/
    }
};

(function ($) {
    $.extend({
        tablesorterPager: new function () {

            function updatePageDisplay(c) {
                var s = $(c.cssPageDisplay, c.container).val((c.page + 1) + c.seperator + c.totalPages);
            }

            function setPageSize(table, size) {
                var c = table.config;
                c.size = size;
                c.totalPages = Math.ceil(c.totalRows / c.size);
                c.pagerPositionSet = false;
                moveToPage(table);
                fixPosition(table);
            }

            function fixPosition(table) {
                var c = table.config;
                if (!c.pagerPositionSet && c.positionFixed) {
                    var c = table.config, o = $(table);
                    if (o.offset) {
                        c.container.css({
                            top: o.offset().top + o.height() + 'px',
                            position: 'absolute'
                        });
                    }
                    c.pagerPositionSet = true;
                }
            }

            function moveToFirstPage(table) {
                var c = table.config;
                c.page = 0;
                moveToPage(table);
            }

            function moveToLastPage(table) {
                var c = table.config;
                c.page = (c.totalPages - 1);
                moveToPage(table);
            }

            function moveToNextPage(table) {
                var c = table.config;
                c.page++;
                if (c.page >= (c.totalPages - 1)) {
                    c.page = (c.totalPages - 1);
                }
                moveToPage(table);
            }

            function moveToPrevPage(table) {
                var c = table.config;
                c.page--;
                if (c.page <= 0) {
                    c.page = 0;
                }
                moveToPage(table);
            }


            function moveToPage(table) {
                var c = table.config;
                if (c.page < 0 || c.page > (c.totalPages - 1)) {
                    c.page = 0;
                }

                renderTable(table, c.rowsCopy);
            }

            function renderTable(table, rows) {

                var c = table.config;
                var l = rows.length;
                var s = (c.page * c.size);
                var e = (s + c.size);
                if (e > rows.length) {
                    e = rows.length;
                }


                var tableBody = $(table.tBodies[0]);

                // clear the table body

                $.tablesorter.clearTableBody(table);

                for (var i = s; i < e; i++) {

                    //tableBody.append(rows[i]);

                    var o = rows[i];
                    var l = o.length;
                    for (var j = 0; j < l; j++) {

                        tableBody[0].appendChild(o[j]);

                    }
                }

                fixPosition(table, tableBody);

                $(table).trigger("applyWidgets");

                if (c.page >= c.totalPages) {
                    moveToLastPage(table);
                }

                updatePageDisplay(c);
            }

            this.appender = function (table, rows) {

                var c = table.config;

                c.rowsCopy = rows;
                c.totalRows = rows.length;
                c.totalPages = Math.ceil(c.totalRows / c.size);

                renderTable(table, rows);
            };

            this.defaults = {
                size: 10,
                offset: 0,
                page: 0,
                totalRows: 0,
                totalPages: 0,
                container: null,
                cssNext: '.next',
                cssPrev: '.prev',
                cssFirst: '.first',
                cssLast: '.last',
                cssPageDisplay: '.pagedisplay',
                cssPageSize: '.pagesize',
                seperator: "/",
                positionFixed: true,
                appender: this.appender
            };

            this.construct = function (settings) {

                return this.each(function () {

                    config = $.extend(this.config, $.tablesorterPager.defaults, settings);

                    var table = this, pager = config.container;

                    $(this).trigger("appendCache");

                    config.size = parseInt($(".pagesize", pager).val());

                    $(config.cssFirst, pager).click(function () {
                        moveToFirstPage(table);
                        return false;
                    });
                    $(config.cssNext, pager).click(function () {
                        moveToNextPage(table);
                        return false;
                    });
                    $(config.cssPrev, pager).click(function () {
                        moveToPrevPage(table);
                        return false;
                    });
                    $(config.cssLast, pager).click(function () {
                        moveToLastPage(table);
                        return false;
                    });
                    $(config.cssPageSize, pager).change(function () {
                        setPageSize(table, parseInt($(this).val()));
                        return false;
                    });
                });
            };

        }
    });
    // extend plugin scope
    $.fn.extend({
        tablesorterPager: $.tablesorterPager.construct
    });

})(jQuery);				