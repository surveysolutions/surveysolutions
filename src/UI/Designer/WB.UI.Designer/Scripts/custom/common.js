function ItemViewModel() {
    var self = this;
    self.itemId = "";
    self.itemName = "";
    self.itemType = "";
    self.pdfStatusUrl = '';
    self.selectedTransalation = null;

    self.deleteItem = function (id, type, name) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;

        $('#delete-modal-questionnaire-id').val(self.itemId);
        $('#delete-modal-questionnaire-title').html(self.itemName);
    };

    self.exportItemAsPdf = function (id, type, name, pdfDownloadUrl, pdfStatusUrl, pdfRetryUrl, getLanguagesUrl) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;
        self.pdfStatusUrl = pdfStatusUrl;
        self.pdfDownloadUrl = pdfDownloadUrl;
        self.pdfRetryUrl = pdfRetryUrl;
        self.getLanguagesUrl = getLanguagesUrl;

        $('#export-pdf-modal-questionnaire-id').val(self.itemId);
        $('#export-pdf-modal-questionnaire-title').text(self.itemName);

        $('#pdfDownloadButton').hide();
        $('#pdfRetryGenerate').hide();

        self.getLanguages(getLanguagesUrl);
        //self.updateExportPdfStatusNeverending();
    };

    self.retryPdfExport = function() {
        $.post(self.pdfRetryUrl, { id: self.itemId });
        $('#pdfRetryGenerate').hide();
        self.setPdfMessage("Retrying export as PDF.");
    };

    self.updateExportPdfStatus = function (translationId) {
        if (self.pdfStatusUrl == '') return { always: function() {} };
        return $.ajax({
            url: self.pdfStatusUrl + '?translation=' + translationId,
            cache: false
        }).done(function (result) {
            if (result.Message != null) {
                self.setPdfMessage(result.Message);
            } else {
                self.setPdfMessage("Unexpected server response.\r\nPlease contact support@mysurvey.solutions if problem persists.");
            }
            if (result.ReadyForDownload == true) {
                $('#pdfDownloadButton').unbind('click');
                $('#pdfDownloadButton').click(function () {
                    self.pdfStatusUrl = '';
                    window.location = self.pdfDownloadUrl + '?translation=' + translationId;
                    $('#pdfCancelButton').click();
                });
                $('#pdfDownloadButton').show();
            }
            if (result.CanRetry) {
                $('#pdfRetryGenerate').show();
            } else {
                $('#pdfRetryGenerate').hide();
            }
        }).fail(function (xhr, status, error) {
            self.pdfStatusUrl = '';
            self.setPdfMessage("Unexpected error occurred.\r\nPlease contact support@mysurvey.solutions if problem persists.");
        });
    }

    self.updateExportPdfStatusNeverending = function (translation) {
        $.when(self.updateExportPdfStatus(translation)).always(function () {
            setTimeout(self.updateExportPdfStatusNeverending(translation), 1000);
        });
    }

    self.setPdfMessage = function (message) {
        $('#export-pdf-modal-status').text(
            message
            //+ '\r\n\r\n' + 'Status updated ' + new Date().toLocaleTimeString()
        );
    }

    self.getLanguages = function (languagesUrl) {
        $.ajax({
            url: languagesUrl,
            cache: false,
            method: "POST"
        }).done(function (result) {
            if (result.length && result.length > 1) {
                self.initLanguageComboBox(result);
                $('.start-pdf-generation').show();
                $('#export-pdf-modal-status').hide();
            } else {
                $('.start-pdf-generation').hide();
                $('#export-pdf-modal-status').show();
                self.updateExportPdfStatusNeverending(null);
            }
        }).fail(function (xhr, status, error) {
            self.pdfStatusUrl = '';
            self.setPdfMessage("Unexpected error occurred.\r\nPlease contact support@mysurvey.solutions if problem persists.");
        });
    }

    self.initLanguageComboBox = function (translationList) {
        var typeaheadCtrl = $(".languages-combobox");
        typeaheadCtrl.empty();

        for (var i = 0; i < translationList.length; i++) {
            var translationItem = translationList[i];
            typeaheadCtrl.append('<li><a href="#" value="' + translationItem.Value + '">' + translationItem.Name + '</a></li>');
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
        $('#pdfGenerateButton').click(function(evn) {
            self.updateExportPdfStatusNeverending(self.selectedTransalation);
            $('.start-pdf-generation').hide();
            $('#export-pdf-modal-status').show();
        });
    }
}

$(function () {
    window.questionnaireActionModel = new ItemViewModel();
    
    $('#table-content-holder > .scroller-container').perfectScrollbar();
});