define('validator',
['underscore', 'expressionParser', 'config'],
    function (_, expressionParser, config) {
        var _dc = null,
            dc = function (dc) {
                if (dc) {
                    _dc = dc;
                }
                return _dc || require('datacontext');
            };

        var questionTypesSupportedForSubstitution = [
            config.questionTypes.Numeric,
            config.questionTypes.Text,
            config.questionTypes.DateTime,
            config.questionTypes.SingleOption,
        ];

        return {

            isValidQuestionTitle: function (title, contextQuestion) {

                var parts = title.split('%');

                if (parts.length % 2 == 0)
                    return { isValid: false, errorMessage: 'Count of special % symbols is odd but should be even. Seems like one of variables for substitution does not have closing % character.' };


                var titleVariables = _.uniq(_.filter(parts, function (part, index) { return index % 2 == 1; }));

                if (titleVariables.length == 0)
                    return { isValid: true };


                if (contextQuestion.isFeatured())
                    return { isValid: false, errorMessage: 'Pre-filled question cannot use variables for substitution but following variables are used: %' + titleVariables.join('%, %') + '%.' };


                var existingVariables = dc().questions.getAllVariables();
                var variableNotExists = function(variable) { return !_.contains(existingVariables, variable); };

                var notExistingTitleVariables = _.filter(titleVariables, variableNotExists);

                if (notExistingTitleVariables.length > 0)
                    return { isValid: false, errorMessage: 'Following variables do not exist in questionnaire: %' + notExistingTitleVariables.join('%, %') + '%.' };


                var variableTypeIsNotSuppoted = function (variable) {
                    var question = dc().questions.getLocalByVariable(variable);
                    return !_.contains(questionTypesSupportedForSubstitution, question.qtype());
                };

                var titleVariablesWithNotSupportedTypes = _.filter(titleVariables, variableTypeIsNotSuppoted);

                if (titleVariablesWithNotSupportedTypes.length > 0)
                    return { isValid: false, errorMessage: 'Following variables have not supported type for substitution: %' + titleVariablesWithNotSupportedTypes.join('%, %') + '%. Only numeric, text, date and single categorical types are supported.' };


                if (_.contains(titleVariables, contextQuestion.alias()))
                    return { isValid: false, errorMessage: 'Question cannot reference self variable %' + contextQuestion.alias() + '% for substitution.' };


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

