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
            $httpBackend.whenGET('../api/questionnaire/get/7c97b1925b0244b782ed6741a5035fae').respond(
                $resource('./data/questionnaire/7c97b1925b0244b782ed6741a5035fae.json').get()
            );


            $httpBackend.whenGET('../api/questionnaire/chapter/7c97b1925b0244b782ed6741a5035fae?chapterId=6e240642274c4bdea937baa78cd4ad6f').respond(
                $resource('./data/chapter/6e240642274c4bdea937baa78cd4ad6f.json').get()
            );

            $httpBackend.whenGET('../api/questionnaire/chapter/7c97b1925b0244b782ed6741a5035fae?chapterId=34e31f00cdbff703a680db5fa66bf8b5').respond(
                $resource('./data/chapter/34e31f00cdbff703a680db5fa66bf8b5.json').get()
            );

            $httpBackend.whenGET('../api/questionnaire/editQuestion/7c97b1925b0244b782ed6741a5035fae?questionId=20ec89157c0e41e49d77db46e929db5d').respond(
                $resource('./data/editQuestion/20ec89157c0e41e49d77db46e929db5d.json').get()
            );

            //Empty Questionnaire
            $httpBackend.whenGET('../api/questionnaire/get/C772F0868D6E4B46B2EB281382F280AB').respond(
                $resource('./data/questionnaire/empty.json').get()
            );

            //Verifier
            $httpBackend.whenGET('../api/questionnaire/editGroup/7c97b1925b0244b782ed6741a5035fae?groupId=ddfaab0f37394a679f088add19325cfe').respond(
                $resource('./data/editGroup/ddfaab0f37394a679f088add19325cfe.json').get()
            );

            $httpBackend.whenGET('../api/questionnaire/verify/7c97b1925b0244b782ed6741a5035fae').respond(
                $resource('./data/verify/with-errors.json').get()
            );

            //Comands
            $httpBackend.whenPOST('../command/execute').respond();

            //Views
            $httpBackend.whenGET(/views\/.*/).passThrough();

            //Data files
            $httpBackend.whenGET(/data\/.*/).passThrough();

            // Validation
            $httpBackend.whenPOST('../account/findbyemail', {email: 'test@test.com'})
                .respond({"isUserExist": false});
        });
}());