import * as toastr from "toastr"
import "toastr/build/toastr.css"
import * as Vue from "vue"

Vue.config.errorHandler = (error, vm) => {
    console.error(error, vm)
    toastr.error(error)
}

function toastErr(err, message) {
    toastr.error(message)
    console.error(message, err)
}

function wrap(name, method) {
    // tslint:disable-next-line:only-arrow-functions - we need arguments param here, it cannot be used in arrow function
    return function () {
        try {
            const result = method.apply(this, arguments);

            // handle async exceptions
            if (result && result.catch) {
                result.catch(err => {
                    toastErr(err, name + ": " + err.message)
                })
            }

            return result;
        } catch (err) {
            toastr.error(err.message)
            console.error(name, err)
        }
    }
}

function handleErrors(object) {
    let name
    let method

    for (name in object) {
        if (typeof object[name] === "function") {
            method = object[name]
            object[name] = wrap(name, method)
        }
    }

    return object
}

export function safeStore(storeConfig, fieldToSafe = ["actions", "mutations", "getters"]) {

    for (let field in fieldToSafe) {
        let item = fieldToSafe[field]
        if (storeConfig[item]) {
            storeConfig[item] = handleErrors(storeConfig[item])
        }
    }

    storeConfig.actions.UNHANDLED_ERROR = (ctx, error: Error) => {
        toastErr(error, error.message)
    }

    return storeConfig
}
