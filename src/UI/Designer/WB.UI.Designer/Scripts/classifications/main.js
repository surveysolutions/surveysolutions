$(function() {
    
    Vue.use(VeeValidate);

    var app = new Vue({
        store: store,
        el: '#designer-list',
        data: {
            isAdmin: Vue.$config.isAdmin
        },
        created() {
            this.$store.dispatch('loadGroups');
            Vue.nextTick(function() {
                $('.scroller').perfectScrollbar();
            });
        },
        computed: {
            isLoading () {
                return this.$store.state.isLoading;
            },
            groups() {
                return this.$store.state.groups;
            },
            classifications() {
                return this.$store.state.classifications;
            },
            categories() {
                return this.$store.state.categories;
            },
            activeGroup() {
                return this.$store.state.activeGroup;
            },
            activeClassification() {
                return this.$store.state.activeClassification;
            }
        },
        methods: {
            addGroup() {
                this.$store.dispatch('addGroup', { id: guid(), isNew: true, title: '' });
            },
            addClassification() {
                this.$store.dispatch('addClassification',
                    { id: guid(), isNew: true, title: '', parent: this.activeGroup.id, userId: Vue.$config.userId });
            }
        }
    });
});
