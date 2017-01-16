import * as toastr from "toastr"
import "toastr/build/toastr.css"
import * as Vue from "vue"
import { verboseMode } from "./config"

Vue.config.errorHandler = (error, vm) => {
    console.error(error, vm)
    toastr.error(error)
}

function toastErr(err, message) {
    toastr.error(message)
    console.error(message, err)
}

function wrap(name, method, section) {
    // tslint:disable-next-line:only-arrow-functions - we need arguments param here, it cannot be used in arrow function
    return function () {
        try {
            if (verboseMode) {
                let argument = arguments[1] == null ? null : JSON.parse(JSON.stringify(arguments[1]))

                // tslint:disable-next-line:no-console
                console.info("call", section, name, argument) // , new Error().stack)
            }

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

function handleErrors(object, section) {
    let name
    let method

    for (name in object) {
        if (typeof object[name] === "function") {
            method = object[name]
            object[name] = wrap(name, method, section)
        }
    }

    return object
}

export function safeStore(storeConfig, fieldToSafe = ["actions", "mutations", "getters"]) {

    for (let field in fieldToSafe) {
        let item = fieldToSafe[field]
        if (storeConfig[item]) {
            storeConfig[item] = handleErrors(storeConfig[item], item)
        }
    }

    storeConfig.actions.UNHANDLED_ERROR = (ctx, error: Error) => {
        toastErr(error, error.message)
    }

    return storeConfig
}
