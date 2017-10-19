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
        return flatten(this.components.map((c) => c.routes || []));
    }
}

function flatten(arr) {
    return arr.reduce(function (flat, toFlatten) {
        return flat.concat(Array.isArray(toFlatten) ? flatten(toFlatten) : toFlatten);
    }, []);
}