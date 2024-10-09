export function cloneWithWritableProperties(obj) {
    const descriptors = Object.getOwnPropertyDescriptors(obj);

    for (const key in descriptors) {
        if (descriptors.hasOwnProperty(key)) {

            if (typeof descriptors[key].value === 'object' && descriptors[key].value !== null) {
                descriptors[key].value = cloneWithWritableProperties(descriptors[key].value);
            }

            descriptors[key].writable = true;
            descriptors[key].configurable = true;
        }
    }

    return Object.create(Object.getPrototypeOf(obj), descriptors);
}