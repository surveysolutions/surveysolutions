function ItemViewModel() {
    var self = this;
    self.itemId = "";
    self.itemName = "";
    self.itemType = "";
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

    self.exportItemAsPdf = function (id, type, name, pdfDownloadUrl, pdfStatusUrl, pdfRetryUrl, getLanguagesUrl) {
        var encName = decodeURIComponent(name);
        self.itemName = encName;
        self.itemType = type;
        self.itemId = id;
        self.pdfStatusUrl = pdfStatusUrl;
        self.pdfDownloadUrl = pdfDownloadUrl;
        self.pdfRetryUrl = pdfRetryUrl;
        self.getLanguagesUrl = getLanguagesUrl;

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

	self.exportItemAsHtml = function (id, type, name, htmlDownloadUrl,  getLanguagesUrl) {
		var encName = decodeURIComponent(name);
		self.itemName = encName;
		self.itemType = type;
		self.itemId = id;
		self.htmlDownloadUrl = htmlDownloadUrl;
		self.getLanguagesUrl = getLanguagesUrl;

        $.ajax({
	        url: getLanguagesUrl,
	        cache: false,
	        method: "POST",
	        async: false,
            headers: {'X-CSRF-TOKEN': getCsrfCookie()}
	    }).done(function (result) {
            if (result.length && result.length > 1){

                $('#mExportHtml').modal("show");

                $('#export-html-modal-questionnaire-id').val(self.itemId);
                $('#export-html-modal-questionnaire-title').text(self.itemName);

                self.ExportDialogClosed = false;
                self.selectedTransalationHtml = null;
                var dropButton = $('#dropdownMenuButtonHtml');
                dropButton.text(dropButton[0].title);

                self.initLanguageComboBoxHtml(result);
            }
            else {
                window.open(self.htmlDownloadUrl, '_blank');
            }
		});
	};

    self.retryPdfExport = function() {
        $.post(self.pdfRetryUrl, { id: self.itemId, translation: self.selectedTransalation });
        $('#pdfRetryGenerate').hide();
        self.setPdfMessage("Retrying export as PDF.");
    };

    self.updateExportPdfStatus = function (translationId) {
        if (self.pdfStatusUrl === '') return { always: function() {} };
        return $.ajax({
            url: self.pdfStatusUrl + '?timezoneOffsetMinutes=' + new Date().getTimezoneOffset() + '&translation=' + translationId,
            cache: false
        }).done(function (result) {
            if (result.message != null) {
                self.setPdfMessage(result.message);
            } else {
                self.setPdfMessage("Unexpected server response.\r\nPlease contact support@mysurvey.solutions if problem persists.");
            }
            if (result.readyForDownload == true) {
                $('#pdfDownloadButton').unbind('click');
                $('#pdfDownloadButton').click(function () {
                    self.pdfStatusUrl = '';
                    window.location = self.pdfDownloadUrl + '?translation=' + translationId;
                    $('#pdfCancelButton').click();
                });
                $('#pdfDownloadButton').show();
            }
            if (result.canRetry) {
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
	    $.when(self.updateExportPdfStatus(translation)).done(function () {
			if (!self.ExportDialogClosed) {
				setTimeout(function() { self.updateExportPdfStatusNeverending(translation) }, 1500);
		    }
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
            method: "POST",
            headers: {'X-CSRF-TOKEN': getCsrfCookie()}
            
        }).done(function (result) {
            if (result.length && result.length > 1) {
				self.initLanguageComboBoxPdf(result);
                $('#startPdf').show();
                $('#export-pdf-modal-status').hide();
                $('#pdfDownloadButton').hide();
            } else {
                self.startExportProcess(null);
            }
        }).fail(function (xhr, status, error) {
            self.pdfStatusUrl = '';
            self.setPdfMessage("Unexpected error occurred.\r\nPlease contact support@mysurvey.solutions if problem persists.");
        });
    }

    self.initLanguageComboBoxPdf = function (translationList) {
        const generatePdf = function(evn) {
            self.startExportProcess(self.selectedTransalation);
        };

        self.initLanguageComboBox(translationList, generatePdf)
    }

    self.initLanguageComboBoxHtml = function (translationList) {
        const generateHtml = function(evn) {
            window.open(self.htmlDownloadUrl + '?translation=' + self.selectedTransalationHtml, '_blank');
            $('#mExportHtml').modal("hide");
        };
        
        self.initLanguageComboBox(translationList, generateHtml)
    }

    self.initLanguageComboBox = function (translationList, clickFunc) {
        var typeaheadCtrl = $(".languages-combobox");
        typeaheadCtrl.empty();

        for (var i = 0; i < translationList.length; i++) {
            var translationItem = translationList[i];
            
            var li = document.createElement('li');
            var a = document.createElement('a');

            a.href = "javascript:void(0)";
            a.setAttribute('value', translationItem.value);
            a.textContent = translationItem.name;
            
            li.appendChild(a);
            typeaheadCtrl[0].appendChild(li);
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
            clickFunc();
        });
	}

    self.startExportProcess = function (translation) {
		$('#startPdf').hide();
        $('#export-pdf-modal-status').show();

	    self.updateExportPdfStatusNeverending(translation);

		$('.close-pdf-dialog').unbind('click');
		$('.close-pdf-dialog').click(function(evn) {
		    self.ExportDialogClosed = true;
			self.setPdfMessage('');
		});
    }
}

$(function () {
    window.questionnaireActionModel = new ItemViewModel();
    
    $('#table-content-holder > .scroller-container').perfectScrollbar();
});
