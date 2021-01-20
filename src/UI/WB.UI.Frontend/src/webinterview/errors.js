import * as toastr from 'toastr'
import Vue from 'vue'

/* eslint:disable:no-console */
Vue.config.errorHandler = (error, vm) => {
    if (error.response.data != null) {
        var data = error.response.data

        // handling asp net core validation errors
        if (data.Type == 'https://tools.ietf.org/html/rfc7231#section-6.5.1') {
            let message = ''
            Object.keys(data.Errors).forEach(k => {
                message += k + ':\r\n'
                data.Errors[k].forEach(errMessage => message += '  ' + errMessage + '\r\n')
            })

            console.error(data)
            toastr.error(message, data.Title)
            return
        }
    }
    console.error(error, vm)
    toastr.error(error)
}

function toastErr(err, message) {
    if (window.CONFIG.verboseMode) {
        console.error(message, err)
    }

    toastr.error(message, 'Error', {
        preventDuplicates: true,
    })
}

function wrap(name, method, section) {
    // tslint:disable-next-line:only-arrow-functions - we need arguments param here, it cannot be used in arrow function
    return function __action_wrapper() {
        try {
            if (window.CONFIG.verboseMode) {
                const argument = arguments[1] == null ? null : JSON.parse(JSON.stringify(arguments[1]))
                if (argument && Object.prototype.hasOwnProperty.call(argument, 'source'))
                    console.debug('call', section, 'from', argument.source, name, argument)
                else
                    console.debug('call', section, name, argument)
            }

            const result = method.apply(this, arguments)

            if (result) {
                var isPromise = typeof result.then == 'function'

                // handle async exceptions
                if (isPromise) {
                    result.catch(err => {
                        console.error(name, err)
                        toastErr(err, name + ': ' + err.message)
                    })
                }
            }

            return result
        } catch (err) {
            toastr.error(err.message)
            console.error(name, err)
        }
    }
}

function handleErrors(object, section) {
    let method

    for (const name in object) {
        if (typeof object[name] === 'function') {
            method = object[name]
            if (method.name != '__action_wrapper')
                object[name] = wrap(name, method, section)
        }
    }

    return object
}

export function safeStore(storeConfig, fieldToSafe = ['actions', 'mutations']) {

    function wrapFields(config) {
        for (const field in fieldToSafe) {
            const item = fieldToSafe[field]

            if (Object.prototype.hasOwnProperty.call(config, item)) {
                config[item] = handleErrors(config[item], item)
            }
        }
    }

    wrapFields(storeConfig)

    if (Object.prototype.hasOwnProperty.call(storeConfig, 'modules')) {
        Object.keys(storeConfig.modules).forEach((module) => wrapFields(storeConfig.modules[module]))
    }

    storeConfig.actions.UNHANDLED_ERROR = (ctx, error) => {
        console.error(name, error)
        toastErr(error, error.message)
    }

    return storeConfig
}
