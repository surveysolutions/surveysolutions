define('validator',
['underscore', 'expressionParser'],
    function (_, expressionParser) {
        var _dc = null,
            dc = function (dc) {
                if (dc) {
                    _dc = dc;
                }
                return _dc || require('datacontext');
            };
        return {
            isValidQuestionTitle: function (title) {

                var parts = title.split('%');

                if (parts.length % 2 == 0)
                    return { isValid: false, errorMessage: 'Count of special % symbols is odd but should be even. Seems like one of variables for substitution does not have closing % character.' };

                var titleVariables = _.uniq(_.filter(parts, function (part, index) { return index % 2 == 1; }));

                if (titleVariables.length == 0)
                    return { isValid: true };

                var existingVariables = dc().questions.getAllVariables();
                var variableNotExists = function (variable) { return !_.contains(existingVariables, variable); };

                var notExistingTitleVariables = _.filter(titleVariables, variableNotExists);

                if (notExistingTitleVariables.length > 0)
                    return { isValid: false, errorMessage: 'Following variables does not exist in questionnaire: ' + notExistingTitleVariables.join(', ') };

                return { isValid: true };
            },
            isValidExpression: function (expression) {
                if (_.isEmpty(expression)) {
                    return {
                        isValid: true
                    };
                }

                var usingVariables = [];
                var expr = expression;
                try {
                    var usingVariables = expressionParser.expressionParser.parse(expr);
                } catch (e) {
                    return {
                        isValid: false,
                        errorMessage: e.message
                    };
                }

                if (usingVariables.length == 0) {
                    return {
                        isValid: false,
                        errorMessage: "Expression is valid, but does not use any variable name. We recommend you to change or delete it"
                    };
                }

                var existingVariables = dc().questions.getAllVariables();
                
                existingVariables.push("this");
                
                var variablesIntersection = _.intersection(existingVariables, usingVariables);
                if (variablesIntersection.length == usingVariables.length) {
                    return {
                        isValid: true
                    };
                } else {
                    return {
                        isValid: false,
                        errorMessage: "Unknown variables: " + _.difference(usingVariables, variablesIntersection) + ""
                    };
                }
            }
        };
    });

