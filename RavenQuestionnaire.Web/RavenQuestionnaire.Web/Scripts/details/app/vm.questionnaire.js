define('vm.questionnaire',
    ['ko', 'underscore', 'config', 'data'],
    function (ko, _, config, data) {

        var init = function (questionnaire) {
            console.log('vm.questionnaire');
        };

        init(data.questionnaire);

        return {

        };
    });