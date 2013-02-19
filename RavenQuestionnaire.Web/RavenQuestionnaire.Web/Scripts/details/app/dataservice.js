define('dataservice',
    [
        'dataservice.question',
        'dataservice.group'
    ],
    function (question, group) {
        return {
            question: question,
            group: group
        };
    });