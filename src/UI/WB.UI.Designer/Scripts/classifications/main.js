$(function() {

    app = new Vue.createApp({
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
                this.$store.dispatch('addGroup', { id: guid(), isNew: true, title: '', count: 0 });
            },
            addClassification() {
                this.$store.dispatch('addClassification',
                    { id: guid(), isNew: true, title: '', parent: this.activeGroup.id, userId: Vue.$config.userId, count: 0 });
            }
        }
    });
    
    VeeValidate.Validator.extend('stringOptions',
        {
            getMessage(field, args, data) {
                return "You entered an invalid input. Each line should follow the format: 'Title...Value[...Attachment name]'. 'Value' must be an integer number. 'Title' must be an alpha-numeric string. 'Attachment name' is optional. No empty lines are allowed. Lines: " + data + ".";
            },
            validate(value, args) {
                if (!_.isEmpty(value)) {
                    var options = (value || "").split("\n");
                    var matchPattern = true;
                    var invalidLines = [];
                    _.forEach(options, function (option, index) {
                        var currentLineValidationResult = Vue.$config.optionsParseRegex.test((option || ""));
                        matchPattern = matchPattern && currentLineValidationResult;
                        if (!currentLineValidationResult)
                            invalidLines.push(index + 1);
                    });
                    return { valid: matchPattern, data: invalidLines}
                } 
                return true;
            }
        });

    app.use(VeeValidate);    
});
