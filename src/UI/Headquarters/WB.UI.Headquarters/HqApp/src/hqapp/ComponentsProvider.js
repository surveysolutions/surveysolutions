export default class ComponentsProvider {
    constructor(rootStore, initialComponents) {
        this.rootStore = rootStore
        this.components = []

        this.initialize(initialComponents);
    }

    initialize(initialComponents) {
        const queue = initialComponents;

        while (queue.length > 0) {
            const Component = queue.pop();

            if (Array.isArray(Component)) {
                Component.forEach((c) => queue.push(c));
            } else {
                const component = new Component(this.rootStore);
                this.components.push(component);
            }
        }
    }

    get routes() {
        return flatten(this.components.map((component) => {
            const init = component.initialize ? () => component.initialize() : null;
            const beforeEnter = component.beforeEnter ? (from, to, next) => component.beforeEnter(from, to, next) : null;
            const routes = component.routes || [];

            if(init || beforeEnter) {
                routes.forEach((route) => {
                    route.beforeEnter = (from, to, next) => {
                        if(component._isInitialized !== true) {

                            if(component.modules != null) {
                                Object.keys(component.modules).forEach((module) => {
                                    this.rootStore.registerModule(module, component.modules[module]);
                                });
                            }

                            init && init();

                            component._isInitialized = true;
                        }

                        if(beforeEnter != null){ 
                            beforeEnter(from, to, next); 
                        }
                        else next();
                    };
                })
            }

            return routes;
        }));
    }
}

function flatten(arr) {
    return arr.reduce(function (flat, toFlatten) {
        return flat.concat(Array.isArray(toFlatten) ? flatten(toFlatten) : toFlatten);
    }, []);
}