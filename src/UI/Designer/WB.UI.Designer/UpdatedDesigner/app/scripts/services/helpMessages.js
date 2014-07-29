angular.module('designerApp')
    .factory('helpService', [
        function() {
            var helpService = {
                'title': 'Title',
                'description': 'Description',
                'conditionExpression': 'Condition expression',
                'validationExpression': 'Validation Expression',
                'validationMessage': 'Validation Message',
                'instruction': 'Instruction',
                'mask': 'Mask',
                'rosterType': 'Roster Type',
                'variableName': 'Variable Name',
                'sourceQuestion': 'Source question (answer generates number of rows)',
                'titleQuestion': 'Titles of rows',
                'fixedTitles': 'Fixed roster titles',
                'questionType': 'Question Type',
                'variable': 'Variable',
                'variableLabel': 'Variable Label'
            };

            helpService.getHelpMessage = function(key) {
                return helpService[key];
            };

            return helpService;
        }
    ]);