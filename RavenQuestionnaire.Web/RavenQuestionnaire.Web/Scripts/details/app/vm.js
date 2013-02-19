define('vm',
       [ 'vm.menu', 'vm.questionnaire'],
    function (menu, questionnaire) {
        return {
            menu: menu,
            questionnaire: questionnaire
    };
});