'use strict';

$(function () {
    var questionnaireListUrl = $('#questionnaireListUrl').attr('href');

    var requestHeaders = {};
    requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

    var table = $('table.import-interview')
        .on('init.dt', function () {
            $('#DataTables_Table_0_filter label').on('click', function (e) {
                if (e.target !== this)
                    return;
                if ($(this).hasClass("active")) {
                    $(this).removeClass("active");
                }
                else {
                    $(this).addClass("active");
                }
                $(".column-questionnaire-title").toggleClass("padding-left-lide");
            });
        })
        .DataTable({
            processing: true,
            serverSide: true,
            ajax: {
                url: questionnaireListUrl,
                type: "POST",
                headers: requestHeaders
            },
            columns: [
                {
                    data: "title"
                },
                {
                    data:
                    {
                        _: "lastModified.display",
                        sort: "lastModified.timestamp"
                    },
                    "class": "changed-recently"
                },
                {
                    data: "createdBy",
                    "class": "created-by"
                }
            ],
            pagingType: "full_numbers",
            lengthChange: false,
            pageLength: 50
        });
});