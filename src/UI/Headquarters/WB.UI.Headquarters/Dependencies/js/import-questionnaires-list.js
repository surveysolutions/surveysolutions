'use strict';

$(function () {
    var questionnaireListUrl = $('#questionnaireListUrl').attr('href');

    var requestHeaders = {};
    requestHeaders[input.settings.acsrf.tokenName] = input.settings.acsrf.token;

    $('table.import-interview').DataTable({
        processing: true,
        serverSide: true,
        ajax: {
            url: questionnaireListUrl,
            type: "POST",
            headers: requestHeaders
        },
        "columns": [
            {
                data: "title",
                "class": ""
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
        "pagingType": "full_numbers",
        "lengthChange": false,
    });
});