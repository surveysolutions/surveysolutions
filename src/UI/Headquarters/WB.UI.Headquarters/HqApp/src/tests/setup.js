class ApiMocker {
    constructor() {
        this.mockApi = jest.fn();

        const mockSelf = this;

        jest.mock("vue", () => {
            return {
                get $api() {
                    return {
                        async call(callback) {
                            return mockSelf.mockApi(callback)
                        }
                    }
                }
            }
        });
    }

    setupApiMock(apis){

        this.mockApi.mockImplementation((apiCaller) => {
            return apiCaller(apis)
        });

    }
   
}

//console.log(jest);

global.ApiMocker = ApiMocker;
