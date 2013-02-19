define('vm.menu',
    ['ko', 'underscore', 'config', 'data'],
    function (ko, _, config, data) {

        var init = function (questionnaire) {
            console.log('vm.menu');
        };

        init(data.questionnaire);

        return {

        };
    });