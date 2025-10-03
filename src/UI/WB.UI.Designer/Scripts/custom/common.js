import DOMPurify from 'dompurify';
import { Modal } from 'bootstrap';

window.Modal = Modal;

function ItemViewModel() {
    var self = this;
    self.itemId = '';
    self.itemName = '';
    self.itemType = '';
    self.pdfGenerateUrl = '';
    self.pdfStatusUrl = '';
    self.selectedTransalation = null;
    self.selectedTransalationHtml = null;

    self.deleteItem = function (id, type, name) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;

        $('#delete-modal-questionnaire-id').val(self.itemId);
        $('#delete-modal-questionnaire-title').html(self.itemName);
    };

    self.assignFolder = function (id, type, name) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;

        $('#assign-folder-button').prop('disabled', true);
        $('#assign-folder-modal-folder-id').val('');
        $('#assign-folder-modal-questionnaire-id').val(self.itemId);
        $('#assign-folder-modal-questionnaire-title').html(self.itemName);
    };

    self.exportItemAsPdf = function (
        id,
        type,
        name,
        pdfGenerateUrl,
        pdfDownloadUrl,
        pdfStatusUrl,
        pdfRetryUrl,
        getLanguagesUrl
    ) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;
        self.pdfStatusUrl = pdfStatusUrl;
        self.pdfDownloadUrl = pdfDownloadUrl;
        self.pdfRetryUrl = pdfRetryUrl;
        self.getLanguagesUrl = getLanguagesUrl;
        self.pdfGenerateUrl = pdfGenerateUrl;

        self.setPdfMessage('');

        $('#export-pdf-modal-questionnaire-id').val(self.itemId);
        $('#export-pdf-modal-questionnaire-title').text(self.itemName);

        $('#pdfDownloadButton').hide();
        $('#pdfRetryGenerate').hide();

        self.ExportDialogClosed = false;
        self.selectedTransalation = null;
        var dropButton = $('#dropdownMenuButton');
        dropButton.text(dropButton[0].title);

        self.getLanguages(getLanguagesUrl);
    };

    self.exportItemAsHtml = function (
        id,
        type,
        name,
        htmlDownloadUrl,
        getLanguagesUrl
    ) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;
        self.htmlDownloadUrl = htmlDownloadUrl;
        self.getLanguagesUrl = getLanguagesUrl;

        $.ajax({
            url: getLanguagesUrl,
            cache: false,
            method: 'POST',
            async: false,
            headers: { 'X-CSRF-TOKEN': getCsrfCookie() },
        }).done(function (result) {
            if (result.length && result.length > 1) {
                var mExportHtml = document.getElementById('mExportHtml')
                //var mExportHtmlModal = Modal.getInstance(mExportHtml)
                var mExportHtmlModal = new Modal(mExportHtml, {
                    keyboard: false
                });
                mExportHtmlModal.show();

                $('#export-html-modal-questionnaire-id').val(self.itemId);
                $('#export-html-modal-questionnaire-title').text(self.itemName);

                self.ExportDialogClosed = false;
                self.selectedTransalationHtml = null;
                var dropButton = $('#dropdownMenuButtonHtml');
                dropButton.text(dropButton[0].title);

                var typeaheadCtrl = $('.languages-combobox-html');
                typeaheadCtrl.empty();

                for (var i = 0; i < result.length; i++) {
                    let translationItem = result[i];

                    let itemToAppend = '<li><a href="javascript:void(0)" value="' +
                        translationItem.value +
                        '">' +
                        translationItem.name +
                        '</a></li>';

                    typeaheadCtrl.append(DOMPurify.sanitize(itemToAppend));
                }

                typeaheadCtrl.unbind('click');
                typeaheadCtrl.click(function (evn) {
                    var link = $(evn.target);
                    self.selectedTransalationHtml = link.attr('value');
                    $('#dropdownMenuButtonHtml').text(link.text());
                    $('#htmlGenerateButton').prop('disabled', false);
                });

                $('#htmlGenerateButton').prop('disabled', true);
                $('#htmlGenerateButton').unbind('click');
                $('#htmlGenerateButton').click(function (evn) {
                    window.open(
                        self.htmlDownloadUrl +
                            '?translation=' +
                            self.selectedTransalationHtml,
                        '_blank'
                    );
                    mExportHtmlModal.hide();
                });
            } else {
                window.open(self.htmlDownloadUrl, '_blank');
            }
        });
    };

    self.retryPdfExport = function () {
        $.ajax({
            url: self.pdfRetryUrl,
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({
                translation: self.selectedTransalation
            }),
            headers: { 'X-CSRF-TOKEN': getCsrfCookie() },
        });
        $('#pdfRetryGenerate').hide();
        self.setPdfMessage('Retrying export as PDF.');
    };

    self.updateExportPdfStatus = function (translationId) {
        if (self.pdfStatusUrl === '') return { always: function () {} };
        return $.ajax({
            url:
                self.pdfStatusUrl +
                '?timezoneOffsetMinutes=' +
                new Date().getTimezoneOffset() +
                '&translation=' +
                translationId,
            cache: false,
        })
            .done(function (result) {
                if (result.message != null) {
                    self.setPdfMessage(result.message);
                } else {
                    self.setPdfMessage(
                        'Unexpected server response.\r\nPlease contact support@mysurvey.solutions if problem persists.'
                    );
                }
                if (result.readyForDownload == true) {
                    $('#pdfDownloadButton').unbind('click');
                    $('#pdfDownloadButton').click(function () {
                        self.pdfStatusUrl = '';
                        window.location =
                            self.pdfDownloadUrl +
                            '?translation=' +
                            translationId;
                        $('#pdfCancelButton').click();
                    });
                    $('#pdfDownloadButton').show();
                }
                if (result.canRetry) {
                    $('#pdfRetryGenerate').show();
                } else {
                    $('#pdfRetryGenerate').hide();
                }
            })
            .fail(function (xhr, status, error) {
                self.pdfStatusUrl = '';
                self.setPdfMessage(
                    'Unexpected error occurred.\r\nPlease contact support@mysurvey.solutions if problem persists.'
                );
            });
    };

    self.updateExportPdfStatusNeverending = function (translation) {
        $.when(self.updateExportPdfStatus(translation)).done(function () {
            if (!self.ExportDialogClosed) {
                setTimeout(function () {
                    self.updateExportPdfStatusNeverending(translation);
                }, 1500);
            }
        });
    };

    self.setPdfMessage = function (message) {
        $('#export-pdf-modal-status').text(
            message
            //+ '\r\n\r\n' + 'Status updated ' + new Date().toLocaleTimeString()
        );
    };

    self.getLanguages = function (languagesUrl) {
        $.ajax({
            url: languagesUrl,
            cache: false,
            method: 'POST',
            headers: { 'X-CSRF-TOKEN': getCsrfCookie() },
        })
            .done(function (result) {
                if (result.length && result.length > 1) {
                    self.initLanguageComboBox(result);
                    $('#startPdf').show();
                    $('#export-pdf-modal-status').hide();
                    $('#pdfDownloadButton').hide();
                } else {
                    self.startExportProcess(null);
                }
            })
            .fail(function (xhr, status, error) {
                self.pdfStatusUrl = '';
                self.setPdfMessage(
                    'Unexpected error occurred.\r\nPlease contact support@mysurvey.solutions if problem persists.'
                );
            });
    };

    self.initLanguageComboBox = function (translationList) {
        var typeaheadCtrl = $('.languages-combobox');
        typeaheadCtrl.empty();

        for (var i = 0; i < translationList.length; i++) {
            let translationItem = translationList[i];

            let itemToAppend = '<li><a href="javascript:void(0)" value="' +
                translationItem.value +
                '">' +
                translationItem.name +
                '</a></li>';

            typeaheadCtrl.append(DOMPurify.sanitize(itemToAppend));
        }

        typeaheadCtrl.unbind('click');
        typeaheadCtrl.click(function (evn) {
            var link = $(evn.target);
            self.selectedTransalation = link.attr('value');
            $('#dropdownMenuButton').text(link.text());
            $('#pdfGenerateButton').prop('disabled', false);
        });

        $('#pdfGenerateButton').prop('disabled', true);
        $('#pdfGenerateButton').unbind('click');
        $('#pdfGenerateButton').click(function (evn) {
            self.startExportProcess(self.selectedTransalation);
        });
    };

    self.startExportProcess = function (translation) {
        $('#startPdf').hide();
        $('#export-pdf-modal-status').show();

        $.ajax({
            url: self.pdfGenerateUrl,
            method: 'POST',
            data: {
                id: self.itemId,
                translation: translation
            },
            headers: { 'X-CSRF-TOKEN': getCsrfCookie() },
        }).always(function () {
            self.updateExportPdfStatusNeverending(translation);
        });

        $('.close-pdf-dialog').unbind('click');
        $('.close-pdf-dialog').click(function (evn) {
            self.ExportDialogClosed = true;
            self.setPdfMessage('');
        });
    };
}

$(function () {
    window.questionnaireActionModel = new ItemViewModel();

    //$('#table-content-holder > .scroller-container').perfectScrollbar();
});
