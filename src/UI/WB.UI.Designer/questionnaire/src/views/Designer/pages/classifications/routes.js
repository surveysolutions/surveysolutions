var routes = {
    groups: 'groups',
    createGroup: 'group',
    updateGroup: 'group/{0}',
    deleteGroup: 'group/{0}',
    classifications: 'classifications',
    createClassification: 'classification',
    updateClassification: 'classification/{0}',
    deleteClassification: 'classification/{0}',
    categories: 'classification/{0}/categories',
    updateCategories: 'classification/{0}/categories'
};

var $http = axios.create({
    baseURL: './api/classifications'
});

// Add a request interceptor
$http.interceptors.request.use(
    function(config) {
        store.commit('start_loading');
        return config;
    },
    function(error) {
        store.commit('finish_loading');
        console.log(error);
        return Promise.reject(error);
    }
);

// Add a response interceptor
$http.interceptors.response.use(
    function(response) {
        store.commit('finish_loading');
        return response;
    },
    function(error) {
        store.commit('finish_loading');
        console.log(error);
        return Promise.reject(error);
    }
);

if (!String.prototype.format) {
    String.prototype.format = function() {
        var args = arguments;
        var sprintfRegex = /\{(\d+)\}/g;

        var sprintf = function(match, number) {
            return number in args ? args[number] : match;
        };

        return this.replace(sprintfRegex, sprintf);
    };
}

/*var guid = function() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
            .toString(16)
            .substring(1);
    }

    return s4() + s4() + s4() + s4() + s4() + s4() + s4() + s4();
};*/

/*Vue.directive('elastic',
    {
        inserted: function(element) {
            var elasticElement = element,
                $elasticElement = $(element),
                initialHeight = initialHeight || $elasticElement.height(),
                delta = parseInt( $elasticElement.css('paddingBottom') ) + parseInt( $elasticElement.css('paddingTop') ) || 0,
                resize = function() {
                    $elasticElement.height(Math.max(20, initialHeight));
                    $elasticElement.height(Math.max(20, elasticElement.scrollHeight - delta));
                };
      
            $elasticElement.on('input change keyup', resize);
            resize();
        }
    });
*/
