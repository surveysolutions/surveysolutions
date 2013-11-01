define('model.error',
    ['ko', 'config'],
    function (ko, config) {
        var _dc = null,
            Reference = function () {
                var self = this;

                self.title = ko.observable();
                self.href = ko.observable();

                return self;
            },
            error = function (message, code, references) {
                var self = this;

                self.code = ko.observable(code || "WB0000");
                self.message = ko.observable(message || "");

                var uiReferences = _.map(references, function (item) {
                    var reference = new Reference();
                    var isItemIsGroup = item.type === config.verificationReferenceType.group;
                    var qitem = isItemIsGroup
                        ? error.datacontext().groups.getLocalById(item.id)
                        : error.datacontext().questions.getLocalById(item.id);

                    if (_.isNull(qitem)) {
                        reference.href("#");
                        reference.title("Missing " + (isItemIsGroup? "group" : "question") + ". Please refresh page and retry validation.");
                    } else {
                        reference.href(qitem.getHref());
                        reference.title(qitem.title());
                    }
                    return reference;
                });

                self.references = ko.observableArray(uiReferences || []);

                return self;
            };

        error.datacontext = function (dc) {
            if (dc) {
                _dc = dc;
            }
            return _dc || require('datacontext');
        };

        return error;
    });
