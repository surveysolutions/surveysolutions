'use strict';

describe('Service: authorizationInterceptor', function() {

    // load the service's module
    beforeEach(module('designerApp'));

    // instantiate service
    var authorizationInterceptor;
    beforeEach(inject(function(_authorizationInterceptor_) {
        authorizationInterceptor = _authorizationInterceptor_;
    }));

    it('should do something', function() {
        expect(!!authorizationInterceptor).toBe(true);
    });

});
