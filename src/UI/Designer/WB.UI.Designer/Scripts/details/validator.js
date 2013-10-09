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
                        isValid: true
                    };
                }

                var existingVariables = _.map(dc().questions.getAllLocal(), function (question) {
                    return question.alias();
                });
                
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

