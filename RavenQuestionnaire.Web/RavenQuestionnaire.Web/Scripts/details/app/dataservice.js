define('dataservice',
    [
        'dataservice.question',
        'dataservice.group',
        'dataservice.questionnaire'
    ],
    function (question, group, questionnaire) {
        return {
            question: question,
            group: group,
            questionnaire: questionnaire
        };
    });