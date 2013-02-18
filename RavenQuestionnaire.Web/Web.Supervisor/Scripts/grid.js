(function ($) {
    $.extend({
        grid: new function () {

            this.defaults = {
                id: "",
                page: 1,
                pageSize: 20,
                url: "",
                userId: "",
                sortData: null
            };

            var makePagesClickable = function (config) {
                
                $(".pagination li:not(.disabled):not(.active) a").click(function (e) {
                    var page = getParameterByName($(this).attr('href'), "pager.page");
                    getTablePage(
                        config,
                        config.url, {
                            Id: config.id,
                            SortOrder: config.sortData,
                            Pager: {
                                Page: page === ""? 1: parseInt(page),
                                PageSize: config.pageSize
                            },
                            UserId: config.userId
                        });
                    return false;
                });
            };

            var getTablePage = function (config, url, requestData) {
                $.ajax({
                    type: "POST",
                    url: url,
                    data: JSON.stringify(requestData),
                    contentType: 'application/json, charset=utf-8',
                    statusCode: {
                        200: function (response) {
                            $('#questionnaireTable tbody').html($(response).find('#table tbody').html());
                            $('#paging').html($(response).find('#paging').html());

                            makePagesClickable(config);
                        }
                    }
                });
            };

            var setSortData = function (config, data) {
                config.sortData = JSON.parse(JSON.stringify(data));
            };

            function getParameterByName(ref, name) {
                name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
                var regexS = "[\\?&]" + name + "=([^&#]*)";
                var regex = new RegExp(regexS);
                var results = regex.exec(ref);
                if (results == null)
                    return "";
                else
                    return decodeURIComponent(results[1].replace(/\+/g, " "));
            }

            /* public methods */
            this.construct = function (settings) {
                return this.each(function () {

                    var $this, config;

                    this.config = { };

                    config = $.extend(this.config, $.grid.defaults, settings);

                    $this = $(this);

                    $this.find("table").tablesorter({
                            send: function(data) {
                                setSortData(config, data);
                                getTablePage(config, config.url, {
                                    Id: config.id,
                                    SortOrder: data,
                                    Pager: {
                                        Page: config.page,
                                        PageSize: config.pageSize
                                    },
                                    UserId: config.userId
                                });
                            }
                        });

                    makePagesClickable(config);
                });
            };
        }
    });

    // extend plugin scope
    $.fn.extend({
        grid: $.grid.construct
    });

    var gr = $.grid;

})(jQuery);