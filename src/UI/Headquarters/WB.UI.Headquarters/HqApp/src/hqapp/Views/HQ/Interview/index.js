import Review from "./Review"
import store from "./store"

export default {
    routes: [{
        path: '/Interview/Review/:id', 
        component: Review,
        meta: {
            store
        }
    }],

}