define('datacontext', 
    ['jquery', 'underscore', 'ko', 'model', 'config'],
    function ($, _, ko, model, config) {
        var logger = config.logger;

        var datacontext = {
        };
        
        // We did this so we can access the datacontext during its construction
        model.setDataContext(datacontext);

        return datacontext;
});