define('app/model', ['knockout', 'knockout.validation'],
function (ko) {
	var QuestionModel = function () {
            var self = this;
			self.id = ko.observable();
            self.isCapital = ko.observable();
			self.comments = ko.observableArray();
			self.isEnabled = ko.observable();
			self.isFeatured = ko.observable();
			self.isFlagged = ko.observable();
			self.isMandatory = ko.observable();
			self.propagationVector = ko.observable();
			self.questionType = ko.observable();
			self.title = ko.observable();
			self.isValid = ko.observable(true);
			self.isVisible = ko.observable(true);
			self.isAnswered = ko.observable(false);
			self.validationMessage = ko.observable('');
			self.validationExpression = ko.observable('');
			self.scope = ko.observable();
			self.markerStyle = ko.computed(function () {
			    if (self.isValid() == false) {
			        return "invalid";
			    }
			    if (self.scope() == 1) {
			        return "supervisor";
			    }
			    return "";
			});
            return self;
        };
	var model = {
	    GpsQuestion: function () {
	        var self = this;
	        ko.utils.extend(self, new QuestionModel());
	        self.lat = ko.observable();
	        self.lon = ko.observable();
	        return self;
	    },
        TextQuestion : function () {
            var self = this;
			ko.utils.extend(self, new QuestionModel());
			self.answer = ko.observable();
            return self;
        },
		NumericQuestion : function () {
            var self = this;
			ko.utils.extend(self, new QuestionModel());
			self.answer = ko.observable();
            return self;
        },
		DateTimeQuestion : function () {
            var self = this;
			ko.utils.extend(self, new QuestionModel());
			self.answer = ko.observable();
            return self;
        },
		SingleOptionQuestion : function () {
            var self = this;
			ko.utils.extend(self, new QuestionModel());
			self.selectedOption = ko.observable();
            return self;
        },
		MultyOptionQuestion : function () {
            var self = this;
			ko.utils.extend(self, new QuestionModel());
			self.selectedOptions = ko.observableArray();
            return self;
        },
        Group: function () {
            var self = this;
            self.id = ko.observable();
			self.depth = ko.observable();
			self.css = ko.computed(function(){
				return "level" + self.depth();
			});
			self.href = ko.computed(function(){
			    return "#group/" + self.id();
			});
			self.title = ko.observable();
			self.parentId = ko.observable();
			self.propagationVector = ko.observable();
			self.questions = ko.observableArray();
            return self;
        },
		Interview: function () {
            var self = this;
            self.id = ko.observable();
            self.title = ko.observable();
			self.questionnaireId = ko.observable();
			self.status = ko.observable();
			self.responsible = ko.observable();
            return self;
        },
		User : function(){
			var self = this;
            self.id = ko.observable();
            self.name = ko.observable();
            return self;
		}
    };
    return model;
});