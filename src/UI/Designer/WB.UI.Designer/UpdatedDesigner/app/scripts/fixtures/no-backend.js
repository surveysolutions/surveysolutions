(function() {
    if (!(document.URL.indexOf('nobackend') > 0)) {
        return;
    }

    angular.module('designerApp')
        .config(function($provide) {
            $provide.decorator('$httpBackend', angular.mock.e2e.$httpBackendDecorator);
        })
        .run(function($httpBackend, $resource) {
            //Default Questionnaire
            $httpBackend.whenGET('../../api/questionnaire/get/7c97b1925b0244b782ed6741a5035fae').respond(
                $resource('./../data/questionnaire/7c97b1925b0244b782ed6741a5035fae.json').get()
            );

            $httpBackend.whenGET('../../api/questionnaire/editRoster/7c97b1925b0244b782ed6741a5035fae?rosterId=5ebbab39c975441cab18b44fdea5be72').respond(
                $resource('./../data/editGroup/manualListRoster.json').get()
            );

            $httpBackend.whenGET('../../api/questionnaire/chapter/7c97b1925b0244b782ed6741a5035fae?chapterId=6e240642274c4bdea937baa78cd4ad6f').respond(
                $resource('./../data/chapter/6e240642274c4bdea937baa78cd4ad6f.json').get()
            );

            $httpBackend.whenGET('../../api/questionnaire/chapter/7c97b1925b0244b782ed6741a5035fae?chapterId=34e31f00cdbff703a680db5fa66bf8b5').respond(
                $resource('./../data/chapter/34e31f00cdbff703a680db5fa66bf8b5.json').get()
            );

            $httpBackend.whenGET('../../api/questionnaire/editQuestion/7c97b1925b0244b782ed6741a5035fae?questionId=20ec89157c0e41e49d77db46e929db5d').respond(
                $resource('./../data/editQuestion/20ec89157c0e41e49d77db46e929db5d.json').get()
            );

            $httpBackend.whenGET('../../api/questionnaire/editQuestion/7c97b1925b0244b782ed6741a5035fae?questionId=3c0ff7a6d1614aa2b9af21aaa40cdb47').respond(
                $resource('./../data/editQuestion/3c0ff7a6d1614aa2b9af21aaa40cdb47.json').get()
            );

            $httpBackend.whenGET('../../api/questionnaire/editQuestion/7c97b1925b0244b782ed6741a5035fae?questionId=d6c5faeed9fe43338b76e7a3ef184f5a').respond(
                $resource('./../data/editQuestion/d6c5faeed9fe43338b76e7a3ef184f5a.json').get()
            );

            //Empty Questionnaire
            $httpBackend.whenGET('../../api/questionnaire/get/C772F0868D6E4B46B2EB281382F280AB').respond(
                $resource('./../data/questionnaire/empty.json').get()
            );

            //Verifier
            $httpBackend.whenGET('../../api/questionnaire/editGroup/7c97b1925b0244b782ed6741a5035fae?groupId=ddfaab0f37394a679f088add19325cfe').respond(
                $resource('./../data/editGroup/ddfaab0f37394a679f088add19325cfe.json').get()
            );

            $httpBackend.whenGET('../../api/questionnaire/verify/7c97b1925b0244b782ed6741a5035fae').respond(
                $resource('./../data/verify/with-errors.json').get()
            );

            //Comands
            $httpBackend.whenPOST('../../command/execute').respond(function(method, url, data, headers) {
                    if (!angular.fromJson(data).type.indexOf('Update')) {
                        return [200, { "Error": "Custom Validation Error From Api", "HasPermissions": true, "IsSuccess": false }, {}];
                    }
                    return [200, { "Error": "", "HasPermissions": true, "IsSuccess": true }, {}];
                }
            );

            //Views
            $httpBackend.whenGET(/views\/.*/).passThrough();

            //Data files
            $httpBackend.whenGET(/data\/.*/).passThrough();

            // Validation
            $httpBackend.whenPOST('../../account/findbyemail', { email: 'test@test.com' })
                .respond({ "isUserExist": false });
        });
}());